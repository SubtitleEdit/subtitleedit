; *** Inno Setup version 5.5.3+ Croatian messages ***
; Translated by: Elvis Gambiraža (el.gambo@gmail.com)
; Based on translation by Krunoslav Kanjuh (krunoslav.kanjuh@zg.t-com.hr)
;
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
LanguageName=Hrvatski
LanguageID=$041a
LanguageCodePage=1250
; If the language you are translating to requires special font faces or
; sizes, uncomment any of the following entries and change them accordingly.
;DialogFontName=MS Shell Dlg
;DialogFontSize=8
;WelcomeFontName=Arial
;WelcomeFontSize=12
;TitleFontName=Arial
;TitleFontSize=29
;CopyrightFontName=Arial
;CopyrightFontSize=8

[Messages]

; *** Application titles
SetupAppTitle=Instalacija
SetupWindowTitle=Instalacija - %1
UninstallAppTitle=Deinstalacija
UninstallAppFullTitle=Deinstalacija programa %1

; *** Misc. common
InformationTitle=Informacija
ConfirmTitle=Potvrda
ErrorTitle=Greška

; *** SetupLdr messages
SetupLdrStartupMessage=Zapoèeli ste instalaciju programa %1. Želite li nastaviti?
LdrCannotCreateTemp=Ne mogu kreirati privremenu datoteku. Instalacija je prekinuta.
LdrCannotExecTemp=Ne mogu pokrenuti datoteku u privremenoj mapi. Instalacija je prekinuta.

; *** Startup error messages
LastErrorMessage=%1.%n%nGreška %2: %3
SetupFileMissing=Datoteka %1 se ne nalazi u instalacijskoj mapi. Ispravite problem ili nabavite novu kopiju programa.
SetupFileCorrupt=Instalacijske datoteke su ošteæene. Nabavite novu kopiju programa.
SetupFileCorruptOrWrongVer=Instalacijske datoteke su ošteæene, ili nisu kompatibilne s ovom verzijom instalacije. Ispravite problem ili nabavite novu kopiju programa.
InvalidParameter=Neispravan parametar je prenijet na komandnu liniju: %n%n%1
SetupAlreadyRunning=Instalacija je veæ pokrenuta.
WindowsVersionNotSupported=Program ne podržava verziju Windowsa koju koristite.
WindowsServicePackRequired=Program zahtijeva %1 servisni paket %2 ili noviji.
NotOnThisPlatform=Ovaj program neæe raditi na %1.
OnlyOnThisPlatform=Ovaj program se mora pokrenuti na %1.
OnlyOnTheseArchitectures=Ovaj program može biti instaliran na verziji Windowsa dizajniranim za sljedeæu procesorsku arhitekturu:%n%n%1
MissingWOW64APIs=Ova verzija Windowsa ne posjeduje funkcije koje zahtjeva instalacija za 64-bitnu instalaciju. Kako bi riješili problem instalirajte servisni paket %1.
WinVersionTooLowError=Ovaj program zahtijeva %1 verziju %2 ili noviju.
WinVersionTooHighError=Ovaj program ne može biti instaliran na %1 verziji %2 ili novijoj.
AdminPrivilegesRequired=Morate biti prijavljeni kao administrator prilikom pokretanja ovog programa.
PowerUserPrivilegesRequired=Morate biti prijavljeni kao administrator ili èlan grupe Power Users prilikom instaliranja ovog programa.
SetupAppRunningError=Instalacija je otkrila da je %1 pokrenut.%n%nZatvorite program i potom kliknite Dalje za nastavak ili Odustani za prekid instalacije.
UninstallAppRunningError=Deinstalacija je otkrila da je %1 pokrenut.%n%nZatvorite program i potom kliknite Dalje za nastavak ili Odustani za prekid instalacije.

; *** Misc. errors
ErrorCreatingDir=Instalacija nije mogla kreirati mapu "%1".
ErrorTooManyFilesInDir=Instalacija nije mogla kreirati datoteku u mapi "%1" zato što ona sadrži previše datoteka.

; *** Setup common messages
ExitSetupTitle=Prekid instalacije
ExitSetupMessage=Instalacija nije završena. Ako sad izaðete, program neæe biti instaliran.%n%nInstalaciju možete pokrenuti kasnije ukoliko ju želite završiti.%n%nPrekid instalacije?
AboutSetupMenuItem=&O programu
AboutSetupTitle=Podaci o programu
AboutSetupMessage=%1 verzija %2%n%3%n%n%1 home page:%n%4
AboutSetupNote=
TranslatorNote=Translated by: Elvis Gambiraža

