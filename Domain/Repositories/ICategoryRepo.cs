using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface ICategoryRepo
    {
        Task<Category> GetCategory(uint id);
        Task<Category> CreateCategory(string name, ulong owner);
        Task DeleteCategory(uint id);
        Task<IEnumerable<Category>> GetCategoriesByOwner(ulong owner);
    }
}
