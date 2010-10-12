;* Subtitle Edit - Installer script
;*
;* Copyright (C) 2010 XhmikosR
;*
;* This file is part of Subtitle Edit.
;*
;* Subtitle Edit is free software; you can redistribute it and/or modify
;* it under the terms of the GNU General Public License as published by
;* the Free Software Foundation, either version 3 of the License, or
;* (at your option) any later version.
;*
;* Subtitle Edit is distributed in the hope that it will be useful,
;* but WITHOUT ANY WARRANTY; without even the implied warranty of
;* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
;* GNU General Public License for more details.
;*
;* You should have received a copy of the GNU General Public License
;* along with Subtitle Edit.  If not, see <http://www.gnu.org/licenses/>.
;
;
;
; Requirements:
; *Inno Setup QuickStart Pack v5.3.11(+): http://www.jrsoftware.org/isdl.php#qsp


#define installer_build_number "02"

#define VerMajor
#define VerMinor
#define VerBuild
#define VerRevision

#expr ParseVersion("..\src\bin\Release\SubtitleEdit.exe", VerMajor, VerMinor, VerBuild, VerRevision)
#define app_version str(VerMajor) + "." + str(VerMinor) + "." + str(VerBuild) + "." + str(VerRevision)

;the following simple_app_version is for 2 digit releases, one of the two must be uncommented at a time
;#define simple_app_version str(VerMajor) + "." + str(VerMinor) + "." + str(VerBuild)
#define simple_app_version str(VerMajor) + "." + str(VerMinor)

#define installer_build_date GetDateTimeString('mmm, d yyyy', '', '')


