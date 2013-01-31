; *** Inno Setup version 5.1.11+ Croatian messages ***
; Translated by: Krunoslav Kanjuh (krunoslav.kanjuh@zg.t-com.hr), updated by Jurko Gospodnetic (jurko.gospodnetic@pke.hr)


[LangOptions]
LanguageName=Hrvatski
LanguageID=$041a
LanguageCodePage=1250
[Messages]

; If the language you are translating to requires special font faces or
; sizes, uncomment any of the following entries and change them accordingly.
;DialogFontName=MS Shell Dlg
;DialogFontSize=8
;DialogFontStandardHeight=13
;TitleFontName=Arial
;TitleFontSize=29
;WelcomeFontName=Arial
;WelcomeFontSize=12
;CopyrightFontName=Arial
;CopyrightFontSize=8
; *** Application titles
SetupAppTitle=Instalacija
SetupWindowTitle=Instalacija - %1
UninstallAppTitle=Deinstalacija
UninstallAppFullTitle=Deinstalacija %1

; *** Misc. common
InformationTitle=Informacija
ConfirmTitle=Potvrda
ErrorTitle=Greška

; *** SetupLdr messages
SetupLdrStartupMessage=Zapoèeli ste instalaciju programa %1. Želite li nastaviti?
LdrCannotCreateTemp=Ne mogu kreirati privremenu datoteku. Instalacija prekinuta
LdrCannotExecTemp=Ne mogu izvršiti datoteku u privremenom direktoriju. Instalacija prekinuta

; *** Startup error messages
LastErrorMessage=%1.%n%nGreška %2: %3
SetupFileMissing=Datoteka %1 se ne nalazi u instalacijskom direktoriju. Molimo vas riješite problem ili nabavite novu kopiju programa.
SetupFileCorrupt=Instalacijske datoteke sadrže grešku. Nabavite novu kopiju programa.
SetupFileCorruptOrWrongVer=Instalacijske datoteke sadrže grešku, ili nisu kompatibilne sa ovom verzijom instalacije. Molimo vas riješite problem ili nabavite novu kopiju programa.
NotOnThisPlatform=Ovaj program neæe raditi na %1.
OnlyOnThisPlatform=Ovaj program se mora pokrenuti na %1.
OnlyOnTheseArchitectures=Ovaj program može biti instaliran na verziji Windowsa dizajniranim za slijedeæu procesorsku arhitekturu:%n%n%1
MissingWOW64APIs=Ova verzija Windowsa ne posjeduje funkcije koje zahtijeva 64-bitna instalacija. Kako bi riješili problem molim instalirajte Service Pack %1
WinVersionTooLowError=Ovaj program zahtijeva %1 verziju %2 ili noviju.
WinVersionTooHighError=Ovaj program ne može biti instaliran na %1 verziji %2 ili novijoj.
AdminPrivilegesRequired=Morate imati administratorske privilegije prilikom pokretanja ovog programa.
PowerUserPrivilegesRequired=Morate imati administratorske privilegije ili biti èlan grupe Power Users prilikom instaliranja ovog programa.
SetupAppRunningError=Instalacija je otkrila da je %1 pokrenut.%n%nMolimo zatvorite program i sve njegove kopije te potom kliknite Dalje za nastavak ili Odustani za prekid.
UninstallAppRunningError=Deinstalacija je otkrila da je %1 pokrenut.%n%nMolimo zatvorite program i sve njegove kopije te potom kliknite Dalje za nastavak ili Odustani za prekid.

; *** Misc. errors
ErrorCreatingDir=Instalacija nije mogla kreirati direktorij "%1"
ErrorTooManyFilesInDir=Instalacija nije mogla kreirati datoteku u direktoriju "%1" zato što on sadrži previše datoteka.

