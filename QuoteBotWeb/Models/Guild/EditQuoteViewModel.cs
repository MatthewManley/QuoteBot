using Domain.Models;
using System.Collections.Generic;

namespace QuoteBotWeb.Models.Guild
{
    public class EditQuoteViewModel
    {
        public NamedAudio NamedAudio { get; init; }
        public List<Category> InCategories { get; init; }
        public List<Category> NotInCategories { get; init; }
        public string Server { get; init; }
        public string Quote { get; init; }
    }
}
