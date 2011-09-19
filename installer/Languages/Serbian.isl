; *** Inno Setup version 5.1.11+ Serbian (Latin) messages ***
;
; Vladimir Stefanovic, antivari@gmail.com, 18.10.2008
;
; Note: When translating this text, do not add periods (.) to the end of
; messages that didn't have them already, because on those messages Inno
; Setup adds the periods automatically (appending a period would result in
; two periods being displayed).

[LangOptions]
LanguageName=Srpski
LanguageID=$081A
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
SetupWindowTitle=Instalacija - %1
UninstallAppTitle=Deinstalacija
UninstallAppFullTitle=%1 Deinstalacija

; *** Misc. common
InformationTitle=Informacije
ConfirmTitle=Potvrda
ErrorTitle=Greška

; *** SetupLdr messages
SetupLdrStartupMessage=Ovim programom æete instalirati %1. Da li želite da nastavite?
LdrCannotCreateTemp=Nije moguæe napraviti privremenu datoteku. Instalacija je prekinuta
LdrCannotExecTemp=Nije moguæe pokrenuti datoteku u privremenom direktorijumu. Instalacija je prekinuta

; *** Startup error messages
LastErrorMessage=%1.%n%nGreška %2: %3
SetupFileMissing=Datoteka %1 nedostaje u instalacionom direktorijumu. Molimo Vas ispravite problem ili nabavite novu kopiju programa.
SetupFileCorrupt=Instalacione datoteke su neispravne. Molimo Vas da nabavite novu kopiju programa.
SetupFileCorruptOrWrongVer=Instalacione datoteke su neispravne, ili nisu usaglašene sa ovom verzijom instalacije. Molimo Vas ispravite problem ili nabavite novu kopiju programa.
NotOnThisPlatform=Ovaj program se neæe pokrenuti na %1.
OnlyOnThisPlatform=Ovaj program se mora pokrenuti na %1.
OnlyOnTheseArchitectures=Ovaj program se može instalirati samo na verzijama Windows-a projektovanim za sledeæe procesorske arhitekture:%n%n%1
MissingWOW64APIs=Verzija Windows-a koju koristite ne sadrži moguænosti potrebne za instalacionu proceduru da uradi 64-bitnu instalaciju. Da bi rešili ovaj problem, molimo instalirajte Service Pack %1.
WinVersionTooLowError=Ovaj program zahteva %1 verziju %2 ili noviju.
WinVersionTooHighError=Ovaj program se ne može instalirati na %1 verziji %2 ili novijoj.
AdminPrivilegesRequired=Morate biti prijavljeni kao administrator da bi ste instalirali ovaj program.
PowerUserPrivilegesRequired=Morate biti prijavljeni kao administrator ili kao èlan Power Users grupe kada instalirate ovaj program.
SetupAppRunningError=Instalacija je otkrila da se %1 trenutno izvršava.%n%nMolimo da odmah zatvorite sve njegove instance, a zatim pritisnite OK za nastavak, ili Cancel da odustanete.
UninstallAppRunningError=Deinstalacija je otkrila da se %1 trenutno izvršava.%n%nMolimo da odmah zatvorite sve njegove instance, a zatim pritisnite OK za nastavak, ili Cancel da odustanete.

; *** Misc. errors
ErrorCreatingDir=Instalacija nije mogla da napravi direktorijum "%1"
ErrorTooManyFilesInDir=Nije moguæe napraviti datoteku u direktorijumu "%1" zato što sadrži previše datoteka

; *** Setup common messages
ExitSetupTitle=Prekidanje instalacije
ExitSetupMessage=Instalacija nije završena. Ako sada prekinete Instalaciju, program neæe biti instaliran.%n%nInstalaciju možete pokrenuti i dovršiti nekom dugom prilikom.%n%nPrekid instalacije?
AboutSetupMenuItem=&O instalaciji...
AboutSetupTitle=O instalaciji
AboutSetupMessage=%1 verzija %2%n%3%n%n%1 matièna stranica:%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< &Nazad
ButtonNext=&Dalje >
ButtonInstall=&Instaliraj
ButtonOK=OK
ButtonCancel=Odustani
ButtonYes=&Da
ButtonYesToAll=Da za &Sve
ButtonNo=&Ne
ButtonNoToAll=N&e za Sve
ButtonFinish=&Završetak
ButtonBrowse=&Izaberi...
ButtonWizardBrowse=I&zaberi...
ButtonNewFolder=&Napravi novi direktorijum