; *** Setup common messages
ExitSetupTitle=Prekid instalacije
ExitSetupMessage=Instalacija nije izvršena. Ako sad izaðete, program neæe biti instaliran.%n%nInstalaciju možete pokrenuti kasnije ukoliko želite završiti instalaciju.%n%nPrekid instalacije?
AboutSetupMenuItem=&O instalaciji...
AboutSetupTitle=O instalaciji
AboutSetupMessage=%1 verzija %2%n%3%n%n%1 home page:%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< Na&zad
ButtonNext=&Nastavak >
ButtonInstall=&Instaliraj
ButtonOK=U redu
ButtonCancel=Otkaži
ButtonYes=&Da
ButtonYesToAll=Da za &sve
ButtonNo=&Ne
ButtonNoToAll=N&e za sve
ButtonFinish=&Završi
ButtonBrowse=&Odaberi...
ButtonWizardBrowse=&Odaberi...
ButtonNewFolder=&Kreiraj novi direktorij

; *** "Select Language" dialog messages
SelectLanguageTitle=Izaberite jezik instalacije
SelectLanguageLabel=Izberite jezik koji želite koristiti pri instalaciji:

; *** Common wizard text
ClickNext=Kliknite na Nastavak za nastavak ili Otkaži za prekid instalacije.
BeveledLabel=
BrowseDialogTitle=Odabir direktorija
BrowseDialogLabel=Odaberi direktorij iz liste koja slijedi te klikni na OK.
NewFolderName=Novi direktorij

; *** "Welcome" wizard page
WelcomeLabel1=Dobro došli u instalaciju programa [name].
WelcomeLabel2=Ovaj program æe instalirati [name/ver] na vaše raèunalo.%n%nPreporuèamo da zatvorite sve programe prije nego nastavite dalje.

; *** "Password" wizard page
WizardPassword=Lozinka
PasswordLabel1=Instalacija je zaštiæena lozinkom.
PasswordLabel3=Upišite lozinku. Lozinke su osjetljive na mala i velika slova.
PasswordEditLabel=&Lozinka:
IncorrectPassword=Upisana je pogrešna lozinka. Pokušajte ponovo.

; *** "License Agreement" wizard page
WizardLicense=Ugovor o korištenju
LicenseLabel=Molimo vas, prije nastavka, pažljivo proèitajte slijedeæe važne informacije.
LicenseLabel3=Molimo vas, pažljivo proèitajte Ugovor o korištenju. Morate prihvatiti uvjete ugovora kako bi mogli nastaviti s instalacijom.
LicenseAccepted=&Prihvaæam ugovor
LicenseNotAccepted=&Ne prihvaæam ugovor

; *** "Information" wizard pages
WizardInfoBefore=Informacija
InfoBeforeLabel=Molimo vas, proèitajte slijedeæe važne informacije prije nastavka.
InfoBeforeClickLabel=Kada budete spremni nastaviti instalaciju odaberite Nastavak.
WizardInfoAfter=Informacija
InfoAfterLabel=Molimo vas, proèitajte slijedeæe važne informacije prije nastavka.
InfoAfterClickLabel=Kada budete spremni nastaviti instalaciju odaberite Nastavak.

; *** "User Information" wizard page
WizardUserInfo=Informacije o korisniku
UserInfoDesc=Upišite informacije o vama.
UserInfoName=&Ime korisnika:
UserInfoOrg=&Organizacija:
UserInfoSerial=&Serijski broj:
UserInfoNameRequired=Morate upisati ime.

; *** "Select Destination Directory" wizard page
WizardSelectDir=Odaberite odredišni direktorij
SelectDirDesc=Direktorij gdje æe biti instaliran program.
SelectDirLabel3=Instalacija æe instalirati [name] u slijedeæi direktorij
SelectDirBrowseLabel=Za nastavak kliknite na Nastavak. Za odabrir drugog direktorija kliknite na Odaberi
DiskSpaceMBLabel=Ovaj program zahtijeva minimalno [mb] MB slobodnog prostora na disku.
ToUNCPathname=Instalacija ne može instalirati na UNC stazu. Ako pokušavate instalirati na mrežu, morate mapirati mrežni disk.
InvalidPath=Morate unijeti punu stazu zajedno sa slovom diska; npr:%nC:\APP
InvalidDrive=Disk koji ste odabrali ne postoji. Odaberite neki drugi.
DiskSpaceWarningTitle=Nedovoljno prostora na disku
DiskSpaceWarning=Instalacija zahtijeva minimalno %1 KB slobodnog prostora, a odabrani disk ima samo %2 KB na raspolaganju.%n%nŽelite li nastaviti?
DirNameTooLong=Predugi naziv direktorija ili staze.
InvalidDirName=Naziv direktorija je pogrešan.
BadDirName32=Naziv direktorija ne smije sadržavati niti jedan od slijedeæih znakova nakon toèke:%n%n%1
DirExistsTitle=Direktorij veæ postoji
DirExists=Direktorij:%n%n%1%n%nveæ postoji. Želite li svejedno instalirati u njega?
DirDoesntExistTitle=Direktorij ne postoji
DirDoesntExist=Direktorij:%n%n%1%n%nne postoji. Želite li ga napraviti?

