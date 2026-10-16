using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace LolAccountSwitcher
{
    public partial class App : System.Windows.Application
    {
        private static Mutex? _singleInstanceMutex;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "Local\\LolAccountSwitcher_SingleInstance_Mutex_2026";
            _singleInstanceMutex = new Mutex(true, mutexName, out bool isNewInstance);

            if (!isNewInstance)
            {
                BringExistingInstanceToFront();
                Shutdown();
                return;
            }

            try
            {
                base.OnStartup(e);

                // 1. Yapılandırma dosyasını (config.ini) yükle
                ConfigManager.Load();

                // 2. Dil tercihini (TR / EN) yükle
                LocalizationManager.Initialize();

                // 3. Tema tercihini (Sistem / Koyu / Açık) yükle ve uygula
                ThemeManager.InitializeTheme();
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup_error.log"), ex.ToString());
                MessageBox.Show(ex.ToString(), "Startup Error");
            }
        }

        private static void BringExistingInstanceToFront()
        {
            try
            {
                int currentPid = Process.GetCurrentProcess().Id;
                Process[] processes = Process.GetProcessesByName("LolAccountSwitcher");
                foreach (var proc in processes)
                {
                    if (proc.Id != currentPid && proc.MainWindowHandle != IntPtr.Zero)
                    {
                        IntPtr hWnd = proc.MainWindowHandle;
                        if (IsIconic(hWnd))
                        {
                            ShowWindowAsync(hWnd, SW_RESTORE);
                        }
                        else
                        {
                            ShowWindowAsync(hWnd, SW_SHOW);
                        }
                        SetForegroundWindow(hWnd);
                        break;
                    }
                }
            }
            catch { }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _singleInstanceMutex?.ReleaseMutex();
                _singleInstanceMutex?.Dispose();
            }
            catch { }
            base.OnExit(e);
        }
    }
}
