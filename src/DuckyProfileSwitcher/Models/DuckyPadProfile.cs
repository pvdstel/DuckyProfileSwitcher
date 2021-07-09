using System;
using System.Collections.Generic;

namespace DuckyProfileSwitcher.Models
{
    public class DuckyPadProfile : IEquatable<DuckyPadProfile?>
    {
        public DuckyPadProfile(byte number, string name)
        {
            Number = number;
            Name = name;
        }

        public byte Number { get; set; }

        public string Name { get; set; }

        public string DisplayText => $"{Number} - {Name}";

        public override bool Equals(object? obj)
        {
            return Equals(obj as DuckyPadProfile);
        }

        public bool Equals(DuckyPadProfile? other)
        {
            return other != null &&
                   Number == other.Number &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = 1230293284;
            hashCode = (hashCode * -1521134295) + Number.GetHashCode();
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        public override string ToString()
        {
            return DisplayText;
        }

        public static bool operator ==(DuckyPadProfile? left, DuckyPadProfile? right)
        {
            return EqualityComparer<DuckyPadProfile>.Default.Equals(left!, right!);
        }

        public static bool operator !=(DuckyPadProfile? left, DuckyPadProfile? right)
        {
            return !(left == right);
        }
    }
}
