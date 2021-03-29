using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface IUserService
    {
        Task<List<UserGuild>> GetUserGuilds(AuthEntry authEntry);
    }
}
