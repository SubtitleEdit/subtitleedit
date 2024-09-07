; *** Inno Setup version 6.1.0+ Croatian messages ***
; Translated by: Milo Ivir (mail@milotype.de)
; Based on translation by Elvis Gambiraža (el.gambo@gmail.com)
; Based on translation by Krunoslav Kanjuh (krunoslav.kanjuh@zg.t-com.hr)
;
; To download user-contributed translations of this file, go to:
;   https://www.jrsoftware.org/files/istrans/
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
SetupWindowTitle=Instalacija – %1
UninstallAppTitle=Deinstalacija
UninstallAppFullTitle=Deinstalacija programa %1

; *** Misc. common
InformationTitle=Informacija
ConfirmTitle=Potvrda
ErrorTitle=Greška

; *** SetupLdr messages
SetupLdrStartupMessage=Ovime će se instalirati %1. Želiš li nastaviti?
LdrCannotCreateTemp=Nije moguće stvoriti privremenu datoteku. Instalacija je prekinuta
LdrCannotExecTemp=Nije moguće pokrenuti datoteku u privremenoj mapi. Instalacija je prekinuta
HelpTextNote=

; *** Startup error messages
LastErrorMessage=%1.%n%nnGreška %2: %3
SetupFileMissing=Datoteka %1 se ne nalazi u mapi instalacije. Ispravi problem ili nabavi novu kopiju programa.
SetupFileCorrupt=Datoteke instalacije su oštećene. Nabavi novu kopiju programa.
SetupFileCorruptOrWrongVer=Datoteke instalacije su oštećene ili nisu kompatibilne s ovom verzijom instalacije. Ispravi problem ili nabavi novu kopiju programa.
InvalidParameter=Neispravan parametar je prenijet u naredbenom retku:%n%n%1
SetupAlreadyRunning=Instalacija je već pokrenuta.
WindowsVersionNotSupported=Program ne podržava Windows verziju koju koristiš.
WindowsServicePackRequired=Program zahtijeva %1 servisni paket %2 ili noviji.
NotOnThisPlatform=Program neće raditi na %1.
OnlyOnThisPlatform=Program se mora pokrenuti na %1.
OnlyOnTheseArchitectures=Program se može instalirati na Windows verzijama za sljedeće procesorske arhitekture:%n%n%1
WinVersionTooLowError=Program zahtijeva %1 verziju %2 ili noviju.
WinVersionTooHighError=Program se ne može instalirati na %1 verziji %2 ili novijoj.
AdminPrivilegesRequired=Za instaliranje programa moraš biti prijavljen/a kao administrator.
PowerUserPrivilegesRequired=Za instaliranje programa moraš biti prijavljen/a kao administrator ili kao član grupe naprednih korisnika.
SetupAppRunningError=Instalacija je otkrila da je %1 trenutačno pokrenut.%n%nZatvori program i potom pritisni "Dalje" za nastavak ili "Odustani" za prekid.
UninstallAppRunningError=Deinstalacija je otkrila da je %1 trenutačno pokrenut.%n%nZatvori program i potom pritisni "Dalje" za nastavak ili "Odustani" za prekid.

; *** Startup questions
PrivilegesRequiredOverrideTitle=Odaberi način instaliranja
PrivilegesRequiredOverrideInstruction=Odaberi način instaliranja
PrivilegesRequiredOverrideText1=%1 se može instalirati za sve korisnike (potrebna su administratorska prava) ili samo za tebe.
PrivilegesRequiredOverrideText2=%1 se može instalirati samo za tebe ili za sve korisnike (potrebna su administratorska prava).
PrivilegesRequiredOverrideAllUsers=Instaliraj z&a sve korisnike
PrivilegesRequiredOverrideAllUsersRecommended=Instaliraj z&a sve korisnike (preporučeno)
PrivilegesRequiredOverrideCurrentUser=Instaliraj samo za &mene
PrivilegesRequiredOverrideCurrentUserRecommended=Instaliraj samo za &mene (preporučeno)

; *** Misc. errors
ErrorCreatingDir=Instalacija nije mogla stvoriti mapu "%1"
ErrorTooManyFilesInDir=Datoteku nije moguće stvoriti u mapi "%1", jer mapa sadrži previše datoteka

