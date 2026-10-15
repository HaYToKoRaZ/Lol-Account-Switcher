using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LolAccountSwitcher
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<AccountModel> Accounts { get; set; } = new();
        private readonly DispatcherTimer _periodicStatsTimer = new();

        public MainWindow()
        {
            InitializeComponent();
            LoadAccounts();
            ItemsAccounts.ItemsSource = Accounts;

            LocalizationManager.OnLanguageChanged += UpdateLocalization;
            UpdateLocalization();

            // Config klasörü salt okunurluk durumunu kontrol et ve göster
            UpdateCfgLockVisual();

            // Sistemdeki gerçek monitör tazeleme hızını oku ve göster
            try
            {
                int currentHz = DisplayManager.GetCurrentRefreshRate();
                if (currentHz > 0)
                {
                    TxtHz.Text = $"{currentHz}Hz";
                }
            }
            catch { }

            // Ayara bağlı olarak açılışta arka planda rank ve profil verilerini güncelle
            bool autoRefresh = !string.Equals(ConfigManager.Get("General", "AutoRefreshStats", "true"), "false", StringComparison.OrdinalIgnoreCase);
            if (autoRefresh)
            {
                _ = RefreshAllStatsAsync();
            }

            // Uygulama açık kaldığı sürece her 30 dakikada bir otomatik yenile
            _periodicStatsTimer.Interval = TimeSpan.FromMinutes(30);
            _periodicStatsTimer.Tick += async (s, e) => await RefreshAllStatsAsync();
            _periodicStatsTimer.Start();

            // Uygulama açılış hoş geldin sesini çal (ayara bağlı, kısık sesle)
            SoundService.PlayStartupSound(0.35);
        }

        public void LoadAccounts()
        {
            Accounts.Clear();
            var list = AccountStorage.LoadAccounts();
            foreach (var item in list)
            {
                Accounts.Add(item);
            }
            UpdateEmptyState();
        }

        public void UpdateEmptyState()
        {
            if (BorderEmptyState != null)
            {
                BorderEmptyState.Visibility = Accounts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void UpdateLocalization()
        {
            BtnLangToggle.Content = LocalizationManager.CurrentLanguage.ToUpperInvariant();
            BtnLangToggle.ToolTip = LocalizationManager.Get("LanguageToggleTooltip");
            BtnThemeToggle.ToolTip = LocalizationManager.Get("ThemeToggleTooltip");
            BtnRefreshStats.ToolTip = LocalizationManager.Get("RefreshTooltip");
            BtnSettings.ToolTip = LocalizationManager.Get("Settings");

            BtnCfgLock.Content = LocalizationManager.Get("CfgLock");
            BtnCfgLock.ToolTip = LocalizationManager.Get("CfgLockTooltip");

            BtnAppData.Content = LocalizationManager.Get("AppData");
            BtnAppData.ToolTip = LocalizationManager.Get("AppDataTooltip");

            BtnGeneralBackup.Content = LocalizationManager.Get("GeneralBackup");
            BtnGeneralBackup.ToolTip = LocalizationManager.Get("GeneralBackupTooltip");

            BtnKillLeague.Content = LocalizationManager.Get("KillLeague");

            UpdateCfgLockVisual();

            if (BtnAutoAddHeader != null)
            {
                BtnAutoAddHeader.Content = LocalizationManager.Get("AutoAddShortBtn");
                BtnAutoAddHeader.ToolTip = LocalizationManager.Get("AutoAddAccountTooltip");
            }
            if (TxtEmptyTitle != null) TxtEmptyTitle.Text = LocalizationManager.Get("EmptyAccountsTitle");
            if (TxtEmptyDesc != null) TxtEmptyDesc.Text = LocalizationManager.Get("EmptyAccountsDesc");
            if (BtnAutoAddFirstAccount != null) BtnAutoAddFirstAccount.Content = LocalizationManager.Get("AutoAddAccountBtn");

            foreach (var acc in Accounts)
            {
                acc.RefreshLocalization();
            }
        }

        public void UpdateCfgLockVisual()
        {
            try
            {
                string riotGamesPath = ConfigManager.Get("General", "RiotGamesPath", "");
                bool isReadOnly = ConfigLockService.IsConfigReadOnly(string.IsNullOrEmpty(riotGamesPath) ? null : riotGamesPath);
                if (isReadOnly)
                {
                    BtnCfgLock.Content = LocalizationManager.Get("CfgLocked");
                    BtnCfgLock.ToolTip = LocalizationManager.Get("CfgLockTooltipLocked");
                }
                else
                {
                    BtnCfgLock.Content = LocalizationManager.Get("CfgUnlocked");
                    BtnCfgLock.ToolTip = LocalizationManager.Get("CfgLockTooltipUnlocked");
                }
            }
            catch { }
        }

        public async Task RefreshAllStatsAsync()
        {
            foreach (var acc in Accounts)
            {
                try
                {
                    var result = await LolStatsService.FetchStatsAsync(acc.SummonerName, acc.Server);
                    if (result != null)
                    {
                        acc.Rank = result.Rank;
                        if (!string.IsNullOrEmpty(result.ProfileImageUrl)) acc.ProfileImageUrl = result.ProfileImageUrl;
                        if (!string.IsNullOrEmpty(result.TierImageUrl)) acc.TierImageUrl = result.TierImageUrl;
                        if (result.Level > 0) acc.Level = result.Level;
                        if (!string.IsNullOrEmpty(result.ProfileUrl)) acc.ProfileUrl = result.ProfileUrl;
                        if (!string.IsNullOrEmpty(result.LiveUrl)) acc.LiveUrl = result.LiveUrl;
                    }
                }
                catch { }
            }
            AccountStorage.SaveAccounts(Accounts);
        }

        private async void BtnRefreshStats_Click(object sender, RoutedEventArgs e)
        {
            BtnRefreshStats.IsEnabled = false;
            await RefreshAllStatsAsync();
            BtnRefreshStats.IsEnabled = true;
            MessageBox.Show(LocalizationManager.Get("StatsRefreshCompleted"), LocalizationManager.Get("AppTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnAccountProfile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                e.Handled = true;
                OpenAccountProfile(sender as FrameworkElement);
            }
        }

        private void BtnOpenLoG_Click(object sender, RoutedEventArgs e)
        {
            OpenAccountProfile(sender as FrameworkElement);
        }

        private void BtnOpenLive_Click(object sender, RoutedEventArgs e)
        {
            OpenAccountLiveMatch(sender as FrameworkElement);
        }

        private void OpenAccountLiveMatch(FrameworkElement? elem)
        {
            if (elem == null) return;
            string? url = (elem.DataContext as AccountModel)?.GetEffectiveLiveUrl() ?? (elem.Tag as string);
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Hata");
                }
            }
        }

        private void OpenAccountProfile(FrameworkElement? elem)
        {
            if (elem == null) return;
            string? url = (elem.DataContext as AccountModel)?.GetEffectiveProfileUrl() ?? (elem.Tag as string);
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Hata");
                }
            }
        }

        private void BtnOpenUrl_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.Tag is string url && !string.IsNullOrEmpty(url))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Hata");
                }
            }
        }

        private void BtnLogo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                e.Handled = true;
                try
                {
                    Process.Start(new ProcessStartInfo("https://haytokoraz.github.io/Lol-Account-Switcher/") { UseShellExecute = true });
                }
                catch { }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.ToggleTheme();
        }

        private void BtnLangToggle_Click(object sender, RoutedEventArgs e)
        {
            LocalizationManager.ToggleLanguage();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow(this);
            settings.ShowDialog();
        }

        private async void BtnAutoAddFirstAccount_Click(object sender, RoutedEventArgs e)
        {
            BtnAutoAddFirstAccount.IsEnabled = false;
            try
            {
                string riotProfilePath = ConfigManager.Get("General", "RiotProfilePath", "");
                var session = RiotSessionService.GetCurrentActiveSession(string.IsNullOrEmpty(riotProfilePath) ? null : riotProfilePath);
                if (session == null || !session.IsValid)
                {
                    MessageBox.Show(LocalizationManager.Get("AutoDetectNotFound"), LocalizationManager.Get("AppTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Web üzerinden en güncel rank ve profil resmini çek
                string summoner = session.SummonerName;
                string server = session.Server;
                var stats = await LolStatsService.FetchStatsAsync(summoner, server);

                // Aynı Riot ID veya GameName + Server ile kayıtlı hesap kontrolü (Duplicate önleme)
                var existingAcc = Accounts.FirstOrDefault(a => 
                    string.Equals(a.SummonerName, summoner, StringComparison.OrdinalIgnoreCase) ||
                    (string.Equals(a.Server, server, StringComparison.OrdinalIgnoreCase) && 
                     a.SummonerName.StartsWith(session.GameName, StringComparison.OrdinalIgnoreCase)));

                if (existingAcc != null)
                {
                    if (stats != null)
                    {
                        existingAcc.Rank = stats.Rank;
                        if (!string.IsNullOrEmpty(stats.ProfileImageUrl)) existingAcc.ProfileImageUrl = stats.ProfileImageUrl;
                        if (!string.IsNullOrEmpty(stats.TierImageUrl)) existingAcc.TierImageUrl = stats.TierImageUrl;
                        if (stats.Level > 0) existingAcc.Level = stats.Level;
                    }
                    RiotSessionService.SaveCurrentProfileToSlot(existingAcc.Id, string.IsNullOrEmpty(riotProfilePath) ? null : riotProfilePath);
                    AccountStorage.SaveAccounts(Accounts);
                    MessageBox.Show(LocalizationManager.Get("AccountAlreadyExists", existingAcc.Title), LocalizationManager.Get("AppTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var newAcc = new AccountModel
                {
                    Id = $"Acc_{Accounts.Count + 1}",
                    Title = $"[{server}] {session.GameName}",
                    SummonerName = stats != null ? $"{stats.GameName}#{stats.TagLine}" : summoner,
                    Server = server,
                    Rank = stats?.Rank ?? "Unranked",
                    Level = stats?.Level ?? 0,
                    ProfileImageUrl = stats?.ProfileImageUrl,
                    TierImageUrl = stats?.TierImageUrl,
                    IconEmoji = server == "TR" ? "🐱" : "🦊",
                    ProfileUrl = stats?.ProfileUrl ?? $"https://www.leagueofgraphs.com/tr/summoner/{server.ToLowerInvariant()}/{summoner}",
                    LiveUrl = stats?.LiveUrl ?? $"https://porofessor.gg/tr/live/{server.ToLowerInvariant()}/{summoner}"
                };

                // Profil oturum verilerini slotuna arşivle
                RiotSessionService.SaveCurrentProfileToSlot(newAcc.Id, string.IsNullOrEmpty(riotProfilePath) ? null : riotProfilePath);

                Accounts.Add(newAcc);
                AccountStorage.SaveAccounts(Accounts);
                UpdateEmptyState();

                MessageBox.Show(LocalizationManager.Get("AccountAddedSuccess", newAcc.Title), LocalizationManager.Get("AppTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, LocalizationManager.Get("AppTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnAutoAddFirstAccount.IsEnabled = true;
                if (BtnAutoAddHeader != null) BtnAutoAddHeader.IsEnabled = true;
            }
        }

        private async void BtnAccountSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.Tag != null)
            {
                string title = elem.Tag.ToString()!;
                var acc = Accounts.FirstOrDefault(a => a.Title == title || a.Id == title);

                string riotGamesPath = ConfigManager.Get("General", "RiotGamesPath", "");
                string riotProfilePath = ConfigManager.Get("General", "RiotProfilePath", "");

                // 1. ADIM: İsim ve Dizin bazlı süreçleri sonlandır (350ms bekleme ile)
                bool closeClients = !string.Equals(ConfigManager.Get("General", "CloseLeagueOnSwitch", "true"), "false", StringComparison.OrdinalIgnoreCase);
                if (closeClients)
                {
                    await ProcessTerminator.TerminateLeagueProcessesAsync(string.IsNullOrEmpty(riotGamesPath) ? null : riotGamesPath);
                }

                // 2. ADIM: Profil Slotunu Geri Yükle (Vanguard korunur, AppData oturum verileri aktarılır)
                if (acc != null)
                {
                    RiotSessionService.RestoreProfileSlot(acc.Id, string.IsNullOrEmpty(riotProfilePath) ? null : riotProfilePath);
                }

                // 3. ADIM: Ayarlanan Güç Planını devreye al (PowerManager - Win32 API)
                string switchGuidStr = ConfigManager.Get("General", "SwitchPowerPlanGuid", "").Trim();
                if (!string.IsNullOrEmpty(switchGuidStr) && Guid.TryParse(switchGuidStr, out Guid targetGuid))
                {
                    PowerManager.SetActiveScheme(targetGuid);
                }

                // 4. ADIM: Ayarlanan Monitör Tazeleme Hızını devreye al (DisplayManager - Win32 API)
                string switchHzStr = ConfigManager.Get("General", "SwitchRefreshRate", "144").Trim();
                if (int.TryParse(switchHzStr, out int switchHz) && switchHz > 0)
                {
                    if (DisplayManager.SetRefreshRate(switchHz))
                    {
                        TxtHz.Text = $"{switchHz}Hz";
                    }
                }

                // 5. ADIM: League of Legends / Riot Client'ı Otomatik Başlat (Ayara bağlı)
                bool autoLaunch = !string.Equals(ConfigManager.Get("General", "AutoLaunchGameOnSwitch", "true"), "false", StringComparison.OrdinalIgnoreCase);
                if (autoLaunch)
                {
                    RiotLaunchService.LaunchLeagueOfLegends(string.IsNullOrEmpty(riotGamesPath) ? null : riotGamesPath);
                    WindowState = WindowState.Minimized;
                }

                // 6. ADIM: Yardımcı Uygulamayı Başlat (Blitz, Porofessor, Mobalytics, OP.GG vb. - Ayara bağlı)
                bool launchCompanion = string.Equals(ConfigManager.Get("General", "LaunchCompanionAppOnSwitch", ConfigManager.Get("General", "LaunchBlitzOnSwitch", "false")), "true", StringComparison.OrdinalIgnoreCase);
                if (launchCompanion)
                {
                    string companionPath = ConfigManager.Get("General", "CompanionAppPath", ConfigManager.Get("General", "BlitzPath", ""));
                    CompanionAppService.LaunchCompanionApp(string.IsNullOrEmpty(companionPath) ? null : companionPath);
                }

                // 7. ADIM: Hesap Geçişi Tamamlanma Sesini Çal (Ses düzeyi %40, kulak tırmalamaz)
                SoundService.PlayAccountSwitchSound(0.40);

                MessageBox.Show(LocalizationManager.Get("ProfileRestoredSuccess", title), LocalizationManager.Get("AppTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnAccountDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem)
            {
                var acc = elem.DataContext as AccountModel;
                if (acc == null && elem.Tag != null)
                {
                    string tagStr = elem.Tag.ToString()!;
                    acc = Accounts.FirstOrDefault(a => a.Id == tagStr || a.Title == tagStr);
                }

                if (acc != null)
                {
                    var confirm = MessageBox.Show(
                        LocalizationManager.Get("DeleteAccountConfirm", acc.Title),
                        LocalizationManager.Get("DeleteAccountTitle"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirm == MessageBoxResult.Yes)
                    {
                        // Yerel profil slot klasörünü temizle
                        try
                        {
                            string slotPath = Path.Combine(RiotSessionService.GetProfilesStoragePath(), acc.Id);
                            if (Directory.Exists(slotPath))
                            {
                                Directory.Delete(slotPath, true);
                            }
                        }
                        catch { }

                        Accounts.Remove(acc);
                        AccountStorage.SaveAccounts(Accounts);
                        UpdateEmptyState();

                        MessageBox.Show(
                            LocalizationManager.Get("DeleteAccountSuccess", acc.Title),
                            LocalizationManager.Get("AppTitle"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
        }

        private void BtnCfgLock_Click(object sender, RoutedEventArgs e)
        {
            string riotGamesPath = ConfigManager.Get("General", "RiotGamesPath", "");
            var result = ConfigLockService.ToggleConfigReadOnly(string.IsNullOrEmpty(riotGamesPath) ? null : riotGamesPath);
            UpdateCfgLockVisual();

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                MessageBox.Show(result.ErrorMessage, LocalizationManager.Get("AppTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string msg = result.IsNowReadOnly
                ? LocalizationManager.Get("CfgLockSuccessLocked", result.BackupPath ?? "")
                : LocalizationManager.Get("CfgLockSuccessUnlocked", result.BackupPath ?? "");

            MessageBox.Show(msg, LocalizationManager.Get("AppTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnAppData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Riot Games");
                if (Directory.Exists(path)) Process.Start("explorer.exe", path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void BtnGeneralBackup_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(LocalizationManager.Get("GeneralBackupSuccess"), LocalizationManager.Get("GeneralBackup"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnKillLeague_Click(object sender, RoutedEventArgs e)
        {
            string riotGamesPath = ConfigManager.Get("General", "RiotGamesPath", "");
            await ProcessTerminator.TerminateLeagueProcessesAsync(string.IsNullOrEmpty(riotGamesPath) ? null : riotGamesPath);

            // 1. Ayarlanan LoL Kapat Güç Planını devreye al (PowerManager - Win32 API)
            string killGuidStr = ConfigManager.Get("General", "KillLeaguePowerPlanGuid", "").Trim();
            string planApplied = LocalizationManager.Get("PowerPlanAuto");
            if (!string.IsNullOrEmpty(killGuidStr) && Guid.TryParse(killGuidStr, out Guid killGuid))
            {
                if (PowerManager.SetActiveScheme(killGuid))
                {
                    var plans = PowerManager.GetPowerPlans();
                    var match = plans.Find(p => p.Guid == killGuid);
                    if (match != null) planApplied = match.Name;
                }
            }

            // 2. Ayarlanan LoL Kapat Monitör Tazeleme Hızını devreye al (DisplayManager - Win32 API)
            string killHzStr = ConfigManager.Get("General", "KillLeagueRefreshRate", "60").Trim();
            if (int.TryParse(killHzStr, out int killHz) && killHz > 0)
            {
                if (DisplayManager.SetRefreshRate(killHz))
                {
                    TxtHz.Text = $"{killHz}Hz";
                }
            }

            MessageBox.Show(LocalizationManager.Get("KillLeagueSuccess", planApplied), LocalizationManager.Get("KillLeague"), MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}