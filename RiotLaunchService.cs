using System;
using System.Diagnostics;
using System.IO;

namespace LolAccountSwitcher
{
    public static class RiotLaunchService
    {
        public static bool LaunchLeagueOfLegends(string? customRiotGamesPath = null)
        {
            try
            {
                const string rcArgs = "--launch-product=league_of_legends --launch-patchline=live";

                // 1. Özel belirtilmiş Riot Games dizini varsa RiotClientServices.exe kontrol et
                if (!string.IsNullOrEmpty(customRiotGamesPath) && Directory.Exists(customRiotGamesPath))
                {
                    string candidateRc = Path.Combine(customRiotGamesPath, "Riot Client", "RiotClientServices.exe");
                    if (File.Exists(candidateRc))
                    {
                        return StartProcess(candidateRc, rcArgs);
                    }

                    string directRc = Path.Combine(customRiotGamesPath, "RiotClientServices.exe");
                    if (File.Exists(directRc))
                    {
                        return StartProcess(directRc, rcArgs);
                    }
                }

                // 2. Sistemden otomatik tespit edilen RiotClientServices.exe
                string rcPath = RiotPathDetector.DetectRiotClientServicesPath();
                if (!string.IsNullOrEmpty(rcPath) && File.Exists(rcPath))
                {
                    return StartProcess(rcPath, rcArgs);
                }

                // 3. Yaygın sürücülerdeki standart RiotClientServices.exe konumları
                string[] rcFallbackCandidates = new[]
                {
                    @"D:\Riot Games\Riot Client\RiotClientServices.exe",
                    @"C:\Riot Games\Riot Client\RiotClientServices.exe",
                    @"E:\Riot Games\Riot Client\RiotClientServices.exe",
                    @"F:\Riot Games\Riot Client\RiotClientServices.exe"
                };

                foreach (var path in rcFallbackCandidates)
                {
                    if (File.Exists(path))
                    {
                        return StartProcess(path, rcArgs);
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool StartProcess(string exePath, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(exePath) ?? ""
                };
                Process.Start(psi);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
