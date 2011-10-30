; *** Inno Setup version 5.1.11+ Serbian (Latin) messages ***
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
LanguageName=Srpski
LanguageID=$081a
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
SetupAppTitle=Instalacija
SetupWindowTitle=Instalacija – %1
UninstallAppTitle=Uklanjanje
UninstallAppFullTitle=Uklanjanje programa %1

; *** Misc. common
InformationTitle=Podaci
ConfirmTitle=Potvrda
ErrorTitle=Greška

; *** SetupLdr messages
SetupLdrStartupMessage=Instaliraæete %1. Želite li da nastavite?
LdrCannotCreateTemp=Ne mogu da napravim privremenu datoteku. Instalacija je prekinuta
LdrCannotExecTemp=Ne mogu da pokrenem datoteku u privremenoj fascikli. Instalacija je prekinuta

; *** Startup error messages
LastErrorMessage=%1.%n%nGreška %2: %3
SetupFileMissing=Datoteka %1 nedostaje u fascikli za instalaciju. Ispravite problem ili nabavite novi primerak programa.
SetupFileCorrupt=Datoteke za instalaciju su ošteæene. Nabavite novi primerak programa.
SetupFileCorruptOrWrongVer=Datoteke za instalaciju su ošteæene ili nisu saglasne s ovim izdanjem instalacije. Ispravite problem ili nabavite novi primerak programa.
NotOnThisPlatform=Program neæe raditi na %1.
OnlyOnThisPlatform=Program æe raditi na %1.
OnlyOnTheseArchitectures=Program se može instalirati samo na izdanjima vindousa koji rade sa sledeæim arhitekturama procesora:%n%n%1
MissingWOW64APIs=Izdanje vindousa koje koristite ne sadrži moguænosti potrebne za izvršavanje 64-bitnih instalacija. Instalirajte servisni paket %1 da biste rešili ovaj problem.
WinVersionTooLowError=Program zahteva %1, izdanje %2 ili novije.
WinVersionTooHighError=Program ne možete instalirati na %1 izdanju %2 ili novijem.
AdminPrivilegesRequired=Morate biti prijavljeni kao administrator da biste instalirali program.
PowerUserPrivilegesRequired=Morate biti prijavljeni kao administrator ili ovlašæeni korisnik da biste instalirali program.
SetupAppRunningError=Program %1 je trenutno pokrenut.%n%nZatvorite ga i kliknite na dugme „U redu“ da nastavite ili „Otkaži“ da napustite instalaciju.
UninstallAppRunningError=Program %1 je trenutno pokrenut.%n%nZatvorite ga i kliknite na dugme „U redu“ da nastavite ili „Otkaži“ da napustite instalaciju.

; *** Misc. errors
ErrorCreatingDir=Ne mogu da napravim fasciklu „%1“
ErrorTooManyFilesInDir=Ne mogu da napravim datoteku u fascikli „%1“ jer sadrži previše datoteka

; *** Setup common messages
ExitSetupTitle=Napuštanje instalacije
ExitSetupMessage=Instalacija nije završena. Ako sada izaðete, program neæe biti instaliran.%n%nInstalaciju možete pokrenuti i dovršiti nekom dugom prilikom.%n%nŽelite li da je zatvorite?
AboutSetupMenuItem=&O programu
AboutSetupTitle=Podaci o programu
AboutSetupMessage=%1 izdanje %2%n%3%n%n%1 poèetna stranica:%n%4
AboutSetupNote=
TranslatorNote=Serbian translation by Rancher

; *** Buttons
ButtonBack=< &Nazad
ButtonNext=&Dalje >
ButtonInstall=&Instaliraj
ButtonOK=&U redu
ButtonCancel=&Otkaži
ButtonYes=&Da
ButtonYesToAll=D&a za sve
ButtonNo=&Ne
ButtonNoToAll=N&e za sve
ButtonFinish=&Završi
ButtonBrowse=&Potraži…
ButtonWizardBrowse=&Potraži…
ButtonNewFolder=&Napravi fasciklu

