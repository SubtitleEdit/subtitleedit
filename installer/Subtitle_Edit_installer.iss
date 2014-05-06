;* Subtitle Edit - Installer script
;*
;* Copyright (C) 2010-2014 XhmikosR
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
#if VER < EncodeVer(5,5,4)
  #error Update your Inno Setup version (5.5.4 or newer)
#endif

#if VER > EncodeVer(5,5,4)
  #error Update script to use Turkish from the compiler dir and remove this check
#endif

#ifndef UNICODE
  #error Use Inno Setup unicode
#endif


#define app_copyright "Copyright © 2001-2014, Nikse"
; If you don't define "localize", i.e. comment out the following line then no translations
; for SubtitleEdit or the installer itself will be included in the installer
#define localize
;#define TWO_DIGIT_VER
#define THREE_DIGIT_VER


#define VerMajor
#define VerMinor
#define VerBuild
#define VerRevision

#define bindir     "..\src\bin\Release\x86"
#define bindir_x64 "..\src\bin\Release\x64"

#ifnexist bindir + "\SubtitleEdit.exe"
  #error Compile Subtitle Edit first
#endif

#expr ParseVersion(bindir + "\SubtitleEdit.exe", VerMajor, VerMinor, VerBuild, VerRevision)

#if defined(TWO_DIGIT_VER) && defined(THREE_DIGIT_VER)
  #error You can't define TWO_DIGIT_VER and THREE_DIGIT_VER at the same time
#elif !defined(TWO_DIGIT_VER) && !defined(THREE_DIGIT_VER)
  #error You must define TWO_DIGIT_VER or THREE_DIGIT_VER
#elif defined(TWO_DIGIT_VER)
  #define app_ver       str(VerMajor) + "." + str(VerMinor)
  #define app_ver_full  str(VerMajor) + "." + str(VerMinor) + ".0." + str(VerRevision)
#elif defined(THREE_DIGIT_VER)
  #define app_ver       str(VerMajor) + "." + str(VerMinor) + "." + str(VerBuild)
  #define app_ver_full  str(VerMajor) + "." + str(VerMinor) + "." + str(VerBuild) + "." + str(VerRevision)
#endif

#define quick_launch  "{userappdata}\Microsoft\Internet Explorer\Quick Launch"


