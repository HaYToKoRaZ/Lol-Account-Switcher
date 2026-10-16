using System.Windows;
using System.Windows.Input;

namespace LolAccountSwitcher
{
    public partial class AccountEditWindow : Window
    {
        private readonly AccountModel _account;

        public AccountEditWindow(AccountModel account)
        {
            InitializeComponent();
            _account = account;

            TxtAccountHeader.Text = $"{_account.Title} ({_account.SummonerName}) • {_account.Server}";
            TxtEmail.Text = _account.Email ?? "";
            TxtNote.Text = _account.Note ?? "";
            ChkMaskEmail.IsChecked = ConfigManager.GetBool("General", "MaskEmailInUI", true);

            UpdateLocalization();
        }

        private void UpdateLocalization()
        {
            TxtTitle.Text = LocalizationManager.Get("EditAccountTitle");
            TxtEmailLabel.Text = LocalizationManager.Get("AccountEmailLabel");
            TxtNoteLabel.Text = LocalizationManager.Get("AccountNoteLabel");
            ChkMaskEmail.Content = LocalizationManager.Get("MaskEmailCheckbox");
            BtnSave.Content = LocalizationManager.Get("SaveBtn") + " ✓";
            BtnCancel.Content = LocalizationManager.Get("CancelBtn");

            if (LocalizationManager.CurrentLanguage == "en")
            {
                TxtEmailSub.Text = "Used for password resets or reminders (e.g. user@gmail.com or Old Hotmail)";
                TxtNoteSub.Text = "Purpose or status of the account (e.g. Main Mid, Smurf, Clash team, 3 refunds left...)";
            }
            else
            {
                TxtEmailSub.Text = "Şifre sıfırlama veya hesap hatırlatma için kullanılır (Örn: koraz@gmail.com veya Eski Hotmail)";
                TxtNoteSub.Text = "Hesabın amacı veya durumu (Örn: Main Mid, Smurf, Clash takımı, 3. iade hakkı duruyor...)";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            _account.Email = string.IsNullOrWhiteSpace(TxtEmail.Text) ? null : TxtEmail.Text.Trim();
            _account.Note = string.IsNullOrWhiteSpace(TxtNote.Text) ? null : TxtNote.Text.Trim();

            bool maskPref = ChkMaskEmail.IsChecked ?? true;
            ConfigManager.Set("General", "MaskEmailInUI", maskPref ? "true" : "false");

            if (Application.Current.MainWindow is MainWindow mw)
            {
                foreach (var acc in mw.Accounts)
                {
                    acc.RefreshLocalization();
                }
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
