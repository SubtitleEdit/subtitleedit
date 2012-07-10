; *** Inno Setup version 5.1.11+ Basque messages ***
;
; Translated by 3ARRANO (3arrano@3arrano.com)
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
LanguageName=Euskara
LanguageID=$042d
LanguageCodePage=0
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
SetupAppTitle=Instalaketa
SetupWindowTitle=Instalaketa - %1
UninstallAppTitle=Desinstalaketa
UninstallAppFullTitle=Desinstalaketa - %1

; *** Misc. common
InformationTitle=Argibideak
ConfirmTitle=Berretsi
ErrorTitle=Akatsa

; *** SetupLdr messages
SetupLdrStartupMessage=Programa honek %1 instalatuko du. Jarraitu nahi duzu?
LdrCannotCreateTemp=Ezin izan da aldi baterako fitxategirik sortu. Instalaketa ezeztatu da
LdrCannotExecTemp=Aldi baterako karpetako fitxategia ezin izan da abiarazi. Instalaketa ezeztatu da
; *** Startup error messages
LastErrorMessage=%1.%n%n%2 akatsa: %3
SetupFileMissing=Ez da %1 fitxategia aurkitu instalaketaren direktorioan. Zuzendu arazoa edo eskuratu programaren kopia berri bat.
SetupFileCorrupt=Instalaketa fitxategiak hondaturik daude. Eskuratu programaren kopia berri bat.
SetupFileCorruptOrWrongVer=Instalaketa fitxategiak hondaturik daude, edo ez dira instalatzailearen bertsio honekin bateragarriak. Zuzendu arazoa edo eskuratu programaren kopia berri bat.
NotOnThisPlatform=Programa hau ez dabil %1 sistemapean.
OnlyOnThisPlatform=Programa hau %1 sistemapean soilik dabil.
OnlyOnTheseArchitectures=Programa hau ondorengo prozesagailuen arkitekturetarako diseinatu diren Windowsen bertsioetan soilik instala daiteke:%n%n%1
MissingWOW64APIs=Darabilzun Windowsen bertsioak ez dakar 64-biteko instalaketa bat burutzeko instalatzaileak behar duen funtzionalitaterik. Arazo hau konpontzeko, instalatu Service Pack %1 zerbitzu paketea.
WinVersionTooLowError=Programa honek %1 %2 edo bertsio berriagoa behar du.
WinVersionTooHighError=Programa hau ezin da instalatu %1 %2 edo bertsio berriagoan.
AdminPrivilegesRequired=Programa hau instalatzeko administratzaile gisa hasi behar duzu saioa.
PowerUserPrivilegesRequired=Programa hau instalatzeko administratzaile gisa edo Agintedun Erabiltzaileen taldeko kide gisa hasi behar duzu saioa.
SetupAppRunningError=Instalatzaileak une honetan %1 irekita dagoela nabaritu du.%n%nItxi bere leiho guztiak, ondoren klikatu Ados jarraitzeko, edo Utzi irteteko.
UninstallAppRunningError=Instalatzaileak une honetan %1 irekita dagoela nabaritu du.%n%nItxi bere leiho guztiak, ondoren klikatu Ados jarraitzeko, edo Utzi irteteko.

; *** Misc. errors
ErrorCreatingDir=Instalatzaileak ezin izan du "%1" direktorioa sortu
ErrorTooManyFilesInDir=Ezinezkoa izan da "%1" direktorioan fitxategi bat sortzea, fitxategi gehiegi dituelako barnean

; *** Setup common messages
ExitSetupTitle=Instalatzailetik Irten
ExitSetupMessage=Instalaketa ez da burutu. Orain irtenez gero, programa ez da instalatuko.%n%nInstalaketa burutzeko edonoiz ireki dezakezu berriro instalatzailea.%n%nInstalatzailetik Irten?
AboutSetupMenuItem=&Instalatzaileari Buruz...
AboutSetupTitle=Instalatzaileari Buruz
AboutSetupMessage=%1 %2%n%3%n%n%1en webgunea :%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< A&urrekoa
ButtonNext=&Hurrengoa >
ButtonInstall=&Instalatu
ButtonOK=Ados
ButtonCancel=Utzi
ButtonYes=&Bai
ButtonYesToAll=Bai &Guztiari
ButtonNo=&Ez
ButtonNoToAll=E&z Guztiari
ButtonFinish=A&maitu
ButtonBrowse=&Arakatu...
ButtonWizardBrowse=A&rakatu...
ButtonNewFolder=&Karpeta Berria Sortu

