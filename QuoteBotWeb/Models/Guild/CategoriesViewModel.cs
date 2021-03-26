using Domain.Models;
using System.Collections.Generic;

namespace QuoteBotWeb.Models.Guild
{
    public class CategoriesViewModel
    {
        public List<(Category, List<AudioOwner>)> Categories { get; init; }
        public List<AudioOwner> AllAudio { get; set; }
    }
}
