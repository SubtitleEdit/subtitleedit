; preprocessor checks
#if VER < EncodeVer(6,2,1)
  #error Update your Inno Setup version (6.2.1 or newer)
#endif

#ifndef UNICODE
  #error Use Inno Setup unicode
#endif

#define app_name             "Subtitle Edit"
#define app_copyright        "Nikse"
#define app_copyright_start  "2001"
#define app_copyright_end    GetDateTimeString('yyyy','','')

#define VerMajor
#define VerMinor
#define VerBuild
#define VerRevision

#define bindir "..\..\src\UI\bin\Release\net10.0\publish"

#ifnexist bindir + "\SubtitleEdit.exe"
  #error Compile Subtitle Edit first
#endif

#expr ParseVersion(bindir + "\SubtitleEdit.exe", VerMajor, VerMinor, VerBuild, VerRevision)

#define app_ver       str(VerMajor) + "." + str(VerMinor) + "." + str(VerBuild)
#define app_ver_full  str(VerMajor) + "." + str(VerMinor) + "." + str(VerBuild) + "." + str(VerRevision)

#define keyAppPaths  "Software\Microsoft\Windows\CurrentVersion\App Paths"


[Setup]
AppID={{B5E6D1E0-A9F3-4B2C-8E7D-1F5C3A9B0E4D}
AppName={#app_name}
AppVersion={#app_ver_full}
AppVerName={#app_name} {#app_ver}

AppCopyright={#app_copyright} {#app_copyright_start} {#app_copyright_end}
AppPublisher={#app_copyright}

AppContact=https://subtitleedit.github.io/subtitleedit/
AppPublisherURL=https://subtitleedit.github.io/subtitleedit/
AppSupportURL=https://subtitleedit.github.io/subtitleedit/
AppUpdatesURL=https://subtitleedit.github.io/subtitleedit/

VersionInfoVersion={#app_ver_full}
VersionInfoDescription={#app_name} installer
VersionInfoProductName={#app_name}

UninstallDisplayName={#app_name}
UninstallDisplayIcon={app}\SubtitleEdit.exe

WizardStyle=modern

DefaultDirName={autopf}\{#app_name}
DefaultGroupName={#app_name}
MinVersion=10.0
LicenseFile=..\..\LICENSE
SetupIconFile=..\..\src\UI\SE.ico
WizardImageFile=Icons\WizardImageFile.bmp
WizardSmallImageFile=Icons\WizardSmallImageFile.bmp
OutputDir=.
OutputBaseFilename=SubtitleEdit-{#app_ver}-Setup
AllowNoIcons=yes
Compression=lzma2/ultra
InternalCompressLevel=ultra
SolidCompression=yes
ShowTasksTreeLines=yes
DisableReadyPage=yes
PrivilegesRequired=admin
ChangesAssociations=yes
DisableDirPage=auto
DisableProgramGroupPage=auto
CloseApplications=true
SetupMutex='subtitle_edit_setup_mutex'
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"


; Include the installer's custom messages
#include "Subtitle_Edit_Localization.iss"


[Tasks]
Name: desktopicon;        Description: {cm:CreateDesktopIcon};  GroupDescription: {cm:AdditionalIcons}
Name: desktopicon\user;   Description: {cm:tsk_CurrentUser};    GroupDescription: {cm:AdditionalIcons}; Flags: exclusive
Name: desktopicon\common; Description: {cm:tsk_AllUsers};       GroupDescription: {cm:AdditionalIcons}; Flags: unchecked exclusive
Name: reset_settings;     Description: {cm:tsk_ResetSettings};  GroupDescription: {cm:tsk_Other};       Flags: checkedonce unchecked; Check: SettingsExist()


[Files]
Source: {#bindir}\SubtitleEdit.exe;      DestDir: {app}; Flags: ignoreversion
Source: {#bindir}\av_libglesv2.dll;      DestDir: {app}; Flags: ignoreversion
Source: {#bindir}\libHarfBuzzSharp.dll;  DestDir: {app}; Flags: ignoreversion
;Source: {#bindir}\libmpv-2.dll;          DestDir: {app}; Flags: ignoreversion
Source: {#bindir}\libonigwrap.dll;       DestDir: {app}; Flags: ignoreversion
Source: {#bindir}\libSkiaSharp.dll;      DestDir: {app}; Flags: ignoreversion
;Source: ..\Changelog.txt;               DestDir: {app}; Flags: ignoreversion
Source: ..\..\LICENSE;                 DestDir: {app}; Flags: ignoreversion


[Icons]
Name: {group}\Subtitle Edit;                                        Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit {#app_ver}; AppUserModelID: Nikse.SubtitleEdit5; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0
Name: {group}\Help and Support\Changelog;                          Filename: {app}\Changelog.txt; WorkingDir: {app}; Comment: {cm:sm_com_Changelog}
Name: {group}\Help and Support\{cm:ProgramOnTheWeb,Subtitle Edit}; Filename: https://subtitleedit.github.io/subtitleedit/; Comment: {cm:ProgramOnTheWeb,Subtitle Edit}
Name: {group}\{cm:UninstallProgram,Subtitle Edit};                  Filename: {uninstallexe}; Comment: {cm:UninstallProgram,Subtitle Edit}; WorkingDir: {app}

Name: {commondesktop}\Subtitle Edit; Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit {#app_ver}; AppUserModelID: Nikse.SubtitleEdit5; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: desktopicon\common
Name: {userdesktop}\Subtitle Edit;   Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit {#app_ver}; AppUserModelID: Nikse.SubtitleEdit5; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: desktopicon\user


[InstallDelete]
Type: files; Name: {userdesktop}\Subtitle Edit.lnk;   Check: not IsTaskSelected('desktopicon\user')   and WasPreviousVersionInstalled()
Type: files; Name: {commondesktop}\Subtitle Edit.lnk; Check: not IsTaskSelected('desktopicon\common') and WasPreviousVersionInstalled()
Type: files; Name: {userappdata}\Subtitle Edit\Settings.xml; Tasks: reset_settings


[Run]
Filename: {app}\SubtitleEdit.exe; Description: {cm:LaunchProgram,Subtitle Edit}; WorkingDir: {app}; Flags: nowait postinstall skipifsilent unchecked


[Registry]
; Register app path so other apps can locate SubtitleEdit.exe by name
Root: HKLM; Subkey: "{#keyAppPaths}\SubtitleEdit.exe"; ValueType: string; ValueName: "";     ValueData: "{app}\SubtitleEdit.exe"; Flags: uninsdeletekey
Root: HKLM; Subkey: "{#keyAppPaths}\SubtitleEdit.exe"; ValueType: string; ValueName: "Path"; ValueData: "{app}"

; Advertise the app to Windows as a registered application (capabilities populated by the app at runtime)
Root: HKCU; Subkey: "Software\RegisteredApplications"; ValueType: string; ValueName: "SubtitleEdit5"; ValueData: "Software\SubtitleEdit5\Capabilities"; Flags: uninsdeletevalue


[Code]
function SettingsExist(): Boolean;
begin
  Result := FileExists(ExpandConstant('{userappdata}\Subtitle Edit\Settings.xml'));
end;


// Safe upgrade check using the registry App Paths key written by a previous install
function WasPreviousVersionInstalled(): Boolean;
var
  PrevExe: String;
begin
  Result := RegQueryStringValue(HKEY_LOCAL_MACHINE, '{#keyAppPaths}\SubtitleEdit.exe', '', PrevExe)
    and FileExists(PrevExe);
end;


// Keep IsUpgrade() for use only inside the wizard (CurPageChanged, ShouldSkipPage etc.)
function IsUpgrade(): Boolean;
begin
  Result := (WizardForm.PrevAppDir <> '');
end;


function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := (IsUpgrade() and (PageID = wpLicense));
end;


procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpSelectTasks then
    WizardForm.NextButton.Caption := SetupMessage(msgButtonInstall)
  else if CurPageID = wpFinished then
    WizardForm.NextButton.Caption := SetupMessage(msgButtonFinish)
  else
    WizardForm.NextButton.Caption := SetupMessage(msgButtonNext);
end;


procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    if SettingsExist() then
    begin
      if SuppressibleMsgBox(CustomMessage('msg_DeleteSettings'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2, IDNO) = IDYES then
      begin
        DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Settings.json'));
        DelTree(ExpandConstant('{userappdata}\Subtitle Edit'), True, True, True);
      end;
    end;
  end;
end;