[Setup]
AppID=SubtitleEdit
AppCopyright=Copyright © 2009-2010, Nikse
AppContact=http://www.nikse.dk/se/
AppName=Subtitle Edit
AppVerName=Subtitle Edit {#= simple_app_version}
AppVersion={#= simple_app_version}
AppPublisher=Nikse
AppPublisherURL=http://www.nikse.dk/se/
AppSupportURL=http://www.nikse.dk/se/
AppUpdatesURL=http://www.nikse.dk/se/
UninstallDisplayName=Subtitle Edit {#= simple_app_version}
DefaultDirName={pf}\Subtitle Edit
DefaultGroupName=Subtitle Edit
VersionInfoCompany=Nikse
VersionInfoCopyright=Copyright © 2009-2010, Nikse
VersionInfoDescription=Subtitle Edit {#= simple_app_version} Setup
VersionInfoTextVersion={#= simple_app_version}
VersionInfoVersion={#= simple_app_version}
VersionInfoProductName=Subtitle Edit
VersionInfoProductVersion={#= simple_app_version}
VersionInfoProductTextVersion={#= simple_app_version}
MinVersion=0,5.0.2195
;AppReadmeFile={app}\Readme.txt
LicenseFile=..\src\gpl.txt
InfoAfterFile=Changelog.txt
;InfoBeforeFile=..\Readme.txt
SetupIconFile=..\src\Icons\SE.ico
UninstallDisplayIcon={app}\SubtitleEdit.exe
;WizardImageFile=Icons\WizardImageFile.bmp
WizardSmallImageFile=Icons\WizardSmallImageFile.bmp
OutputDir=.
OutputBaseFilename=SubtitleEdit-{#= simple_app_version}-setup
AllowNoIcons=yes
Compression=lzma/ultra64
SolidCompression=yes
EnableDirDoesntExistWarning=no
DirExistsWarning=no
ShowTasksTreeLines=yes
AlwaysShowDirOnReadyPage=yes
AlwaysShowGroupOnReadyPage=yes
PrivilegesRequired=admin
ShowLanguageDialog=yes
DisableDirPage=auto
DisableProgramGroupPage=auto
AppMutex=Subtitle_Edit_Mutex


[Languages]
Name: en; MessagesFile: compiler:Default.isl
Name: dk; MessagesFile: compiler:Languages\Danish.isl
Name: es; MessagesFile: compiler:Languages\Spanish.isl
Name: fr; MessagesFile: compiler:Languages\French.isl
Name: it; MessagesFile: compiler:Languages\Italian.isl
Name: nl; MessagesFile: compiler:Languages\Dutch.isl
Name: pl; MessagesFile: compiler:Languages\Polish.isl
Name: ro; MessagesFile: Languages\Romanian.isl

; Include the installer's custom messages and services script
#include "Custom_Messages.iss"


[Messages]
BeveledLabel=Subtitle Edit v{#= simple_app_version} by Nikse, Setup v{#= installer_build_number} built on {#= installer_build_date}


[Files]
Source: Changelog.txt; DestDir: {app}; Flags: ignoreversion
Source: ..\src\Bin\Release\Hunspellx86.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\src\Bin\Release\Interop.QuartzTypeLib.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\src\Bin\Release\NHunspell.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\src\Bin\Release\SubtitleEdit.exe; DestDir: {app}; Flags: ignoreversion
Source: ..\src\Bin\Release\tessnet2_32.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\Find.png; DestDir: {app}\Icons; Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\Help.png; DestDir: {app}\Icons; Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\New.png; DestDir: {app}\Icons; Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\Open.png; DestDir: {app}\Icons; Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\Replace.png; DestDir: {app}\Icons; Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\Save.png; DestDir: {app}\Icons; Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\SaveAs.png; DestDir: {app}\Icons; Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\Settings.png; DestDir: {app}\Icons; Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\SpellCheck.png; DestDir: {app}\Icons; Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\VideoToogle.png; DestDir: {app}\Icons; Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\VisualSync.png; DestDir: {app}\Icons; Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\WaveFormToogle.png; DestDir: {app}\Icons; Flags: ignoreversion
Source: ..\Dictionaries\en_US_names_etc.xml; DestDir: {app}\Dictionaries; Flags: ignoreversion
Source: ..\Dictionaries\en_US_user.xml; DestDir: {app}\Dictionaries; Flags: ignoreversion
Source: ..\Dictionaries\eng_OCRFixReplaceList.xml; DestDir: {app}\Dictionaries; Flags: ignoreversion
Source: ..\Dictionaries\en_US.aff; DestDir: {app}\Dictionaries; Flags: ignoreversion
Source: ..\Dictionaries\en_US.dic; DestDir: {app}\Dictionaries; Flags: ignoreversion
Source: ..\Dictionaries\names_etc.xml; DestDir: {app}\Dictionaries; Flags: ignoreversion
Source: ..\TessData\eng.DangAmbigs; DestDir: {app}\TessData; Flags: ignoreversion
Source: ..\TessData\eng.freq-dawg; DestDir: {app}\TessData; Flags: ignoreversion
Source: ..\TessData\eng.inttemp; DestDir: {app}\TessData; Flags: ignoreversion
Source: ..\TessData\eng.normproto; DestDir: {app}\TessData; Flags: ignoreversion
Source: ..\TessData\eng.pffmtable; DestDir: {app}\TessData; Flags: ignoreversion
Source: ..\TessData\eng.unicharset; DestDir: {app}\TessData; Flags: ignoreversion
Source: ..\TessData\eng.user-words; DestDir: {app}\TessData; Flags: ignoreversion
Source: ..\TessData\eng.word-dawg; DestDir: {app}\TessData; Flags: ignoreversion
Source: Icons\uninstall.ico; DestDir: {app}; Flags: ignoreversion


[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}
Name: desktopicon\user; Description: {cm:tsk_CurrentUser}; GroupDescription: {cm:AdditionalIcons}; Flags: exclusive
Name: desktopicon\common; Description: {cm:tsk_AllUsers}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked exclusive
Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; OnlyBelowVersion: 0,6.01; Flags: unchecked

Name: reset_settings; Description: {cm:tsk_ResetSettings}; GroupDescription: {cm:tsk_Other}; Check: SettingsExistCheck(); Flags: checkedonce unchecked


[Icons]
Name: {group}\Subtitle Edit; Filename: {app}\SubtitleEdit.exe; Comment: Subtitle Edit {#= simple_app_version}; WorkingDir: {app}; AppUserModelID: Nikse.SubtitleEdit; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0
Name: {group}\Help and Support\Changelog; Filename: {app}\Changelog.txt; Comment: {cm:sm_com_Changelog}; WorkingDir: {app}
;Name: {group}\Help and Support\Readme; Filename: {app}\Readme.txt; Comment: {cm:sm_com_ReadmeFile}; WorkingDir: {app}
Name: {group}\Help and Support\{cm:ProgramOnTheWeb,Subtitle Edit}; Filename: http://www.nikse.dk/se/; Comment: {cm:ProgramOnTheWeb,Subtitle Edit}
Name: {group}\{cm:UninstallProgram,Subtitle Edit}; Filename: {uninstallexe}; IconFilename: {app}\uninstall.ico; Comment: {cm:UninstallProgram,Subtitle Edit}; WorkingDir: {app}

Name: {commondesktop}\Subtitle Edit; Filename: {app}\SubtitleEdit.exe; Tasks: desktopicon\common; Comment: Subtitle Edit {#= simple_app_version}; WorkingDir: {app}; AppUserModelID: Nikse.SubtitleEdit; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0
Name: {userdesktop}\Subtitle Edit; Filename: {app}\SubtitleEdit.exe; Tasks: desktopicon\user; Comment: Subtitle Edit {#= simple_app_version}; WorkingDir: {app}; AppUserModelID: Nikse.SubtitleEdit; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0
Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\Subtitle Edit; Filename: {app}\SubtitleEdit.exe; Tasks: quicklaunchicon; Comment: Subtitle Edit {#= simple_app_version}; WorkingDir: {app}; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0


[InstallDelete]
Type: files; Name: {userdesktop}\Subtitle Edit.lnk; Check: NOT IsTaskSelected('desktopicon\user') AND IsUpdate()
Type: files; Name: {commondesktop}\Subtitle Edit.lnk; Check: NOT IsTaskSelected('desktopicon\common') AND IsUpdate()

Type: files; Name: {userappdata}\Subtitle Edit\settings.xml; Tasks: reset_settings
Type: dirifempty; Name: {userappdata}\Subtitle Edit; Tasks: reset_settings


[Run]
Filename: {app}\SubtitleEdit.exe; Description: {cm:LaunchProgram,Subtitle Edit}; WorkingDir: {app}; Flags: nowait postinstall skipifsilent runascurrentuser
Filename: http://www.nikse.dk/se/; Description: {cm:run_VisitWebsite}; Flags: nowait postinstall skipifsilent shellexec runascurrentuser unchecked


[Code]
// Global variables and constants
const installer_mutex_name = 'subtitle_edit_setup_mutex';
var
  is_update: Boolean;


// Check if Subtitle Edit's settings exist
function SettingsExistCheck(): Boolean;
begin
  Result := False;
  if FileExists(ExpandConstant('{app}\Settings.xml')) then
  Result := True;
end;


Procedure CleanUpFiles();
begin
  DeleteFile(ExpandConstant('{app}\Settings.xml'));
end;


function IsUpdate(): Boolean;
begin
  Result := is_update;
end;


function ShouldSkipPage(PageID: Integer): Boolean;
begin
  if IsUpdate then begin
    Case PageID of
      // Hide the license page
      wpLicense: Result := True;
    else
      Result := False;
    end;
  end;
end;


Procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  // When uninstalling ask user to delete Subtitle Edit's logs and settings
  // based on whether these files exist only
  if CurUninstallStep = usUninstall then begin
  if SettingsExistCheck then begin
    if MsgBox(ExpandConstant('{cm:msg_DeleteSettings}'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then begin
       CleanUpFiles;
     end;
      RemoveDir(ExpandConstant('{app}'));
    end;
  end;
end;


function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
begin
  // Create a mutex for the installer and if it's already running then expose a message and stop installation
  if CheckForMutexes(installer_mutex_name) then begin
    if not WizardSilent() then
      MsgBox(ExpandConstant('{cm:msg_SetupIsRunningWarning}'), mbCriticalError, MB_OK);
    exit;
  end;
  CreateMutex(installer_mutex_name);

  // Check if .NET Framework 2.0 is installed and if not offer to download it
  try
    ExpandConstant('{dotnet20}');
    Result := True;
  except
    begin
      if not WizardSilent() then
        if MsgBox(ExpandConstant('{cm:msg_AskToDownNET}'), mbCriticalError, MB_YESNO or MB_DEFBUTTON1) = IDYES then begin
          Result := False;
          ShellExec('open','http://download.microsoft.com/download/5/6/7/567758a3-759e-473e-bf8f-52154438565a/dotnetfx.exe','','',SW_SHOWNORMAL,ewNoWait,ErrorCode);
        end
        else begin
          Result := False;
        end;
      end;
    end;

    is_update := RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SubtitleEdit_is1');

end;


function InitializeUninstall(): Boolean;
begin
  Result := True;
  if CheckForMutexes(installer_mutex_name) then begin
    if not WizardSilent() then
      MsgBox(ExpandConstant('{cm:msg_SetupIsRunningWarning}'), mbCriticalError, MB_OK);
      exit;
   end;
   CreateMutex(installer_mutex_name);
end;
