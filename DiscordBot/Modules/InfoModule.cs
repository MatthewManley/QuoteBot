﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class InfoModule : MyCommandSet
    {
        private readonly StatsService statsService;
        private readonly IDiscordClient discordClient;

        public InfoModule(StatsService statsService, IDiscordClient discordClient)
        {
            this.statsService = statsService;
            this.discordClient = discordClient;
        }

        [MyCommand("ping")]
        public async Task Ping(SocketCommandContext context)
        {
            await context.Reply("!Pong");
        }

        private StringBuilder GetUptimeString()
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
            return builder;
        }


        [MyCommand("stats")]
        public async Task Stats(SocketCommandContext context)
        {
            var guilds = await discordClient.GetGuildsAsync();
            var builder = new StringBuilder();
            builder.Append("Uptime: ");
            builder.Append(GetUptimeString());
            builder.AppendLine();
            builder.Append("Messages Seen: ");
            builder.Append(statsService.GetSeenMessages());
            builder.AppendLine();
            builder.Append("Server Count: ");
            builder.Append(guilds.Count);
            await context.Reply(builder.ToString());
        }

        [MyCommand("invite")]
        public async Task Invite(SocketCommandContext context)
        {
            var app = await discordClient.GetApplicationInfoAsync();
            var invite = $"https://discordapp.com/api/oauth2/authorize?client_id={app.Id}&permissions=0&scope=bot";
            await context.Reply(invite);
        }
    }
}
