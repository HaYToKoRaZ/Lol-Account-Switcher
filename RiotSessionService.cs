using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
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
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (Path.GetFileName(baseDir.TrimEnd('\\', '/')).Equals("bin", StringComparison.OrdinalIgnoreCase))
            {
                baseDir = Directory.GetParent(baseDir)?.FullName ?? baseDir;
            }
            string path = Path.Combine(baseDir, "profiles");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        // Format-proof ve taşınabilir (portable) 256-bit AES anahtarı
        private static readonly byte[] s_aesKey = SHA256.HashData(Encoding.UTF8.GetBytes("HaYTooL_LolAccountSwitcher_MasterKey_2026_SecurePortable"));
        private static readonly byte[] s_entropy = Encoding.UTF8.GetBytes("LolStart_Dpapi_Entropy_2026");

        public static bool SaveCurrentProfileToSlot(string profileId, string? customRiotProfilePath = null)
        {
            try
            {
                string sourceDir = string.IsNullOrWhiteSpace(customRiotProfilePath) 
                    ? RiotPathDetector.DetectProfileDataPath() 
                    : customRiotProfilePath;

                if (!Directory.Exists(sourceDir)) return false;

                string targetBase = GetProfilesStoragePath();
                string encryptedFilePath = Path.Combine(targetBase, $"{profileId}.dat");

                // Riot profil klasörünü bellekte (RAM) en hızlı DEFLATE algoritmasıyla ZIP arşivine dönüştür
                byte[] rawZipBytes;
                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
                    {
                        AddDirectoryToArchive(archive, sourceDir, sourceDir);
                    }
                    rawZipBytes = memoryStream.ToArray();
                }

                // Donanım destekli (AES-NI) AES-256 ile şifrele:
                // PC formatlansa, USB ile başka PC'ye taşınsa bile sorunsuz açılır;
                // diski tarayan infostealer zararlıları açık hesap veya token bulamaz.
                byte[] encryptedBytes = EncryptBytes(rawZipBytes);

                // Diske şifreli tek bir .dat dosyası olarak yaz
                File.WriteAllBytes(encryptedFilePath, encryptedBytes);

                // Eski unencrypted slot klasörü varsa kalıntı bırakmamak için temizle
                string legacyDir = Path.Combine(targetBase, profileId);
                if (Directory.Exists(legacyDir))
                {
                    try { Directory.Delete(legacyDir, true); } catch { }
                }

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
                string encryptedFilePath = Path.Combine(sourceBase, $"{profileId}.dat");
                string legacyDir = Path.Combine(sourceBase, profileId);

                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                // 1. Şifreli .dat dosyasından geri yükle (öncelikli, format-proof yöntem)
                if (File.Exists(encryptedFilePath))
                {
                    byte[] encryptedBytes = File.ReadAllBytes(encryptedFilePath);
                    byte[]? decryptedBytes = null;

                    try
                    {
                        decryptedBytes = DecryptBytes(encryptedBytes);
                    }
                    catch
                    {
                        // Eski DPAPI test kayıtları için geçiş toleransı
                        try
                        {
                            decryptedBytes = ProtectedData.Unprotect(encryptedBytes, s_entropy, DataProtectionScope.CurrentUser);
                        }
                        catch { }
                    }

                    if (decryptedBytes != null)
                    {
                        using var memoryStream = new MemoryStream(decryptedBytes);
                        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

                        foreach (var entry in archive.Entries)
                        {
                            string destinationPath = Path.GetFullPath(Path.Combine(targetDir, entry.FullName));
                            if (!destinationPath.StartsWith(Path.GetFullPath(targetDir), StringComparison.OrdinalIgnoreCase))
                            {
                                continue; // Zip-Slip güvenlik koruması
                            }

                            if (string.IsNullOrEmpty(entry.Name))
                            {
                                Directory.CreateDirectory(destinationPath);
                                continue;
                            }

                            string? parentDir = Path.GetDirectoryName(destinationPath);
                            if (!string.IsNullOrEmpty(parentDir))
                            {
                                Directory.CreateDirectory(parentDir);
                            }

                            bool extracted = false;
                            for (int attempt = 0; attempt < 5; attempt++)
                            {
                                try
                                {
                                    entry.ExtractToFile(destinationPath, overwrite: true);
                                    extracted = true;
                                    break;
                                }
                                catch (IOException)
                                {
                                    System.Threading.Thread.Sleep(100);
                                }
                                catch
                                {
                                    break;
                                }
                            }

                            if (!extracted && destinationPath.EndsWith("RiotGamesPrivateSettings.yaml", StringComparison.OrdinalIgnoreCase))
                            {
                                return false;
                            }
                        }

                        // Kritik oturum dosyasının (RiotGamesPrivateSettings.yaml) varlığını ve kilit durumunu doğrula
                        string criticalYaml = Path.Combine(targetDir, "Riot Client", "Data", "RiotGamesPrivateSettings.yaml");
                        if (File.Exists(criticalYaml))
                        {
                            try
                            {
                                using var fs = new FileStream(criticalYaml, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                                if (fs.Length == 0) return false;
                            }
                            catch
                            {
                                // Dosya kilitli veya erişilemez
                                return false;
                            }
                        }

                        return true;
                    }
                }

                // 2. Geriye dönük uyumluluk: Eski açık klasör yapısı varsa oradan geri yükle
                if (Directory.Exists(legacyDir))
                {
                    CopyDirectory(legacyDir, targetDir);
                    string criticalYaml = Path.Combine(targetDir, "Riot Client", "Data", "RiotGamesPrivateSettings.yaml");
                    if (File.Exists(criticalYaml))
                    {
                        try
                        {
                            using var fs = new FileStream(criticalYaml, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                            if (fs.Length == 0) return false;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static byte[] EncryptBytes(byte[] rawBytes)
        {
            using var aes = Aes.Create();
            aes.Key = s_aesKey;
            aes.GenerateIV();
            byte[] iv = aes.IV;

            using var memoryStream = new MemoryStream();
            memoryStream.Write(iv, 0, iv.Length);

            using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cryptoStream.Write(rawBytes, 0, rawBytes.Length);
                cryptoStream.FlushFinalBlock();
            }

            return memoryStream.ToArray();
        }

        private static byte[] DecryptBytes(byte[] encryptedData)
        {
            if (encryptedData.Length < 16)
                throw new InvalidDataException("Geçersiz şifreli profil verisi.");

            using var aes = Aes.Create();
            aes.Key = s_aesKey;

            byte[] iv = new byte[16];
            Buffer.BlockCopy(encryptedData, 0, iv, 0, 16);
            aes.IV = iv;

            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(
                new MemoryStream(encryptedData, 16, encryptedData.Length - 16),
                aes.CreateDecryptor(),
                CryptoStreamMode.Read))
            {
                cryptoStream.CopyTo(memoryStream);
            }

            return memoryStream.ToArray();
        }

        public static void DeleteProfileSlot(string profileId)
        {
            try
            {
                string targetBase = GetProfilesStoragePath();
                string encryptedFilePath = Path.Combine(targetBase, $"{profileId}.dat");
                if (File.Exists(encryptedFilePath))
                {
                    File.Delete(encryptedFilePath);
                }

                string legacyDir = Path.Combine(targetBase, profileId);
                if (Directory.Exists(legacyDir))
                {
                    Directory.Delete(legacyDir, true);
                }
            }
            catch { }
        }

        private static void AddDirectoryToArchive(ZipArchive archive, string currentDir, string rootDir)
        {
            var dir = new DirectoryInfo(currentDir);
            if (!dir.Exists) return;

            foreach (FileInfo file in dir.GetFiles())
            {
                try
                {
                    string relativePath = Path.GetRelativePath(rootDir, file.FullName).Replace('\\', '/');
                    var entry = archive.CreateEntry(relativePath, CompressionLevel.Fastest);
                    using var entryStream = entry.Open();
                    using var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    fileStream.CopyTo(entryStream);
                }
                catch { }
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                // Logs, Crashes ve Vanguard klasörlerini atla (hız, hafiflik ve kilitlenme önleme)
                if (subDir.Name.Equals("Logs", StringComparison.OrdinalIgnoreCase) ||
                    subDir.Name.Equals("Crashes", StringComparison.OrdinalIgnoreCase) ||
                    subDir.Name.Equals("Riot Vanguard", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                AddDirectoryToArchive(archive, subDir.FullName, rootDir);
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
