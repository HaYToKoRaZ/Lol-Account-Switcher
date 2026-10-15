using System;
using System.Collections.Generic;
using System.IO;

namespace LolAccountSwitcher
{
    public static class ConfigManager
    {
        public static readonly string IniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LolAccountSwitcher.ini");
        private static readonly Dictionary<string, Dictionary<string, string>> Data = new(StringComparer.OrdinalIgnoreCase);

        static ConfigManager()
        {
            Load();
        }

        public static void Load()
        {
            Data.Clear();
            if (!File.Exists(IniPath))
            {
                CreateDefaultConfig();
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(IniPath);
                string currentSection = "General";

                foreach (string rawLine in lines)
                {
                    string line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith(";") || line.StartsWith("#"))
                        continue;

                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        currentSection = line.Substring(1, line.Length - 2).Trim();
                        if (!Data.ContainsKey(currentSection))
                            Data[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                    else
                    {
                        int idx = line.IndexOf('=');
                        if (idx > 0)
                        {
                            string key = line.Substring(0, idx).Trim();
                            string val = line.Substring(idx + 1).Trim();
                            if (!Data.ContainsKey(currentSection))
                                Data[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            Data[currentSection][key] = val;
                        }
                    }
                }
            }
            catch
            {
                CreateDefaultConfig();
            }
        }

        public static string Get(string section, string key, string defaultValue)
        {
            if (Data.TryGetValue(section, out var sec) && sec.TryGetValue(key, out var val))
            {
                return val;
            }
            return defaultValue;
        }

        public static void Set(string section, string key, string value)
        {
            if (!Data.ContainsKey(section))
                Data[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            Data[section][key] = value;
            Save();
        }

        public static void Save()
        {
            try
            {
                using var writer = new StreamWriter(IniPath, false, System.Text.Encoding.UTF8);
                writer.WriteLine("; ==============================================================================");
                writer.WriteLine("; Lol Account Switcher Configuration File / Yapılandırma Dosyası");
                writer.WriteLine("; ==============================================================================");
                writer.WriteLine("; Konum / Location: LolAccountSwitcher.exe ile aynı dizin / Same folder as .exe");
                writer.WriteLine("; Yayıncı / Publisher: HaYTooL (https://haytokoraz.github.io/)");
                writer.WriteLine("; Geliştirici / Developer: HaYTo (https://x.com/HaYTo)");
                writer.WriteLine("; Web Sitesi / Website: https://haytokoraz.github.io/Lol-Account-Switcher/");
                writer.WriteLine("; İletişim / Contact: korazhayto@gmail.com");
                writer.WriteLine("; ==============================================================================");
                writer.WriteLine();

                string langVal = Get("General", "Language", "tr");
                string themeVal = Get("General", "Theme", "system");
                string autoRefreshVal = Get("General", "AutoRefreshStats", "true");
                string playStartupSoundVal = Get("General", "PlayStartupSound", "true");
                string closeLeagueVal = Get("General", "CloseLeagueOnSwitch", "true");
                string autoLaunchGameVal = Get("General", "AutoLaunchGameOnSwitch", "true");
                string launchCompanionVal = Get("General", "LaunchCompanionAppOnSwitch", Get("General", "LaunchBlitzOnSwitch", "false"));
                string companionAppPath = Get("General", "CompanionAppPath", Get("General", "BlitzPath", ""));
                string switchPowerPlanGuid = Get("General", "SwitchPowerPlanGuid", "");
                string killLeaguePowerPlanGuid = Get("General", "KillLeaguePowerPlanGuid", "");
                string switchRefreshRate = Get("General", "SwitchRefreshRate", "144");
                string killLeagueRefreshRate = Get("General", "KillLeagueRefreshRate", "60");
                string riotGamesPath = Get("General", "RiotGamesPath", "");
                string riotProfilePath = Get("General", "RiotProfilePath", "");

                writer.WriteLine("[General]");
                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; Language / Uygulama Dili");
                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; [TR] Uygulamanın arayüz dilini belirler.");
                writer.WriteLine(";      Seçenekler: tr (Türkçe), en (İngilizce)");
                writer.WriteLine(";      Varsayılan Değer: tr");
                writer.WriteLine("; [EN] Determines the UI language of the application.");
                writer.WriteLine(";      Options: tr (Turkish), en (English)");
                writer.WriteLine(";      Default Value: tr");
                writer.WriteLine($"Language={langVal}");
                writer.WriteLine();

                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; Theme / Görünüm Teması");
                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; [TR] Uygulamanın renk temasını belirler.");
                writer.WriteLine(";      Seçenekler:");
                writer.WriteLine(";        - system (Windows işletim sistemi temasını otomatik algılar)");
                writer.WriteLine(";        - dark   (Koyu Hextech temasını zorlar)");
                writer.WriteLine(";        - light  (Açık Piltover gümüş temasını zorlar)");
                writer.WriteLine(";      Varsayılan Değer: system");
                writer.WriteLine("; [EN] Determines the visual color theme of the application.");
                writer.WriteLine(";      Options:");
                writer.WriteLine(";        - system (Automatically detects Windows OS theme)");
                writer.WriteLine(";        - dark   (Forces Dark Hextech theme)");
                writer.WriteLine(";        - light  (Forces Light Piltover silver theme)");
                writer.WriteLine(";      Default Value: system");
                writer.WriteLine($"Theme={themeVal}");
                writer.WriteLine();

                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; AutoRefreshStats / Açılışta Otomatik Rank & Profil Güncelleme");
                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; [TR] Uygulama her açıldığında kayıtlı hesapların rank ve avatar verilerini");
                writer.WriteLine(";      arka planda sessizce otomatik yeniler.");
                writer.WriteLine(";      Seçenekler: true (Açık), false (Kapalı)");
                writer.WriteLine(";      Varsayılan Değer: true");
                writer.WriteLine("; [EN] Automatically and silently updates ranked tier and profile pictures");
                writer.WriteLine(";      in the background when the application launches.");
                writer.WriteLine(";      Options: true (Enabled), false (Disabled)");
                writer.WriteLine(";      Default Value: true");
                writer.WriteLine($"AutoRefreshStats={autoRefreshVal}");
                writer.WriteLine();

                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; PlayStartupSound / Açılış Hoş Geldin Sesi");
                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; [TR] Uygulama her açıldığında hoş geldin sesini çalar.");
                writer.WriteLine(";      Seçenekler: true (Açık), false (Kapalı)");
                writer.WriteLine(";      Varsayılan Değer: true");
                writer.WriteLine("; [EN] Plays welcome sound upon application startup.");
                writer.WriteLine(";      Options: true (Enabled), false (Disabled)");
                writer.WriteLine(";      Default Value: true");
                writer.WriteLine($"PlayStartupSound={playStartupSoundVal}");
                writer.WriteLine();

                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; CloseLeagueOnSwitch / Geçişte İstemci Kapatma");
                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; [TR] Profil geçişi yapılırken arka planda açık olan LoL veya Riot Client");
                writer.WriteLine(";      süreçlerini otomatik ve temiz şekilde kapatır.");
                writer.WriteLine(";      Seçenekler: true (Açık), false (Kapalı)");
                writer.WriteLine(";      Varsayılan Değer: true");
                writer.WriteLine("; [EN] Automatically and cleanly closes open League of Legends or Riot Client");
                writer.WriteLine(";      processes during profile switching.");
                writer.WriteLine(";      Options: true (Enabled), false (Disabled)");
                writer.WriteLine(";      Default Value: true");
                writer.WriteLine($"CloseLeagueOnSwitch={closeLeagueVal}");
                writer.WriteLine();

                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; AutoLaunchGameOnSwitch / Hesap Geçişinde Oyunu Otomatik Başlatma");
                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; [TR] Profil geçişi başarıyla tamamlandığında League of Legends veya Riot Client'ı");
                writer.WriteLine(";      otomatik olarak başlatır.");
                writer.WriteLine(";      Seçenekler: true (Açık), false (Kapalı)");
                writer.WriteLine(";      Varsayılan Değer: true");
                writer.WriteLine("; [EN] Automatically launches League of Legends or Riot Client after profile switch.");
                writer.WriteLine(";      Options: true (Enabled), false (Disabled)");
                writer.WriteLine(";      Default Value: true");
                writer.WriteLine($"AutoLaunchGameOnSwitch={autoLaunchGameVal}");
                writer.WriteLine();

                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; LaunchCompanionAppOnSwitch / Hesap Geçişinde Yardımcı Uygulama Başlatma");
                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; [TR] Profil geçişi yapıldıktan sonra kurulu yardımcı uygulamayı otomatik başlatır");
                writer.WriteLine(";      (Blitz.gg, Porofessor, Mobalytics, OP.GG, Zar.gg vb.).");
                writer.WriteLine(";      Seçenekler: true (Açık), false (Kapalı)");
                writer.WriteLine(";      Varsayılan Değer: false");
                writer.WriteLine("; [EN] Automatically launches installed companion app after profile switch");
                writer.WriteLine(";      (Blitz.gg, Porofessor, Mobalytics, OP.GG, Zar.gg, etc.).");
                writer.WriteLine(";      Options: true (Enabled), false (Disabled)");
                writer.WriteLine(";      Default Value: false");
                writer.WriteLine($"LaunchCompanionAppOnSwitch={launchCompanionVal}");
                writer.WriteLine($"LaunchBlitzOnSwitch={launchCompanionVal}");
                writer.WriteLine();

                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; CompanionAppPath / Yardımcı Uygulama (.exe) Dosya Yolu");
                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; [TR] Yardımcı uygulamanın tam dosya yolu (Örn: C:\\Users\\...\\Blitz.exe veya Porofessor.exe).");
                writer.WriteLine(";      Boş bırakılırsa sistemden otomatik tespit edilir.");
                writer.WriteLine("; [EN] Full path to companion app executable (e.g. Blitz.exe, Porofessor.exe, Mobalytics.exe).");
                writer.WriteLine($"CompanionAppPath={companionAppPath}");
                writer.WriteLine($"BlitzPath={companionAppPath}");
                writer.WriteLine();

                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; SwitchPowerPlanGuid / Hesap Değişiminde Aktif Edilecek Güç Planı GUID");
                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; [TR] Hesap geçişi yapıldığında otomatik aktif edilecek Windows güç planı GUID'i.");
                writer.WriteLine(";      Boş bırakılırsa varsayılan yüksek performans planı aranır.");
                writer.WriteLine("; [EN] Windows power plan GUID activated automatically during account switch.");
                writer.WriteLine($"SwitchPowerPlanGuid={switchPowerPlanGuid}");
                writer.WriteLine();

                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; KillLeaguePowerPlanGuid / LoL Kapatıldığında Aktif Edilecek Güç Planı GUID");
                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; [TR] 'LOL KAPAT' tıklandığında otomatik aktif edilecek Windows güç planı GUID'i.");
                writer.WriteLine(";      Boş bırakılırsa dengeli / varsayılan plan uygulanır.");
                writer.WriteLine("; [EN] Windows power plan GUID activated when 'KILL LEAGUE' is triggered.");
                writer.WriteLine($"KillLeaguePowerPlanGuid={killLeaguePowerPlanGuid}");
                writer.WriteLine();

                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; SwitchRefreshRate / Hesap Değişiminde Monitör Tazeleme Hızı (Hz)");
                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; [TR] Hesap geçişinde monitöre uygulanacak ekran yenileme hızı (Hz).");
                writer.WriteLine(";      Örnekler: 60, 75, 120, 144, 165, 240, 360 veya 0 (değiştirme).");
                writer.WriteLine(";      Varsayılan Değer: 144");
                writer.WriteLine("; [EN] Monitor refresh rate (Hz) to set when switching accounts.");
                writer.WriteLine(";      Examples: 60, 75, 120, 144, 165, 240, 360 or 0 (keep unchanged).");
                writer.WriteLine(";      Default Value: 144");
                writer.WriteLine($"SwitchRefreshRate={switchRefreshRate}");
                writer.WriteLine();

                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; KillLeagueRefreshRate / LoL Kapatıldığında Monitör Tazeleme Hızı (Hz)");
                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; [TR] 'LOL KAPAT' tıklandığında monitöre uygulanacak ekran yenileme hızı (Hz).");
                writer.WriteLine(";      Örnekler: 60, 75, 120, 144 veya 0 (değiştirme).");
                writer.WriteLine(";      Varsayılan Değer: 60");
                writer.WriteLine("; [EN] Monitor refresh rate (Hz) to set when League is closed.");
                writer.WriteLine(";      Examples: 60, 75, 120, 144 or 0 (keep unchanged).");
                writer.WriteLine(";      Default Value: 60");
                writer.WriteLine($"KillLeagueRefreshRate={killLeagueRefreshRate}");
                writer.WriteLine();

                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; RiotGamesPath / Riot Games Kurulum Klasörü");
                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; [TR] Riot Games ana kurulum klasörü (Örn: D:\\Riot Games).");
                writer.WriteLine(";      Boş bırakılırsa sistemden otomatik tespit edilir.");
                writer.WriteLine("; [EN] Main installation folder of Riot Games.");
                writer.WriteLine($"RiotGamesPath={riotGamesPath}");
                writer.WriteLine();

                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; RiotProfilePath / Riot İstemci & Profil Veri Klasörü");
                writer.WriteLine("; ------------------------------------------------------------------------------");
                writer.WriteLine("; [TR] Riot Client ve profil verilerinin saklandığı AppData klasörü (Örn: C:\\Users\\...\\AppData\\Local\\Riot Games).");
                writer.WriteLine(";      Boş bırakılırsa sistemden otomatik tespit edilir.");
                writer.WriteLine("; [EN] Riot Client and profile data directory in AppData Local.");
                writer.WriteLine($"RiotProfilePath={riotProfilePath}");
                writer.WriteLine();
            }
            catch { }
        }

        private static void CreateDefaultConfig()
        {
            Set("General", "Language", "tr");
            Set("General", "Theme", "system");
            Set("General", "AutoRefreshStats", "true");
            Set("General", "PlayStartupSound", "true");
            Set("General", "CloseLeagueOnSwitch", "true");
            Set("General", "AutoLaunchGameOnSwitch", "true");
            Set("General", "LaunchCompanionAppOnSwitch", "false");
            Set("General", "CompanionAppPath", "");
            Set("General", "LaunchBlitzOnSwitch", "false");
            Set("General", "BlitzPath", "");
            Set("General", "SwitchPowerPlanGuid", "");
            Set("General", "KillLeaguePowerPlanGuid", "");
            Set("General", "SwitchRefreshRate", "144");
            Set("General", "KillLeagueRefreshRate", "60");
            Set("General", "RiotGamesPath", "");
            Set("General", "RiotProfilePath", "");
            Save();
        }
    }
}
