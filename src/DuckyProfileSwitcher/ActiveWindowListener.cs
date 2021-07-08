using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace DuckyProfileSwitcher
{
    public class ActiveWindowListener
    {
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        private readonly WINEVENTPROC callback;

        public ActiveWindowListener()
        {
            callback = new WINEVENTPROC(WinEventProc);
            IntPtr m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, callback, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        private delegate void WINEVENTPROC(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        public event EventHandler<ActiveWindowChangedEventArgs>? ActiveWindowChanged;

        public class ActiveWindowChangedEventArgs : EventArgs
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

            public override string ToString()
            {
                return $"{{Handle: {Handle}, Title: '{Title}', ProcessName: '{ProcessName}', ClassName: '{ClassName}'}}";
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

        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                string title = ManagedGetWindowText(foregroundWindow);
                string className = ManagedGetWindowClassName(foregroundWindow);
                _ = GetWindowThreadProcessId(foregroundWindow, out uint procId);
                var process = Process.GetProcessById((int)procId);
                string processName = process.ProcessName;
                ActiveWindowChanged?.Invoke(this, new ActiveWindowChangedEventArgs(foregroundWindow, title, processName, className));
            }
            catch (ArgumentException)
            {
            }
        }
    }
}
