using System;

namespace Domain.Models
{
    public class Audio : IEquatable<Audio>
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        public override bool Equals(object obj) => Equals(obj as Audio);
        public override int GetHashCode() => (Id, Name, Path).GetHashCode();

        public bool Equals(Audio other)
        {
            if (other is null)
                return false;
            return Id == other.Id;
        }
    }
}