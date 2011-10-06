; *** Inno Setup version 5.1.11+ Serbian (Cyrillic) messages ***
;
; Translated by Rancher (theranchcowboy@googlemail.com).
;
; Note: When translating this text, do not add periods (.) to the end of
; messages that didn't have them already, because on those messages Inno
; Setup adds the periods automatically (appending a period would result in
; two periods being displayed).

[LangOptions]
; The following three entries are very important. Be sure to read and 
; understand the '[LangOptions] section' topic in the help file.
LanguageName=<0421><0440><043F><0441><043A><0438>
LanguageID=$0C1A
LanguageCodePage=1251
; If the language you are translating to requires special font faces or
; sizes, uncomment any of the following entries and change them accordingly.
;DialogFontName=
;DialogFontSize=8
;WelcomeFontName=Verdana
;WelcomeFontSize=12
;TitleFontName=Arial
;TitleFontSize=29
;CopyrightFontName=Arial
;CopyrightFontSize=8

[Messages]

; *** Application titles
SetupAppTitle=Инсталација
SetupWindowTitle=Инсталација – %1
UninstallAppTitle=Уклањање
UninstallAppFullTitle=Уклањање програма %1

; *** Misc. common
InformationTitle=Подаци
ConfirmTitle=Потврда
ErrorTitle=Грешка

; *** SetupLdr messages
SetupLdrStartupMessage=Инсталираћете %1. Желите ли да наставите?
LdrCannotCreateTemp=Не могу да направим привремену датотеку. Инсталација је прекинута
LdrCannotExecTemp=Не могу да покренем датотеку у привременој фасцикли. Инсталација је прекинута

; *** Startup error messages
LastErrorMessage=%1.%n%nГрешка %2: %3
SetupFileMissing=Датотека %1 недостаје у фасцикли за инсталацију. Исправите проблем или набавите нови примерак програма.
SetupFileCorrupt=Датотеке за инсталацију су оштећене. Набавите нови примерак програма.
SetupFileCorruptOrWrongVer=Датотеке за инсталацију су оштећене или нису сагласне с овим издањем инсталације. Исправите проблем или набавите нови примерак програма.
NotOnThisPlatform=Програм неће радити на %1.
OnlyOnThisPlatform=Програм ће радити на %1.
OnlyOnTheseArchitectures=Програм се може инсталирати само на издањима виндоуса који раде са следећим архитектурама процесора:%n%n%1
MissingWOW64APIs=Издање виндоуса које користите не садржи могућности потребне за извршавање 64-битних инсталација. Инсталирајте сервисни пакет %1 да бисте решили овај проблем.
WinVersionTooLowError=Програм захтева %1, издање %2 или новије.
WinVersionTooHighError=Програм не можете инсталирати на %1 издању %2 или новијем.
AdminPrivilegesRequired=Морате бити пријављени као администратор да бисте инсталирали програм.
PowerUserPrivilegesRequired=Морате бити пријављени као администратор или овлашћени корисник да бисте инсталирали програм.
SetupAppRunningError=Програм %1 је тренутно покренут.%n%nЗатворите га и кликните на дугме „У реду“ да наставите или „Откажи“ да напустите инсталацију.
UninstallAppRunningError=Програм %1 је тренутно покренут.%n%nЗатворите га и кликните на дугме „У реду“ да наставите или „Откажи“ да напустите инсталацију.

; *** Misc. errors
ErrorCreatingDir=Не могу да направим фасциклу „%1“
ErrorTooManyFilesInDir=Не могу да направим датотеку у фасцикли „%1“ јер садржи превише датотека

; *** Setup common messages
ExitSetupTitle=Напуштање инсталације
ExitSetupMessage=Инсталација није завршена. Ако сада изађете, програм неће бити инсталиран.%n%nИнсталацију можете покренути и довршити неком дугом приликом.%n%nЖелите ли да је затворите?
AboutSetupMenuItem=&О програму
AboutSetupTitle=Подаци о програму
AboutSetupMessage=%1 издање %2%n%3%n%n%1 почетна страница:%n%4
AboutSetupNote=
TranslatorNote=Serbian translation by Rancher

; *** Buttons
ButtonBack=< &Назад
ButtonNext=&Даље >
ButtonInstall=&Инсталирај
ButtonOK=&У реду
ButtonCancel=&Откажи
ButtonYes=&Да
ButtonYesToAll=Д&а за све
ButtonNo=&Не
ButtonNoToAll=Н&е за све
ButtonFinish=&Заврши
ButtonBrowse=&Потражи…
ButtonWizardBrowse=&Потражи…
ButtonNewFolder=&Направи фасциклу

; *** "Select Language" dialog messages
SelectLanguageTitle=Одабир језика
SelectLanguageLabel=Изаберите језик који желите да користите приликом инсталације:

