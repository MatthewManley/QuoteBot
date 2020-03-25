using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Repos;
using System.Linq;

namespace Infrastructure
{
    public class AudioCategoryRepo : IAudioCategoryRepo
    {
        private readonly DbConnection connection;

        public AudioCategoryRepo(DbConnection connection)
        {
            this.connection = connection;
        }

        public async Task AddAudioCategory(AudioCategory audioCategory)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "INSERT INTO audio_category (category, audio_owner) VALUES ($category, $audio);";
            cmd.AddParameterWithValue("$category", audioCategory.CategoryId);
            cmd.AddParameterWithValue("$audio", audioCategory.AudioOwnerId);
            await cmd.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

        public async Task<List<AudioCategory>> GetAudioCategoriesByAudioOwnerId(long audioOwnerId)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = 
                "SELECT category, audio_owner FROM audio_category " +
                "WHERE audio_owner = $audio_owner;";
            cmd.AddParameterWithValue("$audio_owner", audioOwnerId);
            using var reader = await cmd.ExecuteReaderAsync();
            var enumerable = reader.ReadToEnumerable(() => new AudioCategory {
                AudioOwnerId = reader.GetInt64(0),
                CategoryId = reader.GetInt64(1)
            });
            var result = await enumerable.ToListAsync();
            await connection.CloseAsync();
            return result;
        }

        public async Task<List<AudioCategory>> GetAudioCategoriesByCategory(long category)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = 
                "SELECT category, audio_owner FROM audio_category " +
                "WHERE category = $category;";
            cmd.AddParameterWithValue("$category", category);
            using var reader = await cmd.ExecuteReaderAsync();
            var enumerable = reader.ReadToEnumerable(() => new AudioCategory {
                AudioOwnerId = reader.GetInt64(0),
                CategoryId = reader.GetInt64(1)
            });
            var result = await enumerable.ToListAsync();
            await connection.CloseAsync();
            return result;
        }

        public async Task<AudioCategory> GetAudioCategory(long category, long audioOwner)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = 
                "SELECT category, audio_owner FROM audio_category " +
                "WHERE category = $category AND audio_owner = $audio_owner;";
            cmd.AddParameterWithValue("$category", category);
            cmd.AddParameterWithValue("$audio_owner", audioOwner);
            var reader = await cmd.ExecuteReaderAsync();
            var enumerable = reader.ReadToEnumerable(() => new AudioCategory {
                AudioOwnerId = reader.GetInt64(0),
                CategoryId = reader.GetInt64(1)
            });
            var result = await enumerable.FirstOrDefaultAsync();
            await connection.CloseAsync();
            return result;
        }
    }
}