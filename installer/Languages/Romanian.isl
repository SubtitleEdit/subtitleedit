; *** Inno Setup version 5.5.3+ Romanian messages ***
; Translator : Alexandru Bogdan Munteanu (muntealb@gmail.com)
;
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
LanguageName=Rom<00E2>n<0103>
LanguageID=$0418
LanguageCodePage=1250
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
SetupAppTitle=Instalare
SetupWindowTitle=Instalare - %1
UninstallAppTitle=Dezinstalare
UninstallAppFullTitle=Dezinstalare %1

; *** Misc. common
InformationTitle=Informaþii
ConfirmTitle=Confirmare
ErrorTitle=Eroare

; *** SetupLdr messages
SetupLdrStartupMessage=Va fi instalat programul %1. Vrei sã continui?
LdrCannotCreateTemp=Nu pot crea o filã temporarã. Instalare abandonatã
LdrCannotExecTemp=Nu pot executa o filã din dosarul temporar. Instalare abandonatã

; *** Startup error messages
LastErrorMessage=%1.%n%nEroarea %2: %3
SetupFileMissing=Fila %1 lipseºte din dosarul de instalare. Corecteazã problema sau foloseºte o altã copie a programului.
SetupFileCorrupt=Filele de instalare sînt stricate (corupte). Foloseºte o altã copie a programului.
SetupFileCorruptOrWrongVer=Filele de instalare sînt stricate (corupte) sau sînt incompatibile cu aceastã versiune a Instalatorului. Remediazã problema sau foloseºte o altã copie a programului.
InvalidParameter=Un parametru invalid a fost trecut cãtre linia de comandã:%n%n%1
SetupAlreadyRunning=Instalarea ruleazã deja.
WindowsVersionNotSupported=Acest program nu suportã versiunea de Windows care ruleazã pe calculatorul tãu.
WindowsServicePackRequired=Acest program necesitã %1 Service Pack %2 sau mai nou.
NotOnThisPlatform=Acest program nu va rula pe %1.
OnlyOnThisPlatform=Acest program trebuie sã ruleze pe %1.
OnlyOnTheseArchitectures=Acest program poate fi instalat doar pe versiuni de Windows proiectate pentru urmãtoarele arhitecturi de procesor:%n%n%1
MissingWOW64APIs=Versiunea de Windows pe care o rulezi nu include funcþionalitatea cerutã de Instalator pentru a realiza o instalare pe 64-biþi. Pentru a corecta problema, va trebui sã instalezi Service Pack %1.
WinVersionTooLowError=Acest program necesitã %1 versiunea %2 sau mai nouã.
WinVersionTooHighError=Acest program nu poate fi instalat pe %1 versiunea %2 sau mai nouã.
AdminPrivilegesRequired=Trebuie sã fii logat ca Administrator pentru instalarea acestui program.
PowerUserPrivilegesRequired=Trebuie sã fii logat ca Administrator sau ca Membru al Grupului de Utilizatori Pricepuþi ("Power Users") pentru a instala acest program.
SetupAppRunningError=Instalatorul a detectat cã %1 ruleazã în acest moment.%n%nÎnchide toate instanþele programului respectiv, apoi clicheazã OK pentru a continua sau Anuleazã pentru a abandona instalarea.
UninstallAppRunningError=Dezinstalatorul a detectat cã %1 ruleazã în acest moment.%n%nÎnchide toate instanþele programului respectiv, apoi clicheazã OK pentru a continua sau Anuleazã pentru a abandona dezinstalarea.

; *** Misc. errors
ErrorCreatingDir=Instalatorul nu a putut crea dosarul "%1"
ErrorTooManyFilesInDir=Nu pot crea o filã în dosarul "%1" din cauzã cã are deja prea multe file

; *** Setup common messages
ExitSetupTitle=Abandonarea Instalãrii
ExitSetupMessage=Instalarea nu este terminatã. Dacã o abandonezi acum, programul nu va fi instalat.%n%nPoþi sã rulezi Instalatorul din nou altã datã pentru a termina instalarea.%n%nAbandonezi Instalarea?
AboutSetupMenuItem=&Despre Instalator...
AboutSetupTitle=Despre Instalator
AboutSetupMessage=%1 versiunea %2%n%3%n%n%1 sit:%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< Îna&poi
ButtonNext=&Continuã >
ButtonInstall=&Instaleazã
ButtonOK=OK
ButtonCancel=Anuleazã
ButtonYes=&Da
ButtonYesToAll=Da la &Tot
ButtonNo=&Nu
ButtonNoToAll=N&u la Tot
ButtonFinish=&Finalizeazã
ButtonBrowse=&Exploreazã...
ButtonWizardBrowse=Explo&reazã...
ButtonNewFolder=Creea&zã Dosar Nou

