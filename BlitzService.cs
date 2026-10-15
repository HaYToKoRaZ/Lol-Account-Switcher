using System;

namespace LolAccountSwitcher
{
    /// <summary>
    /// Geriye dönük uyumluluk köprüsü. Yeni geliştirmeler için CompanionAppService kullanılmalıdır.
    /// </summary>
    public static class BlitzService
    {
        public static string DetectBlitzPath()
        {
            return CompanionAppService.DetectCompanionApp()?.ExecutablePath ?? string.Empty;
        }

        public static bool LaunchBlitz(string? customPath = null)
        {
            return CompanionAppService.LaunchCompanionApp(customPath);
        }
    }
}