; *** "Select Language" dialog messages
SelectLanguageTitle=Hautatu Instalatzailearen Hizkuntza
SelectLanguageLabel=Hautatu instalaketarako erabili nahi duzun hizkuntza:

; *** Common wizard text
ClickNext=Klikatu Hurrengoa jarraitzeko, edo Utzi instalatzailetik irteteko.
BeveledLabel=
BrowseDialogTitle=Karpetak Arakatu
BrowseDialogLabel=Hautatu karpeta bat azpiko zerrendan, ondoren klikatu Ados.
NewFolderName=Karpeta Berria

; *** "Welcome" wizard page
WelcomeLabel1=Ongietorri [name] Instalatzeko Morroira
WelcomeLabel2=Programa honek [name/ver] zure konputagailuan instalatuko du.%n%nGomendagarria da jarraitu aurretik gainontzeko aplikazioak ixtea.

; *** "Password" wizard page
WizardPassword=Pasahitza
PasswordLabel1=Instalaketa hau pasahitzez babesturik dago.
PasswordLabel3=Sartu pasahitza, ondoren klikatu Hurrengoa jarraitzeko. Pasahitzetan maiuskulak bereizten dira.
PasswordEditLabel=&Pasahitza:
IncorrectPassword=Sartu duzun pasahitza ez da zuzena. Saiatu berriro.

; *** "License Agreement" wizard page
WizardLicense=Lizentziaren Onarpena
LicenseLabel=Irakurri ondorengo argibide garrantzitsu hauek jarraitu aurretik.
LicenseLabel3=Irakurri ondorengo Lizentziaren Onarpena. Lizentzia honen baldintzak onartu behar dituzu instalaketaren jarraitu aurretik.
LicenseAccepted=Lizentziaren baldintzak &onartzen ditut
LicenseNotAccepted=&Ez ditut lizentziaren baldintzak onartzen

; *** "Information" wizard pages
WizardInfoBefore=Argibideak
InfoBeforeLabel=Irakurri ondorengo argibide garrantzitsu hauek jarraitu aurretik.
InfoBeforeClickLabel=Instalaketarekin jarraitzeko gertu egotean, klikatu Hurrengoa.
WizardInfoAfter=Argibideak
InfoAfterLabel=Irakurri ondorengo argibide garrantzitsu hauek jarraitu aurretik.
InfoAfterClickLabel=Instalaketarekin jarraitzeko gertu egotean, klikatu Hurrengoa.

; *** "User Information" wizard page
WizardUserInfo=Erabiltzailearen Datuak
UserInfoDesc=Sartu zure datuak.
UserInfoName=&Erabiltzaile Izena:
UserInfoOrg=E&rakundea:
UserInfoSerial=&Serie Zenbakia:
UserInfoNameRequired=Izen bat sartu behar duzu.

; *** "Select Destination Location" wizard page
WizardSelectDir=Hautatu Helburu Direktorioa
SelectDirDesc=Non instalatu beharko litzateke [name]?
SelectDirLabel3=Instalatzaileak [name] ondorengo karpetan instalatuko du.
SelectDirBrowseLabel=Jarraitzeko, klikatu Hurrengoa. Beste karpeta bat hautatu nahi baduzu, klikatu Arakatu.
DiskSpaceMBLabel=Gutxienez [mb] MBko toki hutsa behar da diskan.
ToUNCPathname=Instalatzaileak ezin du UNC bideizen baten instalatu. Sarean instalatzen saiatzen ari bazara, sareko diska bat mapeatu beharko duzu.
InvalidPath=Bideizen oso bat sartu behar duzu, unitate hizki eta guzti; adibidez:%n%nC:\APP%n%nedo UNC bideizen bat honela:%n%n\\zerbitzaria\elkarbanatua
InvalidDrive=Hautatu duzun unitatea edo UNC elkarbanatua ez dago edo ezin da bertara sartu. Hautatu beste bat.
DiskSpaceWarningTitle=Ez Dago Behar Beste Toki Diskan
DiskSpaceWarning=Instalatzaileak gutxienez %1 KBko toki hutsa behar du instalatzeko, baina hautaturiko unitateak %2 KB soilik ditu hutsik.%n%nHala ere jarraitu nahi duzu?
DirNameTooLong=Karpetaren izena edo bideizena luzeegia da.
InvalidDirName=Karpetaren izena ez da zuzena.
BadDirName32=Karpetaren izenak ezin dezake ondorengo karaktereetarik bat ere eduki:%n%n%1
DirExistsTitle=Karpeta Badago
DirExists=Karpeta hau:%n%n%1%n%nlehendik ere badago. Hala ere bertan instalatu nahi duzu?
DirDoesntExistTitle=Karpeta Ez Dago
DirDoesntExist=Karpeta hau:%n%n%1%n%nez dago. Sortu nahi duzu?

