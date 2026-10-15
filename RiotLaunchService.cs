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
                // 1. Öncelik: Özel belirtilmiş Riot Games dizini varsa kontrol et
                if (!string.IsNullOrEmpty(customRiotGamesPath) && Directory.Exists(customRiotGamesPath))
                {
                    string candidateRc = Path.Combine(customRiotGamesPath, "Riot Client", "RiotClientServices.exe");
                    if (File.Exists(candidateRc))
                    {
                        return StartProcess(candidateRc, "--launch-product=league_of_legends --launch-patchline=live");
                    }

                    string candidateLeague = Path.Combine(customRiotGamesPath, "League of Legends", "LeagueClient.exe");
                    if (File.Exists(candidateLeague))
                    {
                        return StartProcess(candidateLeague, "");
                    }
                }

                // 2. Öncelik: Sistemden otomatik tespit edilen RiotClientServices.exe
                string rcPath = RiotPathDetector.DetectRiotClientServicesPath();
                if (!string.IsNullOrEmpty(rcPath) && File.Exists(rcPath))
                {
                    return StartProcess(rcPath, "--launch-product=league_of_legends --launch-patchline=live");
                }

                // 3. Öncelik: League of Legends kurulum klasöründeki LeagueClient.exe
                string leagueDir = RiotPathDetector.DetectLeaguePath();
                if (!string.IsNullOrEmpty(leagueDir))
                {
                    string leagueExe = Path.Combine(leagueDir, "LeagueClient.exe");
                    if (File.Exists(leagueExe))
                    {
                        return StartProcess(leagueExe, "");
                    }
                }

                // 4. Öncelik: Yaygın alternatif yollar
                string[] fallbackCandidates = new[]
                {
                    @"D:\Riot Games\Riot Client\RiotClientServices.exe",
                    @"C:\Riot Games\Riot Client\RiotClientServices.exe",
                    @"D:\Riot Games\League of Legends\LeagueClient.exe",
                    @"C:\Riot Games\League of Legends\LeagueClient.exe"
                };

                foreach (var path in fallbackCandidates)
                {
                    if (File.Exists(path))
                    {
                        string args = path.Contains("Riot Client", StringComparison.OrdinalIgnoreCase)
                            ? "--launch-product=league_of_legends --launch-patchline=live"
                            : "";
                        return StartProcess(path, args);
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
