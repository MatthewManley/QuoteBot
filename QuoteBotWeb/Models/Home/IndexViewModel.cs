namespace QuoteBotWeb.Models.Home
{
    public class IndexViewModel
    {
        public bool LoggedIn { get; init; }
        public string Avatar { get; init; }
        public ulong? UserId { get; init; }
        public ulong AppClientId { get; init; }
    }
}
