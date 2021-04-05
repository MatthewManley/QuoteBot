using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.Services;
using Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordBot
{
    public class Bot
    {
        private readonly ILogger<Bot> logger;
        private readonly DiscordSocketClient client;
        private readonly AuthOptions authOptions;
        private readonly StatsService statsService;
        private readonly CommandHandler commandHandler;
        private const ulong announceChannelId = 178546341314691072UL; //TODO: make configurable

        public Bot(ILogger<Bot> logger, DiscordSocketClient client, IOptions<AuthOptions> authOptions, StatsService statsService, CommandHandler commandHandler)
        {
            this.logger = logger;
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

        public async Task Stop()
        {
            await client.StopAsync();
            await client.LogoutAsync();
        }

        private Task Log(LogMessage msg)
        {
            logger.Log(Convert(msg.Severity), msg.Exception, msg.Message, msg.Source);
            return Task.CompletedTask;
        }

        private static LogLevel Convert(LogSeverity logSeverity)
        {
            return logSeverity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Debug,
                LogSeverity.Debug => LogLevel.Trace,
                _ => throw new NotImplementedException(),
            };
        }

        private async Task Ready()
        {
            if (client.GetChannel(announceChannelId) is SocketTextChannel announceChannel)
            {
                await announceChannel.SendMessageAsync("I just started up!");
                await client.SetGameAsync("https://quotebot.xyz");
            }
            else
            {
                logger.LogWarning($"Could not announce to channel {announceChannelId}");
            }
        }
    }
}