; *** Common wizard text
ClickNext=Кликните на „Даље“ да наставите или „Откажи“ да напустите инсталацију.
BeveledLabel=
BrowseDialogTitle=Одабир фасцикле
BrowseDialogLabel=Изаберите фасциклу са списка и кликните на „У реду“.
NewFolderName=Нова фасцикла

; *** "Welcome" wizard page
WelcomeLabel1=Добро дошли на инсталацију програма [name]
WelcomeLabel2=Инсталираћете [name/ver] на свој рачунар.%n%nПрепоручује се да затворите све друге програме пре него што наставите.

; *** "Password" wizard page
WizardPassword=Лозинка
PasswordLabel1=Инсталација је заштићена лозинком.
PasswordLabel3=Унесите лозинку и кликните на „Даље“ да наставите. Имајте на уму да је лозинка осетљива на мала и велика слова.
PasswordEditLabel=&Лозинка:
IncorrectPassword=Наведена лозинка није исправна. Покушајте поново.

; *** "License Agreement" wizard
WizardLicense=Уговор о лиценци
LicenseLabel=Пажљиво прочитајте следеће пре него што наставите.
LicenseLabel3=Прочитајте Уговор о лиценци који се налази испод. Морате прихватити услове овог уговора пре него што наставите.
LicenseAccepted=&Прихватам уговор
LicenseNotAccepted=&Не прихватам уговор

; *** "Information" wizard pages
WizardInfoBefore=Информације
InfoBeforeLabel=Пажљиво прочитајте следеће пре него што наставите.
InfoBeforeClickLabel=Када будете спремни да наставите инсталацију, кликните на „Даље“.
WizardInfoAfter=Информације
InfoAfterLabel=Пажљиво прочитајте следеће пре него што наставите.
InfoAfterClickLabel=Када будете спремни да наставите инсталацију, кликните на „Даље“.

; *** "User Information" wizard page
WizardUserInfo=Кориснички подаци
UserInfoDesc=Унесите своје податке.
UserInfoName=&Корисник:
UserInfoOrg=&Организација:
UserInfoSerial=&Серијски број:
UserInfoNameRequired=Морате навести име.

; *** "Select Destination Location" wizard page
WizardSelectDir=Одабир одредишне фасцикле
SelectDirDesc=Изаберите место на ком желите да инсталирате [name].
SelectDirLabel3=Програм ће инсталирати [name] у следећу фасциклу.
SelectDirBrowseLabel=Кликните на „Даље“ да наставите. Ако желите да изаберете другу фасциклу, кликните на „Потражи…“.
DiskSpaceMBLabel=Потребно је најмање [mb] MB слободног простора на диску.
ToUNCPathname=Не могу да инсталирам програм у наведену фасциклу. Ако покушавате да га инсталирате на мрежи, прво ћете морати да мапирате мрежни диск.
InvalidPath=Морате навести пуну путању с обележјем диска (нпр.%n%nC:\APP%n%nили путања у облику%n%n\\server\shаre)
InvalidDrive=Диск који сте изабрали не постоји или није доступан. Изаберите неки други.
DiskSpaceWarningTitle=Нема довољно простора на диску
DiskSpaceWarning=Програм захтева најмање %1 kB слободног простора, а изабрани диск на располагању има само %2 kB.%n%nЖелите ли ипак да наставите?
DirNameTooLong=Назив фасцикле или путања је предугачка.
InvalidDirName=Назив фасцикле није исправан.
BadDirName32=Назив фасцикле не сме садржати ништа од следећег:%n%n%1
DirExistsTitle=Фасцикла већ постоји
DirExists=Фасцикла:%n%n%1%n%nвећ постоји. Желите ли ипак да инсталирате програм у њу?
DirDoesntExistTitle=Фасцикла не постоји
DirDoesntExist=Фасцикла:%n%n%1%n%nне постоји. Желите ли да је направите?

