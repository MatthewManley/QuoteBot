using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    public static class ReplyExtensions
    {
        public static Task<RestUserMessage> Reply(this SocketCommandContext context, string text)
        {
            return context.Channel.SendMessageAsync(text);
        }

        public static bool HasPrefix(this SocketUserMessage message, ISelfUser user, ref int argPos)
        {
            return message.HasStringPrefix(Settings.Prefix, ref argPos) ||
                message.HasMentionPrefix(user, ref argPos);
        }

        public static bool IsBotOwner(this SocketCommandContext context)
        {
            return Settings.Owner.HasValue && context.User.Id == Settings.Owner.Value;
        }
    }
}
