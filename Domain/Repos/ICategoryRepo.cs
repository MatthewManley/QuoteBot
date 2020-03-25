using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Repos
{
    public interface ICategoryRepo
    {
        Task<List<Category>> GetCategories(ulong ownerId);
        Task<List<Category>> GetCategoriesWithAudio(ulong ownerId);
        Task<Category> CreateCategory(Category category);
    }
}