; *** "Select Language" dialog messages
SelectLanguageTitle=Selectarea Limbii Instalatorului
SelectLanguageLabel=Selecteazã limba folositã pentru instalare:

; *** Common wizard text
ClickNext=Clicheazã pe Continuã pentru a avansa cu instalarea sau pe Anuleazã pentru a o abandona.
BeveledLabel=
BrowseDialogTitle=Explorare dupã Dosar
BrowseDialogLabel=Selecteazã un dosar din lista de mai jos, apoi clicheazã pe OK.
NewFolderName=Dosar Nou

; *** "Welcome" wizard page
WelcomeLabel1=Bun venit la Instalarea [name]
WelcomeLabel2=Programul [name/ver] va fi instalat pe calculator.%n%nEste recomandat sã închizi toate celelalte aplicaþii înainte de a continua.

; *** "Password" wizard page
WizardPassword=Parolã
PasswordLabel1=Aceastã instalare este protejatã prin parolã.
PasswordLabel3=Completeazã parola, apoi clicheazã pe Continuã pentru a merge mai departe. Tipul literelor din parolã (Majuscule/minuscule) este luat în considerare.
PasswordEditLabel=&Parolã:
IncorrectPassword=Parola pe care ai introdus-o nu este corectã. Reîncearcã.

; *** "License Agreement" wizard page
WizardLicense=Acord de Licenþiere
LicenseLabel=Citeºte informaþiile urmãtoare înainte de a continua, sînt importante.
LicenseLabel3=Citeºte urmãtorul Acord de Licenþiere. Trebuie sã accepþi termenii acestui acord înainte de a continua instalarea.
LicenseAccepted=&Accept licenþa
LicenseNotAccepted=&Nu accept licenþa

; *** "Information" wizard pages
WizardInfoBefore=Informaþii
InfoBeforeLabel=Citeºte informaþiile urmãtoare înainte de a continua, sînt importante.
InfoBeforeClickLabel=Cînd eºti gata de a trece la Instalare, clicheazã pe Continuã.
WizardInfoAfter=Informaþii
InfoAfterLabel=Citeºte informaþiile urmãtoare înainte de a continua, sînt importante.
InfoAfterClickLabel=Cînd eºti gata de a trece la Instalare, clicheazã pe Continuã.

; *** "User Information" wizard page
WizardUserInfo=Informaþii despre Utilizator
UserInfoDesc=Completeazã informaþiile cerute.
UserInfoName=&Utilizator:
UserInfoOrg=&Organizaþie:
UserInfoSerial=Numãr de &Serie:
UserInfoNameRequired=Trebuie sã introduci un nume.

; *** "Select Destination Location" wizard page
WizardSelectDir=Selectarea Locului de Destinaþie
SelectDirDesc=Unde vrei sã instalezi [name]?
SelectDirLabel3=Instalatorul va pune [name] în dosarul specificat mai jos.
SelectDirBrowseLabel=Pentru a avansa cu instalarea, clicheazã pe Continuã. Dacã vrei sã selectezi un alt dosar, clicheazã pe Exploreazã.
DiskSpaceMBLabel=Este necesar un spaþiu liber de stocare de cel puþin [mb] MB.
CannotInstallToNetworkDrive=Instalatorul nu poate realiza instalarea pe un dispozitiv de reþea.
CannotInstallToUNCPath=Instalatorul nu poate realiza instalarea pe o cale în format UNC.
InvalidPath=Trebuie sã introduci o cale completã, inclusiv litera dispozitivului; de exemplu:%n%nC:\APP%n%nsau o cale UNC de forma:%n%n\\server\share
InvalidDrive=Dispozitivul sau partajul UNC pe care l-ai selectat nu existã sau nu este accesibil. Selecteazã altul.
DiskSpaceWarningTitle=Spaþiu de Stocare Insuficient
DiskSpaceWarning=Instalarea necesitã cel puþin %1 KB de spaþiu de stocare liber, dar dispozitivul selectat are doar %2 KB liberi.%n%nVrei sã continui oricum?
DirNameTooLong=Numele dosarului sau al cãii este prea lung.
InvalidDirName=Numele dosarului nu este valid.
BadDirName32=Numele dosarelor nu pot include unul din urmãtoarele caractere:%n%n%1
DirExistsTitle=Dosarul Existã
DirExists=Dosarul:%n%n%1%n%nexistã deja. Vrei totuºi sã instalezi în acel dosar?
DirDoesntExistTitle=Dosarul Nu Existã
DirDoesntExist=Dosarul:%n%n%1%n%nnu existã. Vrei ca el sã fie creat?

