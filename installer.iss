; ==============================================================================
; Lol Account Switcher - Inno Setup Script
; Yayinci: HaYTooL | Gelistirici: HaYTo
; ==============================================================================

#ifndef AppVersion
  #define AppVersion "3.7"
#endif

#define MyAppName "Lol Account Switcher"
#define MyAppPublisher "HaYTooL"
#define MyAppDeveloper "HaYTo"
#define MyAppURL "https://haytokoraz.github.io/Lol-Account-Switcher/"
#define MyPortalURL "https://haytokoraz.github.io/"
#define MyRepoURL "https://github.com/HaYToKoRaZ/Lol-Account-Switcher"
#define MyAppExeName "LolAccountSwitcher.exe"

[Setup]
AppId={{D1A2E4B8-93F1-4E11-9A82-4E2F7C018A5E}
AppName={#MyAppName}
AppVersion={#AppVersion}
AppVerName={#MyAppName} v{#AppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyPortalURL}
AppSupportURL={#MyRepoURL}/issues
AppUpdatesURL={#MyRepoURL}/releases
AppComments=Riot & LoL Profile Switcher and System Optimizer
AppCopyright=Copyright (C) 2026 {#MyAppPublisher} / {#MyAppDeveloper}
DefaultDirName={autopf}\LolAccountSwitcher
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=dist
OutputBaseFilename=LolAccountSwitcher-v{#AppVersion}-Setup
SetupIconFile=app.ico
UninstallDisplayIcon={app}\app.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequiredOverridesAllowed=dialog
DisableProgramGroupPage=auto
CloseApplications=yes
RestartApplications=no

; Versiyon ve Dosya Bilgileri (Windows Explorer Detaylari)
VersionInfoVersion={#AppVersion}.0.0
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Riot & LoL Profile Switcher and System Optimizer
VersionInfoCopyright=Copyright (C) 2026 {#MyAppPublisher} / {#MyAppDeveloper}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#AppVersion}

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce

[Files]
; Kok Baslatici ve Simgeler
Source: "build\LolAccountSwitcher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "build\app.ico"; DestDir: "{app}"; Flags: ignoreversion

; Yapilandirma: Mevcut kullanici ayarlari guncelleme sirasinda ezilmesin
Source: "build\LolAccountSwitcher.ini"; DestDir: "{app}"; Flags: onlyifdoesntexist

; Medya (Sesler ve Rank Logolari)
Source: "build\media\*"; DestDir: "{app}\media"; Flags: ignoreversion recursesubdirs createallsubdirs

; Bin (Izole C# .NET DLL ve PDB Dosyalari)
Source: "build\bin\*"; DestDir: "{app}\bin"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Baslat Menusu Kisayollari
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"
Name: "{group}\Web Sitesi (HaYTooL)"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

; Masaustu Kisayolu
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
