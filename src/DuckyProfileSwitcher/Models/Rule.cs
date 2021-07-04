namespace DuckyProfileSwitcher.Models
{
    public class Rule
    {
        public string Name { get; set; } = string.Empty;

        public ProfileSearchDescription ProfileDescription { get; set; } = new ProfileSearchDescription();

        public bool Enabled { get; set; } = true;
    }
}