; *** "Select Components" wizard page
WizardSelectComponents=Selectarea Componentelor
SelectComponentsDesc=Care dintre componente trebuie instalate?
SelectComponentsLabel2=Selecteazã componentele de instalat; deselecteazã componentele care nu trebuie instalate. Clicheazã pe Continuã pentru a merge mai departe.
FullInstallation=Instalare Completã
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Instalare Compactã
CustomInstallation=Instalare Personalizatã
NoUninstallWarningTitle=Componentele Existã
NoUninstallWarning=Instalatorul a detectat cã urmãtoarele componente sînt deja instalate pe calculator:%n%n%1%n%nDeselectarea lor nu le va dezinstala.%n%nVrei sã continui oricum?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Selecþia curentã necesitã cel puþin [mb] MB spaþiu de stocare.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Selectarea Sarcinilor Suplimentare
SelectTasksDesc=Ce sarcini suplimentare trebuie îndeplinite?
SelectTasksLabel2=Selecteazã sarcinile suplimentare care trebuie îndeplinite în timpul instalãrii [name], apoi clicheazã pe Continuã.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Selectarea Dosarului din Meniul de Start
SelectStartMenuFolderDesc=Unde trebuie sã fie plasate scurtãturile programului?
SelectStartMenuFolderLabel3=Scurtãturile vor fi plasate în dosarul specificat mai jos al Meniului de Start.
SelectStartMenuFolderBrowseLabel=Pentru a avansa cu instalarea, clicheazã pe Continuã. Dacã vrei sã selectezi un alt dosar, clicheazã pe Exploreazã.
MustEnterGroupName=Trebuie sã introduci numele dosarului.
GroupNameTooLong=Numele dosarului sau al cãii este prea lung.
InvalidGroupName=Numele dosarului nu este valid.
BadGroupName=Numele dosarului nu poate include unul dintre caracterele urmãtoarele:%n%n%1
NoProgramGroupCheck2=Nu crea un &dosar în Meniul de Start

; *** "Ready to Install" wizard page
WizardReady=Pregãtit de Instalare
ReadyLabel1=Instalatorul e pregãtit pentru instalarea [name] pe calculator.
ReadyLabel2a=Clicheazã pe Instaleazã pentru a continua cu instalarea, sau clicheazã pe Înapoi dacã vrei sã revezi sau sã schimbi setãrile.
ReadyLabel2b=Clicheazã pe Instaleazã pentru a continua cu instalarea.
ReadyMemoUserInfo=Info Utilizator:
ReadyMemoDir=Loc de Destinaþie:
ReadyMemoType=Tip de Instalare:
ReadyMemoComponents=Componente Selectate:
ReadyMemoGroup=Dosarul Meniului de Start:
ReadyMemoTasks=Sarcini Suplimentare:

; *** "Preparing to Install" wizard page
WizardPreparing=Pregãtire pentru Instalare
PreparingDesc=Instalatorul pregãteºte instalarea [name] pe calculator.
PreviousInstallNotCompleted=Instalarea/dezinstalarea anterioarã a unui program nu a fost terminatã. Va trebui sã reporneºti calculatorul pentru a termina operaþia precedentã.%n%nDupã repornirea calculatorului, ruleazã Instalatorul din nou pentru a realiza instalarea [name].
CannotContinue=Instalarea nu poate continua. Clicheazã pe Anuleazã pentru a o închide.
ApplicationsFound=Aplicaþiile urmãtoare folosesc file care trebuie actualizate de cãtre Instalator. Este recomandat sã permiþi Instalatorului sã închidã automat aplicaþiile respective.
ApplicationsFound2=Aplicaþiile urmãtoare folosesc file care trebuie actualizate de cãtre Instalator. Este recomandat sã permiþi Instalatorului sã închidã automat aplicaþiile respective. Dupã ce instalarea e terminatã, Instalatorul va încerca sã reporneascã aplicaþiile.
CloseApplications=Închide &automat aplicaþiile
DontCloseApplications=Nu închi&de aplicaþiile
ErrorCloseApplications=Instalatorul nu a putut închide automat toate aplicaþiile. Înainte de a continua, e recomandat sã închizi manual toate aplicaþiile care folosesc file ce trebuie actualizate de Instalator.

