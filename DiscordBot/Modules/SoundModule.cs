using Amazon.S3;
using Discord;
using Discord.Audio;
using Discord.Commands;
using DiscordBot.Options;
using DiscordBot.Services;
using Domain.Models;
using Domain.Repos;
using Microsoft.Extensions.Options;
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
        private readonly ICategoryRepo categoryRepo;
        private readonly IAmazonS3 s3Client;
        private readonly BotOptions botOptions;

        public SoundModule(
            IAudioRepo audioRepo,
            StatsService statsService,
            IUserRepo userRepo,
            IDiscordClient client,
            CommandHandler commandHandler,
            ICategoryRepo categoryRepo,
            IAmazonS3 s3Client,
            IOptions<BotOptions> botOptions)
        {
            this.audioRepo = audioRepo;
            this.statsService = statsService;
            this.userRepo = userRepo;
            this.client = client;
            this.commandHandler = commandHandler;
            this.categoryRepo = categoryRepo;
            this.s3Client = s3Client;
            this.botOptions = botOptions.Value;
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
                var categories = await categoryRepo.GetAllCategoriesWithAudio();
                await context.Reply(string.Join("\n", categories.Select(x => $"{botOptions.Prefix}{x.Name}")));
            }
            else if (parts.Length == 2)
            {
                var sounds = await audioRepo.GetAllAudioForCategory(parts[1]);
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
            var history = statsService.GetHistory(context.Guild.Id);
            history.Reverse();
            await context.Reply(string.Join("\n", history.Select(x => $"!{x.Item1.Name} {x.Item2.Name}")));
        }
        
        //TODO: Setup with s3
        // [MyCommand("upload")]
        // public async Task Upload(SocketCommandContext context)
        // {
        //     var hasUploadRole = await userRepo.UserHasAnyRole(context.User.Id, "Upload");
        //     var isOwner = context.IsBotOwner();
        //     if (!(hasUploadRole || isOwner))
        //     {
        //         await context.Reply("no");
        //         return;
        //     }
        //     var attachments = context.Message.Attachments;
        //     if (attachments.Count != 1)
        //     {
        //         await context.Reply("The message must contain 1 audio attachment.");
        //         return;
        //     }
        //     int argPos = 0;
        //     context.Message.HasPrefix(client.CurrentUser, ref argPos);
        //     var commandParts = context.Message.Content.Substring(argPos).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        //     if (commandParts.Length != 3)
        //     {
        //         await context.Reply("!upload category name");
        //         return;
        //     }
        //     var attachment = attachments.First();
        //     var category = commandParts[1].ToLower();
        //     var name = commandParts[2].ToLower();
        //     var validChars = "abcdefghijklmnopqrstuvwxyz0123456789-_";
        //     if (category.Except(validChars).Any())
        //     {
        //         await context.Reply($"Invalid category, only use the following characters: {validChars}");
        //         return;
        //     }
        //     if (name.Except(validChars).Any())
        //     {
        //         await context.Reply($"Invalid name, only use the following characters: {validChars}");
        //         return;
        //     }
        //     if (commandHandler.GetCommands().Contains(category))
        //     {
        //         await context.Reply("Invalid command category!");
        //         return;
        //     }
        //     var currentAudio = await audioRepo.GetAllAudioForCategory(category);
        //     if (currentAudio.Any(x => x.Name == name))
        //     {
        //         await context.Reply("A quote already exists with that name");
        //         return;
        //     }

        //     try
        //     {
        //         var newName = Guid.NewGuid().ToString();
        //         var tempPath = Path.Combine(botOptions.TempPath, newName);
        //         new WebClient().DownloadFile(attachment.Url, tempPath);
        //         var duration = await GetDuration(tempPath);
        //         if (duration > 30)
        //         {
        //             await context.Reply("30 seconds maximum");
        //             return;
        //         }
        //         var newPath = Path.Combine(botOptions.AudioPath, newName);
        //         File.Move(tempPath, newPath);
        //         await audioRepo.AddAudio(new Audio
        //         {
        //             Name = name,
        //             Path = newName
        //         });
        //         await context.Reply($"Done, you can now do\n{botOptions.Prefix}{category} {name}");
        //     }
        //     catch (Exception ex)
        //     {
        //         await context.Reply("Something went wrong. Are you sure thats an audio file?");
        //     }
        // }

        [MyCommand("random")]
        public async Task Rnaomd(SocketCommandContext context)
        {
            // If the bot is already in a voice channel in the server, then don't play a quote
            if (!((context.Guild.CurrentUser as IGuildUser)?.VoiceChannel is null))
            {
                await context.Reply("I am already doing a quote calm the fuck down.");
                return;
            }
            var category = await GetRandomCategory(context.User.Id);
            var audio = await GetRandomAudioForCategory(context.Guild.Id, category);

            var channel = (context.User as IGuildUser)?.VoiceChannel;
            if (channel is null)
            {
                await context.Reply("You gotta be in a voice channel");
                return;
            }
            statsService.AddToHistory(context.Guild.Id, category, audio);
            await Play(channel, audio);
        }

        public async Task PlaySound(SocketCommandContext context)
        {
            // If the bot is already in a voice channel in the server, then don't play a quote
            if (!((context.Guild.CurrentUser as IGuildUser)?.VoiceChannel is null))
            {
                await context.Reply("I am already doing a quote calm the fuck down.");
                return;
            }
            var argPos = 0;
            context.Message.HasPrefix(botOptions.Prefix, client.CurrentUser, ref argPos);
            var commandParts = context.Message.Content.Substring(argPos).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var msgCategory = commandParts[0].ToLower();
            IVoiceChannel channel = null;
            string msgQuote = null;
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
                    if (!(msgQuote is null))
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
                    msgQuote = commandParts[i].ToLower();
                }
                else if (msgQuote is null)
                {
                    msgQuote = part;
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
            Category category;
            // Katie isn't allow to use her own voice
            if ((context.User.Id == 302955588327833622 && msgCategory == "heck") ||
                (context.User.Id == 181537270526902272 && msgCategory == "chacons") ||
                (context.User.Id == 336341485655949313 && msgCategory == "zach") ||
                (context.User.Id == 218600945372758016 && msgCategory == "saxton") ||
                (context.User.Id == 155123403383242753 && msgCategory == "ted"))
            {
                category = await categoryRepo.GetCategoryByName("trump");
                audio = await audioRepo.GetAudio("trump", "i-dont-think-so");
            }
            else if (msgQuote is null)
            {
                category = await categoryRepo.GetCategoryByName(msgCategory);
                audio = await GetRandomAudioForCategory(context.Guild.Id, category);
            }
            else
            {
                category = await categoryRepo.GetCategoryByName(msgCategory);
                audio = await audioRepo.GetAudio(msgCategory, msgQuote);
                if (audio is null)
                {
                    await context.Reply("That quote don't exist...");
                    return;
                }
            }
            statsService.AddToHistory(context.Guild.Id, category, audio);
            Console.WriteLine($"Playing {audio.Name}: {audio.Path}");
            await Play(channel, audio);
        }

        private async Task<Category> GetRandomCategory(ulong userId)
        {
            var allCategories = await categoryRepo.GetAllCategoriesWithAudio();
            switch (userId)
            {
                case 302955588327833622:
                    allCategories.RemoveAll(x => x.Name == "hexk");
                    break;
                case 181537270526902272:
                    allCategories.RemoveAll(x => x.Name == "chacons");
                    break;
                case 336341485655949313:
                    allCategories.RemoveAll(x => x.Name == "zach");
                    break;
                case 218600945372758016:
                    allCategories.RemoveAll(x => x.Name == "saxton");
                    break;
                case 155123403383242753:
                    allCategories.RemoveAll(x => x.Name == "ted");
                    break;
                default:
                    break;
            }
            var index = StaticRandom.Next(allCategories.Count);
            return allCategories[index];
        }

        private async Task<Audio> GetRandomAudioForCategory(ulong serverId, Category category)
        {
            var playlist = await audioRepo.GetAllAudioForCategory(category.Id);
            var takeAmount = Math.Max(1, Math.Min(botOptions.RecentCountValue, playlist.Count - 2));
            var history = statsService.GetHistory(serverId).AsEnumerable().Reverse().Take(takeAmount).Select(x => x.Item2);
            var available = playlist.Except(history).ToList();
            var index = StaticRandom.Next(available.Count);
            return available[index];
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

        private static Process CreateStream()
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i pipe:0 -af \"adelay=50|50\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
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
            var s3object = await s3Client.GetObjectAsync("quotebot-audio", audio.Path);
            var audioClient = await vc.ConnectAsync();
            using (var ffmpeg = CreateStream())
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = audioClient.CreatePCMStream(AudioApplication.Voice, bufferMillis: 1))
            using (var memStream = new MemoryStream())
            {
                try
                {   
                    using (var input = ffmpeg.StandardInput.BaseStream)
                    {
                        await s3object.ResponseStream.CopyToAsync(input);
                    }
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