; *** Buttons
ButtonBack=< Na&trag
ButtonNext=Na&stavak >
ButtonInstall=&Instaliraj
ButtonOK=&U redu
ButtonCancel=&Otkaži
ButtonYes=&Da
ButtonYesToAll=D&a za sve
ButtonNo=&Ne
ButtonNoToAll=N&e za sve
ButtonFinish=&Završi
ButtonBrowse=&Odaberi...
ButtonWizardBrowse=O&daberi...
ButtonNewFolder=&Kreiraj novu mapu

; *** "Select Language" dialog messages
SelectLanguageTitle=Izaberite jezik
SelectLanguageLabel=Izberite jezik koji želite koristiti pri instalaciji:

; *** Common wizard text
ClickNext=Kliknite na Nastavak za nastavak ili Otkaži za prekid instalacije.
BeveledLabel=
BrowseDialogTitle=Odabir mape
BrowseDialogLabel=Odaberite mapu iz liste koja slijedi te kliknite OK.
NewFolderName=Nova mapa

; *** "Welcome" wizard page
WelcomeLabel1=Dobro došli u instalaciju programa [name]
WelcomeLabel2=Ovaj program æe instalirati [name/ver] na vaše raèunalo.%n%nPreporuèamo da zatvorite sve programe prije nego nastavite dalje.

; *** "Password" wizard page
WizardPassword=Lozinka
PasswordLabel1=Instalacija je zaštiæena lozinkom.
PasswordLabel3=Upišite lozinku i kliknite Nastavak. Lozinke su osjetljive na mala i velika slova.
PasswordEditLabel=&Lozinka:
IncorrectPassword=Upisana je pogrešna lozinka. Pokušajte ponovo.

; *** "License Agreement" wizard
WizardLicense=Ugovor o korištenju
LicenseLabel=Molimo prije nastavka pažljivo proèitajte sljedeæe važne informacije.
LicenseLabel3=Molimo pažljivo proèitajte Ugovor o korištenju. Morate prihvatiti uvjete ugovora kako bi mogli nastaviti s instalacijom.
LicenseAccepted=&Prihvaæam ugovor
LicenseNotAccepted=&Ne prihvaæam ugovor

; *** "Information" wizard pages
WizardInfoBefore=Informacije
InfoBeforeLabel=Proèitajte sljedeæe važne informacije prije nastavka.
InfoBeforeClickLabel=Kada budete spremni nastaviti instalaciju kliknite Nastavak.
WizardInfoAfter=Informacije
InfoAfterLabel=Proèitajte sljedeæe važne informacije prije nastavka.
InfoAfterClickLabel=Kada budete spremni nastaviti instalaciju kliknite Nastavak.

; *** "User Information" wizard page
WizardUserInfo=Informacije o korisniku
UserInfoDesc=Upišite informacije o vama.
UserInfoName=&Ime korisnika:
UserInfoOrg=&Organizacija:
UserInfoSerial=&Serijski broj:
UserInfoNameRequired=Morate upisati ime.

; *** "Select Destination Location" wizard page
WizardSelectDir=Odaberite odredišnu mapu
SelectDirDesc=Mapa u koju æe biti instaliran program.
SelectDirLabel3=Instalacija æe instalirati [name] u sljedeæu mapu
SelectDirBrowseLabel=Za nastavak kliknite na Nastavak. Ako želite odabrati drugu mapu kliknite na Odaberi.
DiskSpaceMBLabel=Ovaj program zahtjeva minimalno [mb] MB slobodnog prostora na disku.
CannotInstallToNetworkDrive=Instalacija ne može instalirati na mrežnu jedinicu.
CannotInstallToUNCPath=Instalacija ne može instalirati na UNC putanju.
InvalidPath=Morate unijeti punu stazu zajedno sa slovom diska (npr.%n%nC:\APP%n%nili stazu u obliku%n%n\\server\share)
InvalidDrive=Disk koji ste odabrali ne postoji. Odaberite neki drugi.
DiskSpaceWarningTitle=Nedovoljno prostora na disku
DiskSpaceWarning=Instalacija zahtjeva bar %1 KB slobodnog prostora, a odabrani disk ima samo %2 KB na raspolaganju.%n%nŽelite li nastaviti?
DirNameTooLong=Predugaèak naziv mape ili staze.
InvalidDirName=Naziv mape je pogrešan.
BadDirName32=Naziv mape ne smije sadržavati niti jedan od sljedeæih znakova nakon toèke:%n%n%1
DirExistsTitle=Mapa veæ postoji
DirExists=Mapa:%n%n%1%n%nveæ postoji. Želite li instalirati u nju?
DirDoesntExistTitle=Mapa ne postoji
DirDoesntExist=Mapa:%n%n%1%n%nne postoji. Želite li ju napraviti?

