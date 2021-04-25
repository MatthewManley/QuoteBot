using System.Collections.Generic;

namespace QuoteBotWeb.Models.Guild
{
    public class ConfigPostBody
    {
        public string ModeratorRole { get; init; }
        public string Prefix { get; init; }
        public string TextListType { get; init; }
        public List<ulong> TextChannels { get; init; }
        public string VoiceListType { get; init; }
        public List<ulong> VoiceChannels { get; init; }

    }
}
