; VoiceBook Studio — Inno Setup Script
; Requires Inno Setup 6 — run build_installer.bat to build

#define AppName    "VoiceBook Studio"
#define AppVersion "1.0.0"
#define AppExe     "VoiceBookStudio.exe"
#define AppId      "{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}"

[Setup]
AppId={{#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=VoiceBook Studio
DefaultDirName={autopf}\VoiceBookStudio
DefaultGroupName={#AppName}
AllowNoIcons=yes
OutputDir=Installer
OutputBaseFilename=VoiceBookStudio_Setup_v{#AppVersion}
SetupIconFile=
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible
MinVersion=10.0
UninstallDisplayName={#AppName}
UninstallDisplayIcon={app}\{#AppExe}
; Accessibility: the installer wizard is keyboard-navigable by default
DisableWelcomePage=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon";    Description: "Create a &desktop shortcut";    GroupDescription: "Additional icons:"; Flags: unchecked
Name: "startmenuicon";  Description: "Create a &Start Menu shortcut"; GroupDescription: "Additional icons:"

[Files]
; Main application — all files from dotnet publish output
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Prompt library data (shipped read-only with the app)
Source: "Data\PromptLibrary\prompts.json"; DestDir: "{app}\Data\PromptLibrary"; Flags: ignoreversion

; User manual
Source: "VoiceBookStudio_User_Manual.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Start Menu
Name: "{group}\{#AppName}";     Filename: "{app}\{#AppExe}"; Comment: "Accessibility-first book writing with AI"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"

; Desktop (only if task selected)
Name: "{commondesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
; Offer to launch after install — useful for sighted users; JAWS users can decline and use Start Menu
Filename: "{app}\{#AppExe}"; Description: "Launch {#AppName} now"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Remove any user-created response cards on uninstall (optional — comment out to keep user data)
; Type: filesandordirs; Name: "{userappdata}\VoiceBookStudio"
