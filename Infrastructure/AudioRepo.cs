using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Repos;
using Microsoft.Data.Sqlite;

namespace Infrastructure
{
    public class AudioRepo : IAudioRepo
    {
        private readonly SqliteConnection connection;

        public AudioRepo(SqliteConnection connection)
        {
            this.connection = connection;
        }

        public async Task<List<string>> GetCategories()
        {
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT(category) FROM audio;";
            var result = new List<string>();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Add(reader.GetString(0));
                }
            }
            connection.Close();
            return result;
        }

        public async Task<List<Audio>> GetAudioForCategory(string category)
        {
            var lowerCategory = category.ToLower();
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT name, path FROM audio WHERE category = $category;";
            cmd.Parameters.AddWithValue("$category", lowerCategory);
            var result = new List<Audio>();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Add(new Audio
                    {
                        Category = lowerCategory,
                        Name = reader.GetString(0),
                        Path = reader.GetString(1)
                    });
                }
            }
            connection.Close();
            return result;
        }

        public async Task<Audio> GetAudio(string category, string name)
        {
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT category, name, path FROM audio WHERE category = $category AND name = $name LIMIT 1;";
            cmd.Parameters.AddWithValue("$category", category.ToLower());
            cmd.Parameters.AddWithValue("$name", name.ToLower());
            Audio result = null;
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    result = new Audio
                    {
                        Category = reader.GetString(0),
                        Name = reader.GetString(1),
                        Path = reader.GetString(2)
                    };
                }
            }
            connection.Close();
            return result;
        }

        public async Task<List<Audio>> GetAllAudio()
        {
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT name, category, path FROM audio;";
            var result = new List<Audio>();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Add(new Audio
                    {
                        Name = reader.GetString(0),
                        Category = reader.GetString(1),
                        Path = reader.GetString(2)
                    });
                }
            }
            return result;
        }

        public async Task AddAudio(Audio audio)
        {
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO audio (name, category, path) VALUES ($name, $category, $path);";
            cmd.Parameters.AddWithValue("$name", audio.Name);
            cmd.Parameters.AddWithValue("$category", audio.Category.ToLower());
            cmd.Parameters.AddWithValue("$path", audio.Path);
            await cmd.ExecuteNonQueryAsync();
            connection.Close();
        }

        public async Task RemoveAudio(string name, string category)
        {
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM audio WHERE name = $name AND category = $category;";
            cmd.Parameters.AddWithValue("$name", name.ToLower());
            cmd.Parameters.AddWithValue("$category", category.ToLower());
            await cmd.ExecuteNonQueryAsync();
            connection.Close();
        }
    }
}