; Macedonian translation is made for Inno Setup version 5.1.11+
; by Bojan Stosevski, M.D. Macedonia, bxxxn@hotmail.com
;
; To download user-contributed translations of this file, go to:
;   http://www.jrsoftware.org/is3rdparty.php
;
; Note: When translating this text, do not add periods (.) to the end of
; messages that didn't have them already, because on those messages Inno
; Setup adds the periods automatically (appending a period would result in
; two periods being displayed).

[LangOptions]
; The following three entries are very important. Be sure to read and 
; understand the '[LangOptions] section' topic in the help file.
LanguageName=Ma<043A>e<0434>o<043d>c<043A><0438>
LanguageID=$042F
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
InformationTitle=Информација
ConfirmTitle=Потврди
ErrorTitle=Грешка

; *** SetupLdr messages
SetupLdrStartupMessage=Програмот ќе го инсталира %1. Да ли сакате да продолжите?
LdrCannotCreateTemp=Неможност да се креира привремена датотека. Инсталацијата е прекината
LdrCannotExecTemp=Неможност да се изврши датотеката во привремениот директориум. Инсталацијата е прекината

; *** Startup error messages
LastErrorMessage=%1.%n%nГрешка %2: %3
SetupFileMissing=Датотеката %1 недостасува од инсталацискиот директориум. Ве молиме решете го проблемот или обезбедете нова копија од програмот.
SetupFileCorrupt=Инсталациските датотеки не се читливи. Ве молиме обезбедете нова копија од програмот.
SetupFileCorruptOrWrongVer=Инсталациските датотеки не се читливи, или се инкомпатибилни со оваа верзија на инсталацијата. Ве молиме решете го проблемот или обезбедете нова копија од програмот.
NotOnThisPlatform=Овој програм нема да работи на %1.
OnlyOnThisPlatform=Овој програм мора да работи на %1.
OnlyOnTheseArchitectures=Овој програм може да се инсталира само на верзии на Windows дизајнирани за следните процесорски архитектури:%n%n%1
MissingWOW64APIs=Верзијата на Windows која ја користите не вклучува функционалност побарувана од Инсталерот за да изврши 64-битна инсталација. За да го корегирате овој проблем, Ве молам инсталирајте Service Pack %1.
WinVersionTooLowError=Овој програм бара %1 верзија %2 или понова.
WinVersionTooHighError=Овој програм не може да се инсталира на %1 верзија %2 или понова.
AdminPrivilegesRequired=Мора да бидете пријавени како администратор на системот кога го инсталирате овој програм.
PowerUserPrivilegesRequired=Мора да бидете пријавени како администратор на системот или како член на Пауер Узер група (Power User Group)  кога го инсталирате овој програм.
SetupAppRunningError=Инсталацијата детектираше дека %1 се моментно активни.%n%nВе молиме затворете ги сите, тогаш притиснете Прифати за продолжување, или Откажи за крај.
UninstallAppRunningError=Инсталацијата детектираше дека %1 се моментно активни.%n%nВе молиме затворете ги сите, тогаш притиснете Прифати за продолжување, или Откажи за крај.

; *** Misc. errors
ErrorCreatingDir=Инсталацијата не можеше да ги креира директориумите "%1"
ErrorTooManyFilesInDir=Инсталацијата не можеше да ја креира датотеката во директориумите "%1" затоа што содржи премногу датотеки

; *** Setup common messages
ExitSetupTitle=Излези од инсталацијата
ExitSetupMessage=Инсталацијата не е комплетна. Ако прекинете сега, програмот нема да биде инсталиран.%n%nМожете да го стартувате инсталациониот програм друг пат и да ја комплетирате инсталацијата.%n%nИзлези од инсталацијата?
AboutSetupMenuItem=&За инсталацијата...
AboutSetupTitle=За инсталацијата
AboutSetupMessage=%1 верзија %2%n%3%n%n%1 домашна страна:%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< &Назад
ButtonNext=&Следна >
ButtonInstall=&Инсталирај
ButtonOK=Прифати
ButtonCancel=Откажи
ButtonYes=Да
ButtonYesToAll=Да за &сите
ButtonNo=Не
ButtonNoToAll=Н&е за сите
ButtonFinish=&Заврши
ButtonBrowse=&Барај...
ButtonWizardBrowse=Б&арај...
ButtonNewFolder=&Направи нов директориум

