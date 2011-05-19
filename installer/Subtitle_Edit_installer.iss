;* Subtitle Edit - Installer script
;*
;* Copyright (C) 2010-2011 XhmikosR
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

; Requirements:
; Inno Setup Unicode: http://www.jrsoftware.org/isdl.php


; preprocessor checks
#if VER < 0x05040200
  #error Update your Inno Setup version
#endif

;#ifnexist "..\src\bin\Release\SubtitleEdit.exe"
;  #error Compile Subtitle Edit first
;#endif


#define installer_build_number "14"

#define VerMajor
#define VerMinor
#define VerBuild
#define VerRevision

#expr ParseVersion("..\src\bin\Release\SubtitleEdit.exe", VerMajor, VerMinor, VerBuild, VerRevision)
#define app_version str(VerMajor) + "." + str(VerMinor) + "." + str(VerBuild) + "." + str(VerRevision)

; the following simple_app_version is for 3 digit releases, one of the two must be uncommented at a time
;#define simple_app_version str(VerMajor) + "." + str(VerMinor) + "." + str(VerBuild)
#define simple_app_version str(VerMajor) + "." + str(VerMinor)

#define installer_build_date GetDateTimeString('mmm, d yyyy', '', '')


[Setup]
AppID=SubtitleEdit
AppCopyright=Copyright © 2001-2011, Nikse
AppContact=http://www.nikse.dk/se/
AppName=Subtitle Edit
AppVerName=Subtitle Edit v{#simple_app_version}
AppVersion={#simple_app_version}
AppPublisher=Nikse
AppPublisherURL=http://www.nikse.dk/se/
AppSupportURL=http://www.nikse.dk/se/
AppUpdatesURL=http://www.nikse.dk/se/
UninstallDisplayName=Subtitle Edit v{#simple_app_version}
UninstallDisplayIcon={app}\SubtitleEdit.exe
DefaultDirName={pf}\Subtitle Edit
DefaultGroupName=Subtitle Edit
VersionInfoCompany=Nikse
VersionInfoCopyright=Copyright © 2001-2011, Nikse
VersionInfoDescription=Subtitle Edit v{#simple_app_version} Setup
VersionInfoTextVersion={#simple_app_version}
VersionInfoVersion={#simple_app_version}
VersionInfoProductName=Subtitle Edit
VersionInfoProductVersion={#simple_app_version}
VersionInfoProductTextVersion={#simple_app_version}
MinVersion=0,5.1
LicenseFile=..\src\gpl.txt
InfoAfterFile=..\src\Changelog.txt
SetupIconFile=..\src\Icons\SE.ico
WizardSmallImageFile=Icons\WizardSmallImageFile.bmp
OutputDir=.
OutputBaseFilename=SubtitleEdit-{#simple_app_version}-setup
AllowNoIcons=yes
Compression=lzma2/ultra
SolidCompression=yes
EnableDirDoesntExistWarning=no
DirExistsWarning=auto
ShowTasksTreeLines=yes
DisableReadyPage=yes
PrivilegesRequired=admin
ShowLanguageDialog=yes
DisableDirPage=auto
DisableProgramGroupPage=auto


[Languages]
Name: en; MessagesFile: compiler:Default.isl
Name: bg; MessagesFile: Languages\Bulgarian.isl
Name: cs; MessagesFile: compiler:Languages\Czech.isl
Name: de; MessagesFile: compiler:Languages\German.isl
Name: dk; MessagesFile: compiler:Languages\Danish.isl
Name: es; MessagesFile: compiler:Languages\Spanish.isl
Name: fr; MessagesFile: compiler:Languages\French.isl
Name: hu; MessagesFile: compiler:Languages\Hungarian.isl
Name: it; MessagesFile: compiler:Languages\Italian.isl
Name: ja; MessagesFile: compiler:Languages\Japanese.isl
Name: nl; MessagesFile: compiler:Languages\Dutch.isl
Name: pl; MessagesFile: compiler:Languages\Polish.isl
Name: ro; MessagesFile: Languages\Romanian.isl
Name: sv; MessagesFile: Languages\Swedish.isl

; Include the installer's custom messages
#include "Custom_Messages.iss"


[Messages]
BeveledLabel=Subtitle Edit v{#simple_app_version} by Nikse, Setup v{#installer_build_number} built on {#installer_build_date}


[Tasks]
Name: desktopicon;        Description: {cm:CreateDesktopIcon};     GroupDescription: {cm:AdditionalIcons}
Name: desktopicon\user;   Description: {cm:tsk_CurrentUser};       GroupDescription: {cm:AdditionalIcons}; Flags: exclusive
Name: desktopicon\common; Description: {cm:tsk_AllUsers};          GroupDescription: {cm:AdditionalIcons}; Flags: unchecked exclusive
Name: quicklaunchicon;    Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked;             OnlyBelowVersion: 0,6.01
Name: reset_dictionaries; Description: {cm:tsk_ResetDictionaries}; GroupDescription: {cm:tsk_Other};       Flags: checkedonce unchecked; Check: DictionariesExistCheck()
Name: reset_settings;     Description: {cm:tsk_ResetSettings};     GroupDescription: {cm:tsk_Other};       Flags: checkedonce unchecked; Check: SettingsExistCheck()


[Files]
Source: psvince.dll;                                  DestDir: {app};                                    Flags: ignoreversion
Source: ..\src\Changelog.txt;                         DestDir: {app};                                    Flags: ignoreversion
Source: ..\src\gpl.txt;                               DestDir: {app};                                    Flags: ignoreversion
Source: ..\src\Bin\Release\Hunspellx86.dll;           DestDir: {app};                                    Flags: ignoreversion
Source: ..\src\Bin\Release\Interop.QuartzTypeLib.dll; DestDir: {app};                                    Flags: ignoreversion
Source: ..\src\Bin\Release\NHunspell.dll;             DestDir: {app};                                    Flags: ignoreversion
Source: ..\src\Bin\Release\SubtitleEdit.exe;          DestDir: {app};                                    Flags: ignoreversion
Source: Icons\uninstall.ico;                          DestDir: {app};                                    Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\Find.png;            DestDir: {app}\Icons;                              Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\Help.png;            DestDir: {app}\Icons;                              Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\New.png;             DestDir: {app}\Icons;                              Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\Open.png;            DestDir: {app}\Icons;                              Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\Replace.png;         DestDir: {app}\Icons;                              Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\Save.png;            DestDir: {app}\Icons;                              Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\SaveAs.png;          DestDir: {app}\Icons;                              Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\Settings.png;        DestDir: {app}\Icons;                              Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\SpellCheck.png;      DestDir: {app}\Icons;                              Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\VideoToogle.png;     DestDir: {app}\Icons;                              Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\VisualSync.png;      DestDir: {app}\Icons;                              Flags: ignoreversion
Source: ..\src\Bin\Release\Icons\WaveFormToogle.png;  DestDir: {app}\Icons;                              Flags: ignoreversion
Source: ..\Dictionaries\da_DK_names_etc.xml;          DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall
Source: ..\Dictionaries\da_DK_user.xml;               DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall
Source: ..\Dictionaries\dan_OCRFixReplaceList.xml;    DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall
Source: ..\Dictionaries\en_US.aff;                    DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion
Source: ..\Dictionaries\en_US.dic;                    DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion
Source: ..\Dictionaries\en_US_names_etc.xml;          DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall
Source: ..\Dictionaries\en_US_user.xml;               DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall
Source: ..\Dictionaries\eng_OCRFixReplaceList.xml;    DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall
Source: ..\Dictionaries\names_etc.xml;                DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall
Source: ..\Tesseract\tessdata\eng.traineddata;        DestDir: {app}\Tesseract\tessdata;                 Flags: ignoreversion
Source: ..\Tesseract\leptonlib.dll;                   DestDir: {app}\Tesseract;                          Flags: ignoreversion
Source: ..\Tesseract\tesseract.exe;                   DestDir: {app}\Tesseract;                          Flags: ignoreversion


[Icons]
Name: {group}\Subtitle Edit;                Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit v{#simple_app_version}; AppUserModelID: Nikse.SubtitleEdit; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0
Name: {group}\Help and Support\Changelog;   Filename: {app}\Changelog.txt;    WorkingDir: {app}; Comment: {cm:sm_com_Changelog}
Name: {group}\Help and Support\Online Help; Filename: http://www.nikse.dk/se/Help.aspx
Name: {group}\Help and Support\{cm:ProgramOnTheWeb,Subtitle Edit}; Filename: http://www.nikse.dk/se/;  Comment: {cm:ProgramOnTheWeb,Subtitle Edit}
Name: {group}\{cm:UninstallProgram,Subtitle Edit};                 Filename: {uninstallexe};     Comment: {cm:UninstallProgram,Subtitle Edit}; WorkingDir: {app}; IconFilename: {app}\uninstall.ico

Name: {commondesktop}\Subtitle Edit;        Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit v{#simple_app_version}; AppUserModelID: Nikse.SubtitleEdit; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: desktopicon\common
Name: {userdesktop}\Subtitle Edit;          Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit v{#simple_app_version}; AppUserModelID: Nikse.SubtitleEdit; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: desktopicon\user
Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\Subtitle Edit; Filename: {app}\SubtitleEdit.exe; Comment: Subtitle Edit v{#simple_app_version}; WorkingDir: {app};     IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: quicklaunchicon


[InstallDelete]
Type: files;      Name: {userdesktop}\Subtitle Edit.lnk;          Check: NOT IsTaskSelected('desktopicon\user')   AND IsUpdate()
Type: files;      Name: {commondesktop}\Subtitle Edit.lnk;        Check: NOT IsTaskSelected('desktopicon\common') AND IsUpdate()

Type: files;      Name: {userappdata}\Subtitle Edit\Settings.xml; Tasks: reset_settings

; remove old files from the {app} dir
Type: files;      Name: {app}\Dictionaries\da_DK_names_etc.xml
Type: files;      Name: {app}\Dictionaries\da_DK_user.xml
Type: files;      Name: {app}\Dictionaries\dan_OCRFixReplaceList.xml
Type: files;      Name: {app}\Dictionaries\en_US.aff
Type: files;      Name: {app}\Dictionaries\en_US.dic
Type: files;      Name: {app}\Dictionaries\en_US_names_etc.xml
Type: files;      Name: {app}\Dictionaries\en_US_user.xml
Type: files;      Name: {app}\Dictionaries\eng_OCRFixReplaceList.xml
Type: files;      Name: {app}\Dictionaries\names_etc.xml
Type: dirifempty; Name: {app}\Dictionaries

Type: files;      Name: {app}\tessnet2_32.dll
Type: files;      Name: {app}\TessData\eng.DangAmbigs
Type: files;      Name: {app}\TessData\eng.freq-dawg
Type: files;      Name: {app}\TessData\eng.inttemp
Type: files;      Name: {app}\TessData\eng.normproto
Type: files;      Name: {app}\TessData\eng.pffmtable
Type: files;      Name: {app}\TessData\eng.unicharset
Type: files;      Name: {app}\TessData\eng.user-words
Type: files;      Name: {app}\TessData\eng.word-dawg
Type: dirifempty; Name: {app}\TessData

Type: files;      Name: {app}\Settings.xml


[Run]
Filename: {app}\SubtitleEdit.exe;  Description: {cm:LaunchProgram,Subtitle Edit}; WorkingDir: {app}; Flags: nowait postinstall skipifsilent runascurrentuser unchecked
Filename: http://www.nikse.dk/se/; Description: {cm:run_VisitWebsite};                               Flags: nowait postinstall skipifsilent shellexec runascurrentuser unchecked


[Code]
// Global variables and constants
const installer_mutex_name = 'subtitle_edit_setup_mutex';
var
  is_update: Boolean;


// General functions
function IsModuleLoaded(modulename: AnsiString ):  Boolean;
external 'IsModuleLoaded@files:psvince.dll stdcall setuponly';


function IsModuleLoadedU(modulename: AnsiString ):  Boolean;
external 'IsModuleLoaded@{app}\psvince.dll stdcall uninstallonly';


// Check if Subtitle Edit's settings exist
function SettingsExistCheck(): Boolean;
begin
  if FileExists(ExpandConstant('{userappdata}\Subtitle Edit\Settings.xml')) then begin
    Result := True;
  end else
    Result := False;
end;


// Check if Dictionaries exist
function DictionariesExistCheck(): Boolean;
var
  FindRec: TFindRec;
begin
  if FindFirst(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.aff'), FindRec) OR
  FindFirst(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.dic'), FindRec) OR
  FindFirst(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.xml'), FindRec) then begin
    Result := True;
    FindClose(FindRec);
  end else
    Result := False;
end;


procedure CleanUpDictionaries();
begin
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\da_DK_names_etc.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\da_DK_user.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\dan_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\en_US_names_etc.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\en_US_user.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\eng_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\names_etc.xml'));
  DelTree(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.dic'), False, True, False);
  DelTree(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.aff'), False, True, False);
  RemoveDir(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries'));
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
  if CurStep = ssInstall then begin
    if IsTaskSelected('reset_dictionaries') then begin
      CleanUpDictionaries;
    end;
  end;
end;


procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  // When uninstalling ask user to delete Subtitle Edit's dictionaries and settings
  // based on whether these files exist only
  if CurUninstallStep = usUninstall then begin
    if SettingsExistCheck OR DictionariesExistCheck then begin
      if MsgBox(ExpandConstant('{cm:msg_DeleteSettings}'), mbConfirmation, MB_YESNO OR MB_DEFBUTTON2) = IDYES then begin
        CleanUpDictionaries;
        DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Settings.xml'));
      end;

      RemoveDir(ExpandConstant('{app}\WaveForms'));
      RemoveDir(ExpandConstant('{app}'));
      RemoveDir(ExpandConstant('{userappdata}\Subtitle Edit\WaveForms'));
      RemoveDir(ExpandConstant('{userappdata}\Subtitle Edit'));

    end;
  end;
end;


function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
begin
  // Create a mutex for the installer and if it's already running then expose a message and stop installation
  if CheckForMutexes(installer_mutex_name) then begin
    if NOT WizardSilent() then
      MsgBox(ExpandConstant('{cm:msg_SetupIsRunningWarning}'), mbError, MB_OK);
    exit;
  end;

  CreateMutex(installer_mutex_name);

  if IsModuleLoaded( 'SubtitleEdit.exe' ) then begin
    MsgBox(ExpandConstant('{cm:msg_AppIsRunning}'), mbError, MB_OK );
    Result := False;
    Abort;
  end else
    Result := True;

  // Check if .NET Framework 2.0 is installed and if not offer to download it
  try
    ExpandConstant('{dotnet20}');
    Result := True;
  except
    begin
      if NOT WizardSilent() then
        if MsgBox(ExpandConstant('{cm:msg_AskToDownNET}'), mbCriticalError, MB_YESNO OR MB_DEFBUTTON1) = IDYES then begin
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
  // Check if app is running during uninstallation
  if IsModuleLoadedU( 'SubtitleEdit.exe' ) then begin
    MsgBox(ExpandConstant('{cm:msg_AppIsRunning}'), mbError, MB_OK );
    Result := False;
  end else
    Result := True;

  if NOT IsModuleLoadedU( 'SubtitleEdit.exe' ) then begin
    if CheckForMutexes(installer_mutex_name) then begin
      MsgBox(ExpandConstant('{cm:msg_SetupIsRunningWarning}'), mbError, MB_OK);
      Result := False;
    end
    else begin
      CreateMutex(installer_mutex_name);
      Result := True;
    end;
  end;

  // Unload the psvince.dll in order to be uninstalled
  UnloadDLL(ExpandConstant('{app}\psvince.dll'));
end;
