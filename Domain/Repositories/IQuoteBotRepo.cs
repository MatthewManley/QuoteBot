using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IQuoteBotRepo
    {
        Task<IEnumerable<Category>> GetCategoriesWithAudio(ulong ownerId);
        Task<IEnumerable<NamedAudio>> GetAudioInCategory(ulong ownerId, string category);
        Task<IEnumerable<NamedAudio>> GetAudioInCategory(uint categoryId);
        Task<NamedAudio> GetAudio(ulong ownerId, string category, string name);
        Task<IEnumerable<AudioOwnerCategory>> GetAudioCategoriesByOwner(ulong ownerId);
        Task<IEnumerable<Category>> GetCategoriesByOwner(ulong ownerId);
        Task<AudioOwner> CreateAudioOwner(uint audioId, ulong owner, string name);
        Task<NamedAudio> GetNamedAudioByAudioOwnerId(uint audioOwnerId);

    }
}
