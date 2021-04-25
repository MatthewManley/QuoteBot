namespace Domain.Models.Discord
{
    public class GuildRole
    {
        /// <summary>
        /// Role id
        /// </summary>
        public ulong Id { get; init; }

        /// <summary>
        /// Role name
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// integer representation of hexadecimal color code
        /// </summary>
        public int Color { get; init; }

        /// <summary>
        /// if this role is pinned in the user listing
        /// </summary>
        public bool Hoist { get; init; }

        /// <summary>
        /// position of this role
        /// </summary>
        public int Position { get; init; }

        /// <summary>
        /// permission bit set
        /// </summary>
        public string Permissions { get; init; }

        /// <summary>
        /// whether this role is managed by an integration
        /// </summary>
        public bool Managed { get; init; }

        /// <summary>
        /// whether this role is mentionable
        /// </summary>
        public bool Mentionable { get; init; }
    }
}
