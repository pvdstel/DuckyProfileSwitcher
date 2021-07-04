using DuckyProfileSwitcher.Models;
using DuckyProfileSwitcher.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DuckyProfileSwitcher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private const int PollingDelayMS = 10000;
        private const int ActionCancellationTimeMS = 10000;
        private const int InfoRetrievalCount = 1;
        private const int ProfileRetrievalCount = 6;
        private const int DeviceChangeDelay = 200;
        private bool disposedValue;
        private readonly CancellationTokenSource cancellationTokenSource = new();
        private readonly CancellationToken viewModelLifetimeToken;

        private bool isBusy;
        private bool isConnected;
        private bool isRunning;
        private string duckyPadDetails = "(disconnected)";
        private ObservableCollection<DuckyPadProfile> profiles = new();
        private DuckyPadProfile? selectedProfile;

        public MainWindowViewModel()
        {
            viewModelLifetimeToken = cancellationTokenSource.Token;
            PreviousProfile = new RelayCommand(() => PreviousProfileInternal(), () => CanSwitchProfile);
            NextProfile = new RelayCommand(() => NextProfileInternal(), () => CanSwitchProfile);

            if (Environment.GetCommandLineArgs().Any(a => string.Compare("run", a, true) == 0))
            {
                isRunning = true;
            }
            PollConnected();
        }

        public event EventHandler? Timeout;

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                OnPropertyChanged();
                UpdateExecutability();
            }
        }

        public bool IsConnected
        {
            get => isConnected;
            set
            {
                isConnected = value;
                OnPropertyChanged();
            }
        }

        public bool CanSwitchProfile => !IsBusy && IsConnected;

        public bool IsRunning
        {
            get => isRunning;
            set
            {
                isRunning = value;
                OnPropertyChanged();
                UpdateExecutability();
            }
        }

        public ObservableCollection<DuckyPadProfile> Profiles
        {
            get => profiles;
            private set
            {
                profiles = value;
                OnPropertyChanged();
            }
        }

        public DuckyPadProfile? SelectedProfile
        {
            get => selectedProfile;
            private set
            {
                selectedProfile = value;
                OnPropertyChanged();
            }
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

        public async void SetProfile(DuckyPadProfile profile)
        {
            using var ltct = new LinkedTimeoutCancellationToken(viewModelLifetimeToken, ActionCancellationTimeMS);
            if (!IsConnected || profile == selectedProfile)
            {
                return;
            }

            IsBusy = true;
            await RunCatchDuckyPadException(async () =>
            {
                await HID.DuckyPadCommunication.GotoProfile(profile.Number, ltct.Token);
                await RefreshInfo();
            });
            IsBusy = false;
        }

        public async void DeviceChange()
        {
            await Task.Delay(DeviceChangeDelay);
            await RefreshConnected();
        }

        private void UpdateExecutability()
        {
            OnPropertyChanged(nameof(CanSwitchProfile));
            PreviousProfile.RaiseCanExecuteChanged();
            NextProfile.RaiseCanExecuteChanged();
            System.Diagnostics.Debug.WriteLine("Updated executability.");
        }

        private async Task RefreshConnected()
        {
            await Task.Delay(100);
            bool nextIsConnected = await HID.DuckyPadCommunication.IsConnected(cancellationTokenSource.Token);
            if (nextIsConnected && !isConnected)
            {
                IsConnected = true;
                await RefreshProfiles();
            }
            else if (isConnected && !nextIsConnected)
            {
                IsConnected = false;
                OnDisconnected();
            }
        }

        private async Task RefreshInfo()
        {
            if (IsConnected)
            {
                using var ltct = new LinkedTimeoutCancellationToken(viewModelLifetimeToken, ActionCancellationTimeMS);
                await RunCatchDuckyPadException(async () =>
                {
                    var info = await HID.DuckyPadCommunication.GetDuckyPadInfo(ltct.Token);
                    DuckyPadDetails = $"Firmware: {info.Major}.{info.Minor}.{info.Patch} | Model: {info.Model} | Serial: {info.SerialNumber:X}";
                    SelectedProfile = profiles.FirstOrDefault(p => p.Number == info.Profile);
                });
            }
            else
            {
                DuckyPadDetails = "(disconnected)";
            }
        }

        private async void PreviousProfileInternal()
        {
            using var ltct = new LinkedTimeoutCancellationToken(viewModelLifetimeToken, ActionCancellationTimeMS);
            if (!IsConnected)
            {
                return;
            }

            IsBusy = true;
            await RunCatchDuckyPadException(async () =>
            {
                await HID.DuckyPadCommunication.PreviousProfile(ltct.Token);
                await RefreshInfo();
            });
            IsBusy = false;
        }

        private async void NextProfileInternal()
        {
            using var ltct = new LinkedTimeoutCancellationToken(viewModelLifetimeToken, ActionCancellationTimeMS);
            if (!IsConnected)
            {
                return;
            }

            IsBusy = true;
            await RunCatchDuckyPadException(async () =>
            {
                await HID.DuckyPadCommunication.NextProfile(ltct.Token);
                await RefreshInfo();
            });
            IsBusy = false;
        }

        private async Task RefreshProfiles()
        {
            if (IsConnected)
            {
                using var ltct = new LinkedTimeoutCancellationToken(viewModelLifetimeToken, ActionCancellationTimeMS);
                await RunCatchDuckyPadException(async () =>
                {
                    var r = await HID.DuckyPadCommunication.ListFiles(ltct.Token);
                    IsBusy = false;
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
                });
            }
        }

        private async Task RunCatchDuckyPadException(Func<Task> task)
        {
            try
            {
                await task();
            }
            catch (HID.DuckyPadException dpex)
            {
                System.Diagnostics.Debug.WriteLine(dpex);
            }
            catch (OperationCanceledException)
            {
                Timeout?.Invoke(this, new EventArgs());
            }
        }

        private async void PollConnected()
        {
            int i = 0;
            while (!viewModelLifetimeToken.IsCancellationRequested)
            {
                await RefreshConnected();
                ++i;
                if (i >= ProfileRetrievalCount)
                {
                    i = 0;
                    await RefreshProfiles();
                }
                else if (i % InfoRetrievalCount == 0)
                {
                    await RefreshInfo();
                }
                await Task.Delay(PollingDelayMS);
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
