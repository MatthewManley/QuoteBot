using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Repos
{
    public interface ICategoryRepo
    {
        Task<long> CreateCategory(string name);
        Task<List<Category>> GetAllCategories();
        Task<List<Category>> GetAllCategoriesWithAudio();
        Task<List<Category>> GetAllCategoriesWithNoAudio();
        Task<Category> GetCategoryById(long id);
        Task<Category> GetCategoryByName(string name);
    }
}