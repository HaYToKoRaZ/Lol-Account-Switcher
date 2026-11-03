using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LolAccountSwitcher
{
    public enum ScriptTriggerTiming
    {
        PostSwitch = 0, // Geçiş Sonrası
        PreSwitch = 1,  // Geçiş Öncesi
        KillLeague = 2  // LoL Kapat
    }

    public class CustomCommandItem : System.ComponentModel.INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString("N");
        private string _name = "Yeni Komut";
        private string _command = "";
        private string _args = "";
        private ScriptTriggerTiming _timing = ScriptTriggerTiming.PostSwitch;
        private bool _isEnabled = true;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propName));
        }

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Command
        {
            get => _command;
            set { _command = value; OnPropertyChanged(); }
        }

        public string Args
        {
            get => _args;
            set { _args = value; OnPropertyChanged(); }
        }

        public ScriptTriggerTiming Timing
        {
            get => _timing;
            set { _timing = value; OnPropertyChanged(); OnPropertyChanged(nameof(TimingIndex)); }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public int TimingIndex
        {
            get => (int)_timing;
            set { Timing = (ScriptTriggerTiming)value; }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(); }
        }
    }

    /// <summary>
    /// Kullanıcının hesap geçişi öncesinde, sonrasında veya LoL kapatıldığında
    /// çalıştırılmak üzere tanımladığı özel komutları (.exe, .bat, .cmd, .ps1, CLI) güvenle yöneten servis.
    /// </summary>
    public static class CustomScriptService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = false };

        public static List<CustomCommandItem> LoadCommands()
        {
            var list = new List<CustomCommandItem>();
            string raw = ConfigManager.Get("CustomScripts", "CommandsJson", "").Trim();
            if (!string.IsNullOrEmpty(raw))
            {
                string json = raw;
                // Base64 kodlanmışsa çöz, değilse doğrudan JSON dene (geriye dönük uyumluluk)
                if (!raw.StartsWith("[") && !raw.StartsWith("{"))
                {
                    try
                    {
                        byte[] bytes = Convert.FromBase64String(raw);
                        json = System.Text.Encoding.UTF8.GetString(bytes);
                    }
                    catch
                    {
                        json = raw;
                    }
                }

                try
                {
                    var parsed = JsonSerializer.Deserialize<List<CustomCommandItem>>(json, JsonOptions);
                    if (parsed != null && parsed.Count > 0)
                    {
                        return parsed;
                    }
                }
                catch { }
            }

            // Geriye dönük uyumluluk: Eski PreSwitchCommand, PostSwitchCommand, KillLeagueCommand değerleri varsa aktar
            string preCmd = ConfigManager.Get("General", "PreSwitchCommand", "").Trim();
            string preArgs = ConfigManager.Get("General", "PreSwitchArgs", "").Trim();
            if (!string.IsNullOrEmpty(preCmd))
            {
                list.Add(new CustomCommandItem { Name = "Pre-Switch", Command = preCmd, Args = preArgs, Timing = ScriptTriggerTiming.PreSwitch, IsEnabled = true });
            }

            string postCmd = ConfigManager.Get("General", "PostSwitchCommand", "").Trim();
            string postArgs = ConfigManager.Get("General", "PostSwitchArgs", "").Trim();
            if (!string.IsNullOrEmpty(postCmd))
            {
                list.Add(new CustomCommandItem { Name = "Post-Switch", Command = postCmd, Args = postArgs, Timing = ScriptTriggerTiming.PostSwitch, IsEnabled = true });
            }

            string killCmd = ConfigManager.Get("General", "KillLeagueCommand", "").Trim();
            string killArgs = ConfigManager.Get("General", "KillLeagueArgs", "").Trim();
            if (!string.IsNullOrEmpty(killCmd))
            {
                list.Add(new CustomCommandItem { Name = "Kill-League", Command = killCmd, Args = killArgs, Timing = ScriptTriggerTiming.KillLeague, IsEnabled = true });
            }

            return list;
        }

        public static void SaveCommands(List<CustomCommandItem> commands)
        {
            try
            {
                string json = JsonSerializer.Serialize(commands ?? new List<CustomCommandItem>(), JsonOptions);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                string base64 = Convert.ToBase64String(bytes);
                ConfigManager.Set("CustomScripts", "CommandsJson", base64);

                // Geriye dönük uyumluluk için ilk Pre, Post ve Kill komutlarını General bölümüne de senkronla
                var firstPre = commands?.FirstOrDefault(c => c.Timing == ScriptTriggerTiming.PreSwitch && c.IsEnabled);
                var firstPost = commands?.FirstOrDefault(c => c.Timing == ScriptTriggerTiming.PostSwitch && c.IsEnabled);
                var firstKill = commands?.FirstOrDefault(c => c.Timing == ScriptTriggerTiming.KillLeague && c.IsEnabled);

                ConfigManager.Set("General", "PreSwitchCommand", firstPre?.Command ?? "");
                ConfigManager.Set("General", "PreSwitchArgs", firstPre?.Args ?? "");
                ConfigManager.Set("General", "PostSwitchCommand", firstPost?.Command ?? "");
                ConfigManager.Set("General", "PostSwitchArgs", firstPost?.Args ?? "");
                ConfigManager.Set("General", "KillLeagueCommand", firstKill?.Command ?? "");
                ConfigManager.Set("General", "KillLeagueArgs", firstKill?.Args ?? "");
            }
            catch { }
        }

        public static async Task ExecuteTriggerAsync(ScriptTriggerTiming timing, AccountModel? acc = null)
        {
            var commands = LoadCommands();
            var active = commands.Where(c => c.Timing == timing && c.IsEnabled).ToList();

            foreach (var item in active)
            {
                if (!string.IsNullOrWhiteSpace(item.Command))
                {
                    await ExecuteCommandAsync(item.Command, item.Args, acc);
                }
            }
        }

        /// <summary>
        /// Verilen komut dizesini ve isteğe bağlı başlatma parametrelerini asenkron olarak arka planda çalıştırır.
        /// {account}, {summoner}, {server}, {id} gibi değişkenleri otomatik çözümler.
        /// </summary>
        public static async Task ExecuteCommandAsync(string? rawCommand, string? rawArgs = null, AccountModel? acc = null)
        {
            if (string.IsNullOrWhiteSpace(rawCommand)) return;

            string cmd = rawCommand.Trim();
            string args = (rawArgs ?? "").Trim();

            // Değişkenleri dinamik doldur
            if (acc != null)
            {
                cmd = ResolveVariables(cmd, acc);
                args = ResolveVariables(args, acc);
            }

            await Task.Run(() =>
            {
                try
                {
                    string exePath = cmd;
                    string executionArgs = args;

                    // 1. Akıllı Parçalama: Eğer args verilmemişse ama cmd içinde hem dosya yolu hem argüman varsa ayıkla
                    if (string.IsNullOrEmpty(executionArgs))
                    {
                        var parsed = SplitCommandAndArgs(cmd);
                        exePath = parsed.ExePath;
                        executionArgs = parsed.Args;
                    }
                    else
                    {
                        exePath = exePath.Trim().Trim('"', '\'');
                    }

                    // 2. Doğrudan var olan bir dosya (.exe, .bat, .cmd, .ps1) mı?
                    if (File.Exists(exePath))
                    {
                        string ext = Path.GetExtension(exePath).ToLowerInvariant();

                        if (ext == ".ps1")
                        {
                            string psArgs = $"-NoProfile -ExecutionPolicy Bypass -File \"{exePath}\"";
                            if (!string.IsNullOrWhiteSpace(executionArgs))
                            {
                                psArgs += $" {executionArgs}";
                            }

                            var psPsi = new ProcessStartInfo
                            {
                                FileName = "powershell.exe",
                                Arguments = psArgs,
                                WorkingDirectory = Path.GetDirectoryName(exePath) ?? "",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WindowStyle = ProcessWindowStyle.Hidden
                            };
                            Process.Start(psPsi);
                            return;
                        }

                        // .exe, .bat, .cmd veya diğer çalıştırılabilir dosyalar
                        var directPsi = new ProcessStartInfo
                        {
                            FileName = exePath,
                            Arguments = executionArgs,
                            WorkingDirectory = Path.GetDirectoryName(exePath) ?? "",
                            UseShellExecute = true
                        };
                        Process.Start(directPsi);
                        return;
                    }

                    // 3. PowerShell komutu mu? (Örn: powershell ...)
                    if (cmd.StartsWith("powershell", StringComparison.OrdinalIgnoreCase))
                    {
                        string psArgContent = cmd.Substring("powershell".Length).Trim();
                        if (!string.IsNullOrWhiteSpace(executionArgs))
                        {
                            psArgContent += $" {executionArgs}";
                        }

                        var psCmdPsi = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = psArgContent,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };
                        Process.Start(psCmdPsi);
                        return;
                    }

                    // 4. Genel CLI / CMD komutu (Örn: taskkill /f /im ..., ipconfig /flushdns vb.)
                    string fullCmd = string.IsNullOrWhiteSpace(executionArgs) ? cmd : $"{cmd} {executionArgs}";
                    var cmdPsi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"{fullCmd}\"",
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

        private static string ResolveVariables(string input, AccountModel acc)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Replace("{account}", acc.Title, StringComparison.OrdinalIgnoreCase)
                        .Replace("%ACCOUNT%", acc.Title, StringComparison.OrdinalIgnoreCase)
                        .Replace("{summoner}", acc.SummonerName, StringComparison.OrdinalIgnoreCase)
                        .Replace("%SUMMONER%", acc.SummonerName, StringComparison.OrdinalIgnoreCase)
                        .Replace("{server}", acc.Server, StringComparison.OrdinalIgnoreCase)
                        .Replace("%SERVER%", acc.Server, StringComparison.OrdinalIgnoreCase)
                        .Replace("{id}", acc.Id, StringComparison.OrdinalIgnoreCase);
        }

        private static (string ExePath, string Args) SplitCommandAndArgs(string raw)
        {
            string trimmed = raw.Trim();
            if (string.IsNullOrEmpty(trimmed)) return ("", "");

            // Durum A: Eğer tırnakla başlıyorsa: "D:\Program Files\...\app.exe" toff
            if (trimmed.StartsWith("\""))
            {
                int closeIdx = trimmed.IndexOf('"', 1);
                if (closeIdx > 1)
                {
                    string candidate = trimmed.Substring(1, closeIdx - 1).Trim();
                    string rest = trimmed.Substring(closeIdx + 1).Trim();
                    if (File.Exists(candidate))
                    {
                        return (candidate, rest);
                    }
                }
            }

            // Durum B: Tırnak tüm dizeyi kapsıyorsa: "D:\Program Files\...\app.exe"
            string unquoted = trimmed.Trim('"', '\'');
            if (File.Exists(unquoted))
            {
                return (unquoted, "");
            }

            // Durum C: İçinde bilinen bir dosya uzantısı (.exe, .bat, .cmd, .ps1) geçiyorsa
            string[] knownExtensions = new[] { ".exe", ".bat", ".cmd", ".ps1" };
            foreach (var ext in knownExtensions)
            {
                int extIdx = trimmed.IndexOf(ext, StringComparison.OrdinalIgnoreCase);
                if (extIdx >= 0)
                {
                    int cutPoint = extIdx + ext.Length;
                    string candidatePath = trimmed.Substring(0, cutPoint).Trim('"', '\'', ' ');
                    string candidateArgs = trimmed.Substring(cutPoint).Trim('"', '\'', ' ');

                    if (File.Exists(candidatePath))
                    {
                        return (candidatePath, candidateArgs);
                    }
                }
            }

            // Durum D: İlk boşluğa göre ayır
            int firstSpace = trimmed.IndexOf(' ');
            if (firstSpace > 0)
            {
                string firstPart = trimmed.Substring(0, firstSpace).Trim('"', '\'');
                string secondPart = trimmed.Substring(firstSpace + 1).Trim();
                if (File.Exists(firstPart))
                {
                    return (firstPart, secondPart);
                }
            }

            return (unquoted, "");
        }
    }
}
