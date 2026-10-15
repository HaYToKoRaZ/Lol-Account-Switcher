using System;
using System.IO;
using System.Windows;

namespace LolAccountSwitcher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
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
    }
}
