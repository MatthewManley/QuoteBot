using Domain.Models;

namespace Aws.Models
{
    internal class GetNamedAudioResponse
    {
        public uint AudioId { get; init; }
        public string AudioPath { get; init; }
        public ulong AudioUploader { get; init; }
        public uint AudioOwnerId { get; init; }
        public uint AudioOwnerAudioId { get; init; }
        public string AudioOwnerName { get; init; }
        public ulong AudioOwnerOwnerId { get; init; }

        public NamedAudio ToNamedAudio()
        {
            return new NamedAudio
            {
                Audio = new Audio
                {
                    Id = AudioId,
                    Path = AudioPath,
                    Uploader = AudioUploader
                },
                AudioOwner = new AudioOwner
                {
                    Id = AudioOwnerId,
                    AudioId = AudioOwnerAudioId,
                    Name = AudioOwnerName,
                    OwnerId = AudioOwnerOwnerId
                }
            };
        }
    }
}
