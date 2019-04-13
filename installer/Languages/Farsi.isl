; *** Inno Setup version 5.5.3+ Farsi messages ***
;Translator:Hessam Mohamadi
;Email:hessam55@hotmail.com
; To download user-contributed translations of this file, go to:
;   http://www.jrsoftware.org/files/istrans/
;
; Note: When translating this text, do not add periods (.) to the end of
; messages that didn't have them already, because on those messages Inno
; Setup adds the periods automatically (appending a period would result in
; two periods being displayed).

[LangOptions]
; The following three entries are very important. Be sure to read and 
; understand the '[LangOptions] section' topic in the help file.
LanguageName=Farsi
LanguageID=$0429
LanguageCodePage=1256
; If the language you are translating to requires special font faces or
; sizes, uncomment any of the following entries and change them accordingly.
DialogFontName=Tahoma
DialogFontSize=8
WelcomeFontName=Tahoma
WelcomeFontSize=11
TitleFontName=Tahoma
TitleFontSize=28
CopyrightFontName=Tahoma
CopyrightFontSize=8

[Messages]

; *** Application titles
SetupAppTitle=ÑÇå ÇäÏÇÒ
SetupWindowTitle=%1 - ÑÇå ÇäÏÇÒ
UninstallAppTitle=ÍĞİ ÈÑäÇãå
UninstallAppFullTitle=%1 ÍĞİ ÈÑäÇãå

; *** Misc. common
InformationTitle=ÇØáÇÚÇÊ
ConfirmTitle=ÊÕÏíŞ
ErrorTitle=ÎØÇ

; *** SetupLdr messages
SetupLdrStartupMessage=ÑÇ äÕÈ ÎæÇåÏ ˜ÑÏ.ÂíÇ ÇÏÇãå ãíÏåíÏ¿ %1 Çíä
LdrCannotCreateTemp=İÇíá ãæŞÊí äãíÊæÇäÏ ÓÇÎÊå ÔæÏ. äÕÈ áÛæ ÔÏ
LdrCannotExecTemp=ŞÇÏÑ Èå ÇÌÑÇí İÇíá ÏÑ æÔå ãæŞÊí äíÓÊ. äÕÈ áÛæ ÔÏ

; *** Startup error messages
LastErrorMessage=%1.%n%nÎØÇ %2: %3
SetupFileMissing=ÇÒ æÔå äÕÈ íÇİÊ äãíÔæÏ.áØİÇ ãÔ˜á ÑÇ ÈÑØÑİ ˜ÑÏå íÇ äÓÎå ÌÏíÏí ÇÒ ÈÑäÇãå ÑÇ Êåíå ˜äíÏ %1 İÇíá
SetupFileCorrupt=İÇíáåÇí ÑÇå ÇäÏÇÒ ÎÑÇÈ åÓÊäÏ¡áØİÇ äÓÎå ÌÏíÏí ÇÒ ÈÑäÇãå ÑÇ İÑÇåã äãÇííÏ
SetupFileCorruptOrWrongVer=İÇíá åÇí äÕÈ ÎÑÇÈ ÔÏåÇäÏ¡ íÇ ÈÇ Çíä äÓÎå ÇÒ äÕÈ äÇåãÇåä ãíÈÇÔäÏ.áØİÇ ãÔ˜á ÑÇ ÈÑØÑİ äãÇííÏ íÇ äÓÎå ÌÏíÏí ÇÒ ÈÑäÇãå ÑÇ İÑÇåã äãÇííÏ
InvalidParameter=í˜ ÇÑÇãÊÑ äÇãÚÊÈÑ ÏÑ İÑãÇä ÎØí ÑÏ ÔÏå ÇÓÊ:%n%n%1
SetupAlreadyRunning=ÈÑäÇãå äÕÈ ÇÒ ŞÈá ÏÑ ÇöÌÑÇÓÊ
WindowsVersionNotSupported=Çíä ÈÑäÇãå ÇÒ äÓÎå æíäÏæÒí ˜å ÑÇíÇäå ÔãÇ ÏÑ ÍÇá ÇöÌÑÇÓÊ¡ÔÊíÈÇäí äãí˜äÏ
WindowsServicePackRequired=Çíä ÈÑäÇãå äíÇÒ Èå %1 ÓÑæíÓ ó˜ %2 íÇ ÈÇáÇÊÑ äíÇÒ ÏÇÑÏ
NotOnThisPlatform=ÇÌÑÇ äãíÔæÏ %1 Çíä ÈÑäÇãå ÏÑ
OnlyOnThisPlatform=ÇÌÑÇ ÔæÏ %1 Çíä ÈÑäÇãå ÈÇíÏ ÏÑ
OnlyOnTheseArchitectures=Çíä ÈÑäÇãå ÊäåÇ ÏÑ äÓÎååÇíí ÇÒ æíäÏæÒ ˜å ÈÑÇí ÑÏÇÒÔÑåÇíí ÈÇ ãÚãÇÑíåÇí ÒíÑ ØÑÇÍí ÔÏå ÇÓÊ¡ ãíÊæÇäÏ äÕÈ ÔæÏ:%n%n%1
MissingWOW64APIs=äÕÈ ˜äíÏ %1 Çíä äÓÎå ÇÒ æíäÏæÒ ˜å Ç˜äæä ÏÑ ÇÌÑÇ ãíÈÇÔÏ ÔÇãá ÊæÇÈÚ ãæÑÏ äíÇÒ ÈÑÇí äÕÈ ÔÕÊ æ åÇÑ ÈíÊí äíÓÊ¡ÈÑÇí ÊÕÍíÍ Çíä ãÔ˜á¡áØİÇ ÓÑæíÓ ˜
WinVersionTooLowError=Çíä ÈÑäÇãå äíÇÒ Èå %1 äÓÎå %2 íÇÈÇáÇÊÑ äíÇÒ ÏÇÑÏ
WinVersionTooHighError=Çíä ÈÑäÇãå äãíÊæÇäÏ ÏÑ %1 äÓÎå %2 íÇ ÈÇáÇÊÑ ÇÌÑÇ ÔæÏ
AdminPrivilegesRequired=ÈÑÇí äÕÈ Çíä ÈÑäÇãå¡ ÔãÇ ÈÇíÏ Èå ÚäæÇä ãÏíÑ ÓíÓÊã æÇÑÏ ÔæíÏ
PowerUserPrivilegesRequired=ÈÑÇí äÕÈ Çíä ÈÑäÇãå¡ ÔãÇ ÈÇíÏ Èå ÚäæÇä ãÏíÑ ÓíÓÊã íÇ ÚÖæí ÇÒ ˜ÇÑÈÑÇä ãÌÇÒ æÇÑÏ ÔæíÏ
SetupAppRunningError=Ç˜äæä ÏÑÇÌÑÇ ÇÓÊ %1 ÑÇå ÇäÏÇÒ ÊÔÎíÕ ÏÇÏå%n%náØİÇ ÍÇáÇ åãå äãæäå åÇ ÑÇ ÈÓÊå¡ÓÓ Ñæí ÊÇííÏ ÈÑÇí ÇÏÇãå¡áÛæ ÈÑÇí ÎÑæÌ ˜áí˜ ˜äíÏ
UninstallAppRunningError=Ç˜äæä ÏÑÇÌÑÇ ÇÓÊ %1 ÍĞİ ˜ääÏå ÊÔÎíÕ ÏÇÏå%n%náØİÇ ÍÇáÇ åãå äãæäå åÇ ÑÇ ÈÓÊå¡ÓÓ Ñæí ÊÇííÏ ÈÑÇí ÇÏÇãå¡áÛæ ÈÑÇí ÎÑæÌ ˜áí˜ ˜äíÏ

