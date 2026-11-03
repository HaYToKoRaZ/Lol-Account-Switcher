using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Shell;

namespace LolAccountSwitcher
{
    public static class JumpListService
    {
        public static void UpdateJumpList(IEnumerable<AccountModel>? accounts)
        {
            try
            {
                var app = System.Windows.Application.Current;
                if (app == null) return;

                // Görev çubuğunda sabitlenen veya çalışan doğrudan hedef exe yolu
                string? targetExe = GetApplicationExecutablePath();
                if (string.IsNullOrEmpty(targetExe) || !File.Exists(targetExe)) return;

                string appIconPath = ResolveIconPath(targetExe);
                string workingDir = Path.GetDirectoryName(targetExe) ?? "";

                var jumpList = new JumpList
                {
                    ShowFrequentCategory = false,
                    ShowRecentCategory = false
                };

                // 1. EYLEM: Riot Servislerini Kapat
                string killTitle = LocalizationManager.Get("JumpListKillLeague") ?? "🛑 Riot Servislerini Kapat";
                string killDesc = LocalizationManager.Get("JumpListKillLeagueDesc") ?? "Tüm LoL ve Riot istemcilerini güvenle kapatır";
                var killTask = new JumpTask
                {
                    Title = killTitle,
                    Description = killDesc,
                    ApplicationPath = targetExe,
                    Arguments = "--kill-riot",
                    WorkingDirectory = workingDir
                };
                if (!string.IsNullOrEmpty(appIconPath) && File.Exists(appIconPath))
                {
                    killTask.IconResourcePath = appIconPath;
                    killTask.IconResourceIndex = 0;
                }
                jumpList.JumpItems.Add(killTask);

                // 2. EYLEM: Riot Launcher'ı Aç
                string launchTitle = LocalizationManager.Get("JumpListLaunchLeague") ?? "⚔️ Riot Launcher'ı Aç";
                string launchDesc = LocalizationManager.Get("JumpListLaunchLeagueDesc") ?? "Riot Games & LoL istemcisini başlatır";
                var launchTask = new JumpTask
                {
                    Title = launchTitle,
                    Description = launchDesc,
                    ApplicationPath = targetExe,
                    Arguments = "--launch-riot",
                    WorkingDirectory = workingDir
                };
                if (!string.IsNullOrEmpty(appIconPath) && File.Exists(appIconPath))
                {
                    launchTask.IconResourcePath = appIconPath;
                    launchTask.IconResourceIndex = 0;
                }
                jumpList.JumpItems.Add(launchTask);

                // 3. EYLEMLER: Kayıtlı Profiller
                if (accounts != null)
                {
                    foreach (var acc in accounts)
                    {
                        if (string.IsNullOrWhiteSpace(acc.Id)) continue;

                        string prefix = acc.IsActive ? "★ " : "• ";
                        string srv = !string.IsNullOrEmpty(acc.Server) ? $"[{acc.Server}] " : "";
                        string rank = !string.IsNullOrEmpty(acc.Rank) ? $" ({acc.Rank})" : "";
                        string title = $"{prefix}{srv}{acc.CleanSummonerName}{rank}".Trim();

                        string desc = string.Format(
                            LocalizationManager.Get("JumpListSwitchAccountDesc") ?? "{0} hesabına anında geçiş yap",
                            acc.CleanSummonerName
                        );

                        var accTask = new JumpTask
                        {
                            Title = title,
                            Description = desc,
                            ApplicationPath = targetExe,
                            Arguments = $"--switch-account \"{acc.Id}\"",
                            WorkingDirectory = workingDir
                        };

                        if (!string.IsNullOrEmpty(appIconPath) && File.Exists(appIconPath))
                        {
                            accTask.IconResourcePath = appIconPath;
                            accTask.IconResourceIndex = 0;
                        }

                        jumpList.JumpItems.Add(accTask);
                    }
                }

                JumpList.SetJumpList(app, jumpList);
                jumpList.Apply();
            }
            catch { }
        }

        private static string? GetApplicationExecutablePath()
        {
            try
            {
                // Görev çubuğunda sabitlenen kısayol (TaskBar shortcut) varsa onun TargetPath'ini öncelikli kullan
                string taskbarShortcut = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar\LolAccountSwitcher.lnk"
                );

                if (File.Exists(taskbarShortcut))
                {
                    try
                    {
                        dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell")!)!;
                        dynamic shortcut = shell.CreateShortcut(taskbarShortcut);
                        string target = (string)shortcut.TargetPath;
                        if (!string.IsNullOrEmpty(target) && File.Exists(target))
                        {
                            return target;
                        }
                    }
                    catch { }
                }

                // Aksi takdirde çalışan mevcut exe yolunu kullan
                string? currentExe = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
                return currentExe;
            }
            catch
            {
                return null;
            }
        }

        private static string ResolveIconPath(string targetExe)
        {
            try
            {
                string dir = Path.GetDirectoryName(targetExe) ?? "";
                string directIcon = Path.Combine(dir, "app.ico");
                if (File.Exists(directIcon)) return directIcon;

                string parentDir = Directory.GetParent(dir)?.FullName ?? "";
                string parentIcon = Path.Combine(parentDir, "app.ico");
                if (File.Exists(parentIcon)) return parentIcon;

                return targetExe;
            }
            catch
            {
                return targetExe;
            }
        }
    }
}
