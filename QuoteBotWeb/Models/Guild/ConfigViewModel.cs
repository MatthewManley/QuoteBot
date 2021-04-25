using Domain.Models;
using Domain.Models.Discord;
using System.Collections.Generic;

namespace QuoteBotWeb.Models.Guild
{
    public class ConfigViewModel
    {
        public ConfigViewModel(ServerConfig serverConfig,
                               List<GuildRole> guildRoles,
                               List<GuildChannel> textChannels,
                               List<GuildChannel> voiceChannels,
                               bool isAdmin,
                               bool isModerator)
        {
            ServerConfig = serverConfig;
            GuildRoles = guildRoles;
            TextChannels = textChannels;
            VoiceChannels = voiceChannels;
            IsAdmin = isAdmin;
        }

        public ServerConfig ServerConfig { get; }
        public List<GuildRole> GuildRoles { get; }
        public List<GuildChannel> TextChannels { get; }
        public List<GuildChannel> VoiceChannels { get; }
        public bool IsAdmin { get; }
    }
}