; *** Misc. errors
ErrorCreatingDir=äíÓÊ "%1" ÈÑäÇãå ŞÇÏÑ Èå ÇíÌÇÏ æÔå
ErrorTooManyFilesInDir=äíÓÊ ÒíÑÇ Âä ãÍÊæí ÊÚÏÇÏ ÒíÇÏí İÇíá ÇÓÊ "%1" ŞÇÏÑ Èå ÇíÌÇÏ İÇíá ÏÑ æÔå

; *** Setup common messages
ExitSetupTitle=ÎÑæÌ ÇÒ äÕÈ
ExitSetupMessage=äÕÈ ˜Çãá äÔÏ. ÇÑ Ç˜äæä ÎÇÑÌ ÔæíÏ¡ ÈÑäÇãå äÕÈ äãíÔæÏ%n%nÔãÇ ãíÊæÇäíÏ äÕÈ ÑÇ ÏÑ æŞÊí ÏíÑ ÏæÈÇÑå ÇÌÑÇ ˜äíÏ ÊÇ äÕÈ ˜Çãá ÑÏÏ. ÎÇÑÌ ãíÔæíÏ¿
AboutSetupMenuItem=...ÏÑÈÇÑå äÕÈ&
AboutSetupTitle=ÏÑÈÇÑå äÕÈ
AboutSetupMessage=%2äÓÎå %1%n%3%n%n%1 ÕİÍå ÎÇäí:%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< Ş&Èáí
ButtonNext=ÈÚ&Ïí >
ButtonInstall=ä&ÕÈ
ButtonOK=&ÊÇííÏ
ButtonCancel=áÛæ&
ButtonYes=Èá&å
ButtonYesToAll=Èáå Èå &åãå
ButtonNo=&ÎíÑ
ButtonNoToAll=äå Ñæí &åãå
ButtonFinish=&ÇíÇä
ButtonBrowse=ÌÓ&ÊÌæ
ButtonWizardBrowse=ÌÓ&ÊÌæ
ButtonNewFolder=ÇíÌÇÏ æÔå ÌÏíÏ

; *** "Select Language" dialog messages
SelectLanguageTitle=ÒÈÇä ÑÇå ÇäÏÇÒ ÑÇ ÇäÊÎÇÈ ˜äíÏ
SelectLanguageLabel=ÒÈÇä ãæÑÏ ÇÓÊİÇÏå Ííä äÕÈ ÑÇ ÇäÊÎÇÈ ˜äíÏ:

; *** Common wizard text
ClickNext=Ñæí ÈÚÏí ÈÑÇí ÇÏÇãå¡íÇ áÛæ ÈÑÇí ÎÑæÌ ÇÒ ÑÇå ÇäÏÇÒ ˜áí˜ ˜äíÏ
BeveledLabel=
BrowseDialogTitle=ÌÓÊÌæ ÈÑÇí æÔå
BrowseDialogLabel=í˜ æÔå ÇÒ áíÓÊ ÒíÑ ÇäÊÎÇÈ ˜äíÏ¡ ÓÓ ÊÇííÏ ÑÇ ˜áí˜ ˜äíÏ
NewFolderName=æÔå ÌÏíÏ

; *** "Welcome" wizard page
WelcomeLabel1=ÎæÔ ÂãÏíÏ [name] Èå æíÒÇÑÏ äÕÈ
WelcomeLabel2=ÑÇ Ñæí ÑÇíÇäå ÔãÇ äÕÈ ãí˜äÏ [name/ver] Çíä ÈÑäÇãå%n%nÊæÕíå ãíÔæÏ¡ÓÇíÑ ÈÑäÇãå åÇ ÑÇ ŞÈá ÇÒ ÇÏÇãå ÏÇÏä ÈÈäÏíÏ

