namespace DuckyProfileSwitcher.Models
{
    public class DuckyPadProfile
    {
        public DuckyPadProfile(byte number, string name)
        {
            Number = number;
            Name = name;
        }

        public byte Number { get; set; }

        public string Name { get; set; }

        public string DisplayText => $"{Number} - {Name}";
    }
}