; *** "Select Language" dialog messages
SelectLanguageTitle=Odabir jezika
SelectLanguageLabel=Izaberite jezik koji želite da koristite prilikom instalacije:

; *** Common wizard text
ClickNext=Kliknite na „Dalje“ da nastavite ili „Otkaži“ da napustite instalaciju.
BeveledLabel=
BrowseDialogTitle=Odabir fascikle
BrowseDialogLabel=Izaberite fasciklu sa spiska i kliknite na „U redu“.
NewFolderName=Nova fascikla

; *** "Welcome" wizard page
WelcomeLabel1=Dobro došli na instalaciju programa [name]
WelcomeLabel2=Instaliraæete [name/ver] na svoj raèunar.%n%nPreporuèuje se da zatvorite sve druge programe pre nego što nastavite.

; *** "Password" wizard page
WizardPassword=Lozinka
PasswordLabel1=Instalacija je zaštiæena lozinkom.
PasswordLabel3=Unesite lozinku i kliknite na „Dalje“ da nastavite. Imajte na umu da je lozinka osetljiva na mala i velika slova.
PasswordEditLabel=&Lozinka:
IncorrectPassword=Navedena lozinka nije ispravna. Pokušajte ponovo.

; *** "License Agreement" wizard
WizardLicense=Ugovor o licenci
LicenseLabel=Pažljivo proèitajte sledeæe pre nego što nastavite.
LicenseLabel3=Proèitajte Ugovor o licenci koji se nalazi ispod. Morate prihvatiti uslove ovog ugovora pre nego što nastavite.
LicenseAccepted=&Prihvatam ugovor
LicenseNotAccepted=&Ne prihvatam ugovor

; *** "Information" wizard pages
WizardInfoBefore=Informacije
InfoBeforeLabel=Pažljivo proèitajte sledeæe pre nego što nastavite.
InfoBeforeClickLabel=Kada budete spremni da nastavite instalaciju, kliknite na „Dalje“.
WizardInfoAfter=Informacije
InfoAfterLabel=Pažljivo proèitajte sledeæe pre nego što nastavite.
InfoAfterClickLabel=Kada budete spremni da nastavite instalaciju, kliknite na „Dalje“.

; *** "User Information" wizard page
WizardUserInfo=Korisnièki podaci
UserInfoDesc=Unesite svoje podatke.
UserInfoName=&Korisnik:
UserInfoOrg=&Organizacija:
UserInfoSerial=&Serijski broj:
UserInfoNameRequired=Morate navesti ime.

; *** "Select Destination Location" wizard page
WizardSelectDir=Odabir odredišne fascikle
SelectDirDesc=Izaberite mesto na kom želite da instalirate [name].
SelectDirLabel3=Program æe instalirati [name] u sledeæu fasciklu.
SelectDirBrowseLabel=Kliknite na „Dalje“ da nastavite. Ako želite da izaberete drugu fasciklu, kliknite na „Potraži…“.
DiskSpaceMBLabel=Potrebno je najmanje [mb] MB slobodnog prostora na disku.
ToUNCPathname=Ne mogu da instaliram program u navedenu fasciklu. Ako pokušavate da ga instalirate na mreži, prvo æete morati da mapirate mrežni disk.
InvalidPath=Morate navesti punu putanju s obeležjem diska (npr.%n%nC:\APP%n%nili putanja u obliku%n%n\\server\share)
InvalidDrive=Disk koji ste izabrali ne postoji ili nije dostupan. Izaberite neki drugi.
DiskSpaceWarningTitle=Nema dovoljno prostora na disku
DiskSpaceWarning=Program zahteva najmanje %1 kB slobodnog prostora, a izabrani disk na raspolaganju ima samo %2 kB.%n%nŽelite li ipak da nastavite?
DirNameTooLong=Naziv fascikle ili putanja je predugaèka.
InvalidDirName=Naziv fascikle nije ispravan.
BadDirName32=Naziv fascikle ne sme sadržati ništa od sledeæeg:%n%n%1
DirExistsTitle=Fascikla veæ postoji
DirExists=Fascikla:%n%n%1%n%nveæ postoji. Želite li ipak da instalirate program u nju?
DirDoesntExistTitle=Fascikla ne postoji
DirDoesntExist=Fascikla:%n%n%1%n%nne postoji. Želite li da je napravite?

