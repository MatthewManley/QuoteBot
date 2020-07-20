using Discord.Commands;
using Discord.Rest;
using System.Threading.Tasks;

namespace DiscordBot
{
    public static class ReplyExtensions
    {
        public static Task<RestUserMessage> Reply(this SocketCommandContext context, string text)
        {
            return context.Channel.SendMessageAsync(text);
        }
    }
}