; *** Setup common messages
ExitSetupTitle=Prekini instalaciju
ExitSetupMessage=Instalacija nije završena. Ako sad izađeš, program neće biti instaliran.%n%nInstalaciju možeš pokrenuti kasnije, ukoliko je želiš dovršiti.%n%nPrekinuti instalaciju?
AboutSetupMenuItem=&O instalaciji …
AboutSetupTitle=O instalaciji
AboutSetupMessage=%1 verzija %2%n%3%n%n%1 web-stranica:%n%4
AboutSetupNote=
TranslatorNote=Prevodioci:%n%nKrunoslav Kanjuh%n%nElvis Gambiraža%n%nMilo Ivir

; *** Buttons
ButtonBack=< Na&trag
ButtonNext=&Dalje >
ButtonInstall=&Instaliraj
ButtonOK=U redu
ButtonCancel=Odustani
ButtonYes=&Da
ButtonYesToAll=D&a za sve
ButtonNo=&Ne
ButtonNoToAll=N&e za sve
ButtonFinish=&Završi
ButtonBrowse=&Pretraži …
ButtonWizardBrowse=Odabe&ri …
ButtonNewFolder=&Stvori novu mapu

; *** "Select Language" dialog messages
SelectLanguageTitle=Odaberi jezik za instalaciju
SelectLanguageLabel=Odaberi jezik koji želiš koristiti tijekom instaliranja.

; *** Common wizard text
ClickNext=Pritisni "Dalje" za nastavak ili "Odustani" za prekid instalacije.
BeveledLabel=
BrowseDialogTitle=Odaberi mapu
BrowseDialogLabel=Odaberi mapu iz popisa i pritisni "U redu".
NewFolderName=Nova mapa

; *** "Welcome" wizard page
WelcomeLabel1=Čarobnjak za instalaciju programa [name]
WelcomeLabel2=Ovime ćeš instalirati [name/ver].%n%nPreporučujemo da zatvoriš sve programe prije nego što nastaviš dalje.

; *** "Password" wizard page
WizardPassword=Lozinka
PasswordLabel1=Instalacija je zaštićena lozinkom.
PasswordLabel3=Upiši lozinku i pritisni "Dalje". Lozinke su osjetljive na mala i velika slova.
PasswordEditLabel=&Lozinka:
IncorrectPassword=Upisana je pogrešna lozinka. Pokušaj ponovo.

; *** "License Agreement" wizard page
WizardLicense=Licencni ugovor
LicenseLabel=Prije nego što nastaviš dalje, pažljivo pročitaj sljedeće važne informacije.
LicenseLabel3=Pročitaj licencni ugovor. Moraš prihvatiti uvjete ugovora, ako želiš nastaviti instalirati.
LicenseAccepted=&Prihvaćam ugovor
LicenseNotAccepted=&Ne prihvaćam ugovor

; *** "Information" wizard pages
WizardInfoBefore=Informacije
InfoBeforeLabel=Pročitaj sljedeće važne informacije prije nego što nastaviš dalje.
InfoBeforeClickLabel=Kad želiš nastaviti instalirati, pritisni "Dalje".
WizardInfoAfter=Informacije
InfoAfterLabel=Pročitaj sljedeće važne informacije prije nego što nastaviš dalje.
InfoAfterClickLabel=Kad želiš nastaviti instalirati, pritisni "Dalje".

; *** "User Information" wizard page
WizardUserInfo=Korisnički podaci
UserInfoDesc=Upiši svoje podatke.
UserInfoName=&Ime korisnika:
UserInfoOrg=&Organizacija:
UserInfoSerial=&Serijski broj:
UserInfoNameRequired=Ime je obavezno polje.

