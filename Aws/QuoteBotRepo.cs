using Aws.Models;
using Dapper;
using Domain.Models;
using Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aws
{
    public class QuoteBotRepo : IQuoteBotRepo
    {
        private readonly DbConnectionFactory dbConnectionFactory;

        public QuoteBotRepo(DbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IEnumerable<Category>> GetCategoriesByOwner(ulong ownerId)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText =
                "Select category.id Id, category.name Name, category.owner OwnerId FROM category " +
                "WHERE category.owner = @ownerId;";
            var parameters = new
            {
                ownerId = ownerId
            };
            return await dbConnection.QueryAsync<Category>(cmdText, parameters);
        }

        public async Task<IEnumerable<AudioOwnerCategory>> GetAudioCategoriesByOwner(ulong ownerId)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText =
                "Select audio_category.category CategoryId, audio_category.audio_owner AudioOwnerId FROM audio_category " +
                "INNER JOIN audio_owner ON audio_category.audio_owner = audio_owner.id " +
                "INNER JOIN category ON audio_category.category = category.id " +
                "WHERE audio_owner.owner = @ownerId OR category.owner = @ownerId ;";
            var parameters = new
            {
                ownerId = ownerId
            };
            return await dbConnection.QueryAsync<AudioOwnerCategory>(cmdText, parameters);
        }

        public async Task<AudioOwner> CreateAudioOwner(uint audioId, ulong owner, string name)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "INSERT INTO audio_owner (audio_owner.audio, audio_owner.owner, audio_owner.name) VALUES (@audio, @owner, @name); SELECT LAST_INSERT_ID();";
            var parameters = new
            {
                audio = audioId,
                owner = owner,
                name = name
            };
            var id = await dbConnection.ExecuteScalarAsync<uint>(cmdText, parameters);
            return new AudioOwner
            {
                AudioId = audioId,
                Id = id,
                Name = name,
                OwnerId = owner
            };
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithAudio(ulong ownerId)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT DISTINCT category.id Id, category.name Name, category.owner OwnerId FROM category " +
                "INNER JOIN audio_category ON audio_category.category = category.id " +
                "WHERE category.owner = @owner;";
            var parameters = new
            {
                owner = ownerId
            };
            return await dbConnection.QueryAsync<Category>(cmdText, parameters);
        }

        public async Task<NamedAudio> GetNamedAudioByAudioOwnerId(uint audioOwnerId)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT audio.id AS AudioId, audio.path AS AudioPath, audio.uploader AS AudioUploader, audio_owner.id AS AudioOwnerId, audio_owner.audio AS AudioOwnerAudioId, audio_owner.name AS AudioOwnerName, audio_owner.owner AS AudioOwnerOwnerId FROM audio_owner " +
                "INNER JOIN audio ON audio_owner.audio = audio.id " +
                "WHERE audio_owner.id = @id " +
                "LIMIT 1;";
            var parameters = new
            {
                id = audioOwnerId
            };
            var namedAudioResponse = await dbConnection.QuerySingleAsync<GetNamedAudioResponse>(cmdText, parameters);
            return namedAudioResponse.ToNamedAudio();
        }

        public async Task<IEnumerable<NamedAudio>> GetAudioInCategory(ulong ownerId, string category)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT audio.id AS AudioId, audio.path AS AudioPath, audio.uploader AS AudioUploader, audio_owner.id AS AudioOwnerId, audio_owner.audio AS AudioOwnerAudioId, audio_owner.name AS AudioOwnerName, audio_owner.owner AS AudioOwnerOwnerId FROM audio_owner " +
                "INNER JOIN audio_category ON audio_category.audio_owner = audio_owner.id " +
                "INNER JOIN category ON audio_category.category = category.id " +
                "INNER JOIN audio ON audio_owner.audio = audio.id " +
                "WHERE audio_owner.owner = @owner AND category.name = @category;";
            var parameters = new
            {
                owner = ownerId,
                category = category
            };
            var result = await dbConnection.QueryAsync<GetNamedAudioResponse>(cmdText, parameters);
            return result.Select(x => x.ToNamedAudio());
        }

        public async Task<IEnumerable<NamedAudio>> GetAudioInCategory(uint categoryId)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT audio.id AS AudioId, audio.path AS AudioPath, audio.uploader AS AudioUploader, audio_owner.id AS AudioOwnerId, audio_owner.audio AS AudioOwnerAudioId, audio_owner.name AS AudioOwnerName, audio_owner.owner AS AudioOwnerOwnerId FROM audio_owner " +
                 "INNER JOIN audio_category ON audio_category.audio_owner = audio_owner.id " +
                 "INNER JOIN category ON audio_category.category = category.id " +
                 "INNER JOIN audio ON audio_owner.audio = audio.id " +
                 "WHERE category.id = @category;";
            var parameters = new
            {
                category = categoryId
            };
            var result = await dbConnection.QueryAsync<GetNamedAudioResponse>(cmdText, parameters);
            return result.Select(x => x.ToNamedAudio());
        }

        public async Task<NamedAudio> GetAudio(ulong ownerId, string category, string name)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT audio.id AS AudioId, audio.path AS AudioPath, audio.uploader AS AudioUploader, audio_owner.id AS AudioOwnerId, audio_owner.audio AS AudioOwnerAudioId, audio_owner.name AS AudioOwnerName, audio_owner.owner AS AudioOwnerOwnerId FROM audio_owner " +
                   "INNER JOIN audio on audio_owner.audio = audio.id " +
                   "INNER JOIN audio_category on audio_owner.id = audio_category.audio_owner " +
                   "INNER JOIN category on audio_category.category = category.id " +
                   "WHERE audio_owner.owner = @ownerId AND category.name = @category AND audio_owner.name = @name " +
                   "LIMIT 1;";
            var parameters = new
            {
                category = category,
                ownerId = ownerId,
                name = name
            };
            var result = await dbConnection.QueryFirstAsync<GetNamedAudioResponse>(cmdText, parameters);
            return result.ToNamedAudio();
        }
    }
}
