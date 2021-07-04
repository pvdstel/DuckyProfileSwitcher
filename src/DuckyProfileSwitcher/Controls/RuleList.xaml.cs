using DuckyProfileSwitcher.ViewModels;
using System.Windows.Controls;

namespace DuckyProfileSwitcher.Controls
{
    /// <summary>
    /// Interaction logic for RuleList.xaml
    /// </summary>
    public partial class RuleList : UserControl
    {
        private readonly RuleListViewModel viewModel = new();

        public RuleList()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