; *** "Select Destination Location" wizard page
WizardSelectDir=Odaberi odredišno mjesto
SelectDirDesc=Gdje želiš instalirati [name]?
SelectDirLabel3=[name] će se instalirati u sljedeću mapu.
SelectDirBrowseLabel=Za nastavak instalacije, pritisni "Dalje". Za odabir jedne druge mape, pritisni "Odaberi".
DiskSpaceGBLabel=Potrebno je barem [gb] GB slobodnog prostora na disku.
DiskSpaceMBLabel=Potrebno je barem [mb] MB slobodnog prostora na disku.
CannotInstallToNetworkDrive=Instalacija ne može instalirati na mrežnu jedinicu.
CannotInstallToUNCPath=Instalacija ne može instalirati na UNC stazu.
InvalidPath=Moraš upisati punu stazu zajedno sa slovom diska, npr.:%n%nC:\APP%n%nili UNC stazu u obliku:%n%n\\server\share
InvalidDrive=Odabrani disk ne postoji. Odaberi jedan drugi.
DiskSpaceWarningTitle=Nedovoljno prostora na disku
DiskSpaceWarning=Instalacija treba barem %1 KB slobodnog prostora, no odabrani disk ima samo %2 KB.%n%nSvejedno nastaviti?
DirNameTooLong=Naziv mape ili staze je predugačak.
InvalidDirName=Naziv mape je neispravan.
BadDirName32=Naziv mape ne smije sadržavati sljedeće znakove:%n%n%1
DirExistsTitle=Mapa već postoji
DirExists=Mapa:%n%n%1%n%nveć postoji. Želiš li svejedno u nju instalirati?
DirDoesntExistTitle=Mapa ne postoji
DirDoesntExist=Mapa:%n%n%1%n%nne postoji. Želiš li je stvoriti?

; *** "Select Components" wizard page
WizardSelectComponents=Odaberi komponente
SelectComponentsDesc=Koje komponente želiš instalirati?
SelectComponentsLabel2=Odaberi komponente koje želiš instalirati, isključi komponente koje ne želiš instalirati. Za nastavak instalacije pritisni "Dalje".
FullInstallation=Kompletna instalacija
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Kompaktna instalacija
CustomInstallation=Prilagođena instalacija
NoUninstallWarningTitle=Postojeće komponente
NoUninstallWarning=Instalacija je utvrdila da na tvom računalu već postoje sljedeće komponente:%n%n%1%n%nIsključivanjem tih komponenata, one se neće deinstalirati.%n%nŽeliš li svejedno nastaviti?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceGBLabel=Trenutačni odabir zahtijeva barem [gb] GB na disku.
ComponentsDiskSpaceMBLabel=Trenutačni odabir zahtijeva barem [mb] MB na disku.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Odaberi dodatne zadatke
SelectTasksDesc=Koje dodatne zadatke želiš izvršiti?
SelectTasksLabel2=Odaberi zadatke koje želiš izvršiti tijekom instaliranja programa [name], zatim pritisni "Dalje".

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Odaberi mapu iz "Start" izbornika
SelectStartMenuFolderDesc=Gdje želiš da instalacija spremi programske prečace?
SelectStartMenuFolderLabel3=Instalacija će stvoriti programske prečace u sljedeću mapu "Start" izbornika.
SelectStartMenuFolderBrowseLabel=Ako želiš nastaviti, pritisni "Dalje". Ako želiš odabrati jednu drugu mapu, pritisni "Odaberi".
MustEnterGroupName=Moraš upisati naziv mape.
GroupNameTooLong=Naziv mape ili staze je predugačak.
InvalidGroupName=Naziv mape nije ispravan.
BadGroupName=Naziv mape ne smije sadržavati sljedeće znakove:%n%n%1
NoProgramGroupCheck2=&Ne stvaraj mapu u "Start" izborniku

; *** "Ready to Install" wizard page
WizardReady=Sve je spremno za instaliranje
ReadyLabel1=Instalacija je spremna za instaliranje programa [name].
ReadyLabel2a=Pritisni "Instaliraj", ako želiš instalirati program. Pritisni "Natrag", ako želiš pregledati ili promijeniti postavke
ReadyLabel2b=Pritisni "Instaliraj", ako želiš instalirati program.
ReadyMemoUserInfo=Korisnički podaci:
ReadyMemoDir=Odredišno mjesto:
ReadyMemoType=Vrsta instalacije:
ReadyMemoComponents=Odabrane komponente:
ReadyMemoGroup=Mapa u "Start" izborniku:
ReadyMemoTasks=Dodatni zadaci:

