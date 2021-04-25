using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Controllers
{
    [ApiController]
    public class QuoteController
    {
        private readonly DiscordSocketClient discordSocketClient;

        public QuoteController(DiscordSocketClient discordSocketClient)
        {
            this.discordSocketClient = discordSocketClient;
        }

        [HttpPost("/sendmessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageBody body)
        {
            var channel = discordSocketClient.GetChannel(body.ChannelId);
            if (channel is not ISocketMessageChannel sgc)
            {
                return new BadRequestResult();
            }
            await sgc.SendMessageAsync(body.Message);
            return new OkResult();
        }
    }

    public class SendMessageBody
    {
        public string Message { get; set; }
        public ulong ChannelId { get; set; }
    }
}