; *** "Select Components" wizard page
WizardSelectComponents=Одабир делова
SelectComponentsDesc=Које делове желите да инсталирате?
SelectComponentsLabel2=Изаберите делове које желите да инсталирате, а очистите оне које не желите. Кликните на „Даље“ да наставите.
FullInstallation=Пуна инсталација
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Подразумевана инсталација
CustomInstallation=Прилагођена инсталација
NoUninstallWarningTitle=Делови већ постоје
NoUninstallWarning=Следећи делови већ постоје на вашем рачунару:%n%n%1%n%nДештриклирање ових делова их неће уклонити.%n%nЖелите ли да наставите?
ComponentSize1=%1 kB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Изабране ставке захтевају најмање [mb] MB слободног простора.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Одабир додатних задатака
SelectTasksDesc=Изаберите неке додатне задатке.
SelectTasksLabel2=Изаберите додатне задатке које желите да извршите при инсталирању програма [name] и кликните на „Даље“.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Одабир фасцикле на менију „Старт“
SelectStartMenuFolderDesc=Изаберите место на ком желите да поставите пречице.
SelectStartMenuFolderLabel3=Инсталација ће поставити пречице програма у следећој фасцикли на менију „Старт“.
SelectStartMenuFolderBrowseLabel=Кликните на „Даље“ да наставите. Ако желите да изаберете другу фасциклу, кликните на „Потражи…“.
MustEnterGroupName=Морате навести назив фасцикле.
GroupNameTooLong=Назив фасцикле или путања је предугачка.
InvalidGroupName=Назив фасцикле није исправан.
BadGroupName=Назив фасцикле не сме садржати ништа од следећег:%n%n%1
NoProgramGroupCheck2=Н&е прави фасциклу у менију „Старт“

; *** "Ready to Install" wizard page
WizardReady=Инсталација је спремна
ReadyLabel1=Програм је спреман да инсталира [name] на ваш рачунар.
ReadyLabel2a=Кликните на „Инсталирај“ да започнете инсталацију или „Назад“ да поново прегледате и промените поједине поставке.
ReadyLabel2b=Кликните на „Инсталирај“ да започнете инсталацију.
ReadyMemoUserInfo=Кориснички подаци:
ReadyMemoDir=Одредишна фасцикла:
ReadyMemoType=Врста инсталације:
ReadyMemoComponents=Изабрани делови:
ReadyMemoGroup=Фасцикла на менију „Старт“:
ReadyMemoTasks=Додатни задаци:

; *** "Preparing to Install" wizard page
WizardPreparing=Припрема за инсталацију
PreparingDesc=Програм се припрема да инсталира [name] на ваш рачунар.
PreviousInstallNotCompleted=Инсталација или уклањање претходног програма није завршено. Потребно је да поново покренете рачунар да би се инсталација завршила.%n%nНакон поновног покретања, отворите инсталацију и инсталирајте програм [name].
CannotContinue=Не могу да наставим инсталирање. Кликните на „Откажи“ да изађете.

; *** "Installing" wizard page
WizardInstalling=Инсталирање
InstallingLabel=Сачекајте да се [name] инсталира на ваш рачунар.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=[name] – завршетак инсталације
FinishedLabelNoIcons=Инсталирање програма [name] је завршено.
FinishedLabel=Инсталирање програма [name] је завршено. Можете га покренути преко постављених икона.
ClickFinish=Кликните на „Заврши“ да изађете.
FinishedRestartLabel=Потребно је поновно покретање рачунара да би се инсталација завршила. Желите ли да га поново покренете?
FinishedRestartMessage=Потребно је поновно покретање рачунара да би се инсталација завршила.%n%nЖелите ли да га поново покренете?
ShowReadmeCheck=Да, желим да погледам текстуалну датотеку
YesRadio=&Да, поново покрени рачунар
NoRadio=&Не, касније ћу га покренути
; used for example as 'Run MyProg.exe'
RunEntryExec=&Покрени %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Погледај %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Следећи диск
SelectDiskLabel2=Убаците диск %1 и кликните на „У реду“.%n%nАко се датотеке на овом диску могу пронаћи у некој другој фасцикли, унесите одговарајућу путању или кликните на „Потражи…“.
PathLabel=&Путања:
FileNotInDir2=Датотека „%1“ се не налази у „%2“. Убаците прави диск или изаберите другу фасциклу.
SelectDirectoryLabel=Изаберите место следећег диска.

; *** Installation phase messages
SetupAborted=Инсталација није завршена.%n%nИсправите проблем и покрените је поново.
EntryAbortRetryIgnore=Кликните на „Покушај опет“ да поновите радњу, „Занемари“ да наставите у сваком случају или „Прекини“ да обуставите инсталацију.

; *** Installation status messages
StatusCreateDirs=Правим фасцикле…
StatusExtractFiles=Распакујем датотеке…
StatusCreateIcons=Постављам пречице…
StatusCreateIniEntries=Постављам уносе INI…
StatusCreateRegistryEntries=Постављам уносе у регистар…
StatusRegisterFiles=Уписујем датотеке…
StatusSavingUninstall=Чувам податке о уклањању…
StatusRunProgram=Завршавам инсталацију…
StatusRollback=Поништавам измене…

; *** Misc. errors
ErrorInternal2=Унутрашња грешка: %1
ErrorFunctionFailedNoCode=%1 неуспех
ErrorFunctionFailed=%1 неуспех; код %2
ErrorFunctionFailedWithMessage=%1 неуспех; код %2.%n%3
ErrorExecutingProgram=Не могу да покренем датотеку:%n%1