[Setup]
AppID=SubtitleEdit
AppCopyright={#app_copyright}
AppContact=http://www.nikse.dk/SubtitleEdit/
AppName=Subtitle Edit
AppVerName=Subtitle Edit {#app_ver}
AppVersion={#app_ver_full}
AppPublisher=Nikse
AppPublisherURL=http://www.nikse.dk/SubtitleEdit/
AppSupportURL=http://www.nikse.dk/SubtitleEdit/
AppUpdatesURL=http://www.nikse.dk/SubtitleEdit/
UninstallDisplayName=Subtitle Edit {#app_ver}
UninstallDisplayIcon={app}\SubtitleEdit.exe
DefaultDirName={pf}\Subtitle Edit
DefaultGroupName=Subtitle Edit
VersionInfoCompany=Nikse
VersionInfoCopyright={#app_copyright}
VersionInfoDescription=Subtitle Edit Setup
VersionInfoProductName=Subtitle Edit
VersionInfoProductVersion={#app_ver_full}
VersionInfoProductTextVersion={#app_ver_full}
VersionInfoTextVersion={#app_ver_full}
VersionInfoVersion={#app_ver_full}
MinVersion=5.1
LicenseFile=..\gpl.txt
InfoAfterFile=..\src\Changelog.txt
SetupIconFile=..\src\Icons\SE.ico
WizardImageFile=Icons\WizardImageFile.bmp
WizardSmallImageFile=Icons\WizardSmallImageFile.bmp
OutputDir=.
OutputBaseFilename=SubtitleEdit-{#app_ver}-setup
AllowNoIcons=yes
Compression=lzma2/ultra
InternalCompressLevel=ultra
SolidCompression=yes
ShowTasksTreeLines=yes
DisableReadyPage=yes
PrivilegesRequired=admin
ShowLanguageDialog=yes
DisableDirPage=auto
DisableProgramGroupPage=auto
ArchitecturesAllowed=x86 x64
ArchitecturesInstallIn64BitMode=x64


[Languages]
Name: en;   MessagesFile: compiler:Default.isl
#ifdef localize
Name: ar;   MessagesFile: Languages\Arabic.isl
Name: bg;   MessagesFile: Languages\Bulgarian.isl
Name: br;   MessagesFile: compiler:Languages\BrazilianPortuguese.isl
Name: ca;   MessagesFile: compiler:Languages\Catalan.isl
Name: cs;   MessagesFile: compiler:Languages\Czech.isl
Name: de;   MessagesFile: compiler:Languages\German.isl
Name: dk;   MessagesFile: compiler:Languages\Danish.isl
Name: el;   MessagesFile: compiler:Languages\Greek.isl
Name: es;   MessagesFile: compiler:Languages\Spanish.isl
Name: eu;   MessagesFile: Languages\Basque.isl
Name: fa;   MessagesFile: Languages\Farsi.isl
Name: fi;   MessagesFile: compiler:Languages\Finnish.isl
Name: fr;   MessagesFile: compiler:Languages\French.isl
Name: hr;   MessagesFile: Languages\Croatian.isl
Name: hu;   MessagesFile: compiler:Languages\Hungarian.isl
Name: it;   MessagesFile: compiler:Languages\Italian.isl
Name: ja;   MessagesFile: compiler:Languages\Japanese.isl
Name: kr;   MessagesFile: Languages\Korean.isl
Name: nl;   MessagesFile: compiler:Languages\Dutch.isl
Name: pl;   MessagesFile: compiler:Languages\Polish.isl
Name: pt;   MessagesFile: compiler:Languages\Portuguese.isl
Name: ro;   MessagesFile: Languages\Romanian.isl
Name: ru;   MessagesFile: compiler:Languages\Russian.isl
Name: srC;  MessagesFile: compiler:Languages\SerbianCyrillic.isl
Name: srL;  MessagesFile: compiler:Languages\SerbianLatin.isl
Name: sv;   MessagesFile: Languages\Swedish.isl
Name: tr;   MessagesFile: Languages\Turkish.isl
Name: vn;   MessagesFile: Languages\Vietnamese.isl
Name: zh;   MessagesFile: Languages\ChineseSimplified.isl
Name: zhTW; MessagesFile: Languages\ChineseTraditional.isl
#endif

; Include the installer's custom messages
#include "Custom_Messages.iss"


[Messages]
BeveledLabel=Subtitle Edit {#app_ver} by Nikse
SetupAppTitle=Setup - Subtitle Edit
SetupWindowTitle=Setup - Subtitle Edit


[Types]
Name: default;            Description: {cm:types_default}
Name: custom;             Description: {cm:types_custom}; Flags: iscustom


[Components]
Name: main;               Description: Subtitle Edit {#app_ver}; Types: default custom; Flags: fixed
#ifdef localize
Name: translations;       Description: {cm:comp_translations};   Types: default custom; Flags: disablenouninstallwarning
#endif

[Tasks]
Name: desktopicon;        Description: {cm:CreateDesktopIcon};     GroupDescription: {cm:AdditionalIcons}
Name: desktopicon\user;   Description: {cm:tsk_CurrentUser};       GroupDescription: {cm:AdditionalIcons}; Flags: exclusive
Name: desktopicon\common; Description: {cm:tsk_AllUsers};          GroupDescription: {cm:AdditionalIcons}; Flags: unchecked exclusive
Name: quicklaunchicon;    Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked;             OnlyBelowVersion: 6.01
Name: reset_dictionaries; Description: {cm:tsk_ResetDictionaries}; GroupDescription: {cm:tsk_Other};       Flags: checkedonce unchecked; Check: DictionariesExistCheck()
Name: reset_settings;     Description: {cm:tsk_ResetSettings};     GroupDescription: {cm:tsk_Other};       Flags: checkedonce unchecked; Check: SettingsExistCheck()


[Files]
Source: psvince.dll;                               DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: ..\Dictionaries\da_DK_names_etc.xml;       DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\da_DK_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\dan_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\de_DE_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\deu_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\en_GB_names_etc.xml;       DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\en_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\en_US.aff;                 DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\en_US.dic;                 DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\en_US_names_etc.xml;       DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\en_US_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\eng_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\fi_fi_names_etc.xml;       DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\fi_fi_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\fin_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\fra_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\names_etc.xml;             DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\nor_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\ru_RU_names_etc.xml;       DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\ru_RU_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\rus_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: ..\Dictionaries\swe_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall; Components: main
Source: {#bindir_x64}\Hunspellx64.dll;             DestDir: {app};                                    Flags: ignoreversion; Components: main; Check: Is64BitInstallMode()
Source: {#bindir}\Hunspellx86.dll;                 DestDir: {app};                                    Flags: ignoreversion; Components: main; Check: not Is64BitInstallMode()
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
Source: {#bindir}\Interop.QuartzTypeLib.dll;       DestDir: {app};                                    Flags: ignoreversion; Components: main

#ifdef localize
Source: {#bindir}\Languages\ar-EG.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\bg-BG.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\ca-ES.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\cs-CZ.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\da-DK.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\de-DE.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\el-GR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\es-AR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\es-ES.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\eu-ES.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\fa-IR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\fi-FI.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\fr-FR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\hr-HR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\hu-HU.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\it-IT.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\ja-JP.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\ko-KR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\nl-NL.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\pl-PL.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\pt-BR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\pt-PT.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\ro-RO.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\ru-RU.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\sr-Cyrl-RS.xml;        DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\sr-Latn-RS.xml;        DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\sv-SE.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\tr-TR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\vi-VN.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\zh-CHS.xml;            DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\zh-tw.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
#endif

Source: {#bindir_x64}\SubtitleEdit.exe;            DestDir: {app};                                    Flags: ignoreversion; Components: main; Check: Is64BitInstallMode()
Source: {#bindir}\SubtitleEdit.exe;                DestDir: {app};                                    Flags: ignoreversion; Components: main; Check: not Is64BitInstallMode()
Source: ..\src\Changelog.txt;                      DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: ..\gpl.txt;                                DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: ..\Tesseract\msvcp90.dll;                  DestDir: {app}\Tesseract;                          Flags: ignoreversion; Components: main
Source: ..\Tesseract\msvcr90.dll;                  DestDir: {app}\Tesseract;                          Flags: ignoreversion; Components: main
Source: ..\Tesseract\tessdata\configs\hocr;        DestDir: {app}\Tesseract\tessdata\configs;         Flags: ignoreversion; Components: main
Source: ..\Tesseract\tessdata\eng.traineddata;     DestDir: {app}\Tesseract\tessdata;                 Flags: ignoreversion; Components: main
Source: ..\Tesseract\tessdata\music.traineddata;   DestDir: {app}\Tesseract\tessdata;                 Flags: ignoreversion; Components: main
Source: ..\Tesseract\tesseract.exe;                DestDir: {app}\Tesseract;                          Flags: ignoreversion; Components: main
Source: Icons\uninstall.ico;                       DestDir: {app}\Icons;                              Flags: ignoreversion; Components: main


[Icons]
Name: {group}\Subtitle Edit;                Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit {#app_ver}; AppUserModelID: Nikse.SubtitleEdit; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0
Name: {group}\Help and Support\Changelog;   Filename: {app}\Changelog.txt;    WorkingDir: {app}; Comment: {cm:sm_com_Changelog}
Name: {group}\Help and Support\Online Help; Filename: http://www.nikse.dk/SubtitleEdit/Help
Name: {group}\Help and Support\{cm:ProgramOnTheWeb,Subtitle Edit}; Filename: http://www.nikse.dk/SubtitleEdit/;  Comment: {cm:ProgramOnTheWeb,Subtitle Edit}
Name: {group}\{cm:UninstallProgram,Subtitle Edit};                 Filename: {uninstallexe};                     Comment: {cm:UninstallProgram,Subtitle Edit}; WorkingDir: {app}; IconFilename: {app}\Icons\uninstall.ico

Name: {commondesktop}\Subtitle Edit;        Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit {#app_ver}; AppUserModelID: Nikse.SubtitleEdit; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: desktopicon\common
Name: {userdesktop}\Subtitle Edit;          Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit {#app_ver}; AppUserModelID: Nikse.SubtitleEdit; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: desktopicon\user
Name: {#quick_launch}\Subtitle Edit;        Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit {#app_ver};                                     IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: quicklaunchicon


[InstallDelete]
Type: files;      Name: {userdesktop}\Subtitle Edit.lnk;   Check: not IsTaskSelected('desktopicon\user')   and IsUpgrade()
Type: files;      Name: {commondesktop}\Subtitle Edit.lnk; Check: not IsTaskSelected('desktopicon\common') and IsUpgrade()
Type: files;      Name: {#quick_launch}\Subtitle Edit.lnk; Check: not IsTaskSelected('quicklaunchicon')    and IsUpgrade(); OnlyBelowVersion: 6.01

Type: files;      Name: {userappdata}\Subtitle Edit\Settings.xml; Tasks: reset_settings

; Remove old files from the {app} dir
Type: files;      Name: {app}\Dictionaries\da_DK_names_etc.xml;       Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\da_DK_user.xml;            Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\dan_OCRFixReplaceList.xml; Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\en_US.aff;                 Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\en_US.dic;                 Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\en_US_names_etc.xml;       Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\en_US_user.xml;            Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\eng_OCRFixReplaceList.xml; Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\names_etc.xml;             Check: IsUpgrade()
Type: dirifempty; Name: {app}\Dictionaries;                           Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.DangAmbigs;                Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.freq-dawg;                 Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.inttemp;                   Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.normproto;                 Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.pffmtable;                 Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.unicharset;                Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.user-words;                Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.word-dawg;                 Check: IsUpgrade()
Type: dirifempty; Name: {app}\TessData;                               Check: IsUpgrade()
Type: files;      Name: {app}\Tesseract\leptonlib.dll;                Check: IsUpgrade()
Type: files;      Name: {app}\Settings.xml;                           Check: IsUpgrade()
Type: files;      Name: {app}\tessnet2_32.dll;                        Check: IsUpgrade()
Type: files;      Name: {app}\uninstall.ico;                          Check: IsUpgrade()

#ifdef localize
; Language files not included anymore
Type: files;      Name: {app}\Languages\sr-Cyrl-CS.xml
Type: files;      Name: {app}\Languages\sr-Latn-CS.xml

; Remove the language files if it's an upgrade and the translations are not selected
Type: files;      Name: {app}\Languages\ar-EG.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\bg-BG.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\ca-ES.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\cs-CZ.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\da-DK.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\de-DE.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\el-GR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\es-AR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\es-ES.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\eu-ES.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\fa-IR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\fi-FI.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\fr-FR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\hr-HR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\hu-HU.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\it-IT.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\ja-JP.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\ko-KR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\nl-NL.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\pl-PL.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\pt-BR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\pt-PT.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\ro-RO.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\ru-RU.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\sr-Cyrl-RS.xml; Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\sr-Latn-RS.xml; Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\sv-SE.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\tr-TR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\vi-VN.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\zh-CHS.xml;     Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\zh-tw.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: dirifempty; Name: {app}\Languages;                Check: not IsComponentSelected('translations') and IsUpgrade()
#endif


[Run]
Filename: {win}\Microsoft.NET\Framework\v4.0.30319\ngen.exe;   Parameters: "install ""{app}\SubtitleEdit.exe"""; StatusMsg: {cm:msg_OptimizingPerformance}; Flags: runhidden runascurrentuser skipifdoesntexist; Check: not Is64BitInstallMode()
Filename: {win}\Microsoft.NET\Framework64\v4.0.30319\ngen.exe; Parameters: "install ""{app}\SubtitleEdit.exe"""; StatusMsg: {cm:msg_OptimizingPerformance}; Flags: runhidden runascurrentuser skipifdoesntexist; Check: Is64BitInstallMode()
Filename: {app}\SubtitleEdit.exe;            Description: {cm:LaunchProgram,Subtitle Edit}; WorkingDir: {app}; Flags: nowait postinstall skipifsilent unchecked
Filename: http://www.nikse.dk/SubtitleEdit/; Description: {cm:run_VisitWebsite};                               Flags: nowait postinstall skipifsilent unchecked shellexec


[UninstallRun]
Filename: {win}\Microsoft.NET\Framework\v4.0.30319\ngen.exe;   Parameters: "uninstall ""{app}\SubtitleEdit.exe"""; Flags: runhidden runascurrentuser skipifdoesntexist; Check: not Is64BitInstallMode()
Filename: {win}\Microsoft.NET\Framework64\v4.0.30319\ngen.exe; Parameters: "uninstall ""{app}\SubtitleEdit.exe"""; Flags: runhidden runascurrentuser skipifdoesntexist; Check: Is64BitInstallMode()


[Code]
// Global variables/constants and general functions
const installer_mutex = 'subtitle_edit_setup_mutex';

function IsModuleLoaded(modulename: AnsiString): Boolean;
external 'IsModuleLoaded@files:psvince.dll stdcall setuponly';

function IsModuleLoadedU(modulename: AnsiString): Boolean;
external 'IsModuleLoaded@{app}\psvince.dll stdcall uninstallonly';


// Check if Subtitle Edit's settings exist
function SettingsExistCheck(): Boolean;
begin
  if FileExists(ExpandConstant('{userappdata}\Subtitle Edit\Settings.xml')) then
    Result := True
  else
    Result := False;
end;


// Check if Dictionaries exist
function DictionariesExistCheck(): Boolean;
var
  FindRec: TFindRec;
begin
  if FindFirst(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.aff'), FindRec) or
  FindFirst(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.dic'), FindRec) or
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
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\de_DE_user.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\deu_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\en_GB_names_etc.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\en_US_names_etc.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\en_US_user.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\eng_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\fi_fi_names_etc.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\fi_fi_user.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\fin_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\fra_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\names_etc.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\nor_OCRFixReplaceList.xml'));
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
  // Hide the license page
  if IsUpgrade() and (PageID = wpLicense) then
    Result := True;
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
    if IsTaskSelected('reset_dictionaries') then
      CleanUpDictionaries();
  end;
end;


procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  // When uninstalling ask user to delete Subtitle Edit's dictionaries and settings
  // based on whether these files exist only
  if CurUninstallStep = usUninstall then begin
    if SettingsExistCheck() or DictionariesExistCheck() then begin
      if SuppressibleMsgBox(CustomMessage('msg_DeleteSettings'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2, IDNO) = IDYES then begin
        CleanUpDictionaries();
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
  iErrorCode, iMsgBoxResult: Integer;
begin
  // Create a mutex for the installer and if it's already running then expose a message and stop installation
  if CheckForMutexes(installer_mutex) and not WizardSilent() then begin
    SuppressibleMsgBox(CustomMessage('msg_SetupIsRunningWarning'), mbError, MB_OK, MB_OK);
    Result := False;
  end
  else begin
    Result := True;
    CreateMutex(installer_mutex);

    while IsModuleLoaded('SubtitleEdit.exe') and (iMsgBoxResult <> IDCANCEL) do
      iMsgBoxResult := SuppressibleMsgBox(CustomMessage('msg_AppIsRunning'), mbError, MB_OKCANCEL, IDCANCEL);

    if iMsgBoxResult = IDCANCEL then
      Result := False;

    // Check if .NET Framework 4.0 is installed and if not offer to download it
    try
      ExpandConstant('{dotnet40}');
    except
      begin
        if not WizardSilent() then begin
          if SuppressibleMsgBox(CustomMessage('msg_AskToDownNET'), mbCriticalError, MB_YESNO or MB_DEFBUTTON1, IDNO) = IDYES then
            ShellExec('open','http://download.microsoft.com/download/5/6/2/562A10F9-C9F4-4313-A044-9C94E0A8FAC8/dotNetFx40_Client_x86_x64.exe','','',SW_SHOWNORMAL,ewNoWait,iErrorCode);
          Result := False;
        end;
      end;
    end;
  end;
end;


function InitializeUninstall(): Boolean;
var
  iMsgBoxResult: Integer;
begin
  if CheckForMutexes(installer_mutex) then begin
    SuppressibleMsgBox(CustomMessage('msg_SetupIsRunningWarning'), mbError, MB_OK, MB_OK);
    Result := False;
  end
  else begin
    Result := True;
    CreateMutex(installer_mutex);

    // Check if app is running during uninstallation
    while IsModuleLoadedU('SubtitleEdit.exe') and (iMsgBoxResult <> IDCANCEL) do
      iMsgBoxResult := SuppressibleMsgBox(CustomMessage('msg_AppIsRunningUninstall'), mbError, MB_OKCANCEL, IDCANCEL);

    if iMsgBoxResult = IDCANCEL then
      Result := False;

    // Unload the psvince.dll in order to be uninstalled
    UnloadDLL(ExpandConstant('{app}\psvince.dll'));
  end;
end;
