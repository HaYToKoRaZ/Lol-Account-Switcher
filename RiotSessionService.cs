using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LolAccountSwitcher
{
    public class DetectedRiotSession
    {
        public string SummonerName { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string TagLine { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Server { get; set; } = "TR";
        public bool IsValid => !string.IsNullOrEmpty(GameName);
    }

    public static class RiotSessionService
    {
        public static DetectedRiotSession? GetCurrentActiveSession(string? customRiotProfilePath = null)
        {
            string profileDir = string.IsNullOrWhiteSpace(customRiotProfilePath) 
                ? RiotPathDetector.DetectProfileDataPath() 
                : customRiotProfilePath;

            string yamlPath = Path.Combine(profileDir, "Riot Client", "Data", "RiotGamesPrivateSettings.yaml");
            if (!File.Exists(yamlPath))
            {
                return null;
            }

            try
            {
                string content = File.ReadAllText(yamlPath, Encoding.UTF8);
                var match = Regex.Match(content, @"id_token:\s*""([^""]+)""", RegexOptions.Singleline);
                if (!match.Success) return null;

                string jwt = match.Groups[1].Value.Trim().Replace("\r", "").Replace("\n", "");
                string[] parts = jwt.Split('.');
                if (parts.Length < 2) return null;

                string payload = parts[1].Replace('-', '+').Replace('_', '/');
                while (payload.Length % 4 != 0)
                {
                    payload += "=";
                }

                byte[] bytes = Convert.FromBase64String(payload);
                string jsonString = Encoding.UTF8.GetString(bytes);

                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                var result = new DetectedRiotSession();

                // acct: { game_name: "...", tag_line: "..." }
                if (root.TryGetProperty("acct", out var acct))
                {
                    if (acct.TryGetProperty("game_name", out var gName))
                        result.GameName = gName.GetString() ?? string.Empty;

                    if (acct.TryGetProperty("tag_line", out var tLine))
                        result.TagLine = tLine.GetString() ?? string.Empty;

                    if (!string.IsNullOrEmpty(result.GameName))
                    {
                        result.SummonerName = string.IsNullOrEmpty(result.TagLine) 
                            ? result.GameName 
                            : $"{result.GameName}#{result.TagLine}";
                    }
                }

                // lol: [ { uname: "...", cpid: "TR1" } ]
                if (root.TryGetProperty("lol", out var lolArr) && lolArr.ValueKind == JsonValueKind.Array && lolArr.GetArrayLength() > 0)
                {
                    var firstLol = lolArr[0];
                    if (firstLol.TryGetProperty("uname", out var uName))
                        result.Username = uName.GetString() ?? string.Empty;

                    if (firstLol.TryGetProperty("cpid", out var cpid))
                    {
                        result.Server = MapRegion(cpid.GetString());
                    }
                }
                else if (root.TryGetProperty("lol_region", out var regArr) && regArr.ValueKind == JsonValueKind.Array && regArr.GetArrayLength() > 0)
                {
                    var firstReg = regArr[0];
                    if (firstReg.TryGetProperty("cpid", out var cpid))
                    {
                        result.Server = MapRegion(cpid.GetString());
                    }
                }

                return result.IsValid ? result : null;
            }
            catch
            {
                return null;
            }
        }

        public static string GetProfilesStoragePath()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static bool SaveCurrentProfileToSlot(string profileId, string? customRiotProfilePath = null)
        {
            try
            {
                string sourceDir = string.IsNullOrWhiteSpace(customRiotProfilePath) 
                    ? RiotPathDetector.DetectProfileDataPath() 
                    : customRiotProfilePath;

                if (!Directory.Exists(sourceDir)) return false;

                string targetBase = GetProfilesStoragePath();
                string targetDir = Path.Combine(targetBase, profileId);

                if (Directory.Exists(targetDir))
                {
                    Directory.Delete(targetDir, true);
                }
                Directory.CreateDirectory(targetDir);

                CopyDirectory(sourceDir, targetDir);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool RestoreProfileSlot(string profileId, string? customRiotProfilePath = null)
        {
            try
            {
                string targetDir = string.IsNullOrWhiteSpace(customRiotProfilePath) 
                    ? RiotPathDetector.DetectProfileDataPath() 
                    : customRiotProfilePath;

                string sourceBase = GetProfilesStoragePath();
                string sourceDir = Path.Combine(sourceBase, profileId);

                if (!Directory.Exists(sourceDir)) return false;

                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                CopyDirectory(sourceDir, targetDir);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists) return;

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                try
                {
                    string targetFilePath = Path.Combine(destinationDir, file.Name);
                    file.CopyTo(targetFilePath, true);
                }
                catch { }
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                // Logs, Crash ve Vanguard klasörlerini atla (hız, hafiflik ve kilitlenme önleme)
                if (subDir.Name.Equals("Logs", StringComparison.OrdinalIgnoreCase) ||
                    subDir.Name.Equals("Crashes", StringComparison.OrdinalIgnoreCase) ||
                    subDir.Name.Equals("Riot Vanguard", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }

        private static string MapRegion(string? cpid)
        {
            if (string.IsNullOrEmpty(cpid)) return "TR";
            string upper = cpid.ToUpperInvariant();
            if (upper.StartsWith("TR")) return "TR";
            if (upper.StartsWith("EUW")) return "EUW";
            if (upper.StartsWith("NA")) return "NA";
            if (upper.StartsWith("EUN")) return "EUNE";
            if (upper.StartsWith("KR")) return "KR";
            return "EUW";
        }
    }
}
