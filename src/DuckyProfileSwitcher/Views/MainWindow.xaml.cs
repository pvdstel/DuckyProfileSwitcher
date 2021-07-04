using DuckyProfileSwitcher.Models;
using DuckyProfileSwitcher.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace DuckyProfileSwitcher.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel viewModel = new();
        private readonly NotifyIcon notifyIcon = new();
        private readonly HID.DeviceListener deviceListener = new();
        private bool allowClose = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateNotifyIcon();
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(HwndHandler);
            deviceListener.RegisterDeviceNotification(source.Handle);
        }

        private IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == HID.DeviceListener.WM_DEVICECHANGE)
            {
                switch ((int)wparam)
                {
                    case HID.DeviceListener.DBT_DEVICEARRIVAL:
                    case HID.DeviceListener.DBT_DEVICEREMOVECOMPLETE:
                        viewModel.DeviceChange();
                        break;
                }
            }

            handled = false;
            return IntPtr.Zero;
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
            MenuItem runningToggle = new("Monitoring", (s, e) => viewModel.IsRunning = !viewModel.IsRunning);
            MenuItem goToProfile = new("Go to", (s, e) => viewModel.IsRunning = !viewModel.IsRunning);
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                new MenuItem("Open", (s, e) => Show()),
                runningToggle,
                goToProfile,
                new MenuItem("-"),
                new MenuItem("Exit", (s, e) => ExitApplication()) {  },
            });
            notifyIcon.ContextMenu.Popup += (s, e) =>
            {
                runningToggle.Checked = viewModel.IsRunning;
                goToProfile.MenuItems.Clear();
                foreach (DuckyPadProfile profile in viewModel.Profiles)
                {
                    goToProfile.MenuItems.Add(new MenuItem(profile.DisplayText, (s, e) => viewModel.SetProfile(profile))
                    {
                        RadioCheck = true,
                        Checked = viewModel.SelectedProfile == profile,
                    });
                }
            };

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
                deviceListener.Dispose();
                viewModel.Dispose();
                notifyIcon.Dispose();
            }
            else
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void ExitApplication_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }
    }
}
