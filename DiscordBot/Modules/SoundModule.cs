using Discord;
using Discord.Audio;
using Discord.Commands;
using DiscordBot.Services;
using Domain.Repos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class SoundModule : MyCommandSet
    {
        private readonly IAudioRepo audioRepo;
        private readonly StatsService statsService;
        private readonly IUserRepo userRepo;

        public SoundModule(IAudioRepo audioRepo, StatsService statsService, IUserRepo userRepo)
        {
            this.audioRepo = audioRepo;
            this.statsService = statsService;
            this.userRepo = userRepo;
        }

        [MyCommand("list")]
        public async Task List(SocketCommandContext context)
        {
            var parts = context.Message.Content.Trim().Split(" ");
            if (parts.Length != 1 && parts.Length != 2)
            {
                return;
            }
            if (parts.Length == 1)
            {
                var categories = await audioRepo.GetCategories();
                await context.Reply(string.Join("\n", categories.Select(x => $"!{x}")));
            }
            else if (parts.Length == 2)
            {
                var sounds = await audioRepo.GetAudioForCategory(parts[1]);
                if (sounds.Count == 0)
                {
                    await context.Reply("No sounds found for that category.");
                }
                else
                {
                    await context.Reply(string.Join(", ", sounds.Select(x => x.Name)));
                }
            }
        }

        [MyCommand("history")]
        public async Task History(SocketCommandContext context)
        {
            var history = statsService.GetHistory();
            history.Reverse();
            await context.Reply(string.Join("\n", history));
        }

        [MyCommand("upload")]
        public async Task Upload(SocketCommandContext context)
        {
            var hasUploadRole = await userRepo.UserHasAnyRole(context.User.Id, "Upload");
            var isOwner = context.User.Id.ToString() == Environment.GetEnvironmentVariable("owner");
            if (!(hasUploadRole || isOwner))
            {
                await context.Reply("no");
                return;
            }
            var attachments = context.Message.Attachments;
            if (attachments.Count != 1)
            {
                await context.Reply("The message must contain 1 audio attachment.");
                return;
            }
            var attachment = attachments.First();
            var section = context.Message.Content.Split(" ")[1];
            try
            {
                var dir = $"audio\\{section}";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                new WebClient().DownloadFile(attachment.Url, $"{dir}\\{attachment.Filename}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task PlaySound(SocketCommandContext context, string person, string quote)
        {
            var path = await audioRepo.GetPathForAudio(person, quote);
            if (path == null)
            {
                //TODO: print error
                return;
            }
            await DoThing(context, path);
        }

        public async Task PlaySound(SocketCommandContext context, string person)
        {
            var playlist = await audioRepo.GetAudioForCategory(person);
            var pathIdx = StaticRandom.Next(playlist.Count);
            var path = playlist[pathIdx];
            await DoThing(context, path.Path);
        }

        private async Task DoThing(SocketCommandContext context, string path)
        {
            var vc = (context.User as IGuildUser)?.VoiceChannel;
            if (vc == null)
            {
                await context.Reply("You have to be in a voice channel to do this.");
                return;
            }
            // Create FFmpeg using the previous example
            //await context.Channel.SendMessageAsync(path);
            path = Path.Combine(Environment.GetEnvironmentVariable("audio_path"), path);
            await Play(vc, path);
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -af \"adelay=100|100\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
        }

        private async Task Play(IVoiceChannel vc, string path)
        {
            var audioClient = await vc.ConnectAsync();
            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var error = ffmpeg.StandardError)
            using (var discord = audioClient.CreatePCMStream(AudioApplication.Voice, bufferMillis: 1))
            using (var memStream = new MemoryStream())
            {
                try
                {
                    await output.CopyToAsync(discord);
                }
                finally
                {
                    await discord.FlushAsync();
                }

            }
            await vc.DisconnectAsync();
        }
    }
}
