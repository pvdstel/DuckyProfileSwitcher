using DuckyProfileSwitcher.Models;
using DuckyProfileSwitcher.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace DuckyProfileSwitcher.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel viewModel = new();
        private readonly NotifyIcon notifyIcon;
        private bool allowClose = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;

            notifyIcon = new();
            CreateNotifyIcon();
        }

        private void CreateNotifyIcon()
        {
            using Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/small.ico")).Stream;
            notifyIcon.Text = "duckyPad Profile Switcher";
            notifyIcon.Icon = new System.Drawing.Icon(iconStream);
            notifyIcon.Visible = true;
            notifyIcon.Click += (sender, e) =>
            {
                Show();
            };
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                new MenuItem("Open", (s, e) => Show()),
                new MenuItem("Exit", (s, e) => ExitApplication()),
            });

            viewModel.Timeout += OnViewModelTimeout;
        }

        private void OnViewModelTimeout(object sender, EventArgs e)
        {
            notifyIcon.ShowBalloonTip(5000, "duckyPad Profile Switcher", "A duckyPad operation could not complete in time.", ToolTipIcon.Warning);
        }

        private void ExitApplication()
        {
            allowClose = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (allowClose)
            {
                viewModel.Dispose();
                notifyIcon.Dispose();
            }
            else
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void PreviousProfileButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.PreviousProfile();
        }

        private void NextProfileButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.NextProfile();
        }

        private void ExitApplication_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var profile = e.AddedItems.OfType<DuckyPadProfile>().FirstOrDefault();
            if (profile != null)
            {
                viewModel.SetProfile(profile);
            }
        }
    }
}
