using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IDiscordHttp
    {
        Task<AccessTokenResponse> GetAccessToken(string code);
        Task<GetCurrentUserResponse> GetCurrentUser(string bearerToken);
        Task<List<UserGuild>> GetCurrentUserGuilds(string bearerToken);
        string GetRedirectUri(string state);
    }
}