; *** "Select Components" wizard page
WizardSelectComponents=Odaberite komponente
SelectComponentsDesc=Koje komponente želite instalirati?
SelectComponentsLabel2=Odaberite komponente koje želite instalirati, odnosno uklonite kvaèicu uz komponente koje ne želite:
FullInstallation=Puna instalacija

; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Kompaktna instalacija
CustomInstallation=Instalacija po izboru
NoUninstallWarningTitle=Postojeæe komponente
NoUninstallWarning=Instalacija je utvrdila da na vašem raèunalu veæ postoje sljedeæe komponente:%n%n%1%n%nNeodabir tih komponenata ne dovodi do njihove deinstalacije.%n%nŽelite li ipak nastaviti?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Vaš izbor zahtijeva najmanje [mb] MB prostora na disku.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Odaberite zadatke
SelectTasksDesc=Koje dodatne zadatke želite izvršiti?
SelectTasksLabel2=Odaberite zadatke koji æe se izvršiti tijekom instalacije programa [name].

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Odaberite programsku grupu
SelectStartMenuFolderDesc=Lokacija preèice programa
SelectStartMenuFolderLabel3=Instalacija æe kreirati preèice za programe u sljedeæoj Start Menu mapi
SelectStartMenuFolderBrowseLabel=Kako bi nastavili, kliknite na Nastavak. Ako želite odabrati drugu mapu klikni na Odabir.
MustEnterGroupName=Morate unijeti ime programske grupe.
GroupNameTooLong=Predugi naziv mape ili staze.
InvalidGroupName=Naziv mape je pogrešan.
BadGroupName=Ime programske grupe ne smije sadržavati sljedeæe znakove:%n%n%1
NoProgramGroupCheck2=&Ne kreiraj %1 programsku grupu

; *** "Ready to Install" wizard page
WizardReady=Instalacija je spremna
ReadyLabel1=Instalacija je spremna instalirati [name] na vaše raèunalo.
ReadyLabel2a=Kliknite na Instaliraj ako želite instalirati program ili na Nazad ako želite pregledati ili promijeniti postavke.
ReadyLabel2b=Kliknite na Instaliraj ako želite instalirati program.
ReadyMemoUserInfo=Korisnièki podaci:
ReadyMemoDir=Odredišna mapa:
ReadyMemoType=Tip instalacije:
ReadyMemoComponents=Odabrane komponente:
ReadyMemoGroup=Programska grupa:
ReadyMemoTasks=Dodatni zadaci:

; *** "Preparing to Install" wizard page
WizardPreparing=Priprema instalacije
PreparingDesc=Instalacija se priprema za instaliranje [name] na vaše raèunalo.
PreviousInstallNotCompleted=Instalacija/deinstalacija prethodnog programa nije završena. Morate restartati raèunalo kako bi završili tu instalaciju.%n%nNakon restartanja raèunala, ponovno pokrenite Setup kako bi dovršili instalaciju [name].
CannotContinue=Instalacija ne može nastaviti. Kliknite na Odustani za izlaz.
ApplicationsFound=Sljedeæi programi koriste datoteke koje instalacijski program treba ažurirati. Preporuèujemo da dopustite instalacijskom programu da zatvori ove programe.
ApplicationsFound2=Sljedeæi programi koriste datoteke koje instalacijski program treba ažurirati. Preporuèujemo da dopustite instalacijskom programu da zatvori ove programe.
CloseApplications=&Zatvori programe
DontCloseApplications=&Ne zatvaraj programe
ErrorCloseApplications=Ne mogu zatvoriti sve programe. Prije nego nastavite, preporuèujemo da zatvorite sve programe koji koriste datoteke koje instalacijski program treba ažurirati.