; *** "Password" wizard page
WizardPassword=ÑãÒÚÈæÑ
PasswordLabel1=Çíä ÑÇå ÇäÏÇÒ ÊæÓØ ÑãÒÚÈæÑ ãÍÇİÙÊ ãíÔæÏ
PasswordLabel3=áØİÇ ÑãÒÚÈæÑ ÑÇ æÇÑÏ ˜äíÏ¡ÓÓ Ñæí ÈÚÏí ÈÑÇí ÇÏÇãå ˜áí˜ ˜äíÏ.ÑãÒ ÚÈæÑ Èå ˜æ˜ æÈÒÑ ÈæÏä ÍÑæİ ÍÓÇÓ ÇÓÊ
PasswordEditLabel=ÑãÒÚÈæÑ:
IncorrectPassword=ÑãÒÚÈæÑ æÇÑÏ ÔÏå ÕÍíÍ äíÓÊ.áØİÇ ÏæÈÇÑå ÓÚí ˜äíÏ

; *** "License Agreement" wizard page
WizardLicense=ãÌæÒ ÇÓÊİÇÏå
LicenseLabel=áØİÇ ÇØáÇÚÇÊ ãåã ÒíÑ ÑÇ ŞÈá ÇÒ ÇÏÇãå ÏÇÏä ÈÎæÇäíÏ
LicenseLabel3=áØİÇ ãÌæÒ ÇÓÊİÇÏå ÑÇ ÈÎæÇäíÏ.ÔãÇ ÈÇíÏ ÔÑÇíØ Çíä ãÌæÒ ÑÇ ŞÈá ÇÒ ÇÏÇãå ÏÇÏä ÈĞíÑíÏ
LicenseAccepted=ÊæÇİŞäÇãå ÑÇ &ãíĞíÑã
LicenseNotAccepted=ÊæÇİŞäÇãå ÑÇ &äãíĞíÑã

; *** "Information" wizard pages
WizardInfoBefore=ÇØáÇÚÇÊ
InfoBeforeLabel=áØİÇ ÇØáÇÚÇÊ ãåã ÒíÑ ÑÇ ŞÈá ÇÒ ÇÏÇãå ÏÇÏä ÈÎæÇäíÏ
InfoBeforeClickLabel=ÒãÇäí˜å ÂãÇÏå ÇÏÇãå äÕÈ åÓÊíÏ¡ Ñæí ÈÚÏí ˜áí˜ ˜äíÏ
WizardInfoAfter=ÇØáÇÚÇÊ
InfoAfterLabel=áØİÇ ÇØáÇÚÇÊ ãåã ÒíÑ ÑÇ ŞÈá ÇÒ ÇÏÇãå ÏÇÏä ÈÎæÇäíÏ
InfoAfterClickLabel=ÒãÇäí˜å ÂãÇÏå ÇÏÇãå äÕÈ åÓÊíÏ¡ Ñæí ÈÚÏí ˜áí˜ ˜äíÏ

; *** "User Information" wizard page
WizardUserInfo=ÇØáÇÚÇÊ ˜ÇÑÈÑ
UserInfoDesc=áØİÇ ÇØáÇÚÇÊ ÎæÏ ÑÇ æÇÑÏ ˜äíÏ
UserInfoName=äÇã ˜ÇÑÈÑ&:
UserInfoOrg=Ó&ÇÒãÇä:
UserInfoSerial=ÔãÇÑå ÓÑíÇá&:
UserInfoNameRequired=ÔãÇ ÈÇíÏ í˜ äÇã æÇÑÏ ˜äíÏ

; *** "Select Destination Location" wizard page
WizardSelectDir=ãŞÕÏ äÕÈ ÑÇ ÇäÊÎÇÈ äãÇííÏ
SelectDirDesc=˜ÌÇ ÈÇíÏ äÕÈ ÔæÏ¿ [name]
SelectDirLabel3=ÏÑ æÔå ÒíÑ äÕÈ ÎæÇåÏ ÔÏ [name]
SelectDirBrowseLabel=ÈÑÇí ÇÏÇãå¡ÈÚÏí ÑÇ ˜áí˜ ˜äíÏ.ÈÑÇí ÇäÊÎÇÈ æÔå ãÊİÇæÊ¡Ñæí ÌÓÊÌæ ˜áí˜ ˜äíÏ
DiskSpaceMBLabel=ÏÓÊ˜ã Èå [mb] ãÇÈÇíÊ İÖÇí ÎÇáí äíÇÒ ÎæÇåÏ ÈæÏ
CannotInstallToNetworkDrive=ÑÇå ÇäÏÇÒ äãíÊæÇäÏ ÏÑ ÏÑÇíæ ÔÈ˜å äÕÈ ˜äÏ
CannotInstallToUNCPath=ÑÇå ÇäÏÇÒ äãíÊæÇäÏ ÏÑ ãÓíÑ íæ Çöä Óí äÕÈ ÔæÏ
InvalidPath=ÔãÇ ÈÇíÏ í˜ ãÓíÑ ˜Çãá åãÑÇå ÍÑİ ÏÑÇíæ æÇÑÏ ˜äíÏ¡ÈÑÇí ãËÇá:%n%nC:\APP%n%níÇ í˜ ãÓíÑ íæ Çä Óí ÏÑ Çíä ŞÇáÈ:%n%n\\server\share
InvalidDrive=ÏÑÇíæ íÇ ÇÔÊÑÇ˜ íæ Çä Óí ˜å ÔãÇ ÇäÊÎÇÈ ˜ÑÏå ÇíÏ¡æÌæÏ äÏÇÑÏ íÇ ŞÇÈá ÏÓÊÑÓí äíÓÊ.áØİÇ í˜í ÏíÑ ÑÇ ÇäÊÎÇÈ ˜äíÏ
DiskSpaceWarningTitle=Èå ÇäÏÇÒå ˜Çİí İÖÇí ÎÇáí æÌæÏ äÏÇÑÏ
DiskSpaceWarning=ÍÏÇŞá Èå %1 ˜íáæÈÇíÊ İÖÇí ÎÇáí ÈÑÇí äÕÈ ÈÑäÇãå äíÇÒ ÇÓÊ¡ æáí ÏÑ ÏÑÇíæ ÇäÊÎÇÈ ÔÏå ÊäåÇ %2 ˜íáæÈÇíÊ ŞÇÈá ÇÓÊİÇÏå ÇÓÊ.%n%nÈÇ Çíä æÌæÏ ÂíÇ ãíÎæÇåíÏ ÇÏÇãå ÏåíÏ¿
DirNameTooLong=äÇã æÔå íÇ ãÓíÑ Îíáí ÈáäÏ ÇÓÊ
InvalidDirName=äÇã æÔå ÕÍíÍ äíÓÊ
BadDirName32=äÇã æÔå åÇ äãíÊæÇäÏ ˜ÇÑÇ˜ÊÑåÇí ÒíÑ ÑÇ ÏÑÈÑ ÈíÑÏ:%n%n%1
DirExistsTitle=æÔå æÌæÏ ÏÇÑÏ
DirExists=Çíä æÔå:%n%n%1%n%nÇ˜äæä æÌæÏ ÏÇÑÏ. ÈÇ Çíä æÌæÏ¡ÂíÇ ÏÑ åãíä æÔå äÕÈ ÑÇ ÇÏÇãå ÏåíÏ¿
DirDoesntExistTitle=æÔå æÌæÏ äÏÇÑÏ
DirDoesntExist=Çíä æÔå:%n%n%1%n%næÌæÏ äÏÇÑÏ. ÂíÇ ãíÎæÇåíÏ Çíä æÔå ÇíÌÇÏ ÔæÏ¿

