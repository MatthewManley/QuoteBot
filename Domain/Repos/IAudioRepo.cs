using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Repos
{
    public interface IAudioRepo
    {
        Task AddAudio(Audio audio);
        Task<List<Audio>> GetAllAudio();
        Task<List<Audio>> GetAllAudioForCategory(string category);
        Task<Audio> GetAudio(int id);
        Task<Audio> GetAudio(string category, string name);
    }
}