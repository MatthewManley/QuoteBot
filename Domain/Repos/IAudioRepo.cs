using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Repos
{
    public interface IAudioRepo
    {
        Task<List<NamedAudio>> GetAudioForCategory(ulong ownerId, string categoryName);
        Task<List<NamedAudio>> GetAudioForCategory(long categoryId);
        Task<NamedAudio> GetAudio(ulong ownerId, string categoryName, string audioName);
    }
}