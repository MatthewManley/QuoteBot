namespace QuoteBot.WebApi.Options
{
    public class DiscordOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public string DiscordApiEndpoint { get; set; }
    }
}