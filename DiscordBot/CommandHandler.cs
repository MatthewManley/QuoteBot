using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Modules;
using Domain.Models;
using Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider serviceProvider;
        private Dictionary<string, MethodInfo> commands = null;
        private readonly IServerRepo serverRepo;
        private readonly IQuoteBotRepo quoteBotRepo;
        private readonly IMemoryCache memoryCache;
        private readonly SemaphoreSlim semaphoreSlim = new(5, 5);

        public CommandHandler(
            DiscordSocketClient client,
            IServiceProvider serviceProvider,
            IServerRepo serverRepo,
            IQuoteBotRepo quoteBotRepo,
            IMemoryCache memoryCache)
        {
            this.serverRepo = serverRepo;
            this.quoteBotRepo = quoteBotRepo;
            this.memoryCache = memoryCache;
            this.serviceProvider = serviceProvider;
            _client = client;
        }

        public void InitializeAsync()
        {
            var methodInfos = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(MyCommandSet)))
                .Select(GetCommandMethods)
                .SelectMany(x => x)
                .ToList();
            foreach (var value in methodInfos)
            {
                var parameterInfo = value.GetParameters();
                if (parameterInfo.Length != 2)
                    throw new Exception();
                if (parameterInfo[0].ParameterType != typeof(SocketCommandContext))
                    throw new Exception();
                if (parameterInfo[1].ParameterType != typeof(string[]))
                    throw new Exception();
            }
            commands = methodInfos.ToDictionary(x => ((MyCommand)x.GetCustomAttribute(typeof(MyCommand))).Name);
            _client.MessageReceived += HandleCommandAsyncWrapper;
            _client.JoinedGuild += _client_JoinedGuild;
            _client.GuildAvailable += _client_JoinedGuild;
        }

        private async Task _client_JoinedGuild(SocketGuild arg)
        {
            try
            {
                await semaphoreSlim.WaitAsync();
                var serverConfig = await serverRepo.GetServerConfig(arg.Id);
                if (serverConfig is null)
                {
                    serverConfig = new ServerConfig
                    {
                        ServerId = arg.Id,
                        ModeratorRole = null,
                        Prefix = null,
                        TextChannelList = null,
                        TextChannelListType = "BLOCK",
                        VoiceChannelList = null,
                        VoiceChannelListType = "BLOCK",
                    };
                    await serverRepo.PutServerConfig(serverConfig);
                }
                memoryCache.Set($"serverconfig={arg.Id}", serverConfig, TimeSpan.FromMinutes(1));
            }
            finally
            {
                semaphoreSlim.Release();
            }

        }

        private IEnumerable<MethodInfo> GetCommandMethods(Type x)
        {
            return x.GetMethods()
                .Where(x => x.GetCustomAttributes(typeof(MyCommand), false).Length > 0);
        }

        private Task HandleCommandAsyncWrapper(SocketMessage rawMessage)
        {
            Task.Run(async () =>
            {
                try
                {
                    await HandleCommandAsync(rawMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage rawMessage)
        {
            // Ignore messages that aren't socket user messages
            if (rawMessage is not SocketUserMessage message)
                return;

            // Ingore whoose source is not a user
            //TODO: is this handeled by the above?
            if (message.Source != MessageSource.User)
                return;

            // Get server config if the message is from a server
            ulong? serverId = (message.Channel as SocketGuildChannel)?.Guild.Id;
            ServerConfig serverConfig = null;
            if (serverId.HasValue)
            {
                serverConfig = memoryCache.Get<ServerConfig>($"serverconfig={serverId}");
                if (serverConfig == null)
                {
                    serverConfig = await serverRepo.GetServerConfig(serverId.Value);
                    if (serverConfig == null)
                        return;
                    memoryCache.Set($"serverconfig={serverId}", serverConfig, TimeSpan.FromMinutes(1));
                }

                if (serverConfig.TextChannelListType == "BLOCK")
                {
                    if (serverConfig.TextChannelList.Contains(message.Channel.Id))
                    {
                        return;
                    }
                }
                else if (serverConfig.TextChannelListType == "ALLOW")
                {
                    if (!serverConfig.TextChannelList.Contains(message.Channel.Id))
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            // determine if we think the user is trying to execute a command
            var (hasPrefix, argPos) = HasPrefix(message, serverConfig.Prefix);
            if (!hasPrefix)
                return;

            var command = message.Content[argPos..].Split(' ');
            if (command.Length <= 0)
                return;


            //var context = new SocketCommandContext(_client, message);
            if (commands.TryGetValue(command.First().ToLowerInvariant(), out var method))
            {
                var context = new SocketCommandContext(_client, message);
                var parent = serviceProvider.GetRequiredService(method.DeclaringType);
                method.Invoke(parent, new object[] { context, command });
                return;
            }
            
            // Sounds can only be played in a guild
            if (serverId.HasValue)
                return;

            var categories = await quoteBotRepo.GetCategoriesWithAudio(serverId.Value);
            if (categories.Select(x => x.Name).Contains(command.First()))
            {
                var context = new SocketCommandContext(_client, message);
                var soundMod = serviceProvider.GetRequiredService<SoundModule>();
                await soundMod.PlaySound(context, command);
                return;
            }
        }

        private (bool, int) HasPrefix(SocketUserMessage message, string prefix)
        {
            int argPos = 0;
            if (message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                return (true, argPos);

            if (prefix is null)
                return (false, argPos);

            string trimmed = prefix.Trim();

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return (false, argPos);
            }

            var hasPrefix = message.HasStringPrefix(trimmed, ref argPos, StringComparison.InvariantCultureIgnoreCase);

            return (hasPrefix, argPos);
        }
    }
}
