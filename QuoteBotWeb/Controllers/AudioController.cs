using Domain.Models;
using Domain.Repositories;
using Domain.Services;
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
        private readonly IAudioOwnerRepo audioOwnerRepo;
        private readonly IUserService userService;

        public AudioController(IAudioRepo audioRepo,
                               IMemoryCache memoryCache,
                               IAudioOwnerRepo audioOwnerRepo,
                               IUserService userService)
        {
            this.audioRepo = audioRepo;
            this.memoryCache = memoryCache;
            this.audioOwnerRepo = audioOwnerRepo;
            this.userService = userService;
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
            var guildsTask = userService.GetUserGuilds(authEntry);
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
    }
}
