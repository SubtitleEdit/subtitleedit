; *** Inno Setup version 5.1.11+ Romanian messages ***
; Traducator : Alexandru Bogdan Munteanu (muntealb@gmail.com)
; Corecturi : dr.jackson
; To download user-contributed translations of this file, go to:
; http://www.jrsoftware.org/files/istrans/
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
InformationTitle=Informatii
ConfirmTitle=Confirmare
ErrorTitle=Eroare

; *** SetupLdr messages
SetupLdrStartupMessage=Va fi instalat programul %1. Doriti sã continuati?
LdrCannotCreateTemp=Nu se poate crea un fisier temporar. Instalare abandonatã
LdrCannotExecTemp=Nu se poate executa un fisier din dosarul temporar. Instalare abandonatã

; *** Startup error messages
LastErrorMessage=%1.%n%nEroarea %2: %3
SetupFileMissing=Fisierul %1 lipseste din dosarul de instalare. Corectati problema sau faceti rost de o copie nouã a programului.
SetupFileCorrupt=Fisierele de instalare sunt deteriorate. Faceti rost de o copie nouã a programului.
SetupFileCorruptOrWrongVer=Fisierele de instalare sunt deteriorate sau sunt incompatibile cu aceastã versiune a Instalatorului. Remediati problema sau obtineti o copie nouã a programului.
NotOnThisPlatform=Acest program nu va rula pe %1.
OnlyOnThisPlatform=Acest program trebuie sã ruleze pe %1.
OnlyOnTheseArchitectures=Acest program poate fi instalat doar pe versiuni de Windows proiectate pentru urmãtoarele arhitecturi de procesor:%n%n%1
MissingWOW64APIs=Versiunea de Windows pe care o rulati nu include functionalitatea cerutã de Instalator pentru a realiza o instalare pe 64-biti. Pentru a corecta problema, va trebui sã instalati Service Pack %1.
WinVersionTooLowError=Acest program necesitã %1 versiunea %2 sau mai nouã.
WinVersionTooHighError=Acest program nu poate fi instalat pe %1 versiunea %2 sau mai nouã.
AdminPrivilegesRequired=Trebuie sã fiti logat ca Administrator pentru instalarea acestui program.
PowerUserPrivilegesRequired=Trebuie sã fiti logat ca Administrator sau ca Membru al Grupului de Utilizatori Împuterniciti pentru a instala acest program.
SetupAppRunningError=Programul de instalare a detectat cã %1 ruleazã în acest moment.%n%nÎnchideti toate instantele programului respectiv, apoi apasati OK pentru a continua sau Anuleazã pentru a abandona instalarea.
UninstallAppRunningError=Programul de dezinstalare a detectat cã %1 ruleazã în acest moment.%n%nÎnchideti toate instantele programului respectiv, apoi apasati OK pentru a continua sau Anuleazã pentru a abandona dezinstalarea.

; *** Misc. errors
ErrorCreatingDir= Programul de instalare nu a putut crea dosarul "%1"
ErrorTooManyFilesInDir=Nu se poate crea un fisier în dosarul "%1" din cauzã cã are deja prea multe fisiere

; *** Setup common messages
ExitSetupTitle=Abandonarea Instalãrii
ExitSetupMessage=Instalarea nu este terminatã. Dacã o abandonati acum, programul nu va fi instalat.%n%nPuteti sã rulati Programul de instalare din nou altã datã pentru a termina instalarea.%n%nAbandonati Instalarea?
AboutSetupMenuItem=&Despre Programul de instalare...
AboutSetupTitle=Despre Programul de instalare
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
SelectLanguageTitle=Selectarea Limbii Programului de instalare
SelectLanguageLabel=Selectati limba folositã pentru instalare:

; *** Common wizard text
ClickNext=Apasa Continuã pentru a avansa cu instalarea sau Anuleazã pentru a o abandona.
BeveledLabel=
BrowseDialogTitle=Explorare dupã Dosar
BrowseDialogLabel=Selectati un dosar din lista de mai jos, apoi apasa OK.
NewFolderName=Dosar Nou

