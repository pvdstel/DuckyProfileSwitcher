﻿using System.Windows;

namespace DuckyProfileSwitcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ConfigurationManager.Load();
        }
    }
}
