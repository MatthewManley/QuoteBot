using Domain.Models;
using Domain.Repositories;
using Domain.Services;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class UserService : IUserService
    {
        private readonly IMemoryCache memoryCache;
        private readonly IDiscordHttp discordHttp;

        public UserService(IMemoryCache memoryCache, IDiscordHttp discordHttp)
        {
            this.memoryCache = memoryCache;
            this.discordHttp = discordHttp;
        }

        public async Task<List<UserGuild>> GetUserGuilds(AuthEntry authEntry)
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
