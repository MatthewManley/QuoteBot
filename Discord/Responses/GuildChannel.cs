using System.Text.Json.Serialization;

namespace Discord.Responses
{
    public class GuildChannel
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("type")]
        public int ChannelType { get; init; }

        [JsonPropertyName("guild_id")]
        public string GuildId { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("position")]
        public int? Position { get; init; }
    }
}
