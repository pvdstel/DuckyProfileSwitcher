namespace DuckyProfileSwitcher.Models
{
    public class Rule
    {
        public Rule() { }

        public Rule(Rule original)
        {
            Name = original.Name;
            ProfileDescription = new(original.ProfileDescription);
            Enabled = original.Enabled;
        }

        public string Name { get; set; } = "New rule";

        public ProfileSearchDescription ProfileDescription { get; set; } = new ProfileSearchDescription();

        public bool Enabled { get; set; } = true;
    }
}
