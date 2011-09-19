; *** Inno Setup version 5.1.11+ Serbian (Cyrillic) messages ***
;
; Vladimir Stefanovic, antivari@gmail.com, 18.10.2008
;
; Note: When translating this text, do not add periods (.) to the end of
; messages that didn't have them already, because on those messages Inno
; Setup adds the periods automatically (appending a period would result in
; two periods being displayed).

[LangOptions]
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
SetupWindowTitle=Инсталација - %1
UninstallAppTitle=Деинсталација
UninstallAppFullTitle=%1 Деинсталација

; *** Misc. common
InformationTitle=Информације
ConfirmTitle=Потврда
ErrorTitle=Грешка

; *** SetupLdr messages
SetupLdrStartupMessage=Овим програмом ћете инсталирати %1. Да ли желите да наставите?
LdrCannotCreateTemp=Није могуће направити привремену датотеку. Инсталација је прекинута
LdrCannotExecTemp=Није могуће покренути датотеку у привременом директоријуму. Инсталација је прекинута

; *** Startup error messages
LastErrorMessage=%1.%n%nГрешка %2: %3
SetupFileMissing=Датотека %1 недостаје у инсталационом директоријуму. Молимо Вас исправите проблем или набавите нову копију програма.
SetupFileCorrupt=Инсталационе датотеке су неисправне. Молимо Вас да набавите нову копију програма.
SetupFileCorruptOrWrongVer=Инсталационе датотеке су неисправне, или нису усаглашене са овом верзијом инсталације. Молимо Вас исправите проблем или набавите нову копију програма.
NotOnThisPlatform=Овај програм се неће покренути на %1.
OnlyOnThisPlatform=Овај програм се мора покренути на %1.
OnlyOnTheseArchitectures=Овај програм се може инсталирати само на верзијама Windows-а пројектованим за следеће процесорске архитектуре:%n%n%1
MissingWOW64APIs=Верзија Windows-а коју користите не садржи могућности потребне за инсталациону процедуру да уради 64-битну инсталацију. Да би решили овај проблем, молимо инсталирајте Service Pаck %1.
WinVersionTooLowError=Овај програм захтева %1 верзију %2 или новију.
WinVersionTooHighError=Овај програм се не може инсталирати на %1 верзији %2 или новијој.
AdminPrivilegesRequired=Морате бити пријављени као администратор да би сте инсталирали овај програм.
PowerUserPrivilegesRequired=Морате бити пријављени као администратор или као члан Power Users групе када инсталирате овај програм.
SetupAppRunningError=Инсталација је открила да се %1 тренутно извршава.%n%nМолимо да одмах затворите све његове инстанце, а затим притисните OK за наставак, или Cancel да одустанете.
UninstallAppRunningError=Деинсталација је открила да се %1 тренутно извршава.%n%nМолимо да одмах затворите све његове инстанце, а затим притисните OK за наставак, или Cancel да одустанете.

; *** Misc. errors
ErrorCreatingDir=Инсталација није могла да направи директоријум "%1"
ErrorTooManyFilesInDir=Није могуће направити датотеку у директоријуму "%1" зато што садржи превише датотека

; *** Setup common messages
ExitSetupTitle=Прекидање инсталације
ExitSetupMessage=Инсталација није завршена. Ако сада прекинете Инсталацију, програм неће бити инсталиран.%n%nИнсталацију можете покренути и довршити неком дугом приликом.%n%nПрекид инсталације?
AboutSetupMenuItem=&О инсталацији...
AboutSetupTitle=О инсталацији
AboutSetupMessage=%1 верзија %2%n%3%n%n%1 матична страница:%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< &Назад
ButtonNext=&Даље >
ButtonInstall=&Инсталирај
ButtonOK=ОК
ButtonCancel=Одустани
ButtonYes=&Да
ButtonYesToAll=Да за &Све
ButtonNo=&Не
ButtonNoToAll=Н&е за Све
ButtonFinish=&Завршетак
ButtonBrowse=&Изабери...
ButtonWizardBrowse=И&забери...
ButtonNewFolder=&Направи нови директоријум

; *** "Select Language" dialog messages
SelectLanguageTitle=Изаберите језик инсталације
SelectLanguageLabel=Изаберите језик који желите да користите приликом инсталације:

; *** Common wizard text
ClickNext=Притисните Даље да наставите, или Одустани да напустите инсталацију.
BeveledLabel=
BrowseDialogTitle=Изаберите директоријум
BrowseDialogLabel=Изаберите један од понуђених директоријума из листе, а затим притисните ОК.
NewFolderName=Нови Директоријум

; *** "Welcome" wizard page
WelcomeLabel1=Добродошли у [name] инсталациону процедуру
WelcomeLabel2=Сада ће се инсталирати [name/ver] на Ваш рачунар.%n%nПрепоручује се да затворите све друге програме пре наставка.