; *** "Select Components" wizard page
WizardSelectComponents=ÇäÊÎÇÈ ÇÌÒÇÁ
SelectComponentsDesc=˜ÏÇã í˜ ÇÒ ÇÌÒÇí ÒíÑ ÈÇíÏ äÕÈ ÔæäÏ¿
SelectComponentsLabel2=ÇÌÒÇíí ˜å ãíÎæÇåíÏ äÕÈ ÔæÏ ÑÇ ÇäÊÎÇÈ¡ ÂäÏÓÊå ÇÒ ÇÌÒÇí ÈÑäÇãå ÑÇ ˜å äãíÎæÇåíÏ äÕÈ ÔæÏ¡ ÇÒ ÍÇáÊ ÇäÊÎÇÈ ÏÑÂæÑíÏ. ÒãÇäí˜å ÂãÇÏå ÔÏíÏ¡ÈÚÏí ÑÇ ˜áí˜ ˜äíÏ
FullInstallation=äÕÈ ˜Çãá
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=äÕÈ İÔÑÏå
CustomInstallation=äÕÈ ÓİÇÑÔí
NoUninstallWarningTitle=ÇÌÒÇ æÌæÏ ÏÇÑäÏ
NoUninstallWarning=ÑÇå ÇäÏÇÒ ÊÔÎíÕ ÏÇÏå ˜å Ç˜äæä ÇÌÒÇí ÒíÑ ÏÑ ÑÇíÇäå ÔãÇ äÕÈ ÔÏåÇäÏ:%n%n%1%n%nÚÏã ÇäÊÎÇÈ Çíä ÇÌÒÇ ÂäåÇ ÑÇ ÍĞİ äÎæÇåÏ ˜ÑÏ.%n%nÈÇ Çíä æÌæÏ ÂíÇ ãíÎæÇåíÏ ÇÏÇãå ÏåíÏ¿
ComponentSize1=%1 ˜íáæÈÇíÊ
ComponentSize2=%1 ãÇÈÇíÊ
ComponentsDiskSpaceMBLabel=ÇäÊÎÇÈ ÌÇÑí¡ ÏÓÊ˜ã Èå [mb] ãÇÈÇíÊ İÖÇí ÎÇáí ÏÑ ÏíÓ˜ äíÇÒ ÏÇÑÏ

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=ÇäÊÎÇÈ æÙÇíİ ÇÖÇİí
SelectTasksDesc=˜ÏÇã í˜ ÇÒ æÙÇíİ ÇÖÇİí ÈÇíÏ ÇäÌÇã ÔæÏ¿
SelectTasksLabel2=ÑÇ ÇäÊÎÇÈ¡ÓÓ Ñæí ÈÚÏí ˜áí˜ ˜äíÏ [name] æÙÇíİ ÇÖÇİí¡Ííä äÕÈ

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=ÇäÊÎÇÈ æÔå ÇÓÊÇÑÊ ãäæ
SelectStartMenuFolderDesc=ãíÇäÈÑåÇí ÈÑäÇãå ˜ÌÇ ÈÇíÏ ŞÑÇÑ íÑäÏ¿
SelectStartMenuFolderLabel3=ÑÇå ÇäÏÇÒ¡ãíÇäÈÑ ÑÇ ÏÑ æÔå ÇÓÊÇÑÊ ãäæí ÒíÑ ÇíÌÇÏ ãí˜äÏ
SelectStartMenuFolderBrowseLabel=ÈÑÇí ÇÏÇãå¡ÈÚÏí ÑÇ ˜áí˜ ˜äíÏ.ÈÑÇí ÇäÊÎÇÈ æÔå ãÊİÇæÊ¡Ñæí ÌÓÊÌæ ˜áí˜ ˜äíÏ
MustEnterGroupName=ÔãÇ ÈÇíÏ äÇã æÔå ÑÇ æÇÑÏ ˜äíÏ
GroupNameTooLong=äÇã æÔå íÇ ãÓíÑ Îíáí ÈáäÏ ÇÓÊ
InvalidGroupName=äÇã æÔå ÕÍíÍ äíÓÊ
BadGroupName=äÇã æÔå äãíÊæÇäÏ ˜ÇÑÇ˜ÊÑåÇí ÒíÑ ÑÇ ÔÇãá ÔæÏ:%n%n%1
NoProgramGroupCheck2=æÔå ÇÓÊÇÑÊ ãäæ ÑÇ ÇíÌÇÏ ä˜ä

