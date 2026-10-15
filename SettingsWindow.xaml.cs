using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
                    else if (string.IsNullOrEmpty(savedSwitchGuid) && (plan.Name.Contains("Yüksek", StringComparison.OrdinalIgnoreCase) || plan.Name.Contains("High", StringComparison.OrdinalIgnoreCase) || plan.Name.Contains("Ultimate", StringComparison.OrdinalIgnoreCase) || plan.Name.Contains("Nihai", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (switchSelectIdx == 0) switchSelectIdx = currentIdx;
                    }

                    if (!string.IsNullOrEmpty(savedKillGuid) && string.Equals(savedKillGuid, guidStr, StringComparison.OrdinalIgnoreCase))
                    {
                        killSelectIdx = currentIdx;
                    }
                    else if (string.IsNullOrEmpty(savedKillGuid) && (plan.Name.Contains("Dengeli", StringComparison.OrdinalIgnoreCase) || plan.Name.Contains("Balanced", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (killSelectIdx == 0) killSelectIdx = currentIdx;
                    }

                    currentIdx++;
                }

                CmbSwitchPowerPlan.SelectedIndex = switchSelectIdx;
                CmbKillLeaguePowerPlan.SelectedIndex = killSelectIdx;

                // Monitör Tazeleme Hızları (Hz)
                CmbSwitchRefreshRate.Items.Clear();
                CmbKillLeagueRefreshRate.Items.Clear();

                var rates = DisplayManager.GetSupportedRefreshRates();
                string savedSwitchHz = ConfigManager.Get("General", "SwitchRefreshRate", "144").Trim();
                string savedKillHz = ConfigManager.Get("General", "KillLeagueRefreshRate", "60").Trim();

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

                if (switchHzIdx == 0 && savedSwitchHz != "0" && !string.IsNullOrEmpty(savedSwitchHz) && rates.Count > 0)
                {
                    switchHzIdx = rates.Count;
                }
                if (killHzIdx == 0 && savedKillHz != "0" && !string.IsNullOrEmpty(savedKillHz))
                {
                    for (int i = 0; i < CmbKillLeagueRefreshRate.Items.Count; i++)
                    {
                        if ((CmbKillLeagueRefreshRate.Items[i] as ComboBoxItem)?.Tag as string == "60")
                        {
                            killHzIdx = i;
                            break;
                        }
                    }
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
            TxtThemeHeader.Text = LocalizationManager.Get("ThemeLabel");
            BtnToggleTheme.Content = LocalizationManager.Get("ToggleThemeBtn");
            TxtLangHeader.Text = LocalizationManager.Get("LanguageLabel");
            BtnToggleLang.Content = LocalizationManager.Get("ToggleLanguageBtn");
            TxtCurrentLang.Text = LocalizationManager.CurrentLanguage == "tr" ? " [TR / Türkçe]" : " [EN / English]";

            TxtCloseLeagueHeader.Text = LocalizationManager.Get("CloseLeagueOption");
            TxtCloseLeagueSub.Text = LocalizationManager.Get("CloseLeagueOptionDesc");
            TxtAutoLaunchHeader.Text = LocalizationManager.Get("AutoLaunchOption");
            TxtAutoLaunchSub.Text = LocalizationManager.Get("AutoLaunchOptionDesc");
            TxtAutoRefreshStatsHeader.Text = LocalizationManager.Get("AutoRefreshStatsOption");
            TxtAutoRefreshStatsSub.Text = LocalizationManager.Get("AutoRefreshStatsOptionDesc");
            TxtPlayStartupSoundHeader.Text = LocalizationManager.Get("PlayStartupSoundOption");
            TxtPlayStartupSoundSub.Text = LocalizationManager.Get("PlayStartupSoundOptionDesc");
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

        private void BtnToggleLang_Click(object sender, RoutedEventArgs e)
        {
            LocalizationManager.ToggleLanguage();
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
    }
}
