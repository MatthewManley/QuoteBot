namespace Domain.Models
{
    public class AuthEntry
    {
        public string Key { get; init; }
        public string AccessToken { get; init; }
        public long Expires { get; init; }
        public string RefreshToken { get; init; }
        public string Scope { get; init; }
        public ulong UserId { get; init; }
        public string Avatar { get; init; }
    }
}
