using DuckyProfileSwitcher.Models;
using DuckyProfileSwitcher.Utilities;
using DuckyProfileSwitcher.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DuckyProfileSwitcher.ViewModels
{
    public class RuleListViewModel : Notifyable
    {
        private ObservableCollection<Rule> rules = new();
        private Rule? selectedRule;

        public RuleListViewModel()
        {
            Rule r = new();
            r.Name = "test";
            
            Rules.Add(r);

            AddRuleCommand = new(NewRule);
            EditRuleCommand = new(EditRule, () => SelectedRule != null);
            DeleteRuleCommand = new(DeleteRule, () => SelectedRule != null);
        }

        public ObservableCollection<Rule> Rules
        {
            get => rules;
            private set
            {
                rules = value;
                OnPropertyChanged();
            }
        }

        public Rule? SelectedRule
        {
            get => selectedRule;
            set
            {
                selectedRule = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand AddRuleCommand { get; }

        public RelayCommand EditRuleCommand { get; }

        public RelayCommand DeleteRuleCommand { get; }

        public void NewRule()
        {
            Rule newRule = new();
            RuleEditor re = new(newRule);
            re.Owner = System.Windows.Application.Current.MainWindow;
            if (re.ShowDialog().GetValueOrDefault())
            {
                Rules.Add(newRule);
                SelectedRule = newRule;
            }
        }

        public void EditRule()
        {
            Rule? r = SelectedRule;
            if (r == null)
            {
                return;
            }

            Rule copy = new(r);
            RuleEditor re = new(copy);
            re.Owner = System.Windows.Application.Current.MainWindow;
            if (re.ShowDialog().GetValueOrDefault())
            {
                ReplaceInList(Rules, r, copy);
                SelectedRule = copy;
            }
        }

        public void DeleteRule()
        {
            Rule? r = SelectedRule;
            if (r == null || !Rules.Contains(r))
            {
                return;
            }

            Rules.Remove(r);
        }

        private void ReplaceInList(IList<Rule> list, Rule existingRule, Rule newRule)
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
