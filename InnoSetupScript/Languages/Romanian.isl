; *** Inno Setup version 5.1.11+ Romanian messages ***
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
LanguageName=Român<0103>
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
SetupLdrStartupMessage=Va fi instalat programul %1. Doriþi sã continuaþi?
LdrCannotCreateTemp=Nu se poate crea un fiºier temporar. Instalare abandonatã
LdrCannotExecTemp=Nu se poate executa un fiºier din dosarul temporar. Instalare abandonatã

; *** Startup error messages
LastErrorMessage=%1.%n%nEroarea %2: %3
SetupFileMissing=Fiºierul %1 lipseºte din dosarul de instalare. Corectaþi problema sau faceþi rost de o copie nouã a programului.
SetupFileCorrupt=Fiºierele de instalare sînt deteriorate. Faceþi rost de o copie nouã a programului.
SetupFileCorruptOrWrongVer=Fiºierele de instalare sînt deteriorate sau sînt incompatibile cu aceastã versiune a Instalatorului. Remediaþi problema sau obþineþi o copie nouã a programului.
NotOnThisPlatform=Acest program nu va rula pe %1.
OnlyOnThisPlatform=Acest program trebuie sã ruleze pe %1.
OnlyOnTheseArchitectures=Acest program poate fi instalat doar pe versiuni de Windows proiectate pentru urmãtoarele arhitecturi de procesor:%n%n%1
MissingWOW64APIs=Versiunea de Windows pe care o rulaþi nu include funcþionalitatea cerutã de Instalator pentru a realiza o instalare pe 64-biþi. Pentru a corecta problema, va trebui sã instalaþi Service Pack %1.
WinVersionTooLowError=Acest program necesitã %1 versiunea %2 sau mai nouã.
WinVersionTooHighError=Acest program nu poate fi instalat pe %1 versiunea %2 sau mai nouã.
AdminPrivilegesRequired=Trebuie sã fiþi logat ca Administrator pentru instalarea acestui program.
PowerUserPrivilegesRequired=Trebuie sã fiþi logat ca Administrator sau ca Membru al Grupului de Utilizatori Împuterniciþi pentru a instala acest program.
SetupAppRunningError=Instalatorul a detectat cã %1 ruleazã în acest moment.%n%nÎnchideþi toate instanþele programului respectiv, apoi clicaþi OK pentru a continua sau Anuleazã pentru a abandona instalarea.
UninstallAppRunningError=Dezinstalatorul a detectat cã %1 ruleazã în acest moment.%n%nÎnchideþi toate instanþele programului respectiv, apoi clicaþi OK pentru a continua sau Anuleazã pentru a abandona dezinstalarea.

; *** Misc. errors
ErrorCreatingDir=Instalatorul nu a putut crea dosarul "%1"
ErrorTooManyFilesInDir=Nu se poate crea un fiºier în dosarul "%1" din cauzã cã are deja prea multe fiºiere

; *** Setup common messages
ExitSetupTitle=Abandonarea Instalãrii
ExitSetupMessage=Instalarea nu este terminatã. Dacã o abandonaþi acum, programul nu va fi instalat.%n%nPuteþi sã rulaþi Instalatorul din nou altã datã pentru a termina instalarea.%n%nAbandonaþi Instalarea?
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
ButtonFinish=Închide
ButtonBrowse=&Exploreazã...
ButtonWizardBrowse=Explo&reazã...
ButtonNewFolder=Creea&zã Dosar Nou

; *** "Select Language" dialog messages
SelectLanguageTitle=Selectarea Limbii Instalatorului
SelectLanguageLabel=Selectaþi limba folositã pentru instalare:

; *** Common wizard text
ClickNext=Clicaþi Continuã pentru a avansa cu instalarea sau Anuleazã pentru a o abandona.
BeveledLabel=
BrowseDialogTitle=Explorare dupã Dosar
BrowseDialogLabel=Selectaþi un dosar din lista de mai jos, apoi clicaþi OK.
NewFolderName=Dosar Nou

