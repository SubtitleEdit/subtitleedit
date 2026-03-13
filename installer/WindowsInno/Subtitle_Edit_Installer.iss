; preprocessor checks
#if VER < EncodeVer(6,7,1)
  #error Update your Inno Setup version (6.7.1 or newer)
#endif

#ifndef UNICODE
  #error Use Inno Setup unicode
#endif

#define app_name             "Subtitle Edit"
#define app_copyright        "Nikse"
#define app_copyright_start  "2001"
#define app_copyright_end    GetDateTimeString('yyyy','','')

; Version constants — updated by installer/WindowsInno/update-version.ps1

#define app_ver              "5.0.0"
#define app_ver_suffix       "beta4"
#define app_ver_full         "5.0.0.4"

; Shows "5.0.0 beta4" when suffix is set, plain "5.0.0" for release builds
#define app_ver_display app_ver_suffix != "" ? app_ver + " " + app_ver_suffix : app_ver

#define bindir "..\..\src\UI\bin\Release\net10.0\publish"

#ifnexist bindir + "\SubtitleEdit.exe"
  #error Compile Subtitle Edit first
#endif

#define keyAppPaths  "Software\Microsoft\Windows\CurrentVersion\App Paths"

#define SupportURL "https://subtitleedit.github.io/subtitleedit/"

