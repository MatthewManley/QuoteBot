using Domain.Models;
using System.Collections.Generic;

namespace QuoteBotWeb.Models.Guild
{
    public class QuotesViewModel
    {
        public List<(AudioOwner, List<Category>)> Quotes { get; init; }
        public ulong Server { get; init; }
    }
}
