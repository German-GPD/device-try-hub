#define MyAppName "German'$ Device Tray Hub"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "German'$ Code Team"
#define MyAppExeName "DeviceTrayHub.exe"

[Setup]
AppId={{A8F2D2B5-79B4-4D1D-8B0A-DA3B11E1C4A9}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\German'$ Device Tray Hub
DefaultGroupName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=..\artifacts\installer
OutputBaseFilename=German-Device-Tray-Hub-Setup
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64
PrivilegesRequired=lowest
SetupLogging=yes
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no

[Tasks]
Name: "startup"; Description: "Запускать вместе с Windows"; Flags: unchecked

[Files]
Source: "..\artifacts\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\German'$ Device Tray Hub"; Filename: "{app}\{#MyAppExeName}"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "GermanDeviceTrayHub"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Запустить German'$ Device Tray Hub"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\GermanDeviceTrayHub"