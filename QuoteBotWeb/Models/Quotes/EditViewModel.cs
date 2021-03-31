using Domain.Models;
using System.Collections.Generic;

namespace QuoteBotWeb.Models.Quotes
{
    public class EditViewModel
    {
        public EditViewModel(NamedAudio namedAudio,
                             List<Category> inCategories,
                             List<Category> notInCategories,
                             ulong server,
                             uint quote)
        {
            NamedAudio = namedAudio;
            InCategories = inCategories;
            NotInCategories = notInCategories;
            Server = server;
            Quote = quote;
        }

        public NamedAudio NamedAudio { get; }
        public List<Category> InCategories { get; }
        public List<Category> NotInCategories { get; }
        public ulong Server { get; }
        public uint Quote { get; }
    }
}
