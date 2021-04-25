using Domain.Models.Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IDiscordHttp
    {
        Task<AccessTokenResponse> GetAccessToken(string code);
        Task<User> GetCurrentUser(string bearerToken);
        Task<List<UserGuild>> GetCurrentUserGuilds(string bearerToken);
        string GetRedirectUri(string state);
        Task<List<GuildRole>> GetGuildRoles(TokenType tokenType, string bearerToken, ulong guildId);
        Task<List<GuildChannel>> GetGuildChannels(TokenType tokenType, string bearerToken, ulong guildId);
        Task<GuildMember> GetGuildMember(string bearerToken, ulong guildId, ulong userId);
    }
}
