using DuckyProfileSwitcher.Models;

namespace DuckyProfileSwitcher.ViewModels
{
    public class RuleViewModel : Notifyable
    {
        public RuleViewModel(Rule rule)
        {
            Rule = rule;
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
            set => Rule.ProfileDescription.Number = value;
        }

        public string? ProfileName
        {
            get => Rule.ProfileDescription.Name;
            set => Rule.ProfileDescription.Name = value;
        }
    }
}
