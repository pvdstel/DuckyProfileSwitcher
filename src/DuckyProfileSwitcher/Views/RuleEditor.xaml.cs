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
        private int errorCount = 0;

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

        private void MetroWindow_Error(object sender, System.Windows.Controls.ValidationErrorEventArgs e)
        {
            if (e.Action == System.Windows.Controls.ValidationErrorEventAction.Added)
            {
                ++errorCount;
            }
            else if (e.Action == System.Windows.Controls.ValidationErrorEventAction.Removed)
            {
                --errorCount;
            }
            save.IsEnabled = errorCount == 0;
        }
    }
}