; *** TDownloadWizardPage wizard page and DownloadTemporaryFile
DownloadingLabel=Preuzimanje dodatnih datoteka …
ButtonStopDownload=&Prekini preuzimanje
StopDownload=Stvarno želiš prekinuti preuzimanje?
ErrorDownloadAborted=Preuzimanje je prekinuto
ErrorDownloadFailed=Neuspjelo preuzimanje: %1 %2
ErrorDownloadSizeFailed=Neuspjelo dohvaćanje veličine: %1 %2
ErrorFileHash1=Izračunavanje kontrolnog zbroja datoteke neuspjelo: %1
ErrorFileHash2=Neispravan kontrolni zbroj datoteke: očekivano %1, pronađeno %2
ErrorProgress=Neispravan napredak: %1 od %2
ErrorFileSize=Neispravna veličina datoteke: očekivano %1, pronađeno %2

; *** "Preparing to Install" wizard page
WizardPreparing=Priprema za instaliranje
PreparingDesc=Instalacija se priprema za instaliranje programa [name].
PreviousInstallNotCompleted=Instaliranje/uklanjanje jednog prethodnog programa nije bilo gotovo. Morat ćeš ponovo pokrenuti računalo i dovršiti to instaliranje.%n%nNakon ponovnog pokretanja računala, pokreni instalaciju ponovo, kako bi se dovršilo instaliranje programa [name].
CannotContinue=Instalacija ne može nastaviti rad. Pritisni "Odustani" za izlaz iz instalacije.
ApplicationsFound=Sljedeći programi koriste datoteke koje instalacija mora aktualizirati. Preporučujemo da dopustiš instalaciji zatvoriti ove programe.
ApplicationsFound2=Sljedeći programi koriste datoteke koje instalacija mora aktualizirati. Preporučujemo da dopustiš instalaciji zatvoriti ove programe. Kad instaliranje završi, instalacija će pokušati ponovo pokrenuti programe.
CloseApplications=&Zatvori programe automatski
DontCloseApplications=&Ne zatvaraj programe
ErrorCloseApplications=Instalacija nije uspjela automatski zatvoriti programe. Preporučujemo da zatvoriš sve programe koji koriste datoteke koje se moraju aktualizirati.
PrepareToInstallNeedsRestart=Instalacija mora ponovo pokrenuti računalo. Nakon ponovnog pokretanja računala, pokreni instalaciju ponovo, kako bi se dovršilo instaliranje programa [name].%n%nŽeliš li sada ponovo pokrenuti računalo?

; *** "Installing" wizard page
WizardInstalling=Instaliranje
InstallingLabel=Pričekaj dok ne završi instaliranje programa [name].

; *** "Setup Completed" wizard page
FinishedHeadingLabel=Završavanje instalacijskog čarobnjaka za [name]
FinishedLabelNoIcons=Instalacija je završila instaliranje programa [name].
FinishedLabel=Instalacija je završila instaliranje programa [name]. Program se može pokrenuti pomoću instaliranih prečaca.
ClickFinish=Za izlaz iz instalacije pritisni "Završi".
FinishedRestartLabel=Za završavanje instaliranja programa [name], instalacija mora ponovo pokrenuti računalo. Želiš li sada ponovo pokrenuti računalo?
FinishedRestartMessage=Za završavanje instaliranja programa [name], instalacija mora ponovo pokrenuti računalo.%n%nŽeliš li sada ponovo pokrenuti računalo?
ShowReadmeCheck=Da, želim pročitati README datoteku
YesRadio=&Da, sada ponovo pokrenuti računalo
NoRadio=&Ne, računalo ću kasnije ponovo pokrenuti 
; used for example as 'Run MyProg.exe'
RunEntryExec=Pokreni %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Prikaži %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Instalacija treba sljedeći disk
SelectDiskLabel2=Umetni disk %1 i pritisni "U redu".%n%nAko se datoteke s ovog diska nalaze na nekom drugom mjestu od dolje prikazanog, upiši ispravnu stazu ili pritisni "Odaberi".
PathLabel=&Staza:
FileNotInDir2=Staza "%1" ne postoji u "%2". Umetni odgovarajući disk ili odaberi jednu drugu mapu.
SelectDirectoryLabel=Odredi mjesto sljedećeg diska.

