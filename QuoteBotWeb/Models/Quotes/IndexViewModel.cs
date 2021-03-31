using Domain.Models;
using System.Collections.Generic;

namespace QuoteBotWeb.Models.Quotes
{
    public class IndexViewModel
    {
        public IndexViewModel(List<(AudioOwner, List<Category>)> quotes, ulong server)
        {
            Quotes = quotes;
            Server = server;
        }

        public List<(AudioOwner, List<Category>)> Quotes { get; }
        public ulong Server { get; }
    }
}
