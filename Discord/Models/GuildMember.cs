using System;
using System.Text.Json.Serialization;

namespace Discord.Models
{
    internal class GuildMember
    {
        [JsonPropertyName("user")]
        public User User { get; init; }

        [JsonPropertyName("nick")]
        public string Nickname { get; init; }

        [JsonPropertyName("roles")]
        public ulong[] Roles { get; init; }

        [JsonPropertyName("joined_at")]
        public DateTime JoinedAt { get; init; }

        [JsonPropertyName("premium_since")]
        public DateTime? PremiumSince { get; init; }

        [JsonPropertyName("deaf")]
        public bool Deaf { get; init; }

        [JsonPropertyName("mute")]
        public bool Mute { get; init; }

        [JsonPropertyName("pending")]
        public bool? Pending { get; init; }

        [JsonPropertyName("permissions")]
        public string Permissions { get; init; }

        internal Domain.Models.Discord.GuildMember MapToDomain()
        {
            return new Domain.Models.Discord.GuildMember
            {
                User = User.MapToDomain(),
                Nickname = Nickname,
                Roles = Roles,
                JoinedAt = JoinedAt,
                PremiumSince = PremiumSince,
                Deaf = Deaf,
                Mute = Mute,
                Pending = Pending,
                Permissions = Permissions
            };
        }
    }
}