; *** "Ready to Install" wizard page
WizardReady=ÂãÇÏå ÈÑÇí äÕÈ
ReadyLabel1=Ñæí ÑÇíÇäå ÔãÇ ãíÈÇÔÏ [name] ÑÇå ÇäÏÇÒ¡ ÂãÇÏå äÕÈ
ReadyLabel2a=Ñæí äÕÈ ÈÑÇí ÇÏÇãå äÕÈ ÈÑäÇãå ¡íÇ Ñæí ŞÈáí ÈÑÇí ÈÇÒÈíäí íÇ ÊÛííÑ ÊäÙíãÇÊ ˜áí˜ ˜äíÏ
ReadyLabel2b=Ñæí äÕÈ¡ÈÑÇí ÇÏÇãå äÕÈ ÈÑäÇãå ˜áí˜ ˜äíÏ
ReadyMemoUserInfo=ÇØáÇÚÇÊ ˜ÇÑÈÑ:
ReadyMemoDir=ã˜Çä ãŞÕÏ:
ReadyMemoType=äæÚ äÕÈ:
ReadyMemoComponents=ÇÌÒÇí ÇäÊÎÇÈ ÔÏå:
ReadyMemoGroup=æÔå ÇÓÊÇÑÊ ãäæ:
ReadyMemoTasks=æÙÇíİ ÇÖÇİí:

; *** "Preparing to Install" wizard page
WizardPreparing=ÂãÇÏå ÓÇÒí ÈÑÇí äÕÈ
PreparingDesc=Ñæí ÑÇíÇäå ÇÓÊ [name] ÈÑäÇãå ÂãÇÏå äÕÈ
PreviousInstallNotCompleted=İÑÇíäÏ äÕÈ¡ÍĞİ ÈÑäÇãå ŞÈáí ˜Çãá äÔÏå¡ÔãÇ äíÇÒ Èå ÑíÓÊÇÑÊ ÈÑÇí ˜Çãá ÔÏä İÑÇíäÏ ÏÇÑíÏ%n%nÈÚÏ ÇÒ ÑíÓÊÇÑÊ ÑÇíÇäå¡ÏæÈÇÑå ÑÇå ÇäÏÇÒ [name] ÈÑÇí ˜Çãá ÔÏä äÕÈ ÇÌÑÇ ˜äíÏ
CannotContinue=äÕÈ ÑÇ äãí ÊæÇä ÇÏÇãå ÏÇÏ¡ÈÑÇí ÎÑæÌ Ñæí áÛæ ˜áí˜ ˜äíÏ
ApplicationsFound=ÈÑäÇãå åÇí ÒíÑ ÇÒ İÇíá åÇíí ˜å äíÇÒ Èå ÈÑæÒÑÓÇäí ÊæÓØ ÈÑäÇãå ÑÇ ÏÇÑäÏ ÇÓÊİÇÏå ãí˜ääÏ.ÊæÕíå ãíÔæÏ ÇÌÇÒå ÏåíÏ ÈÑäÇãå ÎæÏ˜ÇÑ Âä ÈÑäÇãå åÇ ÑÇ ÈÈäÏÏ
ApplicationsFound2=ÈÑäÇãå åÇí ÒíÑ ÇÒ İÇíá åÇíí ˜å äíÇÒ Èå ÈÑæÒÑÓÇäí ÊæÓØ ÑÇå ÇäÏÇÒ ÑÇ ÏÇÑäÏ ÇÓÊİÇÏå ãí˜ääÏ.ÊæÕíå ãíÔæÏ ÇÌÇÒå ÏåíÏ ÑÇå ÇäÏÇÒ ÎæÏ˜ÇÑ Âä ÈÑäÇãå åÇ ÑÇ ÈÈäÏÏ.ÈÚÏ ÇÒ ˜Çãá ÔÏä äÕÈ¡ÑÇå ÇäÏÇÒ Âä ÈÑäÇãå åÇ ÑÇ ÑíÓÊÇÑÊ ÎæÇåÏ ˜ÑÏ.
CloseApplications=&ÈÓÊä ÎæÏ˜ÇÑ ÈÑäÇãå åÇ
DontCloseApplications=&ÈÑäÇãå åÇ ÑÇ äÈäÏ
ErrorCloseApplications=ÑÇå ÇäÏÇÒå ŞÇÏÑ Èå ÈÓÊä ÎæÏ˜ÇÑ åãå ÈÑäÇãå åÇ äíÓÊ.ÊæÕíå ãíÔæÏ ÔãÇ åãå ÈÑäÇãå åÇíí ÑÇ ˜å İÇíá åÇí Âä ÈÇíÏ ÈÑæÒÑÓÇäí ÔæäÏ ŞÈá ÇÒ ÇÏÇãå äÕÈ ÈÈäÏíÏ

; *** "Installing" wizard page
WizardInstalling=ÏÑ ÍÇá äÕÈ
InstallingLabel=Ñæí ÑÇíÇäå¡ãäÊÙÑ ÈãÇäíÏ [name] áØİÇ ÊÇ ÒãÇä äÕÈ

