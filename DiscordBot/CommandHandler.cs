using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Modules;
using DiscordBot.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot
{
    class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly SoundsService _soundsService;
        private readonly IServiceProvider serviceProvider;
        private Dictionary<string, MethodInfo> commands = null;
         
        public CommandHandler(DiscordSocketClient client, SoundsService soundsService, IServiceProvider serviceProvider)
        {
            _soundsService = soundsService;
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
            _client.MessageReceived += HandleCommandAsync;
        }

        private IEnumerable<MethodInfo> GetCommandMethods(Type x)
        {
            return x.GetMethods()
                .Where(x => x.GetCustomAttributes(typeof(MyCommand), false).Length > 0);
        }

        private Task HandleCommandAsync(SocketMessage rawMessage)
        {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message)) return Task.CompletedTask;
            if (message.Source != MessageSource.User) return Task.CompletedTask;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('!', ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return Task.CompletedTask;

            var command = message.Content.Substring(argPos).Split(' ');
            var sounds = _soundsService.GetSounds();
            var context = new SocketCommandContext(_client, message);
            if (sounds.TryGetValue(command[0], out var soundsList))
            {
                var soundMod = serviceProvider.GetRequiredService<SoundModule>();
                if (command.Length == 2)
                {
                    var thread = new Thread(async () =>
                    {
                        await soundMod.PlaySound(context, command[0], command[1]);
                    });
                    thread.IsBackground = true;
                    thread.Start();
                    return Task.CompletedTask;
                }
                else if (command.Length == 1)
                {
                    var thread = new Thread(async () =>
                    {
                        await soundMod.PlaySound(context, command[0]);
                    });
                    thread.IsBackground = true;
                    thread.Start();
                    return Task.CompletedTask;
                }
            }
            if (commands.TryGetValue(command[0], out var method))
            {
                var parent = serviceProvider.GetRequiredService(method.DeclaringType);
                try
                {
                    method.Invoke(parent, new object[] { context });
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return Task.CompletedTask;
        }
    }
}
