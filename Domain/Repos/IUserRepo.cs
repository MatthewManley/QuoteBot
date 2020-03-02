using Domain.Models;
using System;
using System.Threading.Tasks;

namespace Domain.Repos
{
    public interface IUserRepo
    {
        Task AddRoleToUser(ulong userId, string role);
        Task RemoveRoleFromUser(ulong userId, string role);
        Task RemoveAllRolesFromUser(ulong userId);
        Task RemoveAllUsersFromRole(string role);
        Task<bool> UserHasAnyRole(ulong userId, params string[] roles);
    }
}