; *** "Installing" wizard page
WizardInstalling=Instalare în Desfãºurare
InstallingLabel=Aºteaptã sã se termine instalarea [name] pe calculator.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=Finalizarea Instalãrii [name]
FinishedLabelNoIcons=Instalarea [name] pe calculator a fost terminatã.
FinishedLabel=Instalarea [name] pe calculator a fost terminatã. Aplicaþia poate fi lansatã prin clicarea pe icoanele instalate.
ClickFinish=Clicheazã pe Finalizeazã pentru a pãrãsi Instalatorul.
FinishedRestartLabel=Pentru a termina instalarea [name], trebuie repornit calculatorul. Vrei sã fie repornit acum?
FinishedRestartMessage=Pentru a termina instalarea [name], trebuie repornit calculatorul.%n%nVrei sã fie repornit acum?
ShowReadmeCheck=Da, vreau sã vãd fila de informare (README)
YesRadio=&Da, reporneºte calculatorul acum
NoRadio=&Nu, voi reporni eu calculatorul mai tîrziu
; used for example as 'Run MyProg.exe'
RunEntryExec=Ruleazã %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Vezi %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Instalatorul Necesitã Discul Urmãtor
SelectDiskLabel2=Introdu Discul %1 ºi clicheazã pe OK.%n%nDacã filele de pe acest disc pot fi gãsite într-un alt dosar decît cel afiºat mai jos, introdu calea corectã sau clicheazã pe Exploreazã.
PathLabel=&Cale:
FileNotInDir2=Fila "%1" nu poate fi gãsitã în "%2". Introdu discul corect sau selecteazã alt dosar.
SelectDirectoryLabel=Specificã locul discului urmãtor.

; *** Installation phase messages
SetupAborted=Instalarea nu a fost terminatã.%n%nCorecteazã problema ºi apoi ruleazã Instalarea din nou.
EntryAbortRetryIgnore=Clicheazã pe Reîncearcã pentru a încerca din nou, pe Ignorã pentru a continua oricum, sau pe Abandoneazã pentru a anula instalarea.

; *** Installation status messages
StatusClosingApplications=Închid aplicaþiile...
StatusCreateDirs=Creez dosarele...
StatusExtractFiles=Extrag filele...
StatusCreateIcons=Creez scurtãturile...
StatusCreateIniEntries=Creez intrãrile INI...
StatusCreateRegistryEntries=Creez intrãrile în registru...
StatusRegisterFiles=Înregistrez filele...
StatusSavingUninstall=Salvez informaþiile de dezinstalare...
StatusRunProgram=Finalizez instalarea...
StatusRestartingApplications=Repornesc aplicaþiile...
StatusRollback=Reîntorc la starea iniþialã, prin anularea modificãrilor fãcute...

; *** Misc. errors
ErrorInternal2=Eroare Internã: %1
ErrorFunctionFailedNoCode=%1 a eºuat
ErrorFunctionFailed=%1 a eºuat; cod %2
ErrorFunctionFailedWithMessage=%1 a eºuat; cod %2.%n%3
ErrorExecutingProgram=Nu pot executa fila:%n%1

; *** Registry errors
ErrorRegOpenKey=Eroare la deschiderea cheii de registru:%n%1\%2
ErrorRegCreateKey=Eroare la crearea cheii de registru:%n%1\%2
ErrorRegWriteKey=Eroare la scrierea în cheia de registru:%n%1\%2

; *** INI errors
ErrorIniEntry=Eroare la crearea intrãrii INI în fiºierul "%1".

; *** File copying errors
FileAbortRetryIgnore=Clicheazã pe Reîncearcã pentru a încerca din nou, pe Ignorã pentru a sãri aceastã filã (nerecomandat), sau pe Abandoneazã pentru a anula instalarea.
FileAbortRetryIgnore2=Clicheazã pe Reîncearcã pentru a încerca din nou, pe Ignorã pentru a continua oricum (nerecomandat), sau pe Abandoneazã pentru a anula instalarea.
SourceIsCorrupted=Fila sursã este stricatã (coruptã)
SourceDoesntExist=Fila sursã "%1" nu existã
ExistingFileReadOnly=Fila deja existentã este marcatã doar-citire.%n%nClicheazã pe Reîncearcã pentru a înlãtura atributul doar-citire ºi a încerca din nou, pe Ignorã pentru a sãri aceastã filã, sau pe Abandoneazã pentru a anula instalarea.
ErrorReadingExistingDest=A apãrut o eroare în timpul citirii filei deja existente:
FileExists=Fila existã deja.%n%Vrei ca ea sã fie suprascrisã de Instalator?
ExistingFileNewer=Fila deja existentã este mai nouã decît cea care trebuie instalatã. Este recomandat s-o pãstrezi pe cea existentã.%n%nVrei sã pãstrezi fila deja existentã?
ErrorChangingAttr=A apãrut o eroare în timpul schimbãrii atributelor filei deja existente:
ErrorCreatingTemp=A apãrut o eroare în timpul creãrii filei în dosarul de destinaþie:
ErrorReadingSource=A apãrut o eroare în timpul citirii filei sursã:
ErrorCopying=A apãrut o eroare în timpul copierii filei:
ErrorReplacingExistingFile=A apãrut o eroare în timpul înlocuirii filei deja existente:
ErrorRestartReplace=Repornirea/Înlocuirea a eºuat:
ErrorRenamingTemp=A apãrut o eroare în timpul renumirii unei file din dosarul de destinaþie:
ErrorRegisterServer=Nu pot înregistra DLL/OCX: %1
ErrorRegSvr32Failed=RegSvr32 a eºuat, avînd codul de ieºire %1
ErrorRegisterTypeLib=Nu pot înregistra biblioteca de tipuri: %1

