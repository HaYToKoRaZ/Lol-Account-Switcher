using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LolAccountSwitcher
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<AccountModel> Accounts { get; set; } = new();
        private readonly DispatcherTimer _periodicStatsTimer = new();
        private bool _isExplicitExit = false;

        // GLOBAL KISAYOL TUŞU (Ctrl + Shift + L) WIN32 ENTEGRASYONU
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int GLOBAL_HOTKEY_ID = 9021;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint VK_L = 0x4C; // 'L' tuşu
        private IntPtr _hwnd = IntPtr.Zero;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            _hwnd = helper.Handle;
            var source = System.Windows.Interop.HwndSource.FromHwnd(_hwnd);
            source?.AddHook(HwndHook);
            RegisterHotKey(_hwnd, GLOBAL_HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, VK_L);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY && wParam.ToInt32() == GLOBAL_HOTKEY_ID)
            {
                ActivateWindowFromAnywhere();
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void ActivateWindowFromAnywhere()
        {
            Show();
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
            ShowInAppToast("Lol Account Switcher Aktif (Ctrl+Shift+L)", "⚡", 1800);
        }

        public MainWindow()
        {
            InitializeComponent();
            LoadAccounts();
            ItemsAccounts.ItemsSource = Accounts;
            Accounts.CollectionChanged += (s, e) => TrayService.RebuildMenu();

            LocalizationManager.OnLanguageChanged += UpdateLocalization;
            UpdateLocalization();

            // Sistem Tepsisi (Saat Yanı) Servisini Başlat
            TrayService.Initialize(this);
            TrayService.RebuildMenu();
            StateChanged += MainWindow_StateChanged;
            Closing += MainWindow_Closing;

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

            // Pencere konumunu hatırla ve geri yükle
            RestoreWindowPosition();

            // Uygulama açılış hoş geldin sesini pencere yüklendiğinde çal (ayara bağlı, %50 kısık sesle)
            Loaded += (s, e) => SoundService.PlayStartupSound(0.50);
        }

        public void LoadAccounts()
        {
            Accounts.Clear();
            var list = AccountStorage.LoadAccounts();
            string activeId = ConfigManager.Get("General", "ActiveAccountId", "").Trim();
            foreach (var item in list)
            {
                item.IsActive = !string.IsNullOrEmpty(activeId) && string.Equals(item.Id, activeId, StringComparison.OrdinalIgnoreCase);
                Accounts.Add(item);
            }
            UpdateEmptyState();
            TrayService.RebuildMenu();
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
            bool isTr = LocalizationManager.CurrentLanguage == "tr";
            if (CurrentLangFlag != null)
            {
                CurrentLangFlag.Template = (ControlTemplate)FindResource(isTr ? "FlagTurkeyTemplate" : "FlagUKTemplate");
            }
            if (TxtCurrentLangCode != null)
            {
                TxtCurrentLangCode.Text = isTr ? "TR" : "EN";
            }
            if (MenuLangTR != null)
            {
                MenuLangTR.FontWeight = isTr ? FontWeights.Bold : FontWeights.Normal;
            }
            if (MenuLangEN != null)
            {
                MenuLangEN.FontWeight = !isTr ? FontWeights.Bold : FontWeights.Normal;
            }
            BtnThemeToggle.ToolTip = LocalizationManager.Get("ThemeToggleTooltip");
            BtnRefreshStats.ToolTip = LocalizationManager.Get("RefreshTooltip");
            BtnSettings.ToolTip = LocalizationManager.Get("Settings");

            if (BorderHz != null)
            {
                BorderHz.ToolTip = LocalizationManager.Get("HzBadgeTooltip");
            }

            BtnCfgLock.Content = LocalizationManager.Get("CfgLock");
            BtnCfgLock.ToolTip = LocalizationManager.Get("CfgLockTooltip");

            BtnAppData.Content = LocalizationManager.Get("AppData");
            BtnAppData.ToolTip = LocalizationManager.Get("AppDataTooltip");

            BtnKillLeague.Content = LocalizationManager.Get("KillLeague");
            BtnKillLeague.ToolTip = LocalizationManager.Get("KillLeagueTooltip");

            if (TxtLaunchLeague != null)
            {
                TxtLaunchLeague.Text = LocalizationManager.Get("LaunchLeague");
            }
            if (BtnLaunchLeague != null)
            {
                BtnLaunchLeague.ToolTip = LocalizationManager.Get("LaunchLeagueTooltip");
            }

            if (BtnPatchNotes != null)
            {
                BtnPatchNotes.Content = LocalizationManager.Get("PatchNotes");
                BtnPatchNotes.ToolTip = LocalizationManager.Get("PatchNotesTooltip");
            }

            if (BtnWiki != null)
            {
                BtnWiki.Content = LocalizationManager.Get("Wiki");
                BtnWiki.ToolTip = LocalizationManager.Get("WikiTooltip");
            }

            UpdateToolbarsVisibility();
            UpdateCfgLockVisual();

            if (BtnAutoAddHeader != null)
            {
                BtnAutoAddHeader.Content = LocalizationManager.Get("AutoAddShortBtn");
                BtnAutoAddHeader.ToolTip = LocalizationManager.Get("AutoAddAccountTooltip");
            }
            if (TxtEmptyTitle != null) TxtEmptyTitle.Text = LocalizationManager.Get("EmptyAccountsTitle");
            if (TxtEmptyDesc != null) TxtEmptyDesc.Text = LocalizationManager.Get("EmptyAccountsDesc");
            if (BtnAutoAddFirstAccount != null)
            {
                BtnAutoAddFirstAccount.Content = LocalizationManager.Get("AutoAddAccountBtn");
                BtnAutoAddFirstAccount.ToolTip = LocalizationManager.Get("AutoAddAccountTooltip");
            }

            foreach (var acc in Accounts)
            {
                acc.RefreshLocalization();
            }
        }

        public void UpdateToolbarsVisibility()
        {
            // Üst Bar Butonları & Rozetleri
            bool showHz = !string.Equals(ConfigManager.Get("General", "ShowHzBadge", "true"), "false", StringComparison.OrdinalIgnoreCase);
            if (BorderHz != null)
            {
                BorderHz.Visibility = showHz ? Visibility.Visible : Visibility.Collapsed;
            }

            bool showLang = !string.Equals(ConfigManager.Get("General", "ShowLanguageToggle", "true"), "false", StringComparison.OrdinalIgnoreCase);
            if (BtnLangMenu != null)
            {
                BtnLangMenu.Visibility = showLang ? Visibility.Visible : Visibility.Collapsed;
            }

            bool showTheme = !string.Equals(ConfigManager.Get("General", "ShowThemeToggle", "true"), "false", StringComparison.OrdinalIgnoreCase);
            if (BtnThemeToggle != null)
            {
                BtnThemeToggle.Visibility = showTheme ? Visibility.Visible : Visibility.Collapsed;
            }

            bool showRefresh = !string.Equals(ConfigManager.Get("General", "ShowRefreshStatsButton", "true"), "false", StringComparison.OrdinalIgnoreCase);
            if (BtnRefreshStats != null)
            {
                BtnRefreshStats.Visibility = showRefresh ? Visibility.Visible : Visibility.Collapsed;
            }

            // Alt Araç Çubuğu Butonları
            bool showAppData = string.Equals(ConfigManager.Get("General", "ShowAppDataButton", "false"), "true", StringComparison.OrdinalIgnoreCase);
            if (BtnAppData != null)
            {
                BtnAppData.Visibility = showAppData ? Visibility.Visible : Visibility.Collapsed;
            }

            bool showPatchNotes = !string.Equals(ConfigManager.Get("General", "ShowPatchNotesButton", "true"), "false", StringComparison.OrdinalIgnoreCase);
            if (BtnPatchNotes != null)
            {
                BtnPatchNotes.Visibility = showPatchNotes ? Visibility.Visible : Visibility.Collapsed;
            }

            bool showWiki = !string.Equals(ConfigManager.Get("General", "ShowWikiButton", "true"), "false", StringComparison.OrdinalIgnoreCase);
            if (BtnWiki != null)
            {
                BtnWiki.Visibility = showWiki ? Visibility.Visible : Visibility.Collapsed;
            }

            // Dinamik Pencere Genişliği: Üst çubuktaki aktif buton ve rozetlere göre pencere genişliği otomatik ölçeklenir
            try
            {
                double baseWidth = 530; // Logo + Uygulama Adı + v3.3 + Minimize + Kapat + Pencere Kenar Boşlukları
                if (showHz) baseWidth += 95;
                if (showLang) baseWidth += 82;
                if (showTheme) baseWidth += 38;
                if (showRefresh) baseWidth += 38;
                if (BtnWebPortal != null && BtnWebPortal.Visibility == Visibility.Visible) baseWidth += 38;
                if (BtnAutoAddHeader != null && BtnAutoAddHeader.Visibility == Visibility.Visible) baseWidth += 135;
                if (BtnSettings != null && BtnSettings.Visibility == Visibility.Visible) baseWidth += 38;

                this.Width = Math.Clamp(baseWidth + 30, 660, 880);
            }
            catch { }
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
                    BtnCfgLock.Foreground = new SolidColorBrush(Color.FromRgb(255, 76, 76)); // Kırmızı (#FF4C4C)
                    BtnCfgLock.BorderBrush = new SolidColorBrush(Color.FromArgb(180, 255, 76, 76));
                    BtnCfgLock.ToolTip = LocalizationManager.Get("CfgLockTooltipLocked");
                }
                else
                {
                    BtnCfgLock.Content = LocalizationManager.Get("CfgUnlocked");
                    BtnCfgLock.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 136)); // Yeşil (#00FF88)
                    BtnCfgLock.BorderBrush = new SolidColorBrush(Color.FromArgb(180, 0, 255, 136));
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
                        if (!string.IsNullOrEmpty(result.InGameName)) acc.InGameName = result.InGameName;
                    }
                }
                catch { }
            }
            AccountStorage.SaveAccounts(Accounts);
            TrayService.RebuildMenu();
        }

        private async void BtnRefreshStats_Click(object sender, RoutedEventArgs e)
        {
            BtnRefreshStats.IsEnabled = false;
            await RefreshAllStatsAsync();
            BtnRefreshStats.IsEnabled = true;
            ShowInAppToast(LocalizationManager.Get("StatsRefreshCompleted"), "🔄", 2200);
        }

        private System.Windows.Threading.DispatcherTimer? _toastTimer;

        public void ShowInAppToast(string message, string icon = "📋", int durationMs = 2500)
        {
            if (BorderToast == null || TxtToastMessage == null) return;

            if (TxtToastIcon != null)
            {
                TxtToastIcon.Text = string.IsNullOrWhiteSpace(icon) ? "" : $"{icon} ";
            }
            TxtToastMessage.Text = message;
            BorderToast.BeginAnimation(UIElement.OpacityProperty, null);

            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(160));
            BorderToast.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            _toastTimer?.Stop();
            _toastTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(durationMs) };
            _toastTimer.Tick += (s, e) =>
            {
                _toastTimer.Stop();
                var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(250));
                BorderToast.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            };
            _toastTimer.Start();
        }

        private void BtnCopySummonerName_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                e.Handled = true;
                if (sender is FrameworkElement elem && elem.DataContext is AccountModel acc)
                {
                    string nameToCopy = string.IsNullOrWhiteSpace(acc.SummonerName) ? acc.Title : acc.SummonerName;
                    if (!string.IsNullOrWhiteSpace(nameToCopy))
                    {
                        try
                        {
                            Clipboard.SetText(nameToCopy);
                            ShowInAppToast(string.Format(LocalizationManager.Get("SummonerCopiedBalloon"), nameToCopy), "📋", 2200);
                            TrayService.ShowBalloonNotification(
                                LocalizationManager.Get("AppTitle"),
                                string.Format(LocalizationManager.Get("SummonerCopiedBalloon"), nameToCopy));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, LocalizationManager.Get("AppTitle"));
                        }
                    }
                }
            }
        }

        private void BtnCopyEmail_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                e.Handled = true;
                if (sender is FrameworkElement elem)
                {
                    string? email = (elem.DataContext as AccountModel)?.Email ?? (elem.Tag as string);
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        try
                        {
                            Clipboard.SetText(email);
                            string msg = string.Format(LocalizationManager.Get("EmailCopiedBalloon"), email);
                            ShowInAppToast(msg, "📧", 2400);
                            TrayService.ShowBalloonNotification(LocalizationManager.Get("AppTitle"), msg);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, LocalizationManager.Get("AppTitle"));
                        }
                    }
                }
            }
        }

        private void BtnNoteBadge_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                e.Handled = true;
                if (sender is FrameworkElement elem && elem.DataContext is AccountModel acc)
                {
                    OpenAccountEditDialog(acc);
                }
            }
        }

        private void BtnEditAccount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem)
            {
                var acc = elem.DataContext as AccountModel ?? elem.Tag as AccountModel;
                if (acc != null)
                {
                    OpenAccountEditDialog(acc);
                }
            }
        }

        private void BtnMoveAccountUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem)
            {
                var acc = elem.DataContext as AccountModel ?? elem.Tag as AccountModel;
                if (acc != null)
                {
                    int oldIndex = Accounts.IndexOf(acc);
                    if (oldIndex > 0)
                    {
                        Accounts.Move(oldIndex, oldIndex - 1);
                        AccountStorage.SaveAccounts(Accounts);
                        TrayService.RebuildMenu();
                        ShowInAppToast(string.Format(LocalizationManager.Get("AccountMovedSuccess"), acc.Title), "▲", 1600);
                    }
                }
            }
        }

        private void BtnMoveAccountDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem)
            {
                var acc = elem.DataContext as AccountModel ?? elem.Tag as AccountModel;
                if (acc != null)
                {
                    int oldIndex = Accounts.IndexOf(acc);
                    if (oldIndex >= 0 && oldIndex < Accounts.Count - 1)
                    {
                        Accounts.Move(oldIndex, oldIndex + 1);
                        AccountStorage.SaveAccounts(Accounts);
                        TrayService.RebuildMenu();
                        ShowInAppToast(string.Format(LocalizationManager.Get("AccountMovedSuccess"), acc.Title), "▼", 1600);
                    }
                }
            }
        }

        private void OpenAccountEditDialog(AccountModel acc)
        {
            var editWin = new AccountEditWindow(acc) { Owner = this };
            if (editWin.ShowDialog() == true)
            {
                AccountStorage.SaveAccounts(Accounts);
                ShowInAppToast(string.Format(LocalizationManager.Get("AccountDetailsSaved"), acc.Title), "✓", 2500);
            }
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

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                bool minimizeToTray = !string.Equals(ConfigManager.Get("General", "MinimizeToTray", "true"), "false", StringComparison.OrdinalIgnoreCase);
                if (minimizeToTray)
                {
                    Hide();
                    TrayService.ShowBalloonNotification(LocalizationManager.Get("TrayBalloonTitle"), LocalizationManager.Get("TrayBalloonMessage"));
                }
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowPosition();
            if (!_isExplicitExit)
            {
                bool minimizeToTray = !string.Equals(ConfigManager.Get("General", "MinimizeToTray", "true"), "false", StringComparison.OrdinalIgnoreCase);
                if (minimizeToTray)
                {
                    e.Cancel = true;
                    Hide();
                    TrayService.ShowBalloonNotification(LocalizationManager.Get("TrayBalloonTitle"), LocalizationManager.Get("TrayBalloonMessage"));
                }
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            bool minimizeToTray = !string.Equals(ConfigManager.Get("General", "MinimizeToTray", "true"), "false", StringComparison.OrdinalIgnoreCase);
            if (minimizeToTray)
            {
                Hide();
                TrayService.ShowBalloonNotification(LocalizationManager.Get("TrayBalloonTitle"), LocalizationManager.Get("TrayBalloonMessage"));
            }
            else
            {
                WindowState = WindowState.Minimized;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            bool minimizeToTray = !string.Equals(ConfigManager.Get("General", "MinimizeToTray", "true"), "false", StringComparison.OrdinalIgnoreCase);
            if (minimizeToTray)
            {
                SaveWindowPosition();
                Hide();
                TrayService.ShowBalloonNotification(LocalizationManager.Get("TrayBalloonTitle"), LocalizationManager.Get("TrayBalloonMessage"));
            }
            else
            {
                _isExplicitExit = true;
                Close();
            }
        }

        public void ForceExitApp()
        {
            _isExplicitExit = true;
            if (_hwnd != IntPtr.Zero)
            {
                UnregisterHotKey(_hwnd, GLOBAL_HOTKEY_ID);
            }
            SaveWindowPosition();
            TrayService.Shutdown();
            Close();
            System.Windows.Application.Current.Shutdown();
        }

        public async void SwitchToAccountById(string accId)
        {
            var acc = Accounts.FirstOrDefault(a => a.Id == accId);
            if (acc != null)
            {
                await SwitchAccountCoreAsync(acc);
            }
        }

        public void TriggerKillLeague()
        {
            BtnKillLeague_Click(this, new RoutedEventArgs());
        }

        public void TriggerLaunchLeague()
        {
            BtnLaunchLeague_Click(this, new RoutedEventArgs());
        }

        private void BtnThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.ToggleTheme();
        }

        private void BtnLangMenu_Click(object sender, RoutedEventArgs e)
        {
            if (BtnLangMenu?.ContextMenu != null)
            {
                BtnLangMenu.ContextMenu.PlacementTarget = BtnLangMenu;
                BtnLangMenu.ContextMenu.IsOpen = true;
            }
        }

        private void MenuLangTR_Click(object sender, RoutedEventArgs e)
        {
            LocalizationManager.SetLanguage("tr");
        }

        private void MenuLangEN_Click(object sender, RoutedEventArgs e)
        {
            LocalizationManager.SetLanguage("en");
        }

        private void RestoreWindowPosition()
        {
            try
            {
                string sLeft = ConfigManager.Get("General", "WindowLeft", "");
                string sTop = ConfigManager.Get("General", "WindowTop", "");
                if (double.TryParse(sLeft, out double left) && double.TryParse(sTop, out double top))
                {
                    double screenWidth = SystemParameters.VirtualScreenWidth;
                    double screenHeight = SystemParameters.VirtualScreenHeight;
                    double screenLeft = SystemParameters.VirtualScreenLeft;
                    double screenTop = SystemParameters.VirtualScreenTop;

                    // Pencerenin ekran sınırları içinde olduğundan emin ol
                    if (left >= screenLeft - 100 && left < screenLeft + screenWidth - 100 &&
                        top >= screenTop - 50 && top < screenTop + screenHeight - 50)
                    {
                        WindowStartupLocation = WindowStartupLocation.Manual;
                        Left = left;
                        Top = top;
                    }
                }
            }
            catch { }
        }

        private void SaveWindowPosition()
        {
            try
            {
                if (WindowState == WindowState.Normal)
                {
                    ConfigManager.Set("General", "WindowLeft", Left.ToString("F0"));
                    ConfigManager.Set("General", "WindowTop", Top.ToString("F0"));
                }
            }
            catch { }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow(this);
            settings.ShowDialog();
            UpdateToolbarsVisibility();
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
                    ShowInAppToast(LocalizationManager.Get("AutoDetectNotFound"), "⚠️", 3000);
                    return;
                }

                // Web üzerinden en güncel rank ve profil resmini çek
                string summoner = session.SummonerName;
                string server = session.Server;
                string gameName = session.GameName;
                var stats = await LolStatsService.FetchStatsAsync(summoner, server);

                // Aynı hesap mı kontrolü:
                // 1) SummonerName tam uyuşuyorsa (örn: "FevFlash#hhk")
                // 2) Veya aynı sunucuda GameName (Riot ID'nin hashtag öncesi) tam uyuşuyorsa
                // 3) Veya Title tam uyuşuyorsa (örn: "[EUW] FevFlash")
                var existingAcc = Accounts.FirstOrDefault(a => 
                    string.Equals(a.SummonerName, summoner, StringComparison.OrdinalIgnoreCase) ||
                    (string.Equals(a.Server, server, StringComparison.OrdinalIgnoreCase) && 
                     (string.Equals(a.SummonerName.Split('#')[0], gameName, StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(a.Title, $"[{server}] {gameName}", StringComparison.OrdinalIgnoreCase))));

                if (existingAcc != null)
                {
                    // KRİTİK GÜVENCE: Kullanıcının girdiği Not ve E-posta kesinlikle silinmez, eksiksiz korunur.
                    // Sadece rank, seviye, avatarlar ve şifrelenmiş yerel Riot oturum token'ları yenilenir.
                    if (stats != null)
                    {
                        existingAcc.Rank = stats.Rank;
                        if (!string.IsNullOrEmpty(stats.ProfileImageUrl)) existingAcc.ProfileImageUrl = stats.ProfileImageUrl;
                        if (!string.IsNullOrEmpty(stats.TierImageUrl)) existingAcc.TierImageUrl = stats.TierImageUrl;
                        if (stats.Level > 0) existingAcc.Level = stats.Level;
                        existingAcc.SummonerName = $"{stats.GameName}#{stats.TagLine}";
                    }

                    RiotSessionService.SaveCurrentProfileToSlot(existingAcc.Id, string.IsNullOrEmpty(riotProfilePath) ? null : riotProfilePath);

                    // Bu hesabı aktif yap
                    ConfigManager.Set("General", "ActiveAccountId", existingAcc.Id);
                    foreach (var a in Accounts)
                    {
                        a.IsActive = string.Equals(a.Id, existingAcc.Id, StringComparison.OrdinalIgnoreCase);
                    }

                    AccountStorage.SaveAccounts(Accounts);
                    TrayService.RebuildMenu();
                    ShowInAppToast(LocalizationManager.Get("AccountUpdatedSuccess", existingAcc.Title), "⚡", 3000);
                    return;
                }

                // YENİ HESAP EKLEME:
                // Benzersiz ID belirle (Acc_1, Acc_2...)
                int maxId = 0;
                foreach (var a in Accounts)
                {
                    if (a.Id.StartsWith("Acc_") && int.TryParse(a.Id.Substring(4), out int n) && n > maxId)
                    {
                        maxId = n;
                    }
                }
                string newId = $"Acc_{maxId + 1}";

                var newAcc = new AccountModel
                {
                    Id = newId,
                    IsActive = true,
                    Title = $"[{server}] {gameName}",
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

                // Diğer tüm hesapların aktifliğini kaldır, yeni hesabı aktif yap
                ConfigManager.Set("General", "ActiveAccountId", newAcc.Id);
                foreach (var a in Accounts)
                {
                    a.IsActive = false;
                }

                // Profil oturum verilerini slotuna arşivle
                RiotSessionService.SaveCurrentProfileToSlot(newAcc.Id, string.IsNullOrEmpty(riotProfilePath) ? null : riotProfilePath);

                Accounts.Add(newAcc);
                AccountStorage.SaveAccounts(Accounts);
                TrayService.RebuildMenu();
                UpdateEmptyState();

                ShowInAppToast(LocalizationManager.Get("AccountAddedSuccess", newAcc.Title), "➕", 2800);
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
                if (acc != null)
                {
                    await SwitchAccountCoreAsync(acc);
                }
            }
        }

        public async Task SwitchAccountCoreAsync(AccountModel acc)
        {
            string riotGamesPath = ConfigManager.Get("General", "RiotGamesPath", "");
            string riotProfilePath = ConfigManager.Get("General", "RiotProfilePath", "");

            // 0. ADIM: Hesap Geçişi Öncesi Özel Komut / .exe / CLI Çalıştır
            string preSwitchCmd = ConfigManager.Get("General", "PreSwitchCommand", "").Trim();
            if (!string.IsNullOrEmpty(preSwitchCmd))
            {
                await CustomScriptService.ExecuteCommandAsync(preSwitchCmd, acc);
            }

            // 1. ADIM: İsim ve Dizin bazlı süreçleri sonlandır
            ShowInAppToast(LocalizationManager.Get("SwitchProgress1"), "🛑", 1600);
            bool closeClients = !string.Equals(ConfigManager.Get("General", "CloseLeagueOnSwitch", "true"), "false", StringComparison.OrdinalIgnoreCase);
            if (closeClients)
            {
                await ProcessTerminator.TerminateLeagueProcessesAsync(string.IsNullOrEmpty(riotGamesPath) ? null : riotGamesPath);
            }

            // 1.1 ADIM: Yardımcı Uygulama Açıksa ve Yeniden Başlatma Seçiliyse Temizce Kapat
            bool launchCompanion = string.Equals(ConfigManager.Get("General", "LaunchCompanionAppOnSwitch", ConfigManager.Get("General", "LaunchBlitzOnSwitch", "false")), "true", StringComparison.OrdinalIgnoreCase);
            string companionPath = ConfigManager.Get("General", "CompanionAppPath", ConfigManager.Get("General", "BlitzPath", ""));
            if (launchCompanion)
            {
                CompanionAppService.TerminateCompanionApp(string.IsNullOrEmpty(companionPath) ? null : companionPath);
            }

            // 2. ADIM: Profil Slotunu Geri Yükle ve Bütünlüğünü Doğrula
            ShowInAppToast(LocalizationManager.Get("SwitchProgress2"), "📂", 1600);
            bool restored = RiotSessionService.RestoreProfileSlot(acc.Id, string.IsNullOrEmpty(riotProfilePath) ? null : riotProfilePath);
            if (!restored)
            {
                string failMsg = LocalizationManager.Get("ProfileRestoreFailed", acc.Title);
                ShowInAppToast(failMsg, "⚠️", 3500);
                TrayService.ShowBalloonNotification(LocalizationManager.Get("AppTitle"), failMsg);
                return; // Kritik Güvenlik: Dosyalar kopyalanmadan / kilitliyken LoL veya yardımcı uygulama ASLA açılmamalı!
            }

            // 3. ADIM: Güç Planını devreye al
            ShowInAppToast(LocalizationManager.Get("SwitchProgress3"), "⚡", 1600);
            string switchGuidStr = ConfigManager.Get("General", "SwitchPowerPlanGuid", "").Trim();
            if (!string.IsNullOrEmpty(switchGuidStr) && Guid.TryParse(switchGuidStr, out Guid targetGuid))
            {
                PowerManager.SetActiveScheme(targetGuid);
            }

            // 4. ADIM: Monitör Tazeleme Hızını devreye al
            string switchHzStr = ConfigManager.Get("General", "SwitchRefreshRate", "144").Trim();
            if (int.TryParse(switchHzStr, out int switchHz) && switchHz > 0)
            {
                if (DisplayManager.SetRefreshRate(switchHz))
                {
                    TxtHz.Text = $"{switchHz}Hz";
                }
            }

            // 5. ADIM: LoL / Riot Client'ı Otomatik Başlat
            bool autoLaunch = !string.Equals(ConfigManager.Get("General", "AutoLaunchGameOnSwitch", "true"), "false", StringComparison.OrdinalIgnoreCase);
            if (autoLaunch)
            {
                RiotLaunchService.LaunchLeagueOfLegends(string.IsNullOrEmpty(riotGamesPath) ? null : riotGamesPath);
            }

            // 6. ADIM: Yardımcı Uygulamayı Başlat
            if (launchCompanion)
            {
                CompanionAppService.LaunchCompanionApp(string.IsNullOrEmpty(companionPath) ? null : companionPath);
            }

            // 7. ADIM: Bildirim Sesini Çal
            SoundService.PlayAccountSwitchSound(1.0);

            // 8. ADIM: Hesap Geçişi Sonrası Özel Komut / .exe / CLI Çalıştır
            string postSwitchCmd = ConfigManager.Get("General", "PostSwitchCommand", "").Trim();
            if (!string.IsNullOrEmpty(postSwitchCmd))
            {
                await CustomScriptService.ExecuteCommandAsync(postSwitchCmd, acc);
            }
            // 9. ADIM: Aktif Hesabı İşaretle ve INI Yapılandırmasına Kaydet
            ConfigManager.Set("General", "ActiveAccountId", acc.Id);
            foreach (var a in Accounts)
            {
                a.IsActive = string.Equals(a.Id, acc.Id, StringComparison.OrdinalIgnoreCase);
            }

            string switchSuccessMsg = LocalizationManager.Get("SwitchProgress4", acc.Title);
            ShowInAppToast(switchSuccessMsg, "🚀", 3200);
            TrayService.ShowBalloonNotification(LocalizationManager.Get("AppTitle"), switchSuccessMsg);
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
                        // Yerel profil slot dosyasını (.dat) ve varsa eski klasörünü güvenli temizle
                        RiotSessionService.DeleteProfileSlot(acc.Id);

                        if (acc.IsActive)
                        {
                            ConfigManager.Set("General", "ActiveAccountId", "");
                        }

                        Accounts.Remove(acc);
                        AccountStorage.SaveAccounts(Accounts);
                        UpdateEmptyState();

                        ShowInAppToast(LocalizationManager.Get("DeleteAccountSuccess", acc.Title), "🗑️", 2800);
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
                ShowInAppToast(result.ErrorMessage, "⚠️", 3000);
                return;
            }

            string msg = result.IsNowReadOnly
                ? LocalizationManager.Get("CfgLockSuccessLocked")
                : LocalizationManager.Get("CfgLockSuccessUnlocked");

            string icon = result.IsNowReadOnly ? "🔒" : "🔓";
            ShowInAppToast(msg, icon, 2800);
        }

        private void BtnPatchNotes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = string.Equals(LocalizationManager.CurrentLanguage, "tr", StringComparison.OrdinalIgnoreCase)
                    ? "https://www.leagueoflegends.com/tr-tr/news/tags/patch-notes/"
                    : "https://www.leagueoflegends.com/en-gb/news/tags/patch-notes/";

                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowInAppToast(ex.Message, "⚠️", 2500);
            }
        }

        private void BtnWiki_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://wiki.leagueoflegends.com/en-us/",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowInAppToast(ex.Message, "⚠️", 2500);
            }
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


        private void BtnLaunchLeague_Click(object sender, RoutedEventArgs e)
        {
            string riotGamesPath = ConfigManager.Get("General", "RiotGamesPath", "");
            RiotLaunchService.LaunchLeagueOfLegends(string.IsNullOrEmpty(riotGamesPath) ? null : riotGamesPath);
            ShowInAppToast(LocalizationManager.Get("LaunchLeagueSuccess"), "⚔️", 2500);
        }

        private async void BtnKillLeague_Click(object sender, RoutedEventArgs e)
        {
            string riotGamesPath = ConfigManager.Get("General", "RiotGamesPath", "");
            await ProcessTerminator.TerminateLeagueProcessesAsync(string.IsNullOrEmpty(riotGamesPath) ? null : riotGamesPath);

            // Yardımcı Uygulamayı Kapat (Blitz, Porofessor, Mobalytics, OP.GG vb.)
            string companionPath = ConfigManager.Get("General", "CompanionAppPath", ConfigManager.Get("General", "BlitzPath", ""));
            CompanionAppService.TerminateCompanionApp(string.IsNullOrEmpty(companionPath) ? null : companionPath);

            // 0. ADIM: LoL Kapat Sırasında Özel Komut / .exe / CLI Çalıştır
            string killCmd = ConfigManager.Get("General", "KillLeagueCommand", "").Trim();
            if (!string.IsNullOrEmpty(killCmd))
            {
                await CustomScriptService.ExecuteCommandAsync(killCmd);
            }

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

            string killSuccessMsg = LocalizationManager.Get("KillLeagueSuccess", planApplied);
            ShowInAppToast(killSuccessMsg, "🛑", 3200);
            TrayService.ShowBalloonNotification(LocalizationManager.Get("KillLeague"), killSuccessMsg);

            // LoL Kapat yapıldığında uygulamayı arka plana (küçült / tepsi) al
            WindowState = WindowState.Minimized;
        }
    }
}