; *** "Select Components" wizard page
WizardSelectComponents=Odabir delova
SelectComponentsDesc=Koje delove želite da instalirate?
SelectComponentsLabel2=Izaberite delove koje želite da instalirate, a oèistite one koje ne želite. Kliknite na „Dalje“ da nastavite.
FullInstallation=Puna instalacija
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Podrazumevana instalacija
CustomInstallation=Prilagoðena instalacija
NoUninstallWarningTitle=Delovi veæ postoje
NoUninstallWarning=Sledeæi delovi veæ postoje na vašem raèunaru:%n%n%1%n%nDeštrikliranje ovih delova ih neæe ukloniti.%n%nŽelite li da nastavite?
ComponentSize1=%1 kB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Izabrane stavke zahtevaju najmanje [mb] MB slobodnog prostora.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Odabir dodatnih zadataka
SelectTasksDesc=Izaberite neke dodatne zadatke.
SelectTasksLabel2=Izaberite dodatne zadatke koje želite da izvršite pri instaliranju programa [name] i kliknite na „Dalje“.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Odabir fascikle na meniju „Start“
SelectStartMenuFolderDesc=Izaberite mesto na kom želite da postavite preèice.
SelectStartMenuFolderLabel3=Instalacija æe postaviti preèice programa u sledeæoj fascikli na meniju „Start“.
SelectStartMenuFolderBrowseLabel=Kliknite na „Dalje“ da nastavite. Ako želite da izaberete drugu fasciklu, kliknite na „Potraži…“.
MustEnterGroupName=Morate navesti naziv fascikle.
GroupNameTooLong=Naziv fascikle ili putanja je predugaèka.
InvalidGroupName=Naziv fascikle nije ispravan.
BadGroupName=Naziv fascikle ne sme sadržati ništa od sledeæeg:%n%n%1
NoProgramGroupCheck2=N&e pravi fasciklu u meniju „Start“

; *** "Ready to Install" wizard page
WizardReady=Instalacija je spremna
ReadyLabel1=Program je spreman da instalira [name] na vaš raèunar.
ReadyLabel2a=Kliknite na „Instaliraj“ da zapoènete instalaciju ili „Nazad“ da ponovo pregledate i promenite pojedine postavke.
ReadyLabel2b=Kliknite na „Instaliraj“ da zapoènete instalaciju.
ReadyMemoUserInfo=Korisnièki podaci:
ReadyMemoDir=Odredišna fascikla:
ReadyMemoType=Vrsta instalacije:
ReadyMemoComponents=Izabrani delovi:
ReadyMemoGroup=Fascikla na meniju „Start“:
ReadyMemoTasks=Dodatni zadaci:

; *** "Preparing to Install" wizard page
WizardPreparing=Priprema za instalaciju
PreparingDesc=Program se priprema da instalira [name] na vaš raèunar.
PreviousInstallNotCompleted=Instalacija ili uklanjanje prethodnog programa nije završeno. Potrebno je da ponovo pokrenete raèunar da bi se instalacija završila.%n%nNakon ponovnog pokretanja, otvorite instalaciju i instalirajte program [name].
CannotContinue=Ne mogu da nastavim instaliranje. Kliknite na „Otkaži“ da izaðete.

