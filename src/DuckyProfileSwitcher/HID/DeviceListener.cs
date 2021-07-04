using System;
using System.Runtime.InteropServices;

namespace DuckyProfileSwitcher.HID
{
    class DeviceListener : IDisposable
    {
        public const int WM_DEVICECHANGE = 0x0219;
        public const int DBT_DEVICEARRIVAL = 0x8000;         
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        public const int DBT_DEVNODES_CHANGED = 0x0007;

        private const int DBT_DEVTYP_DEVICEINTERFACE = 5;
        private static readonly Guid GUID_DEVINTERFACE_USB_DEVICE = new("A5DCBF10-6530-11D2-901F-00C04FB951ED");
        private static IntPtr notificationHandle;

        public void RegisterDeviceNotification(IntPtr windowHandle)
        {
            DevBroadcastDeviceinterface dbi = new()
            {
                DeviceType = DBT_DEVTYP_DEVICEINTERFACE,
                Reserved = 0,
                ClassGuid = GUID_DEVINTERFACE_USB_DEVICE,
                Name = 0
            };

            dbi.Size = Marshal.SizeOf(dbi);
            //IntPtr buffer = Marshal.AllocHGlobal(dbi.Size);
            //Marshal.StructureToPtr(dbi, buffer, true);
            notificationHandle = RegisterDeviceNotification(windowHandle, dbi, 0);

        }

        public void UnregisterDeviceNotification()
        {
            UnregisterDeviceNotification(notificationHandle);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, DevBroadcastDeviceinterface notificationFilter, int flags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);

        public void Dispose()
        {
            UnregisterDeviceNotification();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DevBroadcastDeviceinterface
        {
            internal int Size;
            internal int DeviceType;
            internal int Reserved;
            internal Guid ClassGuid;
            internal short Name;
        }
    }
}
