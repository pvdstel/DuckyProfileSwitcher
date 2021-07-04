using DuckyProfileSwitcher.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

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

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            viewModel.EditRule();
        }
    }
}
