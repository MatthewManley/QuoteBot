using Aws.Models;
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
    public class AudioCategoryRepo : IAudioCategoryRepo
    {
        private readonly DbConnectionFactory dbConnectionFactory;

        public AudioCategoryRepo(DbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory;
        }

        public async Task Create(uint audioOwnerId, uint categoryId)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "INSERT INTO audio_category (category, audio_owner) VALUES (@category, @audio);";
            var parameters = new
            {
                category = categoryId,
                audio = audioOwnerId
            };
            await dbConnection.ExecuteAsync(cmdText, parameters);
        }

        public async Task Delete(uint audioOwnerId, uint categoryId)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "DELETE FROM audio_category WHERE category = @category AND audio_owner = @audio;";
            var parameters = new
            {
                category = categoryId,
                audio = audioOwnerId
            };
            await dbConnection.ExecuteAsync(cmdText, parameters);
        }

        public async Task<IEnumerable<AudioOwner>> GetAudioOwnersByCategoryId(uint categoryId)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT audio_owner.id Id, audio_owner.audio AudioId, audio_owner.owner OwnerId, audio_owner.name Name FROM audio_category " +
                "INNER JOIN audio_owner ON audio_category.audio_owner = audio_owner.id " +
                "WHERE audio_category.category = @categoryId;";
            var parameters = new
            {
                categoryId
            };
            return await dbConnection.QueryAsync<AudioOwner>(cmdText, parameters);
        }

        public async Task<IEnumerable<Category>> GetCategoriesForAudioOwner(uint audioOwnerId)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT category.id Id, category.owner OwnerId, category.name Name FROM audio_category " +
                "INNER JOIN category ON audio_category.category = category.id " +
                "WHERE audio_category.audio_owner = @audioOwnerId;";
            var parameters = new
            {
                audioOwnerId
            };
            return await dbConnection.QueryAsync<Category>(cmdText, parameters);
        }

        public async Task<IEnumerable<NamedAudio>> GetNamedAudiosInCategory(uint categoryId)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT audio.id AS AudioId, audio.path AS AudioPath, audio.uploader AS AudioUploader, audio_owner.id AS AudioOwnerId, audio_owner.audio AS AudioOwnerAudioId, audio_owner.name AS AudioOwnerName, audio_owner.owner AS AudioOwnerOwnerId FROM audio_category " +
                "INNER JOIN audio_owner ON audio_category.audio_owner = audio_owner.id " +
                "INNER JOIN audio ON audio_owner.audio = audio.id " +
                "WHERE audio_category.category = @categoryId;";
            var parameters = new
            {
                categoryId
            };
            var intermediate = await dbConnection.QueryAsync<GetNamedAudioResponse>(cmdText, parameters);
            return intermediate.Select(x => x.ToNamedAudio());
        }
    }
}
