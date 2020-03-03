using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Repos
{
    public interface IAudioRepo
    {
        Task AddAudio(Audio audio);
        Task<List<Audio>> GetAllAudio();
        Task<List<Audio>> GetAudioForCategory(string category);
        Task<List<string>> GetCategories();
        Task<Audio> GetAudio(string category, string name);
        Task RemoveAudio(string name, string category);
    }
}