; *** "Password" wizard page
WizardPassword=Шифра
PasswordLabel1=Ова инсталација је заштићена шифром.
PasswordLabel3=Молимо упишите шифру, а затим притисните Даље за наставак. Водите рачуна да су велика и мала слова у шифри битна.
PasswordEditLabel=&Шифра:
IncorrectPassword=Шифра коју сте уписали није исправна. Молимо покушајте поново.

; *** "License Agreement" wizard
WizardLicense=Уговор о коришћењу
LicenseLabel=Молимо прочитајте пажљиво следеће важне информације пре наставка.
LicenseLabel3=Молимо прочитајте Уговор о коришћењу, који следи. Морате прихватити услове овог уговора пре наставка инсталације.
LicenseAccepted=&Прихватам уговор
LicenseNotAccepted=&Не прихватам уговор

; *** "Information" wizard pages
WizardInfoBefore=Информације
InfoBeforeLabel=Молимо прочитајте пажљиво следеће важне информације пре наставка.
InfoBeforeClickLabel=Када будете спремни да наставите инсталацију, притисните Даље.
WizardInfoAfter=Информације
InfoAfterLabel=Молимо Вас прочитајте пажљиво следеће важне информације пре наставка.
InfoAfterClickLabel=Када будете спремни да наставите инсталацију, притисните Даље.

; *** "User Information" wizard page
WizardUserInfo=Подаци о кориснику
UserInfoDesc=Молимо унесите Ваше податке.
UserInfoName=&Корисник:
UserInfoOrg=&Организација:
UserInfoSerial=&Серијски број:
UserInfoNameRequired=Морате уписати име.

; *** "Select Destination Location" wizard page
WizardSelectDir=Изаберите одредишну локацију
SelectDirDesc=Где [name] треба да се инсталира?
SelectDirLabel3=Инсталација ће поставити [name] у следећи директоријум.
SelectDirBrowseLabel=Да наставите, притисните Даље. Ако желите да изаберете неки други директоријум, притисните Изабери.
DiskSpaceMBLabel=Потребно је најмање [mb] MB слободног простора на диску.
ToUNCPathname=Путања за инсталацију не сме бити у UNC облику. Ако покушавате да инсталирате програм на мрежу, мораћете претходно да мапирате мрежни диск.
InvalidPath=Морате уписати пуну путању са обележјем диска; на пример:%n%nC:\APP%n%nили UNC путања у облику:%n%n\\server\shаre
InvalidDrive=Диск или UNC путања коју сте изабрали не постоје или нису доступни. Молимо изаберите нешто друго.
DiskSpaceWarningTitle=Нема довољно простора на диску
DiskSpaceWarning=Инсталација захтева најмање %1 KB слободног простора, а изабрани диск има само %2 KB на располагању.%n%nДа ли ипак желите да наставите?
DirNameTooLong=Назив директоријума или путања су предугачки.
InvalidDirName=Назив директоријума није исправан.
BadDirName32=Називи директоријума не смеју имати било које од следећих слова:%n%n%1
DirExistsTitle=Директоријум постоји
DirExists=Директоријум:%n%n%1%n%nвећ постоји. Да ли ипак желите да програм инсталирате у њему?
DirDoesntExistTitle=Директоријум не постоји
DirDoesntExist=Директоријум:%n%n%1%n%nне постоји. Да ли желите да га направим?

; *** "Select Components" wizard page
WizardSelectComponents=Изаберите компоненте
SelectComponentsDesc=Које компоненте ћете инсталирати?
SelectComponentsLabel2=Изаберите компоненте које желите да инсталирате; обришите компоненте које не желите да инсталирате. Притисните Даље када будете спремни да наставите.
FullInstallation=Пуна инсталација
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Компактна инсталација
CustomInstallation=Инсталација само жељених компоненти
NoUninstallWarningTitle=Компоненте постоје
NoUninstallWarning=Инсталација је открила да следеће компоненте већ постоје на Вашем рачунару:%n%n%1%n%nНеодабирање ових компоненти их неће уклонити.%n%nДа ли ипак желите да наставите?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Тренутно одабране ставке захтевају најмање [mb] MB простора на диску.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Изаберите додатне задатке
SelectTasksDesc=Какве додатне задатке је још потребно обавити?
SelectTasksLabel2=Изаберите додатне задатке које желите да Инсталација [name] обави, а затим притисните Даље.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Изаберите директоријум за Старт мени
SelectStartMenuFolderDesc=Где желите да инсталација постави пречице за програм?
SelectStartMenuFolderLabel3=Инсталација ће поставити пречице за програм у следећем директоријуму Старт менија.
SelectStartMenuFolderBrowseLabel=Да наставите, притисните Даље. Ако желите да изаберете неки други директоријум, притисните Изабери.
MustEnterGroupName=Морате уписати назив директоријума.
GroupNameTooLong=Назив директоријума или путања су предугачки.
InvalidGroupName=Назив директоријума није исправан.
BadGroupName=Назив директоријума не сме имати било које од следећих слова:%n%n%1
NoProgramGroupCheck2=&Немој да правиш директоријум у Старт менију

