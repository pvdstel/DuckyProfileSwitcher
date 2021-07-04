using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckyProfileSwitcher.Models
{
    public class ProfileSearchDescription
    {
        public ProfileSearchDescription() { }

        public ProfileSearchDescription(ProfileSearchDescription original)
        {
            Number = original.Number;
            Name = original.Name;
        }

        public int? Number { get; set; } = 1;

        public string? Name { get; set; }

        public DuckyPadProfile? FindMatch(IEnumerable<DuckyPadProfile> profiles)
        {
            var outEnumerable = profiles;

            if (Number.HasValue)
            {
                outEnumerable = outEnumerable.Where(d => d.Number == Number);
            }

            if (!string.IsNullOrEmpty(Name))
            {
                outEnumerable = outEnumerable.Where(d => string.Equals(d.Name, Name, StringComparison.CurrentCultureIgnoreCase));
            }

            return outEnumerable.FirstOrDefault();
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name) && Number.HasValue)
            {
                return $"{Number.Value} + {Name}";
            }
            else if (!string.IsNullOrEmpty(Name))
            {
                return Name!;
            }
            else if (Number.HasValue)
            {
                return Number.Value.ToString();
            }
            return "-";
        }
    }
}
