using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Repos;
using System.Linq;

namespace Infrastructure
{
    public class AudioOwnerRepo : IAudioOwnerRepo
    {
        private readonly DbConnection connection;

        public AudioOwnerRepo(DbConnection connection)
        {
            this.connection = connection;
        }

        public async Task<AudioOwner> AddAudioOwner(AudioOwner audioOwner)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "INSERT INTO audio_owner (audio, owner, name) VALUES ($audio, $owner, $name); " +
                "SELECT last_insert_rowid();";
            cmd.AddParameterWithValue("$audio", audioOwner.AudioId);
            cmd.AddParameterWithValue("$owner", audioOwner.OwnerId.ToString());
            cmd.AddParameterWithValue("$name", audioOwner.Name);
            var id = (long)await cmd.ExecuteScalarAsync();
            audioOwner.Id = id;
            await connection.CloseAsync();
            return audioOwner;
        }

        public async Task<List<AudioOwner>> GetAudioOwners(ulong ownerId)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT id, audio, owner, name FROM audio_owner WHERE owner = $owner;";
            cmd.AddParameterWithValue("$owner", ownerId.ToString());
            var reader = await cmd.ExecuteReaderAsync();
            var enumerable = reader.ReadToEnumerable(() => new AudioOwner {
                Id = reader.GetInt64(0),
                AudioId = reader.GetInt64(1),
                OwnerId = ulong.Parse(reader.GetString(2)),
                Name = reader.GetString(3)
            });
            var result = await enumerable.ToListAsync();
            await connection.CloseAsync();
            return result;
        }
    }
}