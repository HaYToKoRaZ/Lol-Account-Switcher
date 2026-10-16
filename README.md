<p align="center">
  <img src="logo.png" alt="Lol Account Switcher Logo" width="96" height="96" style="border-radius: 50%;">
</p>

<h1 align="center">Lol Account Switcher</h1>

<p align="center">
  <strong>Native Riot & League of Legends Profile Switcher & System Optimizer</strong><br>
  <em>Built with pure C# .NET 10, direct Win32 APIs, zero third-party dependencies.</em>
</p>

<p align="center">
  <a href="https://github.com/HaYToKoRaZ/Lol-Account-Switcher/actions"><img src="https://img.shields.io/github/actions/workflow/status/HaYToKoRaZ/Lol-Account-Switcher/release.yml?style=flat-square&label=Build" alt="Build Status"></a>
  <a href="https://github.com/HaYToKoRaZ/Lol-Account-Switcher/releases"><img src="https://img.shields.io/github/v/release/HaYToKoRaZ/Lol-Account-Switcher?style=flat-square&color=0AC8B9&label=Release" alt="Latest Release"></a>
  <img src="https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0AC8B9?style=flat-square" alt="Platform">
  <img src="https://img.shields.io/badge/.NET-10.0%20WPF-C8AA6E?style=flat-square" alt=".NET 10">
  <a href="https://haytokoraz.github.io/Lol-Account-Switcher/"><img src="https://img.shields.io/badge/Website-Live%20Showcase-C8AA6E?style=flat-square" alt="Website"></a>
</p>

---

<p align="center">
  <a href="#english">English</a> • <a href="#t%C3%BCrk%C3%A7e">Türkçe</a>
</p>

---

<a name="english"></a>
## 🌐 English Documentation

### ⚡ Overview
**Lol Account Switcher** is an ultra-fast, zero-bloat desktop application for competitive League of Legends players. It allows switching between multiple Riot Games accounts in milliseconds without repeatedly typing credentials or two-factor authentication codes.

In addition to seamless profile switching, it directly hooks into Windows kernel APIs to dynamically optimize your monitor's display refresh rate (e.g. 144Hz/240Hz for gaming, 60Hz for desktop) and Windows Power Plans without sluggish third-party utilities.

### 🌟 Key Features

#### 🎛️ Main Screen & Toolbar Controls
- **Auto Account Add (`+` Button):** Automatically pulls summoner name, level, server, and rank badge directly from an active Riot Client session.
- **Dynamic 144Hz & Power Plan Switcher:** One-click toggle between gaming refresh rates (144Hz/240Hz) and desktop mode (60Hz) using native Win32 `ChangeDisplaySettingsEx` and `PowerSetActiveScheme`.
- **CfgLock (Keybind & Config Protector):** Locks `game.cfg` and `PersistedSettings.json` as read-only. Prevents client updates or crashes from wiping your custom in-game keybindings.
- **Kill League (Panic Button):** Instantly and cleanly terminates League of Legends, Riot Client, Vanguard tray services, and companion apps in one swift operation.
- **Direct LoL Launcher:** Bypasses slow splash screens by launching Riot Client Services with direct League flags.
- **Account Reordering (Up / Down):** Sort and prioritize your profile cards in any custom order.
- **Click-to-Copy Riot ID:** Clicking any account's summoner name or tagline copies it to clipboard with instant toast confirmation.
- **Live Match History (Porofessor & League of Graphs):** Clicking any account avatar or rank medal opens detailed stats in your default browser.

#### ⚙️ Settings & Preferences Dashboard
- **Switch Automation:** Configure client auto-exit, game auto-launch, and companion app (Blitz, Porofessor) auto-restarts upon profile change.
- **Custom Hardware Profiles:** Specify independent gaming and desktop refresh rates (60, 75, 120, 144, 165, 240, 360 Hz).
- **Power Management:** Automatically switch to "Ultimate Performance" on launch and revert to "Balanced" upon exit.
- **Custom Pre/Post Scripts:** Run custom `.bat`, `.cmd`, `.exe`, or CLI commands before/after switching or closing League.
- **Email Privacy Masking:** Masks sensitive email addresses with asterisks (`k****@domain.com`) for streamer safety.
- **Toolbar Customization:** Individually toggle the visibility of header buttons (Wiki, Patch Notes, CfgLock, Kill League).
- **Curated Themes:** Dark Hextech and Light Piltover palettes aligned with Windows system dark/light preferences.

