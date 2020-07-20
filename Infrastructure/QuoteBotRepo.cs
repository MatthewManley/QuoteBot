using Domain.Models;
using Domain.Repos;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class QuoteBotRepo : IQuoteBotRepo
    {
        private readonly DbConnectionFactory dbConnectionFactory;

        public QuoteBotRepo(DbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<NamedAudio> GetAudio(ulong ownerId, string category, string name)
        {
            using var dbConnection = (MySqlConnection)await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT audio.id AS aId, audio.path, audio.uploader, audio_owner.id AS oId, audio_owner.audio, audio_owner.name, audio_owner.owner FROM audio_owner " +
                "INNER JOIN audio on audio_owner.audio = audio.id " +
                "INNER JOIN audio_category on audio_owner.id = audio_category.audio_owner " +
                "INNER JOIN category on audio_category.category = category.id " +
                "WHERE audio_owner.owner = @ownerId AND category.name = @category AND audio_owner.name = @name " +
                "LIMIT 1;";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = cmdText;
            cmd.AddParameterWithValue(@"ownerId", ownerId);
            cmd.AddParameterWithValue("@category", category);
            cmd.AddParameterWithValue("@name", name);
            var reader = cmd.ExecuteReader();
            return await reader.ReadToEnumerable(ToNamedAudio).FirstOrDefaultAsync();
        }

        public async Task<List<NamedAudio>> GetAudioInCategory(ulong ownerId, string category)
        {
            using var dbConnection = (MySqlConnection)await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT audio.id AS aId, audio.path, audio.uploader, audio_owner.id AS oId, audio_owner.audio, audio_owner.name, audio_owner.owner FROM audio_owner " +
                "INNER JOIN audio_category ON audio_category.audio_owner = audio_owner.id " +
                "INNER JOIN category ON audio_category.category = category.id " +
                "INNER JOIN audio ON audio_owner.audio = audio.id " +
                "WHERE audio_owner.owner = @owner AND category.name = @category;";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = cmdText;
            cmd.AddParameterWithValue("@owner", ownerId);
            cmd.AddParameterWithValue("@category", category);
            var reader = cmd.ExecuteReader();
            return await reader.ReadToEnumerable(ToNamedAudio).ToListAsync();
        }

        public async Task<List<NamedAudio>> GetAudioInCategory(uint categoryId)
        {
            using var dbConnection = (MySqlConnection)await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT audio.id AS aId, audio.path, audio.uploader, audio_owner.id AS oId, audio_owner.audio, audio_owner.name, audio_owner.owner FROM audio_owner " +
                "INNER JOIN audio_category ON audio_category.audio_owner = audio_owner.id " +
                "INNER JOIN category ON audio_category.category = category.id " +
                "INNER JOIN audio ON audio_owner.audio = audio.id " +
                "WHERE category.id = @category;";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = cmdText;
            cmd.AddParameterWithValue("@category", categoryId);
            var reader = cmd.ExecuteReader();
            return await reader.ReadToEnumerable(ToNamedAudio).ToListAsync();
        }

        public async Task<List<Category>> GetCategoriesWithAudio(ulong ownerId)
        {
            using var dbConnection = (MySqlConnection)await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT DISTINCT category.id, category.name, category.owner FROM category " +
                "INNER JOIN audio_category ON audio_category.category = category.id " +
                "WHERE category.owner = @owner;";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = cmdText;
            cmd.AddParameterWithValue("@owner", ownerId);
            var reader = cmd.ExecuteReader();
            var asyncEnumerable = reader.ReadToEnumerable(r => new Category
            {
                Id = (uint)r.GetInt32("id"),
                Name = r.GetString("name"),
                OwnerId = (ulong)r.GetInt64("owner")
            });
            return await asyncEnumerable.ToListAsync();
        }

        public NamedAudio ToNamedAudio(MySqlDataReader reader)
        {
            return new NamedAudio
            {
                Audio = new Audio
                {
                    Id = reader.GetUInt32("aid"),
                    Path = reader.GetString("path"),
                    Uploader = reader.GetUInt64("uploader")
                },
                AudioOwner = new AudioOwner
                {
                    Id = reader.GetUInt32("oId"),
                    AudioId = reader.GetUInt32("audio"),
                    Name = reader.GetString("name"),
                    OwnerId = reader.GetUInt64("owner")
                }
            };
        }
    }
}