; *** "Welcome" wizard page
WelcomeLabel1=Bun venit la Instalarea [name]
WelcomeLabel2=Programul [name/ver] va fi instalat pe calculator.%n%nEste recomandat sã închideþi toate celelalte aplicaþii înainte de a continua.

; *** "Password" wizard page
WizardPassword=Parolã
PasswordLabel1=Aceastã instalare este protejatã prin parolã.
PasswordLabel3=Completaþi parola, apoi clicaþi Continuã pentru a merge mai departe. Se ia în considerare tipul literelor din parolã (Majuscule/minuscule).
PasswordEditLabel=&Parolã:
IncorrectPassword=Parola pe care aþi introdus-o nu este corectã. Reîncercaþi.

; *** "License Agreement" wizard page
WizardLicense=Acord de Licenþiere
LicenseLabel=Citiþi informaþiile urmãtoare înainte de a continua, sînt importante.
LicenseLabel3=Citiþi urmãtorul Acord de Licenþiere. Trebuie sã acceptaþi termenii acestui acord înainte de a continua instalarea.
LicenseAccepted=&Accept licenþa
LicenseNotAccepted=&Nu accept licenþa

; *** "Information" wizard pages
WizardInfoBefore=Informaþii
InfoBeforeLabel=Citiþi informaþiile urmãtoare înainte de a continua, sînt importante.
InfoBeforeClickLabel=Cînd sînteþi gata de a trece la Instalare, clicaþi Continuã.
WizardInfoAfter=Informaþii
InfoAfterLabel=Citiþi informaþiile urmãtoare înainte de a continua, sînt importante.
InfoAfterClickLabel=Cînd sînteþi gata de a trece la Instalare, clicaþi Continuã.

; *** "User Information" wizard page
WizardUserInfo=Informaþii despre Utilizator
UserInfoDesc=Introduceþi informaþiile solicitate.
UserInfoName=&Utilizator:
UserInfoOrg=&Organizaþie:
UserInfoSerial=Numãr de &Serie:
UserInfoNameRequired=Trebuie sã introduceþi un nume.

; *** "Select Destination Location" wizard page
WizardSelectDir=Selectarea Locului de Destinaþie
SelectDirDesc=Unde doriþi sã instalaþi [name]?
SelectDirLabel3=Instalatorul va pune [name] în dosarul specificat mai jos.
SelectDirBrowseLabel=Pentru a avansa cu instalarea, clicaþi Continuã. Dacã doriþi sã selectaþi un alt dosar, clicaþi Exploreazã.
DiskSpaceMBLabel=Este necesar un spaþiu liber de stocare de cel puþin [mb] MB.
ToUNCPathname=Instalatorul nu poate realiza instalarea pe o cale în format UNC. Dacã încercaþi sã instalaþi într-o reþea, va trebui sã mapaþi un dispozitiv de reþea.
InvalidPath=Trebuie sã introduceþi o cale completã, inclusiv litera dispozitivului; de exemplu:%n%nC:\APP%n%nsau o cale UNC de forma:%n%n\\server\share
InvalidDrive=Dispozitivul sau partajul UNC pe care l-aþi selectat nu existã sau nu este accesibil. Selectaþi altul.
DiskSpaceWarningTitle=Spaþiu de Stocare Insuficient
DiskSpaceWarning=Instalarea necesitã cel puþin %1 KB de spaþiu de stocare liber, dar dispozitivul selectat are doar %2 KB liberi.%n%nDoriþi sã continuaþi oricum?
DirNameTooLong=Numele dosarului sau al cãii este prea lung.
InvalidDirName=Numele dosarului nu este valid.
BadDirName32=Numele dosarelor nu pot include unul din urmãtoarele caractere:%n%n%1
DirExistsTitle=Dosarul Existã
DirExists=Dosarul:%n%n%1%n%nexistã deja. Doriþi totuºi sã instalaþi în acel dosar?
DirDoesntExistTitle=Dosarul Nu Existã
DirDoesntExist=Dosarul:%n%n%1%n%nnu existã. Doriþi ca el sã fie creat?

