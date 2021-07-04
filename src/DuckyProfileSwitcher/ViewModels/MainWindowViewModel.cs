using DuckyProfileSwitcher.Models;
using DuckyProfileSwitcher.Utilities;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace DuckyProfileSwitcher.ViewModels
{
    public class MainWindowViewModel : Notifyable, IDisposable
    {
        private const int DeviceChangeDelay = 200;
        private bool disposedValue;

        private string duckyPadDetails = "(disconnected)";

        public MainWindowViewModel()
        {
            PreviousProfile = new RelayCommand(() => _ = DuckyPadManager.Instance.PreviousProfile(), () => CanSwitchProfile);
            NextProfile = new RelayCommand(() => _ = DuckyPadManager.Instance.NextProfile(), () => CanSwitchProfile);

            DuckyPadManager.Instance.InfoChanged += DuckyPadManagerChange;
            DuckyPadManager.Instance.IsBusyChanged += DuckyPadManagerChange;
            DuckyPadManager.Instance.IsConnectedChanged += DuckyPadManagerChange;
            DuckyPadManager.Instance.IsRunningChanged += DuckyPadManagerChange;
            DuckyPadManager.Instance.ProfilesChanged += DuckyPadManagerChange;
            DuckyPadManager.Instance.SelectedProfileChanged += DuckyPadManagerChange;
        }

        public event EventHandler? Timeout
        {
            add { DuckyPadManager.Instance.Timeout += value; } 
            remove { DuckyPadManager.Instance.Timeout -= value; } 
        }

        public bool IsBusy
        {
            get => DuckyPadManager.Instance.IsBusy;
        }

        public bool IsConnected
        {
            get => DuckyPadManager.Instance.IsConnected;
        }

        public bool CanSwitchProfile => !IsBusy && IsConnected;

        public bool IsRunning
        {
            get => DuckyPadManager.Instance.IsRunning;
            set => DuckyPadManager.Instance.IsRunning = value;
        }

        public ImmutableList<DuckyPadProfile>? Profiles
        {
            get => DuckyPadManager.Instance.Profiles;
        }

        public DuckyPadProfile? SelectedProfile
        {
            get => DuckyPadManager.Instance.SelectedProfile;
            set
            {
                if (value != null)
                {
                    _ = DuckyPadManager.Instance.SetProfile(value);
                }
                OnPropertyChanged();
            }
        }

        public async void SetProfile(DuckyPadProfile profile)
        {
            await DuckyPadManager.Instance.SetProfile(profile);
        }

        public string DuckyPadDetails
        {
            get => duckyPadDetails;
            private set
            {
                duckyPadDetails = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand PreviousProfile { get; }

        public RelayCommand NextProfile { get; }

        public async void DeviceChange()
        {
            await Task.Delay(DeviceChangeDelay);
            await DuckyPadManager.Instance.RefreshConnected();
        }

        private void DuckyPadManagerChange(object sender, EventArgs e)
        {
            PreviousProfile.RaiseCanExecuteChanged();
            NextProfile.RaiseCanExecuteChanged();
            HID.DuckyPadInfo? info = DuckyPadManager.Instance.Info;
            DuckyPadDetails = info != null
                ? $"Firmware: {info.Major}.{info.Minor}.{info.Patch} | Model: {info.Model} | Serial: {info.SerialNumber:X}"
                : "(disconnected)";
            System.Diagnostics.Debug.WriteLine("DuckyPadManager updated.");
            OnPropertyChanged(string.Empty);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DuckyPadManager.Instance.InfoChanged -= DuckyPadManagerChange;
                    DuckyPadManager.Instance.IsBusyChanged -= DuckyPadManagerChange;
                    DuckyPadManager.Instance.IsConnectedChanged -= DuckyPadManagerChange;
                    DuckyPadManager.Instance.IsRunningChanged -= DuckyPadManagerChange;
                    DuckyPadManager.Instance.ProfilesChanged -= DuckyPadManagerChange;
                    DuckyPadManager.Instance.SelectedProfileChanged -= DuckyPadManagerChange;
                }

                disposedValue = true;
            }
        }

        ~MainWindowViewModel()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
