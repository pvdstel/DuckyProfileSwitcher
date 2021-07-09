using DuckyProfileSwitcher.Models;
using DuckyProfileSwitcher.Utilities;
using System.Collections.Immutable;
using System.Linq;

namespace DuckyProfileSwitcher.ViewModels
{
    public class RuleViewModel : Notifyable
    {
        private readonly bool autoCommit;

        public RuleViewModel()
            : this(new Rule(), false) { }

        public RuleViewModel(Rule rule, bool autoCommit = false)
        {
            Rule = rule;
            this.autoCommit = autoCommit;
            ProfileNames = DuckyPadManager.Instance.Profiles?.Select(r => r.Name).ToImmutableList() ?? ImmutableList<string>.Empty;
        }

        public Rule Rule { get; }

        public ImmutableList<string> ProfileNames { get; }

        public string Name
        {
            get => Rule.Name;
            set
            {
                Rule.Name = value;
                OnPropertyChanged();
                CommitAndSave();
            }
        }

        public bool Enabled
        {
            get => Rule.Enabled;
            set
            {
                Rule.Enabled = value;
                OnPropertyChanged();
                CommitAndSave();
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
                CommitAndSave();
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
                OnPropertyChanged(nameof(ProfileNumberNotFound));
                OnPropertyChanged(nameof(ProfileNameNotFound));
                CommitAndSave();
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
                OnPropertyChanged(nameof(ProfileNumberNotFound));
                OnPropertyChanged(nameof(ProfileNameNotFound));
                CommitAndSave();
            }
        }

        public int? ProfileNumber
        {
            get => Rule.ProfileNumber;
            set
            {
                Rule.ProfileNumber = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProfileNumberNotFound));
                CommitAndSave();
            }
        }

        public bool ProfileNumberNotFound => UsesProfileNumber && Rule.FindProfile() == null;

        public string? ProfileName
        {
            get => Rule.ProfileName;
            set
            {
                Rule.ProfileName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProfileNameNotFound));
                CommitAndSave();
            }
        }

        public bool ProfileNameNotFound => UsesProfileSearch && Rule.FindProfile() == null;

        public string? AppName
        {
            get => Rule.AppName;
            set
            {
                Rule.AppName = value;
                OnPropertyChanged();
                CommitAndSave();
            }
        }

        public string? WindowTitle
        {
            get => Rule.WindowTitle;
            set
            {
                Rule.WindowTitle = value;
                OnPropertyChanged();
                CommitAndSave();
            }
        }

        public string? WindowClass
        {
            get => Rule.WindowClass;
            set
            {
                Rule.WindowClass = value;
                OnPropertyChanged();
                CommitAndSave();
            }
        }

        private void CommitAndSave()
        {
            if (autoCommit)
            {
                ConfigurationManager.Configuration.Rules.ReplaceWith(Rule, Rule);
                ConfigurationManager.Save();
            }
        }
    }
}
