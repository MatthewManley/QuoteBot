using Domain.Models;

namespace QuoteBotWeb.Models.Categories
{
    public class DeleteViewModel
    {
        public DeleteViewModel(Category category, ulong Server)
        {
            Category = category;
            this.Server = Server;
        }

        public Category Category { get; }
        public ulong Server { get; }
    }
}
