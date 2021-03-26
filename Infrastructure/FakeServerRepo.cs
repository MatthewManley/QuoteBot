using Domain.Repositories;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class FakeServerRepo : IServerRepo
    {
        public Task<string> GetServerPrefix(ulong serverId)
        {
            return Task.FromResult(">");
        }
    }
}
