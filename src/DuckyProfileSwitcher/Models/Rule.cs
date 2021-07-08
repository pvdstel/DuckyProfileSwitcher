using System;
using System.Collections.Generic;

namespace DuckyProfileSwitcher.Models
{
    public class Rule : IEquatable<Rule?>
    {
        public Rule()
        {
            Id = Guid.NewGuid();
        }

        public Rule(Rule original)
        {
            Id = original.Id;
            Name = original.Name;
            SwitchAction = original.SwitchAction;
            Enabled = original.Enabled;
            ProfileNumber = original.ProfileNumber;
            ProfileName = original.ProfileName;
        }

        public Guid Id { get; set; }

        public string Name { get; set; } = "New rule";

        public SwitchAction SwitchAction { get; set; } = SwitchAction.SwitchToProfileNumber;

        public bool Enabled { get; set; } = true;

        public int? ProfileNumber { get; set; } = 1;

        public string? ProfileName { get; set; }


        public override bool Equals(object? obj)
        {
            return Equals(obj as Rule);
        }

        public bool Equals(Rule? other)
        {
            return other != null && Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            int hashCode = -594382924;
            hashCode = (hashCode * -1521134295) + Id.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Rule? left, Rule? right)
        {
            return EqualityComparer<Rule>.Default.Equals(left!, right!);
        }

        public static bool operator !=(Rule? left, Rule? right)
        {
            return !(left == right);
        }
    }
}
