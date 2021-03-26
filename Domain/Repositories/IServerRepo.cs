using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IServerRepo
    {
        Task<string> GetServerPrefix(ulong serverId);
    }
}
