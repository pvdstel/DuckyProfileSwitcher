using DuckyProfileSwitcher.Models;
using System.Collections.Generic;

namespace DuckyProfileSwitcher
{
    public class Config
    {
        /// <summary>
        /// Controls whether a tray icon is shown.
        /// </summary>
        public bool ShowTrayIcon { get; set; } = true;

        /// <summary>
        /// Controls whether the main window is shown on startup.
        /// </summary>
        public bool ShowOnStartup { get; set; } = true;

        /// <summary>
        /// Controls whether application monitors the active window on start-up.
        /// </summary>
        public bool MonitorOnStartup { get; set; } = false;

        /// <summary>
        /// Controls whether you will be prompted on exiting the application.
        /// </summary>
        public bool ConfirmExitApplication { get; set; } = true;

        /// <summary>
        /// The list of switching rules.
        /// </summary>
        public List<Rule> Rules { get; set; } = new();
    }
}
