using System;

namespace Domain.Models
{
    public class Audio : IEquatable<Audio>
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        public override bool Equals(object obj) => Equals(obj as Audio);
        public override int GetHashCode() => (Category, Name, Path).GetHashCode();

        public bool Equals(Audio other)
        {
            if (other is null)
                return false;
            return Path == other.Path;
        }
    }
}