; *** "Select Components" wizard page
WizardSelectComponents=Hautatu Osagaiak
SelectComponentsDesc=Zein osagai instalatu behar dira?
SelectComponentsLabel2=Hautatu instalatu nahi dituzun osagaiak; garbitu instalatu nahi ez dituzunak. Klikatu Hurrengoa jarraitzeko gertu egotean.
FullInstallation=Guztia instalatu
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Instalaketa Trinkoa
CustomInstallation=Instalaketa Pertsonalizatua
NoUninstallWarningTitle=Osagai Hauek Badituzu
NoUninstallWarning=Instalatzaileak nabaritu du ondorengo osagaiok jadanik konputagailuan instalaturik dituzula:%n%n%1%n%nOsagai hauek ez aukeratzeak ez ditu desinstalatuko.%n%nHala ere jarraitu nahi duzu?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Uneko aukeraketak gutxienez [mb] MBko toki hutsa behar du diskan.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Hautatu Ataza Gehigarriak
SelectTasksDesc=Zein ataza gehigarri burutu behar dira?
SelectTasksLabel2=Hautatu [name] instalatu bitartean instalatzaileak burutu beharreko ataza gehigarriak, ondoren klikatu Hurrengoa.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Hautatu Hasi Menuko Karpeta
SelectStartMenuFolderDesc=Non sortu behar ditu instalatzaileak programaren lasterbideak?
SelectStartMenuFolderLabel3=Instalatzaileak Hasi Menuko ondorengo karpetan sortuko ditu programaren lasterbideak.
SelectStartMenuFolderBrowseLabel=Jarraitzeko, klikatu Hurrengoa. Beste karpeta bat hautatu nahi baduzu, klikatu Arakatu.
MustEnterGroupName=Karpeta izen bat sartu behar duzu.
GroupNameTooLong=Karpetaren izena edo bideizena luzeegia da.
InvalidGroupName=Karpetaren izena ez da zuzena.
BadGroupName=Karpetaren izenak ezin dezake ondorengo karaktereetarik bat ere eduki:%n%n%1
NoProgramGroupCheck2=&Ez sortu Hasi Menuko karpetarik

; *** "Ready to Install" wizard page
WizardReady=Instalatzeko Gertu
ReadyLabel1=Instalatzailea [name] zure konputagailuan instalatzen hasteko gertu dago.
ReadyLabel2a=Klikatu Instalatu instalaketarekin jarraitzeko, edo klikatu Aurrekoa ezarpenen bat berrikusi edo aldatu nahi baduzu.
ReadyLabel2b=Klikatu Instalatu instalaketarekin jarraitzeko.
ReadyMemoUserInfo=Erabiltzailearen datuak:
ReadyMemoDir=Helburu direktorioa:
ReadyMemoType=Instalaketa mota:
ReadyMemoComponents=Hautaturiko osagaiak:
ReadyMemoGroup=Hasi Menuko karpeta:
ReadyMemoTasks=Ataza gehigarriak:

; *** "Preparing to Install" wizard page
WizardPreparing=Instalatzeko Gertatzen
PreparingDesc=Instalatzailea [name] zure konputagailuan instalatzeko gertatzen ari da.
PreviousInstallNotCompleted=Aurreko programa baten instalaketa/desinstalaketa ez da burutu. Instalaketa hura burutzeko konputagailua berrabiarazi beharko duzu.%n%nKonputagailua berrabiarazi ondoren, ireki instalatzailea berriro [name] instalatzen bukatzeko.
CannotContinue=Ezinezkoa da instalaketarekin jarraitzea. Klikatu Utzi irteteko.

