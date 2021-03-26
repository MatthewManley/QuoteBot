using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Modules;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public CommandHandler(
            DiscordSocketClient client,
            IServiceProvider serviceProvider,
            IServerRepo serverRepo,
            IQuoteBotRepo quoteBotRepo)
        {
            this.serverRepo = serverRepo;
            this.quoteBotRepo = quoteBotRepo;
            this.serviceProvider = serviceProvider;
            _client = client;
        }

        public void InitializeAsync()
        {

            commands = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(MyCommandSet)))
                .Select(GetCommandMethods)
                .SelectMany(x => x)
                .ToDictionary(x => ((MyCommand)x.GetCustomAttribute(typeof(MyCommand))).Name);
            _client.MessageReceived += HandleCommandAsyncWrapper;
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
            if (!(rawMessage is SocketUserMessage message))
                return;

            // Ingore whoose source is not a user
            //TODO: is this handeled by the above?
            if (message.Source != MessageSource.User)
                return;

            // determine if we think the user is trying to execute a command
            var (hasPrefix, argPos) = await HasPrefix(message);
            if (!hasPrefix)
                return;

            var command = message.Content.Substring(argPos).Split(' ');
            if (command.Length <= 0)
                return;


            //var context = new SocketCommandContext(_client, message);
            if (commands.TryGetValue(command.First(), out var method))
            {
                var context = new SocketCommandContext(_client, message);
                var parent = serviceProvider.GetRequiredService(method.DeclaringType);
                method.Invoke(parent, new object[] { context, command });
                return;
            }
            
            // Sounds can only be played in a guild
            if (!(message.Channel is SocketGuildChannel guildChannel))
                return;

            var categories = await quoteBotRepo.GetCategoriesWithAudio(guildChannel.Guild.Id);
            if (categories.Select(x => x.Name).Contains(command.First()))
            {
                var context = new SocketCommandContext(_client, message);
                var soundMod = serviceProvider.GetRequiredService<SoundModule>();
                await soundMod.PlaySound(context, command);
                return;
            }
        }

        private async Task<(bool, int)> HasPrefix(SocketUserMessage message)
        {
            int argPos = 0;
            if (message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                return (true, argPos);

            if (!(message.Channel is IGuildChannel guildChannel))
                return (false, argPos);

            var prefix = (await serverRepo.GetServerPrefix(guildChannel.GuildId))?.Trim();

            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = "!"; 
            }

            var hasPrefix = message.HasStringPrefix(prefix, ref argPos, StringComparison.InvariantCultureIgnoreCase);

            return (hasPrefix, argPos);
        }
    }
}
