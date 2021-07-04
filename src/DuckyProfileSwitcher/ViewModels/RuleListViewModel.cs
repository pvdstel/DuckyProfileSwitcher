using DuckyProfileSwitcher.Models;
using System.Collections.ObjectModel;

namespace DuckyProfileSwitcher.ViewModels
{
    public class RuleListViewModel : ViewModelBase
    {
        private ObservableCollection<Rule> rules = new();

        public RuleListViewModel()
        {
            Rule r = new();
            r.Name = "test";
            
            Rules.Add(r);
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
    }
}
