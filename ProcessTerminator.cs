using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace LolAccountSwitcher
{
    public static class ProcessTerminator
    {
        private static readonly string[] TargetProcessNames = new[]
        {
            "LeagueClient",
            "LeagueClientUx",
            "LeagueClientUxRender",
            "League of Legends",
            "RiotClientServices",
            "RiotClientUx",
            "RiotClientUxRender",
            "RiotClientCrashHandler",
            "Valorant",
            "VALORANT-Win64-Shipping",
            "Blitz",
            "BlitzUpdater",
            "BlitzApp"
        };

        public static async Task TerminateLeagueProcessesAsync(string? customRiotGamesPath = null)
        {
            // 1. İsim bazlı sonlandırma
            foreach (var procName in TargetProcessNames)
            {
                try
                {
                    var procs = Process.GetProcessesByName(procName);
                    foreach (var p in procs)
                    {
                        try 
                        { 
                            p.Kill(); 
                            p.WaitForExit(1000); 
                        } 
                        catch { }
                    }
                }
                catch { }
            }

            // 2. Dizin bazlı tarama (Riot Games dizini altında çalışan tüm süreçler)
            string riotPath = customRiotGamesPath?.Trim() ?? "";
            if (string.IsNullOrEmpty(riotPath))
            {
                riotPath = RiotPathDetector.DetectRiotGamesRoot();
            }

            if (!string.IsNullOrEmpty(riotPath) && Directory.Exists(riotPath))
            {
                try
                {
                    var allProcesses = Process.GetProcesses();
                    foreach (var p in allProcesses)
                    {
                        try
                        {
                            string? filePath = null;
                            try
                            {
                                filePath = p.MainModule?.FileName;
                            }
                            catch { }

                            if (!string.IsNullOrEmpty(filePath) &&
                                filePath.StartsWith(riotPath, StringComparison.OrdinalIgnoreCase))
                            {
                                try 
                                { 
                                    p.Kill(); 
                                    p.WaitForExit(1000); 
                                } 
                                catch { }
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }

            // 3. Dosya kilitlerinin Windows kernel tarafından tamamen bırakılması için kısa bekleme
            await Task.Delay(400);
        }
    }
}
