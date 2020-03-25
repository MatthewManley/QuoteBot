using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Repos
{
    public interface IAudioCategoryRepo
    {
        Task<AudioCategory> GetAudioCategory(long category, long audioOwner);
        Task<List<AudioCategory>> GetAudioCategoriesByCategory(long category);
        Task<List<AudioCategory>> GetAudioCategoriesByAudioOwnerId(long audioOwner);
        Task AddAudioCategory(AudioCategory audioCategory);
    }
}