; *** "Select Language" dialog messages
SelectLanguageTitle=Izaberite jezik instalacije
SelectLanguageLabel=Izaberite jezik koji želite da koristite prilikom instalacije:

; *** Common wizard text
ClickNext=Pritisnite Dalje da nastavite, ili Odustani da napustite instalaciju.
BeveledLabel=
BrowseDialogTitle=Izaberite direktorijum
BrowseDialogLabel=Izaberite jedan od ponuðenih direktorijuma iz liste, a zatim pritisnite OK.
NewFolderName=Novi Direktorijum

; *** "Welcome" wizard page
WelcomeLabel1=Dobrodošli u [name] instalacionu proceduru
WelcomeLabel2=Sada æe se instalirati [name/ver] na Vaš raèunar.%n%nPreporuèuje se da zatvorite sve druge programe pre nastavka.

; *** "Password" wizard page
WizardPassword=Šifra
PasswordLabel1=Ova instalacija je zaštiæena šifrom.
PasswordLabel3=Molimo upišite šifru, a zatim pritisnite Dalje za nastavak. Vodite raèuna da su velika i mala slova u šifri bitna.
PasswordEditLabel=&Šifra:
IncorrectPassword=Šifra koju ste upisali nije ispravna. Molimo pokušajte ponovo.

; *** "License Agreement" wizard
WizardLicense=Ugovor o korišæenju
LicenseLabel=Molimo proèitajte pažljivo sledeæe važne informacije pre nastavka.
LicenseLabel3=Molimo proèitajte Ugovor o korišæenju, koji sledi. Morate prihvatiti uslove ovog ugovora pre nastavka instalacije.
LicenseAccepted=&Prihvatam ugovor
LicenseNotAccepted=&Ne prihvatam ugovor

; *** "Information" wizard pages
WizardInfoBefore=Informacije
InfoBeforeLabel=Molimo proèitajte pažljivo sledeæe važne informacije pre nastavka.
InfoBeforeClickLabel=Kada budete spremni da nastavite instalaciju, pritisnite Dalje.
WizardInfoAfter=Informacije
InfoAfterLabel=Molimo Vas proèitajte pažljivo sledeæe važne informacije pre nastavka.
InfoAfterClickLabel=Kada budete spremni da nastavite instalaciju, pritisnite Dalje.

; *** "User Information" wizard page
WizardUserInfo=Podaci o korisniku
UserInfoDesc=Molimo unesite Vaše podatke.
UserInfoName=&Korisnik:
UserInfoOrg=&Organizacija:
UserInfoSerial=&Serijski broj:
UserInfoNameRequired=Morate upisati ime.

; *** "Select Destination Location" wizard page
WizardSelectDir=Izaberite odredišnu lokaciju
SelectDirDesc=Gde [name] treba da se instalira?
SelectDirLabel3=Instalacija æe postaviti [name] u sledeæi direktorijum.
SelectDirBrowseLabel=Da nastavite, pritisnite Dalje. Ako želite da izaberete neki drugi direktorijum, pritisnite Izaberi.
DiskSpaceMBLabel=Potrebno je najmanje [mb] MB slobodnog prostora na disku.
ToUNCPathname=Putanja za instalaciju ne sme biti u UNC obliku. Ako pokušavate da instalirate program na mrežu, moraæete prethodno da mapirate mrežni disk.
InvalidPath=Morate upisati punu putanju sa obeležjem diska; na primer:%n%nC:\APP%n%nili UNC putanja u obliku:%n%n\\server\share
InvalidDrive=Disk ili UNC putanja koju ste izabrali ne postoje ili nisu dostupni. Molimo izaberite nešto drugo.
DiskSpaceWarningTitle=Nema dovoljno prostora na disku
DiskSpaceWarning=Instalacija zahteva najmanje %1 KB slobodnog prostora, a izabrani disk ima samo %2 KB na raspolaganju.%n%nDa li ipak želite da nastavite?
DirNameTooLong=Naziv direktorijuma ili putanja su predugaèki.
InvalidDirName=Naziv direktorijuma nije ispravan.
BadDirName32=Nazivi direktorijuma ne smeju imati bilo koje od sledeæih slova:%n%n%1
DirExistsTitle=Direktorijum postoji
DirExists=Direktorijum:%n%n%1%n%nveæ postoji. Da li ipak želite da program instalirate u njemu?
DirDoesntExistTitle=Direktorijum ne postoji
DirDoesntExist=Direktorijum:%n%n%1%n%nne postoji. Da li želite da ga napravim?

