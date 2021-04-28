using Amazon.S3;
using Discord;
using Discord.Audio;
using Discord.Commands;
using DiscordBot.Services;
using Domain;
using Domain.Models;
using Domain.Options;
using Domain.Repositories;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class SoundModule : MyCommandSet
    {
        private readonly IQuoteBotRepo quoteBotRepo;
        private readonly StatsService statsService;
        private readonly IAmazonS3 s3Client;
        private readonly IAudioRepo audioRepo;
        private readonly BotOptions botOptions;

        public SoundModule(
            IQuoteBotRepo quoteBotRepo,
            StatsService statsService,
            IAmazonS3 s3Client,
            IOptions<BotOptions> botOptions,
            IAudioRepo audioRepo)
        {
            this.quoteBotRepo = quoteBotRepo;
            this.statsService = statsService;
            this.s3Client = s3Client;
            this.audioRepo = audioRepo;
            this.botOptions = botOptions.Value;
        }

        [MyCommand("list")]
        public async Task List(SocketCommandContext context, string[] command, ServerConfig serverConfig)
        {
            if (command.Length == 1)
            {
                await ListCategories(context, 1);
                return;
            }
            else if (command.Length == 2)
            {
                if (int.TryParse(command[1], out var result))
                {
                    await ListCategories(context, result);
                    return;
                }
                else
                {
                    await ListAudioForCategory(context, command[1], 1);
                    return;
                }
            }
            else if (command.Length == 3)
            {
                if (int.TryParse(command[2], out var result))
                {
                    await ListAudioForCategory(context, command[1], result);
                    return;
                }
            }
        }

        private async Task ListAudioForCategory(SocketCommandContext context, string category, int page)
        {
            var sounds = await quoteBotRepo.GetAudioInCategory(context.Guild.Id, category.ToLowerInvariant()).ToList();
            if (sounds.Count == 0)
            {
                await context.Reply("No sounds found for that category.");
                return;
            }
            var pageMinusOne = page - 1;
            const int soundsPerPage = 25;
            var pages = sounds.Count / soundsPerPage;
            if (pageMinusOne > pages)
            {
                var errorMsg = pages == 0 ?
                    "There is only 1 page" :
                    $"There are only {pages + 1} pages.";
                await context.Reply(errorMsg);
                return;
            }
            IEnumerable<NamedAudio> display = sounds.OrderBy(x => x.AudioOwner.Name);
            var msg = new StringBuilder();
            msg.Append("Total Quotes: ");
            msg.Append(sounds.Count);
            msg.AppendLine();
            if (pages > 0)
            {
                display = display.Skip(soundsPerPage * pageMinusOne).Take(soundsPerPage);
                msg.Append("Page ");
                msg.Append(page);
                msg.Append("/");
                msg.Append(pages + 1);
                msg.AppendLine();
            }
            msg.Append("----------");
            msg.AppendLine();
            foreach (var sound in display)
            {
                msg.Append(sound.AudioOwner.Name);
                msg.AppendLine();
            }
            await context.Reply(msg.ToString());
        }

        private async Task ListCategories(SocketCommandContext context, int page)
        {
            var categories = await quoteBotRepo.GetCategoriesWithAudio(context.Guild.Id).ToList();
            var pageMinusOne = page - 1;
            const int categoriesPerPage = 30;
            var pages = categories.Count() / categoriesPerPage;
            if (pageMinusOne > pages)
            {
                var errorMsg = pages == 0 ?
                    "There is only 1 page." :
                    $"There are only {pages + 1} pages.";
                await context.Reply(errorMsg);
                return;
            }
            IEnumerable<Category> display = categories.OrderBy(x => x.Name);
            bool displayPageHeader = false;
            if (pages > 0)
            {
                displayPageHeader = true;
                display = display.Skip(categoriesPerPage * pageMinusOne).Take(categoriesPerPage);
            }
            var msg = new StringBuilder();
            msg.Append("Total Categories: ");
            msg.Append(categories.Count);
            msg.AppendLine();
            if (displayPageHeader)
            {
                msg.Append("Page ");
                msg.Append(page);
                msg.Append("/");
                msg.Append(pages + 1);
                msg.AppendLine();
            }
            msg.Append("----------");
            msg.AppendLine();
            foreach (var category in display)
            {
                msg.Append(category.Name);
                msg.AppendLine();
            }
            await context.Reply(msg.ToString());
        }

        [MyCommand("history")]
        public async Task History(SocketCommandContext context, string[] command, ServerConfig serverConfig)
        {
            var history = statsService.GetHistory(context.Guild.Id);
            await context.Reply(string.Join("\n", history.Select(x => $"{x.Category.Name} {x.NamedAudio.AudioOwner.Name}")));
        }

        [MyCommand("random")]
        public async Task Random(SocketCommandContext context, string[] command, ServerConfig serverConfig)
        {
            // If the bot is already in a voice channel in the server, then don't play a quote
            if (!((context.Guild.CurrentUser as IGuildUser)?.VoiceChannel is null))
            {
                await context.Reply("I am already doing a quote calm down.");
                return;
            }
            var category = await GetRandomCategory(context.Guild.Id);
            var audio = await GetRandomAudioForCategory(context.Guild.Id, category.Id);

            var channel = (context.User as IGuildUser)?.VoiceChannel;
            if (channel is null)
            {
                await context.Reply("You gotta be in a voice channel");
                return;
            }
            switch (serverConfig.VoiceChannelListType)
            {
                case "ALLOW":
                    if (!serverConfig.VoiceChannelList.Contains(channel.Id))
                    {
                        await context.Reply("You cannot play quotes in that voice channel!");
                        return;
                    }                        
                    break;
                case "BLOCK":
                    if (serverConfig.VoiceChannelList.Contains(channel.Id))
                    {
                        await context.Reply("You cannot play quotes in that voice channel!");
                        return;
                    }
                    break;
                default:
                    break;
            }
            statsService.AddToHistory(context.Guild.Id, new HistoryEntry
            {
                Category = category,
                NamedAudio = audio,
            });
            await Play(channel, audio.Audio, CancellationToken.None);
        }

        public async Task PlaySound(SocketCommandContext context, string[] command, ServerConfig serverConfig)
        {
            // If the bot is already in a voice channel in the server, then don't play a quote
            if (!((context.Guild.CurrentUser as IGuildUser)?.VoiceChannel is null))
            {
                await context.Reply("I am already doing a quote calm down.");
                return;
            }
            var msgCategory = command[0].ToLowerInvariant();
            IVoiceChannel channel = null;
            string msgQuote = null;
            for (int i = 1; i < command.Length; i++)
            {
                var part = command[i].ToLower();
                if (part.Equals("-c") || part.Equals("--channel"))
                {
                    if (!(channel is null))
                    {
                        await context.Reply("How many voice channels you think I can take at once??? Cause I think its one...");
                        return;
                    }
                    // Error if there is no channel after -c or its not a proper mention
                    if (i + 1 >= command.Length ||
                        !TryGetVoiceChannel(context, command[i + 1], out var voiceChannel))
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
                    if (i + 1 >= command.Length)
                    {
                        await context.Reply("Don't leave me hanging, say what quote you want played.");
                        return;
                    }
                    i++;
                    msgQuote = command[i].ToLower();
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
            channel ??= (context.User as IGuildUser)?.VoiceChannel;
            if (channel is null)
            {
                await context.Reply("You gotta be in a voice channel or use -c channelName");
                return;
            }
            else if (serverConfig.VoiceChannelListType == "BLOCK")
            {
                if (serverConfig.VoiceChannelList.Contains(channel.Id))
                {
                    await context.Reply("You cannot play quotes in that voice channel!");
                    return;
                }
            }
            else if (serverConfig.VoiceChannelListType == "ALLOW")
            {
                if (!serverConfig.VoiceChannelList.Contains(channel.Id))
                {
                    await context.Reply("You cannot play quotes in that voice channel!");
                    return;
                }
            }
            NamedAudio namedAudio;
            if (msgQuote is null)
            {
                namedAudio = await GetRandomAudioForCategory(context.Guild.Id, msgCategory);
            }
            else
            {
                namedAudio = await quoteBotRepo.GetAudio(context.Guild.Id, msgCategory, msgQuote);
                if (namedAudio is null)
                {
                    await context.Reply("That quote don't exist...");
                    return;
                }
            }
            statsService.AddToHistory(context.Guild.Id, new HistoryEntry
            {
                Category = new Category
                {
                    Name = msgCategory
                },
                NamedAudio = namedAudio
            });
            await Play(channel, namedAudio.Audio, CancellationToken.None);
        }

        private async Task<Category> GetRandomCategory(ulong serverId)
        {
            //TODO: handle error if there are no categories
            var allCategories = await quoteBotRepo.GetCategoriesWithAudio(serverId).ToList();
            var index = StaticRandom.Next(allCategories.Count);
            return allCategories[index];
        }

        private async Task<NamedAudio> GetRandomAudioForCategory(ulong serverId, string category)
        {
            var playlist = await quoteBotRepo.GetAudioInCategory(serverId, category);
            return SelectRandomAudio(serverId, playlist.ToList());
        }

        private async Task<NamedAudio> GetRandomAudioForCategory(ulong serverId, uint categoryId)
        {
            var playlist = await quoteBotRepo.GetAudioInCategory(categoryId).ToList();
            return SelectRandomAudio(serverId, playlist);
        }

        private NamedAudio SelectRandomAudio(ulong serverId, List<NamedAudio> playlist)
        {
            var takeAmount = Math.Max(1, Math.Min(botOptions.RecentCountValue, playlist.Count - 2));
            var historyAudio = statsService.GetHistory(serverId).AsEnumerable().Take(takeAmount).Select(x => x.NamedAudio.Audio.Id).ToList();
            var available = playlist.Where(x => !historyAudio.Contains(x.Audio.Id)).ToList();
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
                voiceChannel = context.Guild.VoiceChannels.FirstOrDefault(x => x.Name.ToLower() == name);
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
            using var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"-show_entries format=duration -of default=noprint_wrappers=1:nokey=1 -i \"{path}\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            });
            var result = await proc.StandardOutput.ReadToEndAsync();
            return Convert.ToDouble(result);
        }

        public async Task Play(IVoiceChannel vc, Audio audio, CancellationToken cancellationToken)
        {

            using var s3object = await s3Client.GetObjectAsync("quotebot-audio-post", audio.Path, cancellationToken);
            using var ffmpeg = CreateStream();
            using var output = ffmpeg.StandardOutput.BaseStream;
            var inputTask = Task.Run(async () =>
            {
                using var input = ffmpeg.StandardInput.BaseStream;
                await s3object.ResponseStream.CopyToAsync(input, cancellationToken);
            });
            using var audioClient = await vc.ConnectAsync();
            using var discord = audioClient.CreatePCMStream(AudioApplication.Mixed, bufferMillis: 1);
            try
            {
                await output.CopyToAsync(discord, cancellationToken);
                await inputTask;
            }
            finally
            {
                await discord.FlushAsync(cancellationToken);
            }

            await vc.DisconnectAsync();
        }
    }
}
