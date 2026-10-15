using System;
using System.Collections.Generic;

namespace LolAccountSwitcher
{
    public static class LocalizationManager
    {
        public static string CurrentLanguage { get; private set; } = "tr";
        public static event Action? OnLanguageChanged;

        private static readonly Dictionary<string, Dictionary<string, string>> Translations = new(StringComparer.OrdinalIgnoreCase)
        {
            ["tr"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["AppTitle"] = "Lol Account Switcher",
                ["UltimatePower"] = "Nihai Güç",
                ["BalancedPower"] = "Dengeli Güç",
                ["Settings"] = "Ayarlar",
                ["SwitchBtn"] = "Geçiş Yap ➔",
                ["BackupBtnTooltip"] = "Bu Hesabı Yedekle",
                ["CfgLock"] = "🔒 Cfg",
                ["CfgLockTooltip"] = "Config Dosyasını Kilitle / Aç",
                ["AppData"] = "📁 AppData",
                ["AppDataTooltip"] = "Riot AppData Klasörünü Aç",
                ["GeneralBackup"] = "💾 Genel",
                ["GeneralBackupTooltip"] = "Tarih Damgalı Genel Sistem Yedeği Al",
                ["KillLeague"] = "🛑 LOL KAPAT",
                ["SystemReady"] = "Sistem Hazır",
                ["SwitchingTo"] = "{0} Yükleniyor...",
                ["ActiveAccount"] = "Aktif: ",
                ["ThemeToggleTooltip"] = "Koyu / Açık Tema Değiştir",
                ["LanguageToggleTooltip"] = "Dil Değiştir (TR / EN)",
                ["SettingsTitle"] = "Ayarlar & Hesaplar",
                ["ThemeLabel"] = "Görünüm Teması:",
                ["ThemeSystem"] = "[Sistem Algılandı]",
                ["ThemeDark"] = "[Koyu / Dark]",
                ["ThemeLight"] = "[Açık / Light]",
                ["ToggleThemeBtn"] = "Tema Değiştir 🌓",
                ["LanguageLabel"] = "Uygulama Dili:",
                ["ToggleLanguageBtn"] = "Dili Değiştir (TR/EN) 🌐",
                ["AddAccountTitle"] = "Yeni Hesap Ekle",
                ["AccountTitleLabel"] = "Hesap Başlığı (Zorunlu):",
                ["AccountTitleRequired"] = "Lütfen hesaba bir başlık belirleyin (Örn: EUW Main, TR Smurf)!",
                ["SummonerLabel"] = "Sihirdar Adı (Opsiyonel: İsim#Etiket):",
                ["ServerLabel"] = "Sunucu:",
                ["RankLabel"] = "Lig / Derece (Rank):",
                ["AutoStatsInfo"] = "💡 Bilgi: Sihirdar Adı (Riot ID) girdiğinizde profil resmi, lig derecesi ve seviye web servislerinden otomatik tespit edilip kartınıza eklenir.",
                ["AddAccountBtn"] = "➕ Hesabı Listeye Ekle",
                ["CloseBtn"] = "Kapat",
                ["AccountAddedSuccess"] = "'{0}' hesabı listeye eklendi!",
                ["SwitchSuccess"] = "{0} hesabı seçildi!\n(144Hz + Nihai Güç + Profil geçişi yapılacak)",
                ["BackupSuccess"] = "{0} hesabının güncel oyun ayarları yedekleniyor...",
                ["GeneralBackupSuccess"] = "Genel tarih damgalı sistem yedeği alınıyor...",
                ["KillLeagueConfirm"] = "Tüm LoL ve Riot süreçleri kapatılıyor, ekran 60Hz ve güç modu dengeliye alınıyor...",
                ["KillLeagueSuccess"] = "LoL/Riot istemcileri sonlandırıldı ve güç planı uygulandı: {0}",
                ["CloseLeagueOption"] = "Hesap Geçişinde LoL/Riot Kapat:",
                ["CloseLeagueOptionDesc"] = "Profil değişirken istemcileri otomatik kapat (⚠️ Önemli: Kapatılırsa hesap geçişlerinde oturum kilitlenmesi veya dosya çakışması yaşanabilir!)",
                ["AutoLaunchOption"] = "Hesap Geçişinde LoL'ü Otomatik Başlat:",
                ["AutoLaunchOptionDesc"] = "Profil başarıyla yüklendiğinde Riot Client veya League of Legends'ı otomatik açar",
                ["LaunchCompanionAppOption"] = "Hesap Geçişinde Yardımcı Uygulamayı Başlat:",
                ["LaunchCompanionAppOptionDesc"] = "Profil değiştiğinde kurulu yardımcı uygulamanızı (Blitz, Porofessor, Mobalytics, OP.GG vb.) yeni oturum için otomatik açar",
                ["LaunchBlitzOption"] = "Hesap Geçişinde Yardımcı Uygulamayı Başlat:",
                ["LaunchBlitzOptionDesc"] = "Profil değiştiğinde kurulu yardımcı uygulamanızı (Blitz, Porofessor, Mobalytics vb.) yeni oturum için otomatik açar",
                ["AutoRefreshStatsOption"] = "Açılışta Rank & Profilleri Güncelle:",
                ["AutoRefreshStatsOptionDesc"] = "Uygulama açıldığında tüm hesapların dereceli ve seviye verilerini otomatik yeniler",
                ["PlayStartupSoundOption"] = "Uygulama Açılışında Hoş Geldin Sesini Çal:",
                ["PlayStartupSoundOptionDesc"] = "Uygulama başladığında yumuşak bir açılış sesi (sounds/startup.mp3) çalar",
                ["CompanionAppPathLabel"] = "Yardımcı Uygulama Yolu (Blitz, Porofessor vb.):",
                ["CompanionAppDetectedSuccess"] = "Yardımcı uygulama algılandı: {0}\n\nKonum: {1}",
                ["CompanionAppNotFound"] = "Sistemde bilinen bir yardımcı uygulama (Blitz, Porofessor, Mobalytics, OP.GG vb.) bulunamadı. Lütfen 'Seç' butonuyla .exe dosyasını gösterin.",
                ["BlitzPathLabel"] = "Yardımcı Uygulama Yolu (Blitz, Porofessor vb.):",
                ["BlitzDetectedSuccess"] = "Yardımcı uygulama başarıyla algılandı!",
                ["BlitzNotFound"] = "Kurulu yardımcı uygulama bulunamadı. Lütfen 'Seç' butonuyla .exe dosyasını gösterin.",
                ["CfgLocked"] = "🔒 Cfg Kilitli",
                ["CfgUnlocked"] = "🔓 Cfg Açık",
                ["CfgLockTooltipLocked"] = "Game.cfg şu an SALT OKUNUR (Kilitli). Değiştirmek için tıklayın.",
                ["CfgLockTooltipUnlocked"] = "Game.cfg şu an YAZILABİLİR (Kilit Açık). Kilitlemek için tıklayın.",
                ["CfgLockSuccessLocked"] = "Yedek: {0}\n\nDURUM: Kilit AÇILDI (Salt Okunur yapıldı).",
                ["CfgLockSuccessUnlocked"] = "Yedek: {0}\n\nDURUM: Kilit KALDIRILDI (Yazılabilir yapıldı).",
                ["StatsRefreshing"] = "Rank ve profil verileri güncelleniyor...",
                ["StatsRefreshCompleted"] = "Tüm hesapların lig ve avatar verileri güncellendi!",
                ["SwitchPowerPlanLabel"] = "Hesap Geçişi Güç Planı:",
                ["KillLeaguePowerPlanLabel"] = "LoL Kapat Güç Planı:",
                ["SwitchRefreshRateLabel"] = "Hesap Geçişi Ekran Hızı (Hz):",
                ["KillLeagueRefreshRateLabel"] = "LoL Kapat Ekran Hızı (Hz):",
                ["RefreshRateAuto"] = "Varsayılan / Değiştirme",
                ["RiotGamesPathLabel"] = "Riot Games Kurulum Klasörü:",
                ["RiotProfilePathLabel"] = "Riot İstemci & Profil Veri Klasörü (AppData):",
                ["DetectPathBtn"] = "🎯 Otomatik Algıla",
                ["BrowsePathBtn"] = "📁 Seç...",
                ["PathDetectedSuccess"] = "Klasörler başarıyla algılandı!",
                ["PowerPlanAuto"] = "Varsayılan / Değiştirme",
                ["EmptyAccountsTitle"] = "Henüz Kayıtlı Bir Hesap Yok",
                ["EmptyAccountsDesc"] = "League of Legends profilinizi ekleyerek hızlı hesap geçişi yapabilirsiniz.",
                ["AddFirstAccountBtn"] = "➕ İlk Hesabını Ekle",
                ["CfgLockSuccess"] = "game.cfg kilit durumu değiştirildi!",
                ["FetchInfoBtn"] = "🔍 Bilgileri Çek",
                ["FetchingStats"] = "Sorgulanıyor...",
                ["FetchSuccess"] = "Hesap verileri (Rank & Profil) başarıyla çekildi!",
                ["FetchError"] = "Hesap bulunamadı! Riot ID formatı 'İsim#Etiket' şeklinde olmalıdır.",
                ["RefreshTooltip"] = "Tüm Hesapların Rank & Profil Resimlerini Güncelle 🔄",
                ["LoGTooltip"] = "League of Graphs Profilini Tarayıcıda Aç 🌐",
                ["LiveMatchTooltip"] = "Canlı Maçını Porofessor'da Aç 🔴",
                ["SummonerHint"] = "İsim#Etiket (Örn: FevFlash#hhk)",
                ["AutoDetectAccountBtn"] = "⚡ Açık Olan Riot Hesabını Otomatik Çek",
                ["AutoDetectAccountTooltip"] = "Bilgisayarda açık olan Riot Games oturumunu otomatik oku ve formu doldur",
                ["AutoDetectSuccess"] = "Aktif Riot hesabı algılandı: {0}",
                ["AutoDetectNotFound"] = "Açık bir Riot Client oturumu bulunamadı! Lütfen Riot İstemcisine veya LoL'e giriş yapmış olduğunuzdan emin olun.",
                ["AutoAddAccountBtn"] = "⚡ Açık LoL Hesabını 1-Tıkla Ekle",
                ["ProfileSavedSuccess"] = "'{0}' profil oturumu başarıyla kaydedildi!",
                ["ProfileRestoredSuccess"] = "'{0}' profil oturumu yüklendi! Şimdi LoL'e giriş yapabilirsiniz.",
                ["ProfileSlotNotFound"] = "'{0}' hesabına ait yerel profil yedeği bulunamadı.",
                ["DeleteAccountTooltip"] = "Bu Hesabı Listeden Sil 🗑️",
                ["DeleteAccountTitle"] = "Hesap Silme Onayı",
                ["DeleteAccountConfirm"] = "'{0}' hesabını listeden ve yerel profil oturum verilerinden silmek istediğinize emin misiniz?",
                ["DeleteAccountSuccess"] = "'{0}' hesabı başarıyla silindi.",
                ["AutoAddShortBtn"] = "⚡ Ekle",
                ["AccountAlreadyExists"] = "'{0}' hesabı zaten listenizde kayıtlı. Profil verileri ve rankı güncellendi."
            },
            ["en"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["AppTitle"] = "Lol Account Switcher",
                ["UltimatePower"] = "Ultimate Power",
                ["BalancedPower"] = "Balanced Power",
                ["Settings"] = "Settings",
                ["SwitchBtn"] = "Switch ➔",
                ["BackupBtnTooltip"] = "Backup this Account",
                ["CfgLock"] = "🔒 Cfg",
                ["CfgLockTooltip"] = "Lock / Unlock Config File",
                ["AppData"] = "📁 AppData",
                ["AppDataTooltip"] = "Open Riot AppData Folder",
                ["GeneralBackup"] = "💾 Backup",
                ["GeneralBackupTooltip"] = "Create Timestamped Full Backup",
                ["KillLeague"] = "🛑 KILL LEAGUE",
                ["SystemReady"] = "System Ready",
                ["SwitchingTo"] = "Loading {0}...",
                ["ActiveAccount"] = "Active: ",
                ["ThemeToggleTooltip"] = "Toggle Dark / Light Theme",
                ["LanguageToggleTooltip"] = "Switch Language (TR / EN)",
                ["SettingsTitle"] = "Settings & Accounts",
                ["ThemeLabel"] = "Visual Theme:",
                ["ThemeSystem"] = "[System Detected]",
                ["ThemeDark"] = "[Dark Mode]",
                ["ThemeLight"] = "[Light Mode]",
                ["ToggleThemeBtn"] = "Toggle Theme 🌓",
                ["LanguageLabel"] = "App Language:",
                ["ToggleLanguageBtn"] = "Switch Language (TR/EN) 🌐",
                ["CloseLeagueOption"] = "Close League/Riot on Switch:",
                ["CloseLeagueOptionDesc"] = "Cleanly close clients during profile switch (⚠️ Important: Disabling this may cause account session lock or file conflicts!)",
                ["AutoLaunchOption"] = "Auto Launch League on Switch:",
                ["AutoLaunchOptionDesc"] = "Automatically launches Riot Client or League of Legends after successful switch",
                ["LaunchCompanionAppOption"] = "Auto Launch Companion App on Switch:",
                ["LaunchCompanionAppOptionDesc"] = "Automatically launches companion app (Blitz, Porofessor, Mobalytics, OP.GG, etc.) for the new session",
                ["LaunchBlitzOption"] = "Auto Launch Companion App on Switch:",
                ["LaunchBlitzOptionDesc"] = "Automatically launches companion app (Blitz, Porofessor, Mobalytics, etc.) for the new session",
                ["AutoRefreshStatsOption"] = "Auto Refresh Stats on Startup:",
                ["AutoRefreshStatsOptionDesc"] = "Automatically fetches ranked tier and level for all accounts on launch",
                ["PlayStartupSoundOption"] = "Play Welcome Sound on Startup:",
                ["PlayStartupSoundOptionDesc"] = "Plays a soft welcome sound (sounds/startup.mp3) when the application launches",
                ["CompanionAppPathLabel"] = "Companion App Executable Path (Blitz, Porofessor, etc.):",
                ["CompanionAppDetectedSuccess"] = "Companion app detected: {0}\n\nPath: {1}",
                ["CompanionAppNotFound"] = "No known companion app (Blitz, Porofessor, Mobalytics, OP.GG, etc.) was found on the system. Please browse and select the .exe file.",
                ["BlitzPathLabel"] = "Companion App Executable Path (Blitz, Porofessor, etc.):",
                ["BlitzDetectedSuccess"] = "Companion app detected successfully!",
                ["BlitzNotFound"] = "Companion app installation not found. Please browse and select the .exe file.",
                ["CfgLocked"] = "🔒 Cfg Locked",
                ["CfgUnlocked"] = "🔓 Cfg Unlocked",
                ["CfgLockTooltipLocked"] = "Game.cfg is currently READ-ONLY (Locked). Click to unlock.",
                ["CfgLockTooltipUnlocked"] = "Game.cfg is currently WRITABLE (Unlocked). Click to lock.",
                ["CfgLockSuccessLocked"] = "Backup: {0}\n\nSTATUS: LOCKED (Read-Only enabled).",
                ["CfgLockSuccessUnlocked"] = "Backup: {0}\n\nSTATUS: UNLOCKED (Writable enabled).",
                ["StatsRefreshing"] = "Refreshing ranked tier and avatar stats...",
                ["StatsRefreshCompleted"] = "All account rank and avatar stats updated!",
                ["SwitchPowerPlanLabel"] = "Account Switch Power Plan:",
                ["KillLeaguePowerPlanLabel"] = "Kill League Power Plan:",
                ["SwitchRefreshRateLabel"] = "Account Switch Refresh Rate (Hz):",
                ["KillLeagueRefreshRateLabel"] = "Kill League Refresh Rate (Hz):",
                ["RefreshRateAuto"] = "Default / Keep Unchanged",
                ["RiotGamesPathLabel"] = "Riot Games Installation Folder:",
                ["RiotProfilePathLabel"] = "Riot Client & Profile Data Folder (AppData):",
                ["DetectPathBtn"] = "🎯 Auto Detect",
                ["BrowsePathBtn"] = "📁 Browse...",
                ["PathDetectedSuccess"] = "Directories detected successfully!",
                ["PowerPlanAuto"] = "Default / Do not change",
                ["EmptyAccountsTitle"] = "No Accounts Added Yet",
                ["EmptyAccountsDesc"] = "Add your League of Legends profile to enable instant account switching.",
                ["AddFirstAccountBtn"] = "➕ Add Your First Account",
                ["KillLeagueSuccess"] = "League/Riot clients terminated and power plan applied: {0}",
                ["AddAccountTitle"] = "Add New Account",
                ["AccountTitleLabel"] = "Account Title (Required):",
                ["AccountTitleRequired"] = "Please specify an account title (e.g. EUW Main, TR Smurf)!",
                ["SummonerLabel"] = "Summoner Name (Optional: Name#Tag):",
                ["ServerLabel"] = "Server:",
                ["RankLabel"] = "Rank / Tier:",
                ["AutoStatsInfo"] = "💡 Notice: When you enter Summoner Name (Riot ID), profile avatar, ranked tier and level are auto-detected via web services.",
                ["AddAccountBtn"] = "➕ Add Account to List",
                ["CloseBtn"] = "Close",
                ["AccountAddedSuccess"] = "Account '{0}' added to list!",
                ["SwitchSuccess"] = "Account {0} selected!\n(144Hz + Ultimate Power + Profile switched)",
                ["BackupSuccess"] = "Backing up current game settings for {0}...",
                ["GeneralBackupSuccess"] = "Creating timestamped full system backup...",
                ["KillLeagueConfirm"] = "Terminating all League & Riot processes, resetting display to 60Hz and power to balanced...",
                ["CfgLockSuccess"] = "game.cfg lock status toggled!",
                ["FetchInfoBtn"] = "🔍 Fetch Stats (LoG / Web)",
                ["FetchingStats"] = "Fetching...",
                ["FetchSuccess"] = "Account data (Rank & Profile) fetched successfully!",
                ["FetchError"] = "Account not found! Riot ID format must be 'Name#Tag'.",
                ["RefreshTooltip"] = "Refresh All Accounts Rank & Profile Pictures 🔄",
                ["LoGTooltip"] = "Open League of Graphs Profile in Browser 🌐",
                ["LiveMatchTooltip"] = "Open Live Match on Porofessor 🔴",
                ["SummonerHint"] = "Riot ID (e.g. FevFlash#hhk)",
                ["AutoDetectAccountBtn"] = "⚡ Auto-Detect Active Riot Account",
                ["AutoDetectAccountTooltip"] = "Automatically read active Riot Games session and populate form",
                ["AutoDetectSuccess"] = "Active Riot session detected: {0}",
                ["AutoDetectNotFound"] = "No active Riot Client session found! Please make sure you are logged into Riot Client or League.",
                ["AutoAddAccountBtn"] = "⚡ 1-Click Auto Add Active LoL Account",
                ["ProfileSavedSuccess"] = "'{0}' profile session saved successfully!",
                ["ProfileRestoredSuccess"] = "'{0}' profile session restored! You can now start LoL.",
                ["ProfileSlotNotFound"] = "Local profile backup not found for '{0}'.",
                ["DeleteAccountTooltip"] = "Delete this Account from List 🗑️",
                ["DeleteAccountTitle"] = "Confirm Account Deletion",
                ["DeleteAccountConfirm"] = "Are you sure you want to delete '{0}' and its stored profile session data?",
                ["DeleteAccountSuccess"] = "'{0}' account was successfully deleted.",
                ["AutoAddShortBtn"] = "⚡ Add",
                ["AccountAlreadyExists"] = "'{0}' is already in your list. Profile data and rank were updated."
            }
        };

        public static void Initialize()
        {
            string lang = ConfigManager.Get("General", "Language", "tr").ToLowerInvariant();
            if (lang != "en" && lang != "tr") lang = "tr";
            SetLanguage(lang);
        }

        public static void ToggleLanguage()
        {
            string nextLang = (CurrentLanguage == "tr") ? "en" : "tr";
            SetLanguage(nextLang);
        }

        public static void SetLanguage(string lang)
        {
            CurrentLanguage = (lang.ToLowerInvariant() == "en") ? "en" : "tr";
            ConfigManager.Set("General", "Language", CurrentLanguage);
            OnLanguageChanged?.Invoke();
        }

        public static string Get(string key, params object[] args)
        {
            if (Translations.TryGetValue(CurrentLanguage, out var dict) && dict.TryGetValue(key, out var val))
            {
                return args.Length > 0 ? string.Format(val, args) : val;
            }
            return key;
        }
    }
}
