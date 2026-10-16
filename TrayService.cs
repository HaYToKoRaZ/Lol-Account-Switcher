using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace LolAccountSwitcher
{
    public static class TrayService
    {
        private static NotifyIcon? _notifyIcon;
        private static MainWindow? _mainWindow;
        private static bool _initialized;

        public static void Initialize(MainWindow window)
        {
            if (_initialized) return;
            _mainWindow = window;

            try
            {
                _notifyIcon = new NotifyIcon();

                // İkonu yükle: Önce çalışan .exe dosyasının içine derlenmiş orijinal ikonu al, ardından disk yollarını tara
                Icon? appIcon = null;
                try
                {
                    string? exePath = Environment.ProcessPath ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                    {
                        appIcon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                    }
                }
                catch { }

                if (appIcon == null)
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string? parent = Directory.GetParent(baseDir.TrimEnd('\\', '/'))?.FullName;
                    string? grandParent = !string.IsNullOrEmpty(parent) ? Directory.GetParent(parent.TrimEnd('\\', '/'))?.FullName : null;

                    string?[] candidatePaths = new string?[]
                    {
                        Path.Combine(baseDir, "app.ico"),
                        !string.IsNullOrEmpty(parent) ? Path.Combine(parent, "app.ico") : null,
                        !string.IsNullOrEmpty(grandParent) ? Path.Combine(grandParent, "main", "app.ico") : null,
                        !string.IsNullOrEmpty(parent) ? Path.Combine(parent, "main", "app.ico") : null
                    };

                    foreach (var path in candidatePaths)
                    {
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            try
                            {
                                appIcon = new Icon(path);
                                break;
                            }
                            catch { }
                        }
                    }
                }

                _notifyIcon.Icon = appIcon ?? SystemIcons.Application;
                _notifyIcon.Text = "Lol Account Switcher v3.2";
                _notifyIcon.Visible = true;

                _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();
                _notifyIcon.MouseClick += (s, e) =>
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        RebuildMenu();
                    }
                };

                // Dil veya tema değiştiğinde tray menüsünü derhal otomatik yeniden oluştur
                LocalizationManager.OnLanguageChanged += RebuildMenu;
                ThemeManager.OnThemeChanged += RebuildMenu;

                RebuildMenu();
                _initialized = true;
            }
            catch { }
        }

        public static void RebuildMenu()
        {
            if (_notifyIcon == null || _mainWindow == null) return;

            bool isDark = ThemeManager.IsDarkMode;
            var menu = new ContextMenuStrip();
            menu.Renderer = new ThemedMenuRenderer(isDark);
            menu.ShowImageMargin = false;
            menu.Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Regular);

            // 1. Uygulamayı Aç
            var itemOpen = new ToolStripMenuItem(LocalizationManager.Get("TrayOpenApp") ?? "⚡ Uygulamayı Aç", null, (s, e) => ShowMainWindow());
            itemOpen.Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            menu.Items.Add(itemOpen);

            menu.Items.Add(new ToolStripSeparator());

            // 2. Kayıtlı Hesaplar Listesi
            var accountsHeader = new ToolStripMenuItem(LocalizationManager.Get("TraySwitchHeader") ?? "👥 Hesap Değiştir");
            accountsHeader.Font = new System.Drawing.Font("Segoe UI", 9.0f, System.Drawing.FontStyle.Bold);
            accountsHeader.Tag = "header";
            accountsHeader.Enabled = false;
            menu.Items.Add(accountsHeader);

            if (_mainWindow.Accounts != null && _mainWindow.Accounts.Count > 0)
            {
                foreach (var acc in _mainWindow.Accounts)
                {
                    string srv = !string.IsNullOrEmpty(acc.Server) ? $"[{acc.Server}] " : "";
                    string rank = !string.IsNullOrEmpty(acc.Rank) ? $" ({acc.Rank})" : "";
                    string label = $"{srv}{acc.CleanSummonerName}{rank}";

                    string accId = acc.Id;
                    var accItem = new ToolStripMenuItem($"   • {label}", null, (s, e) =>
                    {
                        _mainWindow.Dispatcher.Invoke(() => _mainWindow.SwitchToAccountById(accId));
                    });
                    menu.Items.Add(accItem);
                }
            }
            else
            {
                var noAcc = new ToolStripMenuItem($"   ({LocalizationManager.Get("EmptyAccountsTitle") ?? "Kayıtlı hesap yok"})");
                noAcc.Enabled = false;
                menu.Items.Add(noAcc);
            }

            menu.Items.Add(new ToolStripSeparator());

            // 2.5 Riot Launcher'ı Aç
            var itemLaunch = new ToolStripMenuItem(LocalizationManager.Get("LaunchLeague") ?? "⚔️ Riot Launcher'ı Aç", null, (s, e) =>
            {
                _mainWindow.Dispatcher.Invoke(() => _mainWindow.TriggerLaunchLeague());
            });
            menu.Items.Add(itemLaunch);

            // 3. Riot Servislerini Kapat
            var itemKill = new ToolStripMenuItem(LocalizationManager.Get("KillLeague") ?? "🛑 Riot Servislerini Kapat", null, (s, e) =>
            {
                _mainWindow.Dispatcher.Invoke(() => _mainWindow.TriggerKillLeague());
            });
            itemKill.Tag = "danger";
            menu.Items.Add(itemKill);

            // 3.5 Geri Bildirim
            var itemFeedback = new ToolStripMenuItem(LocalizationManager.Get("TrayFeedback") ?? "💬 Geri Bildirim", null, (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo("https://github.com/HaYToKoRaZ/Lol-Account-Switcher/issues") { UseShellExecute = true });
                }
                catch { }
            });
            menu.Items.Add(itemFeedback);

            menu.Items.Add(new ToolStripSeparator());

            // 4. Çıkış
            var itemExit = new ToolStripMenuItem(LocalizationManager.Get("TrayExit") ?? "✕ Tamamen Kapat (Çıkış)", null, (s, e) =>
            {
                _mainWindow.Dispatcher.Invoke(() => _mainWindow.ForceExitApp());
            });
            itemExit.Tag = "danger";
            menu.Items.Add(itemExit);

            _notifyIcon.ContextMenuStrip = menu;
        }

        public static void ShowBalloonNotification(string title, string message)
        {
            try
            {
                _notifyIcon?.ShowBalloonTip(2500, title, message, ToolTipIcon.Info);
            }
            catch { }
        }

        public static void ShowMainWindow()
        {
            if (_mainWindow == null) return;

            _mainWindow.Dispatcher.Invoke(() =>
            {
                _mainWindow.Show();
                if (_mainWindow.WindowState == WindowState.Minimized)
                {
                    _mainWindow.WindowState = WindowState.Normal;
                }
                _mainWindow.ShowInTaskbar = true;
                _mainWindow.Topmost = true;
                _mainWindow.Topmost = false;
                _mainWindow.Activate();
                _mainWindow.Focus();
            });
        }

        public static void Shutdown()
        {
            try
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                    _notifyIcon = null;
                }
            }
            catch { }
        }

        private class ThemedMenuRenderer : ToolStripProfessionalRenderer
        {
            private readonly bool _isDark;

            public ThemedMenuRenderer(bool isDark) : base(new ThemedColorTable(isDark))
            {
                _isDark = isDark;
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item.Selected && e.Item.Enabled)
                {
                    var rect = new System.Drawing.Rectangle(2, 1, e.Item.Width - 4, e.Item.Height - 2);
                    using var brush = new System.Drawing.SolidBrush(_isDark 
                        ? System.Drawing.Color.FromArgb(32, 45, 62) 
                        : System.Drawing.Color.FromArgb(220, 230, 242));
                    using var pen = new System.Drawing.Pen(_isDark 
                        ? System.Drawing.Color.FromArgb(200, 155, 60) 
                        : System.Drawing.Color.FromArgb(180, 145, 80));
                    e.Graphics.FillRectangle(brush, rect);
                    e.Graphics.DrawRectangle(pen, rect);
                }
                else
                {
                    base.OnRenderMenuItemBackground(e);
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                System.Drawing.Color textColor;

                if (!e.Item.Enabled)
                {
                    if (e.Item.Tag is string tag && tag == "header")
                    {
                        textColor = _isDark 
                            ? System.Drawing.Color.FromArgb(10, 200, 230) 
                            : System.Drawing.Color.FromArgb(10, 80, 140);
                    }
                    else
                    {
                        textColor = _isDark 
                            ? System.Drawing.Color.FromArgb(120, 135, 150) 
                            : System.Drawing.Color.FromArgb(145, 160, 175);
                    }
                }
                else if (e.Item.Tag is string tag && tag == "danger")
                {
                    textColor = _isDark 
                        ? System.Drawing.Color.FromArgb(255, 95, 95) 
                        : System.Drawing.Color.FromArgb(220, 38, 38);
                }
                else if (e.Item.Selected)
                {
                    textColor = _isDark 
                        ? System.Drawing.Color.FromArgb(255, 255, 255) 
                        : System.Drawing.Color.FromArgb(15, 23, 42);
                }
                else
                {
                    textColor = _isDark 
                        ? System.Drawing.Color.FromArgb(240, 230, 210) 
                        : System.Drawing.Color.FromArgb(30, 40, 55);
                }

                System.Windows.Forms.TextRenderer.DrawText(
                    e.Graphics, 
                    e.Text, 
                    e.TextFont, 
                    e.TextRectangle, 
                    textColor, 
                    e.TextFormat);
            }

            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                int y = e.Item.Height / 2;
                using var pen = new System.Drawing.Pen(_isDark 
                    ? System.Drawing.Color.FromArgb(45, 55, 70) 
                    : System.Drawing.Color.FromArgb(215, 222, 232));
                e.Graphics.DrawLine(pen, 10, y, e.Item.Width - 10, y);
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                using var pen = new System.Drawing.Pen(_isDark 
                    ? System.Drawing.Color.FromArgb(200, 155, 60) 
                    : System.Drawing.Color.FromArgb(180, 145, 80), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            }
        }

        private class ThemedColorTable : ProfessionalColorTable
        {
            private readonly bool _isDark;

            public ThemedColorTable(bool isDark)
            {
                _isDark = isDark;
            }

            public override System.Drawing.Color ToolStripDropDownBackground => _isDark
                ? System.Drawing.Color.FromArgb(14, 20, 28)
                : System.Drawing.Color.FromArgb(248, 250, 252);

            public override System.Drawing.Color MenuBorder => _isDark
                ? System.Drawing.Color.FromArgb(200, 155, 60)
                : System.Drawing.Color.FromArgb(180, 145, 80);

            public override System.Drawing.Color MenuItemSelected => _isDark
                ? System.Drawing.Color.FromArgb(32, 45, 62)
                : System.Drawing.Color.FromArgb(220, 230, 242);

            public override System.Drawing.Color MenuItemSelectedGradientBegin => _isDark
                ? System.Drawing.Color.FromArgb(32, 45, 62)
                : System.Drawing.Color.FromArgb(220, 230, 242);

            public override System.Drawing.Color MenuItemSelectedGradientEnd => _isDark
                ? System.Drawing.Color.FromArgb(36, 50, 70)
                : System.Drawing.Color.FromArgb(210, 222, 236);

            public override System.Drawing.Color MenuItemBorder => _isDark
                ? System.Drawing.Color.FromArgb(200, 155, 60)
                : System.Drawing.Color.FromArgb(180, 145, 80);

            public override System.Drawing.Color SeparatorDark => _isDark
                ? System.Drawing.Color.FromArgb(45, 55, 70)
                : System.Drawing.Color.FromArgb(215, 222, 232);

            public override System.Drawing.Color SeparatorLight => System.Drawing.Color.Transparent;

            public override System.Drawing.Color ImageMarginGradientBegin => _isDark
                ? System.Drawing.Color.FromArgb(14, 20, 28)
                : System.Drawing.Color.FromArgb(248, 250, 252);

            public override System.Drawing.Color ImageMarginGradientMiddle => _isDark
                ? System.Drawing.Color.FromArgb(14, 20, 28)
                : System.Drawing.Color.FromArgb(248, 250, 252);

            public override System.Drawing.Color ImageMarginGradientEnd => _isDark
                ? System.Drawing.Color.FromArgb(14, 20, 28)
                : System.Drawing.Color.FromArgb(248, 250, 252);
        }
    }
}
