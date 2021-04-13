using Domain.Models;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IServerRepo
    {
        Task<ServerConfig> GetServerConfig(ulong serverId);
        Task<bool> PutServerConfig(ServerConfig serverConfig);
    }
}