; *** "Select Components" wizard page
WizardSelectComponents=Odaberite komponente
SelectComponentsDesc=Koje komponente želite instalirati?
SelectComponentsLabel2=Odaberite komponente koje želite instalirati ili uklonite kvaèicu uz komponente koje ne želite:
FullInstallation=Puna instalacija

; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Kompakt (minimalna) instalacija
CustomInstallation=Instalacija prema želji
NoUninstallWarningTitle=Postojeæe komponente
NoUninstallWarning=Instalacija je utvrdila da na vašem raèunalu veæ postoje slijedeæe komponente:%n%n%1%n%nNeodabir tih komponenata ne dovodi do njihove deinstalacije.%n%nŽelite li ipak nastaviti?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Vaš izbor zahtijeva najmanje [mb] MB prostora na disku.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Odaberite radnje
SelectTasksDesc=Koje radnje želite izvršiti?
SelectTasksLabel2=Odaberite radnje koje æe se izvršiti tijekom instalacije programa [name]:

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Odaberite programsku grupu
SelectStartMenuFolderDesc=Lokacija preèice programa
SelectStartMenuFolderLabel3=Instalacija æe kreirati preèice za programe u slijedeæem Start Menu direktoriju
SelectStartMenuFolderBrowseLabel=Za nastavak kliknite na Nastavak. Za odabir drugog direktorija kliknite na Odabir.
MustEnterGroupName=Morate unijeti naziv programske grupe.
GroupNameTooLong=Predugi naziv direktorija ili staze.
InvalidGroupName=Naziv direktorija je pogrešan.
BadGroupName=Naziv programske grupe ne smije sadržavati slijedeæe znakove:%n%n%1
NoProgramGroupCheck2=&Ne kreiraj programsku grupu u »Start« izborniku

; *** "Ready to Install" wizard page
WizardReady=Spreman za instalaciju
ReadyLabel1=Instalacija je spremna instalirati [name] na vaše raèunalo.
ReadyLabel2a=Kliknite na Instaliraj ako želite instalirati program ili na Nazad ako želite pregledati ili promjeniti postavke.
ReadyLabel2b=Kliknite na Instaliraj ako želite instalirati program.
ReadyMemoUserInfo=Informacije o korisniku:
ReadyMemoDir=Odredišni direktorij:
ReadyMemoType=Tip instalacije:
ReadyMemoComponents=Odabrane komponente:
ReadyMemoGroup=Programska grupa:
ReadyMemoTasks=Dodatni zadaci:

; *** "Preparing to Install" wizard page
WizardPreparing=Pripremam instalaciju
PreparingDesc=Priprema se instalacija [name] na vaše raèunalo.
PreviousInstallNotCompleted=Instalacija/deinstalacija prethodnog programa nije završena. Morate restartati raèunalo kako bi završili tu instalaciju.%n%nNakon restartanja raèunala, ponovno pokrenite Setup kako bi dovršili instalaciju [name].
CannotContinue=Instalacija ne može nastaviti. Molimo kliknite na Odustani za izlaz.

