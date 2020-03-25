using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Repos
{
    public interface IAudioRepo
    {
        Task<Audio> AddAudio(Audio audio);
        Task<List<Audio>> GetAllAudio();
        Task<List<Audio>> GetAllAudioForCategory(long categoryId);
        Task<List<Audio>> GetAllAudioForCategory(string categoryName);
        Task<Audio> GetAudio(long id);
        Task<Audio> GetAudio(string category, string name);
    }
}