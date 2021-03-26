using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IAudioCategoryRepo
    {
        Task Create(uint audioOwnerId, uint categoryId);
        Task Delete(uint audioOwnerId, uint categoryId);

        Task<IEnumerable<Category>> GetCategoriesForAudioOwner(uint audioOwnerId);
        Task<IEnumerable<AudioOwner>> GetAudioOwnersByCategoryId(uint categoryId);
        Task<IEnumerable<NamedAudio>> GetNamedAudiosInCategory(uint categoryId);

        //TODO: Get categories by audio_owner
        //TODO: Get audio_owners by category
    }
}
