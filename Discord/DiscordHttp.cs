using Discord.Models;
using Discord.Responses;
using Domain.Repositories;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Discord
{
    public class DiscordHttp : IDiscordHttp
    {
        private readonly HttpClient httpClient;
        private readonly DiscordOptions options;

        public DiscordHttp(HttpClient httpClient, IOptions<DiscordOptions> options)
        {
            this.httpClient = httpClient;
            this.options = options.Value;
        }

        public async Task<Domain.Models.Discord.AccessTokenResponse> GetAccessToken(string code)
        {
            var uri = new Uri($"{options.BaseUrl}/oauth2/token");
            var form_content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", options.ClientId),
                new KeyValuePair<string, string>("client_secret", options.ClientSecret),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", options.RedirectUri),
                new KeyValuePair<string, string>("scope", "identify guilds"),
            });
            var response = await httpClient.PostAsync(uri, form_content);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsObjectAsync<AccessTokenResponse>();
            return result.MapToDomain();
        }

        public string GetRedirectUri(string state)
        {
            var uriString = Uri.EscapeDataString(options.RedirectUri);
            return $"https://discord.com/api/oauth2/authorize?client_id={options.ClientId}&redirect_uri={uriString}&response_type=code&scope=identify%20guilds&state={state}&prompt=none";

        }

        public async Task<Domain.Models.Discord.User> GetCurrentUser(string bearerToken)
        {
            var uri = new Uri($"{options.BaseUrl}/users/@me");
            var request = new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Get
            };
            request.Headers.Add("Authorization", $"Bearer {bearerToken}");
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsObjectAsync<GetCurrentUserResponse>();
            return result.MapToDomain();
        }

        public async Task<List<Domain.Models.Discord.UserGuild>> GetCurrentUserGuilds(string bearerToken)
        {
            var uri = new Uri($"{options.BaseUrl}/users/@me/guilds");
            var request = new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Get
            };
            request.Headers.Add("Authorization", $"Bearer {bearerToken}");
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsObjectAsync<List<UserGuild>>();
            return result.Select(x => x.MapToDomain()).ToList();
        }

        public async Task<List<Domain.Models.Discord.GuildRole>> GetGuildRoles(Domain.Models.Discord.TokenType tokenType, string bearerToken, ulong guildId)
        {
            var uri = new Uri($"{options.BaseUrl}/guilds/{guildId}/roles");
            var request = new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Get
            };
            switch (tokenType)
            {
                case Domain.Models.Discord.TokenType.Bearer:
                    request.Headers.Add("Authorization", $"Bearer {bearerToken}");
                    break;
                case Domain.Models.Discord.TokenType.Bot:
                    request.Headers.Add("Authorization", $"Bot {bearerToken}");
                    break;
                default:
                    throw new Exception();
            }
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsObjectAsync<List<GuildRole>>();
            return result.Select(x => x.MapToDomain()).ToList();
        }

        public async Task<List<Domain.Models.Discord.GuildChannel>> GetGuildChannels(Domain.Models.Discord.TokenType tokenType, string bearerToken, ulong guildId)
        {
            var uri = new Uri($"{options.BaseUrl}/guilds/{guildId}/channels");
            var request = new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Get
            };
            switch (tokenType)
            {
                case Domain.Models.Discord.TokenType.Bearer:
                    request.Headers.Add("Authorization", $"Bearer {bearerToken}");
                    break;
                case Domain.Models.Discord.TokenType.Bot:
                    request.Headers.Add("Authorization", $"Bot {bearerToken}");
                    break;
                default:
                    throw new Exception();
            }
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsObjectAsync<List<GuildChannel>>();
            return result.Select(x => x.MapToDomain()).ToList();
        }

        public async Task<Domain.Models.Discord.GuildMember> GetGuildMember(string bearerToken, ulong guildId, ulong userId)
        {
            var uri = new Uri($"{options.BaseUrl}/guilds/{guildId}/members/{userId}");
            var request = new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Get
            };
            request.Headers.Add("Authorization", $"Bearer {bearerToken}");
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsObjectAsync<GuildMember>();
            return result.MapToDomain();

        }
    }
}
