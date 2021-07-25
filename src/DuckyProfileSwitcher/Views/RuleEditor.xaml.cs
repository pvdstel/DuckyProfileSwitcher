using DuckyProfileSwitcher.Models;
using DuckyProfileSwitcher.Validators;
using DuckyProfileSwitcher.ViewModels;
using MahApps.Metro.Controls;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace DuckyProfileSwitcher.Views
{
    /// <summary>
    /// Interaction logic for RuleEditor.xaml
    /// </summary>
    public partial class RuleEditor : MetroWindow
    {
        private readonly RuleViewModel viewModel;
        private int errorCount = 0;

        public RuleEditor(Rule rule)
        {
            InitializeComponent();
            viewModel = new(rule);
            DataContext = viewModel;
        }

        private async void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ruleNameBinding.ValidationRules.Add(new RequiredValidation());
            profileNumberBinding.ValidationRules.Add(new RangeValidation { Minimum = 1, Maximum = 32 });
            ruleName.Focus();
            await Task.Delay(100);
            ruleName.SelectAll();
            await Task.Run(() =>
            {
                var windows = ActiveWindowListener.GetWindows()
                    //.Where(w => w.ProcessName != "DuckyProfileSwitcher")
                    .Distinct()
                    .OrderBy(w => w.ProcessName).ThenBy(w => w.Title)
                    .ToList();
                Dispatcher.Invoke(() => openWindowsList.ItemsSource = windows);
            });
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
            if (e.Action == ValidationErrorEventAction.Added)
            {
                ++errorCount;
            }
            else if (e.Action == ValidationErrorEventAction.Removed)
            {
                --errorCount;
            }
            save.IsEnabled = errorCount == 0;
        }

        private void ProfileNumberRadio_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            profileNumber.Focus();
            profileNumber.SelectAll();
        }

        private void ProfileNameRadio_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            profileSearchName.Focus();
            var comboBoxText = profileSearchName.FindChild<TextBox>("PART_EditableTextBox");
            comboBoxText?.SelectAll();
        }

        private void OpenWindowListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (openWindowsList.SelectedItem is WindowDescription windowDescription)
            {
                viewModel.AppName = windowDescription.ProcessName;
                viewModel.WindowTitle = windowDescription.Title;
            }
        }
    }
}
