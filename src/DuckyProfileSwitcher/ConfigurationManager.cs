using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DuckyProfileSwitcher
{
    internal class ConfigurationManager
    {
        private const int SettingsWatcherTimeout = 100;

        public static readonly string ConfigurationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DuckyProfileSwitcher");
        public static readonly string ConfigurationFile = Path.Combine(ConfigurationDirectory, "config.json");

        public static readonly Config DefaultConfiguration = new();

        private static readonly SemaphoreSlim storageLock = new(1, 1);
        private static readonly JsonSerializerOptions jsonSerializerOptions;
        private static readonly FileSystemWatcher settingsWatcher;
        private static CancellationTokenSource? reloadEventCancellation;

        static ConfigurationManager()
        {
            if (!Directory.Exists(ConfigurationDirectory))
            {
                Directory.CreateDirectory(ConfigurationDirectory);
            }

            jsonSerializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
            };

            settingsWatcher = new FileSystemWatcher(ConfigurationDirectory)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.json"
            };
            settingsWatcher.Changed += SettingsFileChanged;
            settingsWatcher.EnableRaisingEvents = true;
        }

        public static event EventHandler? ConfigurationChanged;

        public static Config Configuration { get; private set; } = DefaultConfiguration;


        public static void Save()
        {
            storageLock.Wait();
            string contents = JsonSerializer.Serialize(Configuration, jsonSerializerOptions);
            File.WriteAllText(ConfigurationFile, contents);
            storageLock.Release();
        }

        public static void Load()
        {
            storageLock.Wait();
            if (!File.Exists(ConfigurationFile))
            {
                string newContents = JsonSerializer.Serialize(DefaultConfiguration, jsonSerializerOptions);
                File.WriteAllText(ConfigurationFile, newContents);
            }

            string contents = File.ReadAllText(ConfigurationFile);
            Configuration = JsonSerializer.Deserialize<Config>(contents, jsonSerializerOptions) ?? DefaultConfiguration;
            storageLock.Release();
        }

        private static async void SettingsFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == ConfigurationFile)
            {
                if (reloadEventCancellation != null)
                {
                    reloadEventCancellation.Cancel();
                    reloadEventCancellation.Dispose();
                }

                reloadEventCancellation = new CancellationTokenSource();
                try
                {
                    await Task.Delay(SettingsWatcherTimeout, reloadEventCancellation.Token);
                    Load();
                    ConfigurationChanged?.Invoke(null, new EventArgs());
                }
                catch (TaskCanceledException) { }
            }
        }
    }
}
