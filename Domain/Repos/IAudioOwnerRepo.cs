using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Repos
{
    public interface IAudioOwnerRepo
    {
        Task<AudioOwner> AddAudioOwner(AudioOwner audioOwner);
        Task<List<AudioOwner>> GetAudioOwners(ulong ownerId);
    }
}