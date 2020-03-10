using Discord;
using Discord.Audio;
using Discord.Commands;
using DiscordBot.Services;
using Domain.Models;
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
        private readonly IDiscordClient client;
        private readonly CommandHandler commandHandler;

        public SoundModule(IAudioRepo audioRepo, StatsService statsService, IUserRepo userRepo, IDiscordClient client, CommandHandler commandHandler)
        {
            this.audioRepo = audioRepo;
            this.statsService = statsService;
            this.userRepo = userRepo;
            this.client = client;
            this.commandHandler = commandHandler;
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
            await context.Reply(string.Join("\n", history.Select(x => $"!{x.Category} {x.Name}")));
        }

        [MyCommand("upload")]
        public async Task Upload(SocketCommandContext context)
        {
            var hasUploadRole = await userRepo.UserHasAnyRole(context.User.Id, "Upload");
            var isOwner = context.IsBotOwner();
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
            int argPos = 0;
            context.Message.HasPrefix(client.CurrentUser, ref argPos);
            var commandParts = context.Message.Content.Substring(argPos).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (commandParts.Length != 3)
            {
                await context.Reply("!upload category name");
                return;
            }
            var attachment = attachments.First();
            var category = commandParts[1].ToLower();
            var name = commandParts[2].ToLower();
            var validChars = "abcdefghijklmnopqrstuvwxyz0123456789-_";
            if (category.Except(validChars).Any())
            {
                await context.Reply($"Invalid category, only use the following characters: {validChars}");
                return;
            }
            if (name.Except(validChars).Any())
            {
                await context.Reply($"Invalid name, only use the following characters: {validChars}");
                return;
            }
            if (commandHandler.GetCommands().Contains(category))
            {
                await context.Reply("Invalid command category!");
                return;
            }
            var currentAudio = await audioRepo.GetAudioForCategory(category);
            if (currentAudio.Any(x => x.Name == name))
            {
                await context.Reply("A quote already exists with that name");
                return;
            }

            try
            {
                var newName = Guid.NewGuid().ToString();
                var tempPath = Path.Combine(Settings.TempPath, newName);
                new WebClient().DownloadFile(attachment.Url, tempPath);
                var duration = await GetDuration(tempPath);
                if (duration > 30)
                {
                    await context.Reply("30 seconds maximum");
                    return;
                }
                var newPath = Path.Combine(Settings.AudioPath, newName);
                File.Move(tempPath, newPath);
                await audioRepo.AddAudio(new Audio {
                    Category = category,
                    Name = name,
                    Path = newPath
                });
                await context.Reply($"Done, you can no do\n{Settings.Prefix}{category} {name}");
            }
            catch (Exception ex)
            {
                await context.Reply("Something went wrong. Are you sure thats an audio file?");
            }
        }

        public async Task PlaySound(SocketCommandContext context)
        {
            if (!((context.Guild.CurrentUser as IGuildUser)?.VoiceChannel is null))
            {
                await context.Reply("I am already doing a quote clam the fuck down.");
                return;
            }
            var argPos = 0;
            context.Message.HasPrefix(client.CurrentUser, ref argPos);
            var commandParts = context.Message.Content.Substring(argPos).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var category = commandParts[0];
            IVoiceChannel channel = null;
            string quote = null;
            for (int i = 1; i < commandParts.Length; i++)
            {
                var part = commandParts[i].ToLower();
                if (part.Equals("-c") || part.Equals("--channel"))
                {
                    if (!(channel is null))
                    {
                        await context.Reply("How many voice channels you think I can take at once??? Cause I think its one...");
                        return;
                    }
                    // Error if there is no channel after -c or its not a proper mention
                    if (i + 1 >= commandParts.Length ||
                        !TryGetVoiceChannel(context, commandParts[i + 1], out var voiceChannel))
                    {
                        await context.Reply("Thats ain't no valid voice channel");
                        return;
                    }
                    i++; // Skip pass the channel id/name
                    channel = voiceChannel;
                    continue; // Continue onto any remaining arguments
                }
                else if (part.Equals("-q") || part.Equals("--quote") || part.Equals("-f") || part.Equals("--file"))
                {
                    if (!(quote is null))
                    {
                        await context.Reply("Don't be greedy, you only get one quote at a time.");
                        return;
                    }
                    if (i + 1 >= commandParts.Length)
                    {
                        await context.Reply("Don't leave me hanging, say what quote you want played.");
                        return;
                    }
                    i++;
                    quote = commandParts[i].ToLower();
                }
                else if (quote is null)
                {
                    quote = part;
                }
                else
                {
                    await context.Reply("You gave me to much stuff. What am i supposed to do with all that?");
                    return;
                }
            }
            channel = channel ?? (context.User as IGuildUser)?.VoiceChannel;
            if (channel is null)
            {
                await context.Reply("You gotta be in a voice channel or use -c channelName");
                return;
            }
            Audio audio;
            if (quote is null)
            {
                var playlist = await audioRepo.GetAudioForCategory(category);
                var takeAmount = Math.Min(Settings.RecentCount, playlist.Count - 1);
                var history = statsService.GetHistory().AsEnumerable().Reverse().Take(takeAmount);
                var available = playlist.Except(history).ToList();
                var index = StaticRandom.Next(available.Count);
                audio = available[index];
            }
            else
            {
                audio = await audioRepo.GetAudio(category, quote);
                if (audio is null)
                {
                    await context.Reply("That quote don't exist...");
                    return;
                }
            }
            // Katie isn't allow to use her own voice
            if (context.User.Id == 302955588327833622 && audio.Category == "heck")
            {
                audio = await audioRepo.GetAudio("trump", "i-dont-think-so");
            }
            statsService.AddToHistory(audio);
            await Play(channel, audio);
        }

        private static bool TryGetVoiceChannel(SocketCommandContext context, string argument, out IVoiceChannel voiceChannel)
        {
            if (ulong.TryParse(argument, out var channelId))
            {
                voiceChannel = context.Guild.GetVoiceChannel(channelId);
            }
            else
            {
                var name = argument.ToLower();
                voiceChannel = context.Guild.VoiceChannels.FirstOrDefault(x => x.Name.ToLower() == argument);
            }
            return !(voiceChannel is null);

        }

        private static Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -af \"adelay=50|50\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
        }

        private static async Task<double> GetDuration(string path)
        {
            using (var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"-show_entries format=duration -of default=noprint_wrappers=1:nokey=1 -i \"{path}\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            }))
            {
                var result = await proc.StandardOutput.ReadToEndAsync();
                return Convert.ToDouble(result);
            }
        }

        private async Task Play(IVoiceChannel vc, Audio audio)
        {
            var path = Path.Combine(Settings.AudioPath, audio.Path);
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
