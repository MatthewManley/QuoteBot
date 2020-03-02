using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class InfoModule : MyCommandSet
    {
        private readonly StatsService statsService;

        public InfoModule(StatsService statsService)
        {
            this.statsService = statsService;
        }

        [MyCommand("ping")]
        public async Task Ping(SocketCommandContext context)
        {
            await context.Reply("!Pong");
        }

        [MyCommand("uptime")]
        public async Task Uptime(SocketCommandContext context)
        {
            var uptime = statsService.GetUptime();
            var builder = new StringBuilder();

            if (uptime.Days > 0)
            {
                builder.Append(uptime.Days);
                builder.Append("d");
            }

            if (uptime.Hours > 0 || builder.Length > 0)
            {
                if (builder.Length > 0)
                    builder.Append(" ");
                builder.Append(uptime.Hours);
                builder.Append("h");
            }

            if (uptime.Minutes > 0 || builder.Length > 0)
            {
                if (builder.Length > 0)
                    builder.Append(" ");
                builder.Append(uptime.Minutes);
                builder.Append("m");
            }

            if (uptime.Seconds > 0 || builder.Length > 0)
            {
                if (builder.Length > 0)
                    builder.Append(" ");
                builder.Append(uptime.Seconds);
                builder.Append("s");
            }
            await context.Reply(builder.ToString());
        }
    }
}
