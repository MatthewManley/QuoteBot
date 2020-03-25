using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Modules;
using DiscordBot.Options;
using DiscordBot.Services;
using Domain.Repos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        private readonly IAudioRepo audioRepo;
        private readonly StatsService statsService;
        private readonly ICategoryRepo categoryRepo;
        private readonly BotOptions botOptions;
        private Dictionary<string, MethodInfo> commands = null;

        public CommandHandler(
            DiscordSocketClient client,
            IServiceProvider serviceProvider,
            IAudioRepo audioRepo,
            StatsService statsService,
            ICategoryRepo categoryRepo,
            IOptions<BotOptions> botOptions)
        {
            this.serviceProvider = serviceProvider;
            this.audioRepo = audioRepo;
            this.statsService = statsService;
            this.categoryRepo = categoryRepo;
            this.botOptions = botOptions.Value;
            _client = client;
        }

        public List<string> GetCommands()
        {
            return commands.Keys.ToList();
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
            statsService.SawMessage();
            Thread t = new Thread(async () =>
            {
                try
                {
                    await HandleCommandAsync(rawMessage);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            t.IsBackground = true;
            t.Start();
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage rawMessage)
        {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message))
                return;

            if (message.Source != MessageSource.User)
                return;

            int argPos = 0;
            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!message.HasPrefix(botOptions.Prefix, _client.CurrentUser, ref argPos))
                return;

            var command = message.Content.Substring(argPos).Split(' ')[0].ToLowerInvariant();

            var context = new SocketCommandContext(_client, message);
            if (commands.TryGetValue(command, out var method))
            {
                var parent = serviceProvider.GetRequiredService(method.DeclaringType);
                method.Invoke(parent, new object[] { context });
                return;
            }

            var categories = await categoryRepo.GetCategoriesWithAudio(context.Guild.Id);
            if (categories.Select(x => x.Name).Contains(command))
            {
                var soundMod = serviceProvider.GetRequiredService<SoundModule>();
                await soundMod.PlaySound(context);
                return;
            }
        }
    }
}
