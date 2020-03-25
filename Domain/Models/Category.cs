using System;

namespace Domain.Models
{
    public class Category : IEquatable<Category>
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public ulong OwnerId { get; set; }

        public override bool Equals(object obj) => Equals(obj as Audio);
        public override int GetHashCode() => (Id, OwnerId, Name).GetHashCode();

        public bool Equals(Category other)
        {
            if (other is null)
                return false;
            return Id == other.Id;
        }
    }
}