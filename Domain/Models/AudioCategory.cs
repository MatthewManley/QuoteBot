using System;

namespace Domain.Models
{
    public class AudioCategory : IEquatable<AudioCategory>
    {
        public uint AudioId { get; set; }
        public uint CategoryId { get; set; }

        public override bool Equals(object obj) => Equals(obj as AudioCategory);
        public override int GetHashCode() => (AudioId, CategoryId).GetHashCode();
        public bool Equals(AudioCategory other) => other != null && AudioId == other.AudioId && CategoryId == other.CategoryId;
    }
}