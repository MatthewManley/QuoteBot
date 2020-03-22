using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.Options;
using DiscordBot.Services;
using Microsoft.Extensions.Options;

namespace DiscordBot
{
    public class Bot
    {
        private readonly DiscordSocketClient client;
        private readonly AuthOptions authOptions;
        private readonly StatsService statsService;
        private readonly CommandHandler commandHandler;
        private const ulong announceChannelId = 178546341314691072UL; //TODO: make configurable

        public Bot(DiscordSocketClient client, IOptions<AuthOptions> authOptions, StatsService statsService, CommandHandler commandHandler)
        {
            this.client = client;
            this.authOptions = authOptions.Value;
            this.statsService = statsService;
            this.commandHandler = commandHandler;
        }

        public async Task Run()
        {
            client.Log += Log;
            client.Ready += Ready;
            await client.LoginAsync(TokenType.Bot, authOptions.BotKey);
            await client.StartAsync();
            statsService.Init();
            commandHandler.InitializeAsync();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task Ready()
        {
            if (client.GetChannel(announceChannelId) is SocketTextChannel announceChannel)
            {
                await announceChannel.SendMessageAsync("I just started up!");
            }
            else
            {
                Console.WriteLine($"Could not announce to channel {announceChannelId}");
            }
        }
    }
}