; *** Registry errors
ErrorRegOpenKey=Грешка при отварању уноса у регистру:%n%1\%2
ErrorRegCreateKey=Грешка при стварању уноса у регистру:%n%1\%2
ErrorRegWriteKey=Грешка при уписивању уноса у регистар:%n%1\%2

; *** INI errors
ErrorIniEntry=Грешка при стварању уноса INI у датотеци „%1“.

; *** File copying errors
FileAbortRetryIgnore=Кликните на „Покушај опет“ да поновите радњу, „Занемари“ да прескочите датотеку (не препоручује се) или „Прекини“ да обуставите инсталацију.
FileAbortRetryIgnore2=Кликните на „Покушај опет“ да поновите радњу, „Занемари“ да наставите у сваком случају (не препоручује се) или „Прекини“ да обуставите инсталацију.
SourceIsCorrupted=Изворна датотека је оштећена
SourceDoesntExist=Изворна датотека „%1“ не постоји
ExistingFileReadOnly=Постојећа датотека је само за читање.%n%nКликните на „Покушај опет“ да уклоните особину „само за читање“ и поновите радњу, „Занемари“ да прескочите датотеку или „Прекини“ да обуставите инсталацију.
ErrorReadingExistingDest=Дошло је до грешке при покушају читања постојеће датотеке:
FileExists=Датотека већ постоји.%n%nЖелите ли да је замените?
ExistingFileNewer=Постојећа датотека је новија од оне коју треба поставити. Препоручује се да задржите постојећу датотеку.%n%nЖелите ли то да урадим?
ErrorChangingAttr=Дошло је до грешке при измени особине следеће датотеке:
ErrorCreatingTemp=Дошло је до грешке при стварању датотеке у одредишној фасцикли:
ErrorReadingSource=Дошло је до грешке при читању изворне датотеке:
ErrorCopying=Дошло је до грешке при умножавању датотеке:
ErrorReplacingExistingFile=Дошло је до грешке при замени постојеће датотеке:
ErrorRestartReplace=Не могу да заменим:
ErrorRenamingTemp=Дошло је до грешке при преименовању датотеке у одредишној фасцикли:
ErrorRegisterServer=Не могу да упишем DLL/OCX: %1
ErrorRegSvr32Failed=RegSvr32 није успео. Грешка %1
ErrorRegisterTypeLib=Не могу да упишем библиотеку типова: %1

; *** Post-installation errors
ErrorOpeningReadme=Дошло је до грешке при отварању текстуалне датотеке.
ErrorRestartingComputer=Не могу да поново покренем рачунар. Урадите то сами.

; *** Uninstaller messages
UninstallNotFound=Датотека „%1“ не постоји. Не могу да уклоним програм.
UninstallOpenError=Датотека „%1“ се не може отворити. Не могу да уклоним програм
UninstallUnsupportedVer=Извештај „%1“ је у непрепознатљивом формату. Не могу да уклоним програм
UninstallUnknownEntry=Непознат унос (%1) се појавио у извештају уклањања
ConfirmUninstall=Желите ли да уклоните %1 и све његове делове?
UninstallOnlyOnWin64=Програм се може уклонити само на 64-битном виндоусу.
OnlyAdminCanUninstall=Програм може уклонити само корисник с администраторским правима.
UninstallStatusLabel=Сачекајте да се %1 уклони с вашег рачунара.
UninstalledAll=%1 је уклоњен с вашег рачунара.
UninstalledMost=%1 је уклоњен.%n%nНеке делове ипак морати сами обрисати.
UninstalledAndNeedsRestart=Потребно је поновно покретање рачунара да би се инсталација завршила.%n%nЖелите ли да поново покренете рачунар?
UninstallDataCorrupted=Датотека „%1“ је оштећена. Не могу да уклоним програм

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Брисање дељене датотеке
ConfirmDeleteSharedFile2=Систем је пријавио да следећу дељену датотеку више не користи ниједан програм. Желите ли да је уклоните?%n%nАко неким програмима и даље треба ова датотека а она је обрисана, ти програми можда неће исправно радити. Ако нисте сигурни шта да радите, кликните на „Не“. Остављање датотеке на диску неће проузроковати никакву штету.
SharedFileNameLabel=Назив датотеке:
SharedFileLocationLabel=Путања:
WizardUninstalling=Стање уклањања
StatusUninstalling=Уклањам %1…

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 издање %2
AdditionalIcons=Додатне иконе:
CreateDesktopIcon=&Постави икону на радну површину
CreateQuickLaunchIcon=П&остави икону на траку за брзо покретање
ProgramOnTheWeb=%1 на интернету
UninstallProgram=Уклони %1
LaunchProgram=Покрени %1
AssocFileExtension=&Повежи %1 с форматом %2
AssocingFileExtension=Повезивање %1 с форматом %2…