; *** "Setup Completed" wizard page
FinishedHeadingLabel=˜Çãá ÔÏ [name] æíÒÇÑÏ äÕÈ
FinishedLabelNoIcons=ÏÑ ÑÇíÇäå ÔãÇ Èå ÇíÇä ÑÓíÏå ÇÓÊ [name] äÕÈ
FinishedLabel=ÈÇ ãæİŞíÊ ÇäÌÇã ÔÏ [name] äÕÈ
ClickFinish=ÈÑÇí ÎÑæÌ ÇÒ ÑÇå ÇäÏÇÒ ÇíÇä ÑÇ ˜áí˜ ˜äíÏ
FinishedRestartLabel=ÑÇíÇäå ÈÇíÏ ÑíÓÊÇÑÊ ÔæÏ.ÍÇáÇ ÊãÇíá Èå ÑíÓÊÇÑÊ ÏÇÑíÏ¿ [name] ÈÑÇí ˜Çãá ˜ÑÏä äÕÈ
FinishedRestartMessage=ÑÇíÇäå ÈÇíÏ ÑíÓÊÇÑÊ ÔæÏ [name] ÈÑÇí ˜Çãá ˜ÑÏä äÕÈ%n%nÍÇáÇ ÊãÇíá Èå ÑíÓÊÇÑÊ ÏÇÑíÏ¿
ShowReadmeCheck=Èáå¡ ãä İÇíá «ãÑÇ ÈÎæÇä» ÑÇ ãíÎæÇäã
YesRadio=Èáå¡ÍÇáÇ ÑÇíÇäå ÑÇ ÑíÓÊÇÑÊ ˜ä
NoRadio=ÎíÑ¡ÈÚÏÇ ÑÇíÇäå ÑÇ ÑíÓÊÇÑÊ ãí˜äã
; used for example as 'Run MyProg.exe'
RunEntryExec=%1 ÇÌÑÇí
; used for example as 'View Readme.txt'
RunEntryShellExec=%1 ãÔÇåÏå

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=ÑÇå ÇäÏÇÒ Èå ÏíÓ˜ ÈÚÏí ÇÍÊíÇÌ ÏÇÑÏ
SelectDiskLabel2=ÑÇ ŞÑÇÑ ÏÇÏå æ Ñæí ÊÇííÏ ˜áí˜ ˜äíÏ %1 áØİÇ ÏíÓ˜%n%nÇÑ İÇíáåÇí ÏÑæä Çíä ÏíÓ˜ æÔåí ÏíÑí ÛíÑ ÇÒ Çíä˜å ÏÑ ÒíÑ äãÇíÔ ÏÇÏå¡ íÏÇ ãíÔæäÏ¡ ãÓíÑ ÏÑÓÊ ÑÇ æÇÑÏ ˜äíÏ íÇ ã˜Çä ÏíÑ ÑÇ ÇäÊÎÇÈ ˜äíÏ
PathLabel=&ãÓíÑ:
FileNotInDir2=İÇíá "%1" äãíÊæÇäÏ ÏÑ "%2" íÏÇ ÔæÏ. áØİÇ ÏíÓ˜ ÏÑÓÊ ÑÇ ŞÑÇÑ ÏåíÏ íÇ æÔåí ÏíÑí ÑÇ ÇäÊÎÇÈ ˜äíÏ
SelectDirectoryLabel=áØİÇ ãÓíÑ ÏíÓ˜ ÈÚÏí ÑÇ ãÔÎÕ ˜äíÏ

; *** Installation phase messages
SetupAborted=äÕÈ ˜Çãá äÔÏ%n%náØİÇ äÓÈÊ Èå ÑİÚ ãÔ˜á ÇŞÏÇã æ ÑÇå ÇäÏÇÒ ÑÇ ÏæÈÇÑå ÇÌÑÇ äãÇííÏ
EntryAbortRetryIgnore=Ñæí ÊÌÏíÏäÙÑ ÈÑÇí ÓÚí ÏæÈÇÑå¡ ÔãæÔí ÈÑÇí ÇÏÇãå ÏÑåÑ ÕæÑÊ¡ íÇ áÛæ ÈÑÇí ÎÑæÌ ÇÒ äÕÈ ˜áí˜ ˜äíÏ

; *** Installation status messages
StatusClosingApplications=...ÈÓÊä ÈÑäÇãå åÇ
StatusCreateDirs=...ÇíÌÇÏ æÔå åÇ
StatusExtractFiles=...ÇÓÊÎÑÇÌ İÇíá åÇ
StatusCreateIcons=...ÇíÌÇÏ ãíÇäÈÑåÇ
StatusCreateIniEntries=...INI ÇíÌÇÏ æÑæÏí
StatusCreateRegistryEntries=..ÇíÌÇÏ æÑæÏí ÑíÌÓÊÑí
StatusRegisterFiles=...ËÈÊ İÇíáåÇ
StatusSavingUninstall=...ĞÎíÑå ÇØáÇÚÇÊ ÍĞİ ÈÑäÇãå
StatusRunProgram=...ÏÑ ÍÇá ÇÊãÇã äÕÈ
StatusRestartingApplications=...ÑíÓÊÇÑÊ ÈÑäÇãå åÇ
StatusRollback=...ÏÑ ÍÇá ÈÇÒÑÏÇäÏä ÊÛííÑÇÊ

; *** Misc. errors
ErrorInternal2=ÎØÇí ÏÇÎáí: %1
ErrorFunctionFailedNoCode=%1 ãæİŞ äÔÏ
ErrorFunctionFailed=%1ãæİŞ äÔÏ¡ ˜Ï %2
ErrorFunctionFailedWithMessage=%1ãæİŞ äÔÏ¡ ˜Ï %2.%n%3
ErrorExecutingProgram=ŞÇÏÑ Èå ÇÌÑÇí İÇíá äíÓÊ:%n%1

; *** Registry errors
ErrorRegOpenKey=ÎØÇí ÈÇÒ˜ÑÏä ˜áíÏ ÑíÌÓÊÑí:%n%1\%2
ErrorRegCreateKey=ÎØÇí ÇíÌÇÏ ˜áíÏ ÑíÌÓÊÑí:%n%1\%2
ErrorRegWriteKey=ÎØÇí äæÔÊä ˜áíÏ ÑíÌÓÊÑí:%n%1\%2

