using System;

namespace Domain.Models
{
    public class AudioOwner : IEquatable<AudioOwner>
    {
        public uint Id { get; set; }
        public uint AudioId { get; set; }
        public ulong OwnerId { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj) => Equals(obj as AudioOwner);
        public override int GetHashCode() => (AudioId, OwnerId, Name).GetHashCode();
        public bool Equals(AudioOwner other) => other != null && AudioId == other.AudioId && OwnerId == other.OwnerId;
    }
}