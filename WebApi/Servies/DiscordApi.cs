using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using quote_bot_web_api.Domain;
using quote_bot_web_api.Options;

namespace QuoteBot.WebApi.Services
{
    public class DiscordApi : IDiscordApi
    {
        private readonly HttpClient httpClient;
        private readonly DiscordOptions discordOptions;

        public DiscordApi(HttpClient httpClient, IOptions<DiscordOptions> discordOptions)
        {
            this.httpClient = httpClient;
            this.discordOptions = discordOptions.Value;
        }

        public async Task<AccessTokenResponse> AccessToken(string code)
        {
            var data = new List<KeyValuePair<string, string>>();
            data.Add(new KeyValuePair<string, string>("client_id", discordOptions.ClientId));
            data.Add(new KeyValuePair<string, string>("client_secret", discordOptions.ClientSecret));
            data.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
            data.Add(new KeyValuePair<string, string>("code", code));
            data.Add(new KeyValuePair<string, string>("redirect_uri", discordOptions.RedirectUri));
            var req = new HttpRequestMessage(HttpMethod.Post, discordOptions.DiscordApiEndpoint + "/oauth2/token");
            req.Content = new FormUrlEncodedContent(data);
            var response = await httpClient.SendAsync(req);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception();
            }
            var content = await response.Content.ReadAsStringAsync();
            var atr = JsonConvert.DeserializeObject<AccessTokenResponse>(content);
            var scopes = atr.scope.Split(' ');
            if (!scopes.Contains("identify") || !scopes.Contains("guilds"))
            {
                throw new Exception();
            }
            return atr;
        }
    }
}