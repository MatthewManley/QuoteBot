using Domain.Models;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IAuthRepo
    {
        Task<AuthEntry> CreateAuthEntry(AuthEntry entry);
        Task<AuthEntry> GetAuthEntry(string key);
        Task DeleteAuthEntry(string key);
    }
}
