using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Discord.Responses
{
    public class GuildRole
    {

        /// <summary>
        /// Role id
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; init; }

        /// <summary>
        /// Role name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; }

        /// <summary>
        /// integer representation of hexadecimal color code
        /// </summary>
        [JsonPropertyName("color")]
        public int Color { get; init; }

        /// <summary>
        /// if this role is pinned in the user listing
        /// </summary>
        [JsonPropertyName("hoist")]
        public bool Hoist { get; init; }

        /// <summary>
        /// position of this role
        /// </summary>
        [JsonPropertyName("position")]
        public int Position { get; init; }

        /// <summary>
        /// permission bit set
        /// </summary>
        [JsonPropertyName("permissions")]
        public string Permissions { get; init; }

        /// <summary>
        /// whether this role is managed by an integration
        /// </summary>
        [JsonPropertyName("managed")]
        public bool Managed { get; init; }

        /// <summary>
        /// whether this role is mentionable
        /// </summary>
        [JsonPropertyName("mentionable")]
        public bool Mentionable { get; init; }
    }
}