; *** "Installing" wizard page
WizardInstalling=Instaliram
InstallingLabel=Prièekajte dok ne završi instalacija programa [name] na vaše raèunalo.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=Završavam instalaciju [name]
FinishedLabelNoIcons=Instalacija programa [name] je završena.
FinishedLabel=Instalacija programa [name] je završena. Program možete pokrenuti preko instaliranih ikona.
ClickFinish=Kliknite na Završi za završetak instalacije.
FinishedRestartLabel=Za završetak instalacije programa [name] potrebno je ponovno pokrenuti raèunalo. Želite li to uèiniti odmah?
FinishedRestartMessage=Završetak instalacije programa [name] zahtijeva ponovno pokretanje raèunala.%n%nŽelite li to uèiniti odmah?
ShowReadmeCheck=Da, želim proèitati README datoteku.
YesRadio=&Da, želim odmah ponovno pokrenuti raèunalo
NoRadio=&Ne, kasnije æu ga ponovno pokrenuti

; used for example as 'Run MyProg.exe'
RunEntryExec=Pokreni %1

; used for example as 'View Readme.txt'
RunEntryShellExec=Pogledati %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Instalacija treba slijedeæi disk
SelectDiskLabel2=Umetnite disketu %1 i kliknite na OK.%n%nAko se datoteke s ove diskete nalaze na nekom drugom mediju %2 , upišite ispravnu stazu do njega ili kliknite na Odaberi.
PathLabel=&Staza:
FileNotInDir2=Datoteka "%1" ne postoji u "%2". Molimo vas ubacite odgovorajuæi disk ili odaberete drugi %3.
SelectDirectoryLabel=Molimo vas odaberite lokaciju slijedeæeg diska.

; *** Installation phase messages
SetupAborted=Instalacija nije završena.%n%nMolimo vas, riješite problem i opet pokrenite instalaciju.
EntryAbortRetryIgnore=Kliknite na Ponovi za ponavljanje, Ignoriraj za nastavak, ili Prekid za prekid instalacije.

; *** Installation status messages
StatusCreateDirs=Kreiram direktorije...
StatusExtractFiles=Izdvajam datoteke...
StatusCreateIcons=Kreiram ikone...
StatusCreateIniEntries=Kreiram INI datoteke...
StatusCreateRegistryEntries=Kreiram podatke za registry...
StatusRegisterFiles=Registriram datoteke...
StatusSavingUninstall=Snimam deinstalacijske informacije...
StatusRunProgram=Završavam instalaciju...
StatusRollback=Poništavam promjene...

; *** Misc. errors
ErrorInternal2=Interna greška: %1
ErrorFunctionFailedNoCode=%1 nije uspjelo
ErrorFunctionFailed=%1 nije uspjelo; code %2
ErrorFunctionFailedWithMessage=%1 nije uspjelo; code %2.%n%3
ErrorExecutingProgram=Ne mogu izvršiti datoteku:%n%1

; *** Registry errors
ErrorRegOpenKey=Greška pri otvaranju registry kljuèa:%n%1\%2
ErrorRegCreateKey=Greška pri kreiranju registry kljuèa:%n%1\%2
ErrorRegWriteKey=Greške pri pisanju u registry kljuè:%n%1\%2

; *** INI errors
ErrorIniEntry=Greška pri kreiranju INI podataka u datoteci "%1".

; *** File copying errors
FileAbortRetryIgnore=Kliknite Ponovi za ponavljanje, Ignoriraj za preskakanje datoteke (ne preporuèa se), ili Prekid za prekid instalacije.
FileAbortRetryIgnore2=Kliknite Ponovi za ponavljanje, Ignoriraj za nastavak u svakom sluèaju (ne preporuèa se), ili Prekid za prekid instalacije
SourceIsCorrupted=Izvorišna datoteka je ošteæena
SourceDoesntExist=Izvorišna datoteka "%1" ne postoji
ExistingFileReadOnly=Postojeæa datoteka je oznaèena "samo-za-èitanje".%n%nKliknite Ponovi za ponovni pokušaj nakon uklanjanja oznake "samo-za-èitanje", Ignoriraj za preskakanje datoteke, ili Prekid za prekid instalacije.
ErrorReadingExistingDest=Pojavila se greška prilikom pokušaja èitanja postojeæe datoteke:
FileExists=Datoteka veæ postoji.%n%nŽelite li pisati preko nje?
ExistingFileNewer=Postojeæa datoteka je novija od one koju pokušavate instalirati. Preporuèa se zadržati postojeæu datoteku.%n%nŽelite li zadržati postojeæu datoteku?
ErrorChangingAttr=Pojavila se greška prilikom pokušaja promjene atributa postojeæe datoteke:
ErrorCreatingTemp=Pojavila se greška prilikom pokušaja kreiranja datoteke u odredišnom direktoriju:
ErrorReadingSource=Pojavila se greška prilikom pokušaja èitanja izvorišne datoteke:
ErrorCopying=Pojavila se greška prilikom pokušaja kopiranja datoteke:
ErrorReplacingExistingFile=Pojavila se greška prilikom pokušaja zamjene datoteke:
ErrorRestartReplace=Zamjena nakon ponovnog pokretanja nije uspjela:
ErrorRenamingTemp=Pojavila se greška prilikom pokušaja preimenovanja datoteke u odredisnom direktoriju:
ErrorRegisterServer=Ne mogu registrirati DLL/OCX: %1
ErrorRegSvr32Failed=Greška u RegSvr32: greška %1
;ErrorRegisterServerMissingExport=DllRegisterServer export nije pronaðen
ErrorRegisterTypeLib=Ne mogu registrirati biblioteku tipova: %1

