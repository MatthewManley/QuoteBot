using Domain.Repositories;
using System.Threading.Tasks;

namespace Aws
{
    public class ServerRepo : IServerRepo
    {
        public Task<string> GetServerPrefix(ulong serverId)
        {
            return Task.FromResult<string>(null);
        }
    }
}
