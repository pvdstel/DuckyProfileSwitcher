using DuckyProfileSwitcher.Models;
using DuckyProfileSwitcher.ViewModels;

namespace DuckyProfileSwitcher.Views
{
    /// <summary>
    /// Interaction logic for RuleEditor.xaml
    /// </summary>
    public partial class RuleEditor : MahApps.Metro.Controls.MetroWindow
    {
        private readonly RuleViewModel viewModel;

        public RuleEditor(Rule rule)
        {
            InitializeComponent();
            viewModel = new(rule);
            DataContext = viewModel;
        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