; *** Post-installation errors
ErrorOpeningReadme=A apãrut o eroare la deschiderea filei de informare (README).
ErrorRestartingComputer=Instalatorul nu a putut reporni calculatorul. Va trebui sã-l reporneºti manual.

; *** Uninstaller messages
UninstallNotFound=Fila "%1" nu existã. Dezinstalarea nu poate fi fãcutã.
UninstallOpenError=Fila "%1" nu poate fi deschisã. Dezinstalarea nu poate fi fãcutã
UninstallUnsupportedVer=Fila "%1" ce conþine jurnalul de dezinstalare este într-un format nerecunoscut de aceastã versiune a dezinstalatorului. Dezinstalarea nu poate fi fãcutã
UninstallUnknownEntry=A fost întîlnitã o intrare necunoscutã (%1) în jurnalul de dezinstalare
ConfirmUninstall=Sigur vrei sã înlãturi complet %1 ºi componentele sale?
UninstallOnlyOnWin64=Aceastã instalare poate fi dezinstalatã doar pe un sistem Windows 64-biþi.
OnlyAdminCanUninstall=Aceastã instalare poate fi dezinstalatã doar de cãtre un utilizator cu drepturi de Administrator.
UninstallStatusLabel=Aºteaptã ca %1 sã fie înlãturat de pe calculator.
UninstalledAll=%1 a fost înlãturat cu succes de pe calculator.
UninstalledMost=Dezinstalare completã a %1.%n%nAnumite elemente nu au putut fi înlãturate. Acestea pot fi înlãturate manual.
UninstalledAndNeedsRestart=Pentru a termina dezinstalarea %1, calculatorul trebuie repornit.%n%nVrei sã fie repornit acum?
UninstallDataCorrupted=Fila "%1" este stricatã (coruptã). Dezinstalarea nu poate fi fãcutã

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=ªterg Fila Partajatã?
ConfirmDeleteSharedFile2=Sistemul indicã faptul cã fila partajatã urmãtoare pare sã nu mai fie folositã de vreun alt program. Vrei ca Dezinstalatorul sã ºteargã aceastã filã partajatã?%n%nDacã totuºi mai existã programe care folosesc fila ºi ea este ºtearsã, acele programe ar putea sã funcþioneze greºit. Dacã nu eºti sigur, alege Nu. Lãsarea filei în sistem nu va produce nici o neplãcere.
SharedFileNameLabel=Nume Filã:
SharedFileLocationLabel=Loc:
WizardUninstalling=Starea Dezinstalãrii
StatusUninstalling=Dezinstalez %1...

; *** Shutdown block reasons
ShutdownBlockReasonInstallingApp=Instalez %1.
ShutdownBlockReasonUninstallingApp=Dezinstalez %1.

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 versiunea %2
AdditionalIcons=Icoane suplimentare:
CreateDesktopIcon=Creeazã o icoanã pe &Birou ("Desktop")
CreateQuickLaunchIcon=Creeazã o icoanã în Bara de &Lansare Rapidã ("Quick Launch")
ProgramOnTheWeb=%1 pe internet
UninstallProgram=Dezinstaleazã %1
LaunchProgram=Lanseazã %1
AssocFileExtension=&Asociazã %1 cu extensia de file %2
AssocingFileExtension=Asociez %1 cu extensia de file %2...
AutoStartProgramGroupDescription=Pornire:
AutoStartProgram=Porneºte automat %1
AddonHostProgramNotFound=%1 nu poate fi gãsit în dosarul selectat.%n%nVrei sã continui oricum?
