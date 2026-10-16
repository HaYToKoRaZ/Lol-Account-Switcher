using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace LolAccountSwitcher
{
    public static class ThemeManager
    {
        public static bool IsDarkMode { get; private set; } = true;
        public static string CurrentThemeSetting { get; private set; } = "system";
        public static event Action? OnThemeChanged;

        public static void InitializeTheme()
        {
            CurrentThemeSetting = ConfigManager.Get("General", "Theme", "system").ToLowerInvariant();

            if (CurrentThemeSetting == "dark")
            {
                ApplyTheme(true, false);
            }
            else if (CurrentThemeSetting == "light")
            {
                ApplyTheme(false, false);
            }
            else
            {
                // Sistem rengini registry üzerinden algıla
                bool systemDark = true;
                try
                {
                    using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                    if (key != null)
                    {
                        object? registryValueObject = key.GetValue("AppsUseLightTheme");
                        if (registryValueObject is int lightThemeValue)
                        {
                            systemDark = (lightThemeValue == 0);
                        }
                    }
                }
                catch
                {
                    systemDark = true;
                }

                ApplyTheme(systemDark, false);
            }
        }

        public static void ToggleTheme()
        {
            ApplyTheme(!IsDarkMode, true);
        }

        public static void ApplyTheme(bool darkMode, bool saveToConfig = true)
        {
            IsDarkMode = darkMode;
            CurrentThemeSetting = darkMode ? "dark" : "light";

            if (saveToConfig)
            {
                ConfigManager.Set("General", "Theme", CurrentThemeSetting);
            }

            var res = Application.Current.Resources;

            if (darkMode)
            {
                // ==========================================
                // KOYU HEXTECH TEMASI (Dark Hextech)
                // ==========================================
                SetBrush(res, "BrushBgDeep", "#070C14");
                SetBrush(res, "BrushBgCard", "#0E1B2C");
                SetBrush(res, "BrushBgBadge", "#142940");
                SetBrush(res, "BrushBgAvatar", "#0A1B29");
                SetBrush(res, "BrushBorderSubtle", "#1E3958");
                SetBrush(res, "BrushBorderHover", "#C8AA6E");
                SetBrush(res, "BrushTextPrimary", "#F0E6D2");
                SetBrush(res, "BrushTextMuted", "#8E9DAE");
                SetBrush(res, "BrushHextechGold", "#C8AA6E");
                SetBrush(res, "BrushHextechGoldBright", "#F0E6D2");
                SetBrush(res, "BrushHextechCyan", "#0AC8B9");
                SetBrush(res, "BrushHextechCyanGlow", "#2BEBE0");
                SetBrush(res, "BrushToolBtnBg", "#112033");
                SetBrush(res, "BrushToolBtnHover", "#1A324E");
                SetBrush(res, "BrushSwitchBtnBg", "#092B45");
                SetBrush(res, "BrushSwitchBtnHover", "#0F446D");
                SetBrush(res, "BrushSwitchBtnText", "#2BEBE0");
                SetBrush(res, "BrushSwitchBtnBorder", "#0AC8B9");
                SetBrush(res, "BrushBackupBtnBg", "#1C2B18");
                SetBrush(res, "BrushBackupBtnHover", "#2D3E24");
                SetBrush(res, "BrushBackupBtnText", "#C8AA6E");
                SetBrush(res, "BrushBackupBtnBorder", "#C8AA6E");
                SetBrush(res, "BrushDanger", "#E84057");
                SetBrush(res, "BrushDangerBg", "#3D0E17");
                SetBrush(res, "BrushDangerHover", "#661424");
                SetBrush(res, "BrushDangerText", "#FFA4B2");
                SetBrush(res, "BrushInputBg", "#0D1A2D");
                SetBrush(res, "BrushInputText", "#F0E6D2");
            }
            else
            {
                // ==========================================
                // AÇIK PILTOVER / GÜMÜŞ TEMASI (Light Piltover - Refined Hextech)
                // ==========================================
                SetBrush(res, "BrushBgDeep", "#F4F6F9");
                SetBrush(res, "BrushBgCard", "#FFFFFF");
                SetBrush(res, "BrushBgBadge", "#E9EFF6");
                SetBrush(res, "BrushBgAvatar", "#DFE7F0");
                SetBrush(res, "BrushBorderSubtle", "#CBD7E6");
                SetBrush(res, "BrushBorderHover", "#A67C37");
                SetBrush(res, "BrushTextPrimary", "#0B1626");
                SetBrush(res, "BrushTextMuted", "#4F637A");
                SetBrush(res, "BrushHextechGold", "#A67C37");
                SetBrush(res, "BrushHextechGoldBright", "#7A5416");
                SetBrush(res, "BrushHextechCyan", "#02848A");
                SetBrush(res, "BrushHextechCyanGlow", "#016368");
                SetBrush(res, "BrushToolBtnBg", "#E9EFF6");
                SetBrush(res, "BrushToolBtnHover", "#DCE5F0");
                SetBrush(res, "BrushSwitchBtnBg", "#E0F4F5");
                SetBrush(res, "BrushSwitchBtnHover", "#CCEFF1");
                SetBrush(res, "BrushSwitchBtnText", "#01565A");
                SetBrush(res, "BrushSwitchBtnBorder", "#02848A");
                SetBrush(res, "BrushBackupBtnBg", "#FBF3E3");
                SetBrush(res, "BrushBackupBtnHover", "#F5E7CA");
                SetBrush(res, "BrushBackupBtnText", "#6E4F18");
                SetBrush(res, "BrushBackupBtnBorder", "#A67C37");
                SetBrush(res, "BrushDanger", "#D12A45");
                SetBrush(res, "BrushDangerBg", "#FEECEE");
                SetBrush(res, "BrushDangerHover", "#FCD4D8");
                SetBrush(res, "BrushDangerText", "#9E152B");
                SetBrush(res, "BrushInputBg", "#FFFFFF");
                SetBrush(res, "BrushInputText", "#0B1626");
            }

            // Sistem tepsisi sağ tık menüsünün temasını ve renklerini anında güncelle
            try
            {
                OnThemeChanged?.Invoke();
                TrayService.RebuildMenu();
            }
            catch { }
        }

        private static void SetBrush(ResourceDictionary res, string key, string hexColor)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
            brush.Freeze();
            res[key] = brush;
        }
    }
}
