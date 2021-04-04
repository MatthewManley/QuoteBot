using Domain.Models;
using System.Collections.Generic;

namespace QuoteBotWeb.Models.Quotes
{
    public class UploadViewModel
    {
        public UploadViewModel(List<Category> categories)
        {
            Categories = categories;
        }

        public List<Category> Categories { get; }
    }
}
