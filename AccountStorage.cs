using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LolAccountSwitcher
{
    public static class AccountStorage
    {
        private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "accounts.json");
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

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