; *** INI errors
ErrorIniEntry="ÎØÇí ÇíÌÇÏ æÑæÏí Çíäí ÏÑ İÇíá "%1

; *** File copying errors
FileAbortRetryIgnore=Ñæí ÊÌÏíÏäÙÑ ÈÑÇí ÓÚí ÏæÈÇÑå¡ ÔãæÔí ÈÑÇí ÑÏ Çíä İÇíá(ÊæÕíå äãíÔæÏ)¡ íÇ áÛæ ÈÑÇí ÎÑæÌ ÇÒ äÕÈ ˜áí˜ ˜äíÏ
FileAbortRetryIgnore2=Ñæí ÊÌÏíÏäÙÑ ÈÑÇí ÓÚí ÏæÈÇÑå¡ ÔãæÔí ÈÑÇí ÇÏÇãå ÏÑåÑ ÕæÑÊ(ÊæÕíå äãíÔæÏ)¡ íÇ áÛæ ÈÑÇí ÎÑæÌ ÇÒ äÕÈ ˜áí˜ ˜äíÏ
SourceIsCorrupted=İÇíá ãäÈÚ ÎÑÇÈ ÇÓÊ
SourceDoesntExist=æÌæÏ äÏÇÑÏ "%1" İÇíá ãäÈÚ
ExistingFileReadOnly=İÇíá ãæÌæÏ Èå ÚäæÇä İÇíá İŞØ ÎæÇäÏäí äÔÇäå ÔÏå ÇÓÊ%n%nÑæí ÊÌÏíÏäÙÑ ÈÑÇí ÍĞİ ÕİÊ İŞØ ÎæÇäÏäí æ ÓÚí ÏæÈÇÑå¡ÔãæÔí ÈÑÇí ÑÏ Çíä İÇíá¡ íÇ áÛæ ÈÑÇí ÎÑæÌ ÇÒ äÕÈ ˜áí˜ ˜äíÏ
ErrorReadingExistingDest=í˜ ÎØÇ ÏÑ Ííä ÎæÇäÏä İÇíá ãæÌæÏ ÑÎ ÏÇÏ:
FileExists=İÇíá ÏÑ ÍÇá ÍÇÖÑ æÌæÏ ÏÇÑÏ%n%nãíÎæÇåíÏ Çíä İÇíá ÌÇíÒíä ÔæÏ¿
ExistingFileNewer=İÇíá ãæÌæÏ ÌÏíÏÊÑ ÇÒ İÇíáí ÇÓÊ ˜å ÏÑ äÕÈ ÈÑäÇãå æÌæÏ ÏÇÑÏ. ÊæÕíå ãíÔæÏ ˜å İÇíá ãæÌæÏ ÑÇ äå ÏÇÑíÏ.%n%nÂíÇ ãíÎæÇåíÏ ˜å Çíä İÇíá ãæÌæÏ ÑÇ äå ÏÇÑíÏ¿
ErrorChangingAttr=í˜ ÎØÇ Ííä ÓÚí ÈÑÇí ÊÛííÑ ÕİÇÊ İÇíá ãæÌæÏ ÑÎ ÏÇÏ:
ErrorCreatingTemp=í˜ ÎØÇ Ííä ÓÚí ÈÑÇí ÓÇÎÊä í˜ İÇíá ÏÑ æÔå ãŞÕÏ ÑÎ ÏÇÏ:
ErrorReadingSource=í˜ ÎØÇ Ííä ÓÚí ÈÑÇí ÎæÇäÏä İÇíá ãäÈÚ ÑÎ ÏÇÏ:
ErrorCopying=í˜ ÎØÇ Ííä ÓÚí ÈÑÇí ˜í í˜ İÇíá ÑÎ ÏÇÏ:
ErrorReplacingExistingFile=í˜ ÎØÇ Ííä ÓÚí ÈÑÇí ÌÇíÒíäí İÇíá ãæÌæÏ ÑÎ ÏÇÏ:
ErrorRestartReplace=ãæİŞ Èå ÌÇíÒíäí ÈÚÏ ÇÒ ÑíÓÊÇÑÊ äÔÏ:
ErrorRenamingTemp=í˜ ÎØÇ ÏÑ Ííä ÓÚí ÈÑÇí ÊÛííÑ äÇã í˜ İÇíá ÏÑ æÔå ãŞÕÏ ÑÎ ÏÇÏ:
ErrorRegisterServer=äÇÊæÇä ÏÑ ËÈÊ Ïí Çá Çá/Çæ Óí Çí˜Ó: %1
ErrorRegSvr32Failed=%1 ÑíÌÓÊÑÓÑæÑ Óí æ Ïæ ãæİŞ äÔÏ ÈÇ ˜Ï ÎÑæÌ
ErrorRegisterTypeLib=äÇÊæÇäí ÏÑ ËÈÊ äæÚ ˜ÊÇÈÎÇäå: %1

; *** Post-installation errors
ErrorOpeningReadme=í˜ ÎØÇ ÏÑ Ííä ÓÚí ÈÑÇí ÎæÇäÏä İÇíá «ãÑÇ ÈÎæÇä» ÑÎ ÏÇÏ
ErrorRestartingComputer=ÑÇå ÇäÏÇÒ ŞÇÏÑ Èå ÑíÓÊÇÑÊ ÑÇíÇäå äíÓÊ.áØİÇ Çíä ˜ÇÑ ÑÇ ÈØæÑ ÏÓÊí ÇÌÑÇ ˜äíÏ

