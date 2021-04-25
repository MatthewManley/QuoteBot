using System;

namespace Domain.Models.Discord
{
    public class GuildMember
    {
        public User User { get; init; }
        public string Nickname { get; init; }
        public ulong[] Roles { get; init; }
        public DateTime JoinedAt { get; init; }
        public DateTime? PremiumSince { get; init; }
        public bool Deaf { get; init; }
        public bool Mute { get; init; }
        public bool? Pending { get; init; }
        public string Permissions { get; init; }
    }
}
