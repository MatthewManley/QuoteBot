using Domain.Models;
using Domain.Models.Discord;
using System.Collections.Generic;

namespace QuoteBotWeb.Models.Guild
{
    public class IndexViewModel
    {
        public List<UserGuild> Guilds { get; init; }
    }
}