; *** "Select Language" dialog messages
SelectLanguageTitle=Одберете јазик за инсталација
SelectLanguageLabel=Одберете јазик кој ќе го користите за време на инсталацијата:

; *** Common wizard text
ClickNext=Одберете Следна за да продолжите, или Откажи за да излезете од инсталацијата.
BeveledLabel=
BrowseDialogTitle=Барај директориум
BrowseDialogLabel=Одберете директориум во листата, потоа притиснете Прифати.
NewFolderName=Нов директориум

; *** "Welcome" wizard page
WelcomeLabel1=Добредојдовте во [name] Инсталационен Маѓепсник
WelcomeLabel2=Со оваа инсталациона програма ќе го инсталирате [name/ver] на вашиот компјутер.%n%nПрепорачливо е да ги затворите сите програми пред да продолжите.

; *** "Password" wizard page
WizardPassword=Лозинка
PasswordLabel1=Инсталацијата е заштитена со лозинка.
PasswordLabel3=Ве молиме впишете ја лозинката, потоа притиснете Продолжи за понатака. Лозинката е осетлива на мали и големи букви.
PasswordEditLabel=&Лозинка:
IncorrectPassword=Лозинката што ја впишавте не е во ред. Ве молиме обидете се повторно.

; *** "License Agreement" wizard page
WizardLicense=Согласност со лиценцата
LicenseLabel=Ве молиме прочитајте је следната важна информација пред да продолжите.
LicenseLabel3=Ве молиме прочитајте је следната согласност со лиценцата. Мора да ги прифатите термините од договорот пред да продолжите со инсталацијата.
LicenseAccepted=Јас &го прифаќам договорот
LicenseNotAccepted=Јас &не го прифаќам договорот

; *** "Information" wizard pages
WizardInfoBefore=Информација
InfoBeforeLabel=Ве молиме прочитајте је следната важна информација пред да продолжите.
InfoBeforeClickLabel=Кога сте спремни да продолжите со инсталацијата, притиснете Следна.
WizardInfoAfter=Информација
InfoAfterLabel=Ве молиме прочитајте је следната важна информација пред да продолжите.
InfoAfterClickLabel=Кога сте спремни да продолжите со инсталацијата, притиснете Следна.

; *** "User Information" wizard page
WizardUserInfo=Податоци за корисникот
UserInfoDesc=Ве молиме внесете ги податоците за Вас.
UserInfoName=&Име на корисникот:
UserInfoOrg=&Организација:
UserInfoSerial=&Сериски број:
UserInfoNameRequired=Мора да внесете име.

; *** "Select Destination Location" wizard page
WizardSelectDir=Одберете локација како дестинација
SelectDirDesc=Каде треба [name] да се инсталира?
SelectDirLabel3=Инсталацијата ќе го инсталира [name] во следниот директориум.
SelectDirBrowseLabel=Да продолжите, притиснете Следна. Ако сакате да одберете различен директориум, притиснете Барај.
DiskSpaceMBLabel=Потребно е најмалку [mb] мегабајти слободен простор на дискот.
ToUNCPathname=Инсталационата програма не може да инсталира на зададената патека. Ако се обидувате да инсталирате на мрежа, ќе ви треба мапа на мрежниот диск.
InvalidPath=Мора да ја внесете целата патека заедно со буквата од дискот; на пример:%n%nC:\APP%n%не како на пр. UNC патека во форма:%n%n\\server\share
InvalidDrive=Дискот кој го одбравте не постои или не е достапен. Ве молиме внесете друг.
DiskSpaceWarningTitle=Нема доволно слободен простор на дискот
DiskSpaceWarning=На инсталационата програма и треба најмалку %1 килобајти слободен простор за инсталација, но одбраниот диск содржи само %2 килобајти достапно.%n%nДа ли сеедно сакате да продолжите?
DirNameTooLong=Името на директориумот или патеката е предолго.
InvalidDirName=Името на директориумот не е валидно.
BadDirName32=Името на директориумот не може да ги содржи следните карактери:%n%n%1
DirExistsTitle=Директориумот постои
DirExists=Директориумот:%n%n%1%n%nвеќе постои. Дали би сакале да инсталирате во тој директориум сеедно?
DirDoesntExistTitle=Директориумот не постои
DirDoesntExist=Директориумот:%n%n%1%n%nне постои. Дали би сакале директориумот да се креира?

