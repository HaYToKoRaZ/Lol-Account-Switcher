using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LolAccountSwitcher
{
    /// <summary>
    /// League of Legends yardımcı uygulamalarını (Blitz, Porofessor, Mobalytics, OP.GG, Zar vb.)
    /// otomatik algılayan ve hesap geçişi sonrası başlatan servis.
    /// </summary>
    public static class CompanionAppService
    {
        public record CompanionAppInfo(string Name, string ExecutablePath);

        /// <summary>
        /// Sistemde kurulu olabilecek bilinen tüm yardımcı uygulamaları sırayla tarar ve ilk bulunanı döner.
        /// </summary>
        public static CompanionAppInfo? DetectCompanionApp()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string userName = Environment.UserName;

            var candidates = new List<(string Name, string[] Paths)>
            {
                // 1. Blitz.gg
                ("Blitz.gg", new[]
                {
                    Path.Combine(localAppData, "Programs", "Blitz", "Blitz.exe"),
                    Path.Combine(appData, "Blitz", "Blitz.exe"),
                    Path.Combine(programFiles, "Blitz", "Blitz.exe"),
                    Path.Combine(programFilesX86, "Blitz", "Blitz.exe"),
                    $@"C:\Users\{userName}\AppData\Local\Programs\Blitz\Blitz.exe"
                }),

                // 2. Porofessor / Overwolf
                ("Porofessor", new[]
                {
                    Path.Combine(localAppData, "Programs", "Porofessor", "Porofessor.exe"),
                    Path.Combine(localAppData, "Porofessor", "Porofessor.exe"),
                    Path.Combine(localAppData, "Overwolf", "OverwolfLauncher.exe"),
                    Path.Combine(programFilesX86, "Overwolf", "OverwolfLauncher.exe"),
                    Path.Combine(programFiles, "Overwolf", "OverwolfLauncher.exe")
                }),

                // 3. Mobalytics
                ("Mobalytics", new[]
                {
                    Path.Combine(localAppData, "Programs", "Mobalytics", "Mobalytics.exe"),
                    Path.Combine(localAppData, "Mobalytics", "Mobalytics.exe"),
                    Path.Combine(programFiles, "Mobalytics", "Mobalytics.exe"),
                    Path.Combine(programFilesX86, "Mobalytics", "Mobalytics.exe")
                }),

                // 4. OP.GG
                ("OP.GG", new[]
                {
                    Path.Combine(localAppData, "Programs", "OP.GG", "OP.GG.exe"),
                    Path.Combine(localAppData, "opgg-electron-app", "OP.GG.exe"),
                    Path.Combine(programFiles, "OP.GG", "OP.GG.exe"),
                    Path.Combine(programFilesX86, "OP.GG", "OP.GG.exe")
                }),

                // 5. Zar.gg
                ("Zar.gg", new[]
                {
                    Path.Combine(localAppData, "Programs", "Zar", "Zar.exe"),
                    Path.Combine(localAppData, "Zar", "Zar.exe")
                }),

                // 6. SenpAI.GG
                ("SenpAI.GG", new[]
                {
                    Path.Combine(localAppData, "Programs", "SenpAI", "SenpAI.exe"),
                    Path.Combine(localAppData, "SenpAI", "SenpAI.exe")
                })
            };

            foreach (var candidate in candidates)
            {
                foreach (var path in candidate.Paths)
                {
                    if (File.Exists(path))
                    {
                        return new CompanionAppInfo(candidate.Name, path);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Belirtilen veya otomatik tespit edilen yardımcı uygulamayı başlatır.
        /// </summary>
        public static bool LaunchCompanionApp(string? customPath = null)
        {
            try
            {
                string targetPath = customPath?.Trim() ?? "";
                if (string.IsNullOrEmpty(targetPath) || !File.Exists(targetPath))
                {
                    var detected = DetectCompanionApp();
                    targetPath = detected?.ExecutablePath ?? "";
                }

                if (!string.IsNullOrEmpty(targetPath) && File.Exists(targetPath))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = targetPath,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(targetPath) ?? ""
                    };
                    Process.Start(psi);
                    return true;
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Belirtilen veya sistemde açık olan tüm League yardımcı uygulamalarını (Blitz, Porofessor, Mobalytics, OP.GG vb.) güvenle kapatır.
        /// </summary>
        public static void TerminateCompanionApp(string? customPath = null)
        {
            var targetProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Blitz",
                "Blitz App",
                "Porofessor",
                "Overwolf",
                "OverwolfLauncher",
                "OverwolfBrowser",
                "Mobalytics",
                "MobalyticsDesktop",
                "OP.GG",
                "opgg",
                "opgg-electron-app",
                "Zar",
                "SenpAI"
            };

            if (!string.IsNullOrEmpty(customPath))
            {
                try
                {
                    string customName = Path.GetFileNameWithoutExtension(customPath);
                    if (!string.IsNullOrEmpty(customName))
                    {
                        targetProcesses.Add(customName);
                    }
                }
                catch { }
            }

            foreach (var procName in targetProcesses)
            {
                try
                {
                    var processes = Process.GetProcessesByName(procName);
                    foreach (var p in processes)
                    {
                        try
                        {
                            p.Kill();
                            p.WaitForExit(800);
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }
    }
}
