using System;

namespace Domain.Models
{
    public class AudioCategory : IEquatable<AudioCategory>
    {
        public long AudioOwnerId { get; set; }
        public long CategoryId { get; set; }

        public override bool Equals(object obj) => Equals(obj as AudioCategory);
        public override int GetHashCode() => (AudioOwnerId, CategoryId).GetHashCode();

        public bool Equals(AudioCategory other)
        {
            if (other is null)
                return false;
            return AudioOwnerId == other.AudioOwnerId && CategoryId == other.CategoryId;
        }
    }
}