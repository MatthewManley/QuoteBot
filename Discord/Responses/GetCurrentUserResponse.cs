using System.Text.Json.Serialization;

namespace Discord.Responses
{
    public class GetCurrentUserResponse
    {
        /// <summary>
        /// the user's id
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// the user's username, not unique across the platform
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; }

        /// <summary>
        /// the user's 4-digit discord-tag
        /// </summary>
        [JsonPropertyName("discriminator")]
        public string Discriminator { get; set; }

        /// <summary>
        /// the user's avatar hash
        /// </summary>
        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        /// <summary>
        /// whether the user belongs to an OAuth2 application
        /// </summary>
        [JsonPropertyName("bot")]
        public bool? Bot { get; set; }

        /// <summary>
        /// whether the user is an Official Discord System user (part of the urgent message system)
        /// </summary>
        [JsonPropertyName("system")]
        public bool? System { get; set; }

        /// <summary>
        /// whether the user has two factor enabled on their account
        /// </summary>
        [JsonPropertyName("mfa_enabled")]
        public bool? MfaEnabled { get; set; }

        /// <summary>
        /// the user's chosen language option
        /// </summary>
        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        /// <summary>
        /// whether the email on this account has been verified
        /// </summary>
        [JsonPropertyName("verified")]
        public bool? Verified { get; set; }

        /// <summary>
        /// the user's email
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; }

        /// <summary>
        /// the flags on a user's account
        /// <see cref="https://discord.com/developers/docs/resources/user#user-object-user-flags"/>
        /// </summary>
        [JsonPropertyName("flags")]
        public int? Flags { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("premium_type")]
        public int? PremiumType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("public_flags")]
        public int? PublicFlags { get; set; }
    }
}