; *** "Select Components" wizard page
WizardSelectComponents=Одбери компоненти
SelectComponentsDesc=Кои компоненти треба да се инсталираат?
SelectComponentsLabel2=Одбери кои компоненти треба да се инсталираат; избриши ги компонентите кои не сакаш да се инсталираат. Притисни Продолжи кога си спремен да продолжиш.
FullInstallation=Целосна инсталација
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Компактна инсталација
CustomInstallation=Инсталација по желба
NoUninstallWarningTitle=Компонентите постојат
NoUninstallWarning=Инсталационата програма забележа дека следните компоненти веќе постојат на Вашиот компјутер:%n%n%1%n%nДеселектитањето на истите нема да ги деинсталира.%n%nСакаш ли сеедно да продоложиш?
ComponentSize1=%1 килобајти
ComponentSize2=%1 мегабајти
ComponentsDiskSpaceMBLabel=Моменталната селекција побарува [mb] мегабајти дисков простор.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Select Additional Tasks
SelectTasksDesc=Кои дополнителни задачи треба да се извршат?
SelectTasksLabel2=Одберете ги дополнителните задачи кои инсталационата програма треба да ги изведе за време на инсталацијата [name], потоа притиснете Продолжи.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Одберете го директориумот за стартното мени
SelectStartMenuFolderDesc=Каде би требало да се сместат кратенките на програмот?
SelectStartMenuFolderLabel3=Инсталацијата ќе ги креира кратенките на програмот во следниот Старт мени директориум.
SelectStartMenuFolderBrowseLabel=Да продолжите, притиснете Следна. Ако сакате да одберете друг директориум, притиснете Барај.
MustEnterGroupName=Мора да внесете име на директориум.
GroupNameTooLong=Името на директориумот или патеката е предолго.
InvalidGroupName=Името на директориумот не е валидно.
BadGroupName=Името на директориумот не може да ги содржи следните карактери:%n%n%1
NoProgramGroupCheck2=&Не креирај директориум во стартно мени

; *** "Ready to Install" wizard page
WizardReady=Спремно за инсталација
ReadyLabel1=Инсталациониот програм е спремен да ја започне инсталацијата на [name] на Вашиот компјутер.
ReadyLabel2a=Притиснете Инсталирај за да продолжите со инсталацијата, или притиснете Назад за да направите измени.
ReadyLabel2b=Притиснете Инсталирај за да продолжите со инсталацијата.
ReadyMemoUserInfo=Информација за корисникот:
ReadyMemoDir=Дестинациска локација:
ReadyMemoType=Вид на инсталација:
ReadyMemoComponents=Одберени компоненти:
ReadyMemoGroup=Директориум за почетно мени:
ReadyMemoTasks=Дополнителни задачи:

; *** "Preparing to Install" wizard page
WizardPreparing=Спремно за инсталирање
PreparingDesc=Инсталациониот програм е спремен да го инсталита [name] на Вашиот компјутер.
PreviousInstallNotCompleted=Инсталацијата/деинсталацијата на претходниот програм не е довршена. Ќе треба да го рестартирате вашиот компјутер за да ја довршите инсталацијата.%n%nПо рестартирањето на компјутерот стартувајте ја инсталацијата уште еднаш за да го довршите инсталирањето на [name].
CannotContinue=Инсталацијата не може да продолжи. Ве молиме притиснете Откажи за да излезете.

