using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

        private static readonly ConcurrentDictionary<int, Process> processCache = new();
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
            var oldEntries = processCache.Where(e => e.Value == null || e.Value.HasExited).ToArray();
            foreach (var entry in oldEntries)
            {
                if (processCache.TryRemove(entry.Key, out var _))
                {
                    Debug.WriteLine($"Expunged old cached process {entry.Key} ({entry.Value?.ProcessName ?? "unknown"})");
                    entry.Value?.Dispose();
                }
            }

            try
            {
                _ = GetWindowThreadProcessId(hWnd, out uint procId);
                if (!processCache.TryGetValue((int)procId, out var cachedProcess))
                {
                    cachedProcess = Process.GetProcessById((int)procId);
                    try
                    {
                        // If getting info about the program does not throw, add it to the cache
                        if (!cachedProcess.HasExited)
                        {
                            processCache[(int)procId] = cachedProcess;
                            Debug.WriteLine($"Added process {procId} ({cachedProcess.ProcessName}) to cache");
                        }
                    }
                    catch (Win32Exception) { }
                }
                else
                {
                    Debug.WriteLine($"Found process {cachedProcess.ProcessName} in cache");
                }
                string processName = cachedProcess.ProcessName ?? string.Empty;
                return processName;
            }
            catch (ArgumentException)
            {
                return string.Empty;
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