; *** "Ready to Install" wizard page
WizardReady=Инсталација је спремна
ReadyLabel1=Инсталација је спремна да постави [name] на Ваш рачунар.
ReadyLabel2a=Притисните Инсталирај да наставите са инсталацијом, или притисните Назад ако желите да поново прегледате или промените нека подешавања.
ReadyLabel2b=Притисните Инсталирај да наставите са инсталацијом.
ReadyMemoUserInfo=Подаци о кориснику:
ReadyMemoDir=Одредишна локација:
ReadyMemoType=Тип инсталације:
ReadyMemoComponents=Изабране компоненте:
ReadyMemoGroup=Директоријум Старт менија:
ReadyMemoTasks=Додатни послови:

; *** "Preparing to Install" wizard page
WizardPreparing=Припрема за инсталацију
PreparingDesc=Инсталација се припрема да постави [name] на Ваш рачунар.
PreviousInstallNotCompleted=Инсталација/уклањање претходног програма није завршена. Потребно је да рестартујете Ваш рачунар да би се инсталација завршила.%n%nНакон рестартовања рачунара, покрените поново Инсталацију [name] да би сте је довршили.
CannotContinue=Инсталација се не може наставити. Молимо притисните Одустани да изађете.

; *** "Installing" wizard page
WizardInstalling=Инсталирање
InstallingLabel=Молимо сачекајте док се [name] инсталира на Ваш рачунар.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=[name] - завршетак инсталације
FinishedLabelNoIcons=Инсталација програма [name] на Ваш рачунар је завршена.
FinishedLabel=Инсталација програма [name] на Ваш рачунар је завршена. Програм можете покренути преко постављених икона.
ClickFinish=Притисните Завршетак да изађете.
FinishedRestartLabel=Да би се инсталација [name] довршила, мора се рестартовати рачунар. Да ли желите да га рестартујете одмах?
FinishedRestartMessage=Да би се инсталација [name] довршила, мора се рестартовати рачунар.%n%nДа ли желите да га рестартујете одмах?
ShowReadmeCheck=Да, желим да погледам README датотеку
YesRadio=&Да, рестартуј рачунар одмах
NoRadio=&Не, рестартоваћу рачунар касније
; used for example as 'Run MyProg.exe'
RunEntryExec=Покрени %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Погледај %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Инсталацији је потребан наредни диск
SelectDiskLabel2=Молимо ставите Диск %1 и притисните ОК.%n%nАко се датотеке на овом диску могу пронаћи у неком другом директоријуму него што је испод приказано, упишите одговарајућу путању или притисните Изабери.
PathLabel=&Путања:
FileNotInDir2=Датотека "%1" се не може пронаћи у "%2". Молимо ставите прави диск или изаберите други директоријум.
SelectDirectoryLabel=Молимо изаберите локацију наредног диска.

; *** Installation phase messages
SetupAborted=Инсталација није завршена.%n%nМолимо исправите проблем и покрените Инсталацију поново.
EntryAbortRetryIgnore=Притисните Retry да пробате поново, Ignore да наставите у сваком случају, или Abort да прекинете инсталацију.

; *** Installation status messages
StatusCreateDirs=Прављење директоријума...
StatusExtractFiles=Распакивање датотека...
StatusCreateIcons=Постављање пречица...
StatusCreateIniEntries=Постављање INI уписа...
StatusCreateRegistryEntries=Постављање Registry уписа...
StatusRegisterFiles=Пријављивање датотека...
StatusSavingUninstall=Бележење података за деинсталацију...
StatusRunProgram=Завршавање инсталације...
StatusRollback=Поништавање извршених измена и враћање на претходно стање...

; *** Misc. errors
ErrorInternal2=Интерна грешка: %1
ErrorFunctionFailedNoCode=%1 није успело
ErrorFunctionFailed=%1 није успело; код %2
ErrorFunctionFailedWithMessage=%1 није успело; код %2.%n%3
ErrorExecutingProgram=Није могуће покренути датотеку:%n%1

; *** Registry errors
ErrorRegOpenKey=Грешка при отварању Registry кључа:%n%1\%2
ErrorRegCreateKey=Грешка при постављању Registry кључа:%n%1\%2
ErrorRegWriteKey=Грешка при упису Registry кључа:%n%1\%2

