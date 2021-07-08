using DuckyProfileSwitcher.Models;
using System.Collections.Generic;

namespace DuckyProfileSwitcher
{
    public class Config
    {
        public bool ShowTrayIcon { get; set; } = true;

        public bool ConfirmExitApplication { get; set; } = true;

        public List<Rule> Rules { get; set; } = new();
    }
}
