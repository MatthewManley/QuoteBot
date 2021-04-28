using Dapper;
using Domain.Models;
using Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aws
{
    public class UserJoinedRepo : IUserJoinedRepo
    {
        private readonly DbConnectionFactory dbConnectionFactory;

        public UserJoinedRepo(DbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IEnumerable<Audio>> GetJoinedAudioForUser(ulong userId)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT audio.id Id, audio.path Path, audio.uploader Uploader FROM user_joined " +
                "INNER JOIN audio ON user_joined.audio = audio.id " + 
                "WHERE user_joined.user = @userId;";
            var parameters = new { userId
            };
            return await dbConnection.QueryAsync<Audio>(cmdText, parameters);
        }
    }
}
