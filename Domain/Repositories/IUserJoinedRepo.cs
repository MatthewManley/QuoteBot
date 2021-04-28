using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IUserJoinedRepo
    {
        Task<IEnumerable<Audio>> GetJoinedAudioForUser(ulong userId);
    }
}
