using Domain.Models;
using Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuoteBotWeb.Controllers
{
    public class AudioController : Controller
    {
        private readonly IAudioRepo audioRepo;
        private readonly IMemoryCache memoryCache;
        private readonly IDiscordHttp discordHttp;
        private readonly IAudioOwnerRepo audioOwnerRepo;

        public AudioController(IAudioRepo audioRepo, IMemoryCache memoryCache, IDiscordHttp discordHttp, IAudioOwnerRepo audioOwnerRepo)
        {
            this.audioRepo = audioRepo;
            this.memoryCache = memoryCache;
            this.discordHttp = discordHttp;
            this.audioOwnerRepo = audioOwnerRepo;
        }

        [Route("{controller}/{id}")]
        public async Task<IActionResult> Index(uint id)
        {
            var authEntry = HttpContext.GetAuthEntry();
            if (authEntry is null)
            {
                return Redirect("/login");
            }
            var authorized = await IsAllowedAudio(authEntry, id);
            if (!authorized)
            {
                return Unauthorized();
            }
            var audio = await audioRepo.GetAudioById(id);
            var str = await audioRepo.GetFileFromPath(audio.Path);
            return Redirect(str);
        }

        public async Task<bool> IsAllowedAudio(AuthEntry authEntry, uint id)
        {
            var guildsTask = GetUserGuilds(authEntry);
            var audioOwnersTask = memoryCache.GetOrCreateAsync($"GetAudioOwnersByAudio({id})",
                async (cacheEntry) =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
                    return await audioOwnerRepo.GetAudioOwnersByAudio(id);
                });
            await Task.WhenAll(guildsTask, audioOwnersTask);
            var ownerIds = guildsTask.Result.Select(x => x.Id).Concat(authEntry.UserId.Yield()).ToList();
            return ownerIds.Join(audioOwnersTask.Result, ownerId => ownerId, ao => ao.OwnerId, (outer, inner) => (outer, inner)).Any();
        }


        private async Task<List<UserGuild>> GetUserGuilds(AuthEntry authEntry)
        {
            var guilds = await memoryCache.GetOrCreateAsync($"userguilds={authEntry.UserId}", async (cacheEntry) =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);
                return await discordHttp.GetCurrentUserGuilds(authEntry.AccessToken);
            });
            var validGuilds = guilds.Where(x => x.Owner || (x.PermissionsInt & (uint)Permissions.Administrator) == (uint)Permissions.Administrator).ToList();
            return validGuilds;
        }
    }
}
