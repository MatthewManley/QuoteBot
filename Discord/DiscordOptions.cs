namespace Discord
{
    public class DiscordOptions
    {
        public string ClientId { get; init; }
        public string ClientSecret { get; init; }
        public string RedirectUri { get; init; }
        public string BaseUrl { get; set; }
    }
}
