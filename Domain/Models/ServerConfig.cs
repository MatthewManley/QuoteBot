using System.Collections.Generic;

namespace Domain.Models
{
    public class ServerConfig
    {
        public ulong ServerId { get; init; }
        public string Prefix { get; init; }
        public string TextChannelListType { get; init; }
        public string VoiceChannelListType { get; init; }
        public List<ulong> TextChannelList { get; init; }
        public List<ulong> VoiceChannelList { get; init; }
    }
}