; *** "Installing" wizard page
WizardInstalling=Instaliranje
InstallingLabel=Saèekajte da se [name] instalira na vaš raèunar.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=[name] – završetak instalacije
FinishedLabelNoIcons=Instaliranje programa [name] je završeno.
FinishedLabel=Instaliranje programa [name] je završeno. Možete ga pokrenuti preko postavljenih ikona.
ClickFinish=Kliknite na „Završi“ da izaðete.
FinishedRestartLabel=Potrebno je ponovno pokretanje raèunara da bi se instalacija završila. Želite li da ga ponovo pokrenete?
FinishedRestartMessage=Potrebno je ponovno pokretanje raèunara da bi se instalacija završila.%n%nŽelite li da ga ponovo pokrenete?
ShowReadmeCheck=Da, želim da pogledam tekstualnu datoteku
YesRadio=&Da, ponovo pokreni raèunar
NoRadio=&Ne, kasnije æu ga pokrenuti
; used for example as 'Run MyProg.exe'
RunEntryExec=&Pokreni %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Pogledaj %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Sledeæi disk
SelectDiskLabel2=Ubacite disk %1 i kliknite na „U redu“.%n%nAko se datoteke na ovom disku mogu pronaæi u nekoj drugoj fascikli, unesite odgovarajuæu putanju ili kliknite na „Potraži…“.
PathLabel=&Putanja:
FileNotInDir2=Datoteka „%1“ se ne nalazi u „%2“. Ubacite pravi disk ili izaberite drugu fasciklu.
SelectDirectoryLabel=Izaberite mesto sledeæeg diska.

; *** Installation phase messages
SetupAborted=Instalacija nije završena.%n%nIspravite problem i pokrenite je ponovo.
EntryAbortRetryIgnore=Kliknite na „Pokušaj opet“ da ponovite radnju, „Zanemari“ da nastavite u svakom sluèaju ili „Prekini“ da obustavite instalaciju.

; *** Installation status messages
StatusCreateDirs=Pravim fascikle…
StatusExtractFiles=Raspakujem datoteke…
StatusCreateIcons=Postavljam preèice…
StatusCreateIniEntries=Postavljam unose INI…
StatusCreateRegistryEntries=Postavljam unose u registar…
StatusRegisterFiles=Upisujem datoteke…
StatusSavingUninstall=Èuvam podatke o uklanjanju…
StatusRunProgram=Završavam instalaciju…
StatusRollback=Poništavam izmene…

; *** Misc. errors
ErrorInternal2=Unutrašnja greška: %1
ErrorFunctionFailedNoCode=%1 neuspeh
ErrorFunctionFailed=%1 neuspeh; kod %2
ErrorFunctionFailedWithMessage=%1 neuspeh; kod %2.%n%3
ErrorExecutingProgram=Ne mogu da pokrenem datoteku:%n%1

; *** Registry errors
ErrorRegOpenKey=Greška pri otvaranju unosa u registru:%n%1\%2
ErrorRegCreateKey=Greška pri stvaranju unosa u registru:%n%1\%2
ErrorRegWriteKey=Greška pri upisivanju unosa u registar:%n%1\%2

; *** INI errors
ErrorIniEntry=Greška pri stvaranju unosa INI u datoteci „%1“.

