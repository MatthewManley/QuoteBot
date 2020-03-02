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
        private readonly SoundsService soundsService;
        private readonly StatsService statsService;
        private readonly IUserRepo userRepo;

        public SoundModule(SoundsService soundsService, StatsService statsService, IUserRepo userRepo)
        {
            this.soundsService = soundsService;
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
            var sounds = soundsService.GetSounds();
            if (parts.Length == 1)
            {
                await context.Reply(string.Join("\n", sounds.Keys.Select(x => $"!{x}")));
            }
            else if (parts.Length == 2)
            {
                var key = parts[1];
                if (sounds.TryGetValue(parts[1], out var value))
                {
                    var lst = value.Select(x => x.Split('\\')[2]);
                    var msg = string.Join("\n", lst);
                    await context.Reply(msg);
                }
                else
                {
                    await context.Reply(string.Join("\n", sounds.Keys.Select(x => $"!{x}")));
                }
            }
            //await context.Reply("!Pong");
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
            var test = await userRepo.UserHasAnyRole(context.User.Id, "Upload", "Owner");
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
            var playlist = soundsService.GetSounds()[person];
            var path = playlist.Where(x => x.Split('\\').Last() == quote).FirstOrDefault();
            if (path == null)
            {
                //TODO: print error
                return;
            }
            await DoThing(context, path);
        }

        public async Task PlaySound(SocketCommandContext context, string person)
        {
            var playlist = soundsService.GetSounds()[person];
            var pathIdx = StaticRandom.Next(playlist.Length);
            var path = playlist[pathIdx];
            await DoThing(context, path);
        }

        private async Task DoThing(SocketCommandContext context, string path)
        {
            var pathParts = path.Split('\\');
            statsService.AddToHistory(pathParts[1], pathParts[2]);
            var vc = (context.User as IGuildUser)?.VoiceChannel;
            if (vc == null)
            {
                await context.Reply("You have to be in a voice channel to do this.");
                return;
            }
            // Create FFmpeg using the previous example
            //await context.Channel.SendMessageAsync(path);
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
