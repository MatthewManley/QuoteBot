using Domain.Models;
using System.Collections.Generic;

namespace QuoteBotWeb.Models.Categories
{
    public class IndexViewModel
    {
        public List<(Category, List<AudioOwner>)> Categories { get; init; }
        public List<AudioOwner> AllAudio { get; set; }
        public ulong Server { get; set; }
    }
}
