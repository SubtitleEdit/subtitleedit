;* Subtitle Edit - Installer script
;*
;* Copyright (C) 2010-2016 XhmikosR
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
;* along with Subtitle Edit.  If not, see <https://www.gnu.org/licenses/>.

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

; If you don't define "localize", i.e. comment out the following line then no translations
; for SubtitleEdit or the installer itself will be included in the installer
#define localize
;#define TWO_DIGIT_VER
#define THREE_DIGIT_VER


#define VerMajor
#define VerMinor
#define VerBuild
#define VerRevision

#define bindir "..\src\ui\bin\Release"

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
#define keySE "Software\Nikse\SubtitleEdit"
#define keyCl "Software\Classes"
#define keyApps "Software\Classes\Applications"
#define keyRegApps "Software\RegisteredApplications"
#define keyAppPaths "Software\Microsoft\Windows\CurrentVersion\App Paths"
#define keyMuiCache "Software\Classes\Local Settings\MuiCache"

[Setup]
AppID=SubtitleEdit
AppName={#app_name}
AppVersion={#app_ver_full}
AppVerName={#app_name} {#app_ver}

AppCopyright={#app_copyright} {#app_copyright_start} {#app_copyright_end}
AppPublisher={#app_copyright}

AppContact=https://www.nikse.dk/SubtitleEdit/Help
AppPublisherURL=https://www.nikse.dk/SubtitleEdit/
AppSupportURL=https://www.nikse.dk/SubtitleEdit/
AppUpdatesURL=https://www.nikse.dk/SubtitleEdit/

VersionInfoVersion={#app_ver_full}
VersionInfoDescription={#app_name} installer
VersionInfoProductName={#app_name}

UninstallDisplayName={#app_name}
UninstallDisplayIcon={app}\SubtitleEdit.exe

WizardStyle=modern
ShowLanguageDialog=yes
UsePreviousLanguage=no

DefaultDirName={pf}\{#app_name}
DefaultGroupName={#app_name}
MinVersion=6.0
LicenseFile=..\LICENSE.txt
InfoAfterFile=..\Changelog.txt
SetupIconFile=..\src\ui\Icons\SE.ico
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
Name: "en";   MessagesFile: "compiler:Default.isl"
#ifdef localize
Name: "ar";   MessagesFile: "Languages\Arabic.isl"
Name: "bg";   MessagesFile: "Languages\Bulgarian.isl"
Name: "ca";   MessagesFile: "compiler:Languages\Catalan.isl"
Name: "cs";   MessagesFile: "compiler:Languages\Czech.isl"
Name: "da";   MessagesFile: "compiler:Languages\Danish.isl"
Name: "de";   MessagesFile: "compiler:Languages\German.isl"
Name: "el";   MessagesFile: "Languages\Greek.isl"
Name: "es";   MessagesFile: "compiler:Languages\Spanish.isl"
Name: "eu";   MessagesFile: "Languages\Basque.isl"
Name: "fa";   MessagesFile: "Languages\Farsi.isl"
Name: "fi";   MessagesFile: "compiler:Languages\Finnish.isl"
Name: "fr";   MessagesFile: "compiler:Languages\French.isl"
Name: "hr";   MessagesFile: "Languages\Croatian.isl"
Name: "hu";   MessagesFile: "Languages\Hungarian.isl"
Name: "id";   MessagesFile: "Languages\Indonesian.isl"
Name: "it";   MessagesFile: "compiler:Languages\Italian.isl"
Name: "ja";   MessagesFile: "compiler:Languages\Japanese.isl"
Name: "ko";   MessagesFile: "Languages\Korean.isl"
Name: "mk";   MessagesFile: "Languages\Macedonian.isl"
Name: "nl";   MessagesFile: "compiler:Languages\Dutch.isl"
Name: "no";   MessagesFile: "compiler:Languages\Norwegian.isl"
Name: "pl";   MessagesFile: "compiler:Languages\Polish.isl"
Name: "pt";   MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "ptBR"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "ro";   MessagesFile: "Languages\Romanian.isl"
Name: "ru";   MessagesFile: "compiler:Languages\Russian.isl"
Name: "sl";   MessagesFile: "compiler:Languages\Slovenian.isl"
Name: "srC";  MessagesFile: "Languages\SerbianCyrillic.isl"
Name: "srL";  MessagesFile: "Languages\SerbianLatin.isl"
Name: "sv";   MessagesFile: "Languages\Swedish.isl"
Name: "th";   MessagesFile: "Languages\Thai.isl"
Name: "tr";   MessagesFile: "compiler:Languages\Turkish.isl"
Name: "uk";   MessagesFile: "compiler:Languages\Ukrainian.isl"
Name: "vi";   MessagesFile: "Languages\Vietnamese.isl"
Name: "zh";   MessagesFile: "Languages\ChineseSimplified.isl"
Name: "zhTW"; MessagesFile: "Languages\ChineseTraditional.isl"
#endif


; Include the installer's custom messages
#include "Subtitle_Edit_Localization.iss"

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
Name: reset_dictionaries; Description: {cm:tsk_ResetDictionaries}; GroupDescription: {cm:tsk_Other};       Flags: checkedonce unchecked; Check: DictionariesExist()
Name: reset_settings;     Description: {cm:tsk_ResetSettings};     GroupDescription: {cm:tsk_Other};       Flags: checkedonce unchecked; Check: SettingsExist()
Name: associate_common;   Description: {cm:tsk_SetFileTypes};      GroupDescription: {cm:tsk_Other};

[Files]
Source: ..\Dictionaries\dan_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\deu_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\eng_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\fin_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\fra_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\hrb_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\hrv_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\hun_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\mkd_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\nld_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\nob_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\nor_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\pol_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\por_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\rus_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\spa_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\srp_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\swe_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\da_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\de_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\en_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\es_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\fi_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\fr_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\hr_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\nb_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\nl_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\pt_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\ru_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\names.xml;                 DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\ar_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\bg_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\da_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\el_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\en_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\es_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\hr_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\mk_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\pt_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\ru_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\sr_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\da_DK_se.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\de_DE_se.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\en_US_se.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\en_US_se.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion; Components: main
Source: ..\Dictionaries\es_MX_se.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\fi_FI_se.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\nl_NL_se.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\pt_PT_se.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\ru_RU_se.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\it_IT_se.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\en_US.aff;                 DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\en_US.dic;                 DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\dan_WordSplitList.txt;     DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\eng_WordSplitList.txt;     DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\fra_WordSplitList.txt;     DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\ita_WordSplitList.txt;     DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\pol_WordSplitList.txt;     DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\spa_WordSplitList.txt;     DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\deu_Nouns.txt;             DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\da_interjections_se.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\en_interjections_se.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\es_interjections_se.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Dictionaries\fr_interjections_se.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion onlyifdoesntexist; Components: main


Source: ..\Ocr\Latin.db;                           DestDir: {userappdata}\Subtitle Edit\Ocr;          Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Ocr\Latin.nocr;                         DestDir: {userappdata}\Subtitle Edit\Ocr;          Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main

Source: ..\preview.mkv;                            DestDir: {userappdata}\Subtitle Edit;              Flags: ignoreversion onlyifdoesntexist; Components: main

Source: ..\Icons\ass.ico;                          DestDir: {app}\Icons;                              Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\dfxp.ico;                         DestDir: {app}\Icons;                              Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\lrc.ico;                          DestDir: {app}\Icons;                              Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\sbv.ico;                          DestDir: {app}\Icons;                              Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\srt.ico;                          DestDir: {app}\Icons;                              Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\ssa.ico;                          DestDir: {app}\Icons;                              Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\stl.ico;                          DestDir: {app}\Icons;                              Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\sub.ico;                          DestDir: {app}\Icons;                              Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\sup.ico;                          DestDir: {app}\Icons;                              Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\vtt.ico;                          DestDir: {app}\Icons;                              Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\smi.ico;                          DestDir: {app}\Icons;                              Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\itt.ico;                          DestDir: {app}\Icons;                              Flags: ignoreversion onlyifdoesntexist; Components: main

Source: ..\Icons\DarkTheme\*.png;                  DestDir: {userappdata}\Subtitle Edit\Icons\DarkTheme;    Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\DefaultTheme\*.png;               DestDir: {userappdata}\Subtitle Edit\Icons\DefaultTheme; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\Legacy\*.png;                     DestDir: {userappdata}\Subtitle Edit\Icons\Legacy;       Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\Black\*.png;                      DestDir: {userappdata}\Subtitle Edit\Icons\Black;        Flags: ignoreversion onlyifdoesntexist; Components: main

Source: ..\Icons\DarkTheme\VideoPlayer\*.png;      DestDir: {userappdata}\Subtitle Edit\Icons\DarkTheme\VideoPlayer;    Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\DefaultTheme\VideoPlayer\*.png;   DestDir: {userappdata}\Subtitle Edit\Icons\DefaultTheme\VideoPlayer; Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\Legacy\VideoPlayer\*.png;         DestDir: {userappdata}\Subtitle Edit\Icons\Legacy\VideoPlayer;       Flags: ignoreversion onlyifdoesntexist; Components: main
Source: ..\Icons\Black\VideoPlayer\*.png;          DestDir: {userappdata}\Subtitle Edit\Icons\Black\VideoPlayer;        Flags: ignoreversion onlyifdoesntexist; Components: main

#ifdef localize
Source: {#bindir}\Languages\ar-EG.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\bg-BG.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\br-FR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\ca-ES.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\cs-CZ.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\da-DK.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\de-DE.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\el-GR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\es-AR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\es-ES.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\es-MX.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\eu-ES.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\fa-IR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\fi-FI.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\fr-FR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\hr-HR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\hu-HU.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\id-ID.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\it-IT.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\ja-JP.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\ko-KR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\mk-MK.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\nb-NO.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\nl-NL.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\pl-PL.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\pt-BR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\pt-PT.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\ro-RO.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\ru-RU.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\sl-SI.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\sr-Cyrl-RS.xml;        DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\sr-Latn-RS.xml;        DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\sv-SE.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\th-TH.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\tr-TR.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\uk-UA.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\vi-VN.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\zh-Hans.xml;           DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
Source: {#bindir}\Languages\zh-TW.xml;             DestDir: {app}\Languages;                          Flags: ignoreversion; Components: translations
#endif

Source: {#bindir}\SubtitleEdit.exe;                DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\Hunspellx64.dll;                 DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\Hunspellx86.dll;                 DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\libse.dll;                       DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\zlib.net.dll;                    DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\NHunspell.dll;                   DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\UtfUnknown.dll;                  DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\Vosk.dll;                        DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\NCalc.dll;                       DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: ..\src\ui\DLLs\Interop.QuartzTypeLib.dll;  DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\Newtonsoft.Json.dll;             DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\System.Net.Http.Extensions.dll;  DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\System.Net.Http.Primitives.dll;  DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\NAudio.Core.dll;                 DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\NAudio.WinMM.dll;                DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\Microsoft.Win32.Registry.dll;    DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: ..\Changelog.txt;                          DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: ..\LICENSE.txt;                            DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: Icons\uninstall.ico;                       DestDir: {app}\Icons;                              Flags: ignoreversion; Components: main

Source: ..\Tesseract302\tessdata\configs\hocr;       DestDir: {app}\Tesseract302\tessdata\configs;    Flags: ignoreversion; Components: main
Source: ..\Tesseract302\tessdata\eng.traineddata;    DestDir: {app}\Tesseract302\tessdata;            Flags: ignoreversion; Components: main
Source: ..\Tesseract302\tessdata\music.traineddata;  DestDir: {app}\Tesseract302\tessdata;            Flags: ignoreversion; Components: main
Source: ..\Tesseract302\tesseract.exe;               DestDir: {app}\Tesseract302;                     Flags: ignoreversion; Components: main
Source: ..\Tesseract302\msvcp90.dll;                 DestDir: {app}\Tesseract302;                     Flags: ignoreversion; Components: main
Source: ..\Tesseract302\msvcr90.dll;                 DestDir: {app}\Tesseract302;                     Flags: ignoreversion; Components: main


[Icons]
Name: {group}\Subtitle Edit;                Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit {#app_ver}; AppUserModelID: Nikse.SubtitleEdit; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0
Name: {group}\Help and Support\Changelog;   Filename: {app}\Changelog.txt;    WorkingDir: {app}; Comment: {cm:sm_com_Changelog}
Name: {group}\Help and Support\Online Help; Filename: https://www.nikse.dk/SubtitleEdit/Help
Name: {group}\Help and Support\{cm:ProgramOnTheWeb,Subtitle Edit}; Filename: https://www.nikse.dk/SubtitleEdit/; Comment: {cm:ProgramOnTheWeb,Subtitle Edit}
Name: {group}\{cm:UninstallProgram,Subtitle Edit};                 Filename: {uninstallexe};                     Comment: {cm:UninstallProgram,Subtitle Edit}; WorkingDir: {app}; IconFilename: {app}\Icons\uninstall.ico

Name: {commondesktop}\Subtitle Edit;        Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit {#app_ver}; AppUserModelID: Nikse.SubtitleEdit; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: desktopicon\common
Name: {userdesktop}\Subtitle Edit;          Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit {#app_ver}; AppUserModelID: Nikse.SubtitleEdit; IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: desktopicon\user
Name: {#quick_launch}\Subtitle Edit;        Filename: {app}\SubtitleEdit.exe; WorkingDir: {app}; Comment: Subtitle Edit {#app_ver};                                     IconFilename: {app}\SubtitleEdit.exe; IconIndex: 0; Tasks: quicklaunchicon


[InstallDelete]
Type: files;      Name: {userdesktop}\Subtitle Edit.lnk;   Check: not IsTaskSelected('desktopicon\user')   and IsUpgrade()
Type: files;      Name: {commondesktop}\Subtitle Edit.lnk; Check: not IsTaskSelected('desktopicon\common') and IsUpgrade()
Type: files;      Name: {#quick_launch}\Subtitle Edit.lnk; Check: not IsTaskSelected('quicklaunchicon')    and IsUpgrade(); OnlyBelowVersion: 6.01
Type: files;      Name: {userappdata}\Subtitle Edit\Settings.xml; Tasks: reset_settings
Type: files;      Name: {app}\libse.dll;                              Check: IsUpgrade()
Type: files;      Name: {app}\zlib.net.dll;                           Check: IsUpgrade()
Type: files;      Name: {app}\NHunspell.dll;                          Check: IsUpgrade()
Type: files;      Name: {app}\UtfUnknown.dll;                         Check: IsUpgrade()
Type: files;      Name: {app}\Vosk.dll;                               Check: IsUpgrade()
Type: files;      Name: {app}\NCalc.dll;                              Check: IsUpgrade()
Type: files;      Name: {app}\Interop.QuartzTypeLib.dll;              Check: IsUpgrade()
Type: files;      Name: {app}\Newtonsoft.Json.dll;                    Check: IsUpgrade()
Type: files;      Name: {app}\System.Net.Http.Extensions.dll;         Check: IsUpgrade()
Type: files;      Name: {app}\System.Net.Http.Primitives.dll;         Check: IsUpgrade()
Type: files;      Name: {app}\NAudio.Core.dll;                        Check: IsUpgrade()
Type: files;      Name: {app}\NAudio.WinMM.dll;                       Check: IsUpgrade()
Type: files;      Name: {app}\Microsoft.Win32.Registry.dll;           Check: IsUpgrade()


; Remove old files from the {app} dir
Type: files;      Name: {app}\Dictionaries\da_names.xml;               Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\da_DK_se.xml;               Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\dan_OCRFixReplaceList.xml;  Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\en_US.aff;                  Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\en_US.dic;                  Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\en_names.xml;               Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\en_US_se.xml;               Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\en_US_se.xml;               Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\eng_OCRFixReplaceList.xml;  Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\names.xml;                  Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\no_names.xml;               Check: IsUpgrade()

Type: files;      Name: {app}\Dictionaries\dan_WordSplitList.txt;      Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\eng_WordSplitList.txt;      Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\fra_WordSplitList.txt;      Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\ita_WordSplitList.txt;      Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\pol_WordSplitList.txt;      Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\spa_WordSplitList.txt;      Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\deu_Nouns.txt;              Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\da_interjections_se.xml;    Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\en_interjections_se.xml;    Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\es_interjections_se.xml;    Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\fr_interjections_se.xml;    Check: IsUpgrade()

Type: dirifempty; Name: {app}\Dictionaries;                            Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.DangAmbigs;                 Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.freq-dawg;                  Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.inttemp;                    Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.normproto;                  Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.pffmtable;                  Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.unicharset;                 Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.user-words;                 Check: IsUpgrade()
Type: files;      Name: {app}\TessData\eng.word-dawg;                  Check: IsUpgrade()
Type: dirifempty; Name: {app}\TessData;                                Check: IsUpgrade()
Type: files;      Name: {app}\Tesseract\leptonlib.dll;                 Check: IsUpgrade()
Type: files;      Name: {app}\tessnet2_32.dll;                         Check: IsUpgrade()
Type: files;      Name: {app}\Tesseract302\tessdata\configs\hocr;      Check: IsUpgrade()
Type: files;      Name: {app}\Tesseract302\tessdata\eng.traineddata;   Check: IsUpgrade()
Type: files;      Name: {app}\Tesseract302\tessdata\music.traineddata; Check: IsUpgrade()
Type: files;      Name: {app}\Tesseract302\tesseract.exe;              Check: IsUpgrade()
Type: files;      Name: {app}\Tesseract302\msvcp90.dll;                Check: IsUpgrade()
Type: files;      Name: {app}\Tesseract302\msvcr90.dll;                Check: IsUpgrade()
Type: files;      Name: {app}\Icons\SubtitleEdit.srt.ico;              Check: IsUpgrade()
Type: files;      Name: {app}\DocumentIcons.dll;                       Check: IsUpgrade()
Type: files;      Name: {app}\Settings.xml;                            Check: IsUpgrade()
Type: files;      Name: {app}\gpl.txt;                                 Check: IsUpgrade()
Type: files;      Name: {app}\uninstall.ico;                           Check: IsUpgrade()

#ifdef localize
; Language files not included anymore
Type: files;      Name: {app}\Languages\sr-Cyrl-CS.xml
Type: files;      Name: {app}\Languages\sr-Latn-CS.xml
Type: files;      Name: {app}\Languages\zh-CHS.xml

; Remove the language files if it's an upgrade and the translations are not selected
Type: files;      Name: {app}\Languages\ar-EG.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\bg-BG.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\br-FR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\ca-ES.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\cs-CZ.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\da-DK.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\de-DE.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\el-GR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\es-AR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\es-ES.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\es-MX.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\eu-ES.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\fa-IR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\fi-FI.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\fr-FR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\hr-HR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\hu-HU.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\id-ID.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\it-IT.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\ja-JP.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\ko-KR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\mk-MK.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\nb-NO.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\nl-NL.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\pl-PL.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\pt-BR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\pt-PT.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\ro-RO.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\ru-RU.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\sl-SI.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\sr-Cyrl-RS.xml; Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\sr-Latn-RS.xml; Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\sv-SE.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\th-TH.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\tr-TR.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\uk-UA.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\vi-VN.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\zh-Hans.xml;    Check: not IsComponentSelected('translations') and IsUpgrade()
Type: files;      Name: {app}\Languages\zh-TW.xml;      Check: not IsComponentSelected('translations') and IsUpgrade()
Type: dirifempty; Name: {app}\Languages;                Check: not IsComponentSelected('translations') and IsUpgrade()
#endif


[Run]
Filename: {win}\Microsoft.NET\Framework\v4.0.30319\ngen.exe;   Parameters: "install ""{app}\SubtitleEdit.exe"""; StatusMsg: {cm:msg_OptimizingPerformance}; Flags: runhidden runascurrentuser skipifdoesntexist; Check: not IsWin64
Filename: {win}\Microsoft.NET\Framework64\v4.0.30319\ngen.exe; Parameters: "install ""{app}\SubtitleEdit.exe"""; StatusMsg: {cm:msg_OptimizingPerformance}; Flags: runhidden runascurrentuser skipifdoesntexist; Check: IsWin64
Filename: {app}\SubtitleEdit.exe;             Description: {cm:LaunchProgram,Subtitle Edit}; WorkingDir: {app}; Flags: nowait postinstall skipifsilent unchecked
Filename: https://www.nikse.dk/SubtitleEdit/; Description: {cm:run_VisitWebsite};                               Flags: nowait postinstall skipifsilent unchecked shellexec


[UninstallRun]
Filename: {win}\Microsoft.NET\Framework\v4.0.30319\ngen.exe;   Parameters: "uninstall ""{app}\SubtitleEdit.exe"""; Flags: runhidden runascurrentuser skipifdoesntexist; Check: not IsWin64
Filename: {win}\Microsoft.NET\Framework64\v4.0.30319\ngen.exe; Parameters: "uninstall ""{app}\SubtitleEdit.exe"""; Flags: runhidden runascurrentuser skipifdoesntexist; Check: IsWin64


[Registry]
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.ass"; ValueData: "Advanced Sub Station Alpha subtitle file";  Flags: uninsdeletekey; ValueType: string; ValueName: ""; Check: DoSystemAssoc('ass')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.ass\shell\open\command"; ValueData: """{app}\SubtitleEdit.exe"" ""%1""";  ValueType: string; ValueName: ""; Check: DoSystemAssoc('ass')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.ass\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\Icons\ass.ico"; Check: DoSystemAssoc('ass')
Root: HKCU ; Subkey: "Software\Classes\.ass"; ValueData: "SubtitleEdit.ass"; Flags: uninsdeletevalue; ValueType: string; Check: DoSystemAssoc('ass')

Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.dfxp"; ValueData: "Distribution Format Exchange Profile subtitle file";  Flags: uninsdeletekey; ValueType: string; ValueName: ""; Check: DoSystemAssoc('dfxp')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.dfxp\shell\open\command"; ValueData: """{app}\SubtitleEdit.exe"" ""%1""";  ValueType: string; ValueName: ""; Check: DoSystemAssoc('dfxp')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.dfxp\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\Icons\dfxp.ico"; Check: DoSystemAssoc('dfxp')
Root: HKCU ; Subkey: "Software\Classes\.dfxp"; ValueData: "SubtitleEdit.dfxp"; Flags: uninsdeletevalue; ValueType: string; Check: DoSystemAssoc('dfxp')

Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.sbv"; ValueData: "SBV subtitle file";  Flags: uninsdeletekey; ValueType: string; ValueName: ""; Check: DoSystemAssoc('sbv')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.sbv\shell\open\command"; ValueData: """{app}\SubtitleEdit.exe"" ""%1""";  ValueType: string; ValueName: ""; Check: DoSystemAssoc('sbv')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.sbv\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\Icons\sbv.ico"; Check: DoSystemAssoc('sbv')
Root: HKCU ; Subkey: "Software\Classes\.sbv"; ValueData: "SubtitleEdit.sbv"; Flags: uninsdeletevalue; ValueType: string; Check: DoSystemAssoc('sbv')

Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.srt"; ValueData: "SubRip subtitle file";  Flags: uninsdeletekey; ValueType: string; ValueName: ""; Check: DoSystemAssoc('srt')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.srt\shell\open\command"; ValueData: """{app}\SubtitleEdit.exe"" ""%1""";  ValueType: string; ValueName: ""; Check: DoSystemAssoc('srt')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.srt\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\Icons\srt.ico"; Check: DoSystemAssoc('srt')
Root: HKCU ; Subkey: "Software\Classes\.srt"; ValueData: "SubtitleEdit.srt"; Flags: uninsdeletevalue; ValueType: string; Check: DoSystemAssoc('srt')

Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.ssa"; ValueData: "Sub Station Alpha subtitle file";  Flags: uninsdeletekey; ValueType: string; ValueName: ""; Check: DoSystemAssoc('ssa')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.ssa\shell\open\command"; ValueData: """{app}\SubtitleEdit.exe"" ""%1""";  ValueType: string; ValueName: ""; Check: DoSystemAssoc('ssa')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.ssa\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\Icons\ssa.ico"; Check: DoSystemAssoc('ssa')
Root: HKCU ; Subkey: "Software\Classes\.ssa"; ValueData: "SubtitleEdit.ssa"; Flags: uninsdeletevalue; ValueType: string; Check: DoSystemAssoc('ssa')

Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.stl"; ValueData: "STL subtitle file";  Flags: uninsdeletekey; ValueType: string; ValueName: ""; Check: DoSystemAssoc('stl')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.stl\shell\open\command"; ValueData: """{app}\SubtitleEdit.exe"" ""%1""";  ValueType: string; ValueName: ""; Check: DoSystemAssoc('stl')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.stl\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\Icons\stl.ico"; Check: DoSystemAssoc('stl')
Root: HKCU ; Subkey: "Software\Classes\.stl"; ValueData: "SubtitleEdit.stl"; Flags: uninsdeletevalue; ValueType: string; Check: DoSystemAssoc('stl')

Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.sub"; ValueData: "SUB subtitle file";  Flags: uninsdeletekey; ValueType: string; ValueName: ""; Check: DoSystemAssoc('sub')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.sub\shell\open\command"; ValueData: """{app}\SubtitleEdit.exe"" ""%1""";  ValueType: string; ValueName: ""; Check: DoSystemAssoc('sub')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.sub\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\Icons\sub.ico"; Check: DoSystemAssoc('sub')
Root: HKCU ; Subkey: "Software\Classes\.sub"; ValueData: "SubtitleEdit.sub"; Flags: uninsdeletevalue; ValueType: string; Check: DoSystemAssoc('sub')

Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.sup"; ValueData: "Blu-ray PGS subtitle file";  Flags: uninsdeletekey; ValueType: string; ValueName: ""; Check: DoSystemAssoc('sup')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.sup\shell\open\command"; ValueData: """{app}\SubtitleEdit.exe"" ""%1""";  ValueType: string; ValueName: ""; Check: DoSystemAssoc('sup')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.sup\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\Icons\sup.ico"; Check: DoSystemAssoc('sup')
Root: HKCU ; Subkey: "Software\Classes\.sup"; ValueData: "SubtitleEdit.sup"; Flags: uninsdeletevalue; ValueType: string; Check: DoSystemAssoc('sup')

Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.vtt"; ValueData: "Web Video Text Tracks (WebVTT) subtitle file";  Flags: uninsdeletekey; ValueType: string; ValueName: ""; Check: DoSystemAssoc('vtt')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.vtt\shell\open\command"; ValueData: """{app}\SubtitleEdit.exe"" ""%1""";  ValueType: string; ValueName: ""; Check: DoSystemAssoc('vtt')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.vtt\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\Icons\vtt.ico"; Check: DoSystemAssoc('vtt')
Root: HKCU ; Subkey: "Software\Classes\.vtt"; ValueData: "SubtitleEdit.vtt"; Flags: uninsdeletevalue; ValueType: string; Check: DoSystemAssoc('vtt')

Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.smi"; ValueData: "SAMI subtitle file";  Flags: uninsdeletekey; ValueType: string; ValueName: ""; Check: DoSystemAssoc('smi')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.smi\shell\open\command"; ValueData: """{app}\SubtitleEdit.exe"" ""%1""";  ValueType: string; ValueName: ""; Check: DoSystemAssoc('smi')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.smi\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\Icons\smi.ico"; Check: DoSystemAssoc('smi')
Root: HKCU ; Subkey: "Software\Classes\.smi"; ValueData: "SubtitleEdit.smi"; Flags: uninsdeletevalue; ValueType: string; Check: DoSystemAssoc('smi')

Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.itt"; ValueData: "iTunes Timed Text subtitle file";  Flags: uninsdeletekey; ValueType: string; ValueName: ""; Check: DoSystemAssoc('itt')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.itt\shell\open\command"; ValueData: """{app}\SubtitleEdit.exe"" ""%1""";  ValueType: string; ValueName: ""; Check: DoSystemAssoc('itt')
Root: HKCU ; Subkey: "Software\Classes\SubtitleEdit.itt\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\Icons\itt.ico"; Check: DoSystemAssoc('itt')
Root: HKCU ; Subkey: "Software\Classes\.itt"; ValueData: "SubtitleEdit.itt"; Flags: uninsdeletevalue; ValueType: string; Check: DoSystemAssoc('itt')

; Add .ass (Advanced SubStation Alpha) to the SE-supported file types
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".ass"; ValueData: ""; Check: HklmKeyExists('{#keyApps}')

; Add .dfxp to the SE-supported file types
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".dfxp"; ValueData: ""; Check: HklmKeyExists('{#keyApps}')

; Add .sbv to the SE-supported file types
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".sbv"; ValueData: ""; Check: HklmKeyExists('{#keyApps}')

; Add .srt (SubRip) to the SE-supported file types
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".srt"; ValueData: ""; Check: HklmKeyExists('{#keyApps}')

; Add .ssa (SubStation Alpha) to the SE-supported file types
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".ssa"; ValueData: ""; Check: HklmKeyExists('{#keyApps}')

; Add .sub to the SE-supported file types
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".sub"; ValueData: ""; Check: HklmKeyExists('{#keyApps}')

; Add .sup (Blu-ray sup) to the SE-supported file types
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".sup"; ValueData: ""; Check: HklmKeyExists('{#keyApps}')

; Add .vtt (Web VTT) to the SE-supported file types
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".vtt"; ValueData: ""; Check: HklmKeyExists('{#keyApps}')


; Add video files to the SE-supported file types
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".m2ts"; ValueData: ""; Check: HklmKeyExists('{#keyApps}')
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".mp4";  ValueData: ""; Check: HklmKeyExists('{#keyApps}')
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".mkv";  ValueData: ""; Check: HklmKeyExists('{#keyApps}')
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".mks";  ValueData: ""; Check: HklmKeyExists('{#keyApps}')
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".avi";  ValueData: ""; Check: HklmKeyExists('{#keyApps}')
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".ts";   ValueData: ""; Check: HklmKeyExists('{#keyApps}')


[Code]
// Check if subkey exists in HKLM registry hive
function HklmKeyExists(const KeyName: String): Boolean;
begin
  Result := RegKeyExists(HKEY_LOCAL_MACHINE, KeyName);
end;


// Check if Subtitle Edit's settings exist
function SettingsExist(): Boolean;
begin
  Result := FileExists(ExpandConstant('{userappdata}\Subtitle Edit\Settings.xml'));
end;


// Check if Dictionaries exist
function DictionariesExist(): Boolean;
var
  FindRec: TFindRec;
begin
  if FindFirst(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.aff'), FindRec) or
     FindFirst(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.dic'), FindRec) or
     FindFirst(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.xml'), FindRec) then
  begin
    Result := True;
    FindClose(FindRec);
  end else
    Result := False;
end;


// Reset HKLM file type association if it is currently ours, and check if it should be renewed
function DoSystemAssoc(const FileType: String): Boolean;
var
  CurrentProgId, MyProgId, KeyName: String;
begin
  KeyName := '{#keyCl}\.' + FileType;
  if RegQueryStringValue(HKEY_CURRENT_USER, KeyName, '', CurrentProgId) then
  begin
    MyProgId := 'SubtitleEdit.' + FileType;
    if CompareText(CurrentProgId, MyProgId) = 0 then
      RegWriteStringValue(HKEY_CURRENT_USER, KeyName, '', '');
  end;
  Result := IsTaskSelected('associate_common');
end;


// Remove cached indirect strings from MUI cache
procedure ClearMUICacheKey(const KeyName: String);
var
  Names: TArrayOfString;
  Index: Integer;
begin
  if RegGetSubkeyNames(HKEY_USERS, KeyName, Names) then
  begin
    for Index := Low(Names) to High(Names) do
      ClearMUICacheKey(KeyName + '\' + Names[Index]);
  end;
  if RegGetValueNames(HKEY_USERS, KeyName, Names) then
  begin
    for Index := Low(Names) to High(Names) do
      if Pos('\SubtitleEdit.resources.dll,', Names[Index]) <> 0 then
        RegDeleteValue(HKEY_USERS, KeyName, Names[Index]);
  end;
end;

procedure ClearMUICache();
var
  Users: TArrayOfString;
  Index: Integer;
  Key: String;
begin
  if RegGetSubkeyNames(HKEY_USERS, '', Users) then
  begin
    for Index := Low(Users) to High(Users) do
    begin
      Key := Users[Index] + '\{#keyMuiCache}';
      if RegKeyExists(HKEY_USERS, Key) then
        ClearMUICacheKey(Key);
    end;
  end;
end;


procedure CleanUpDictionaries();
begin
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\dan_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\deu_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\eng_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\fin_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\fra_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\hrb_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\hrv_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\hun_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\mkd_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\nld_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\nob_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\nor_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\pol_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\por_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\rus_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\spa_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\srp_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\swe_OCRFixReplaceList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\da_names.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\de_names.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\en_names.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\es_names.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\fi_names.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\fr_names.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\hr_names.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\nb_names.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\nl_names.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\no_names.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\pt_names.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\ru_names.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\names.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\ar_NoBreakAfterList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\bg_NoBreakAfterList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\da_NoBreakAfterList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\el_NoBreakAfterList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\en_NoBreakAfterList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\es_NoBreakAfterList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\hr_NoBreakAfterList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\mk_NoBreakAfterList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\pt_NoBreakAfterList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\ru_NoBreakAfterList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\sr_NoBreakAfterList.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\da_DK_se.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\de_DE_se.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\en_US_se.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\en_US_se.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\es_MX_se.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\fi_FI_se.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\nl_NL_se.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\pt_PT_se.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\ru_RU_se.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\it_IT_se.xml'));
  DelTree(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\???_OCRFixReplaceList_User.xml'), False, True, False);
  DelTree(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.dic'), False, True, False);
  DelTree(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.aff'), False, True, False);
  RemoveDir(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Ocr\Latin.db'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Ocr\Latin.nocr'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\preview.mkv'));
  DeleteFile(ExpandConstant('{app}\Icons\ass.ico'));
  DeleteFile(ExpandConstant('{app}\Icons\dfxp.ico'));
  DeleteFile(ExpandConstant('{app}\Icons\lrc.ico'));
  DeleteFile(ExpandConstant('{app}\Icons\sbv.ico'));
  DeleteFile(ExpandConstant('{app}\Icons\srt.ico'));
  DeleteFile(ExpandConstant('{app}\Icons\ssa.ico'));
  DeleteFile(ExpandConstant('{app}\Icons\stl.ico'));
  DeleteFile(ExpandConstant('{app}\Icons\sub.ico'));
  DeleteFile(ExpandConstant('{app}\Icons\sup.ico'));
  DeleteFile(ExpandConstant('{app}\Icons\vtt.ico'));
  DeleteFile(ExpandConstant('{app}\Icons\smi.ico'));
  DeleteFile(ExpandConstant('{app}\Icons\itt.ico'));
  DelTree(ExpandConstant('{app}\Icons'), True, True, True);
  RemoveDir(ExpandConstant('{app}\Icons'));
  DelTree(ExpandConstant('{userappdata}\Subtitle Edit\Ocr\*.*'), False, True, False);
  RemoveDir(ExpandConstant('{userappdata}\Subtitle Edit\Ocr'));
  DelTree(ExpandConstant('{userappdata}\Subtitle Edit\Plugins\*.*'), False, True, False);
  RemoveDir(ExpandConstant('{userappdata}\Subtitle Edit\Plugins'));
  DelTree(ExpandConstant('{userappdata}\Subtitle Edit\Icons\*.*'), False, True, False);
  RemoveDir(ExpandConstant('{userappdata}\Subtitle Edit\Icons'));
end;


function IsUpgrade(): Boolean;
var
  PrevPath: String;
begin
  PrevPath := WizardForm.PrevAppDir;
  Result := (PrevPath <> '');
end;


function ShouldSkipPage(PageID: Integer): Boolean;
begin
  // Hide the license page
  Result := (IsUpgrade() and (PageID = wpLicense))
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
  if CurUninstallStep = usUninstall then
  begin
    if SettingsExist() or DictionariesExist() then
    begin
      if SuppressibleMsgBox(CustomMessage('msg_DeleteSettings'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2, IDNO) = IDYES then
      begin
        CleanUpDictionaries();
        DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Settings.xml'));
      end;

      DelTree(ExpandConstant('{userappdata}\Subtitle Edit'), True, True, True);
    end;
  end;
end;


function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1'          .NET Framework 1.1
//    'v2.0'          .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//    'v4.5'          .NET Framework 4.5
//    'v4.5.1'        .NET Framework 4.5.1
//    'v4.5.2'        .NET Framework 4.5.2
//    'v4.6'          .NET Framework 4.6
//    'v4.6.1'        .NET Framework 4.6.1
//    'v4.6.2'        .NET Framework 4.6.2
//    'v4.7'          .NET Framework 4.7
//    'v4.7.1'        .NET Framework 4.7.1
//    'v4.7.2'        .NET Framework 4.7.2
//    'v4.8'          .NET Framework 4.8
//    'v4.8.1'        .NET Framework 4.8.1
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key, versionKey: string;
    install, release, serviceCount, versionRelease: cardinal;
    success: boolean;
begin
    versionKey := version;
    versionRelease := 0;

    // .NET 1.1 and 2.0 embed release number in version key
    if version = 'v1.1' then begin
        versionKey := 'v1.1.4322';
    end else if version = 'v2.0' then begin
        versionKey := 'v2.0.50727';
    end

    // .NET 4.5 and newer install as update to .NET 4.0 Full
    else if Pos('v4.', version) = 1 then begin
        versionKey := 'v4\Full';
        case version of
          'v4.5':   versionRelease := 378389;
          'v4.5.1': versionRelease := 378675; // 378758 on Windows 8 and older
          'v4.5.2': versionRelease := 379893;
          'v4.6':   versionRelease := 393295; // 393297 on Windows 8.1 and older
          'v4.6.1': versionRelease := 394254; // 394271 before Win10 November Update
          'v4.6.2': versionRelease := 394802; // 394806 before Win10 Anniversary Update
          'v4.7':   versionRelease := 460798; // 460805 before Win10 Creators Update
          'v4.7.1': versionRelease := 461308; // 461310 before Win10 Fall Creators Update
          'v4.7.2': versionRelease := 461808; // 461814 before Win10 April 2018 Update
          'v4.8':   versionRelease := 528040; // 528049 before Win10 May 2019 Update
          '4.8.1':  versionRelease := 533325; 
        end;
    end;

    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + versionKey;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0 and newer use value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 and newer use additional value Release
    if versionRelease > 0 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= versionRelease);
    end;

    result := success and (install = 1) and (serviceCount >= service);
end;


function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
begin
  // Returns True if .NET Framework version 4.8 is installed, or a compatible version such as 4.8.1
  Result := IsDotNetDetected('v4.8', 0);
  if not Result then
  begin
    if not WizardSilent() then
    begin
      if SuppressibleMsgBox(CustomMessage('msg_AskToDownNET'), mbCriticalError, MB_YESNO or MB_DEFBUTTON1, IDNO) = IDYES then
        ShellExec('open','https://go.microsoft.com/fwlink/?LinkId=2085155','','',SW_SHOWNORMAL,ewNoWait,ErrorCode);
        Result := False;
      end;
    end;
  end;
end.