; *** File copying errors
FileAbortRetryIgnore=Kliknite na „Pokušaj opet“ da ponovite radnju, „Zanemari“ da preskoèite datoteku (ne preporuèuje se) ili „Prekini“ da obustavite instalaciju.
FileAbortRetryIgnore2=Kliknite na „Pokušaj opet“ da ponovite radnju, „Zanemari“ da nastavite u svakom sluèaju (ne preporuèuje se) ili „Prekini“ da obustavite instalaciju.
SourceIsCorrupted=Izvorna datoteka je ošteæena
SourceDoesntExist=Izvorna datoteka „%1“ ne postoji
ExistingFileReadOnly=Postojeæa datoteka je samo za èitanje.%n%nKliknite na „Pokušaj opet“ da uklonite osobinu „samo za èitanje“ i ponovite radnju, „Zanemari“ da preskoèite datoteku ili „Prekini“ da obustavite instalaciju.
ErrorReadingExistingDest=Došlo je do greške pri pokušaju èitanja postojeæe datoteke:
FileExists=Datoteka veæ postoji.%n%nŽelite li da je zamenite?
ExistingFileNewer=Postojeæa datoteka je novija od one koju treba postaviti. Preporuèuje se da zadržite postojeæu datoteku.%n%nŽelite li to da uradim?
ErrorChangingAttr=Došlo je do greške pri izmeni osobine sledeæe datoteke:
ErrorCreatingTemp=Došlo je do greške pri stvaranju datoteke u odredišnoj fascikli:
ErrorReadingSource=Došlo je do greške pri èitanju izvorne datoteke:
ErrorCopying=Došlo je do greške pri umnožavanju datoteke:
ErrorReplacingExistingFile=Došlo je do greške pri zameni postojeæe datoteke:
ErrorRestartReplace=Ne mogu da zamenim:
ErrorRenamingTemp=Došlo je do greške pri preimenovanju datoteke u odredišnoj fascikli:
ErrorRegisterServer=Ne mogu da upišem DLL/OCX: %1
ErrorRegSvr32Failed=RegSvr32 nije uspeo. Greška %1
ErrorRegisterTypeLib=Ne mogu da upišem biblioteku tipova: %1

; *** Post-installation errors
ErrorOpeningReadme=Došlo je do greške pri otvaranju tekstualne datoteke.
ErrorRestartingComputer=Ne mogu da ponovo pokrenem raèunar. Uradite to sami.

; *** Uninstaller messages
UninstallNotFound=Datoteka „%1“ ne postoji. Ne mogu da uklonim program.
UninstallOpenError=Datoteka „%1“ se ne može otvoriti. Ne mogu da uklonim program
UninstallUnsupportedVer=Izveštaj „%1“ je u neprepoznatljivom formatu. Ne mogu da uklonim program
UninstallUnknownEntry=Nepoznat unos (%1) se pojavio u izveštaju uklanjanja
ConfirmUninstall=Želite li da uklonite %1 i sve njegove delove?
UninstallOnlyOnWin64=Program se može ukloniti samo na 64-bitnom vindousu.
OnlyAdminCanUninstall=Program može ukloniti samo korisnik s administratorskim pravima.
UninstallStatusLabel=Saèekajte da se %1 ukloni s vašeg raèunara.
UninstalledAll=%1 je uklonjen s vašeg raèunara.
UninstalledMost=%1 je uklonjen.%n%nNeke delove ipak morati sami obrisati.
UninstalledAndNeedsRestart=Potrebno je ponovno pokretanje raèunara da bi se instalacija završila.%n%nŽelite li da ponovo pokrenete raèunar?
UninstallDataCorrupted=Datoteka „%1“ je ošteæena. Ne mogu da uklonim program

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Brisanje deljene datoteke
ConfirmDeleteSharedFile2=Sistem je prijavio da sledeæu deljenu datoteku više ne koristi nijedan program. Želite li da je uklonite?%n%nAko nekim programima i dalje treba ova datoteka a ona je obrisana, ti programi možda neæe ispravno raditi. Ako niste sigurni šta da radite, kliknite na „Ne“. Ostavljanje datoteke na disku neæe prouzrokovati nikakvu štetu.
SharedFileNameLabel=Naziv datoteke:
SharedFileLocationLabel=Putanja:
WizardUninstalling=Stanje uklanjanja
StatusUninstalling=Uklanjam %1…

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 izdanje %2
AdditionalIcons=Dodatne ikone:
CreateDesktopIcon=&Postavi ikonu na radnu površinu
CreateQuickLaunchIcon=P&ostavi ikonu na traku za brzo pokretanje
ProgramOnTheWeb=%1 na internetu
UninstallProgram=Ukloni %1
LaunchProgram=Pokreni %1
AssocFileExtension=&Poveži %1 s formatom %2
AssocingFileExtension=Povezivanje %1 s formatom %2…