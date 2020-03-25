using System;

namespace Domain.Models
{
    public class AudioOwner : IEquatable<AudioOwner>
    {
        public long Id { get; set; }
        public long AudioId { get; set; }
        public ulong OwnerId { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj) => Equals(obj as AudioOwner);
        public override int GetHashCode() => (AudioId, OwnerId, Name).GetHashCode();

        public bool Equals(AudioOwner other)
        {
            if (other is null)
                return false;
            return AudioId == other.AudioId && OwnerId == other.OwnerId;
        }
    }
}