; *** Post-installation errors
ErrorOpeningReadme=Pojavila se greška prilikom pokušaja otvaranja README datoteke.
ErrorRestartingComputer=Instalacija ne može ponovno pokrenuti raèunalo. Molimo vas, uèinite to ruèno.

; *** Uninstaller messages
UninstallNotFound=Datoteka "%1" ne postoji. Deinstalacija prekinuta.
UninstallOpenError=Datoteku "%1" ne mogu otvoriti. Deinstalacija nije moguæa
UninstallUnsupportedVer=Deinstalacijska datoteka "%1" je u formatu koji nije propoznat od ove verzije deinstalera. Deinstalacija nije moguæa
UninstallUnknownEntry=Pronaðen nepoznat zapis (%1) u deinstalacijskoj datoteci
ConfirmUninstall=Jeste li sigurni da želite ukloniti %1 i sve njegove komponente?
UninstallOnlyOnWin64=Ova instalacija može biti uklonjena samo na 64-bitnom Windows operacijskom sustavu
OnlyAdminCanUninstall=Ova instalacija može biti uklonjena samo od strane korisnika s administratorskim privilegijama.
UninstallStatusLabel=Prièekajte dok %1 ne bude uklonjen s vašeg raèunala.
UninstalledAll=Program %1 je uspješno uklonjen sa vašeg raèunala.
UninstalledMost=Deinstalacija programa %1 je završena.%n%nNeke elemente nije bilo moguæe ukloniti. Molimo vas da to uèinite ruèno.
UninstalledAndNeedsRestart=Kako bi završili deinstalaciju %1, vaše raèunalo mora biti ponovno pokrenuto%n%nŽelite li to uèiniti odmah?
UninstallDataCorrupted="%1" datoteka je ošteæena. Deinstalacija nije moguæa.

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Brisanje zajednièke datoteke?
ConfirmDeleteSharedFile2=Sustav ukazuje da slijedeæe zajednièke datoteke ne koristi niti jedan program. Želite li da Deinstalacija ukloni te zajednièke datoteke?%n%nAko neki programi i nadalje koriste te datoteke, a one se obrišu, ti programi neæe ipravno raditi. Ako niste sigurni, odaberite Ne. Ostavljanje datoteka neæe uzrokovati štetu vašem sustavu.
SharedFileNameLabel=Datoteka:
SharedFileLocationLabel=Staza:
WizardUninstalling=Deinstalacija
StatusUninstalling=Deinstaliram %1...
[CustomMessages]
NameAndVersion=%1 verzija %2
AdditionalIcons=Dodatne ikone:
CreateDesktopIcon=Kreiraj ikonu na &desktopu
CreateQuickLaunchIcon=Kreiraj ikonu u Quick Launch izborniku
ProgramOnTheWeb=%1 je na Web-u
UninstallProgram=Deinstaliraj %1
LaunchProgram=Pokreni %1
AssocFileExtension=Pridru&ži %1 sa %2 ekstenzijom datoteke
AssocingFileExtension=Pridružujem %1 sa %2 ekstenzijom datoteke
