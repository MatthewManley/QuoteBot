using Domain.Models;
using Domain.Repos;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class UserRepo : IUserRepo
    {
        private readonly SqliteConnection connection;
        private const string TableName = "user_role";
        private const string UserId = "user_id";
        private const String Role = "role";

        public UserRepo(SqliteConnection connection)
        {
            this.connection = connection;
        }

        public async Task AddRoleToUser(ulong userId, string role)
        {
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"INSERT INTO {TableName} ({UserId}, {Role}) VALUES ($userId, $role);";
            cmd.Parameters.AddWithValue("$userId", userId.ToString());
            cmd.Parameters.AddWithValue("$role", role);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveAllRolesFromUser(ulong userId)
        {
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"DELETE FROM {TableName} WHERE {UserId} = $userId;";
            cmd.Parameters.AddWithValue("$userId", userId.ToString());
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveAllUsersFromRole(string role)
        {
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"DELETE FROM {TableName} WHERE {Role} = $role;";
            cmd.Parameters.AddWithValue("$role", role);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveRoleFromUser(ulong userId, string role)
        {
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"DELETE FROM {TableName} WHERE {UserId} = $userId AND {Role} = $role;";
            cmd.Parameters.AddWithValue("$userId", userId.ToString());
            cmd.Parameters.AddWithValue("$role", role);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> UserHasAnyRole(ulong userId, params string[] roles)
        {
            if (roles.Length < 1)
            {
                return false;
            }
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            var cmdBuilder = new StringBuilder();
            cmdBuilder.Append("SELECT EXISTS(SELECT role FROM user_role WHERE user_id = $id AND (");
            for (int i = 0; i < roles.Length; i++)
            {
                cmdBuilder.Append($"role = $role{i}");
                cmd.Parameters.AddWithValue($"$role{i}", roles[i]);
                if (i != roles.Length - 1)
                {
                    cmdBuilder.Append(" OR ");
                }
            }
            cmdBuilder.Append("));");
            cmd.CommandText = cmdBuilder.ToString();
            cmd.Parameters.AddWithValue("$id", userId.ToString());
            var result = (long)await cmd.ExecuteScalarAsync();
            return result == 1;
        }
    }
}
