namespace Domain.Models.Discord
{
    public enum GuildChannelType : int
    {
        Text = 0,
        DirectMessage = 1,
        Voice = 2,
        GroupDirectMessage = 3,
        Category = 4,
        News = 5,
        Store = 6,
        StageVoice = 13
    }
}
