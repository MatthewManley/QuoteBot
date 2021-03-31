using Dapper;
using Domain.Models;
using Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aws
{
    public class AudioOwnerRepo : IAudioOwnerRepo
    {
        private readonly DbConnectionFactory dbConnectionFactory;

        public AudioOwnerRepo(DbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<AudioOwner> GetById(uint id)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT id Id, audio AudioId, owner OwnerId, name Name FROM audio_owner WHERE id = @id;";
            var parameters = new { id = id };
            return await dbConnection.QueryFirstAsync<AudioOwner>(cmdText, parameters);
        }

        public async Task<IEnumerable<AudioOwner>> GetAudioOwnersByOwner(ulong ownerId)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText =
                "Select audio_owner.id Id, audio_owner.audio AudioId, audio_owner.owner OwnerId, audio_owner.name Name FROM audio_owner " +
                "WHERE audio_owner.owner = @ownerId;";
            var paramaters = new { ownerId = ownerId };
            return await dbConnection.QueryAsync<AudioOwner>(cmdText, paramaters);
        }

        public async Task<IEnumerable<AudioOwner>> GetAudioOwnersByAudio(uint audioId)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText =
                "Select audio_owner.id Id, audio_owner.audio AudioId, audio_owner.owner OwnerId, audio_owner.name Name FROM audio_owner " +
                "WHERE audio_owner.audio = @audioId;";
            var paramaters = new
            {
                audioId = audioId
            };
            return await dbConnection.QueryAsync<AudioOwner>(cmdText, paramaters);
        }

        public async Task<AudioOwner> CreateAudioOwner(uint audioId, ulong ownerId, string name)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "INSERT INTO audio_owner (audio, owner, name) VALUES (@audio, @owner, @name);";
            var parameters = new
            {
                audio = audioId,
                owner = ownerId,
                name = name
            };
            await dbConnection.ExecuteAsync(cmdText, parameters);
            var id = await dbConnection.ExecuteScalarAsync<uint>("SELECT LAST_INSERT_ID();");
            var result = new AudioOwner
            {
                AudioId = audioId,
                Id = id,
                Name = name,
                OwnerId = ownerId
            };
            return result;
        }

        public async Task Delete(uint id)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "DELETE FROM audio_owner WHERE id = @id";
            var parameters = new
            {
                id = id
            };
            await dbConnection.ExecuteAsync(cmdText, parameters);
        }
    }
}
