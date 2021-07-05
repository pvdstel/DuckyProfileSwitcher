using DuckyProfileSwitcher.Models;

namespace DuckyProfileSwitcher.ViewModels
{
    public class RuleViewModel : Notifyable
    {
        private readonly bool autoCommit;

        public RuleViewModel(Rule rule, bool autoCommit = false)
        {
            Rule = rule;
            this.autoCommit = autoCommit;
        }

        public Rule Rule { get; }

        public string Name
        {
            get => Rule.Name;
            set => Rule.Name = value;
        }

        public bool UsesProfileSearch
        {
            get => !string.IsNullOrEmpty(Rule.ProfileDescription.Name);
            set
            {
                if (value)
                {
                    Rule.ProfileDescription.Number = null;
                    Rule.ProfileDescription.Name = "profile";
                }
                else
                {
                    Rule.ProfileDescription.Number = 1;
                    Rule.ProfileDescription.Name = null;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(ProfileNumber));
                OnPropertyChanged(nameof(ProfileName));
            }
        }

        public int? ProfileNumber
        {
            get => Rule.ProfileDescription.Number;
            set
            {
                Rule.ProfileDescription.Number = value;
                OnPropertyChanged();
            }
        }

        public string? ProfileName
        {
            get => Rule.ProfileDescription.Name;
            set
            {
                Rule.ProfileDescription.Name = value;
                OnPropertyChanged();
            }
        }

        public bool Enabled
        {
            get => Rule.Enabled;
            set
            {
                Rule.Enabled = value;
                OnPropertyChanged();
            }
        }
    }
}
