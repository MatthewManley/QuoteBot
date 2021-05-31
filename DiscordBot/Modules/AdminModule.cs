using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Domain.Models;
using Domain.Options;
using Microsoft.Extensions.Options;

namespace DiscordBot.Modules
{
    public class AdminModule : MyCommandSet
    {
        private readonly BotOptions botOptions;
        private readonly HttpClient httpClient;

        public AdminModule(IOptions<BotOptions> botOptions, HttpClient httpClient)
        {
            this.botOptions = botOptions.Value;
            this.httpClient = httpClient;
        }

        [MyCommand("deploy")]
        public async Task Deploy(SocketCommandContext context, string[] command, ServerConfig _)
        {
            if (context.User.Id != botOptions.OwnerValue || context.Channel.Id != botOptions.AnnounceChannelValue || command.Length != 2)
                return;
            string endpoint;
            if (command[1].ToLower() == "bot")
            {
                endpoint = "quotebot";
            }
            else if (command[1].ToLower() == "web")
            {
                endpoint = "quotebotweb";
            }
            else
            {
                return;
            }
            var uri = new Uri($"http://localhost:8080/{endpoint}");

            var response = await httpClient.GetAsync(uri);
            var result = await response.Content.ReadAsStringAsync();
            await context.Reply(result);
        }
    }
}
