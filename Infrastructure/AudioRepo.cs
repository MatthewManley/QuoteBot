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

        public async Task<NamedAudio> GetAudio(ulong ownerId, string categoryName, string audioName)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT audio.id, audio.path, audio.uploader, audio_owner.id, audio_owner.audio, audio_owner.owner, audio_owner.name FROM category " +
                "INNER JOIN audio_category ON category.id = audio_category.category " +
                "INNER JOIN audio_owner ON audio_category.audio_owner = audio_owner.id " +
                "INNER JOIN audio ON audio_owner.audio = audio.id " +
                "WHERE category.owner = $ownerId AND category.name = $categoryName AND audio_owner.owner = $ownerId AND audio_owner.name = $audioName;";
            cmd.AddParameterWithValue("$ownerId", ownerId.ToString());
            cmd.AddParameterWithValue("$categoryName", categoryName);
            cmd.AddParameterWithValue("$audioName", audioName);

            using var reader = await cmd.ExecuteReaderAsync();
            var enumerable = reader.ReadToEnumerable(() => {
                var audio =  new Audio {
                    Id = reader.GetInt64(0),
                    Path = reader.GetString(1),
                    Uploader = ulong.Parse(reader.GetString(2)),
                };
                var audioOwner = new AudioOwner {
                    Id = reader.GetInt64(3),
                    AudioId = reader.GetInt64(4),
                    OwnerId = ulong.Parse(reader.GetString(5)),
                    Name = reader.GetString(6)
                };
                return new NamedAudio {
                    Audio = audio,
                    AudioOwner = audioOwner
                };
            });
            var result = await enumerable.FirstOrDefaultAsync();
            await connection.CloseAsync();
            return result;
        }

        public async Task<List<NamedAudio>> GetAudioForCategory(ulong ownerId, string categoryName)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT audio.id, audio.path, audio.uploader, audio_owner.id, audio_owner.audio, audio_owner.owner, audio_owner.name FROM category " +
                "INNER JOIN audio_category ON category.id = audio_category.category " +
                "INNER JOIN audio_owner ON audio_category.audio_owner = audio_owner.id " +
                "INNER JOIN audio ON audio_owner.audio = audio.id " +
                "WHERE category.owner = $ownerId AND category.name = $categoryName AND audio_owner.owner = $ownerId;";
            cmd.AddParameterWithValue("$ownerId", ownerId.ToString());
            cmd.AddParameterWithValue("$categoryName", categoryName);

            using var reader = await cmd.ExecuteReaderAsync();
            var enumerable = reader.ReadToEnumerable(() => {
                var audio =  new Audio {
                    Id = reader.GetInt64(0),
                    Path = reader.GetString(1),
                    Uploader = ulong.Parse(reader.GetString(2)),
                };
                var audioOwner = new AudioOwner {
                    Id = reader.GetInt64(3),
                    AudioId = reader.GetInt64(4),
                    OwnerId = ulong.Parse(reader.GetString(5)),
                    Name = reader.GetString(6)
                };
                return new NamedAudio {
                    Audio = audio,
                    AudioOwner = audioOwner
                };
            });
            var result = await enumerable.ToListAsync();
            await connection.CloseAsync();
            return result;
        }

        public async Task<List<NamedAudio>> GetAudioForCategory(long categoryId)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT audio.id, audio.path, audio.uploader, audio_owner.id, audio_owner.audio, audio_owner.owner, audio_owner.name FROM category " +
                "INNER JOIN audio_category ON category.id = audio_category.category " +
                "INNER JOIN audio_owner ON audio_category.audio_owner = audio_owner.id " +
                "INNER JOIN audio ON audio_owner.audio = audio.id " +
                "WHERE category.id = $categoryId;";
            cmd.AddParameterWithValue("$categoryId", categoryId);

            using var reader = await cmd.ExecuteReaderAsync();
            var enumerable = reader.ReadToEnumerable(() => {
                var audio =  new Audio {
                    Id = reader.GetInt64(0),
                    Path = reader.GetString(1),
                    Uploader = ulong.Parse(reader.GetString(2)),
                };
                var audioOwner = new AudioOwner {
                    Id = reader.GetInt64(3),
                    AudioId = reader.GetInt64(4),
                    OwnerId = ulong.Parse(reader.GetString(5)),
                    Name = reader.GetString(6)
                };
                return new NamedAudio {
                    Audio = audio,
                    AudioOwner = audioOwner
                };
            });
            var result = await enumerable.ToListAsync();
            await connection.CloseAsync();
            return result;
        }

        // public async Task<List<Audio>> GetAllAudioForCategory(string categoryName, ulong ownerId)
        // {
        //     await connection.OpenAsync();
        //     using var cmd = connection.CreateCommand();
        //     cmd.CommandText =
        //         "SELECT audio.id, audio.name, audio.path FROM category " +
        //         "INNER JOIN audio_category on audio.id = audio_category.audio " +
        //         "INNER JOIN category on audio_category.category = category.id " +
        //         "WHERE category.name = $category;";

        //     // Parameterizing input to prevent sql injection
        //     cmd.AddParameterWithValue("$category", categoryName.ToLowerInvariant());

        //     using var reader = await cmd.ExecuteReaderAsync();
        //     var enumerable = reader.ReadToEnumerable(() => new Audio
        //     {
        //         Id = reader.GetInt64(0),
        //         Name = reader.GetString(1),
        //         Path = reader.GetString(2)
        //     });
        //     var result = await enumerable.ToListAsync();
        //     await connection.CloseAsync();
        //     return result;
        // }

        // public async Task<List<Audio>> GetAllAudioForCategory(long categoryId)
        // {
        //     await connection.OpenAsync();
        //     using var cmd = connection.CreateCommand();
        //     cmd.CommandText =
        //         "SELECT audio.id, audio.name, audio.path FROM audio " +
        //         "INNER JOIN audio_category on audio.id = audio_category.audio " +
        //         "INNER JOIN category on audio_category.category = category.id " +
        //         "WHERE category.id = $id;";

        //     // Parameterizing input to prevent sql injection
        //     cmd.AddParameterWithValue("$id", categoryId);

        //     using var reader = await cmd.ExecuteReaderAsync();
        //     var enumerable = reader.ReadToEnumerable(() => new Audio
        //     {
        //         Id = reader.GetInt64(0),
        //         Name = reader.GetString(1),
        //         Path = reader.GetString(2)
        //     });
        //     var result = await enumerable.ToListAsync();
        //     await connection.CloseAsync();
        //     return result;
        // }

        // public async Task<Audio> GetAudio(long id)
        // {
        //     await connection.OpenAsync();
        //     using var cmd = connection.CreateCommand();
        //     cmd.CommandText =
        //         "SELECT id, name, path FROM audio " +
        //         "WHERE id = $id " +
        //         "LIMIT 1;";

        //     // Parameterizing input to prevent sql injection
        //     cmd.AddParameterWithValue("$id", id);

        //     using var reader = await cmd.ExecuteReaderAsync();
        //     var result = await reader.ReadFirstOrDefault(() => new Audio
        //     {
        //         Id = reader.GetInt64(0),
        //         Name = reader.GetString(1),
        //         Path = reader.GetString(2)
        //     });
        //     await connection.CloseAsync();
        //     return result;
        // }

        // public async Task<Audio> GetAudio(string category, string name)
        // {
        //     await connection.OpenAsync();
        //     using var cmd = connection.CreateCommand();
        //     cmd.CommandText =
        //         "SELECT audio.id, audio.name, audio.path FROM audio " +
        //         "INNER JOIN audio_category on audio.id = audio_category.audio " +
        //         "INNER JOIN category on audio_category.category = category.id " +
        //         "WHERE category.name = $category AND audio.name = $name " +
        //         "LIMIT 1;";

        //     // Parameterizing user input to prevent sql injection
        //     cmd.AddParameterWithValue("$category", category.ToLowerInvariant());
        //     cmd.AddParameterWithValue("$name", name.ToLowerInvariant());

        //     using var reader = await cmd.ExecuteReaderAsync();

        //     var result = await reader.ReadFirstOrDefault(() => new Audio
        //     {
        //         Id = reader.GetInt64(0),
        //         Name = reader.GetString(1),
        //         Path = reader.GetString(2)
        //     });

        //     await connection.CloseAsync();
        //     return result;
        // }

        // public async Task<List<Audio>> GetAllAudio()
        // {
        //     await connection.OpenAsync();
        //     using var cmd = connection.CreateCommand();
        //     cmd.CommandText = "SELECT audio.name, audio.id, audio.path FROM audio;";
        //     using var reader = await cmd.ExecuteReaderAsync();
        //     var enumerable = reader.ReadToEnumerable(() => new Audio
        //     {
        //         Name = reader.GetString(0),
        //         Id = reader.GetInt64(1),
        //         Path = reader.GetString(2)
        //     });
        //     var result = await enumerable.ToListAsync();
        //     await connection.CloseAsync();
        //     return result;
        // }

        // public async Task<Audio> AddAudio(Audio audio)
        // {
        //     await connection.OpenAsync();
        //     using var cmd = connection.CreateCommand();
        //     cmd.CommandText = "INSERT INTO audio (name, path) VALUES ($name, $path); SELECT last_insert_rowid();";
        //     cmd.AddParameterWithValue("$name", audio.Name);
        //     cmd.AddParameterWithValue("$path", audio.Path);
        //     var id = (long)await cmd.ExecuteScalarAsync();
        //     await connection.CloseAsync();
        //     audio.Id = id;
        //     return audio;
        // }

        // public async Task<long> AddCategoryToAudio(long categoryId, long audioId)
        // {
        //     await connection.OpenAsync();
        //     using var cmd = connection.CreateCommand();
        //     cmd.CommandText = "INSERT INTO audio_category (audio, category) VALUES ($audio, $category); SELECT last_insert_rowid();";
        //     cmd.AddParameterWithValue("$audio", audioId);
        //     cmd.AddParameterWithValue("$category", categoryId);
        //     var id = (long)await cmd.ExecuteScalarAsync();
        //     await connection.CloseAsync();
        //     return id;
        // }
    }
}