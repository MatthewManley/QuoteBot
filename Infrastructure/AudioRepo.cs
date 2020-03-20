using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Repos;
using Microsoft.Data.Sqlite;
using System.Linq;
using System;

namespace Infrastructure
{
        public class AudioRepo : IAudioRepo
    {
        private readonly DbConnection connection;

        public AudioRepo(DbConnection connection)
        {
            this.connection = connection;
        }

        public async Task<List<Audio>> GetAllAudioForCategory(string category)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT audio.id, audio.name, audio.path FROM audio " +
                "INNER JOIN audio_category on audio.id = audio_category.audio " +
                "INNER JOIN category on audio_category.category = category.id " +
                "WHERE category.name = $category;";

            // Parameterizing input to prevent sql injection
            cmd.AddParameterWithValue("$category", category.ToLowerInvariant());

            using var reader = await cmd.ExecuteReaderAsync();
            var enumerable = reader.ReadToEnumerable(() => new Audio
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Path = reader.GetString(2)
            });
            var result = await enumerable.ToListAsync();
            await connection.CloseAsync();
            return result;
        }

        public async Task<Audio> GetAudio(int id)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT id, name, path FROM audio " +
                "WHERE id = $id " +
                "LIMIT 1;";

            // Parameterizing input to prevent sql injection
            cmd.AddParameterWithValue("$id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            var result = await reader.ReadFirstOrDefault(() => new Audio
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Path = reader.GetString(2)
            });
            await connection.CloseAsync();
            return result;
        }

        public async Task<Audio> GetAudio(string category, string name)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT audio.id, audio.name, audio.path FROM audio " +
                "INNER JOIN audio_category on audio.id = audio_category.audio " +
                "INNER JOIN category on audio_category.category = category.id " +
                "WHERE category.name = $category AND audio.name = $name " +
                "LIMIT 1;";

            // Parameterizing user input to prevent sql injection
            cmd.AddParameterWithValue("$category", category.ToLowerInvariant());
            cmd.AddParameterWithValue("$name", name.ToLowerInvariant());

            using var reader = await cmd.ExecuteReaderAsync();

            var result = await reader.ReadFirstOrDefault(() => new Audio
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Path = reader.GetString(2)
            });

            await connection.CloseAsync();
            return result;
        }

        public async Task<List<Audio>> GetAllAudio()
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT audio.name, audio.id, audio.path FROM audio;";
            using var reader = await cmd.ExecuteReaderAsync();
            var enumerable = reader.ReadToEnumerable(() => new Audio
            {
                Name = reader.GetString(0),
                Id = reader.GetInt32(1),
                Path = reader.GetString(2)
            });
            var result = await enumerable.ToListAsync();
            await connection.CloseAsync();
            return result;
        }

        public async Task AddAudio(Audio audio)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO audio (name, path) VALUES ($name, $path);";
            cmd.AddParameterWithValue("$name", audio.Name);
            cmd.AddParameterWithValue("$path", audio.Path);
            await cmd.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }
    }
}