; *** "Installing" wizard page
WizardInstalling=Instalatzen
InstallingLabel=Itxaron instalatzaileak [name] zure konputagailuan instalatu artean.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=[name] Instalatzeko Morroia Burutzen
FinishedLabelNoIcons=Instalatzaileak [name] zure konputagailuan instalatu du.
FinishedLabel=Instalatzaileak [name] zure konputagailuan instalatu du. Aplikazioa abiarazteko instalaturiko ikonoetan klikatu.
ClickFinish=Klikatu Amaitu instalatzailetik irteteko.
FinishedRestartLabel=[name] programaren instalaketa burutzeko, instalatzaileak konputagailua berrabiarazi beharra du. Orain berrabiarazi nahi duzu?
FinishedRestartMessage=[name] programaren instalaketa burutzeko, instalatzaileak konputagailua berrabiarazi beharra du.%n%nOrain berrabiarazi nahi duzu?
ShowReadmeCheck=Bai, IRAKURRI fitxategia ikusi nahi dut
YesRadio=&Bai, berrabiarazi orain
NoRadio=&Ez, beranduago berrabiaraziko dut
; used for example as 'Run MyProg.exe'
RunEntryExec=Abiarazi %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Ikusi %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Instalatzaileak Hurrengo Diska Behar Du
SelectDiskLabel2=Sartu %1. diska eta klikatu Ados.%n%nDiska honetako fitxategiak ez badaude azpian ageri den karpetan, sartu bideizen egokia edo klikatu Arakatu.
PathLabel=&Bideizena:
FileNotInDir2="%1" fitxategia ezin izan da "%2" karpetan aurkitu. Sartu diska zuzena edo hautatu beste karpeta bat.
SelectDirectoryLabel=Zehaztu hurrengo diskaren kokapena.

; *** Installation phase messages
SetupAborted=Instalaketa ez da burutu.%n%nKonpondu arazoa eta abiarazi instalatzailea berriro.
EntryAbortRetryIgnore=Klikatu Saiatu Berriz berriro saiatzeko, Ezikusi hala ere jarraitzeko, edo Utzi instalaketa uzteko.

; *** Installation status messages
StatusCreateDirs=Direktorioak sortzen...
StatusExtractFiles=Fitxategiak ateratzen...
StatusCreateIcons=Lasterbideak sortzen...
StatusCreateIniEntries=INI sarrerak sortzen...
StatusCreateRegistryEntries=Erregistroko sarrerak sortzen...
StatusRegisterFiles=Fitxategiak erregistratzen...
StatusSavingUninstall=Desinstalaketarako datuak gordetzen...
StatusRunProgram=Instalaketa burutzen...
StatusRollback=Aldaketak desegiten...

; *** Misc. errors
ErrorInternal2=Barneko akatsa: %1
ErrorFunctionFailedNoCode=Hutsegitea: %1
ErrorFunctionFailed=Hutsegitea: %1; Kodea: %2
ErrorFunctionFailedWithMessage=Hutsegitea: %1; Kodea: %2.%n%3
ErrorExecutingProgram=Ezin izan da fitxategi hau abiarazi:%n%1

; *** Registry errors
ErrorRegOpenKey=Akatsa erregistroko gakoa irekitzean:%n%1\%2
ErrorRegCreateKey=Akatsa erregistroko gakoa sortzean:%n%1\%2
ErrorRegWriteKey=Akatsa erregistroko gakoa idaztean:%n%1\%2

; *** INI errors
ErrorIniEntry=Akatsa "%1" fitxategian INI sarrera sortzean.

