using System.Threading.Tasks;

namespace Domain.Repos
{
    public interface IServerRepo
    {
        Task<string> GetServerPrefix(ulong serverId);
    }
}
