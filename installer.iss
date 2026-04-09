[Setup]
AppName=GitPilot3
AppVersion=1.0.5
AppPublisher=Pin Sopheaktra
AppPublisherURL=https://github.com/NASAboy342/GitPilot3
DefaultDirName={autopf}\GitPilot3
DefaultGroupName=GitPilot3
OutputDir=Release\Windows
OutputBaseFilename=GitPilot3Setup
SetupIconFile=GitPilot3\GitPilot3.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
Source: "GitPilot3\bin\Release\net9.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\GitPilot3"; Filename: "{app}\GitPilot3.exe"; IconFilename: "{app}\GitPilot3.ico"
Name: "{group}\Uninstall GitPilot3"; Filename: "{uninstallexe}"
Name: "{commondesktop}\GitPilot3"; Filename: "{app}\GitPilot3.exe"; IconFilename: "{app}\GitPilot3.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\GitPilot3.exe"; Description: "Launch GitPilot3"; Flags: nowait postinstall skipifsilent