; *** Installation phase messages
SetupAborted=Instalacija nije završena.%n%nIspravi problem i ponovo pokreni instalaciju.
AbortRetryIgnoreSelectAction=Odaberi radnju
AbortRetryIgnoreRetry=&Pokušaj ponovo
AbortRetryIgnoreIgnore=&Zanemari grešku i nastavi
AbortRetryIgnoreCancel=Prekini s instaliranjem

; *** Installation status messages
StatusClosingApplications=Zatvaranje programa …
StatusCreateDirs=Stvaranje mapa …
StatusExtractFiles=Izdvajanje datoteka …
StatusCreateIcons=Stvaranje prečaca …
StatusCreateIniEntries=Stvaranje INI unosa …
StatusCreateRegistryEntries=Stvaranje unosa u registar …
StatusRegisterFiles=Registriranje datoteka …
StatusSavingUninstall=Spremanje podataka deinstalacije …
StatusRunProgram=Završavanje instaliranja …
StatusRestartingApplications=Ponovno pokretanje programa …
StatusRollback=Poništavanje promjena …

; *** Misc. errors
ErrorInternal2=Interna greška: %1
ErrorFunctionFailedNoCode=%1 – neuspjelo
ErrorFunctionFailed=%1 – neuspjelo; kod %2
ErrorFunctionFailedWithMessage=%1 – neuspjelo; kod %2.%n%3
ErrorExecutingProgram=Nije moguće izvršiti datoteku:%n%1

; *** Registry errors
ErrorRegOpenKey=Greška prilikom otvaranja ključa registra:%n%1\%2
ErrorRegCreateKey=Greška prilikom stvaranja ključa registra:%n%1\%2
ErrorRegWriteKey=Greška prilikom pisanja u ključ registra:%n%1\%2

; *** INI errors
ErrorIniEntry=Greška prilikom stvaranja INI unosa u datoteci "%1".

; *** File copying errors
FileAbortRetryIgnoreSkipNotRecommended=&Preskoči ovu datoteku (ne preporučuje se)
FileAbortRetryIgnoreIgnoreNotRecommended=&Zanemari grešku i nastavi (ne preporučuje se)
SourceIsCorrupted=Izvorna datoteka je oštećena
SourceDoesntExist=Izvorna datoteka "%1" ne postoji
ExistingFileReadOnly2=Postojeću datoteku nije bilo moguće zamijeniti, jer je označena sa "samo-za-čitanje".
ExistingFileReadOnlyRetry=&Ukloni svojstvo "samo-za-čitanje" i pokušaj ponovo
ExistingFileReadOnlyKeepExisting=&Zadrži postojeću datoteku
ErrorReadingExistingDest=Pojavila se greška prilikom pokušaja čitanja postojeće datoteke:
FileExistsSelectAction=Odaberi radnju
FileExists2=Datoteka već postoji.
FileExistsOverwriteExisting=&Prepiši postojeću datoteku
FileExistsKeepExisting=&Zadrži postojeću datoteku
FileExistsOverwriteOrKeepAll=&Uradi to i u narednim slučajevima
ExistingFileNewerSelectAction=Odaberi radnju
ExistingFileNewer2=Postojeća datoteka je novija od one koja se pokušava instalirati.
ExistingFileNewerOverwriteExisting=&Prepiši postojeću datoteku
ExistingFileNewerKeepExisting=&Zadrži postojeću datoteku (preporučeno)
ExistingFileNewerOverwriteOrKeepAll=&Uradi to i u narednim slučajevima
ErrorChangingAttr=Pojavila se greška prilikom pokušaja promjene svojstva postojeće datoteke:
ErrorCreatingTemp=Pojavila se greška prilikom pokušaja stvaranja datoteke u odredišnoj mapi:
ErrorReadingSource=Pojavila se greška prilikom pokušaja čitanja izvorišne datoteke:
ErrorCopying=Pojavila se greška prilikom pokušaja kopiranja datoteke:
ErrorReplacingExistingFile=Pojavila se greška prilikom pokušaja zamijenjivanja datoteke:
ErrorRestartReplace=Zamijenjivanje nakon ponovnog pokretanja nije uspjelo:
ErrorRenamingTemp=Pojavila se greška prilikom pokušaja preimenovanja datoteke u odredišnoj mapi:
ErrorRegisterServer=Nije moguće registrirati DLL/OCX: %1
ErrorRegSvr32Failed=Greška u RegSvr32. Izlazni kod %1
ErrorRegisterTypeLib=Nije moguće registrirati biblioteku vrsta: %1

