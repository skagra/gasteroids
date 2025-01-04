[Setup]
AppName="{#APP_NAME}"
AppVersion="{#VERSION}"
DefaultGroupName="{#APP_NAME}"
WizardStyle=modern
DefaultDirName="{autopf}\{#APP_NAME}"
UninstallDisplayIcon="{app}\{#EXE_NAME}""
Compression=lzma2
SolidCompression=yes
OutputDir="{#INSTALLER_DIR}"
OutputBaseFilename="{#APP_NAME}-{#VERSION}-setup"
WizardImageFile="{#WIZARD_IMAGE}"

[Files]
Source: {#RELEASE_DIR}\*; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\{#APP_NAME}"; Filename: "{app}\{#EXE_NAME}"
Name: "{group}\README"; Filename: "https://github.com/skagra/gasteroids/blob/main/README.md"
Name: "{group}\Source Code"; Filename: "https://github.com/skagra/gasteroids/"
Name: "{group}\License"; Filename: "https://github.com/skagra/gasteroids/blob/main/LICENSE"

[Run]
Filename: "{app}\{#EXE_NAME}"; Description: "{cm:LaunchProgram,{#APP_NAME}}"; Flags: nowait postinstall skipifsilent