; *** "Select Components" wizard page
WizardSelectComponents=Selectarea Componentelor
SelectComponentsDesc=Care dintre componente ar trebui instalate?
SelectComponentsLabel2=Selectaþi componentele de instalat; deselectaþi componentele pe care nu doriþi sã le instalaþi. Clicaþi Continuã pentru a merge mai departe.
FullInstallation=Instalare Completã
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Instalare Compactã
CustomInstallation=Instalare Personalizatã
NoUninstallWarningTitle=Componentele Existã
NoUninstallWarning=Instalatorul a detectat cã urmãtoarele componente sînt deja instalate pe calculator:%n%n%1%n%nDeselectarea lor nu le va dezinstala.%n%nDoriþi sã continuaþi oricum?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Selecþia curentã necesitã cel puþin [mb] MB spaþiu de stocare.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Selectarea Sarcinilor Suplimentare
SelectTasksDesc=Ce sarcini suplimentare ar trebui îndeplinite?
SelectTasksLabel2=Selectaþi sarcinile suplimentare care ar trebui îndeplinite în timpul instalãrii [name], apoi clicaþi Continuã.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Selectarea Dosarului din Meniul de Pornire
SelectStartMenuFolderDesc=Unde ar trebui sã fie plasate scurtãturile programului?
SelectStartMenuFolderLabel3=Scurtãturile vor fi plasate în dosarul specificat mai jos al Meniului de Pornire (Start Menu).
SelectStartMenuFolderBrowseLabel=Pentru a avansa cu instalarea, clicaþi Continuã. Dacã doriþi sã selectaþi un alt dosar, clicaþi Exploreazã.
MustEnterGroupName=Trebuie sã introduceþi numele dosarului.
GroupNameTooLong=Numele dosarului sau al cãii este prea lung.
InvalidGroupName=Numele dosarului nu este valid.
BadGroupName=Numele dosarului nu poate include unul dintre caracterele urmãtoarele:%n%n%1
NoProgramGroupCheck2=Nu crea un &dosar în Meniul de Pornire

; *** "Ready to Install" wizard page
WizardReady=Pregãtit de Instalare
ReadyLabel1=Instalatorul e pregãtit pentru instalarea [name] pe calculator.
ReadyLabel2a=Clicaþi Instaleazã pentru a continua cu instalarea, sau clicaþi Înapoi dacã doriþi sã revedeþi sau sã schimbaþi setãrile.
ReadyLabel2b=Clicaþi Instaleazã pentru a continua cu instalarea.
ReadyMemoUserInfo=Info Utilizator:
ReadyMemoDir=Loc de Destinaþie:
ReadyMemoType=Tip de Instalare:
ReadyMemoComponents=Componente Selectate:
ReadyMemoGroup=Dosarul Meniului de Pornire:
ReadyMemoTasks=Sarcini Suplimentare:

; *** "Preparing to Install" wizard page
WizardPreparing=Pregãtire pentru Instalare
PreparingDesc=Instalatorul pregãteºte instalarea [name] pe calculator.
PreviousInstallNotCompleted=Instalarea/dezinstalarea anterioarã a unui program nu a fost terminatã. Va trebui sã reporniþi calculatorul pentru a termina operaþiunea precedentã.%n%nDupã repornirea calculatorului, rulaþi Instalatorul din nou pentru a realiza instalarea [name].
CannotContinue=Instalarea nu poate continua. Clicaþi Anuleazã pentru a o închide.

