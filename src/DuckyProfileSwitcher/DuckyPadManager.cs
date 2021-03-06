using Device.Net;
using DuckyProfileSwitcher.HID;
using DuckyProfileSwitcher.Models;
using DuckyProfileSwitcher.Utilities;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DuckyProfileSwitcher
{
    public class DuckyPadManager
    {
        private const int PollingDelayMS = 10000;
        private const int ActionCancellationTimeMS = 10000;
        private const int InfoRetrievalCount = 1;
        private const int ProfileRetrievalCount = 6;

        private readonly CancellationTokenSource cancellationTokenSource = new();
        private readonly CancellationToken lifetimeToken;

        private bool isConnected;
        private bool isBusy;
        private DuckyPadInfo? info;
        private ConnectedDeviceDefinition? deviceDefinition;
        private DuckyPadProfile? selectedProfile;
        private ImmutableList<DuckyPadProfile>? profiles;

        private DuckyPadManager()
        {
            PollConnected();
        }

        public event EventHandler? Timeout;

        public event EventHandler? InfoChanged;

        public event EventHandler? DeviceDefinitionChanged;

        public event EventHandler? ProfilesChanged;

        public event EventHandler? SelectedProfileChanged;

        public event EventHandler? IsBusyChanged;

        public event EventHandler? IsConnectedChanged;

        public static DuckyPadManager Instance { get; } = new DuckyPadManager();

        public bool IsBusy
        {
            get => isBusy;
            private set
            {
                isBusy = value;
                IsBusyChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsConnected
        {
            get => isConnected;
            private set
            {
                isConnected = value;
                IsConnectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public DuckyPadInfo? Info
        {
            get => info;
            set
            {
                info = value;
                InfoChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public ConnectedDeviceDefinition? DeviceDefinition
        {
            get => deviceDefinition;
            set
            {
                deviceDefinition = value;
                DeviceDefinitionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public DuckyPadProfile? SelectedProfile
        {
            get => selectedProfile;
            set
            {
                selectedProfile = value;
                SelectedProfileChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public ImmutableList<DuckyPadProfile>? Profiles
        {
            get => profiles;
            set
            {
                profiles = value;
                ProfilesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public static void InvalidateDeviceCache()
        {
            DuckyPadCommunication.InvalidateDeviceCache();
        }

        public async Task RefreshConnected()
        {
            bool nextIsConnected = await DuckyPadCommunication.IsConnected(cancellationTokenSource.Token);
            if (nextIsConnected && !IsConnected)
            {
                IsConnected = true;
                await RefreshProfiles();
            }
            else if (IsConnected && !nextIsConnected)
            {
                IsConnected = false;
                Info = null;
                Profiles = null;
                SelectedProfile = null;
            }
        }

        public async Task SetProfile(DuckyPadProfile profile)
        {
            using var ltct = new LinkedTimeoutCancellationToken(lifetimeToken, ActionCancellationTimeMS);
            if (!IsConnected || profile == SelectedProfile)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Setting the profile to {profile}.");

            IsBusy = true;
            await RunCatchDuckyPadException(async () =>
            {
                await DuckyPadCommunication.GotoProfile(profile.Number, ltct.Token);
                await RefreshInfo();
            });
            IsBusy = false;
        }

        public async Task PreviousProfile()
        {
            using var ltct = new LinkedTimeoutCancellationToken(lifetimeToken, ActionCancellationTimeMS);
            if (!IsConnected)
            {
                return;
            }

            IsBusy = true;
            await RunCatchDuckyPadException(async () =>
            {
                await DuckyPadCommunication.PreviousProfile(ltct.Token);
                await RefreshInfo();
            });
            IsBusy = false;
        }

        public async Task NextProfile()
        {
            using var ltct = new LinkedTimeoutCancellationToken(lifetimeToken, ActionCancellationTimeMS);
            if (!IsConnected)
            {
                return;
            }

            IsBusy = true;
            await RunCatchDuckyPadException(async () =>
            {
                await DuckyPadCommunication.NextProfile(ltct.Token);
                await RefreshInfo();
            });
            IsBusy = false;
        }

        public async Task Sleep()
        {
            using var ltct = new LinkedTimeoutCancellationToken(lifetimeToken, ActionCancellationTimeMS);
            if (!IsConnected)
            {
                return;
            }

            IsBusy = true;
            await RunCatchDuckyPadException(async () =>
            {
                await DuckyPadCommunication.Sleep(ltct.Token);
                await RefreshInfo();
            });
            IsBusy = false;
        }

        private async Task RefreshInfo()
        {
            if (IsConnected)
            {
                using var ltct = new LinkedTimeoutCancellationToken(lifetimeToken, ActionCancellationTimeMS);
                await RunCatchDuckyPadException(async () =>
                {
                    var info = await DuckyPadCommunication.GetDuckyPadInfo(ltct.Token);
                    Info = info;
                    var definition = await DuckyPadCommunication.GetDeviceInfo(ltct.Token);
                    DeviceDefinition = definition;
                    SelectedProfile = Profiles.FirstOrDefault(p => p.Number == info.Profile);
                });
            }
        }

        private async Task RefreshProfiles()
        {
            if (IsConnected)
            {
                using var ltct = new LinkedTimeoutCancellationToken(lifetimeToken, ActionCancellationTimeMS);
                await RunCatchDuckyPadException(async () =>
                {
                    var r = await DuckyPadCommunication.ListFiles(ltct.Token);
                    IsBusy = false;
                    var nextProfiles = r.Where(f => f.type == DuckyPadFileType.Directory && f.name.StartsWith("profile"))
                        .Select(f =>
                        {
                            string relevantName = f.name.Substring(7);
                            string[] numberAndText = relevantName.Split('_');
                            return new DuckyPadProfile(byte.Parse(numberAndText[0]), numberAndText[1]);
                        })
                        .OrderBy(p => p.Number);

                    if (Profiles == null || !nextProfiles.SequenceEqual(Profiles))
                    {
                        Profiles = nextProfiles.ToImmutableList();
                    }
                });
            }
        }

        private async Task RunCatchDuckyPadException(Func<Task> task)
        {
            try
            {
                await task();
            }
            catch (DuckyPadException dpex)
            {
                System.Diagnostics.Debug.WriteLine(dpex);
            }
            catch (OperationCanceledException)
            {
                Timeout?.Invoke(this, EventArgs.Empty);
            }
        }

        private async void PollConnected()
        {
            int i = 0;
            while (!lifetimeToken.IsCancellationRequested)
            {
                await RefreshConnected();
                ++i;
                if (i >= ProfileRetrievalCount)
                {
                    i = 0;
                    await RefreshProfiles();
                }
                if (i % InfoRetrievalCount == 0)
                {
                    await RefreshInfo();
                }
                await Task.Delay(PollingDelayMS);
            }
        }
    }
}