; *** "Select Components" wizard page
WizardSelectComponents=Izaberite komponente
SelectComponentsDesc=Koje komponente æete instalirati?
SelectComponentsLabel2=Izaberite komponente koje želite da instalirate; obrišite komponente koje ne želite da instalirate. Pritisnite Dalje kada budete spremni da nastavite.
FullInstallation=Puna instalacija
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Kompaktna instalacija
CustomInstallation=Instalacija samo željenih komponenti
NoUninstallWarningTitle=Komponente postoje
NoUninstallWarning=Instalacija je otkrila da sledeæe komponente veæ postoje na Vašem raèunaru:%n%n%1%n%nNeodabiranje ovih komponenti ih neæe ukloniti.%n%nDa li ipak želite da nastavite?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Trenutno odabrane stavke zahtevaju najmanje [mb] MB prostora na disku.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Izaberite dodatne zadatke
SelectTasksDesc=Kakve dodatne zadatke je još potrebno obaviti?
SelectTasksLabel2=Izaberite dodatne zadatke koje želite da Instalacija [name] obavi, a zatim pritisnite Dalje.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Izaberite direktorijum za Start meni
SelectStartMenuFolderDesc=Gde želite da instalacija postavi preèice za program?
SelectStartMenuFolderLabel3=Instalacija æe postaviti preèice za program u sledeæem direktorijumu Start menija.
SelectStartMenuFolderBrowseLabel=Da nastavite, pritisnite Dalje. Ako želite da izaberete neki drugi direktorijum, pritisnite Izaberi.
MustEnterGroupName=Morate upisati naziv direktorijuma.
GroupNameTooLong=Naziv direktorijuma ili putanja su predugaèki.
InvalidGroupName=Naziv direktorijuma nije ispravan.
BadGroupName=Naziv direktorijuma ne sme imati bilo koje od sledeæih slova:%n%n%1
NoProgramGroupCheck2=&Nemoj da praviš direktorijum u Start meniju

; *** "Ready to Install" wizard page
WizardReady=Instalacija je spremna
ReadyLabel1=Instalacija je spremna da postavi [name] na Vaš raèunar.
ReadyLabel2a=Pritisnite Instaliraj da nastavite sa instalacijom, ili pritisnite Nazad ako želite da ponovo pregledate ili promenite neka podešavanja.
ReadyLabel2b=Pritisnite Instaliraj da nastavite sa instalacijom.
ReadyMemoUserInfo=Podaci o korisniku:
ReadyMemoDir=Odredišna lokacija:
ReadyMemoType=Tip instalacije:
ReadyMemoComponents=Izabrane komponente:
ReadyMemoGroup=Direktorijum Start menija:
ReadyMemoTasks=Dodatni poslovi:

; *** "Preparing to Install" wizard page
WizardPreparing=Priprema za instalaciju
PreparingDesc=Instalacija se priprema da postavi [name] na Vaš raèunar.
PreviousInstallNotCompleted=Instalacija/uklanjanje prethodnog programa nije završena. Potrebno je da restartujete Vaš raèunar da bi se instalacija završila.%n%nNakon restartovanja raèunara, pokrenite ponovo Instalaciju [name] da bi ste je dovršili.
CannotContinue=Instalacija se ne može nastaviti. Molimo pritisnite Odustani da izaðete.