; *** "Installing" wizard page
WizardInstalling=Instaliranje
InstallingLabel=Prièekajte dok ne završi instalacija programa [name] na vaše raèunalo.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=Završetak instalacije [name]
FinishedLabelNoIcons=Instalacija programa [name] je završena.
FinishedLabel=Instalacija programa [name] je završena. Program možete pokrenuti preko instaliranih ikona.
ClickFinish=Kliknite na Završi kako biste izašli iz instalacije.
FinishedRestartLabel=Kako biste instalaciju programa [name] završili, potrebno je ponovno pokrenuti raèunalo. Želite li to sada uèiniti?
FinishedRestartMessage=Završetak instalacija programa [name], zahtijeva ponovno pokretanje raèunala.%n%nŽelite li to uèiniti sada?
ShowReadmeCheck=Da, želim proèitati README datoteku
YesRadio=&Da, želim sada ponovno pokrenuti raèunalo
NoRadio=&Ne, kasnije æu ga ponovno pokrenuti

; used for example as 'Run MyProg.exe'
RunEntryExec=&Pokreni %1

; used for example as 'View Readme.txt'
RunEntryShellExec=Pogledaj %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Instalacija treba sljedeæi disk
SelectDiskLabel2=Umetnite disketu %1 i kliknite na OK.%n%nAko se datoteke s ove diskete nalaze na nekom drugom mediju %2 , upišite ispravnu stazu do njega ili kliknite na Odaberi.
PathLabel=&Staza:
FileNotInDir2=Datoteka "%1" ne postoji u "%2". Molimo ubacite odgovarajuæi disk ili odaberete drugi %3.
SelectDirectoryLabel=Odaberite lokaciju sljedeæeg diska.

; *** Installation phase messages
SetupAborted=Instalacija nije završena.%n%nIspravite problem i opet pokrenite instalaciju.
EntryAbortRetryIgnore=Kliknite na Ponovi za novi pokušaj, Ignoriraj za nastavak, ili Prekid za prekid instalacije.

; *** Installation status messages
StatusClosingApplications=Zatvaram programe...
StatusCreateDirs=Kreiram mape...
StatusExtractFiles=Izdvajam datoteke...
StatusCreateIcons=Kreiram ikone...
StatusCreateIniEntries=Kreiram INI datoteke...
StatusCreateRegistryEntries=Kreiram podatke za registry...
StatusRegisterFiles=Registriram datoteke...
StatusSavingUninstall=Snimam deinstalacijske informacije...
StatusRunProgram=Završavam instalaciju...
StatusRestartingApplications=Ponovo pokreæem programe...
StatusRollback=Poništavam promjene...

; *** Misc. errors
ErrorInternal2=Interna greška: %1
ErrorFunctionFailedNoCode=%1 nije uspjelo
ErrorFunctionFailed=%1 nije uspjelo; kod %2
ErrorFunctionFailedWithMessage=%1 nije uspjelo; kod %2.%n%3
ErrorExecutingProgram=Ne mogu pokrenuti datoteku:%n%1

; *** Registry errors
ErrorRegOpenKey=Greška pri otvaranju registry kljuèa:%n%1\%2
ErrorRegCreateKey=Greška pri kreiranju registry kljuèa:%n%1\%2
ErrorRegWriteKey=Greška pri pisanju u registry kljuè:%n%1\%2

; *** INI errors
ErrorIniEntry=Greška pri kreiranju INI podataka u datoteci "%1".

; *** File copying errors
FileAbortRetryIgnore=Kliknite Ponovi za novi pokušaj, Ignoriraj za preskok ove datoteke (ne preporuèa se), ili Prekid za prekid instalacije.
FileAbortRetryIgnore2=Kliknite Ponovi za novi pokušaj, Ignoriraj za nastavak u svakom sluèaju (ne preporuèa se), ili Prekid za prekid instalacije
SourceIsCorrupted=Izvorišna datoteka je ošteæena
SourceDoesntExist=Izvorišna datoteka "%1" ne postoji
ExistingFileReadOnly=Postojeæa datoteka je oznaèena "samo-za-èitanje".%n%nKliknite Ponovi kako biste uklonili oznaku "samo-za-èitanje" i pokušajte ponovno, Ignoriraj za preskok ove datoteke, ili Prekid za prekid instalacije.
ErrorReadingExistingDest=Pojavila se greška prilikom pokušaja èitanja postojeæe datoteke:
FileExists=Datoteka veæ postoji.%n%nŽelite li ju zamijeniti?
ExistingFileNewer=Postojeæa datoteka je novija od one koju pokušavate instalirati. Preporuèa se zadržati postojeæu datoteku.%n%nŽelite li zadržati postojeæu datoteku?
ErrorChangingAttr=Pojavila se greška prilikom pokušaja promjene atributa postojeæe datoteke:
ErrorCreatingTemp=Pojavila se greška prilikom pokušaja kreiranja datoteke u odredišnoj mapi:
ErrorReadingSource=Pojavila se greška prilikom pokušaja èitanja izvorišne datoteke:
ErrorCopying=Pojavila se greška prilikom pokušaja kopiranja datoteke:
ErrorReplacingExistingFile=Pojavila se greška prilikom pokušaja zamjene datoteke:
ErrorRestartReplace=Zamjena nakon ponovnog pokretanja nije uspjela:
ErrorRenamingTemp=Pojavila se greška prilikom pokušaja preimenovanja datoteke u odredišnoj mapi:
ErrorRegisterServer=Ne mogu registrirati DLL/OCX: %1
ErrorRegSvr32Failed=Greška u RegSvr32: greška %1
ErrorRegisterTypeLib=Ne mogu registrirati type library: %1

