using Discord.Responses;
using Domain.Models;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuoteBotWeb.Controllers
{
    public class AudioCategoryController : Controller
    {
        private readonly IAudioCategoryRepo audioCategoryRepo;
        private readonly IAudioOwnerRepo audioOwnerRepo;
        private readonly ICategoryRepo categoryRepo;
        private readonly IMemoryCache memoryCache;
        private readonly IDiscordHttp discordHttp;

        public AudioCategoryController(IAudioCategoryRepo audioCategoryRepo,
                                       IAudioOwnerRepo audioOwnerRepo,
                                       ICategoryRepo categoryRepo,
                                       IMemoryCache memoryCache,
                                       IDiscordHttp discordHttp)
        {
            this.audioCategoryRepo = audioCategoryRepo;
            this.audioOwnerRepo = audioOwnerRepo;
            this.categoryRepo = categoryRepo;
            this.memoryCache = memoryCache;
            this.discordHttp = discordHttp;
        }

        public async Task<IActionResult> Create([FromQuery(Name = "audio")] uint audioOwnerId,
                                                [FromQuery(Name = "category")] uint categoryId,
                                                string redirect = null)
        {
            var authEntry = HttpContext.GetAuthEntry();
            if (authEntry is null)
            {
                return Redirect("/login");
            }

            var audioOwner = await audioOwnerRepo.GetById(audioOwnerId);
            var category = await categoryRepo.GetCategory(categoryId);

            // Ensure that audioOwner and category have the same ownerId
            if (audioOwner.OwnerId != category.OwnerId)
            {
                return BadRequest();
            }

            // ensure the user has permissions to edit the guild
            var userGuilds = await GetUserGuilds(authEntry);
            if (!userGuilds.Any(x => x.Id == audioOwner.OwnerId))
            {
                return Unauthorized();
            }

            await audioCategoryRepo.Create(audioOwnerId, categoryId);

            return string.IsNullOrWhiteSpace(redirect) ? LocalRedirect("/") : LocalRedirect(redirect);
        }
        public async Task<IActionResult> Delete([FromQuery(Name = "audio")] uint audioOwnerId,
                                                [FromQuery(Name = "category")] uint categoryId,
                                                string redirect = null)
        {
            var authEntry = HttpContext.GetAuthEntry();
            if (authEntry is null)
            {
                return Redirect("/login");
            }

            var audioOwner = await audioOwnerRepo.GetById(audioOwnerId);
            var category = await categoryRepo.GetCategory(categoryId);

            // Ensure that audioOwner and category have the same ownerId
            if (audioOwner.OwnerId != category.OwnerId)
            {
                return BadRequest();
            }

            // ensure the user has permissions to edit the guild
            var userGuilds = await GetUserGuilds(authEntry);
            if (!userGuilds.Any(x => x.Id == audioOwner.OwnerId))
            {
                return Unauthorized();
            }

            await audioCategoryRepo.Delete(audioOwnerId, categoryId);

            return string.IsNullOrWhiteSpace(redirect) ? LocalRedirect("/") : LocalRedirect(redirect);
        }

        private async Task<List<Domain.Models.UserGuild>> GetUserGuilds(AuthEntry authEntry)
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
