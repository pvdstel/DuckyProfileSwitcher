using DuckyProfileSwitcher.Models;
using DuckyProfileSwitcher.Utilities;
using DuckyProfileSwitcher.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DuckyProfileSwitcher.ViewModels
{
    public class RuleListViewModel : Notifyable, IDisposable
    {
        private ObservableCollection<RuleViewModel> rules = new();
        private RuleViewModel? selectedRule;
        private bool disposedValue;

        public RuleListViewModel()
        {
            var viewModels = ConfigurationManager.Configuration.Rules
                .Select(r => new RuleViewModel(r, true));
            rules = new ObservableCollection<RuleViewModel>(viewModels);
            ConfigurationManager.ConfigurationChanged += ConfigurationManager_ConfigurationChanged;

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
                ConfigurationManager.Configuration.Rules.ReplaceWith(r, copy);
                RuleViewModel rvm = new(copy, true);
                ConfigurationManager.Save();
                Rules.ReplaceWith(sr, rvm);
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
                ConfigurationManager.Save();
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
                ConfigurationManager.Save();
            }

            SelectedRule = sr;
        }

        private void ConfigurationManager_ConfigurationChanged(object sender, EventArgs e)
        {
            var viewModels = ConfigurationManager.Configuration.Rules
                .Select(r => new RuleViewModel(r, true));
            Rules = new ObservableCollection<RuleViewModel>(viewModels);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ConfigurationManager.ConfigurationChanged -= ConfigurationManager_ConfigurationChanged;
                }

                disposedValue = true;
            }
        }

        ~RuleListViewModel()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