; *** "Installing" wizard page
WizardInstalling=Инсталирање
InstallingLabel=Ве молиме почекајте додека инсталационата програма го инсталира [name] на Вашиот компјутер.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=Завршување со [name] Инсталациониот Маѓепсник
FinishedLabelNoIcons=Инсталационата програма заврши со инсталирање на [name] на Вашиот компјутер.
FinishedLabel=Инсталационата програма заврши со инсталирање на [name] на Вашиот компјутер. Оваа програма може да се стартува со нејзино одбирање.
ClickFinish=Притиснете Заврши да ја комплетирате инсталацијата.
FinishedRestartLabel=Да ја комплетира инсталацијата на [name], инсталацискиот програм мора да го рестартира вашиот компјутер. Дали би сакале да го рестартирате сега?
FinishedRestartMessage=Да ја комплетира инсталацијата на [name], инсталацискиот програм мора да го рестартира вашиот компјутер.%n%nДали би сакале да го рестартирате сега?
ShowReadmeCheck=Да, сакам да ја видам ПРОЧИТАЈМЕ датотеката
YesRadio=&Да, рестартирај го компјутерот сега
NoRadio=&Не, ќе го рестартирам компјутерот подоцна
; used for example as 'Run MyProg.exe'
RunEntryExec=Стартувај го %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Види го %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=На инсталациониот програм му е потребен следниот диск
SelectDiskLabel2=Ве молиме ставете го дискот под број %1 и притиснете Прифати.%n%nАко датотеките на овој диск не можат да бидат пронајдени во директориумот впишан долу, внесете ја точната патека и притиснете Барај.
PathLabel=&Path:
FileNotInDir2=Датотеката "%1" не може да се најде во "%2". Ве молиме ставете го вистинскиот диск или внесете ја вистинската патека.
SelectDirectoryLabel=Ве молиме впишете ја локацијата на следниот диск.

; *** Installation phase messages
SetupAborted=Инсталацијата не се комплетираше.%n%nВе молиме корегирајте го проблемот и стартувајте ја инсталацијата повторно.
EntryAbortRetryIgnore=Притиснете Повторно за да се обидете повторно, Игнорирајте за да продолжите без тој чекор, или притиснете Откажи за да ја прекинете инсталацијата.

; *** Installation status messages
StatusCreateDirs=Креирање на директориуми...
StatusExtractFiles=Екстракција на датотеки...
StatusCreateIcons=Креирање на кратенки...
StatusCreateIniEntries=Креирање на ИНИ влезови...
StatusCreateRegistryEntries=Креирање на регистарски влезови...
StatusRegisterFiles=Регистрирање на датотеки...
StatusSavingUninstall=Снимање на деинсталационата информација...
StatusRunProgram=Завршување со инсталацијата...
StatusRollback=Отповикување на промените...

; *** Misc. errors
ErrorInternal2=Внатрешна грешка: %1
ErrorFunctionFailedNoCode=%1 неуспешно
ErrorFunctionFailed=%1 неуспешно; код %2
ErrorFunctionFailedWithMessage=%1 неуспешно; код %2.%n%3
ErrorExecutingProgram=Неможност да се изврши датотеката:%n%1

; *** Registry errors
ErrorRegOpenKey=Грешка во отворањето на регистарскиот клуч:%n%1\%2
ErrorRegCreateKey=Грешка во креирањето на регистарскиот клуч:%n%1\%2
ErrorRegWriteKey=Грешка во пишувањето на регистарскиот клуч:%n%1\%2

; *** INI errors
ErrorIniEntry=Грешка во креирањето на ИНИ влез во датотеката "%1".

