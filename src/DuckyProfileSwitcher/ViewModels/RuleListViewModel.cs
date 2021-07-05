using DuckyProfileSwitcher.Models;
using DuckyProfileSwitcher.Utilities;
using DuckyProfileSwitcher.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DuckyProfileSwitcher.ViewModels
{
    public class RuleListViewModel : Notifyable
    {
        private ObservableCollection<RuleViewModel> rules = new();
        private RuleViewModel? selectedRule;

        public RuleListViewModel()
        {
            var viewModels = ConfigurationManager.Configuration.Rules
                .Select(r => new RuleViewModel(r, true));
            rules = new ObservableCollection<RuleViewModel>(viewModels);

            AddRuleCommand = new(NewRule);
            EditSelectedRuleCommand = new(EditSelectedRule, () => SelectedRule != null);
            DeleteSelectedRuleCommand = new(DeleteSelectedRule, () => SelectedRule != null);
            MoveSelectedRuleUpCommand = new(MoveSelectedRuleUp, () => SelectedRule != null && Rules.IndexOf(SelectedRule) > 0);
            MoveSelectedRuleDownCommand = new(MoveSelectedRuleDown, () => SelectedRule != null && Rules.IndexOf(SelectedRule) < Rules.Count - 1);
        }

        public ObservableCollection<RuleViewModel> Rules
        {
            get => rules;
            private set
            {
                rules = value;
                OnPropertyChanged();
            }
        }

        public RuleViewModel? SelectedRule
        {
            get => selectedRule;
            set
            {
                selectedRule = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand AddRuleCommand { get; }

        public RelayCommand EditSelectedRuleCommand { get; }

        public RelayCommand DeleteSelectedRuleCommand { get; }

        public RelayCommand MoveSelectedRuleUpCommand { get; }

        public RelayCommand MoveSelectedRuleDownCommand { get; }

        public void NewRule()
        {
            Rule newRule = new();
            RuleEditor re = new(newRule);
            re.Owner = System.Windows.Application.Current.MainWindow;
            if (re.ShowDialog().GetValueOrDefault())
            {
                ConfigurationManager.Configuration.Rules.Add(newRule);
                ConfigurationManager.Save();
                RuleViewModel rvm = new(newRule, true);
                Rules.Add(rvm);
                SelectedRule = rvm;
            }
        }

        public void EditSelectedRule()
        {
            RuleViewModel? sr = SelectedRule;
            Rule? r = sr?.Rule;
            if (r == null || sr == null)
            {
                return;
            }

            Rule copy = new(r);
            RuleEditor re = new(copy);
            re.Owner = System.Windows.Application.Current.MainWindow;
            if (re.ShowDialog().GetValueOrDefault())
            {
                ReplaceInList(ConfigurationManager.Configuration.Rules, r, copy);
                RuleViewModel rvm = new(copy, true);
                ConfigurationManager.Save();
                ReplaceInList(Rules, sr, rvm);
                SelectedRule = rvm;
            }
        }

        public void DeleteSelectedRule()
        {
            RuleViewModel? sr = SelectedRule;
            Rule? r = sr?.Rule;
            if (r == null || sr == null)
            {
                return;
            }

            if (Rules.Contains(sr))
            {
                Rules.Remove(sr);
            }
            if (ConfigurationManager.Configuration.Rules.Contains(r))
            {
                ConfigurationManager.Configuration.Rules.Remove(r);
                ConfigurationManager.Save();
            }
        }

        private void MoveSelectedRuleUp()
        {
            RuleViewModel? sr = SelectedRule;
            Rule? r = sr?.Rule;
            if (r == null || sr == null)
            {
                return;
            }

            int vIndex = Rules.IndexOf(sr);
            if (vIndex > 0)
            {
                Rules.RemoveAt(vIndex);
                Rules.Insert(vIndex - 1, sr);
            }

            int index = ConfigurationManager.Configuration.Rules.IndexOf(r);
            if (index > 0)
            {
                ConfigurationManager.Configuration.Rules.RemoveAt(index);
                ConfigurationManager.Configuration.Rules.Insert(index - 1, r);
            }

            SelectedRule = sr;
        }

        public void MoveSelectedRuleDown()
        {
            RuleViewModel? sr = SelectedRule;
            Rule? r = sr?.Rule;
            if (r == null || sr == null)
            {
                return;
            }

            int vIndex = Rules.IndexOf(sr);
            if (vIndex < Rules.Count - 1)
            {
                Rules.RemoveAt(vIndex);
                Rules.Insert(vIndex + 1, sr);
            }

            int index = ConfigurationManager.Configuration.Rules.IndexOf(r);
            if (index < ConfigurationManager.Configuration.Rules.Count - 1)
            {
                ConfigurationManager.Configuration.Rules.RemoveAt(index);
                ConfigurationManager.Configuration.Rules.Insert(index + 1, r);
            }

            SelectedRule = sr;
        }

        private void ReplaceInList<T>(IList<T> list, T existingRule, T newRule)
        {
            int index = list.IndexOf(existingRule);
            if (index < 0)
            {
                return;
            }

            list.RemoveAt(index);
            list.Insert(index, newRule);
        }
    }
}
