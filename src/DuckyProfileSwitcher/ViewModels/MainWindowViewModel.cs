using DuckyProfileSwitcher.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DuckyProfileSwitcher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private const int PollingDelayMS = 1000;

        private bool disposedValue;
        private readonly CancellationTokenSource cancellationTokenSource = new();
        private readonly CancellationToken token;

        private bool isConnected;
        private string duckyPadDetails = "(disconnected)";
        private ObservableCollection<DuckyPadProfile> profiles = new();
        private DuckyPadProfile? selectedProfile;

        public MainWindowViewModel()
        {
            token = cancellationTokenSource.Token;
            PollConnected();
        }

        public bool IsConnected
        {
            get => isConnected;
            set
            {
                bool changed = isConnected != value;
                isConnected = value;
                OnPropertyChanged();
                if (changed)
                {
                    RefreshProfiles();
                    if (!isConnected)
                    {
                        OnDisconnected();
                    }
                }
            }
        }

        public ObservableCollection<DuckyPadProfile> Profiles
        {
            get => profiles;
            set
            {
                profiles = value;
                OnPropertyChanged();
            }
        }

        public DuckyPadProfile? SelectedProfile
        {
            get => selectedProfile;
            set
            {
                selectedProfile = value;
                OnPropertyChanged();
            }
        }

        public string DuckyPadDetails
        {
            get => duckyPadDetails;
            set
            {
                duckyPadDetails = value;
                OnPropertyChanged();
            }
        }

        public async void PreviousProfile()
        {
            try
            {
                await HID.DuckyPadCommunication.PreviousProfile(token);
                RefreshInfo();
            }
            catch (Exception) { }
        }

        public async void NextProfile()
        {
            try
            {
                await HID.DuckyPadCommunication.NextProfile(token);
                RefreshInfo();
            }
            catch (Exception) { }
        }

        public async void SetProfile(DuckyPadProfile profile)
        {
            if (IsConnected && profile != selectedProfile)
            {
                await HID.DuckyPadCommunication.GotoProfile(profile.Number, token);
            }
        }

        private async void PollConnected()
        {
            while (!token.IsCancellationRequested)
            {
                IsConnected = await HID.DuckyPadCommunication.IsConnected(cancellationTokenSource.Token);
                await Task.Delay(PollingDelayMS);
            }
        }

        private async void RefreshInfo()
        {
            if (IsConnected)
            {
                var info = await HID.DuckyPadCommunication.GetDuckyPadInfo(token);
                DuckyPadDetails = $"Firmware: {info.Major}.{info.Minor}.{info.Patch} | Model: {info.Model} | Serial: {info.SerialNumber:X}";
                SelectedProfile = profiles.FirstOrDefault(p => p.Number == info.Profile);
            }
            else
            {
                DuckyPadDetails = "(disconnected)";
            }
        }

        private async void RefreshProfiles()
        {
            if (IsConnected)
            {
                var r = await HID.DuckyPadCommunication.ListFiles(cancellationToken: token);
                var nextProfiles = r.Where(f => f.type == HID.DuckyPadFileType.Directory)
                    .Where(f => f.name.StartsWith("profile"))
                    .Select(f =>
                    {
                        string relevantName = f.name.Substring(7);
                        string[] numberAndText = relevantName.Split('_');
                        return new DuckyPadProfile(byte.Parse(numberAndText[0]), numberAndText[1]);
                    })
                    .OrderBy(p => p.Number);
                Profiles = new(nextProfiles);
                RefreshInfo();
            }
        }

        private void OnDisconnected()
        {
            DuckyPadDetails = "(disconnected)";
            Profiles.Clear();
            SelectedProfile = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource.Dispose();
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
