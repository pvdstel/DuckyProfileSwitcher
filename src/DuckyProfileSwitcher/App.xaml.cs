using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DuckyProfileSwitcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string ShowSignalName = "7e29830a-272c-4354-85e7-1a85a0e6a48c";
        private const string FirstMutexName = "d0ab88ed-49ac-45f1-b695-1ba3c6d23b1c";

        private static readonly EventWaitHandle signal = new EventWaitHandle(false, EventResetMode.AutoReset, ShowSignalName);
        private static Mutex? isFirst;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ConfigurationManager.Load();

            isFirst = new Mutex(false, FirstMutexName, out bool createdNewMutex);
            if (!createdNewMutex || !isFirst.WaitOne(100))
            {
                signal.Set();
                Environment.Exit(0);
            }

            RunSignalWaitingThread();
        }

        private static void RunSignalWaitingThread()
        {
            Task.Factory.StartNew(() =>
            {
                while (!Current.Dispatcher.HasShutdownStarted)
                {
                    if (signal.WaitOne(5000))
                    {
                        Current.Dispatcher.Invoke(() =>
                        {
                            Current.MainWindow?.Show();
                        });
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
