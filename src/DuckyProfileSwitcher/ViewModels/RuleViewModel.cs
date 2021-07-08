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

        public bool Enabled
        {
            get => Rule.Enabled;
            set
            {
                Rule.Enabled = value;
                OnPropertyChanged();
            }
        }

        public bool UsesGoToSleep
        {
            get => Rule.SwitchAction == SwitchAction.Sleep;
            set
            {
                Rule.SwitchAction = SwitchAction.Sleep;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UsesProfileNumber));
                OnPropertyChanged(nameof(UsesProfileSearch));
            }
        }

        public bool UsesProfileNumber
        {
            get => Rule.SwitchAction == SwitchAction.SwitchToProfileNumber;
            set
            {
                Rule.SwitchAction = SwitchAction.SwitchToProfileNumber;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UsesGoToSleep));
                OnPropertyChanged(nameof(UsesProfileSearch));
            }
        }

        public bool UsesProfileSearch
        {
            get => Rule.SwitchAction == SwitchAction.SwitchToProfileName;
            set
            {
                Rule.SwitchAction = SwitchAction.SwitchToProfileName;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UsesGoToSleep));
                OnPropertyChanged(nameof(UsesProfileNumber));
            }
        }

        public int? ProfileNumber
        {
            get => Rule.ProfileNumber;
            set
            {
                Rule.ProfileNumber = value;
                OnPropertyChanged();
            }
        }

        public string? ProfileName
        {
            get => Rule.ProfileName;
            set
            {
                Rule.ProfileName = value;
                OnPropertyChanged();
            }
        }
    }
}