; *** File copying errors
FileAbortRetryIgnore=Klikatu Saiatu Berriz berriro saiatzeko, Ezikusi fitxategi hau saltatzeko (ez da gomendagarria), edo Utzi instalaketa uzteko.
FileAbortRetryIgnore2=Klikatu Saiatu Berriz berriro saiatzeko, Ezikusi hala ere jarraitzeko (ez da gomendagarria), edo Utzi instalaketa uzteko.
SourceIsCorrupted=Iturburu fitxategia hondaturik dago
SourceDoesntExist=Ez dago "%1" izeneko iturburu fitxategirik
ExistingFileReadOnly=Lehendik zegoen fitxategia irakurtzeko-soilik gisa markaturik dago.%n%nKlikatu Saiatu Berriz irakurtzeko-soilik atributua ezabatu eta berriro saiatzeko, Ezikusi fitxategi hau saltatzeko, edo Utzi instalaketa uzteko.
ErrorReadingExistingDest=Akats bat izan da lehendik zegoen fitxategi hau irakurtzean:
FileExists=Fitxategia lehendik ere bazegoen.%n%nInstalatzaileak gainidatzi dezan nahi duzu?
ExistingFileNewer=Lehendik zegoen fitxategia Instalatzaileak instalatu nahi duena baino berriagoa da. Lehendik zegoena mantentzea gomendatzen da.%n%nLehengoa mantendu nahi duzu?
ErrorChangingAttr=Akats bat izan da lehendik zegoen fitxategi honen atributuak aldatzean:
ErrorCreatingTemp=Akats bat izan da ondorengo helburu direktorioan fitxategi bat sortzean:
ErrorReadingSource=Akats bat izan da iturburu fitxategia irakurtzean:
ErrorCopying=Akats bat izan da fitxategi hau kopiatzean:
ErrorReplacingExistingFile=Akats bat izan da lehendik zegoen fitxategi hau ordezkatzean:
ErrorRestartReplace=RestartReplacek huts egin du:
ErrorRenamingTemp=Akats bat izan da ondorengo helburu direktorioan fitxategi bat berrizendatzean:
ErrorRegisterServer=Ezinezkoa izan da DLL/OCX hau erregistratzea: %1
ErrorRegSvr32Failed=RegSvr32k huts egin du %1 itxiera kodea emanez
ErrorRegisterTypeLib=Ezinezkoa izan da moten liburutegi hau erregistratzea: %1

; *** Post-installation errors
ErrorOpeningReadme=Akats bat izan da IRAKURRI fitxategia irekitzean.
ErrorRestartingComputer=Instalatzaileak ezin izan du konputagailua berrabiarazi. Egin ezazu eskuz.

; *** Uninstaller messages
UninstallNotFound=Ez da "%1" fitxategia aurkitu. Ezin izan da desinstalatu.
UninstallOpenError=Ezin izan da "%1" ireki. Ezin izan da desinstalatu
UninstallUnsupportedVer=Desinstalaketarako "%1" log fitxategia instalatzailearen bertsio honek ezagutzen ez duen formatu batean dago. Ezin izan da desinstalatu
UninstallUnknownEntry=Sarrera ezezagun bat (%1) aurkitu da desinstalaketarako logean
ConfirmUninstall=Ziur %1 eta bere osagai guztiak ezabatu nahi dituzula?
UninstallOnlyOnWin64=Instalaketa hau 64-biteko Windowsean soilik desinstala daiteke.
OnlyAdminCanUninstall=Instalaketa hau administratzaile eskumenak dituen erabiltzaile batek soilik desinstala dezake.
UninstallStatusLabel=Itxaron %1 zure konputagailutik ezabatzen den artean.
UninstalledAll=%1 arrakastatsuki ezabatu da zure konputagailutik.
UninstalledMost=%1 desinstalatu da.%n%nZenbait fitxategi ezin izan dira ezabatu. Fitxategi hauek eskuz ezaba daitezke.
UninstalledAndNeedsRestart=%1 guztiz desinstalatzeko, zure konputagailua berrabiarazi beharra dago.%n%nOrain berrabiarazi nahi duzu?
UninstallDataCorrupted="%1" fitxategia hondaturik dago. Ezin izan da desinstalatu

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Fitxategi Elkarbanatua Ezabatu?
ConfirmDeleteSharedFile2=Sistemaren arabera ondorengo fitxategi elkarbanatua ez du inongo programak erabiliko hemendik aurrera. Desinstalatzaileak fitxategi hau ezabatu nahi duzu?%n%nProgramaren bat fitxategi hau erabiltzen badabil oraindik eta ezabatzen baduzu, programa hori ez da egoki ibiliko. Ziur ez bazaude, hautatu Ez. Fitxategia sisteman uzteak ez dizu inongo kalterik eragingo.
SharedFileNameLabel=Fitxategi izena:
SharedFileLocationLabel=Kokapena:
WizardUninstalling=Desinstalaketaren Egoera
StatusUninstalling=Orain desinstalatzen: %1...

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 %2 bertsioa
AdditionalIcons=Ikono gehigarriak:
CreateDesktopIcon=&Mahaigainean lasterbidea sortu
CreateQuickLaunchIcon=&Ataza Barran lasterbidea sortu
ProgramOnTheWeb=%1 sarean
UninstallProgram=%1 desinstalatu
LaunchProgram=%1 abiarazi
AssocFileExtension=&Lotu %1 programa %2 fitxategi luzapenarekin
AssocingFileExtension=%1 programa %2 fitxategi luzapenarekin lotzen...