; *** "Installing" wizard page
WizardInstalling=Instalare în Desfãºurare
InstallingLabel=Aºteptaþi în timp ce se instaleazã [name] pe calculator.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=Finalizarea Instalãrii [name]
FinishedLabelNoIcons=Instalarea [name] pe calculator a fost terminatã.
FinishedLabel=Instalarea [name] pe calculator a fost terminatã. Aplicaþia poate fi lansatã clicînd pe iconiþele instalate.
ClickFinish=Clicaþi Închide pentru a pãrãsi Instalatorul.
FinishedRestartLabel=Pentru a termina instalarea [name], trebuie repornit calculatorul. Doriþi sã fie repornit acum?
FinishedRestartMessage=Pentru a termina instalarea [name], trebuie repornit calculatorul.%n%nDoriþi sã fie repornit acum?
ShowReadmeCheck=Da, aº dori sã vãd fiºierul de informare (README)
YesRadio=&Da, reporneºte calculatorul acum
NoRadio=&Nu, voi reporni eu calculatorul mai tîrziu
; used for example as 'Run MyProg.exe'
RunEntryExec=Ruleazã %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Vezi %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Instalatorul Necesitã Discul Urmãtor
SelectDiskLabel2=Introduceþi Discul %1 ºi clicaþi OK.%n%nDacã fiºierele de pe acest disc pot fi gãsite într-un alt dosar decît cel afiºat mai jos, introduceþi calea corectã sau clicaþi Exploreazã.
PathLabel=&Cale:
FileNotInDir2=Fiºierul "%1" nu poate fi gãsit în "%2". Introduceþi discul corect sau selectaþi al dosar.
SelectDirectoryLabel=Specificaþi locul discului urmãtor.

; *** Installation phase messages
SetupAborted=Instalarea nu a fost terminatã.%n%nCorectaþi problema ºi rulaþi Instalarea din nou.
EntryAbortRetryIgnore=Clicaþi Reîncearcã pentru a încerca din nou, Ignorã pentru a continua oricum, sau Abandoneazã pentru a anula instalarea.

; *** Installation status messages
StatusCreateDirs=Se creeazã dosarele...
StatusExtractFiles=Se extrag fiºierele...
StatusCreateIcons=Se creeazã scurtãturile...
StatusCreateIniEntries=Se creeazã intrãrile INI...
StatusCreateRegistryEntries=Se creeazã intrãrile în registru...
StatusRegisterFiles=Se înregistreazã fiºierele...
StatusSavingUninstall=Se salveazã informaþiile de dezinstalare...
StatusRunProgram=Se finalizeazã instalarea...
StatusRollback=Se revine la starea iniþialã, anulînd modificãrile fãcute...

; *** Misc. errors
ErrorInternal2=Eroare Internã: %1
ErrorFunctionFailedNoCode=%1 a eºuat
ErrorFunctionFailed=%1 a eºuat; cod %2
ErrorFunctionFailedWithMessage=%1 a eºuat; cod %2.%n%3
ErrorExecutingProgram=Nu se poate executa fiºierul:%n%1

; *** Registry errors
ErrorRegOpenKey=Eroare la deschiderea cheii de registru:%n%1\%2
ErrorRegCreateKey=Eroare la crearea cheii de registru:%n%1\%2
ErrorRegWriteKey=Eroare la scrierea în cheia de registru:%n%1\%2

; *** INI errors
ErrorIniEntry=Eroare la crearea intrãrii INI în fiºierul "%1".

; *** File copying errors
FileAbortRetryIgnore=Clicaþi Reîncearcã pentru a încerca din nou, Ignorã pentru a sãri acest fiºier (nerecomandat), sau Abandoneazã pentru a anula instalarea.
FileAbortRetryIgnore2=Clicaþi Reîncearcã pentru a încerca din nou, Ignorã pentru a continua oricum (nerecomandat), sau Abandoneazã pentru a anula instalarea.
SourceIsCorrupted=Fiºierul sursã este deteriorat
SourceDoesntExist=Fiºierul sursã "%1" nu existã
ExistingFileReadOnly=Fiºierul deja existent este marcat doar-citire.%n%nClicaþi Reîncearcã pentru a înlãtura atributul doar-citire ºi a încerca din nou, Ignorã pentru a sãri acest fiºier, sau Abandoneazã pentru a anula instalarea.
ErrorReadingExistingDest=A apãrut o eroare în timpul citirii fiºierului deja existent:
FileExists=Fiºierul existã deja.%n%Doriþi ca el sã fie suprascris de Instalator?
ExistingFileNewer=Fiºierul deja existent este mai nou decît cel care trebuie instalat. Este recomandat sã îl pãstraþi pe cel existent.%n%nDoriþi sã pãstraþi fiºierul deja existent?
ErrorChangingAttr=A apãrut o eroare în timpul schimbãrii atributelor fiºierului deja existent:
ErrorCreatingTemp=A apãrut o eroare în timpul creãrii fiºierului în dosarul de destinaþie:
ErrorReadingSource=A apãrut o eroare în timpul citirii fiºierului sursã:
ErrorCopying=A apãrut o eroare în timpul copierii fiºierului:
ErrorReplacingExistingFile=A apãrut o eroare în timpul înlocuirii fiºierului deja existent:
ErrorRestartReplace=Repornirea/Înlocuirea a eºuat:
ErrorRenamingTemp=A apãrut o eroare în timpul redenumirii fiºierului din dosarul de destinaþie:
ErrorRegisterServer=Nu se poate înregistra DLL/OCX: %1
ErrorRegSvr32Failed=RegSvr32 a eºuat, avînd codul de ieºire %1
ErrorRegisterTypeLib=Nu se poate înregistra biblioteca de tipul: %1

