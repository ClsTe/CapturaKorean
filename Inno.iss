; Override version before compiling
;#define MyAppVersion "6.0.0"

#define MyAppName "Captura KO"
#define MyAppPublisher "ClassTech"
#define MyAppURL "https://MathewSachin.github.io/Captura"
#define MyAppExeName "capturaKo.exe"

[Setup]
AppId={{C1670C5E-5042-4300-9491-6BFFF963823F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename=Captura-Setup
Compression=lzma
SolidCompression=yes
SetupIconFile=src/Captura/Images/Captura.ico
OutputDir=temp

[Languages]
Name: "korean"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

; Remove Assemblies from previous installation to prevent conflicts
[InstallDelete]
Type: files; Name: "{app}\bass.dll"
Type: files; Name: "{app}\bassmix.dll"
Type: files; Name: "{app}\Captura.*.dll"
Type: files; Name: "{app}\Captura.Core.dll.config"
Type: files; Name: "{app}\captura.exe"
Type: files; Name: "{app}\captura.exe.config"
Type: files; Name: "{app}\Captura.UI.exe"
Type: files; Name: "{app}\Captura.UI.exe.config"
Type: files; Name: "{app}\CommandLine.dll"
Type: files; Name: "{app}\DesktopDuplication.dll"
Type: files; Name: "{app}\DesktopDuplication.dll.config"
Type: files; Name: "{app}\FirstFloor.ModernUI.dll"
Type: files; Name: "{app}\Gma.System.MouseKeyHook.dll"
Type: files; Name: "{app}\Hardcodet.Wpf.TaskbarNotification.dll"
Type: files; Name: "{app}\ManagedBass*.dll"
Type: files; Name: "{app}\ModernUI.Xceed.Toolkit.dll"
Type: files; Name: "{app}\Newtonsoft.Json.dll"
Type: files; Name: "{app}\Ninject.dll"
Type: files; Name: "{app}\Ookii.Dialogs.dll"
Type: files; Name: "{app}\Screna.dll"
Type: files; Name: "{app}\SharpAvi.dll"
Type: files; Name: "{app}\SharpDX*.dll"
Type: files; Name: "{app}\Xceed.Wpf.Toolkit.dll"

[Files]
Source: "dist\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent