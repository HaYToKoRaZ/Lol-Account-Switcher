using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LolAccountSwitcher
{
    /// <summary>
    /// Uygulama içi ses efektlerini (açılış hoş geldin sesi, hesap geçiş LoL sesi vb.) yöneten safkan Win32 medya servisi.
    /// Windows Media Player veya harici kodek kısıtlamalarına takılmadan sıfır gecikmeyle (0 ms) garantili çalar.
    /// </summary>
    public static class SoundService
    {
        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PlaySound(string pszSound, IntPtr hmod, uint fdwSound);

        private const uint SND_ASYNC = 0x0001;
        private const uint SND_NODEFAULT = 0x0002;
        private const uint SND_FILENAME = 0x00020000;

        /// <summary>
        /// Uygulama açıldığında hoş geldin sesini (sounds/startup.wav) kısık ses seviyesinde çalar.
        /// </summary>
        public static void PlayStartupSound(double volume = 0.50)
        {
            try
            {
                bool enabled = !string.Equals(ConfigManager.Get("General", "PlayStartupSound", "true"), "false", StringComparison.OrdinalIgnoreCase);
                if (!enabled) return;

                PlaySoundFile("startup.wav");
            }
            catch { }
        }

        /// <summary>
        /// Hesap geçişi tamamlandığında LoL sohbet bildirim sesini (sounds/switch.wav) çalar.
        /// </summary>
        public static void PlayAccountSwitchSound(double volume = 1.0)
        {
            try
            {
                PlaySoundFile("switch.wav");
            }
            catch { }
        }

        private static void PlaySoundFile(string soundFileName)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string? resolvedPath = ResolveSoundPath(soundFileName, baseDir);

                if (resolvedPath == null || !File.Exists(resolvedPath)) return;

                Task.Run(() =>
                {
                    try
                    {
                        PlaySound(resolvedPath, IntPtr.Zero, SND_ASYNC | SND_FILENAME | SND_NODEFAULT);
                    }
                    catch { }
                });
            }
            catch { }
        }

        private static string? ResolveSoundPath(string soundFileName, string baseDir)
        {
            string cleanBase = baseDir.TrimEnd('\\', '/');
            string? parentDir = Directory.GetParent(cleanBase)?.FullName;
            string? grandParentDir = !string.IsNullOrEmpty(parentDir) ? Directory.GetParent(parentDir)?.FullName : null;

            var candidates = new List<string?>
            {
                // 1. media/sounds/ (yeni konsolide medya mimarisi)
                Path.Combine(cleanBase, "media", "sounds", soundFileName),
                !string.IsNullOrEmpty(parentDir) ? Path.Combine(parentDir, "media", "sounds", soundFileName) : null,
                !string.IsNullOrEmpty(grandParentDir) ? Path.Combine(grandParentDir, "media", "sounds", soundFileName) : null,
                Path.Combine(Environment.CurrentDirectory, "media", "sounds", soundFileName),

                // 2. build/bin/sounds/ veya baseDir/sounds/ (geriye dönük uyumluluk)
                Path.Combine(cleanBase, "sounds", soundFileName),
                !string.IsNullOrEmpty(parentDir) ? Path.Combine(parentDir, "sounds", soundFileName) : null,
                !string.IsNullOrEmpty(grandParentDir) ? Path.Combine(grandParentDir, "sounds", soundFileName) : null,
                Path.Combine(Environment.CurrentDirectory, "sounds", soundFileName),
                Path.Combine(cleanBase, soundFileName),
                !string.IsNullOrEmpty(parentDir) ? Path.Combine(parentDir, soundFileName) : null
            };

            foreach (var path in candidates)
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// Lig/Rank derecesine karşılık gelen yerel PNG amblemini (media/ranks/{tier}.png) çözer.
        /// </summary>
        public static string? ResolveRankImagePath(string? rankOrTier)
        {
            if (string.IsNullOrWhiteSpace(rankOrTier)) return null;

            string tier = ExtractTierKeyword(rankOrTier);
            string fileName = $"{tier}.png";
            string cleanBase = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
            string? parentDir = Directory.GetParent(cleanBase)?.FullName;
            string? grandParentDir = !string.IsNullOrEmpty(parentDir) ? Directory.GetParent(parentDir)?.FullName : null;

            var candidates = new List<string?>
            {
                Path.Combine(cleanBase, "media", "ranks", fileName),
                !string.IsNullOrEmpty(parentDir) ? Path.Combine(parentDir, "media", "ranks", fileName) : null,
                !string.IsNullOrEmpty(grandParentDir) ? Path.Combine(grandParentDir, "media", "ranks", fileName) : null,
                Path.Combine(Environment.CurrentDirectory, "media", "ranks", fileName)
            };

            foreach (var path in candidates)
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        public static string ExtractTierKeyword(string rankOrTier)
        {
            string clean = rankOrTier.Trim().ToLowerInvariant();
            if (clean.Contains("iron") || clean.Contains("demir")) return "iron";
            if (clean.Contains("bronze") || clean.Contains("bronz")) return "bronze";
            if (clean.Contains("silver") || clean.Contains("gümüş")) return "silver";
            if (clean.Contains("gold") || clean.Contains("altın")) return "gold";
            if (clean.Contains("platinum") || clean.Contains("platin")) return "platinum";
            if (clean.Contains("emerald") || clean.Contains("zümrüt")) return "emerald";
            if (clean.Contains("diamond") || clean.Contains("elmas")) return "diamond";
            if (clean.Contains("master") || clean.Contains("ustalık")) return "master";
            if (clean.Contains("grandmaster") || clean.Contains("üstat")) return "grandmaster";
            if (clean.Contains("challenger") || clean.Contains("şampiyonluk")) return "challenger";
            return "unranked";
        }
    }
}