[Setup]
AppID={{B5E6D1E0-A9F3-4B2C-8E7D-1F5C3A9B0E4D}
AppName={#app_name}
AppVersion={#app_ver_full}
AppVerName={#app_name} {#app_ver_display}

AppCopyright={#app_copyright} {#app_copyright_start} {#app_copyright_end}
AppPublisher={#app_copyright}

AppContact={#SupportURL}
AppPublisherURL={#SupportURL}
AppSupportURL={#SupportURL}
AppUpdatesURL={#SupportURL}

VersionInfoVersion={#app_ver_full}
VersionInfoDescription={#app_name} installer
VersionInfoProductName={#app_name}

UninstallDisplayName={#app_name}
UninstallDisplayIcon={app}\SubtitleEdit.exe

WizardStyle=modern

DefaultDirName={autopf}\{#app_name}
DefaultGroupName={#app_name}
MinVersion=10.0
LicenseFile=..\LICENSE.rtf
SetupIconFile=..\..\src\UI\SE.ico
WizardImageFile=Icons\WizardImageFile.png
WizardSmallImageFile=Icons\WizardSmallImageFile.png
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
ArchitecturesInstallIn64BitMode=x64compatible

ShowLanguageDialog=yes
UsePreviousLanguage=no
LanguageDetectionMethod=uilanguage

; Include the installer's custom messages
#include "Subtitle_Edit_Localization.iss"

[Languages]
Name: "en";   MessagesFile: "compiler:Default.isl"
Name: "ar";   MessagesFile: "compiler:Languages\Arabic.isl"
Name: "bg";   MessagesFile: "compiler:Languages\Bulgarian.isl"
Name: "ca";   MessagesFile: "Languages\Catalan.isl"
Name: "cs";   MessagesFile: "compiler:Languages\Czech.isl"
Name: "da";   MessagesFile: "compiler:Languages\Danish.isl"
Name: "de";   MessagesFile: "compiler:Languages\German.isl"
Name: "el";   MessagesFile: "Languages\Greek.isl"
Name: "es";   MessagesFile: "compiler:Languages\Spanish.isl"
Name: "fa";   MessagesFile: "Languages\Farsi.isl"
Name: "fi";   MessagesFile: "compiler:Languages\Finnish.isl"
Name: "fr";   MessagesFile: "compiler:Languages\French.isl"
Name: "hr";   MessagesFile: "Languages\Croatian.isl"
Name: "hu";   MessagesFile: "compiler:Languages\Hungarian.isl"
Name: "it";   MessagesFile: "compiler:Languages\Italian.isl"
Name: "ja";   MessagesFile: "compiler:Languages\Japanese.isl"
Name: "ko";   MessagesFile: "compiler:Languages\Korean.isl"
Name: "nl";   MessagesFile: "compiler:Languages\Dutch.isl"
Name: "pl";   MessagesFile: "compiler:Languages\Polish.isl"
Name: "pt";   MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "ptBR"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "ru";   MessagesFile: "compiler:Languages\Russian.isl"
Name: "sl";   MessagesFile: "compiler:Languages\Slovenian.isl"
Name: "srC";  MessagesFile: "Languages\SerbianCyrillic.isl"
Name: "srL";  MessagesFile: "Languages\SerbianLatin.isl"
Name: "sv";   MessagesFile: "compiler:Languages\Swedish.isl"
Name: "th";   MessagesFile: "compiler:Languages\Thai.isl"
Name: "tr";   MessagesFile: "compiler:Languages\Turkish.isl"
Name: "uk";   MessagesFile: "compiler:Languages\Ukrainian.isl"
Name: "vi";   MessagesFile: "Languages\Vietnamese.isl"
Name: "zh";   MessagesFile: "Languages\ChineseSimplified.isl"
Name: "zhTW"; MessagesFile: "Languages\ChineseTraditional.isl"

[Tasks]
Name: desktopicon;        Description: {cm:CreateDesktopIcon};  GroupDescription: {cm:AdditionalIcons}
Name: desktopicon\user;   Description: {cm:tsk_CurrentUser};    GroupDescription: {cm:AdditionalIcons}; Flags: exclusive
Name: desktopicon\common; Description: {cm:tsk_AllUsers};       GroupDescription: {cm:AdditionalIcons}; Flags: unchecked exclusive
Name: reset_settings;     Description: {cm:tsk_ResetSettings};  GroupDescription: {cm:tsk_Other};       Flags: checkedonce unchecked; Check: SettingsExist()


[Files]
Source: {#bindir}\SubtitleEdit.exe;      DestDir: {app}; Flags: ignoreversion
Source: {#bindir}\av_libglesv2.dll;      DestDir: {app}; Flags: ignoreversion
Source: {#bindir}\libHarfBuzzSharp.dll;  DestDir: {app}; Flags: ignoreversion
Source: {#bindir}\libonigwrap.dll;       DestDir: {app}; Flags: ignoreversion
Source: {#bindir}\libSkiaSharp.dll;      DestDir: {app}; Flags: ignoreversion
Source: ..\..\ChangeLog.txt;             DestDir: {app}; Flags: ignoreversion
Source: ..\LICENSE.rtf;                  DestDir: {app}; Flags: ignoreversion
Source: {#bindir}\libmpv-2.dll;          DestDir: {userappdata}\Subtitle Edit; Flags: ignoreversion

[Icons]
Name: {group}\Subtitle Edit;                                        Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit {#app_ver_display}; AppUserModelID: Nikse.SubtitleEdit5; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0
Name: {group}\Help and Support\Changelog;                           Filename: {app}\Changelog.txt; WorkingDir: {app}; Comment: {cm:sm_com_Changelog}
Name: {group}\Help and Support\{cm:ProgramOnTheWeb,Subtitle Edit};  Filename: https://subtitleedit.github.io/subtitleedit/; Comment: {cm:ProgramOnTheWeb,Subtitle Edit}
Name: {group}\{cm:UninstallProgram,Subtitle Edit};                  Filename: {uninstallexe}; Comment: {cm:UninstallProgram,Subtitle Edit}; WorkingDir: {app}

Name: {commondesktop}\Subtitle Edit; Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit {#app_ver_display}; AppUserModelID: Nikse.SubtitleEdit5; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: desktopicon\common
Name: {userdesktop}\Subtitle Edit;   Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit {#app_ver_display}; AppUserModelID: Nikse.SubtitleEdit5; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: desktopicon\user


[InstallDelete]
Type: files; Name: {userdesktop}\Subtitle Edit.lnk;   Check: not WizardIsTaskSelected('desktopicon\user')   and WasPreviousVersionInstalled()
Type: files; Name: {commondesktop}\Subtitle Edit.lnk; Check: not WizardIsTaskSelected('desktopicon\common') and WasPreviousVersionInstalled()
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
function IsDotNet10Installed(): Boolean;
var
  FindRec: TFindRec;
  RuntimePath: String;
begin
  Result := False;
  // Look in the Desktop App shared framework folder
  RuntimePath := ExpandConstant('{pf}\dotnet\shared\Microsoft.WindowsDesktop.App\');
  
  // Search for any directory starting with '10.'
  if FindFirst(RuntimePath + '10.*', FindRec) then
  begin
    try
      repeat
        if FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY <> 0 then
        begin
          // If we find at least one directory starting with 10., we are good.
          Result := True;
          break;
        end;
      until not FindNext(FindRec);
    finally
      FindClose(FindRec);
    end;
  end;
  
  if not Result then
    Log('No .NET 10.x runtime folders found in ' + RuntimePath);
end;


function InitializeSetup(): Boolean;
var
  ErrorCode: Integer; // Declare the variable here
begin
  Result := True;
  if not IsDotNet10Installed() then
  begin
    if MsgBox(
        'Subtitle Edit requires the .NET 10 Runtime, which is not installed on this computer.' + #13#10 + #13#10 +
        'Please download and install the .NET 10 Runtime and run this setup again.' + #13#10 + #13#10 +
        'Do you want to open the .NET 10 download page now?',
        mbConfirmation, MB_YESNO or MB_DEFBUTTON1) = IDYES then
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/10.0', '', '', SW_SHOW, ewNoWait, ErrorCode); 
    Result := False;
  end;
end;


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


procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    ForceDirectories(ExpandConstant('{userappdata}\Subtitle Edit'));
    SaveStringToFile(
      ExpandConstant('{userappdata}\Subtitle Edit\SetupLanguage.txt'),
      ActiveLanguage, False);
  end;
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
