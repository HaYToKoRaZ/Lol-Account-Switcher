using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LolAccountSwitcher
{
    public static class AccountStorage
    {
        private static readonly string FilePath = ResolveAccountPath();
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        private static string ResolveAccountPath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
            string parentDir = Directory.GetParent(baseDir)?.FullName ?? "";

            // Eğer "bin" klasörü içinden çalışıyorsa (modüler mimari), daima kök dizindeki (parentDir) accounts.json'ı kullan
            if (Path.GetFileName(baseDir).Equals("bin", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(parentDir))
            {
                return Path.Combine(parentDir, "accounts.json");
            }

            string localPath = Path.Combine(baseDir, "accounts.json");
            if (File.Exists(localPath)) return localPath;

            string parentPath = Path.Combine(parentDir, "accounts.json");
            if (File.Exists(parentPath)) return parentPath;

            return localPath;
        }

        public static List<AccountModel> LoadAccounts()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    string json = File.ReadAllText(FilePath);
                    var list = JsonSerializer.Deserialize<List<AccountModel>>(json);
                    if (list != null && list.Count > 0)
                        return list;
                }
                catch { }
            }

            // Varsayılan ilk açılış: Kullanıcının kendi hesabını eklemesi için temiz boş liste
            var defaults = new List<AccountModel>();
            SaveAccounts(defaults);
            return defaults;
        }

        public static void SaveAccounts(IEnumerable<AccountModel> accounts)
        {
            try
            {
                string json = JsonSerializer.Serialize(accounts, JsonOptions);
                File.WriteAllText(FilePath, json);
            }
            catch { }
        }
    }
}
