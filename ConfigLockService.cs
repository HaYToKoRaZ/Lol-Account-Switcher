using System;
using System.IO;

namespace LolAccountSwitcher
{
    public static class ConfigLockService
    {
        public static string ResolveConfigDirectory(string? customRiotGamesPath = null)
        {
            // 1. League of Legends kurulum klasöründeki Config (Örn: D:\Riot Games\League of Legends\Config)
            if (!string.IsNullOrEmpty(customRiotGamesPath) && Directory.Exists(customRiotGamesPath))
            {
                string p = Path.Combine(customRiotGamesPath, "League of Legends", "Config");
                if (Directory.Exists(p)) return p;
            }

            string detectedLeague = RiotPathDetector.DetectLeaguePath();
            if (!string.IsNullOrEmpty(detectedLeague) && Directory.Exists(detectedLeague))
            {
                string p = Path.Combine(detectedLeague, "Config");
                if (Directory.Exists(p)) return p;
            }

            // 2. AppData altındaki Config (Örn: C:\Users\<User>\AppData\Local\Riot Games\League of Legends\Config)
            try
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string appDataConfig = Path.Combine(localAppData, "Riot Games", "League of Legends", "Config");
                if (Directory.Exists(appDataConfig)) return appDataConfig;
            }
            catch { }

            // 3. Yaygın sürücü alternatifleri
            string[] fallbacks = new[]
            {
                @"D:\Riot Games\League of Legends\Config",
                @"C:\Riot Games\League of Legends\Config"
            };

            foreach (var fb in fallbacks)
            {
                if (Directory.Exists(fb)) return fb;
            }

            return !string.IsNullOrEmpty(detectedLeague) 
                ? Path.Combine(detectedLeague, "Config") 
                : @"D:\Riot Games\League of Legends\Config";
        }

        public static bool IsConfigReadOnly(string? customRiotGamesPath = null)
        {
            try
            {
                string configDir = ResolveConfigDirectory(customRiotGamesPath);
                string gameCfg = Path.Combine(configDir, "game.cfg");
                if (File.Exists(gameCfg))
                {
                    var attr = File.GetAttributes(gameCfg);
                    return attr.HasFlag(FileAttributes.ReadOnly);
                }
            }
            catch { }

            return false;
        }

        public static (bool IsNowReadOnly, string? BackupPath, string? ErrorMessage) ToggleConfigReadOnly(string? customRiotGamesPath = null)
        {
            try
            {
                string configDir = ResolveConfigDirectory(customRiotGamesPath);
                if (!Directory.Exists(configDir))
                {
                    return (false, null, $"Config klasörü bulunamadı:\n{configDir}");
                }

                // Otomatik Yedek Al (Config_Backup_yyyyMMdd-HHmm)
                string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmm");
                string parentDir = Directory.GetParent(configDir)?.FullName ?? configDir;
                string backupDir = Path.Combine(parentDir, $"Config_Backup_{timestamp}");

                try
                {
                    CopyDirectory(configDir, backupDir);
                }
                catch { }

                bool currentlyReadOnly = IsConfigReadOnly(customRiotGamesPath);
                bool targetReadOnly = !currentlyReadOnly;

                // Config klasörü içindeki tüm dosyaların salt okunurluğunu güncelle
                var files = Directory.GetFiles(configDir, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    try
                    {
                        var attr = File.GetAttributes(file);
                        if (targetReadOnly)
                        {
                            File.SetAttributes(file, attr | FileAttributes.ReadOnly);
                        }
                        else
                        {
                            File.SetAttributes(file, attr & ~FileAttributes.ReadOnly);
                        }
                    }
                    catch { }
                }

                return (targetReadOnly, backupDir, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        private static void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }
            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(targetDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }
    }
}
