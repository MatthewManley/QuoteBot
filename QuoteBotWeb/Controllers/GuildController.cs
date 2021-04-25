using Domain.Models;
using Domain.Models.Discord;
using Domain.Options;
using Domain.Repositories;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using QuoteBotWeb.Models.Guild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuoteBotWeb.Controllers
{
    public class GuildController : Controller
    {
        private readonly IUserService userService;
        private readonly IDiscordHttp discordHttp;
        private readonly IServerRepo serverRepo;
        private readonly IMemoryCache memoryCache;
        private readonly IOptions<AuthOptions> authOptions;

        public GuildController(IQuoteBotRepo quoteBotRepo,
                               IAudioOwnerRepo audioOwnerRepo,
                               IAudioProcessingService audioProcessingService,
                               IUserService userService,
                               IDiscordHttp discordHttp,
                               IServerRepo serverRepo,
                               IMemoryCache memoryCache,
                               IOptions<AuthOptions> authOptions)
        {
            this.userService = userService;
            this.discordHttp = discordHttp;
            this.serverRepo = serverRepo;
            this.memoryCache = memoryCache;
            this.authOptions = authOptions;
        }

        public async Task<IActionResult> Index()
        {
            var authEntry = (AuthEntry)HttpContext.Items["key"];
            if (authEntry is null)
            {
                return Redirect("/login");
            }

            var userGuilds = await userService.GetAllowedUserGuilds(authEntry);
            var viewmodel = new Models.Guild.IndexViewModel
            {
                Guilds = userGuilds
            };
            return View(viewmodel);
        }

        [HttpGet("Guild/{server}/Config")]
        public async Task<IActionResult> Config([FromRoute] ulong server)
        {
            var authEntry = (AuthEntry)HttpContext.Items["key"];
            if (authEntry is null)
            {
                return Redirect("/login");
            }

            // Get the current users guilds
            var userGuilds = await userService.GetUserGuilds(authEntry);

            // if the user is not in the requested guild, return unauthorized
            var currentGuild = userGuilds.FirstOrDefault(x => x.Id == server);
            if (currentGuild is null)
                return Unauthorized();

            // determine if user has admin permissions on the current guild
            var isAdmin = currentGuild.Owner || (currentGuild.PermissionsInt & (uint)Permissions.Administrator) == (uint)Permissions.Administrator;
            var serverConfig = await CachedGetServerConfig(server);

            // detmine if the user has moderator permissions on the current guild
            bool isModerator = false;
            if (isAdmin)
            {
                isModerator = true; // if user is an admin, then they are a moderator
            }
            else if (serverConfig.ModeratorRole != null)
            {
                var currentUserGuildMember = await CachedGetGuildMember(authEntry.AccessToken, server, authEntry.UserId);
                isModerator = currentUserGuildMember.Roles.Any(x => x == serverConfig.ModeratorRole);
            }

            // If a user is neither an admin nor moderator, then they cannot access this page
            if (!(isAdmin || isModerator))
            {
                return Unauthorized();
            }

            var serverRolesTask = CachedGetGuildRoles(server);
            var serverChannels = await CachedGetGuildChannels(server);

            var textChannels = serverChannels.Where(x => x.ChannelType == GuildChannelType.Text).OrderBy(x => x.Position).ToList();
            var voiceChannels = serverChannels.Where(x => x.ChannelType == GuildChannelType.Voice || x.ChannelType == GuildChannelType.StageVoice).OrderBy(x => x.Position).ToList();

            var serverRoles = (await serverRolesTask).Where(x => x.Position != 0).OrderByDescending(x => x.Position).ToList();
            return View(new ConfigViewModel(serverConfig, serverRoles, textChannels, voiceChannels, isAdmin, isModerator));
        }

        [HttpPost("Guild/{server}/Config")]
        public async Task<IActionResult> Config([FromRoute] ulong server, ConfigPostBody configPostBody)
        {
            var authEntry = (AuthEntry)HttpContext.Items["key"];
            if (authEntry is null)
            {
                return Redirect("/login");
            }

            // Get the current users guilds
            var userGuilds = await userService.GetUserGuilds(authEntry);

            // if the user is not in the requested guild, return unauthorized
            var currentGuild = userGuilds.FirstOrDefault(x => x.Id == server);
            if (currentGuild is null)
                return Unauthorized();

            // determine if user has admin permissions on the current guild
            var isAdmin = currentGuild.Owner || (currentGuild.PermissionsInt & (uint)Permissions.Administrator) == (uint)Permissions.Administrator;
            var serverConfig = await CachedGetServerConfig(server);

            // detmine if the user has moderator permissions on the current guild
            bool isModerator = false;
            if (isAdmin)
            {
                isModerator = true; // if user is an admin, then they are a moderator
            }
            else if (serverConfig.ModeratorRole != null)
            {
                var currentUserGuildMember = await CachedGetGuildMember(authEntry.AccessToken, server, authEntry.UserId);
                isModerator = currentUserGuildMember.Roles.Any(x => x == serverConfig.ModeratorRole);
            }

            // If a user is neither an admin nor moderator, then they cannot submit this page
            if (!(isAdmin || isModerator))
            {
                return Unauthorized();
            }

            var serverChannelsTask = CachedGetGuildChannels(server);
            var serverRolesTask = CachedGetGuildRoles(server);

            ulong? moderatorRole = null;
            if (isAdmin)
            {
                if (configPostBody.ModeratorRole == "none")
                    moderatorRole = null;
                else if (ulong.TryParse(configPostBody.ModeratorRole, out var modRole))
                    moderatorRole = modRole;
                else
                    return BadRequest();
            }
            else
            {
                moderatorRole = serverConfig.ModeratorRole;
            }

            if (configPostBody.TextListType != "ALLOW" && configPostBody.TextListType != "BLOCK")
                return BadRequest();

            if (configPostBody.VoiceListType != "ALLOW" && configPostBody.VoiceListType != "BLOCK")
                return BadRequest();

            var prefix = configPostBody.Prefix.Trim();
            if (prefix.Length > 6 || prefix.Any(x => char.IsWhiteSpace(x)))
                return BadRequest();

            var guildChannels = await serverChannelsTask;
            var guildChannelIdSet = guildChannels.Select(x => x.Id).ToHashSet();

            // Ensure that all posted channels actually exist within the guild
            if (configPostBody.TextChannels != null && configPostBody.TextChannels.Any(x => !guildChannelIdSet.Contains(x)))
                return BadRequest();

            if (configPostBody.VoiceChannels != null && configPostBody.VoiceChannels.Any(x => !guildChannelIdSet.Contains(x)))
                return BadRequest();

            var emptyList = new List<ulong>();
            var newServerConfig = new ServerConfig
            {
                ModeratorRole = moderatorRole,
                Prefix = prefix,
                ServerId = server,
                TextChannelListType = configPostBody.TextListType,
                VoiceChannelListType = configPostBody.VoiceListType,
                TextChannelList = configPostBody.TextChannels ?? emptyList,
                VoiceChannelList = configPostBody.VoiceChannels ?? emptyList
            };
            await serverRepo.PutServerConfig(newServerConfig);


            var textChannels = guildChannels.Where(x => x.ChannelType == GuildChannelType.Text).OrderBy(x => x.Position).ToList();
            var voiceChannels = guildChannels.Where(x => x.ChannelType == GuildChannelType.Voice || x.ChannelType == GuildChannelType.StageVoice).OrderBy(x => x.Position).ToList();
            var guildRoles = await serverRolesTask;
            return View(new ConfigViewModel(newServerConfig, guildRoles, textChannels, voiceChannels, isAdmin, isModerator));
        }

        private readonly TimeSpan CacheTime = TimeSpan.FromSeconds(15);

        private async Task<T> CacheHelper<T>(string key, Func<Task<T>> getter) =>
            await memoryCache.GetOrCreateAsync(key, async (cacheEntry) =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = CacheTime;
                return await getter();
            });

        private async Task<GuildMember> CachedGetGuildMember(string accessToken, ulong serverId, ulong userId) =>
            await CacheHelper($"guildmember={serverId}={userId}", async () => await discordHttp.GetGuildMember(accessToken, serverId, userId));

        private async Task<List<GuildRole>> CachedGetGuildRoles(ulong serverId) =>
            await CacheHelper($"guildroles={serverId}", async () => await discordHttp.GetGuildRoles(TokenType.Bot, authOptions.Value.BotKey, serverId));

        private async Task<List<GuildChannel>> CachedGetGuildChannels(ulong serverId) =>
            await CacheHelper($"guildchannels={serverId}", async () => await discordHttp.GetGuildChannels(TokenType.Bot, authOptions.Value.BotKey, serverId));

        private async Task<ServerConfig> CachedGetServerConfig(ulong serverId) =>
            await CacheHelper($"serverconfig={serverId}", async () => await serverRepo.GetServerConfig(serverId));
    }
}
