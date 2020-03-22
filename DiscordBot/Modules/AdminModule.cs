using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBot.Options;
using Microsoft.Extensions.Options;

namespace DiscordBot.Modules
{
    public class AdminModule : MyCommandSet
    {
        private readonly IDiscordClient discordClient;
        private readonly BotOptions botOptions;

        public AdminModule(IDiscordClient discordClient, IOptions<BotOptions> botOptions)
        {
            this.discordClient = discordClient;
            this.botOptions = botOptions.Value;
        }
        
        [MyCommand("listservers")]
        public async Task ListServers(SocketCommandContext context)
        {
            if (!context.IsBotOwner(botOptions.OwnerValue))
            {
                await context.Reply("no");
                return;
            }
            var guilds = await discordClient.GetGuildsAsync();
            int argPos = 0;
            context.Message.HasPrefix(botOptions.Prefix, discordClient.CurrentUser, ref argPos);
            var commandParts = context.Message.Content.Substring(argPos).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (commandParts.Length != 1 && commandParts.Length != 2)
            {
                await context.Reply("u wat mate?");
                return;
            }
            int pageCount = guilds.Count / 10 + 1;
            int page = 0;
            if (commandParts.Length == 2)
            {
                if (!int.TryParse(commandParts[1], out page))
                {
                    await context.Reply("thats not a number");
                    return;
                }
                if (page > pageCount)
                {
                    await context.Reply($"There is only {pageCount} pages");
                    return;
                }
                page--;
            }
            var show = guilds.Skip(page * 10).Take(10).ToList();
            var builder = new StringBuilder();
            builder.Append("Total Servers: ");
            builder.Append(guilds.Count);
            builder.AppendLine();
            builder.Append("Page: ");
            builder.Append(page + 1);
            builder.Append("/");
            builder.Append(pageCount);
            foreach (var guild in show)
            {
                builder.AppendLine();
                builder.Append(guild.Id);
                builder.Append(": ");
                builder.Append(guild.Name);
            }
            await context.Reply(builder.ToString());
        }
    }
}
