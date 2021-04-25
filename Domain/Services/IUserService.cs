using Domain.Models;
using Domain.Models.Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface IUserService
    {
        Task<List<UserGuild>> GetUserGuilds(AuthEntry authEntry);
        Task<List<UserGuild>> GetAllowedUserGuilds(AuthEntry authEntry);
        Task<List<UserGuild>> AllowedGuildsFilter(List<UserGuild> guilds, AuthEntry authEntry);
    }
}
