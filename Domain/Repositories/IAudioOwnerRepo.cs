using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IAudioOwnerRepo
    {
        Task<IEnumerable<AudioOwner>> GetAudioOwnersByAudio(uint audioId);
        Task<IEnumerable<AudioOwner>> GetAudioOwnersByOwner(ulong ownerId);
        Task<AudioOwner> CreateAudioOwner(uint audioId, ulong ownerId, string name);
        Task<AudioOwner> GetById(uint id);
        Task Delete(uint id);
        Task Rename(uint id, string newName);
    }
}
