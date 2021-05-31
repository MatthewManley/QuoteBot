using Discord.WebSocket;
using DiscordBot.Modules;
using Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.Controllers
{
    [ApiController]
    public class QuoteController
    {
        private readonly DiscordSocketClient discordSocketClient;
        private readonly SoundModule soundModule;
        private readonly IAudioRepo audioRepo;

        public QuoteController(
            DiscordSocketClient discordSocketClient,
            SoundModule soundModule,
            Domain.Repositories.IAudioRepo audioRepo)
        {
            this.discordSocketClient = discordSocketClient;
            this.soundModule = soundModule;
            this.audioRepo = audioRepo;
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

        [HttpPost("/playquote")]
        public async Task<IActionResult> SendMessage([FromBody] PlayQuoteBody body)
        {
            var channel = discordSocketClient.GetChannel(body.VoiceChannel);
            if (channel is not Discord.IVoiceChannel vc)
            {
                return new BadRequestResult();
            }
            var audio = await audioRepo.GetAudioById(body.AudioId);
            await soundModule.Play(vc, audio, CancellationToken.None);
            return new OkResult();
        }
    }

    public class SendMessageBody
    {
        public string Message { get; set; }
        public ulong ChannelId { get; set; }
    }

    public class PlayQuoteBody
    {
        public uint AudioId { get; set; }
        public ulong VoiceChannel { get; set; }
    }
}