; *** "Welcome" wizard page
WelcomeLabel1=Bun venit la Instalarea [name]
WelcomeLabel2=Programul [name/ver] va fi instalat pe calculator.%n%nEste recomandat sã închideti toate celelalte aplicatii înainte de a continua.

; *** "Password" wizard page
WizardPassword=Parolã
PasswordLabel1=Aceastã instalare este protejatã prin parolã.
PasswordLabel3=Completati parola, apoi apasa Continuã pentru a merge mai departe. Se ia în considerare tipul literelor din parolã (Majuscule/minuscule).
PasswordEditLabel=&Parolã:
IncorrectPassword=Parola pe care ati introdus-o nu este corectã. Reîncercati.

; *** "License Agreement" wizard page
WizardLicense=Acord de Licentiere
LicenseLabel=Cititi informatiile urmãtoare înainte de a continua, sunt importante.
LicenseLabel3=Cititi urmãtorul Acord de Licentiere. Trebuie sã acceptati termenii acestui acord înainte de a continua instalarea.
LicenseAccepted=&Accept licenta
LicenseNotAccepted=&Nu accept licenta

; *** "Information" wizard pages
WizardInfoBefore=Informatii
InfoBeforeLabel=Cititi informatiile urmãtoare înainte de a continua, sunt importante.
InfoBeforeClickLabel=Când sunteti gata de a trece la Instalare, apasati Continuã.
WizardInfoAfter=Informatii
InfoAfterLabel=Cititi informatiile urmãtoare înainte de a continua, sunt importante.
InfoAfterClickLabel=Când sunteti gata de a trece la Instalare, apasati Continuã.

; *** "User Information" wizard page
WizardUserInfo=Informatii despre Utilizator
UserInfoDesc=Introduceti informatiile solicitate.
UserInfoName=&Utilizator:
UserInfoOrg=&Organizatie:
UserInfoSerial=Numãr de &Serie:
UserInfoNameRequired=Trebuie sã introduceti un nume.

; *** "Select Destination Location" wizard page
WizardSelectDir=Selectarea Locului de Destinatie
SelectDirDesc=Unde doriti sã instalati [name]?
SelectDirLabel3= Programul de instalare va pune [name] în dosarul specificat mai jos.
SelectDirBrowseLabel=Pentru a avansa cu instalarea, apasati Continuã. Dacã doriti sã selectati un alt dosar, apasati Exploreazã.
DiskSpaceMBLabel=Este necesar un spatiu liber de stocare de cel putin [mb] MB.
InvalidPath=Trebuie sã introduceti o cale completã, inclusiv litera dispozitivului; de exemplu:%n%nC:\APP%n%nsau o cale UNC de forma:%n%n\\server\share
InvalidDrive=Dispozitivul sau partajul UNC pe care l-ati selectat nu existã sau nu este accesibil. Selectati altul.
DiskSpaceWarningTitle=Spatiu de Stocare Insuficient
DiskSpaceWarning=Instalarea necesitã cel putin %1 KB de spatiu de stocare liber, dar dispozitivul selectat are doar %2 KB liberi.%n%nDoriti sã continuati oricum?
DirNameTooLong=Numele dosarului sau al cãii este prea lung.
InvalidDirName=Numele dosarului nu este valid.
BadDirName32=Numele dosarelor nu pot include unul din urmãtoarele caractere:%n%n%1
DirExistsTitle=Dosarul Existã
DirExists=Dosarul:%n%n%1%n%nexistã deja. Doriti totusi sã instalati în acel dosar?
DirDoesntExistTitle=Dosarul Nu Existã
DirDoesntExist=Dosarul:%n%n%1%n%nnu existã. Doriti ca el sã fie creat?

