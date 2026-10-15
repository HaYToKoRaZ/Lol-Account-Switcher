using System;
using System.IO;
using System.Windows.Media;

namespace LolAccountSwitcher
{
    /// <summary>
    /// Uygulama içi ses efektlerini (açılış hoş geldin sesi, hesap geçiş LoL sesi vb.) yöneten safkan WPF medya servisi.
    /// </summary>
    public static class SoundService
    {
        private static MediaPlayer? _player;

        /// <summary>
        /// Uygulama açıldığında hoş geldin sesini (sounds/startup.mp3) çalar.
        /// Kullanıcı ayardan kapattıysa çalmaz. Ses seviyesi kısık (%35) olarak ayarlanır.
        /// </summary>
        /// <param name="volume">0.0 ile 1.0 arasında ses seviyesi</param>
        public static void PlayStartupSound(double volume = 0.35)
        {
            try
            {
                bool enabled = !string.Equals(ConfigManager.Get("General", "PlayStartupSound", "true"), "false", StringComparison.OrdinalIgnoreCase);
                if (!enabled) return;

                PlaySoundFile("startup.mp3", "welcome.mp3", volume);
            }
            catch { }
        }

        /// <summary>
        /// Hesap geçişi tamamlandığında LoL sohbet bildirim sesini (sounds/switch.mp3) çalar.
        /// Ses seviyesi varsayılan %40 (0.40) olarak ayarlanır, kulak tırmalamaz.
        /// </summary>
        /// <param name="volume">0.0 ile 1.0 arasında ses seviyesi</param>
        public static void PlayAccountSwitchSound(double volume = 0.40)
        {
            try
            {
                PlaySoundFile("switch.mp3", "league_of_legends_chat.mp3", volume);
            }
            catch { }
        }

        private static void PlaySoundFile(string standardName, string legacyFallbackName, double volume)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string? resolvedPath = ResolveSoundPath(standardName, legacyFallbackName, baseDir);

            if (resolvedPath == null || !File.Exists(resolvedPath)) return;

            App.Current?.Dispatcher?.Invoke(() =>
            {
                if (_player == null)
                {
                    _player = new MediaPlayer();
                }
                else
                {
                    _player.Stop();
                    _player.Close();
                }

                _player.Open(new Uri(resolvedPath, UriKind.Absolute));
                _player.Volume = Math.Clamp(volume, 0.0, 1.0);
                _player.Play();
            });
        }

        private static string? ResolveSoundPath(string standardName, string legacyName, string baseDir)
        {
            var candidates = new[]
            {
                // 1. build/sounds/ veya uygulama/sounds/
                Path.Combine(baseDir, "sounds", standardName),
                // 2. Kök proje dizini/sounds/
                Path.Combine(Directory.GetParent(baseDir)?.FullName ?? "", "sounds", standardName),
                Path.Combine(Directory.GetParent(baseDir)?.Parent?.FullName ?? "", "sounds", standardName),
                // 3. Doğrudan dosya adı (build/ veya kök dizin)
                Path.Combine(baseDir, standardName),
                Path.Combine(baseDir, legacyName),
                Path.Combine(Directory.GetParent(baseDir)?.FullName ?? "", standardName),
                Path.Combine(Directory.GetParent(baseDir)?.FullName ?? "", legacyName),
                Path.Combine(Environment.CurrentDirectory, "sounds", standardName),
                Path.Combine(Environment.CurrentDirectory, standardName)
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
    }
}