### 🚀 Installation & Usage
1. **Installer (`.exe`):** Download `LolAccountSwitcher-Setup.exe` from [GitHub Releases](https://github.com/HaYToKoRaZ/Lol-Account-Switcher/releases). Includes desktop shortcut, start menu group, and uninstaller.
2. **Portable (`.zip`):** Extract anywhere and run `LolAccountSwitcher.exe`. Zero registry footprint.

---

<a name="türkçe"></a>
## 🇹🇷 Türkçe Dokümantasyon

### ⚡ Genel Bakış
**Lol Account Switcher**, rekabetçi League of Legends oyuncuları için geliştirilmiş yüksek performanslı ve safkan bir masaüstü uygulamasıdır. Her hesap değişiminde şifre veya iki faktörlü kod yazma derdi olmadan hesaplarınız arasında saliseler içinde geçiş yapmanızı sağlar.

Profil yönetiminin yanı sıra doğrudan Windows çekirdek API'lerini kullanarak ekran yenileme hızınızı (144Hz, 240Hz vb.) ve Windows Güç Planını (Nihai Performans) harici konsol araçları olmaksızın otomatik optimize eder.

### 🌟 Öne Çıkan Yetenekler

#### 🎛️ Ana Ekran ve Araç Çubuğu Kontrolleri
- **Otomatik Hesap Ekleme (`+` Butonu):** Riot Client açıkken artı butonuna basıldığında sihirdar adı, seviye, sunucu ve rank madalyasını otomatik algılar ve kaydeder.
- **Canlı 144Hz & Güç Planı Rozeti:** Monitörün yenileme hızını gösterir; tek tıkla oyun hızı (144Hz/240Hz) ile masaüstü hızı (60Hz) arasında doğrudan Win32 API ile geçiş yapar.
- **CfgLock (Oyun Ayarı & Tuş Koruyucu):** `game.cfg` ve `PersistedSettings.json` dosyalarını salt okunur kilitler. İstemci güncellemelerinin tuş atamalarınızı sıfırlamasını engeller.
- **LoL Kapat (Panik Butonu):** League of Legends, Riot Client, Vanguard tepsi servisleri ve tüm eşlikçi uygulamaları tek hamlede temizce sonlandırır.
- **Hızlı LoL Başlatıcı:** İstemci bekleme pencerelerini atlayarak doğrudan League of Legends istemcisini başlatır.
- **Hesap Sıralama (Yukarı / Aşağı):** Hesap kartlarını ok butonlarıyla yukarı veya aşağı kaydırarak dilediğiniz önceliğe göre dizin.
- **Tıkla-Kopyala Riot ID:** Kart üzerindeki ada tıklandığında kullanıcı adını anında panoya kopyalar ve bildirim gösterir.
- **Canlı Porofessor & LoG Bağlantısı:** Avatar veya lig madalyasına tıklandığında ilgili hesabın maç geçmişini tarayıcınızda açar.

#### ⚙️ Ayarlar Kontrol Merkezi
- **Geçiş Otomasyonu:** Hesap geçişinde eski istemciyi kapatma, oyunu otomatik başlatma ve Blitz/Porofessor uygulamalarını otomatik yeniden başlatma.
- **Özel Donanım Hızları:** Oyun için (örn. 144Hz) ve masaüstü için (örn. 60Hz) bağımsız ekran tazeleme oranları belirleme.
- **Windows Güç Planı:** Oyuna girerken "Nihai Performans", oyundan çıkınca "Dengeli" moda otomatik dönüş.
- **Geçiş Öncesi / Sonrası Betikler:** Hesap geçişi öncesi, sonrası veya LoL kapatılırken çalışacak özel `.bat`, `.cmd` veya CLI komutları.
- **E-Posta Gizlilik Maskelemesi:** Canlı yayın güvenliği için kayıtlı e-postaları asteriks ile maskeleme (`k****@domain.com`).
- **Araç Çubuğu Özelleştirme:** Üst çubuktaki butonları (Wiki, Yama Notları, CfgLock, LoL Kapat) dilediğiniz gibi açıp kapatabilme.
- **Seçkin Temalar:** Windows sistem temasına duyarlı Koyu Hextech ve Açık Piltover renk sistemleri.

### 🚀 Kurulum ve Çalıştırma
1. **Kurulum Sihirbazı (`.exe`):** [GitHub Releases](https://github.com/HaYToKoRaZ/Lol-Account-Switcher/releases) sayfasından `Setup.exe` dosyasını indirin. Masaüstü kısayolu, Başlat menüsü ve kaldırma desteği içerir.
2. **Taşınabilir Sürüm (`.zip`):** Arşivi dilediğiniz klasöre çıkartın ve doğrudan `LolAccountSwitcher.exe` başlatıcısını çalıştırın.

---

### 🛡️ Mimari Standartlar (Architecture)
- **Safkan .NET 10 WPF:** Chromium veya Electron yükü içermez, sıfır bellek sızıntısı.
- **Direct Win32 API:** Ekran tazeleme (`ChangeDisplaySettingsEx`) ve Güç Yönetimi (`PowerSetActiveScheme`) çekirdek seviyesindedir.
- **AES-256 & DPAPI Güvenliği:** Hesap bilgileri yerel Windows Veri Koruma API'si (DPAPI) ile şifrelenir.
- **Tekil Medya Mimarisi:** Tüm 11 lig madalyası ve sesler yerel medya klasöründe toplanmıştır.

---

### 👤 Yayıncı & Geliştirici (Credits)
- **Yayıncı (Publisher):** [HaYTooL](https://haytokoraz.github.io/)
- **Geliştirici (Developer):** [HaYTo](https://x.com/HaYTo) (@HaYTo)
- **İletişim (Contact):** korazhayto@gmail.com
- **Web Sitesi (Showcase):** [haytokoraz.github.io/Lol-Account-Switcher/](https://haytokoraz.github.io/Lol-Account-Switcher/)
