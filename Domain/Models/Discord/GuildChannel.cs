namespace Domain.Models.Discord
{
    public class GuildChannel
    {
        public ulong Id { get; init; }
        public GuildChannelType ChannelType { get; init; }
        public ulong? GuildId { get; init; }
        public string Name { get; set; }
        public int? Position { get; init; }
    }
}
