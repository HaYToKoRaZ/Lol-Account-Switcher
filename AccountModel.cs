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

        public string Id { get; set; } = "";

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public string SummonerName
        {
            get => _summonerName;
            set { _summonerName = value; OnPropertyChanged(); }
        }

        public string Server
        {
            get => _server;
            set { _server = value; OnPropertyChanged(); }
        }

        public string Rank
        {
            get => _rank;
            set { _rank = value; OnPropertyChanged(); }
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
            get => _tierImageUrl;
            set 
            { 
                _tierImageUrl = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(HasTierImage)); 
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

        public string LevelText => _level > 0 ? $"{_level}" : "";
        public bool HasProfileImage => !string.IsNullOrEmpty(_profileImageUrl);
        public bool HasTierImage => !string.IsNullOrEmpty(_tierImageUrl);

        public string ProfileUrl { get; set; } = "";
        public string LiveUrl { get; set; } = "";

        public string SwitchBtnText => LocalizationManager.Get("SwitchBtn");
        public string BackupBtnTooltip => LocalizationManager.Get("BackupBtnTooltip");
        public string DeleteBtnTooltip => LocalizationManager.Get("DeleteAccountTooltip");
        public string ProfileTooltip => LocalizationManager.Get("LoGTooltip");
        public string LiveMatchTooltip => LocalizationManager.Get("LiveMatchTooltip");

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
            OnPropertyChanged(nameof(SwitchBtnText));
            OnPropertyChanged(nameof(BackupBtnTooltip));
            OnPropertyChanged(nameof(DeleteBtnTooltip));
            OnPropertyChanged(nameof(ProfileTooltip));
            OnPropertyChanged(nameof(LiveMatchTooltip));
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
