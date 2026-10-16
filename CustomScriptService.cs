using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace LolAccountSwitcher
{
    /// <summary>
    /// Kullanıcının hesap geçişi öncesinde, sonrasında veya LoL kapatıldığında
    /// çalıştırılmak üzere tanımladığı özel komutları (.exe, .bat, .cmd, .ps1, CLI) güvenle yöneten servis.
    /// </summary>
    public static class CustomScriptService
    {
        /// <summary>
        /// Verilen komut dizesini asenkron olarak arka planda çalıştırır.
        /// {account}, {summoner}, {server} gibi değişkenleri otomatik çözümler.
        /// </summary>
        /// <param name="rawCommand">Kullanıcı tarafından girilen komut veya dosya yolu</param>
        /// <param name="acc">İlgili hesap modeli (varsa)</param>
        public static async Task ExecuteCommandAsync(string? rawCommand, AccountModel? acc = null)
        {
            if (string.IsNullOrWhiteSpace(rawCommand)) return;

            string cmd = rawCommand.Trim();

            // Değişkenleri dinamik doldur
            if (acc != null)
            {
                cmd = cmd.Replace("{account}", acc.Title, StringComparison.OrdinalIgnoreCase)
                         .Replace("%ACCOUNT%", acc.Title, StringComparison.OrdinalIgnoreCase)
                         .Replace("{summoner}", acc.SummonerName, StringComparison.OrdinalIgnoreCase)
                         .Replace("%SUMMONER%", acc.SummonerName, StringComparison.OrdinalIgnoreCase)
                         .Replace("{server}", acc.Server, StringComparison.OrdinalIgnoreCase)
                         .Replace("%SERVER%", acc.Server, StringComparison.OrdinalIgnoreCase)
                         .Replace("{id}", acc.Id, StringComparison.OrdinalIgnoreCase);
            }

            await Task.Run(() =>
            {
                try
                {
                    string unquoted = cmd.Trim().Trim('"', '\'');

                    // 1. Doğrudan var olan bir .exe veya .bat dosyası mı?
                    if (File.Exists(unquoted))
                    {
                        string ext = Path.GetExtension(unquoted).ToLowerInvariant();

                        if (ext == ".ps1")
                        {
                            var psPsi = new ProcessStartInfo
                            {
                                FileName = "powershell.exe",
                                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{unquoted}\"",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WindowStyle = ProcessWindowStyle.Hidden
                            };
                            Process.Start(psPsi);
                            return;
                        }

                        var directPsi = new ProcessStartInfo
                        {
                            FileName = unquoted,
                            WorkingDirectory = Path.GetDirectoryName(unquoted) ?? "",
                            UseShellExecute = true
                        };
                        Process.Start(directPsi);
                        return;
                    }

                    // 2. PowerShell komutu mu?
                    if (cmd.StartsWith("powershell", StringComparison.OrdinalIgnoreCase))
                    {
                        var psCmdPsi = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = cmd.Substring("powershell".Length).Trim(),
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };
                        Process.Start(psCmdPsi);
                        return;
                    }

                    // 3. Genel CLI / CMD komutu (Örn: ipconfig /flushdns, taskkill, start vb.)
                    var cmdPsi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {cmd}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    Process.Start(cmdPsi);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[CustomScriptService] Komut çalıştırma hatası: {ex.Message}");
                }
            });
        }
    }
}