; *** "Select Components" wizard page
WizardSelectComponents=Selectarea Componentelor
SelectComponentsDesc=Care dintre componente ar trebui instalate?
SelectComponentsLabel2=Selectati componentele de instalat; deselectati componentele pe care nu doriti sã le instalati. Apasati Continuã pentru a merge mai departe.
FullInstallation=Instalare Completã

; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Instalare Compactã
CustomInstallation=Instalare Personalizatã
NoUninstallWarningTitle=Componentele Existã
NoUninstallWarning= Programul de instalare a detectat cã urmãtoarele componente sunt deja instalate pe calculator:%n%n%1%n%nDeselectarea lor nu le va dezinstala.%n%nDoriti sã continuati oricum?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Selectia curentã necesitã cel putin [mb] MB spatiu de stocare.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Selectarea Sarcinilor Suplimentare
SelectTasksDesc=Ce sarcini suplimentare ar trebui îndeplinite?
SelectTasksLabel2=Selectati sarcinile suplimentare care ar trebui îndeplinite în timpul instalãrii [name], apoi apasati Continuã.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Selectarea Dosarului din Meniul de Pornire
SelectStartMenuFolderDesc=Unde ar trebui sã fie plasate scurtãturile programului?
SelectStartMenuFolderLabel3=Scurtãturile vor fi plasate în dosarul specificat mai jos al Meniului de Pornire (Start Menu).
SelectStartMenuFolderBrowseLabel=Pentru a avansa cu instalarea, apasati Continuã. Dacã doriti sã selectati un alt dosar, apasati Exploreazã.
MustEnterGroupName=Trebuie sã introduceti numele dosarului.
GroupNameTooLong=Numele dosarului sau al cãii este prea lung.
InvalidGroupName=Numele dosarului nu este valid.
BadGroupName=Numele dosarului nu poate include unul dintre caracterele urmãtoarele:%n%n%1
NoProgramGroupCheck2=Nu crea un &dosar în Meniul de Pornire

; *** "Ready to Install" wizard page
WizardReady=Pregãtit de Instalare
ReadyLabel1=Instalatorul e pregãtit pentru instalarea [name] pe calculator.
ReadyLabel2a=Apasati Instaleazã pentru a continua cu instalarea, sau apasati Înapoi dacã doriti sã revedeti sau sã schimbati setãrile.
ReadyLabel2b=Apasati Instaleazã pentru a continua cu instalarea.
ReadyMemoUserInfo=Info Utilizator:
ReadyMemoDir=Loc de Destinatie:
ReadyMemoType=Tip de Instalare:
ReadyMemoComponents=Componente Selectate:
ReadyMemoGroup=Dosarul Meniului de Pornire:
ReadyMemoTasks=Sarcini Suplimentare:

; *** "Preparing to Install" wizard page
WizardPreparing=Pregãtire pentru Instalare
PreparingDesc= Programul de instalare pregãteste instalarea [name] pe calculator.
PreviousInstallNotCompleted=Instalarea/dezinstalarea anterioarã a unui program nu a fost terminatã. Va trebui sã reporniti calculatorul pentru a termina operatiunea precedentã.%n%nDupã repornirea calculatorului, rulati Programul de instalare din nou pentru a realiza instalarea [name].
CannotContinue=Instalarea nu poate continua. Apasati Anuleazã pentru a o închide.