; *** Uninstaller messages
UninstallNotFound=æÌæÏ äÏÇÑÏ.äãíÊæÇä ÍĞİ ˜ÑÏ "%1" İÇíá
UninstallOpenError=äãíÊæÇäÏ ÈÇÒÔæÏ.äãíÊæÇä ÍĞİ ˜ÑÏ "%1" İÇíá
UninstallUnsupportedVer=ÏÑí˜ İÑãÊ ÔäÇÓÇíí äÔÏå ÈÇ Çíä äÓÎå ÇÒ ÍĞİ ˜ääÏå åÓÊ.äãíÊæÇä ÍĞİ ˜ÑÏ "%1" İÇíá æŞÇíÚ ÍĞİ
UninstallUnknownEntry=ãæÇÌå ÔÏå ÏÑ æŞÇíÚ ÍĞİ (%1) í˜ æÑæÏí äÇãÔÎÕ
ConfirmUninstall=ÑÇ ÈØæÑ ˜Çãá ÍĞİ ˜äíÏ¿ %1 ÂíÇ ãíÎæÇåíÏ
UninstallOnlyOnWin64=Çíä ÑÇå ÇäÏÇÒ ÊäåÇ ãíÊæÇäÏ ÇÒ Ñæí æíäÏæÒåÇí 64ÈíÊí ÍĞİ ÔæÏ
OnlyAdminCanUninstall=Çíä ÑÇå ÇäÏÇÒ ÊäåÇ ãíÊæÇäÏ Èå æÓíáå ˜ÇÑÈÑí ÈÇ ÇãÊíÇÒÇÊ ãÏíÑíÊí ÍĞİ ÔæÏ
UninstallStatusLabel=ÇÒ ÑÇíÇäå ãäÊÙÑ ÈãÇäíÏ %1 áØİÇ ÊÇ ÒãÇä ÍĞİ
UninstalledAll=ÈÇ ãæİŞíÊ ÇÒ ÑÇíÇäå ÔãÇ ÍĞİ ÔÏ %1
UninstalledMost=˜Çãá ÔÏ %1 ÍĞİ%n%nÈÚÖí ÇÒ İÇíáåÇ ÍĞİ äÔÏäÏ¡ÂäåÇ ÑÇ ãíÊæÇäíÏ ÏÓÊí ÍĞİ ˜äíÏ
UninstalledAndNeedsRestart=ÑÇíÇäå ÈÇíÏ ÑíÓÊÇÑÊ ÔæÏ %1 ÈÑÇí ˜Çãá ÔÏä ÍĞİ%n%nÍÇáÇ ÊãÇíá Èå ÑíÓÊÇÑÊ ÏÇÑíÏ¿
UninstallDataCorrupted=ÎÑÇÈ åÓÊ.äãíÊæÇä ÍĞİ ˜ÑÏ "%1" İÇíá

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=İÇíáåÇí ãÔÊÑ˜ ÈÇ ÓÇíÑ äÑã ÇİÒÇÑåÇ åã ÍĞİ ÔæäÏ¿
ConfirmDeleteSharedFile2=ÓíÓÊã ÊÔÎíÕ ÏÇÏå ˜å İÇíá ÇÔÊÑÇ˜í ÒíÑ ãÏÊí ÇÓÊ ÊæÓØ åí ÈÑäÇãåÇí ÇÓÊİÇÏå äÔÏå ÇÓÊ. ÂíÇ ÊãÇíáí ÈÑÇí ÍĞİ Çíä İÇíá ÇÔÊÑÇ˜í ÏÇÑíÏ¿%n%nÇÑ ÈÑäÇãååÇíí åäæÒ ÇÒ Çíä İÇíá ÇÓÊİÇÏå ãí˜ääÏ æ Çíä İÇíá ÍĞİ ÔæÏ¡ ãã˜ä ÇÓÊ Âä ÈÑäÇãååÇ Èå ÏÑÓÊí ˜ÇÑ ä˜ääÏ. ÇÑ ÇØãíäÇä äÏÇÑíÏ ÎíÑ ÑÇ ÇäÊÎÇÈ ˜äíÏ.ãÇäÏä Çíä İÇíá ÏÑ ÓíÓÊã ÔãÇ ãÔ˜áí íÔ äãíÂæÑÏ
SharedFileNameLabel=äÇã İÇíá:
SharedFileLocationLabel=ã˜Çä:
WizardUninstalling=æÖÚíÊ ÍĞİ ÈÑäÇãå
StatusUninstalling=...%1 ÏÑ ÍÇá ÍĞİ

; *** Shutdown block reasons
ShutdownBlockReasonInstallingApp=%1 äÕÈ
ShutdownBlockReasonUninstallingApp=%1 ÏÑ ÍÇá ÍĞİ

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 %2
AdditionalIcons=Âí˜æä ÇÖÇİí:
CreateDesktopIcon=ÇíÌÇÏ ãíÇäÈÑ ÏÓ˜ÊÇ
CreateQuickLaunchIcon=ÇíÌÇÏ ãíÇäÈÑ ÇöÌÑÇí ÓÑíÚ
ProgramOnTheWeb=%1 ÏÑ æÈ
UninstallProgram=%1 ÍĞİ
LaunchProgram=%1 ÔÑæÚ
AssocFileExtension=æÇÈÓÊå ÓÇÒí %1 ÈÇ %2 ÓæäÏ İÇíá&
AssocingFileExtension=æÇÈÓÊå ÓÇÒí %1 ÈÇ %2 ÓæäÏ İÇíá
AutoStartProgramGroupDescription=ÇöÓÊÇÑÊ Â:
AutoStartProgram=%1 ÔÑæÚ ÎæÏ˜ÇÑ
AddonHostProgramNotFound=%1 äãíÊæÇäÏ ÏÑ æÔå Çí ˜å ÇäÊÎÇÈ ˜ÑÏíÏ ã˜Çä íÇÈí ÔæÏ.%n%nÈåÑÍÇá ãíÎæÇåíÏ ÇÏÇãå ÏåíÏ¿
