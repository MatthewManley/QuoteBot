using Domain.Models;
using System.Collections.Generic;

namespace QuoteBotWeb.Models
{
    public class CategoryQuotes
    {
        public Category Category;
        public List<AudioOwner> Quotes;
    }
}
