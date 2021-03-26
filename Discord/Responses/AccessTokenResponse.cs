using System.Text.Json.Serialization;

namespace Discord.Responses
{
    public class AccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; init; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; init; }

        [JsonPropertyName("scope")]
        public string Scope
        {
            get
            {
                return _scope;
            }
            init
            {
                Scopes = value.Split(' ');
                _scope = value;
            }
        }

        private string _scope;

        public string[] Scopes { get; private set; }
    }
}
