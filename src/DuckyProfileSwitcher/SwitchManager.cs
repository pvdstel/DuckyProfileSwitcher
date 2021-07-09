using DuckyProfileSwitcher.Models;
using System;
using System.Diagnostics;
using System.Linq;

namespace DuckyProfileSwitcher
{
    public class SwitchManager
    {
        private readonly ActiveWindowListener windowListener;
        private bool isRunning;
        private ActiveWindowListener.ActiveWindowChangedEventArgs? activeWindow;

        private SwitchManager()
        {
            windowListener = new ActiveWindowListener();
            windowListener.ActiveWindowChanged += WindowListener_ActiveWindowChanged;

            if (Environment.GetCommandLineArgs().Any(a => string.Compare("run", a, true) == 0))
            {
                IsRunning = true;
            }
        }

        public static SwitchManager Instance { get; } = new SwitchManager();

        public bool IsRunning
        {
            get => isRunning;
            set
            {
                isRunning = value;
                IsRunningChanged?.Invoke(null, new EventArgs());
            }
        }

        public event EventHandler? IsRunningChanged;

        private async void UpdateProfile()
        {
            if (!IsRunning || activeWindow == null)
            {
                return;
            }

            Rule[]? rules = ConfigurationManager.Configuration.Rules.ToArray();
            if (rules == null)
            {
                return;
            }

            Rule? selectedRule = rules.Where(r => r.Enabled).FirstOrDefault(r =>
            {
                if (!string.IsNullOrEmpty(r.AppName) && string.Compare(r.AppName, activeWindow.ProcessName, true) != 0)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(r.WindowTitle) && !activeWindow.Title.ToUpper().Contains(r.WindowTitle!.ToUpper()))
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(r.WindowClass) && r.WindowClass != activeWindow.ClassName)
                {
                    return false;
                }

                return true;
            });

            if (selectedRule == null)
            {
                return;
            }

            switch (selectedRule.SwitchAction) {
                case SwitchAction.Sleep:
                    break;
                case SwitchAction.SwitchToProfileNumber:
                case SwitchAction.SwitchToProfileName:
                    DuckyPadProfile? profileFromLabel = selectedRule.FindProfile();
                    if (profileFromLabel != null)
                    {
                        await DuckyPadManager.Instance.SetProfile(profileFromLabel);
                    }
                    break;
            }
        }

        private void WindowListener_ActiveWindowChanged(object sender, ActiveWindowListener.ActiveWindowChangedEventArgs e)
        {
            Debug.WriteLine(e);
            activeWindow = e;
            UpdateProfile();
        }
    }
}