; *** "Installing" wizard page
WizardInstalling=Instalare în Desfãsurare
InstallingLabel=Asteptati în timp ce se instaleazã [name] pe calculator.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=Finalizarea Instalãrii [name]
FinishedLabelNoIcons=Instalarea [name] pe calculator a fost terminatã.
FinishedLabel=Instalarea [name] pe calculator a fost terminatã. Aplicatia poate fi lansatã apasând iconurile instalate.
ClickFinish=Apasati Închide pentru a pãrãsi Instalatorul.
FinishedRestartLabel=Pentru a termina instalarea [name], trebuie repornit calculatorul. Doriti sã fie repornit acum?
FinishedRestartMessage=Pentru a termina instalarea [name], trebuie repornit calculatorul.%n%nDoriti sã fie repornit acum?
ShowReadmeCheck=Da, as dori sã vãd fisierul de informare (README)
YesRadio=&Da, reporneste calculatorul acum
NoRadio=&Nu, voi reporni eu calculatorul mai târziu
; used for example as 'Run MyProg.exe'
RunEntryExec=Ruleazã %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Vezi %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle= Programul de instalare Necesitã Discul Urmãtor
SelectDiskLabel2=Introduceti Discul %1 si apasati OK.%n%nDacã fisierele de pe acest disc pot fi gãsite într-un alt dosar decât cel afisat mai jos, introduceti calea corectã sau apasati Exploreazã.
PathLabel=&Cale:
FileNotInDir2=Fisierul "%1" nu poate fi gãsit în "%2". Introduceti discul corect sau selectati al dosar.
SelectDirectoryLabel=Specificati locul discului urmãtor.

; *** Installation phase messages
SetupAborted=Instalarea nu a fost terminatã.%n%nCorectati problema si rulati Instalarea din nou.
EntryAbortRetryIgnore=Apasati Reîncearcã pentru a încerca din nou, Ignorã pentru a continua oricum, sau Abandoneazã pentru a anula instalarea.

; *** Installation status messages
StatusCreateDirs=Se creeazã dosarele...
StatusExtractFiles=Se extrag fisierele...
StatusCreateIcons=Se creeazã scurtãturile...
StatusCreateIniEntries=Se creeazã intrãrile INI...
StatusCreateRegistryEntries=Se creeazã intrãrile în registru...
StatusRegisterFiles=Se înregistreazã fisierele...
StatusSavingUninstall=Se salveazã informatiile de dezinstalare...
StatusRunProgram=Se finalizeazã instalarea...
StatusRollback=Se revine la starea initialã, anulând modificãrile fãcute...

; *** Misc. errors
ErrorInternal2=Eroare Internã: %1
ErrorFunctionFailedNoCode=%1 a esuat
ErrorFunctionFailed=%1 a esuat; cod %2
ErrorFunctionFailedWithMessage=%1 a esuat; cod %2.%n%3
ErrorExecutingProgram=Nu se poate executa fisierul:%n%1

; *** Registry errors
ErrorRegOpenKey=Eroare la deschiderea cheii de registru:%n%1\%2
ErrorRegCreateKey=Eroare la crearea cheii de registru:%n%1\%2
ErrorRegWriteKey=Eroare la scrierea în cheia de registru:%n%1\%2

; *** INI errors
ErrorIniEntry=Eroare la crearea intrãrii INI în fisierul "%1".

; *** File copying errors
FileAbortRetryIgnore=Apasati Reîncearcã pentru a încerca din nou, Ignorã pentru a sãri acest fisier (nerecomandat), sau Abandoneazã pentru a anula instalarea.
FileAbortRetryIgnore2=Apasati Reîncearcã pentru a încerca din nou, Ignorã pentru a continua oricum (nerecomandat), sau Abandoneazã pentru a anula instalarea.
SourceIsCorrupted=Fisierul sursã este deteriorat
SourceDoesntExist=Fisierul sursã "%1" nu existã
ExistingFileReadOnly=Fisierul deja existent este marcat doar-citire.%n%nApasati Reîncearcã pentru a înlãtura atributul doar-citire si a încerca din nou, Ignorã pentru a sãri acest fisier, sau Abandoneazã pentru a anula instalarea.
ErrorReadingExistingDest=A apãrut o eroare în timpul citirii fisierului deja existent:
FileExists=Fisierul existã deja.%n%Doriti ca el sã fie suprascris de Programul de instalare?
ExistingFileNewer=Fisierul deja existent este mai nou decât cel care trebuie instalat. Este recomandat sã îl pãstrati pe cel existent.%n%nDoriti sã pãstrati fisierul deja existent?
ErrorChangingAttr=A apãrut o eroare în timpul schimbãrii atributelor fisierului deja existent:
ErrorCreatingTemp=A apãrut o eroare în timpul creãrii fisierului în dosarul de destinatie:
ErrorReadingSource=A apãrut o eroare în timpul citirii fisierului sursã:
ErrorCopying=A apãrut o eroare în timpul copierii fisierului:
ErrorReplacingExistingFile=A apãrut o eroare în timpul înlocuirii fisierului deja existent:
ErrorRestartReplace=Repornirea/Înlocuirea a esuat:
ErrorRenamingTemp=A apãrut o eroare în timpul redenumirii fisierului din dosarul de destinatie:
ErrorRegisterServer=Nu se poate înregistra DLL/OCX: %1
ErrorRegSvr32Failed=RegSvr32 a esuat, având codul de iesire %1
ErrorRegisterTypeLib=Nu se poate înregistra biblioteca de tipul: %1

