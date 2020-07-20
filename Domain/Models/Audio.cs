using System;

namespace Domain.Models
{
    public class Audio : IEquatable<Audio>
    {
        public uint Id { get; set; }
        public string Path { get; set; }
        public ulong Uploader { get; set; }

        public override bool Equals(object obj) => Equals(obj as Audio);
        public override int GetHashCode() => (Id, Path, Uploader).GetHashCode();

        public bool Equals(Audio other)
        {
            if (other is null)
                return false;
            return Id == other.Id;
        }
    }
}