; *** Post-installation errors
ErrorOpeningReadme=Pojavila se greška prilikom pokušaja otvaranja README datoteke.
ErrorRestartingComputer=Instalacija ne može ponovno pokrenuti raèunalo. Uèinite to ruèno.

; *** Uninstaller messages
UninstallNotFound=Datoteka "%1" ne postoji. Deinstalacija prekinuta.
UninstallOpenError=Datoteku "%1" ne mogu otvoriti. Deinstalacija nije moguæa.
UninstallUnsupportedVer=Deinstalacijska datoteka "%1" je u formatu koji nije prepoznat od ove verzije deinstalacijskog programa. Nije moguæa deinstalacija.
UninstallUnknownEntry=Nepoznat zapis (%1) je pronaðen u deinstalacijskoj datoteci.
ConfirmUninstall=Želite li zaista ukloniti %1 i sve njegove komponente?
UninstallOnlyOnWin64=Ova instalacija može biti uklonjena samo na 64-bitnim Windowsima.
OnlyAdminCanUninstall=Ova instalacija može biti uklonjena samo od korisnika sa administratorskim pravima.
UninstallStatusLabel=Prièekajte dok %1 ne bude uklonjen s vašeg raèunala.
UninstalledAll=Program %1 je uspješno uklonjen sa vašeg raèunala.
UninstalledMost=Deinstalacija programa %1 je završena.%n%nNeke elemente nije bilo moguæe ukloniti. Uèinite to ruèno.
UninstalledAndNeedsRestart=Kako bi završili deinstalaciju %1, Vaše raèunalo morate ponovno pokrenuti%n%nŽelite li to uèiniti sada? 
UninstallDataCorrupted="%1" datoteka je ošteæena. Deinstalacija nije moguæa.

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Brisanje dijeljene datoteke
ConfirmDeleteSharedFile2=Sistem ukazuje da sljedeæe dijeljene datoteke ne koristi niti jedan program. Želite li ukloniti te dijeljene datoteke?%n%nAko neki programi i dalje koriste te datoteke, a one se izbrišu, ti programi neæe ispravno raditi. Ako niste sigurni, odaberite Ne. Ostavljanje datoteka neæe uzrokovati štetu vašem sistemu.
SharedFileNameLabel=Datoteka:
SharedFileLocationLabel=Staza:
WizardUninstalling=Deinstalacija
StatusUninstalling=Deinstaliram %1...
; *** Shutdown block reasons
ShutdownBlockReasonInstallingApp=Instaliram %1.
ShutdownBlockReasonUninstallingApp=Deinstaliram %1.

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 verzija %2
AdditionalIcons=Dodatne ikone:
CreateDesktopIcon=Kreiraj ikonu na &Desktopu
CreateQuickLaunchIcon=Kreiraj ikonu u traci za brzo pokretanje
ProgramOnTheWeb=%1 na internetu
UninstallProgram=Deinstaliraj %1
LaunchProgram=Pokreni %1
AssocFileExtension=Pridru&ži %1 sa %2 ekstenzijom datoteke
AssocingFileExtension=Pridružujem %1 sa %2 ekstenzijom datoteke
AutoStartProgramGroupDescription=Pokretanje:
AutoStartProgram=Automatski pokreni %1
AddonHostProgramNotFound=%1 se ne nalazi u navedenoj mapi.%n%nŽelite li svejedno nastaviti?