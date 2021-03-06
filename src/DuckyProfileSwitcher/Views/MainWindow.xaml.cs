using DuckyProfileSwitcher.Models;
using DuckyProfileSwitcher.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace DuckyProfileSwitcher.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        private readonly MainWindowViewModel viewModel = new();
        private readonly NotifyIcon notifyIcon = new();
        private readonly HID.DeviceListener deviceListener = new();
        private readonly string versionString;
        private bool allowClose = false;
        private CancellationTokenSource? closeDialogCancellation;

        public MainWindow()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version appVersion = assembly.GetName().Version;
            if (assembly.GetCustomAttributes(false).OfType<System.Diagnostics.DebuggableAttribute>().Any(d => d.IsJITTrackingEnabled))
            {
                versionString = appVersion.ToString(3) + " (debug)";
            }
            else
            {
                versionString = appVersion.ToString(3);
            }

            InitializeComponent();
            DataContext = viewModel;

            if (!ConfigurationManager.Configuration.ShowOnStartup || Environment.GetCommandLineArgs().Any(a => string.Compare("hidden", a, true) == 0))
            {
                Visibility = Visibility.Hidden;
            }

            ConfigurationManager.ConfigurationChanged += ConfigurationManager_ConfigurationChanged;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            CreateNotifyIcon();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(HwndHandler);
            deviceListener.RegisterDeviceNotification(source.Handle);
            Title += ' ' + versionString;
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
            notifyIcon.Visible = ConfigurationManager.Configuration.ShowTrayIcon;
            notifyIcon.Click += (sender, e) =>
            {
                if (e is MouseEventArgs mouseEventArgs && mouseEventArgs.Button == MouseButtons.Left)
                {
                    Show();
                }
            };
            MenuItem runningToggle = new("Monitoring", (s, e) => viewModel.IsRunning = !viewModel.IsRunning);
            MenuItem goToProfile = new("Go to", (s, e) => viewModel.IsRunning = !viewModel.IsRunning);
            MenuItem sleep = new("Sleep", (s, e) => viewModel.Sleep.Execute(new object()));
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                new MenuItem("Open", (s, e) => Show()),
                runningToggle,
                goToProfile,
                sleep,
                new MenuItem("-"),
                new MenuItem("Exit", (s, e) => ExitApplication()),
            });
            notifyIcon.ContextMenu.Popup += (s, e) =>
            {
                runningToggle.Checked = viewModel.IsRunning;
                goToProfile.MenuItems.Clear();
                goToProfile.Enabled = viewModel.CanSwitchProfile;
                sleep.Enabled = viewModel.Sleep.CanExecute(new object());
                if (viewModel.Profiles != null)
                {
                    goToProfile.Visible = true;
                    foreach (DuckyPadProfile profile in viewModel.Profiles)
                    {
                        goToProfile.MenuItems.Add(new MenuItem(profile.DisplayText, (s, e) => viewModel.SetProfile(profile))
                        {
                            RadioCheck = true,
                            Checked = viewModel.SelectedProfile == profile,
                        });
                    }
                }
                else
                {
                    goToProfile.Visible = false;
                }
            };

            viewModel.Timeout += OnViewModelTimeout;
        }

        private void OnViewModelTimeout(object sender, EventArgs e)
        {
            notifyIcon.ShowBalloonTip(5000, "duckyPad Profile Switcher", "A duckyPad operation could not complete in time.", ToolTipIcon.Warning);
            DuckyPadManager.InvalidateDeviceCache();
        }

        private async void ExitApplication()
        {
            void doExit()
            {
                allowClose = true;
                Close();
            }

            if (closeDialogCancellation != null)
            {
                return;
            }
            if (ConfigurationManager.Configuration.ConfirmExitApplication)
            {
                closeDialogCancellation = new CancellationTokenSource();
                var result = await this.ShowMessageAsync("Exit duckyPad Profile Switcher?", string.Empty, MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                {
                    AffirmativeButtonText = "Yes",
                    AnimateHide = false,
                    AnimateShow = false,
                    CancellationToken = closeDialogCancellation.Token,
                    DefaultButtonFocus = MessageDialogResult.Affirmative,
                    DialogResultOnCancel = MessageDialogResult.Canceled,
                    DialogTitleFontSize = 18,
                    NegativeButtonText = "No",
                });
                closeDialogCancellation?.Dispose();
                closeDialogCancellation = null;
                if (result == MessageDialogResult.Affirmative)
                {
                    doExit();
                }
            }
            else
            {
                doExit();
            }
        }

        private void ConfigurationManager_ConfigurationChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                notifyIcon.Visible = ConfigurationManager.Configuration.ShowTrayIcon;
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (allowClose)
            {
                ConfigurationManager.ConfigurationChanged -= ConfigurationManager_ConfigurationChanged;
                deviceListener.Dispose();
                viewModel.Dispose();
                notifyIcon.Dispose();
            }
            else
            {
                e.Cancel = true;
                closeDialogCancellation?.Cancel();
                closeDialogCancellation?.Dispose();
                closeDialogCancellation = null;
                Hide();
            }
        }

        private void ExitApplication_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }

        private void OpenConfig_Click(object sender, RoutedEventArgs e)
        {
            string file = ConfigurationManager.ConfigurationFile;

            if (!File.Exists(file))
            {
                this.ShowMessageAsync(
                    "No configuration file found",
                    $"The configuration file does not exist. A configuration file will automatically be created when settings change.\n\nFile: {file}",
                    settings: new MetroDialogSettings
                    {
                        AnimateHide = false,
                        AnimateShow = false,
                    });
                return;
            }

            try
            {
                System.Diagnostics.Process.Start(file);
            }
            catch (Win32Exception ex)
            {
                this.ShowMessageAsync(
                    "System error",
                    $"A system error occurred while opening the configuration file.\n\nFile: {file}\nMessage: {ex.Message}",
                    settings: new MetroDialogSettings
                    {
                        AnimateHide = false,
                        AnimateShow = false,
                    });
            }
            catch (Exception ex)
            {
                this.ShowMessageAsync(
                    "Unknown error",
                    $"An unknown error occurred while opening the configuration file.\n\nFile: {file}\nMessage: {ex.Message}\nType: {ex.GetType().FullName}",
                    settings: new MetroDialogSettings
                    {
                        AnimateHide = false,
                        AnimateShow = false,
                    });
            }
        }
    }
}
