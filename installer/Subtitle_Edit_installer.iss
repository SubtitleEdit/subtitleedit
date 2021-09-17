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

; Requirements:
; Inno Setup Unicode: https://jrsoftware.org/isdl.php


; preprocessor checks
#if VER < EncodeVer(6,0,0)
  #error Update your Inno Setup version (6.0.0 or newer)
#endif

#ifndef UNICODE
  #error Use Inno Setup unicode
#endif


#define app_copyright "Copyright © 2001-2021, Nikse"
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
#define bindirres "..\src\Win32Resources\bin\Release"

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
AppCopyright={#app_copyright}
AppContact=https://www.nikse.dk/SubtitleEdit/Help
AppName=Subtitle Edit
AppVerName=Subtitle Edit {#app_ver}
AppVersion={#app_ver_full}
AppPublisher=Nikse
AppPublisherURL=https://www.nikse.dk/SubtitleEdit/
AppSupportURL=https://www.nikse.dk/SubtitleEdit/
AppUpdatesURL=https://www.nikse.dk/SubtitleEdit/
UninstallDisplayName=Subtitle Edit {#app_ver}
UninstallDisplayIcon={app}\SubtitleEdit.exe
DefaultDirName={pf}\Subtitle Edit
DefaultGroupName=Subtitle Edit
VersionInfoVersion={#app_ver_full}
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
ShowLanguageDialog=yes
DisableDirPage=auto
DisableProgramGroupPage=auto
CloseApplications=true
SetupMutex='subtitle_edit_setup_mutex'
ArchitecturesInstallIn64BitMode=x64
WizardStyle=modern

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
#ifdef localize
Name: "ar"; MessagesFile: "Languages\Arabic.isl"
Name: "bg"; MessagesFile: "Languages\Bulgarian.isl"
Name: "ca"; MessagesFile: "compiler:Languages\Catalan.isl"
Name: "cs"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "da"; MessagesFile: "compiler:Languages\Danish.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "eu"; MessagesFile: "Languages\Basque.isl"
Name: "fa"; MessagesFile: "Languages\Farsi.isl"
Name: "fi"; MessagesFile: "compiler:Languages\Finnish.isl"
Name: "fr"; MessagesFile: "compiler:Languages\French.isl"
Name: "hr"; MessagesFile: "Languages\Croatian.isl"
Name: "it"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "ja"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "ko"; MessagesFile: "Languages\Korean.isl"
Name: "mk"; MessagesFile: "Languages\Macedonian.isl"
Name: "nl"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "no"; MessagesFile: "compiler:Languages\Norwegian.isl"
Name: "pl"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "pt"; MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "ptBR"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "ro"; MessagesFile: "Languages\Romanian.isl"
Name: "ru"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "sl"; MessagesFile: "compiler:Languages\Slovenian.isl"
Name: "sv"; MessagesFile: "Languages\Swedish.isl"
Name: "th"; MessagesFile: "Languages\Thai.isl"
Name: "tr"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "uk"; MessagesFile: "compiler:Languages\Ukrainian.isl"
Name: "vi"; MessagesFile: "Languages\Vietnamese.islu"
Name: "zh"; MessagesFile: "Languages\ChineseSimplified.islu"
Name: "zhTW"; MessagesFile: "Languages\ChineseTraditional.isl"
#endif

; Include the installer's custom messages
#include "Custom_Messages.iss"

[Messages]
;BeveledLabel=Subtitle Edit {#app_ver} by Nikse
SetupAppTitle=Setup - Subtitle Edit
SetupWindowTitle=Setup - Subtitle Edit {#app_ver}


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
Name: associate_srt;      Description: {cm:tsk_SetFileTypes};      GroupDescription: {cm:tsk_Other};       Flags: unchecked


[Files]
Source: ..\Dictionaries\dan_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\deu_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\eng_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\fin_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\fra_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\hrb_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\hrv_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\hun_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\mkd_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\nld_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\nob_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\nor_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\pol_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\por_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\rus_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\spa_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\srp_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\swe_OCRFixReplaceList.xml; DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\da_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\de_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\en_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\es_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\fi_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\fr_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\hr_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\nb_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\nl_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\pt_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\ru_names.xml;              DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\names.xml;                 DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall; Components: main
Source: ..\Dictionaries\ar_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\bg_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\da_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\el_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\en_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\es_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\hr_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\mk_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\pt_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\ru_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\sr_NoBreakAfterList.xml;   DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\da_DK_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\de_DE_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\en_US_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\es_MX_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\fi_FI_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\nl_NL_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\pt_PT_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\ru_RU_user.xml;            DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\en_US.aff;                 DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Dictionaries\en_US.dic;                 DestDir: {userappdata}\Subtitle Edit\Dictionaries; Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main

Source: ..\Ocr\Latin.db;                           DestDir: {userappdata}\Subtitle Edit\Ocr;          Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main
Source: ..\Ocr\Latin.nocr;                         DestDir: {userappdata}\Subtitle Edit\Ocr;          Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main

Source: ..\preview.mkv;                            DestDir: {userappdata}\Subtitle Edit;              Flags: ignoreversion uninsneveruninstall onlyifdoesntexist; Components: main


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
Source: {#bindirres}\SubtitleEdit.resources.dll;   DestDir: {app};                                    Flags: ignoreversion; Components: main; AfterInstall: ClearMUICache
Source: {#bindir}\Hunspellx64.dll;                 DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\Hunspellx86.dll;                 DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\libse.dll;                       DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\zlib.net.dll;                    DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\NHunspell.dll;                   DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\UtfUnknown.dll;                  DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: ..\src\ui\DLLs\Interop.QuartzTypeLib.dll;  DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\Newtonsoft.Json.dll;             DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\System.Net.Http.Extensions.dll;  DestDir: {app};                                    Flags: ignoreversion; Components: main
Source: {#bindir}\System.Net.Http.Primitives.dll;  DestDir: {app};                                    Flags: ignoreversion; Components: main
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
Type: files;      Name: {app}\Interop.QuartzTypeLib.dll;              Check: IsUpgrade()
Type: files;      Name: {app}\Newtonsoft.Json.dll;                    Check: IsUpgrade()
Type: files;      Name: {app}\System.Net.Http.Extensions.dll;         Check: IsUpgrade()
Type: files;      Name: {app}\System.Net.Http.Primitives.dll;         Check: IsUpgrade()

; Remove old files from the {app} dir
Type: files;      Name: {app}\Dictionaries\da_names.xml;               Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\da_DK_user.xml;             Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\dan_OCRFixReplaceList.xml;  Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\en_US.aff;                  Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\en_US.dic;                  Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\en_names.xml;               Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\en_US_user.xml;             Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\eng_OCRFixReplaceList.xml;  Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\names.xml;                  Check: IsUpgrade()
Type: files;      Name: {app}\Dictionaries\no_names.xml;               Check: IsUpgrade()
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
#include bindirres + "\Resources.h"
#define rcicon(id) "{app}\SubtitleEdit.resources.dll,-" + Str(id)
#define rctext(id) "@{app}\SubtitleEdit.resources.dll,-" + Str(id)

Root: HKLM; Subkey: "{#keyAppPaths}\SubtitleEdit.exe"; ValueType: string; ValueName: ""; ValueData: "{app}\SubtitleEdit.exe"; Flags: deletekey uninsdeletekey; Check: HklmKeyExists('{#keyAppPaths}')
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe"; ValueType: string; ValueName: ""; ValueData: "{#SetupSetting('AppName')} {#app_ver_full}"; Flags: deletekey uninsdeletekey; Check: HklmKeyExists('{#keyApps}')
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\SubtitleEdit.exe"" ""%1"""; Check: HklmKeyExists('{#keyApps}')
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\shell\open"; ValueType: string; ValueName: "FriendlyAppName"; ValueData: "{#rctext(RC_APPNAME)}"; Check: HklmKeyExists('{#keyApps}')
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe"; ValueType: string; ValueName: "FriendlyAppName"; ValueData: "{#rctext(RC_APPNAME)}"; Check: HklmKeyExists('{#keyApps}')
Root: HKLM; Subkey: "{#keySE}"; ValueType: string; ValueName: ""; ValueData: "{#SetupSetting('AppName')}"; Flags: deletekey uninsdeletekey
Root: HKLM; Subkey: "{#keySE}\Capabilities"; ValueType: string; ValueName: "ApplicationDescription"; ValueData: "{#rctext(RC_APPDESC)}"
Root: HKLM; Subkey: "{#keySE}\Capabilities"; ValueType: string; ValueName: "ApplicationName"; ValueData: "{#rctext(RC_APPNAME)}"
Root: HKLM; Subkey: "{#keyRegApps}"; ValueType: string; ValueName: "SubtitleEdit"; ValueData: "{#keySE}\Capabilities"; Flags: uninsdeletevalue; Check: HklmKeyExists('{#keyRegApps}')
; Add .srt to the SE-supported file types
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.srt"; ValueType: string; ValueName: ""; ValueData: "{#RCDESC_SRT_DEFAULT}"; Flags: deletekey uninsdeletekey
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.srt"; ValueType: string; ValueName: "FriendlyTypeName"; ValueData: "{#rctext(RCDESC_SRT)}"
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.srt"; ValueType: dword; ValueName: "EditFlags"; ValueData: $00010000
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.srt\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{#rcicon(RCICON_SRT)}"
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.srt\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\SubtitleEdit.exe"" ""%1"""
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.srt\shell"; ValueType: string; ValueName: ""; ValueData: "open"
Root: HKLM; Subkey: "{#keyCl}\.srt\OpenWithProgids"; ValueType: none; ValueName: "SubtitleEdit.srt"; Flags: dontcreatekey deletevalue
Root: HKLM; Subkey: "{#keyCl}\.srt\OpenWithProgids"; ValueType: string; ValueName: "SubtitleEdit.srt"; ValueData: ""; Flags: uninsdeletevalue; OnlyBelowVersion: 6.0
Root: HKLM; Subkey: "{#keyCl}\.srt"; ValueType: string; ValueName: "Content Type"; ValueData: "application/x-subrip"
Root: HKLM; Subkey: "{#keyCl}\.srt"; ValueType: string; ValueName: "PerceivedType"; ValueData: "text"
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".srt"; ValueData: ""; Check: HklmKeyExists('{#keyApps}')
Root: HKLM; Subkey: "{#keySE}\Capabilities\FileAssociations"; ValueType: string; ValueName: ".srt"; ValueData: "SubtitleEdit.srt"
; Associate .srt file type with SE (only if requested by user)
Root: HKLM; Subkey: "{#keyCl}\.srt"; ValueType: string; ValueName: ""; ValueData: "SubtitleEdit.srt"; Check: DoSystemAssoc('srt')
; Add .sup to the SE-supported file types
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.sup"; ValueType: string; ValueName: ""; ValueData: "{#RCDESC_SUP_DEFAULT}"; Flags: deletekey uninsdeletekey
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.sup"; ValueType: string; ValueName: "FriendlyTypeName"; ValueData: "{#rctext(RCDESC_SUP)}"
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.sup"; ValueType: dword; ValueName: "EditFlags"; ValueData: $00010000
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.sup\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{#rcicon(RCICON_SUP)}"
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.sup\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\SubtitleEdit.exe"" ""%1"""
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.sup\shell"; ValueType: string; ValueName: ""; ValueData: "open"
Root: HKLM; Subkey: "{#keyCl}\.sup\OpenWithProgids"; ValueType: none; ValueName: "SubtitleEdit.sup"; Flags: dontcreatekey deletevalue
Root: HKLM; Subkey: "{#keyCl}\.sup\OpenWithProgids"; ValueType: string; ValueName: "SubtitleEdit.sup"; ValueData: ""; Flags: uninsdeletevalue; OnlyBelowVersion: 6.0
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".sup"; ValueData: ""; Check: HklmKeyExists('{#keyApps}')
Root: HKLM; Subkey: "{#keySE}\Capabilities\FileAssociations"; ValueType: string; ValueName: ".sup"; ValueData: "SubtitleEdit.sup"
; Add .sub to the SE-supported file types
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.sub"; ValueType: string; ValueName: ""; ValueData: "{#RCDESC_SUB_DEFAULT}"; Flags: deletekey uninsdeletekey
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.sub"; ValueType: string; ValueName: "FriendlyTypeName"; ValueData: "{#rctext(RCDESC_SUB)}"
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.sub"; ValueType: dword; ValueName: "EditFlags"; ValueData: $00010000
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.sub\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{#rcicon(RCICON_SUB)}"
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.sub\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\SubtitleEdit.exe"" ""%1"""
Root: HKLM; Subkey: "{#keyCl}\SubtitleEdit.sub\shell"; ValueType: string; ValueName: ""; ValueData: "open"
Root: HKLM; Subkey: "{#keyCl}\.sub\OpenWithProgids"; ValueType: none; ValueName: "SubtitleEdit.sub"; Flags: dontcreatekey deletevalue
Root: HKLM; Subkey: "{#keyCl}\.sub\OpenWithProgids"; ValueType: string; ValueName: "SubtitleEdit.sub"; ValueData: ""; Flags: uninsdeletevalue; OnlyBelowVersion: 6.0
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".sub"; ValueData: ""; Check: HklmKeyExists('{#keyApps}')
Root: HKLM; Subkey: "{#keySE}\Capabilities\FileAssociations"; ValueType: string; ValueName: ".sub"; ValueData: "SubtitleEdit.sub"
; Add .ssa (SubStation Alpha) to the SE-supported file types
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".ssa"; ValueData: ""; Check: HklmKeyExists('{#keyApps}')
; Add .ass (Advanced SubStation Alpha) to the SE-supported file types
Root: HKLM; Subkey: "{#keyApps}\SubtitleEdit.exe\SupportedTypes"; ValueType: string; ValueName: ".ass"; ValueData: ""; Check: HklmKeyExists('{#keyApps}')
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
  if RegQueryStringValue(HKEY_LOCAL_MACHINE, KeyName, '', CurrentProgId) then
  begin
    MyProgId := 'SubtitleEdit.' + FileType;
    if CompareText(CurrentProgId, MyProgId) = 0 then
      RegWriteStringValue(HKEY_LOCAL_MACHINE, KeyName, '', '');
  end;
  Result := IsTaskSelected('associate_' + FileType);
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
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\da_DK_user.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\de_DE_user.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\en_US_user.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\es_MX_user.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\fi_FI_user.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\nl_NL_user.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\pt_PT_user.xml'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\ru_RU_user.xml'));
  DelTree(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\???_OCRFixReplaceList_User.xml'), False, True, False);
  DelTree(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.dic'), False, True, False);
  DelTree(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries\*.aff'), False, True, False);
  RemoveDir(ExpandConstant('{userappdata}\Subtitle Edit\Dictionaries'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Ocr\Latin.db'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\Ocr\Latin.nocr'));
  DeleteFile(ExpandConstant('{userappdata}\Subtitle Edit\preview.mkv'));
  DelTree(ExpandConstant('{userappdata}\Subtitle Edit\Ocr\*.*'), False, True, False);
  RemoveDir(ExpandConstant('{userappdata}\Subtitle Edit\Ocr'));
  DelTree(ExpandConstant('{userappdata}\Subtitle Edit\Plugins\*.*'), False, True, False);
  RemoveDir(ExpandConstant('{userappdata}\Subtitle Edit\Plugins'));
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
  // Returns True if .NET Framework version 4.7.2 is installed, or a compatible version such as 4.8
  Result := IsDotNetDetected('v4.7.2', 0);
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
