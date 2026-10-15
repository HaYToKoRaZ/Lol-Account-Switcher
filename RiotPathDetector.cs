using System;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace LolAccountSwitcher
{
    public static class RiotPathDetector
    {
        public static string DetectRiotGamesRoot()
        {
            // 1. League of Legends yolundan bir üst dizini al
            string lolPath = DetectLeaguePath();
            if (!string.IsNullOrEmpty(lolPath))
            {
                string? parent = Directory.GetParent(lolPath)?.FullName;
                if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent))
                {
                    return parent;
                }
            }

            // 2. C:\ProgramData\Riot Games\Metadata\league_of_legends.live\...product_settings.yaml
            try
            {
                string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string yamlPath = Path.Combine(programData, "Riot Games", "Metadata", "league_of_legends.live", "league_of_legends.live.product_settings.yaml");
                if (File.Exists(yamlPath))
                {
                    string content = File.ReadAllText(yamlPath);
                    var matchRoot = Regex.Match(content, @"product_install_root:\s*""?([^""\r\n]+)""?");
                    if (matchRoot.Success)
                    {
                        string rPath = matchRoot.Groups[1].Value.Trim().Replace('/', '\\');
                        if (Directory.Exists(rPath)) return rPath;
                    }
                }
            }
            catch { }

            // 3. Yaygın Riot Games kök dizinleri
            string[] commonRoots = new[]
            {
                @"D:\Riot Games",
                @"C:\Riot Games",
                @"E:\Riot Games",
                @"F:\Riot Games",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Riot Games")
            };

            foreach (var r in commonRoots)
            {
                if (Directory.Exists(r)) return r;
            }

            return @"C:\Riot Games";
        }

        public static string DetectProfileDataPath()
        {
            // Riot Profil klasörü: C:\Users\<User>\AppData\Local\Riot Games
            try
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string riotProfileDir = Path.Combine(localAppData, "Riot Games");
                if (Directory.Exists(riotProfileDir))
                {
                    return riotProfileDir;
                }
            }
            catch { }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Riot Games");
        }

        public static string DetectLeaguePath()
        {
            // 1. C:\ProgramData\Riot Games\Metadata\league_of_legends.live\league_of_legends.live.product_settings.yaml
            try
            {
                string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string yamlPath = Path.Combine(programData, "Riot Games", "Metadata", "league_of_legends.live", "league_of_legends.live.product_settings.yaml");
                if (File.Exists(yamlPath))
                {
                    string content = File.ReadAllText(yamlPath);
                    var match = Regex.Match(content, @"product_install_full_path:\s*""?([^""\r\n]+)""?");
                    if (match.Success)
                    {
                        string path = match.Groups[1].Value.Trim().Replace('/', '\\');
                        if (Directory.Exists(path) && (File.Exists(Path.Combine(path, "LeagueClient.exe")) || File.Exists(Path.Combine(path, "League of Legends.exe"))))
                        {
                            return path;
                        }
                    }
                }
            }
            catch { }

            // 2. C:\ProgramData\Riot Games\RiotClientInstalls.json
            try
            {
                string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string jsonPath = Path.Combine(programData, "Riot Games", "RiotClientInstalls.json");
                if (File.Exists(jsonPath))
                {
                    string json = File.ReadAllText(jsonPath);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("associated_client", out var assoc))
                    {
                        foreach (var prop in assoc.EnumerateObject())
                        {
                            string rawPath = prop.Name.Replace('/', '\\').TrimEnd('\\');
                            if (rawPath.IndexOf("League of Legends", StringComparison.OrdinalIgnoreCase) >= 0 && Directory.Exists(rawPath))
                            {
                                return rawPath;
                            }
                        }
                    }
                }
            }
            catch { }

            // 3. Windows Registry (HKLM ve HKCU)
            try
            {
                string[] regPaths = new[]
                {
                    @"SOFTWARE\WOW6432Node\Riot Games, Inc\League of Legends",
                    @"SOFTWARE\Riot Games, Inc\League of Legends",
                    @"SOFTWARE\WOW6432Node\Riot Games\League of Legends"
                };

                foreach (var rPath in regPaths)
                {
                    using var key = Registry.LocalMachine.OpenSubKey(rPath);
                    if (key != null)
                    {
                        var loc = key.GetValue("Location") as string;
                        if (!string.IsNullOrEmpty(loc) && Directory.Exists(loc))
                        {
                            return loc;
                        }
                    }
                }
            }
            catch { }

            // 4. Yaygın Sürücüler ve Varsayılan Dizinler
            string[] commonPaths = new[]
            {
                @"D:\Riot Games\League of Legends",
                @"C:\Riot Games\League of Legends",
                @"E:\Riot Games\League of Legends",
                @"F:\Riot Games\League of Legends",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Riot Games", "League of Legends")
            };

            foreach (var p in commonPaths)
            {
                if (Directory.Exists(p) && (File.Exists(Path.Combine(p, "LeagueClient.exe")) || File.Exists(Path.Combine(p, "League of Legends.exe"))))
                {
                    return p;
                }
            }

            return string.Empty;
        }

        public static string DetectRiotClientServicesPath()
        {
            try
            {
                string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string jsonPath = Path.Combine(programData, "Riot Games", "RiotClientInstalls.json");
                if (File.Exists(jsonPath))
                {
                    string json = File.ReadAllText(jsonPath);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("rc_default", out var rcDef))
                    {
                        string rcPath = rcDef.GetString()?.Replace('/', '\\') ?? "";
                        if (File.Exists(rcPath)) return rcPath;
                    }
                }
            }
            catch { }

            string[] commonRiot = new[]
            {
                @"D:\Riot Games\Riot Client\RiotClientServices.exe",
                @"C:\Riot Games\Riot Client\RiotClientServices.exe"
            };

            foreach (var p in commonRiot)
            {
                if (File.Exists(p)) return p;
            }

            return string.Empty;
        }
    }
}