; *** "Installing" wizard page
WizardInstalling=Instaliranje
InstallingLabel=Molimo saèekajte dok se [name] instalira na Vaš raèunar.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=[name] - završetak instalacije
FinishedLabelNoIcons=Instalacija programa [name] na Vaš raèunar je završena.
FinishedLabel=Instalacija programa [name] na Vaš raèunar je završena. Program možete pokrenuti preko postavljenih ikona.
ClickFinish=Pritisnite Završetak da izaðete.
FinishedRestartLabel=Da bi se instalacija [name] dovršila, mora se restartovati raèunar. Da li želite da ga restartujete odmah?
FinishedRestartMessage=Da bi se instalacija [name] dovršila, mora se restartovati raèunar.%n%nDa li želite da ga restartujete odmah?
ShowReadmeCheck=Da, želim da pogledam README datoteku
YesRadio=&Da, restartuj raèunar odmah
NoRadio=&Ne, restartovaæu raèunar kasnije
; used for example as 'Run MyProg.exe'
RunEntryExec=Pokreni %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Pogledaj %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Instalaciji je potreban naredni disk
SelectDiskLabel2=Molimo stavite Disk %1 i pritisnite OK.%n%nAko se datoteke na ovom disku mogu pronaæi u nekom drugom direktorijumu nego što je ispod prikazano, upišite odgovarajuæu putanju ili pritisnite Izaberi.
PathLabel=&Putanja:
FileNotInDir2=Datoteka "%1" se ne može pronaæi u "%2". Molimo stavite pravi disk ili izaberite drugi direktorijum.
SelectDirectoryLabel=Molimo izaberite lokaciju narednog diska.

; *** Installation phase messages
SetupAborted=Instalacija nije završena.%n%nMolimo ispravite problem i pokrenite Instalaciju ponovo.
EntryAbortRetryIgnore=Pritisnite Retry da probate ponovo, Ignore da nastavite u svakom sluèaju, ili Abort da prekinete instalaciju.

; *** Installation status messages
StatusCreateDirs=Pravljenje direktorijuma...
StatusExtractFiles=Raspakivanje datoteka...
StatusCreateIcons=Postavljanje preèica...
StatusCreateIniEntries=Postavljanje INI upisa...
StatusCreateRegistryEntries=Postavljanje Registry upisa...
StatusRegisterFiles=Prijavljivanje datoteka...
StatusSavingUninstall=Beleženje podataka za deinstalaciju...
StatusRunProgram=Završavanje instalacije...
StatusRollback=Poništavanje izvršenih izmena i vraæanje na prethodno stanje...

; *** Misc. errors
ErrorInternal2=Interna greška: %1
ErrorFunctionFailedNoCode=%1 nije uspelo
ErrorFunctionFailed=%1 nije uspelo; kod %2
ErrorFunctionFailedWithMessage=%1 nije uspelo; kod %2.%n%3
ErrorExecutingProgram=Nije moguæe pokrenuti datoteku:%n%1

; *** Registry errors
ErrorRegOpenKey=Greška pri otvaranju Registry kljuèa:%n%1\%2
ErrorRegCreateKey=Greška pri postavljanju Registry kljuèa:%n%1\%2
ErrorRegWriteKey=Greška pri upisu Registry kljuèa:%n%1\%2

; *** INI errors
ErrorIniEntry=Greška pri upisu u INI datoteku "%1".

