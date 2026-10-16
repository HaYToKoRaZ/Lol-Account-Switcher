using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LolAccountSwitcher
{
    public partial class SettingsWindow : Window
    {
        private MainWindow _main;

        private bool _isLoadingConfig = true;

        public SettingsWindow(MainWindow main)
        {
            InitializeComponent();
            _main = main;
            Owner = main;

            LoadSettingsAndPowerPlans();

            LocalizationManager.OnLanguageChanged += UpdateLocalization;
            UpdateLocalization();

            Loaded += (s, e) =>
            {
                double maxAvail = SystemParameters.WorkArea.Height - 30;
                if (Height > maxAvail)
                {
                    Height = maxAvail;
                    Top = Math.Max(0, (SystemParameters.WorkArea.Height - Height) / 2);
                }
            };
        }

        private void LoadSettingsAndPowerPlans()
        {
            _isLoadingConfig = true;
            try
            {
                // CloseLeagueOnSwitch
                string closeVal = ConfigManager.Get("General", "CloseLeagueOnSwitch", "true");
                ChkCloseLeague.IsChecked = !string.Equals(closeVal, "false", StringComparison.OrdinalIgnoreCase);

                // AutoLaunchGameOnSwitch
                string autoLaunchVal = ConfigManager.Get("General", "AutoLaunchGameOnSwitch", "true");
                ChkAutoLaunchGame.IsChecked = !string.Equals(autoLaunchVal, "false", StringComparison.OrdinalIgnoreCase);

                // AutoRefreshStats
                string autoRefreshVal = ConfigManager.Get("General", "AutoRefreshStats", "true");
                ChkAutoRefreshStats.IsChecked = !string.Equals(autoRefreshVal, "false", StringComparison.OrdinalIgnoreCase);

                // PlayStartupSound
                string playStartupSoundVal = ConfigManager.Get("General", "PlayStartupSound", "true");
                ChkPlayStartupSound.IsChecked = !string.Equals(playStartupSoundVal, "false", StringComparison.OrdinalIgnoreCase);

                // MinimizeToTray
                string trayVal = ConfigManager.Get("General", "MinimizeToTray", "true");
                ChkMinimizeToTray.IsChecked = !string.Equals(trayVal, "false", StringComparison.OrdinalIgnoreCase);

                // MaskEmailInUI
                string maskVal = ConfigManager.Get("General", "MaskEmailInUI", "true");
                ChkMaskEmailInUI.IsChecked = !string.Equals(maskVal, "false", StringComparison.OrdinalIgnoreCase);

                // ShowAppDataButton
                string showAppDataVal = ConfigManager.Get("General", "ShowAppDataButton", "false");
                ChkShowAppData.IsChecked = string.Equals(showAppDataVal, "true", StringComparison.OrdinalIgnoreCase);

                // ShowHzBadge (default: true)
                string showHzVal = ConfigManager.Get("General", "ShowHzBadge", "true");
                ChkShowHzBadge.IsChecked = !string.Equals(showHzVal, "false", StringComparison.OrdinalIgnoreCase);

                // ShowLanguageToggle (default: true)
                string showLangVal = ConfigManager.Get("General", "ShowLanguageToggle", "true");
                ChkShowLangToggle.IsChecked = !string.Equals(showLangVal, "false", StringComparison.OrdinalIgnoreCase);

                // ShowThemeToggle (default: true)
                string showThemeVal = ConfigManager.Get("General", "ShowThemeToggle", "true");
                ChkShowThemeToggle.IsChecked = !string.Equals(showThemeVal, "false", StringComparison.OrdinalIgnoreCase);

                // ShowRefreshStatsButton (default: true)
                string showRefreshVal = ConfigManager.Get("General", "ShowRefreshStatsButton", "true");
                ChkShowRefreshStats.IsChecked = !string.Equals(showRefreshVal, "false", StringComparison.OrdinalIgnoreCase);

                // ShowWikiButton (default: true)
                string showWikiVal = ConfigManager.Get("General", "ShowWikiButton", "true");
                ChkShowWiki.IsChecked = !string.Equals(showWikiVal, "false", StringComparison.OrdinalIgnoreCase);

                // ShowPatchNotesButton (default: true)
                string showPatchNotesVal = ConfigManager.Get("General", "ShowPatchNotesButton", "true");
                ChkShowPatchNotes.IsChecked = !string.Equals(showPatchNotesVal, "false", StringComparison.OrdinalIgnoreCase);

                // LaunchCompanionAppOnSwitch (fallback to LaunchBlitzOnSwitch)
                string launchCompanionVal = ConfigManager.Get("General", "LaunchCompanionAppOnSwitch", ConfigManager.Get("General", "LaunchBlitzOnSwitch", "false"));
                ChkLaunchCompanionApp.IsChecked = string.Equals(launchCompanionVal, "true", StringComparison.OrdinalIgnoreCase);

                // Power Plans
                var plans = PowerManager.GetPowerPlans();
                string savedSwitchGuid = ConfigManager.Get("General", "SwitchPowerPlanGuid", "").Trim();
                string savedKillGuid = ConfigManager.Get("General", "KillLeaguePowerPlanGuid", "").Trim();

                CmbSwitchPowerPlan.Items.Clear();
                CmbKillLeaguePowerPlan.Items.Clear();

                // Varsayılan öğesi
                var defaultItem1 = new ComboBoxItem { Content = LocalizationManager.Get("PowerPlanAuto"), Tag = "" };
                var defaultItem2 = new ComboBoxItem { Content = LocalizationManager.Get("PowerPlanAuto"), Tag = "" };
                CmbSwitchPowerPlan.Items.Add(defaultItem1);
                CmbKillLeaguePowerPlan.Items.Add(defaultItem2);

                int switchSelectIdx = 0;
                int killSelectIdx = 0;
                int currentIdx = 1;

                foreach (var plan in plans)
                {
                    string guidStr = plan.Guid.ToString();
                    string label = plan.IsActive ? $"{plan.Name} (Aktif / Active)" : plan.Name;

                    var itemSwitch = new ComboBoxItem { Content = label, Tag = guidStr };
                    var itemKill = new ComboBoxItem { Content = label, Tag = guidStr };

                    CmbSwitchPowerPlan.Items.Add(itemSwitch);
                    CmbKillLeaguePowerPlan.Items.Add(itemKill);

                    if (!string.IsNullOrEmpty(savedSwitchGuid) && string.Equals(savedSwitchGuid, guidStr, StringComparison.OrdinalIgnoreCase))
                    {
                        switchSelectIdx = currentIdx;
                    }

                    if (!string.IsNullOrEmpty(savedKillGuid) && string.Equals(savedKillGuid, guidStr, StringComparison.OrdinalIgnoreCase))
                    {
                        killSelectIdx = currentIdx;
                    }

                    currentIdx++;
                }

                CmbSwitchPowerPlan.SelectedIndex = switchSelectIdx;
                CmbKillLeaguePowerPlan.SelectedIndex = killSelectIdx;

                // Monitör Tazeleme Hızları (Hz)
                CmbSwitchRefreshRate.Items.Clear();
                CmbKillLeagueRefreshRate.Items.Clear();

                var rates = DisplayManager.GetSupportedRefreshRates();
                string savedSwitchHz = ConfigManager.Get("General", "SwitchRefreshRate", "0").Trim();
                string savedKillHz = ConfigManager.Get("General", "KillLeagueRefreshRate", "0").Trim();

                var defaultSwitchItem = new ComboBoxItem { Content = LocalizationManager.Get("RefreshRateAuto"), Tag = "0" };
                var defaultKillItem = new ComboBoxItem { Content = LocalizationManager.Get("RefreshRateAuto"), Tag = "0" };
                CmbSwitchRefreshRate.Items.Add(defaultSwitchItem);
                CmbKillLeagueRefreshRate.Items.Add(defaultKillItem);

                int switchHzIdx = 0;
                int killHzIdx = 0;
                int hzCounter = 1;

                foreach (var hz in rates)
                {
                    string hzStr = hz.ToString();
                    var itemS = new ComboBoxItem { Content = $"{hz} Hz", Tag = hzStr };
                    var itemK = new ComboBoxItem { Content = $"{hz} Hz", Tag = hzStr };
                    CmbSwitchRefreshRate.Items.Add(itemS);
                    CmbKillLeagueRefreshRate.Items.Add(itemK);

                    if (savedSwitchHz == hzStr) switchHzIdx = hzCounter;
                    if (savedKillHz == hzStr) killHzIdx = hzCounter;

                    hzCounter++;
                }

                CmbSwitchRefreshRate.SelectedIndex = switchHzIdx >= 0 && switchHzIdx < CmbSwitchRefreshRate.Items.Count ? switchHzIdx : 0;
                CmbKillLeagueRefreshRate.SelectedIndex = killHzIdx >= 0 && killHzIdx < CmbKillLeagueRefreshRate.Items.Count ? killHzIdx : 0;

                // 1. Riot Games Kurulum Klasörü
                string savedRiotPath = ConfigManager.Get("General", "RiotGamesPath", "").Trim();
                if (string.IsNullOrEmpty(savedRiotPath))
                {
                    savedRiotPath = RiotPathDetector.DetectRiotGamesRoot();
                    if (!string.IsNullOrEmpty(savedRiotPath))
                    {
                        ConfigManager.Set("General", "RiotGamesPath", savedRiotPath);
                    }
                }
                TxtRiotGamesPath.Text = savedRiotPath;

                // 2. Riot İstemci & Profil Veri Klasörü (AppData)
                string savedProfilePath = ConfigManager.Get("General", "RiotProfilePath", "").Trim();
                if (string.IsNullOrEmpty(savedProfilePath))
                {
                    savedProfilePath = RiotPathDetector.DetectProfileDataPath();
                    if (!string.IsNullOrEmpty(savedProfilePath))
                    {
                        ConfigManager.Set("General", "RiotProfilePath", savedProfilePath);
                    }
                }
                TxtRiotProfilePath.Text = savedProfilePath;

                // 3. Yardımcı Uygulama (.exe) Yolu (Blitz, Porofessor, Mobalytics vb.)
                string savedCompanionPath = ConfigManager.Get("General", "CompanionAppPath", ConfigManager.Get("General", "BlitzPath", "")).Trim();
                if (string.IsNullOrEmpty(savedCompanionPath))
                {
                    var detected = CompanionAppService.DetectCompanionApp();
                    if (detected != null && !string.IsNullOrEmpty(detected.ExecutablePath))
                    {
                        savedCompanionPath = detected.ExecutablePath;
                        ConfigManager.Set("General", "CompanionAppPath", savedCompanionPath);
                        ConfigManager.Set("General", "BlitzPath", savedCompanionPath);
                    }
                }
                TxtCompanionAppPath.Text = savedCompanionPath;

                // 4. Özel Komut & Betik Çalıştırma (CLI / CMD / Exe)
                TxtPreSwitchCommand.Text = ConfigManager.Get("General", "PreSwitchCommand", "").Trim();
                TxtPostSwitchCommand.Text = ConfigManager.Get("General", "PostSwitchCommand", "").Trim();
                TxtKillLeagueCommand.Text = ConfigManager.Get("General", "KillLeagueCommand", "").Trim();
            }
            finally
            {
                _isLoadingConfig = false;
            }
        }

        private void ChkCloseLeague_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "CloseLeagueOnSwitch", ChkCloseLeague.IsChecked == true ? "true" : "false");
        }

        private void ChkAutoLaunchGame_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "AutoLaunchGameOnSwitch", ChkAutoLaunchGame.IsChecked == true ? "true" : "false");
        }

        private void ChkAutoRefreshStats_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "AutoRefreshStats", ChkAutoRefreshStats.IsChecked == true ? "true" : "false");
        }

        private void ChkPlayStartupSound_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "PlayStartupSound", ChkPlayStartupSound.IsChecked == true ? "true" : "false");
        }

        private void ChkMinimizeToTray_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "MinimizeToTray", ChkMinimizeToTray.IsChecked == true ? "true" : "false");
        }

        private void ChkMaskEmailInUI_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "MaskEmailInUI", ChkMaskEmailInUI.IsChecked == true ? "true" : "false");
            if (_main?.Accounts != null)
            {
                foreach (var acc in _main.Accounts)
                {
                    acc.RefreshLocalization();
                }
            }
        }

        private void ChkShowAppData_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "ShowAppDataButton", ChkShowAppData.IsChecked == true ? "true" : "false");
            _main?.UpdateToolbarsVisibility();
        }

        private void ChkShowHzBadge_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "ShowHzBadge", ChkShowHzBadge.IsChecked == true ? "true" : "false");
            _main?.UpdateToolbarsVisibility();
        }

        private void ChkShowLangToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "ShowLanguageToggle", ChkShowLangToggle.IsChecked == true ? "true" : "false");
            _main?.UpdateToolbarsVisibility();
        }

        private void ChkShowThemeToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "ShowThemeToggle", ChkShowThemeToggle.IsChecked == true ? "true" : "false");
            _main?.UpdateToolbarsVisibility();
        }

        private void ChkShowRefreshStats_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "ShowRefreshStatsButton", ChkShowRefreshStats.IsChecked == true ? "true" : "false");
            _main?.UpdateToolbarsVisibility();
        }

        private void ChkShowWiki_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "ShowWikiButton", ChkShowWiki.IsChecked == true ? "true" : "false");
            _main?.UpdateToolbarsVisibility();
        }

        private void ChkShowPatchNotes_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "ShowPatchNotesButton", ChkShowPatchNotes.IsChecked == true ? "true" : "false");
            _main?.UpdateToolbarsVisibility();
        }

        private void ChkLaunchCompanionApp_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoadingConfig) return;
            string val = ChkLaunchCompanionApp.IsChecked == true ? "true" : "false";
            ConfigManager.Set("General", "LaunchCompanionAppOnSwitch", val);
            ConfigManager.Set("General", "LaunchBlitzOnSwitch", val);
        }

        private void CmbSwitchPowerPlan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingConfig) return;
            if (CmbSwitchPowerPlan.SelectedItem is ComboBoxItem item)
            {
                string guid = (item.Tag as string) ?? "";
                ConfigManager.Set("General", "SwitchPowerPlanGuid", guid);
            }
        }

        private void CmbKillLeaguePowerPlan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingConfig) return;
            if (CmbKillLeaguePowerPlan.SelectedItem is ComboBoxItem item)
            {
                string guid = (item.Tag as string) ?? "";
                ConfigManager.Set("General", "KillLeaguePowerPlanGuid", guid);
            }
        }

        private void CmbSwitchRefreshRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingConfig) return;
            if (CmbSwitchRefreshRate.SelectedItem is ComboBoxItem item)
            {
                string hz = (item.Tag as string) ?? "0";
                ConfigManager.Set("General", "SwitchRefreshRate", hz);
            }
        }

        private void CmbKillLeagueRefreshRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingConfig) return;
            if (CmbKillLeagueRefreshRate.SelectedItem is ComboBoxItem item)
            {
                string hz = (item.Tag as string) ?? "0";
                ConfigManager.Set("General", "KillLeagueRefreshRate", hz);
            }
        }

        private void TxtRiotGamesPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "RiotGamesPath", TxtRiotGamesPath.Text.Trim());
        }

        private void TxtRiotProfilePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "RiotProfilePath", TxtRiotProfilePath.Text.Trim());
        }

        private void BtnDetectRiotPath_Click(object sender, RoutedEventArgs e)
        {
            string detected = RiotPathDetector.DetectRiotGamesRoot();
            if (!string.IsNullOrEmpty(detected))
            {
                TxtRiotGamesPath.Text = detected;
                ConfigManager.Set("General", "RiotGamesPath", detected);
                MessageBox.Show($"{LocalizationManager.Get("PathDetectedSuccess")}\n\n{detected}", LocalizationManager.Get("AppTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnBrowseRiotPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Riot Games Klasörünü Seçin"
            };

            if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.FolderName))
            {
                TxtRiotGamesPath.Text = dialog.FolderName;
                ConfigManager.Set("General", "RiotGamesPath", dialog.FolderName);
            }
        }

        private void BtnDetectProfilePath_Click(object sender, RoutedEventArgs e)
        {
            string detected = RiotPathDetector.DetectProfileDataPath();
            if (!string.IsNullOrEmpty(detected))
            {
                TxtRiotProfilePath.Text = detected;
                ConfigManager.Set("General", "RiotProfilePath", detected);
                MessageBox.Show($"{LocalizationManager.Get("PathDetectedSuccess")}\n\n{detected}", LocalizationManager.Get("AppTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnBrowseProfilePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Riot Games Profil Klasörünü (AppData) Seçin"
            };

            if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.FolderName))
            {
                TxtRiotProfilePath.Text = dialog.FolderName;
                ConfigManager.Set("General", "RiotProfilePath", dialog.FolderName);
            }
        }

        private void TxtCompanionAppPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingConfig) return;
            string path = TxtCompanionAppPath.Text.Trim();
            ConfigManager.Set("General", "CompanionAppPath", path);
            ConfigManager.Set("General", "BlitzPath", path);
        }

        private void BtnDetectCompanionAppPath_Click(object sender, RoutedEventArgs e)
        {
            var detected = CompanionAppService.DetectCompanionApp();
            if (detected != null && !string.IsNullOrEmpty(detected.ExecutablePath))
            {
                TxtCompanionAppPath.Text = detected.ExecutablePath;
                ConfigManager.Set("General", "CompanionAppPath", detected.ExecutablePath);
                ConfigManager.Set("General", "BlitzPath", detected.ExecutablePath);
                MessageBox.Show(LocalizationManager.Get("CompanionAppDetectedSuccess", detected.Name, detected.ExecutablePath), LocalizationManager.Get("AppTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(LocalizationManager.Get("CompanionAppNotFound"), LocalizationManager.Get("AppTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnBrowseCompanionAppPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Yardımcı Uygulama (.exe) Seçin",
                Filter = "Uygulama Dosyaları (*.exe)|*.exe|Tüm Dosyalar (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.FileName))
            {
                TxtCompanionAppPath.Text = dialog.FileName;
                ConfigManager.Set("General", "CompanionAppPath", dialog.FileName);
                ConfigManager.Set("General", "BlitzPath", dialog.FileName);
            }
        }

        private void UpdateLocalization()
        {
            TxtSettingsHeader.Text = LocalizationManager.Get("SettingsTitle");
            if (TxtCardHeaderAppearance != null) TxtCardHeaderAppearance.Text = LocalizationManager.Get("CardHeaderAppearance");
            if (TxtCardHeaderAutomation != null) TxtCardHeaderAutomation.Text = LocalizationManager.Get("CardHeaderAutomation");
            if (TxtCardHeaderVisibility != null) TxtCardHeaderVisibility.Text = LocalizationManager.Get("CardHeaderVisibility");
            if (TxtCardHeaderHardware != null) TxtCardHeaderHardware.Text = LocalizationManager.Get("CardHeaderHardware");
            if (TxtCardHeaderPaths != null) TxtCardHeaderPaths.Text = LocalizationManager.Get("CardHeaderPaths");

            if (BtnFooterPortal != null) BtnFooterPortal.ToolTip = LocalizationManager.Get("PortalTooltip");
            if (BtnFooterWeb != null) BtnFooterWeb.ToolTip = LocalizationManager.Get("WebTooltip");
            if (BtnFooterTwitter != null) BtnFooterTwitter.ToolTip = LocalizationManager.Get("TwitterTooltip");

            TxtThemeHeader.Text = LocalizationManager.Get("ThemeLabel");
            BtnToggleTheme.Content = LocalizationManager.Get("ToggleThemeBtn");
            TxtLangHeader.Text = LocalizationManager.Get("LanguageLabel");
            if (LocalizationManager.CurrentLanguage == "tr")
            {
                BtnSettingsLangTR.Opacity = 1.0;
                BtnSettingsLangTR.BorderBrush = (Brush)FindResource("BrushHextechGold");
                BtnSettingsLangEN.Opacity = 0.55;
                BtnSettingsLangEN.BorderBrush = (Brush)FindResource("BrushBorderSubtle");
                TxtCurrentLang.Text = LocalizationManager.Get("SelectedLangTR");
            }
            else
            {
                BtnSettingsLangEN.Opacity = 1.0;
                BtnSettingsLangEN.BorderBrush = (Brush)FindResource("BrushHextechCyanGlow");
                BtnSettingsLangTR.Opacity = 0.55;
                BtnSettingsLangTR.BorderBrush = (Brush)FindResource("BrushBorderSubtle");
                TxtCurrentLang.Text = LocalizationManager.Get("SelectedLangEN");
            }

            TxtCloseLeagueHeader.Text = LocalizationManager.Get("CloseLeagueOption");
            TxtCloseLeagueSub.Text = LocalizationManager.Get("CloseLeagueOptionDesc");
            TxtAutoLaunchHeader.Text = LocalizationManager.Get("AutoLaunchOption");
            TxtAutoLaunchSub.Text = LocalizationManager.Get("AutoLaunchOptionDesc");
            TxtAutoRefreshStatsHeader.Text = LocalizationManager.Get("AutoRefreshStatsOption");
            TxtAutoRefreshStatsSub.Text = LocalizationManager.Get("AutoRefreshStatsOptionDesc");
            TxtPlayStartupSoundHeader.Text = LocalizationManager.Get("PlayStartupSoundOption");
            TxtPlayStartupSoundSub.Text = LocalizationManager.Get("PlayStartupSoundOptionDesc");
            TxtMinimizeToTrayHeader.Text = LocalizationManager.Get("MinimizeToTrayOption");
            TxtMinimizeToTraySub.Text = LocalizationManager.Get("MinimizeToTrayOptionDesc");
            if (TxtMaskEmailHeader != null) TxtMaskEmailHeader.Text = LocalizationManager.Get("MaskEmailOption");
            if (TxtMaskEmailSub != null) TxtMaskEmailSub.Text = LocalizationManager.Get("MaskEmailOptionDesc");

            if (TxtShowHzBadgeHeader != null) TxtShowHzBadgeHeader.Text = LocalizationManager.Get("ShowHzBadgeOption");
            if (TxtShowHzBadgeSub != null) TxtShowHzBadgeSub.Text = LocalizationManager.Get("ShowHzBadgeOptionDesc");

            if (TxtShowLangToggleHeader != null) TxtShowLangToggleHeader.Text = LocalizationManager.Get("ShowLanguageToggleOption");
            if (TxtShowLangToggleSub != null) TxtShowLangToggleSub.Text = LocalizationManager.Get("ShowLanguageToggleOptionDesc");

            if (TxtShowThemeToggleHeader != null) TxtShowThemeToggleHeader.Text = LocalizationManager.Get("ShowThemeToggleOption");
            if (TxtShowThemeToggleSub != null) TxtShowThemeToggleSub.Text = LocalizationManager.Get("ShowThemeToggleOptionDesc");

            if (TxtShowRefreshStatsHeader != null) TxtShowRefreshStatsHeader.Text = LocalizationManager.Get("ShowRefreshStatsOption");
            if (TxtShowRefreshStatsSub != null) TxtShowRefreshStatsSub.Text = LocalizationManager.Get("ShowRefreshStatsOptionDesc");

            if (TxtShowWikiHeader != null) TxtShowWikiHeader.Text = LocalizationManager.Get("ShowWikiOption");
            if (TxtShowWikiSub != null) TxtShowWikiSub.Text = LocalizationManager.Get("ShowWikiOptionDesc");

            if (TxtShowPatchNotesHeader != null) TxtShowPatchNotesHeader.Text = LocalizationManager.Get("ShowPatchNotesOption");
            if (TxtShowPatchNotesSub != null) TxtShowPatchNotesSub.Text = LocalizationManager.Get("ShowPatchNotesOptionDesc");

            if (TxtShowAppDataHeader != null) TxtShowAppDataHeader.Text = LocalizationManager.Get("ShowAppDataOption");
            if (TxtShowAppDataSub != null) TxtShowAppDataSub.Text = LocalizationManager.Get("ShowAppDataOptionDesc");

            TxtLaunchCompanionAppHeader.Text = LocalizationManager.Get("LaunchCompanionAppOption");
            TxtLaunchCompanionAppSub.Text = LocalizationManager.Get("LaunchCompanionAppOptionDesc");
            TxtSwitchPowerPlan.Text = LocalizationManager.Get("SwitchPowerPlanLabel");
            TxtKillLeaguePowerPlan.Text = LocalizationManager.Get("KillLeaguePowerPlanLabel");
            TxtSwitchRefreshRate.Text = LocalizationManager.Get("SwitchRefreshRateLabel");
            TxtKillLeagueRefreshRate.Text = LocalizationManager.Get("KillLeagueRefreshRateLabel");

            TxtRiotGamesPathLabel.Text = LocalizationManager.Get("RiotGamesPathLabel");
            BtnDetectRiotPath.Content = LocalizationManager.Get("DetectPathBtn");
            BtnBrowseRiotPath.Content = LocalizationManager.Get("BrowsePathBtn");

            TxtRiotProfilePathLabel.Text = LocalizationManager.Get("RiotProfilePathLabel");
            BtnDetectProfilePath.Content = LocalizationManager.Get("DetectPathBtn");
            BtnBrowseProfilePath.Content = LocalizationManager.Get("BrowsePathBtn");

            TxtCompanionAppPathLabel.Text = LocalizationManager.Get("CompanionAppPathLabel");
            BtnDetectCompanionAppPath.Content = LocalizationManager.Get("DetectPathBtn");
            BtnBrowseCompanionAppPath.Content = LocalizationManager.Get("BrowsePathBtn");

            TxtCustomScriptHeader.Text = LocalizationManager.Get("CustomScriptHeader");
            TxtPreSwitchCommandLabel.Text = LocalizationManager.Get("PreSwitchCommandLabel");
            TxtPreSwitchCommand.ToolTip = LocalizationManager.Get("PreSwitchCommandTooltip");
            TxtPostSwitchCommandLabel.Text = LocalizationManager.Get("PostSwitchCommandLabel");
            TxtPostSwitchCommand.ToolTip = LocalizationManager.Get("PostSwitchCommandTooltip");
            TxtKillLeagueCommandLabel.Text = LocalizationManager.Get("KillLeagueCommandLabel");
            TxtKillLeagueCommand.ToolTip = LocalizationManager.Get("KillLeagueCommandTooltip");

            if (CmbSwitchPowerPlan.Items.Count > 0 && CmbSwitchPowerPlan.Items[0] is ComboBoxItem firstSwitch)
            {
                firstSwitch.Content = LocalizationManager.Get("PowerPlanAuto");
            }
            if (CmbKillLeaguePowerPlan.Items.Count > 0 && CmbKillLeaguePowerPlan.Items[0] is ComboBoxItem firstKill)
            {
                firstKill.Content = LocalizationManager.Get("PowerPlanAuto");
            }
            if (CmbSwitchRefreshRate.Items.Count > 0 && CmbSwitchRefreshRate.Items[0] is ComboBoxItem firstSwitchHz)
            {
                firstSwitchHz.Content = LocalizationManager.Get("RefreshRateAuto");
            }
            if (CmbKillLeagueRefreshRate.Items.Count > 0 && CmbKillLeagueRefreshRate.Items[0] is ComboBoxItem firstKillHz)
            {
                firstKillHz.Content = LocalizationManager.Get("RefreshRateAuto");
            }

            BtnCloseBottom.Content = LocalizationManager.Get("CloseBtn");

            UpdateThemeLabel();
        }

        private void UpdateThemeLabel()
        {
            TxtCurrentTheme.Text = ThemeManager.IsDarkMode 
                ? LocalizationManager.Get("ThemeDark") 
                : LocalizationManager.Get("ThemeLight");
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.ToggleTheme();
            UpdateThemeLabel();
        }

        private void BtnSettingsLangTR_Click(object sender, RoutedEventArgs e)
        {
            LocalizationManager.SetLanguage("tr");
        }

        private void BtnSettingsLangEN_Click(object sender, RoutedEventArgs e)
        {
            LocalizationManager.SetLanguage("en");
        }

        private void BtnOpenUrl_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.Tag is string url && !string.IsNullOrEmpty(url))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
                }
                catch { }
            }
        }

        private void TxtPreSwitchCommand_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "PreSwitchCommand", TxtPreSwitchCommand.Text.Trim());
        }

        private void TxtPostSwitchCommand_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "PostSwitchCommand", TxtPostSwitchCommand.Text.Trim());
        }

        private void TxtKillLeagueCommand_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingConfig) return;
            ConfigManager.Set("General", "KillLeagueCommand", TxtKillLeagueCommand.Text.Trim());
        }

        private void BtnBrowsePreSwitch_Click(object sender, RoutedEventArgs e)
        {
            BrowseCommandFile(TxtPreSwitchCommand);
        }

        private void BtnBrowsePostSwitch_Click(object sender, RoutedEventArgs e)
        {
            BrowseCommandFile(TxtPostSwitchCommand);
        }

        private void BtnBrowseKillLeagueCommand_Click(object sender, RoutedEventArgs e)
        {
            BrowseCommandFile(TxtKillLeagueCommand);
        }

        private void BrowseCommandFile(TextBox targetBox)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Çalıştırılacak Dosya veya Betiği Seçin",
                Filter = "Çalıştırılabilir Dosyalar (*.exe;*.bat;*.cmd;*.ps1)|*.exe;*.bat;*.cmd;*.ps1|Tüm Dosyalar (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.FileName))
            {
                targetBox.Text = dialog.FileName;
            }
        }

        private async void BtnTestPreSwitch_Click(object sender, RoutedEventArgs e)
        {
            await TestCommandAsync(TxtPreSwitchCommand.Text);
        }

        private async void BtnTestPostSwitch_Click(object sender, RoutedEventArgs e)
        {
            await TestCommandAsync(TxtPostSwitchCommand.Text);
        }

        private async void BtnTestKillLeagueCommand_Click(object sender, RoutedEventArgs e)
        {
            await TestCommandAsync(TxtKillLeagueCommand.Text);
        }

        private async Task TestCommandAsync(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd))
            {
                MessageBox.Show(LocalizationManager.Get("CommandTestEmpty"), LocalizationManager.Get("AppTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            await CustomScriptService.ExecuteCommandAsync(cmd);
            TrayService.ShowBalloonNotification(LocalizationManager.Get("AppTitle"), LocalizationManager.Get("CommandTestSuccess"));
        }
    }
}
