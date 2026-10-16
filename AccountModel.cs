using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LolAccountSwitcher
{
    public class AccountModel : INotifyPropertyChanged
    {
        private string _title = "";
        private string _summonerName = "";
        private string _server = "EUW";
        private string _rank = "Unranked";
        private string _iconEmoji = "⚔️";

        private string? _profileImageUrl;
        private string? _tierImageUrl;
        private int _level;
        private bool _isActive;

        public string Id { get; set; } = "";

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ActiveBadgeVisibility));
                    OnPropertyChanged(nameof(ActiveBadgeText));
                    OnPropertyChanged(nameof(SwitchBtnText));
                }
            }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); OnPropertyChanged(nameof(CleanSummonerName)); }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public string CleanSummonerName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_summonerName))
                {
                    return _summonerName;
                }
                if (!string.IsNullOrWhiteSpace(_title))
                {
                    if (!string.IsNullOrWhiteSpace(_server) && _title.StartsWith($"[{_server}]", StringComparison.OrdinalIgnoreCase))
                    {
                        return _title.Substring(_server.Length + 2).Trim();
                    }
                    return _title;
                }
                return "League Account";
            }
        }

        private string? _note;
        private string? _email;

        public string? Note
        {
            get => _note;
            set
            {
                _note = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNote));
                OnPropertyChanged(nameof(NoteBadgeVisibility));
            }
        }

        public string? Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasEmail));
                OnPropertyChanged(nameof(EmailBadgeVisibility));
                OnPropertyChanged(nameof(DisplayEmail));
            }
        }

        public static string MaskEmail(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            string trimmed = raw.Trim();
            int atIndex = trimmed.IndexOf('@');
            if (atIndex > 0 && atIndex < trimmed.Length - 1)
            {
                string user = trimmed.Substring(0, atIndex);
                string domain = trimmed.Substring(atIndex); // @gmail.com
                if (user.Length <= 2)
                {
                    return $"{user[0]}***{domain}";
                }
                else if (user.Length <= 4)
                {
                    return $"{user[0]}***{user[^1]}{domain}";
                }
                else
                {
                    return $"{user.Substring(0, 2)}***{user[^1]}{domain}";
                }
            }
            else
            {
                if (trimmed.Length <= 2)
                    return $"{trimmed[0]}***";
                else if (trimmed.Length <= 4)
                    return $"{trimmed[0]}***{trimmed[^1]}";
                else
                    return $"{trimmed.Substring(0, 2)}***{trimmed[^1]}";
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public string DisplayEmail
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_email)) return "";
                bool mask = ConfigManager.GetBool("General", "MaskEmailInUI", true);
                return mask ? MaskEmail(_email) : _email;
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasNote => !string.IsNullOrWhiteSpace(_note);

        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasEmail => !string.IsNullOrWhiteSpace(_email);

        [System.Text.Json.Serialization.JsonIgnore]
        public System.Windows.Visibility NoteBadgeVisibility => HasNote ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        [System.Text.Json.Serialization.JsonIgnore]
        public System.Windows.Visibility EmailBadgeVisibility => HasEmail ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        [System.Text.Json.Serialization.JsonIgnore]
        public string CopyEmailTooltip => LocalizationManager.CurrentLanguage == "tr" ? "E-Postayı Kopyala 📋" : "Copy Email 📋";

        [System.Text.Json.Serialization.JsonIgnore]
        public string EditAccountTooltip => LocalizationManager.CurrentLanguage == "tr" ? "Not & E-Posta Düzenle ✏️" : "Edit Note & Email ✏️";

        [System.Text.Json.Serialization.JsonIgnore]
        public string MoveUpTooltip => LocalizationManager.Get("MoveAccountUp");

        [System.Text.Json.Serialization.JsonIgnore]
        public string MoveDownTooltip => LocalizationManager.Get("MoveAccountDown");

        public string SummonerName
        {
            get => _summonerName;
            set { _summonerName = value; OnPropertyChanged(); OnPropertyChanged(nameof(CleanSummonerName)); }
        }

        public string Server
        {
            get => _server;
            set { _server = value; OnPropertyChanged(); OnPropertyChanged(nameof(CleanSummonerName)); }
        }

        public string Rank
        {
            get => _rank;
            set 
            { 
                _rank = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(TierImageUrl));
                OnPropertyChanged(nameof(TierBorderBrush));
                OnPropertyChanged(nameof(TierColor));
                OnPropertyChanged(nameof(TierGlowOpacity));
            }
        }

        public string IconEmoji
        {
            get => _iconEmoji;
            set { _iconEmoji = value; OnPropertyChanged(); }
        }

        public string? ProfileImageUrl
        {
            get => _profileImageUrl;
            set 
            { 
                _profileImageUrl = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(HasProfileImage)); 
            }
        }

        public string? TierImageUrl
        {
            get 
            {
                string? local = SoundService.ResolveRankImagePath(_rank);
                if (!string.IsNullOrEmpty(local)) return local;
                return _tierImageUrl;
            }
            set 
            { 
                _tierImageUrl = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(HasTierImage)); 
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public System.Windows.Media.Brush TierBorderBrush
        {
            get
            {
                string tier = SoundService.ExtractTierKeyword(_rank);
                string hex = tier switch
                {
                    "iron" => "#665E58",
                    "bronze" => "#A97142",
                    "silver" => "#A0B2C6",
                    "gold" => "#E5B64A",
                    "platinum" => "#25A599",
                    "emerald" => "#00E599",
                    "diamond" => "#00D2FF",
                    "master" => "#B042FF",
                    "grandmaster" => "#FF3355",
                    "challenger" => "#FFD700",
                    "unranked" => "#4B5563",
                    _ => "#4B5563"
                };
                try
                {
                    var brush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex));
                    brush.Freeze();
                    return brush;
                }
                catch
                {
                    return System.Windows.Media.Brushes.Gray;
                }
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public System.Windows.Media.Color TierColor
        {
            get
            {
                string tier = SoundService.ExtractTierKeyword(_rank);
                string hex = tier switch
                {
                    "iron" => "#665E58",
                    "bronze" => "#A97142",
                    "silver" => "#A0B2C6",
                    "gold" => "#E5B64A",
                    "platinum" => "#25A599",
                    "emerald" => "#00E599",
                    "diamond" => "#00D2FF",
                    "master" => "#B042FF",
                    "grandmaster" => "#FF3355",
                    "challenger" => "#FFD700",
                    "unranked" => "#4B5563",
                    _ => "#4B5563"
                };
                try
                {
                    return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
                }
                catch
                {
                    return System.Windows.Media.Colors.Gray;
                }
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public double TierGlowOpacity
        {
            get
            {
                string tier = SoundService.ExtractTierKeyword(_rank);
                return tier == "unranked" ? 0.15 : 0.85;
            }
        }

        public int Level
        {
            get => _level;
            set 
            { 
                _level = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(LevelText)); 
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public string LevelText => _level > 0 ? $"{_level}" : "";
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasProfileImage => !string.IsNullOrEmpty(_profileImageUrl);
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasTierImage => !string.IsNullOrEmpty(_tierImageUrl);

        // GERÇEK OYUNCU İSTATİSTİK VE TELEMETRİ ALANLARI
        private string? _inGameName;

        public string? InGameName
        {
            get => _inGameName;
            set 
            { 
                _inGameName = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(HasInGameName)); 
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasInGameName => !string.IsNullOrEmpty(_inGameName) && 
                                     !string.Equals(_inGameName, SummonerName, StringComparison.OrdinalIgnoreCase) &&
                                     !SummonerName.StartsWith(_inGameName, StringComparison.OrdinalIgnoreCase);

        [System.Text.Json.Serialization.JsonIgnore]
        public string LevelBadge => $"Lv. {Level}";

        [System.Text.Json.Serialization.JsonIgnore]
        public string ServerBadge => !string.IsNullOrEmpty(Server) ? Server.ToUpperInvariant() : "TR";

        public string ProfileUrl { get; set; } = "";
        public string LiveUrl { get; set; } = "";

        [System.Text.Json.Serialization.JsonIgnore]
        public System.Windows.Visibility ActiveBadgeVisibility => _isActive ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        [System.Text.Json.Serialization.JsonIgnore]
        public string ActiveBadgeText => LocalizationManager.CurrentLanguage == "tr" ? "AKTİF" : "ACTIVE";

        [System.Text.Json.Serialization.JsonIgnore]
        public string SwitchBtnText => _isActive 
            ? (LocalizationManager.CurrentLanguage == "tr" ? "✓ Aktif" : "✓ Active") 
            : LocalizationManager.Get("SwitchBtn");

        [System.Text.Json.Serialization.JsonIgnore]
        public string BackupBtnTooltip => LocalizationManager.Get("BackupBtnTooltip");
        [System.Text.Json.Serialization.JsonIgnore]
        public string DeleteBtnTooltip => LocalizationManager.Get("DeleteAccountTooltip");
        [System.Text.Json.Serialization.JsonIgnore]
        public string ProfileTooltip => LocalizationManager.Get("LoGTooltip");
        [System.Text.Json.Serialization.JsonIgnore]
        public string LiveMatchTooltip => LocalizationManager.Get("LiveMatchTooltip");
        [System.Text.Json.Serialization.JsonIgnore]
        public string LiveBtnText => LocalizationManager.Get("LiveBtnText");
        [System.Text.Json.Serialization.JsonIgnore]
        public string CopySummonerTooltip => LocalizationManager.Get("CopySummonerTooltip");

        public string GetEffectiveProfileUrl()
        {
            if (!string.IsNullOrEmpty(ProfileUrl)) return ProfileUrl;
            string srv = (Server ?? "EUW").Trim().ToLowerInvariant();
            string name = (SummonerName ?? "").Trim();
            if (name.Contains('#'))
            {
                name = name.Replace('#', '-');
            }
            return $"https://www.leagueofgraphs.com/tr/summoner/{srv}/{Uri.EscapeDataString(name)}";
        }

        public string GetEffectiveLiveUrl()
        {
            if (!string.IsNullOrEmpty(LiveUrl)) return LiveUrl;
            string srv = (Server ?? "EUW").Trim().ToLowerInvariant();
            string name = (SummonerName ?? "").Trim();
            if (name.Contains('#'))
            {
                name = name.Replace('#', '-');
            }
            return $"https://porofessor.gg/tr/live/{srv}/{Uri.EscapeDataString(name)}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RefreshLocalization()
        {
            OnPropertyChanged(nameof(ActiveBadgeText));
            OnPropertyChanged(nameof(SwitchBtnText));
            OnPropertyChanged(nameof(BackupBtnTooltip));
            OnPropertyChanged(nameof(DeleteBtnTooltip));
            OnPropertyChanged(nameof(ProfileTooltip));
            OnPropertyChanged(nameof(LiveMatchTooltip));
            OnPropertyChanged(nameof(LiveBtnText));
            OnPropertyChanged(nameof(CopySummonerTooltip));
            OnPropertyChanged(nameof(CopyEmailTooltip));
            OnPropertyChanged(nameof(EditAccountTooltip));
            OnPropertyChanged(nameof(MoveUpTooltip));
            OnPropertyChanged(nameof(MoveDownTooltip));
            OnPropertyChanged(nameof(DisplayEmail));
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