; *** Post-installation errors
ErrorOpeningReadme=A apãrut o eroare în timp ce se încerca deschiderea fisierului de informare (README).
ErrorRestartingComputer= Programul de instalare nu a putut reporni calculatorul. Va trebui sã-l reporniti manual.

; *** Uninstaller messages
UninstallNotFound=Fisierul "%1" nu existã. Dezinstalarea nu poate fi fãcutã.
UninstallOpenError=Fisierul "%1" nu poate fi deschis. Dezinstalarea nu poate fi fãcutã
UninstallUnsupportedVer=Fisierul "%1" ce contine jurnalul de dezinstalare este într-un format nerecunoscut de aceastã versiune a Programul de dezinstalare. Dezinstalarea nu poate fi fãcutã
UninstallUnknownEntry=A fost întîlnitã o intrare necunoscutã (%1) în jurnalul de dezinstalare
ConfirmUninstall=Sigur doriti sã înlãturati complet %1 si componentele sale?
UninstallOnlyOnWin64=Aceastã instalare poate fi dezinstalatã doar pe un sistem Windows 64-biti.
OnlyAdminCanUninstall=Aceastã instalare poate fi dezinstalatã doar de cãtre un utilizator cu drepturi de Administrator.
UninstallStatusLabel=Asteptati ca %1 sã fie înlãturat de pe calculator.
UninstalledAll=%1 a fost înlãturat cu succes de pe calculator.
UninstalledMost=Dezinstalare completã a %1.%n%nAnumite elemente nu au putut fi înlãturate. Acestea pot fi înlãturate manual.
UninstalledAndNeedsRestart=Pentru a termina dezinstalarea %1, calculatorul trebuie repornit.%n%nDoriti sã fie repornit acum?
UninstallDataCorrupted=Fisierul "%1" este deteriorat. Dezinstalarea nu poate fi fãcutã

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Sterg Fisierul Partajat?
ConfirmDeleteSharedFile2=Sistemul indicã faptul cã fisierul partajat urmãtor pare sã nu mai fie folosit de vreun alt program. Doriti ca Programul de dezinstalare sã steargã acest fisier partajat?%n%nDacã totusi mai existã programe care folosesc fisierul si el este sters, acele programe ar putea sã functioneze defectuos. Dacã nu sunteti sigur, alegeti Nu. Lãsarea fisierului în sistem nu va produce nici o neplãcere.
SharedFileNameLabel=Nume Fisier:
SharedFileLocationLabel=Loc:
WizardUninstalling=Starea Dezinstalãrii
StatusUninstalling=Dezinstalez %1...

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.
[CustomMessages]
NameAndVersion=%1 versiunea %2
AdditionalIcons=Iconuri suplimentare:
CreateDesktopIcon=Creeazã un icon pe &Birou (Desktop)
CreateQuickLaunchIcon=Creeazã un icon în Bara de &Lansare Rapidã (Quick Launch)
ProgramOnTheWeb=%1 pe internet
UninstallProgram=Dezinstaleazã %1
LaunchProgram=Lanseazã %1
AssocFileExtension=&Asociazã %1 cu extensia de fisiere %2
AssocingFileExtension=Asociez %1 cu extensia de fisiere %2...