; *** INI errors
ErrorIniEntry=Грешка при упису у INI датотеку "%1".

; *** File copying errors
FileAbortRetryIgnore=Притисните Retry да пробате поново, Ignore да прескочите ову датотеку (није препоручљиво), или Abort да прекинете инсталацију.
FileAbortRetryIgnore2=Притисните Retry да пробате поново, Ignore да наставите у сваком случају (није препоручљиво), или Abort да прекинете инсталацију.
SourceIsCorrupted=Изворна датотека је неисправна
SourceDoesntExist=Изворна датотека "%1" не постоји
ExistingFileReadOnly=Постојећа датотека је означена 'само за читање'.%n%nПритисните Retry да уклоните атрибут 'само за читање' и пробате поново, Ignore да прескочите ову датотеку, или Abort да прекинете инсталацију.
ErrorReadingExistingDest=Дошло је до грешке приликом покушаја читања следеће датотеке:
FileExists=Датотека већ постоји.%n%nДа ли желите да је Инсталација замени?
ExistingFileNewer=Постојећа датотека је новија од оне коју Инсталација треба да постави. Препоручује се да задржите постојећу датотеку.%n%nДа ли желите да сачувате постојећу датотеку?
ErrorChangingAttr=Дошло је до грешке приликом покушаја промене атрибута за следећу датотеку:
ErrorCreatingTemp=Дошло је до грешке приликом покушаја прављења датотеке у одредишном директоријуму:
ErrorReadingSource=Дошло је до грешке приликом покушаја читања изворне датотеке:
ErrorCopying=Дошло је до грешке приликом покушаја копирања датотеке:
ErrorReplacingExistingFile=Дошло је до грешке приликом покушаја замене постојеће датотеке:
ErrorRestartReplace=RestartReplace није успео:
ErrorRenamingTemp=Dошло је до грешке приликом покушаја промене назива датотеке у одредишном директоријуму:
ErrorRegisterServer=Није могуће регистровати DLL/OCX: %1
ErrorRegSvr32Failed=RegSvr32 није успешно извршен, грешка %1
ErrorRegisterTypeLib=Није успело регистровање 'type librаry': %1

; *** Post-installation errors
ErrorOpeningReadme=Дошло је до грешке приликом отварања README датотеке.
ErrorRestartingComputer=Инсталација није успела да рестартује рачунар. Молимо Вас урадите то сами.

; *** Uninstaller messages
UninstallNotFound=Датотека "%1" не постоји. Деинсталирање није успело.
UninstallOpenError=Датотека "%1" се не може отворити. Деинсталирање није успело
UninstallUnsupportedVer=Деинсталациона Log датотека "%1" је у облику који не препознаје ова верзија деинсталера. Деинсталирање није успело
UninstallUnknownEntry=Непознат упис (%1) се појавио у деинсталационој Log датотеци
ConfirmUninstall=Да ли сте сигурни да желите да потпуно уклоните %1 и све његове компоненте?
UninstallOnlyOnWin64=Овај програм се може деинсталирати само на 64-битном Windows-у.
OnlyAdminCanUninstall=Овај програм може деинсталирати само корисник са администраторским правима.
UninstallStatusLabel=Молимо сачекајте док %1 не буде уклоњен са Вашег рачунара.
UninstalledAll=%1 jе успешно уклоњен са Вашег рачунара.
UninstalledMost=%1 деинсталација је завршена.%n%nНеки делови се нису могли уклонити. Они се могу уклонити ручно.
UninstalledAndNeedsRestart=Да довршите деинсталацију %1, Ваш рачунар се мора рестартовати.%n%nДа ли желите да га рестартујете одмах?
UninstallDataCorrupted="%1" датотека је оштећена. Деинсталирање није успело

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Обриши заједничку датотеку?
ConfirmDeleteSharedFile2=Систем показује да следеће заједничке датотеке више не користи ни један програм. Да ли желите да деинсталација уклони ову заједничку датотеку?%n%nАко неки програми и даље користе ову датотеку, можда неће исправно функционисати. Ако нисте сигурни, изаберите No. Остављање ове датотеке неће проузроковати никакву штету систему.
SharedFileNameLabel=Назив датотеке:
SharedFileLocationLabel=Путања:
WizardUninstalling=Стање деинсталације
StatusUninstalling=Деинсталирање %1...

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 верзија %2
AdditionalIcons=Додатне иконе:
CreateDesktopIcon=Постави &Desktop икону
CreateQuickLaunchIcon=Постави &Quick Launch икону
ProgramOnTheWeb=%1 на Интернету
UninstallProgram=Деинсталација %1
LaunchProgram=Покрени %1
AssocFileExtension=&Придружи %1 са %2 типом датотеке
AssocingFileExtension=Придруживање %1 са %2 типом датотеке...
