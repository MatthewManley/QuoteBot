using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Repos
{
    public interface ICategoryRepo
    {
        Task<int> CreateCategory(string name);
        Task<List<Category>> GetAllCategories();
        Task<List<Category>> GetAllCategoriesWithAudio();
        Task<List<Category>> GetAllCategoriesWithNoAudio();
        Task<Category> GetCategoryById(int id);
        Task<Category> GetCategoryByName(string name);
    }
}