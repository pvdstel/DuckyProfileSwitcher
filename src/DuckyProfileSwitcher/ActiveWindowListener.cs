using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuckyProfileSwitcher
{
    public class ActiveWindowListener
    {
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;
        private const int PollingDelayMS = 1000;

        private readonly WINEVENTPROC callback;
        private readonly CancellationToken lifetimeToken;
        private ActiveWindowChangedEventArgs? activeWindow;

        public ActiveWindowListener()
        {
            callback = new WINEVENTPROC(WinEventProc);
            IntPtr m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, callback, 0, 0, WINEVENT_OUTOFCONTEXT);
            PollConnected();
        }

        private delegate void WINEVENTPROC(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        public event EventHandler<ActiveWindowChangedEventArgs>? ActiveWindowChanged;

        public class ActiveWindowChangedEventArgs : EventArgs, IEquatable<ActiveWindowChangedEventArgs?>
        {
            public ActiveWindowChangedEventArgs(IntPtr handle, string title, string processName, string className)
            {
                Handle = handle;
                Title = title;
                ProcessName = processName;
                ClassName = className;
            }

            public IntPtr Handle { get; }
            public string Title { get; }
            public string ProcessName { get; }
            public string ClassName { get; }

            public override bool Equals(object? obj)
            {
                return Equals(obj as ActiveWindowChangedEventArgs);
            }

            public bool Equals(ActiveWindowChangedEventArgs? other)
            {
                return other != null &&
                       EqualityComparer<IntPtr>.Default.Equals(Handle, other.Handle) &&
                       Title == other.Title &&
                       ProcessName == other.ProcessName &&
                       ClassName == other.ClassName;
            }

            public override int GetHashCode()
            {
                int hashCode = -1308394375;
                hashCode = hashCode * -1521134295 + Handle.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProcessName);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ClassName);
                return hashCode;
            }

            public override string ToString()
            {
                return $"{{Handle: {Handle}, Title: '{Title}', ProcessName: '{ProcessName}', ClassName: '{ClassName}'}}";
            }

            public static bool operator ==(ActiveWindowChangedEventArgs? left, ActiveWindowChangedEventArgs? right)
            {
                return EqualityComparer<ActiveWindowChangedEventArgs>.Default.Equals(left!, right!);
            }

            public static bool operator !=(ActiveWindowChangedEventArgs? left, ActiveWindowChangedEventArgs? right)
            {
                return !(left == right);
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WINEVENTPROC lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private static string ManagedGetWindowText(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            StringBuilder text = new(length);
            _ = GetWindowText(hWnd, text, length + 1);
            return text.ToString();
        }

        private static string ManagedGetWindowClassName(IntPtr hWnd)
        {
            int length = 40;
            StringBuilder text = new(length);
            _ = GetClassName(hWnd, text, length + 1);
            return text.ToString();
        }

        private void Refresh()
        {
            IntPtr foregroundWindow = GetForegroundWindow();
            string title = ManagedGetWindowText(foregroundWindow);
            string className = ManagedGetWindowClassName(foregroundWindow);
            _ = GetWindowThreadProcessId(foregroundWindow, out uint procId);
            var process = Process.GetProcessById((int)procId);
            string processName = process.ProcessName;
            ActiveWindowChangedEventArgs window = new(foregroundWindow, title, processName, className);
            if (window != activeWindow)
            {
                activeWindow = window;
                ActiveWindowChanged?.Invoke(this, window);
            }
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            try
            {
                Refresh();
            }
            catch (ArgumentException)
            {
            }
        }

        private async void PollConnected()
        {
            while (!lifetimeToken.IsCancellationRequested)
            {
                Refresh();
                await Task.Delay(PollingDelayMS);
            }
        }
    }
}