; *** Post-installation errors
ErrorOpeningReadme=A apãrut o eroare în timp ce se încerca deschiderea fiºierului de informare (README).
ErrorRestartingComputer=Instalatorul nu a putut reporni calculatorul. Va trebui sã-l reporniþi manual.

; *** Uninstaller messages
UninstallNotFound=Fiºierul "%1" nu existã. Dezinstalarea nu poate fi fãcutã.
UninstallOpenError=Fiºierul "%1" nu poate fi deschis. Dezinstalarea nu poate fi fãcutã
UninstallUnsupportedVer=Fiºierul "%1" ce conþine jurnalul de dezinstalare este într-un format nerecunoscut de aceastã versiune a dezinstalatorului. Dezinstalarea nu poate fi fãcutã
UninstallUnknownEntry=A fost întîlnitã o intrare necunoscutã (%1) în jurnalul de dezinstalare
ConfirmUninstall=Sigur doriþi sã înlãturaþi complet %1 ºi componentele sale?
UninstallOnlyOnWin64=Aceastã instalare poate fi dezinstalatã doar pe un sistem Windows 64-biþi.
OnlyAdminCanUninstall=Aceastã instalare poate fi dezinstalatã doar de cãtre un utilizator cu drepturi de Administrator.
UninstallStatusLabel=Aºteptaþi ca %1 sã fie înlãturat de pe calculator.
UninstalledAll=%1 a fost înlãturat cu succes de pe calculator.
UninstalledMost=Dezinstalare completã a %1.%n%nAnumite elemente nu au putut fi înlãturate. Acestea pot fi înlãturate manual.
UninstalledAndNeedsRestart=Pentru a termina dezinstalarea %1, calculatorul trebuie repornit.%n%nDoriþi sã fie repornit acum?
UninstallDataCorrupted=Fiºierul "%1" este deteriorat. Dezinstalarea nu poate fi fãcutã

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=ªterg Fiºierul Partajat?
ConfirmDeleteSharedFile2=Sistemul indicã faptul cã fiºierul partajat urmãtor pare sã nu mai fie folosit de vreun alt program. Doriþi ca Dezinstalatorul sã ºteargã acest fiºier partajat?%n%nDacã totuºi mai existã programe care folosesc fiºierul ºi el este ºters, acele programe ar putea sã funcþioneze defectuos. Dacã nu sînteþi sigur, alegeþi Nu. Lãsarea fiºierului în sistem nu va produce nici o neplãcere.
SharedFileNameLabel=Nume Fiºier:
SharedFileLocationLabel=Loc:
WizardUninstalling=Starea Dezinstalãrii
StatusUninstalling=Dezinstalez %1...

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 versiunea %2
AdditionalIcons=Iconiþe suplimentare:
CreateDesktopIcon=Creeazã o iconiþã pe &Birou (Desktop)
CreateQuickLaunchIcon=Creeazã o iconiþã în Bara de &Lansare Rapidã (Quick Launch)
ProgramOnTheWeb=%1 pe internet
UninstallProgram=Dezinstaleazã %1
LaunchProgram=Lanseazã %1
AssocFileExtension=&Asociazã %1 cu extensia de fiºiere %2
AssocingFileExtension=Asociez %1 cu extensia de fiºiere %2...
