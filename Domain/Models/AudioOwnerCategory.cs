using System;

namespace Domain.Models
{
    public class AudioOwnerCategory : IEquatable<AudioOwnerCategory>
    {
        public uint AudioOwnerId { get; set; }
        public uint CategoryId { get; set; }

        public override bool Equals(object obj) => Equals(obj as AudioOwnerCategory);
        public override int GetHashCode() => (AudioOwnerId, CategoryId).GetHashCode();
        public bool Equals(AudioOwnerCategory other) => other != null && AudioOwnerId == other.AudioOwnerId && CategoryId == other.CategoryId;
    }
}