; *** File copying errors
FileAbortRetryIgnore=Притиснете Повторно за да се обидете повторно, Игнорирај за да ја скокнете оваа датотека (не препорачувам), или Аборт за да ја прекинете инсталацијата.
FileAbortRetryIgnore2=Притиснете Повторно за да се обидете повторно, Игнорирај за да продолжите понатака (не препорачувам), или Аборт за да ја прекинете инсталацијата.
SourceIsCorrupted=Изворната датотека е нечитлива
SourceDoesntExist=Изворната датотека "%1" не постои
ExistingFileReadOnly=Постоечката датотека е маркирана како read-only.%n%nПритиснете Повторно за да ги отстраните read-only атрибутите и да се обидете повторно, Игнорирај за да ја скокнете оваа датотека, или Прекини за да ја прекинете оваа инсталација.
ErrorReadingExistingDest=Се појави грешка при обид да се прочита постоечката датотека:
FileExists=Датотеката веќе постои.%n%nДали би сакале инсталационата програма да пишува преку неа?
ExistingFileNewer=Постоечката датотека е понова од датотеката која треба да се инсталира. Ви препорачуваме да ја задржите постоечката датотека.%n%nДа ли сакате да ја задржите постоечката датотека?
ErrorChangingAttr=Се појави грешка при обид да се сменат атрибутите на постоечката датотека:
ErrorCreatingTemp=Се појави грешка при обид да се креира датотека во дестинацискиот директориум:
ErrorReadingSource=Се појави грешка при обид да се прочита изворната датотека:
ErrorCopying=Се појави грешка при обид да се копира датотеката:
ErrorReplacingExistingFile=Се појави грешка при обид да се смени постоечката датотека:
ErrorRestartReplace=RestartReplace не успеа:
ErrorRenamingTemp=Се појави грешка при обид да се преименува датотека во дестинацискиот директориум:
ErrorRegisterServer=Неможност за регистрирање на DLL/OCX: %1
ErrorRegSvr32Failed=RegSvr32 потфрли со излезен код %1
ErrorRegisterTypeLib=Неможност да се регистрира типот на библиотеката: %1

; *** Post-installation errors
ErrorOpeningReadme=Се појави грешка при обид за отварање на ПРОЧИТАЈМЕ датотеката.
ErrorRestartingComputer=Инсталационата програма не можеше да го рестартира компјутерот. Ве молиме направете го тоа рачно.

; *** Uninstaller messages
UninstallNotFound=Датотеката "%1" не постои. Неможам да деинсталирам.
UninstallOpenError=Датотеката "%1" не може да се отвори. Неможам да деинсталирам
UninstallUnsupportedVer=Деинсталирачката лог датотека "%1" е во формат што е непрепознаен од оваа верзија на деинсталер. Неможам да деинсталирам
UninstallUnknownEntry=Непознат влез (%1) е забележан во деинсталирачкиот лог
ConfirmUninstall=Дали сте сигурни дека сакате комплетно да го отстраните %1 и сите негови компоненти?
UninstallOnlyOnWin64=Оваа инсталација може да биде деинсталирана само на 64-битен Windows.
OnlyAdminCanUninstall=Оваа инсталација може да биде отстранета само од корисник со административни привилегии.
UninstallStatusLabel=Ве молиме почекајте додека %1 се отстранува од Вашиот компјутер.
UninstalledAll=%1 успешно е отстранет од Вашиот компјутер.
UninstalledMost=%1 деинсталацијата е комплетирана.%n%nНекои елементи не можеа да бидат отстранети. Ќе мора да ги отстраните рачно.
UninstalledAndNeedsRestart=Да го комплетирате деинсталирањето на %1, Вашиот компјутер мора да се рестартира.%n%nСакате ли да го рестартирате сега?
UninstallDataCorrupted="%1" датотеката е нечитлива. Неможам да деинсталирам

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Отстрани ги датотеките кои се заеднички?
ConfirmDeleteSharedFile2=Системот оцени дека следните заеднички датотеки не се користат од никој корисник. Дали би сакале да ги отстраните?%n%nАко некои програми ги користат овие датотеки и тие се отстранат, тие програми би можело да не функционираат како што треба. Ако сте несигурни, одберете не. Оставање на овие датотеки на системот нема да му наштети.
SharedFileNameLabel=Име на датотека:
SharedFileLocationLabel=Локација:
WizardUninstalling=Статус на деинсталирање
StatusUninstalling=Деинсталирање %1...

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 верзија %2
AdditionalIcons=Додатни икони:
CreateDesktopIcon=Креирај &десктоп икона
CreateQuickLaunchIcon=Креирај &брз старт икона
ProgramOnTheWeb=%1 на интернет
UninstallProgram=Деинсталирај го %1
LaunchProgram=Стартувај %1
AssocFileExtension=&Асоцирај го %1 со %2 со екстензиите
AssocingFileExtension=Асоцирај ги %1 со %2 екстензиите...
