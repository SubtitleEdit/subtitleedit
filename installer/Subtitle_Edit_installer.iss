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

#define installer_build_number "22"

#define app_copyright "Copyright © 2001-2011, Nikse"
#define VerMajor
#define VerMinor
#define VerBuild
#define VerRevision

#define bindir "..\src\Bin\Release"

;#ifnexist "..\src\bin\Release\SubtitleEdit.exe"
;  #error Compile Subtitle Edit first
;#endif

#expr ParseVersion(bindir + "\SubtitleEdit.exe", VerMajor, VerMinor, VerBuild, VerRevision)
#define app_version str(VerMajor) + "." + str(VerMinor) + "." + str(VerBuild) + "." + str(VerRevision)

; the following simple_app_version is for 3 digit releases, one of the two must be uncommented at a time
;#define simple_app_version str(VerMajor) + "." + str(VerMinor) + "." + str(VerBuild)
#define simple_app_version str(VerMajor) + "." + str(VerMinor)

#define installer_build_date GetDateTimeString('mmm, d yyyy', '', '')

; If you don't define "localize", i.e. comment out the following line then no translations
; for SubtitleEdit or the installer itself will be included in the installer
#define localize


[Setup]
AppID=SubtitleEdit
AppCopyright={#app_copyright}
AppContact=http://www.nikse.dk/SubtitleEdit/
AppName=Subtitle Edit
AppVerName=Subtitle Edit v{#simple_app_version}
AppVersion={#simple_app_version}
AppPublisher=Nikse
AppPublisherURL=http://www.nikse.dk/SubtitleEdit/
AppSupportURL=http://www.nikse.dk/SubtitleEdit/
AppUpdatesURL=http://www.nikse.dk/SubtitleEdit/
UninstallDisplayName=Subtitle Edit v{#simple_app_version}
UninstallDisplayIcon={app}\SubtitleEdit.exe
DefaultDirName={pf}\Subtitle Edit
DefaultGroupName=Subtitle Edit
VersionInfoCompany=Nikse
VersionInfoCopyright={#app_copyright}
VersionInfoDescription=Subtitle Edit Setup
VersionInfoProductName=Subtitle Edit
VersionInfoProductVersion={#simple_app_version}
VersionInfoProductTextVersion={#simple_app_version}
VersionInfoTextVersion={#simple_app_version}
VersionInfoVersion={#simple_app_version}
MinVersion=0,5.1
LicenseFile=..\src\gpl.txt
InfoAfterFile=..\src\Changelog.txt
SetupIconFile=..\src\Icons\SE.ico
WizardImageFile=Icons\WizardImageFile.bmp
WizardSmallImageFile=Icons\WizardSmallImageFile.bmp
OutputDir=.
OutputBaseFilename=SubtitleEdit-{#simple_app_version}-setup
AllowNoIcons=yes
Compression=lzma2/ultra
SolidCompression=yes
ShowTasksTreeLines=yes
DisableReadyPage=yes
PrivilegesRequired=admin
ShowLanguageDialog=yes
DisableDirPage=auto
DisableProgramGroupPage=auto


[Languages]
Name: en; MessagesFile: compiler:Default.isl
#ifdef localize
Name: bg;  MessagesFile: Languages\Bulgarian.isl
Name: cs;  MessagesFile: compiler:Languages\Czech.isl
Name: de;  MessagesFile: compiler:Languages\German.isl
Name: dk;  MessagesFile: compiler:Languages\Danish.isl
Name: es;  MessagesFile: compiler:Languages\Spanish.isl
Name: eu;  MessagesFile: compiler:Languages\Basque.isl
Name: fr;  MessagesFile: compiler:Languages\French.isl
Name: hu;  MessagesFile: compiler:Languages\Hungarian.isl
Name: it;  MessagesFile: compiler:Languages\Italian.isl
Name: ja;  MessagesFile: compiler:Languages\Japanese.isl
Name: nl;  MessagesFile: compiler:Languages\Dutch.isl
Name: pl;  MessagesFile: compiler:Languages\Polish.isl
Name: ro;  MessagesFile: Languages\Romanian.isl
Name: ru;  MessagesFile: compiler:Languages\Russian.isl
Name: srC; MessagesFile: Languages\SerbianCyrillic.isl
Name: srL; MessagesFile: Languages\SerbianLatin.isl
Name: sv;  MessagesFile: Languages\Swedish.isl
#endif

; Include the installer's custom messages
#include "Custom_Messages.iss"


[Messages]
BeveledLabel=Subtitle Edit v{#simple_app_version} by Nikse  -  Setup v{#installer_build_number} built on {#installer_build_date}


[Types]
Name: default;            Description: {cm:types_default}
Name: custom;             Description: {cm:types_custom}; Flags: iscustom


[Components]
Name: main;               Description: Subtitle Edit v{#simple_app_version}; Types: default custom; Flags: fixed
#ifdef localize
Name: translations;       Description: {cm:comp_translations};               Types: default custom; Flags: disablenouninstallwarning
#endif

[Tasks]
Name: desktopicon;        Description: {cm:CreateDesktopIcon};     GroupDescription: {cm:AdditionalIcons}
Name: desktopicon\user;   Description: {cm:tsk_CurrentUser};       GroupDescription: {cm:AdditionalIcons}; Flags: exclusive
Name: desktopicon\common; Description: {cm:tsk_AllUsers};          GroupDescription: {cm:AdditionalIcons}; Flags: unchecked exclusive
Name: quicklaunchicon;    Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked;             OnlyBelowVersion: 0,6.01
Name: reset_dictionaries; Description: {cm:tsk_ResetDictionaries}; GroupDescription: {cm:tsk_Other};       Flags: checkedonce unchecked; Check: DictionariesExistCheck()
Name: reset_settings;     Description: {cm:tsk_ResetSettings};     GroupDescription: {cm:tsk_Other};       Flags: checkedonce unchecked; Check: SettingsExistCheck()


[Files]
Source: psvince.dll;                               DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: ..\src\Changelog.txt;                      DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: ..\src\gpl.txt;                            DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\Hunspellx86.dll;                 DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\Interop.QuartzTypeLib.dll;       DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\NHunspell.dll;                   DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\SubtitleEdit.exe;                DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: Icons\uninstall.ico;                       DestDir: {app}\Icons;                              Flags: ignoreversion; Components: main
Source: {#bindir}\Icons\Find.png;                  DestDir: {app}\Icons;                              Flags: ignoreversion; Components: main
Source: {#bindir}\Icons\Help.png;                  DestDir: {app}\Icons;                              Flags: ignoreversion; Components: main
Source: {#bindir}\Icons\New.png;                   DestDir: {app}\Icons;                              Flags: ignoreversion; Components: main
Source: {#bindir}\Icons\Open.png;                  DestDir: {app}\Icons;                              Flags: ignoreversion; Components: main
Source: {#bindir}\Icons\Replace.png;               DestDir: {app}\Icons;                              Flags: ignoreversion; Components: main
Source: {#bindir}\Icons\Save.png;                  DestDir: {app}\Icons;                              Flags: ignoreversion; Components: main
Source: {#bindir}\Icons\SaveAs.png;                DestDir: {app}\Icons;                              Flags: ignoreversion; Components: main
Source: {#bindir}\Icons\Settings.png;              DestDir: {app}\Icons;                              Flags: ignoreversion; Components: main
Source: {#bindir}\Icons\SpellCheck.png;            DestDir: {app}\Icons;                              Flags: ignoreversion; Components: main
Source: {#bindir}\Icons\VideoToogle.png;           DestDir: {app}\Icons;                              Flags: ignoreversion; Components: main
Source: {#bindir}\Icons\VisualSync.png;            DestDir: {app}\Icons;                              Flags: ignoreversion; Components: main
Source: {#bindir}\Icons\WaveFormToogle.png;        DestDir: {app}\Icons;                              Flags: ignoreversion; Components: main
Source: ..\Tesseract\leptonlib.dll;                DestDir: {app}\Tesseract;                          Flags: ignoreversion; Components: main
Source: ..\Tesseract\tessdata\eng.traineddata;     DestDir: {app}\Tesseract\tessdata;                 Flags: ignoreversion; Components: main
Source: ..\Tesseract\tesseract.exe;                DestDir: {app}\Tesseract;                          Flags: ignoreversion; Components: main

; Uncomment the language files when they are ready to be distributed again
#ifdef localize
Source: {#bindir}\Languages\bg-BG.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\cs-CZ.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\da-DK.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\de-De.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
;Source: {#bindir}\Languages\es-ES.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\eu-ES.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\fr-FR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\hu-HU.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
;Source: {#bindir}\Languages\it-IT.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
;Source: {#bindir}\Languages\ja-JP.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\pl-PL.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\ro-RO.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\ru-RU.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\sr-Cyrl-RS.xml;        DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\sr-Latn-RS.xml;        DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\sv-SE.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\zh-CHS.xml;            DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
#endif

Source: ..\Dictionaries\da_DK_names_etc.xml;       DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\da_DK_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\dan_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\en_GB_names_etc.xml;       DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\en_US.aff;                 DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\en_US.dic;                 DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\en_US_names_etc.xml;       DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\en_US_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\eng_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\names_etc.xml;             DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\nor_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\ru_RU_names_etc.xml;       DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\ru_RU_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\rus_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\swe_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main


[Icons]
Name: {group}\Subtitle Edit;                Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit v{#simple_app_version}; AppUserModelID: Nikse.SubtitleEdit; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0
Name: {group}\Help and Support\Changelog;   Filename: {app}\Changelog.txt;    WorkingDir: {app}; Comment: {cm:sm_com_Changelog}
Name: {group}\Help and Support\Online Help; Filename: http://www.nikse.dk/SubtitleEdit/Help.aspx
Name: {group}\Help and Support\{cm:ProgramOnTheWeb,Subtitle Edit}; Filename: http://www.nikse.dk/SubtitleEdit/;  Comment: {cm:ProgramOnTheWeb,Subtitle Edit}
Name: {group}\{cm:UninstallProgram,Subtitle Edit};                 Filename: {uninstallexe};           Comment: {cm:UninstallProgram,Subtitle Edit}; WorkingDir: {app}; IconFilename: {app}\Icons\uninstall.ico

Name: {commondesktop}\Subtitle Edit;        Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit v{#simple_app_version}; AppUserModelID: Nikse.SubtitleEdit; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: desktopicon\common
Name: {userdesktop}\Subtitle Edit;          Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit v{#simple_app_version}; AppUserModelID: Nikse.SubtitleEdit; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: desktopicon\user
Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\Subtitle Edit; Filename: {app}\SubtitleEdit.exe; Comment: Subtitle Edit v{#simple_app_version}; WorkingDir: {app};     IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: quicklaunchicon


[InstallDelete]
Type: files;      Name: {userdesktop}\Subtitle Edit.lnk;   Check: NOT IsTaskSelected('desktopicon\user')   AND IsUpgrade()
Type: files;      Name: {commondesktop}\Subtitle Edit.lnk; Check: NOT IsTaskSelected('desktopicon\common') AND IsUpgrade()

Type: files;      Name: {userappdata}\Subtitle Edit\Settings.xml; Tasks: reset_settings

; Remove old files from the {app} dir
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
Type: files;      Name: {app}\uninstall.ico

#ifdef localize
; Language files not included anymore
Type: files;      Name: {app}\Languages\sr-Cyrl-CS.xml
Type: files;      Name: {app}\Languages\sr-Latn-CS.xml

; The following language files are incompatible with this SE version,
; so remove them when we are upgrading. If they are updated remove this code.
Type: files;      Name: {app}\Languages\es-ES.xml;      Check: IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\it-IT.xml;      Check: IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\ja-JP.xml;      Check: IsComponentSelected('translations') AND IsUpgrade()

; Cleanup language files if it's an upgrade and the translations are not selected
Type: files;      Name: {app}\Languages\bg-BG.xml;      Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\cs-CZ.xml;      Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\da-DK.xml;      Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\de-De.xml;      Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\es-ES.xml;      Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\eu-ES.xml;      Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\fr-FR.xml;      Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\hu-HU.xml;      Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\it-IT.xml;      Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\ja-JP.xml;      Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\pl-PL.xml;      Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\ro-RO.xml;      Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\ru-RU.xml;      Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\sr-Cyrl-RS.xml; Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\sr-Latn-RS.xml; Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\sv-SE.xml;      Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: files;      Name: {app}\Languages\zh-CHS.xml;     Check: NOT IsComponentSelected('translations') AND IsUpgrade()
Type: dirifempty; Name: {app}\Languages;                Check: NOT IsComponentSelected('translations') AND IsUpgrade()
#endif

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
Filename: {win}\Microsoft.NET\Framework\v2.0.50727\ngen.exe; Parameters: "install ""{app}\SubtitleEdit.exe"""; StatusMsg: {cm:msg_OptimizingPerformance}; Flags: runhidden runascurrentuser skipifdoesntexist
Filename: {app}\SubtitleEdit.exe;  Description: {cm:LaunchProgram,Subtitle Edit}; WorkingDir: {app}; Flags: nowait postinstall skipifsilent unchecked
Filename: http://www.nikse.dk/SubtitleEdit/; Description: {cm:run_VisitWebsite};                     Flags: nowait postinstall skipifsilent shellexec unchecked


[UninstallRun]
Filename: {win}\Microsoft.NET\Framework\v2.0.50727\ngen.exe; Parameters: "uninstall ""{app}\SubtitleEdit.exe"""; Flags: runhidden runascurrentuser skipifdoesntexist


[Code]
// Global variables/constants and general functions
const installer_mutex_name = 'subtitle_edit_setup_mutex';

function IsModuleLoaded(modulename: AnsiString ): Boolean;
external 'IsModuleLoaded@files:psvince.dll stdcall setuponly';

function IsModuleLoadedU(modulename: AnsiString ): Boolean;
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
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\ru_RU_names_etc.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\ru_RU_user.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\rus_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\swe_OCRFixReplaceList.xml'));
  DelTree(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.dic'), False, True, False);
  DelTree(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.aff'), False, True, False);
  RemoveDir(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries'));
end;


function IsUpgrade(): Boolean;
var
  sPrevPath: String;
begin
  sPrevPath := WizardForm.PrevAppDir;
  Result := (sPrevPath <> '');
end;


function ShouldSkipPage(PageID: Integer): Boolean;
begin
  if IsUpgrade() then begin
    // Hide the license page
    if PageID = wpLicense then begin
      Result := True;
    end
    else begin
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
    if SettingsExistCheck() OR DictionariesExistCheck() then begin
      if SuppressibleMsgBox(ExpandConstant('{cm:msg_DeleteSettings}'), mbConfirmation, MB_YESNO OR MB_DEFBUTTON2, IDNO) = IDYES then begin
        CleanUpDictionaries;
        DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Settings.xml'));
      end;

      // Remove the dirs if they are empty
      RemoveDir(ExpandConstant('{app}\Languages'));
      RemoveDir(ExpandConstant('{app}\Spectrograms'));
      RemoveDir(ExpandConstant('{app}\VobSub\English'));
      RemoveDir(ExpandConstant('{app}\VobSub'));
      RemoveDir(ExpandConstant('{app}\WaveForms'));
      RemoveDir(ExpandConstant('{app}'));
      RemoveDir(ExpandConstant('{userappdata}\Subtitle Edit\Spectrograms'));
      RemoveDir(ExpandConstant('{userappdata}\Subtitle Edit\VobSub\English'));
      RemoveDir(ExpandConstant('{userappdata}\Subtitle Edit\VobSub'));
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
  if CheckForMutexes(installer_mutex_name) AND NOT WizardSilent() then begin
    SuppressibleMsgBox(ExpandConstant('{cm:msg_SetupIsRunningWarning}'), mbError, MB_OK, MB_OK);
    Result := False;
  end
  else begin
    Result := True;
    CreateMutex(installer_mutex_name);

    if IsModuleLoaded('SubtitleEdit.exe') then begin
      SuppressibleMsgBox(ExpandConstant('{cm:msg_AppIsRunning}'), mbError, MB_OK, MB_OK);
      Result := False;
    end else
    // Check if .NET Framework 2.0 is installed and if not offer to download it
    try
      ExpandConstant('{dotnet20}');
    except
      begin
        if NOT WizardSilent() then
          if SuppressibleMsgBox(ExpandConstant('{cm:msg_AskToDownNET}'), mbCriticalError, MB_YESNO OR MB_DEFBUTTON1, IDYES) = IDYES then begin
            ShellExec('open','http://download.microsoft.com/download/5/6/7/567758a3-759e-473e-bf8f-52154438565a/dotnetfx.exe','','',SW_SHOWNORMAL,ewNoWait,ErrorCode);
            Result := False;
          end
          else begin
            Result := False;
          end;
        end;
      end;
    end;
end;


function InitializeUninstall(): Boolean;
begin
  if CheckForMutexes(installer_mutex_name) then begin
    SuppressibleMsgBox(ExpandConstant('{cm:msg_SetupIsRunningWarning}'), mbError, MB_OK, MB_OK);
    Result := False;
  end else
    Result := True;

    // Check if app is running during uninstallation
    if IsModuleLoadedU('SubtitleEdit.exe') then begin
      SuppressibleMsgBox(ExpandConstant('{cm:msg_AppIsRunning}'), mbError, MB_OK, MB_OK);
      Result := False;
    end else
      CreateMutex(installer_mutex_name);

      // Unload the psvince.dll in order to be uninstalled
     UnloadDLL(ExpandConstant('{app}\psvince.dll'));
end;
