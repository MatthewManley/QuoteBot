using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repos
{
    public interface IQuoteBotRepo
    {
        Task<List<Category>> GetCategoriesWithAudio(ulong ownerId);
        Task<List<NamedAudio>> GetAudioInCategory(ulong ownerId, string category);
        Task<List<NamedAudio>> GetAudioInCategory(uint categoryId);
        Task<NamedAudio> GetAudio(ulong ownerId, string category, string name);

    }
}