; *** Uninstall display name markings
; used for example as 'My Program (32-bit)'
UninstallDisplayNameMark=%1 (%2)
; used for example as 'My Program (32-bit, All users)'
UninstallDisplayNameMarks=%1 (%2, %3)
UninstallDisplayNameMark32Bit=32-bitni
UninstallDisplayNameMark64Bit=64-bitni
UninstallDisplayNameMarkAllUsers=Svi korisnici
UninstallDisplayNameMarkCurrentUser=Trenutačni korisnik

; *** Post-installation errors
ErrorOpeningReadme=Pojavila se greška prilikom pokušaja otvaranja README datoteke.
ErrorRestartingComputer=Instalacija nije mogla ponovo pokrenuti računalo. Učini to ručno.

; *** Uninstaller messages
UninstallNotFound=Datoteka "%1" ne postoji. Deinstaliranje nije moguće.
UninstallOpenError=Datoteku "%1" nije bilo moguće otvoriti. Deinstaliranje nije moguće
UninstallUnsupportedVer=Deinstalacijska datoteka "%1" je u formatu koji ova verzija deinstalacijskog programa ne prepoznaje. Deinstaliranje nije moguće
UninstallUnknownEntry=Pronađen je nepoznat zapis (%1) u deinstalacijskoj datoteci
ConfirmUninstall=Zaista želiš ukloniti %1 i sve pripadajuće komponente?
UninstallOnlyOnWin64=Ovu instalaciju je moguće ukloniti samo na 64-bitnom Windows sustavu.
OnlyAdminCanUninstall=Ovu instalaciju može ukloniti samo korisnik s administratorskim pravima.
UninstallStatusLabel=Pričekaj dok se %1 uklanja s računala.
UninstalledAll=%1 je uspješno uklonjen s računala.
UninstalledMost=Deinstaliranje programa %1 je završeno.%n%nNeke elemente nije bilo moguće ukloniti. Oni se mogu ukloniti ručno.
UninstalledAndNeedsRestart=Za završavanje deinstaliranja programa %1, potrebno je ponovo pokrenuti računalo.%n%nŽeliš li to sada učiniti?
UninstallDataCorrupted="%1" datoteka je oštećena. Deinstaliranje nije moguće

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Ukloniti dijeljene datoteke?
ConfirmDeleteSharedFile2=Sustav ukazuje na to, da sljedeću dijeljenu datoteku ne koristi niti jedan program. Želiš li ukloniti tu dijeljenu datoteku?%n%nAko neki programi i dalje koriste tu datoteku, a ona se izbriše, ti programi neće ispravno raditi. Ako ne znaš, odaberi "Ne". Datoteka neće štetiti tvom sustavu.
SharedFileNameLabel=Datoteka:
SharedFileLocationLabel=Mjesto:
WizardUninstalling=Stanje deinstalacije
StatusUninstalling=%1 deinstaliranje …

; *** Shutdown block reasons
ShutdownBlockReasonInstallingApp=%1 instaliranje.
ShutdownBlockReasonUninstallingApp=%1 deinstaliranje.

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 verzija %2
AdditionalIcons=Dodatni prečaci:
CreateDesktopIcon=Stvori prečac na ra&dnoj površini
CreateQuickLaunchIcon=Stvori prečac u traci za &brzo pokretanje
ProgramOnTheWeb=%1 na internetu
UninstallProgram=Deinstaliraj %1
LaunchProgram=Pokreni %1
AssocFileExtension=&Poveži program %1 s datotečnim nastavkom %2
AssocingFileExtension=Povezivanje programa %1 s datotečnim nastavkom %2 …
AutoStartProgramGroupDescription=Pokretanje:
AutoStartProgram=Automatski pokreni %1
AddonHostProgramNotFound=%1 nije nađen u odabranoj mapi.%n%nŽeliš li svejedno nastaviti?