; *** File copying errors
FileAbortRetryIgnore=Pritisnite Retry da probate ponovo, Ignore da preskoèite ovu datoteku (nije preporuèljivo), ili Abort da prekinete instalaciju.
FileAbortRetryIgnore2=Pritisnite Retry da probate ponovo, Ignore da nastavite u svakom sluèaju (nije preporuèljivo), ili Abort da prekinete instalaciju.
SourceIsCorrupted=Izvorna datoteka je neispravna
SourceDoesntExist=Izvorna datoteka "%1" ne postoji
ExistingFileReadOnly=Postojeæa datoteka je oznaèena 'samo za èitanje'.%n%nPritisnite Retry da uklonite atribut 'samo za èitanje' i probate ponovo, Ignore da preskoèite ovu datoteku, ili Abort da prekinete instalaciju.
ErrorReadingExistingDest=Došlo je do greške prilikom pokušaja èitanja sledeæe datoteke:
FileExists=Datoteka veæ postoji.%n%nDa li želite da je Instalacija zameni?
ExistingFileNewer=Postojeæa datoteka je novija od one koju Instalacija treba da postavi. Preporuèuje se da zadržite postojeæu datoteku.%n%nDa li želite da saèuvate postojeæu datoteku?
ErrorChangingAttr=Došlo je do greške prilikom pokušaja promene atributa za sledeæu datoteku:
ErrorCreatingTemp=Došlo je do greške prilikom pokušaja pravljenja datoteke u odredišnom direktorijumu:
ErrorReadingSource=Došlo je do greške prilikom pokušaja èitanja izvorne datoteke:
ErrorCopying=Došlo je do greške prilikom pokušaja kopiranja datoteke:
ErrorReplacingExistingFile=Došlo je do greške prilikom pokušaja zamene postojeæe datoteke:
ErrorRestartReplace=RestartReplace nije uspeo:
ErrorRenamingTemp=Došlo je do greške prilikom pokušaja promene naziva datoteke u odredišnom direktorijumu:
ErrorRegisterServer=Nije moguæe registrovati DLL/OCX: %1
ErrorRegSvr32Failed=RegSvr32 nije uspešno izvršen, greška %1
ErrorRegisterTypeLib=Nije uspelo registrovanje 'type library': %1

; *** Post-installation errors
ErrorOpeningReadme=Došlo je do greške prilikom otvaranja README datoteke.
ErrorRestartingComputer=Instalacija nije uspela da restartuje raèunar. Molimo Vas uradite to sami.

; *** Uninstaller messages
UninstallNotFound=Datoteka "%1" ne postoji. Deinstaliranje nije uspelo.
UninstallOpenError=Datoteka "%1" se ne može otvoriti. Deinstaliranje nije uspelo
UninstallUnsupportedVer=Deinstalaciona Log datoteka "%1" je u obliku koji ne prepoznaje ova verzija deinstalera. Deinstaliranje nije uspelo
UninstallUnknownEntry=Nepoznat upis (%1) se pojavio u deinstalacionoj Log datoteci
ConfirmUninstall=Da li ste sigurni da želite da potpuno uklonite %1 i sve njegove komponente?
UninstallOnlyOnWin64=Ovaj program se može deinstalirati samo na 64-bitnom Windows-u.
OnlyAdminCanUninstall=Ovaj program može deinstalirati samo korisnik sa administratorskim pravima.
UninstallStatusLabel=Molimo saèekajte dok %1 ne bude uklonjen sa Vašeg raèunara.
UninstalledAll=%1 je uspešno uklonjen sa Vašeg raèunara.
UninstalledMost=%1 deinstalacija je završena.%n%nNeki delovi se nisu mogli ukloniti. Oni se mogu ukloniti ruèno.
UninstalledAndNeedsRestart=Da dovršite deinstalaciju %1, Vaš raèunar se mora restartovati.%n%nDa li želite da ga restartujete odmah?
UninstallDataCorrupted="%1" datoteka je ošteæena. Deinstaliranje nije uspelo

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Obriši zajednièku datoteku?
ConfirmDeleteSharedFile2=Sistem pokazuje da sledeæe zajednièke datoteke više ne koristi ni jedan program. Da li želite da deinstalacija ukloni ovu zajednièku datoteku?%n%nAko neki programi i dalje koriste ovu datoteku, možda neæe ispravno funkcionisati. Ako niste sigurni, izaberite No. Ostavljanje ove datoteke neæe prouzrokovati nikakvu štetu sistemu.
SharedFileNameLabel=Naziv datoteke:
SharedFileLocationLabel=Putanja:
WizardUninstalling=Stanje deinstalacije
StatusUninstalling=Deinstaliranje %1...

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 verzija %2
AdditionalIcons=Dodatne ikone:
CreateDesktopIcon=Postavi &Desktop ikonu
CreateQuickLaunchIcon=Postavi &Quick Launch ikonu
ProgramOnTheWeb=%1 na Internetu
UninstallProgram=Deinstalacija %1
LaunchProgram=Pokreni %1
AssocFileExtension=&Pridruži %1 sa %2 tipom datoteke
AssocingFileExtension=Pridruživanje %1 sa %2 tipom datoteke...
