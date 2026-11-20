using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SentoLauncher.Services;

namespace SentoLauncher
{
    public partial class App : Application
    {
        private static Mutex? _singleInstanceMutex;
        public static bool RestartRequested;

        public App()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            base.OnFrameworkInitializationCompleted();

            // Single instance guard
            _singleInstanceMutex = new Mutex(true, @"Global\SentoLauncherSingleInstance", out bool createdNew);
            if (!createdNew)
            {
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            EnsureConfigDirectory();

            // One-time migration: auto-close was the old default; users
            // explicitly asked for the launcher to stay open.
            var cfg = ConfigManager.Config;
            if (cfg.Version < 2)
            {
                cfg.Settings.AutoCloseLauncher = false;
                cfg.Version = 2;
                ConfigManager.SaveConfig(cfg);
            }

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void EnsureConfigDirectory()
        {
            try
            {
                if (!Directory.Exists(ConfigManager.ConfigDirectory))
                    Directory.CreateDirectory(ConfigManager.ConfigDirectory);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[App] Config directory error: {ex.Message}");
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Debug.WriteLine($"[App] Unhandled exception: {ex}");
                ShowFatalError(ex);
            }
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Debug.WriteLine($"[App] Unobserved task exception: {e.Exception}");
            e.SetObserved();
        }

        private static void ShowFatalError(Exception ex)
        {
            try
            {
                string detail = ex.ToString();
                File.AppendAllText(
                    Path.Combine(AppContext.BaseDirectory, "crash.log"),
                    $"[{DateTime.Now:O}]\r\n{detail}\r\n\r\n");
            }
            catch { }
        }

        public static new App Current => (App)Application.Current!;
    }
}