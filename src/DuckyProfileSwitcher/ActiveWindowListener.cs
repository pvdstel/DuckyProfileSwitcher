using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        private const int PollingDelayMS = 2000;

        // This dictionary keeps track of windows and process we've already encountered, so that querying processes
        // all the time is not necessary. The combination of hWnd and pid is very, very unlikely to be reused.
        private static readonly Dictionary<(IntPtr hWnd, int pid), string> processNameCache = new();
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
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public event EventHandler<ActiveWindowChangedEventArgs>? ActiveWindowChanged;

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

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        public static ImmutableList<WindowDescription> GetWindows()
        {
            List<WindowDescription> result = new();
            EnumWindows((IntPtr hWnd, IntPtr _) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    result.Add(new WindowDescription(ManagedGetWindowText(hWnd), GetWindowProcessName(hWnd)));
                }
                return true;
            }, IntPtr.Zero);
            return result.ToImmutableList();
        }

        private static string ManagedGetWindowText(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            StringBuilder text = new(length);
            _ = GetWindowText(hWnd, text, length + 1);
            return text.ToString();
        }

        private static string ManagedGetWindowClassName(IntPtr hWnd)
        {
            const int length = 40;
            StringBuilder text = new(length);
            _ = GetClassName(hWnd, text, length + 1);
            return text.ToString();
        }

        private static string GetWindowProcessName(IntPtr hWnd)
        {
            _ = GetWindowThreadProcessId(hWnd, out uint procId);
            return GetWindowProcessName(hWnd, (int)procId);
        }

        private static string GetWindowProcessName(IntPtr hWnd, int processId)
        {
            var key = (hWnd, processId);
            if (processNameCache.TryGetValue(key, out string value))
            {
                return value;
            }
            else
            {
                try
                {
                    using Process? process = Process.GetProcessById(processId);
                    string name = process?.ProcessName ?? string.Empty;
                    processNameCache[key] = name;
                    return name;
                }
                catch (ArgumentException)
                {
                    return string.Empty;
                }
            }
        }

        private void Refresh()
        {
            IntPtr foregroundWindow = GetForegroundWindow();
            string title = ManagedGetWindowText(foregroundWindow);
            string className = ManagedGetWindowClassName(foregroundWindow);
            string processName = GetWindowProcessName(foregroundWindow);
            ActiveWindowChangedEventArgs window = new(foregroundWindow, title, processName, className);
            if (window != activeWindow)
            {
                activeWindow = window;
                ActiveWindowChanged?.Invoke(this, window);
            }
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            Refresh();
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
