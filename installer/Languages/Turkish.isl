; *** Inno Setup version 5.5.3+ Turkish messages ***
; Language	"Turkce" Turkish Translate by "Ceviren"	Kaya Zeren kayazeren@gmail.com
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
LanguageName=T<00FC>rk<00E7>e
LanguageID=$041f
LanguageCodePage=1254
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
SetupAppTitle=Kurulum
SetupWindowTitle=%1 - Kurulumu
UninstallAppTitle=Kald�rma
UninstallAppFullTitle=%1 Kald�rma

; *** Misc. common
InformationTitle=Bilgi
ConfirmTitle=Onay
ErrorTitle=Hata

; *** SetupLdr messages
SetupLdrStartupMessage=%1 yaz�l�m� kurulacak. Devam etmek istiyor musunuz?
LdrCannotCreateTemp=Ge�ici bir dosya olu�turulamad�. Kurulum iptal edildi
LdrCannotExecTemp=Ge�ici klas�rdeki dosya �al��t�r�lamad���ndan kurulum iptal edildi

; *** Startup error messages
LastErrorMessage=%1.%n%nHata %2: %3
SetupFileMissing=Kurulum klas�r�ndeki %1 dosyas� eksik. L�tfen sorunu ��z�n ya da yaz�l�m�n yeni bir kopyas�yla yeniden deneyin.
SetupFileCorrupt=Kurulum dosyalar� bozuk. L�tfen yaz�l�m�n yeni bir kopyas�yla yeniden deneyin.
SetupFileCorruptOrWrongVer=Kurulum dosyalar� bozulmu� ya da bu kurulum s�r�m� ile uyumlu de�il. L�tfen sorunu ��z�n ya da yaz�l�m�n yeni bir kopyas�yla yeniden deneyin.
InvalidParameter=Komut sat�r�ndan ge�ersiz bir parametre g�nderildi:%n%n%1
SetupAlreadyRunning=Kurulum zaten �al���yor.
WindowsVersionNotSupported=Bu yaz�l�m, bilgisayar�n�zda y�kl� olan Windows s�r�m� ile uyumlu de�il.
WindowsServicePackRequired=Bu yaz�l�m, %1 Service Pack %2 ve �zerindeki s�r�mlerle �al���r.
NotOnThisPlatform=Bu yaz�l�m, %1 �zerinde �al��maz.
OnlyOnThisPlatform=Bu yaz�l�m, %1 �zerinde �al��t�r�lmal�d�r.
OnlyOnTheseArchitectures=Bu yaz�l�m, yaln�z �u i�lemci mimarileri i�in tasarlanm�� Windows s�r�mleriyle �al���r:%n%n%1
MissingWOW64APIs=Kulland���n�z Windows s�r�m� 64-bit kurulumu i�in gerekli i�levlere sahip de�il. Bu sorunu ��zmek i�in l�tfen Hizmet Paketi %1 y�kleyin.
WinVersionTooLowError=Bu yaz�l�m i�in %1 s�r�m %2 ya da �zeri gereklidir.
WinVersionTooHighError=Bu yaz�l�m, '%1' s�r�m '%2' ya da �zerine kurulamaz.
AdminPrivilegesRequired=Bu yaz�l�m� kurmak i�in Y�netici olarak oturum a�m�� olman�z gerekir.
PowerUserPrivilegesRequired=Bu yaz�l�m� kurarken, Y�netici ya da G��l� Kullan�c�lar grubunun bir �yesi olarak oturum a�m�� olman�z gerekir.
SetupAppRunningError=Kurulum, %1 yaz�l�m�n�n �al��makta oldu�unu alg�lad�.%n%nL�tfen yaz�l�m�n �al��an t�m kopyalar�n� kapat�p, devam etmek i�in Tamam ya da kurulumdan ��kmak i�in �ptal d��mesine t�klay�n.
UninstallAppRunningError=Kald�rma, %1 yaz�l�m�n�n �al��makta oldu�unu alg�lad�.%n%nL�tfen yaz�l�m�n �al��an t�m kopyalar�n� kapat�p, devam etmek i�in Tamam ya da  kurulumdan ��kmak i�in �ptal d��mesine t�klay�n.

; *** Misc. errors
ErrorCreatingDir=Kurulum "%1" klas�r�n� olu�turamad�.
ErrorTooManyFilesInDir="%1" klas�r� i�inde �ok say�da dosya oldu�undan bir dosya olu�turulamad�

; *** Setup common messages
ExitSetupTitle=Kurulumdan ��k�n
ExitSetupMessage=Kurulum tamamlanmad�. �imdi ��karsan�z, yaz�l�m y�klenmeyecek.%n%nY�klemeyi tamamlamak i�in istedi�iniz zaman kurulum program�n� yeniden �al��t�rabilirsiniz.%n%nKurulumdan ��k�ls�n m�?
AboutSetupMenuItem=Kurulum H&akk�nda...
AboutSetupTitle=Kurulum Hakk�nda
AboutSetupMessage=%1 %2 s�r�m�%n%3%n%n%1 anasayfa:%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< G&eri
ButtonNext=�&leri >
ButtonInstall=&Kurun
ButtonOK=Tamam
ButtonCancel=�ptal
ButtonYes=E&vet
ButtonYesToAll=&T�m�ne Evet
ButtonNo=&Hay�r
ButtonNoToAll=T�m�ne Ha&y�r
ButtonFinish=&Bitti
ButtonBrowse=&G�zat�n...
ButtonWizardBrowse=G�za&t�n...
ButtonNewFolder=Ye&ni Klas�r Olu�turun

; *** "Select Language" dialog messages
SelectLanguageTitle=Kurulum Dilini Se�in
SelectLanguageLabel=Kurulum s�resince kullan�lacak dili se�in:

; *** Common wizard text
ClickNext=Devam etmek i�in �leri, ��kmak i�in �ptal d��mesine bas�n.
BeveledLabel=
BrowseDialogTitle=Klas�re G�zat�n
BrowseDialogLabel=A�a��daki listeden bir klas�r se�ip, Tamam d��mesine t�klay�n.
NewFolderName=Yeni Klas�r

; *** "Welcome" wizard page
WelcomeLabel1=[name] Kurulum Yard�mc�s�na Ho�geldiniz.
WelcomeLabel2=Bilgisayar�n�za [name/ver] yaz�l�m� kurulacak.%n%nDevam etmeden �nce �al��an di�er t�m programlar� kapatman�z �nerilir.

; *** "Password" wizard page
WizardPassword=Parola
PasswordLabel1=Bu kurulum parola korumal�d�r.
PasswordLabel3=L�tfen parolay� yaz�n ve devam etmek i�in �leri d��mesine t�klay�n. Parolalar b�y�k k���k harflere duyarl�d�r.
PasswordEditLabel=&Parola:
IncorrectPassword=Yazd���n�z parola do�ru de�il. L�tfen yeniden deneyin.

; *** "License Agreement" wizard page
WizardLicense=Lisans Anla�mas�
LicenseLabel=L�tfen devam etmeden �nce a�a��daki �nemli bilgileri okuyun.
LicenseLabel3=L�tfen A�a��daki Lisans Anla�mas�n� okuyun. Kuruluma devam edebilmek i�in bu anla�may� kabul etmelisiniz.
LicenseAccepted=Anla�may� kabul &ediyorum.
LicenseNotAccepted=Anla�may� kabul et&miyorum.

; *** "Information" wizard pages
WizardInfoBefore=Bilgiler
InfoBeforeLabel=L�tfen devam etmeden �nce a�a��daki �nemli bilgileri okuyun.
InfoBeforeClickLabel=Kuruluma devam etmeye haz�r oldu�unuzda �leri d��mesine t�klay�n.
WizardInfoAfter=Bilgiler
InfoAfterLabel=L�tfen devam etmeden �nce a�a��daki �nemli bilgileri okuyun.
InfoAfterClickLabel=Kuruluma devam etmeye haz�r oldu�unuzda �leri d��mesine t�klay�n.

; *** "User Information" wizard page
WizardUserInfo=Kullan�c� Bilgileri
UserInfoDesc=L�tfen bilgilerinizi yaz�n.
UserInfoName=K&ullan�c� Ad�:
UserInfoOrg=Ku&rum:
UserInfoSerial=&Seri Numaras�:
UserInfoNameRequired=Bir ad yazmal�s�n�z.

; *** "Select Destination Directory" wizard page
WizardSelectDir=Hedef Klas�r� Se�in
SelectDirDesc=[name] nereye kurulsun?
SelectDirLabel3=[name] yaz�l�m� �u klas�re kurulacak.
SelectDirBrowseLabel=Devam etmek icin �leri d��mesine t�klay�n. Farkl� bir klas�r se�mek i�in G�zat�n d��mesine t�klay�n.
DiskSpaceMBLabel=En az [mb] MB disk alan� gereklidir.
CannotInstallToNetworkDrive=Yaz�l�m bir a� s�r�c�s� �zerine kurulamaz.
CannotInstallToUNCPath=Yaz�l�m bir UNC yolu �zerine (\\yol gibi) kurulamaz.
InvalidPath=S�r�c� ad� ile tam yolu yazmal�s�n�z; �rne�in: %n%nC:\APP%n%n ya da �u bi�imde bir UNC yolu:%n%n\\sunucu\payla��m
InvalidDrive=S�r�c� ya da UNC payla��m� yok ya da eri�ilemiyor. L�tfen ba�ka bir tane se�in.
DiskSpaceWarningTitle=Yeterli Disk Alan� Yok
DiskSpaceWarning=Kurulum i�in %1 KB bo� alan gerekli, ancak se�ilmi� s�r�c�de yaln�z %2 KB bo� alan var.%n%nGene de devam etmek istiyor musunuz?
DirNameTooLong=Klas�r ad� ya da yol �ok uzun.
InvalidDirName=Klas�r ad� ge�ersiz.
BadDirName32=Klas�r adlar�nda �u karakterler bulunamaz:%n%n%1
DirExistsTitle=Klas�r Zaten Var"
DirExists=Klas�r:%n%n%1%n%zaten var. Kurulum i�in bu klas�r� kullanmak ister misiniz?
DirDoesntExistTitle=Klas�r Bulunamad�
DirDoesntExist=Klas�r:%n%n%1%n%nbulunamad�.Klas�r�n olu�turmas�n� ister misiniz?

; *** "Select Components" wizard page
WizardSelectComponents=Bile�enleri Se�in
SelectComponentsDesc=Hangi bile�enler kurulacak?
SelectComponentsLabel2=Kurmak istedi�iniz bile�enleri se�in; kurmak istemedi�iniz bile�enlerin i�aretini kald�r�n. Devam etmeye haz�r oldu�unuzda �leri d��mesine t�klay�n.
FullInstallation=Tam Kurulum
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Normal kurulum
CustomInstallation=�zel kurulum
NoUninstallWarningTitle=Varolan Bile�enler
NoUninstallWarning=Kur �u bile�enlerin bilgisayar�n�za zaten kurulmu� oldu�unu alg�lad�:%n%n%1%n%n Bu bile�enlerin i�aretlerinin kald�r�lmas� bile�enleri kald�rmaz.%n%nGene de devam etmek istiyor musunuz?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Se�ili bile�enler i�in diskte en az [mb] MB bos alan gerekli.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Ek ��lemleri Se�in
SelectTasksDesc=Ba�ka hangi ek i�lemler yap�ls�n?
SelectTasksLabel2=[name] kurulurken yap�lmas�n� istedi�iniz ek i�leri se�in ve �leri d��mesine t�klay�n.

; *** "Ba�lat Men�s� Dizini Se�" sihirbaz sayfas�
WizardSelectProgramGroup=Ba�lat Men�s� Klas�r�n� Se�in
SelectStartMenuFolderDesc=Yaz�l�m�n k�sayollar� nereye kurulsun?
SelectStartMenuFolderLabel3=Kur yaz�l�m k�sayollar�n� a�a��daki Ba�lat Men�s� klas�r�nde olu�turacak.
SelectStartMenuFolderBrowseLabel=Devam etmek i�in �leri d��mesine t�klay�n. Farkl� bir klas�r se�mek i�in G�zat�n d��mesine t�klay�n.
MustEnterGroupName=Bir klas�r ad� yazmal�s�n�z.
GroupNameTooLong=Klas�r ad� ya da yol �ok uzun.
InvalidGroupName=Klas�r ad� ge�ersiz.
BadGroupName=Klas�r ad�nda �u karakterler bulunamaz:%n%n%1
NoProgramGroupCheck2=Ba�lat Men�s� klas�r� &olu�turulmas�n

; *** "Ready to Install" wizard page
WizardReady=Kurulmaya Haz�r
ReadyLabel1=[name] bilgisayar�n�za kurulmaya haz�r.
ReadyLabel2a=Kuruluma devam etmek i�in �leri d��mesine, ayarlar� g�zden ge�irip de�i�tirmek i�in Geri d��mesine t�klay�n.
ReadyLabel2b=Kuruluma devam etmek i�in �leri d��mesine t�klay�n.
ReadyMemoUserInfo=Kullan�c� bilgileri:
ReadyMemoDir=Hedef konumu:
ReadyMemoType=Kurulum tipi:
ReadyMemoComponents=Se�ilmi� bile�enler:
ReadyMemoGroup=Ba�lat Men�s� klas�r�:
ReadyMemoTasks=Ek i�lemler:

; *** "Kurulmaya Haz�r" sihirbaz sayfas�
WizardPreparing=Kuruluma Haz�rlan�l�yor
PreparingDesc=[name] bilgisayar�n�za kurulmaya haz�rlan�yor.
PreviousInstallNotCompleted=�nceki yaz�l�m kurulumu ya da kald�r�lmas� tamamlanmam��. Bu kurulumun tamamlanmas� i�in bilgisayar�n�z� yeniden ba�latmal�s�n�z.%n%nBilgisayar�n�z� yeniden ba�latt�ktan sonra i�lemi tamamlamak i�in [name] kurulumunu yeniden �al��t�r�n.
CannotContinue=Kuruluma devam edilemiyor. ��kmak i�in �ptal d��mesine t�klay�n.
ApplicationsFound=�u uygulamalar, kurulum taraf�ndan g�ncellenmesi gereken dosyalar� kullan�yor. Kurulumun bu uygulamalar� kendili�inden kapatmas�na izin vermeniz �nerilir.
ApplicationsFound2=�u uygulamalar, kurulum taraf�ndan g�ncellenmesi gereken dosyalar� kullan�yor. Kurulumun bu uygulamalar� kendili�inden kapatmas�na izin vermeniz �nerilir. Tamamland�ktan sonra kurulum, uygulamalar� yeniden ba�latmay� deneyecek.
CloseApplications=&Uygulamalar kapat�ls�n
DontCloseApplications=Uygulamalar &kapat�lmas�n
ErrorCloseApplications=Kurulum, uygulamalar� kapatamad�. Kurulum taraf�ndan g�ncellenmesi gereken dosyalar� kullanan uygulamalar� el ile kapatman�z �nerilir.

; *** "Kuruluyor" sihirbaz
WizardInstalling=Kuruluyor
InstallingLabel=L�tfen [name] bilgisayar�n�za kurulurken bekleyin.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=[name] kurulum yard�mc�s� tamamlan�yor
FinishedLabelNoIcons=Bilgisayar�n�za [name] kurulumu tamamland�.
FinishedLabel=Bilgisayar�n�za [name] kurulumu tamamland�. Simgeleri y�klemeyi se�tiyseniz, uygulamay� simgelere t�klayarak ba�latabilirsiniz.
ClickFinish=Kurulumdan ��kmak i�in Bitti d��mesine t�klay�n.
FinishedRestartLabel=[name] kurulumunun tamamlanmas� i�in, bilgisayar�n�z yeniden ba�lat�lmal�. �imdi yeniden ba�latmak ister misiniz?
FinishedRestartMessage=[name] kurulumunun tamamlanmas� i�in, bilgisayar�n�z yeniden ba�lat�lmal�.%n%n�imdi yeniden ba�latmak ister misiniz?
ShowReadmeCheck=Evet README dosyas�na bakmak istiyorum
YesRadio=&Evet, bilgisayar �imdi yeniden ba�lat�ls�n
NoRadio=&Hay�r, bilgisayar� daha sonra yeniden ba�lataca��m
; used for example as 'Run MyProg.exe'
RunEntryExec=%1 �al��t�r�ls�n
; used for example as 'View Readme.txt'
RunEntryShellExec=%1 g�r�nt�lensin

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Kurulum i�in S�radaki Disk Gerekli
SelectDiskLabel2=L�tfen %1. diski tak�p Tamam d��mesine t�klay�n.%n%nDiskteki dosyalar a�a��dakinden farkl� bir klas�rde bulunuyorsa, do�ru yolu yaz�n ya da G�zat�n d��mesine t�klayarak do�ru klas�r� se�in.
PathLabel=&Yol:
FileNotInDir2="%1" dosyas� "%2" i�inde yok. L�tfen do�ru diski tak�n ya da ba�ka bir klas�r se�in.
SelectDirectoryLabel=L�tfen sonraki diskin konumunu belirtin.

; *** Installation phase messages
SetupAborted=Kurulum tamamlanamad�.%n%nL�tfen sorunu d�zelterek kurulumu yeniden �al��t�r�n.
EntryAbortRetryIgnore=Yeniden denemek i�in Yeniden Deneyin d��mesine, devam etmek i�in Yoksay�n d��mesine, kurulumu iptal etmek i�in Vazge�in d��mesine t�klay�n.

; *** Installation status messages
StatusClosingApplications=Uygulamalar kapat�l�yor...
StatusCreateDirs=Klas�rler olu�turuluyor...
StatusExtractFiles=Dosyalar ay�klan�yor...
StatusCreateIcons=K�sayollar olu�turuluyor...
StatusCreateIniEntries=INI kay�tlar� olu�turuluyor...
StatusCreateRegistryEntries=Kay�t Defteri kay�tlar� olu�turuluyor...
StatusRegisterFiles=Dosyalar kaydediliyor...
StatusSavingUninstall=Kald�rma bilgileri kaydediliyor...
StatusRunProgram=Kurulum tamamlan�yor...
StatusRestartingApplications=Uygulamalar yeniden ba�lat�l�yor...
StatusRollback=De�i�iklikler geri al�n�yor...

; *** Misc. errors
ErrorInternal2=�� hata: %1
ErrorFunctionFailedNoCode=%1 tamamlanamad�.
ErrorFunctionFailed=%1 tamamlanamad�; kod %2
ErrorFunctionFailedWithMessage=%1 tamamlanamad�; kod %2.%n%3
ErrorExecutingProgram=�u dosya y�r�t�lemedi:%n%1

; *** Registry errors
ErrorRegOpenKey=Kay�t defteri anahtar� a��l�rken bir hata olu�tu:%n%1%2
ErrorRegCreateKey=Kay�t defteri anahtar� olu�turulurken bir hata olu�tu:%n%1%2
ErrorRegWriteKey=Kay�t defteri anahtar� yaz�l�rken bir hata olu�tu:%n%1%2

; *** INI errors
ErrorIniEntry="%1" dosyas�na INI kayd� eklenirken bir hata olu�tu.

; *** File copying errors
FileAbortRetryIgnore=Yeniden denemek i�in Yeniden Deneyin d��mesine, bu dosyay� atlamak i�in (�nerilmez) Yoksay�n d��mesine, kurulumu iptal etmek i�in Vazge�in d��mesine t�klay�n.
FileAbortRetryIgnore2=Yeniden denemek i�in Yeniden Deneyin d��mesine, devam etmek i�in (�nerilmez) Yoksay�n d��mesine, kurulumu iptal etmek i�in Vazge�in d��mesine t�klay�n.
SourceIsCorrupted=Kaynak dosya bozuk
SourceDoesntExist="%1" kaynak dosyas� bulunamad�
ExistingFileReadOnly=Varolan dosya salt okunabilir olarak i�aretlenmi�.%n%nSalt okunur �zniteli�ini kald�r�p yeniden denemek i�in Yeniden Deneyin d��mesine, bu dosyay� atlamak i�in Yoksay�n d��mesine, kurulumunu iptal etmek i�in Vazge�in d��mesine t�klay�n.
ErrorReadingExistingDest=Varolan dosya okunmaya �al���l�rken bir hata olu�tu.
FileExists=Dosya zaten var.%n%nKurulum bu dosyan�n �zerine yazs�n m�?
ExistingFileNewer=Varolan dosya, kurulum taraf�ndan yaz�lmaya �al���landan daha yeni.Varolan dosyay� koruman�z �nerilir %n%nVarolan dosya korunsun mu?
ErrorChangingAttr=Varolan dosyan�n �znitelikleri de�i�tirilirken bir hata olu�tu:
ErrorCreatingTemp=Hedef klas�rde dosya olu�turulurken bir hata olu�tu:
ErrorReadingSource=Kaynak dosya okunurken bir hata olu�tu:
ErrorCopying=Dosya kopyalan�rken bir hata olu�tu:
ErrorReplacingExistingFile=Varolan dosya de�i�tirilirken bir hata olu�tu.
ErrorRestartReplace=Yeniden ba�latmada de�i�tirme tamamlanamad�:
ErrorRenamingTemp=Hedef klas�rdeki dosyan�n ad� de�i�tirilirken bir hata olu�tu:
ErrorRegisterServer=DLL/OCX kay�t edilemedi: %1
ErrorRegSvr32Failed=RegSvr32 �u kod ile i�lemi tamamlayamad�: %1
ErrorRegisterTypeLib=Tip kitapl��� kaydedilemedi: %1

; *** Post-installation errors
ErrorOpeningReadme=README dosyas� a��l�rken bir hata olu�tu.
ErrorRestartingComputer=Kurulum bilgisayar�n�z� yeniden ba�latam�yor. L�tfen bilgisayar�n�z� yeniden ba�lat�n.

; *** Uninstaller messages
UninstallNotFound="%1" dosyas� bulunamad�. Yaz�l�m kald�r�lam�yor.
UninstallOpenError="%1" dosyas� a��lamad�. Yaz�l�m kald�r�lam�yor.
UninstallUnsupportedVer="%1" kald�rma g�nl�k dosyas�n�n bi�imi, bu kald�r�c� s�r�m� taraf�ndan anla��lamad�. Yaz�l�m kald�r�lam�yor.
UninstallUnknownEntry=Kald�rma g�nl���nde bilinmeyen bir kay�t (%1) bulundu.
ConfirmUninstall=%1 yaz�l�m�n� t�m bile�enleri ile birlikte tamamen kald�rmak istedi�inize emin misiniz?
UninstallOnlyOnWin64=Bu kurulum yaln�z 64-bit Windows �zerinden kald�r�labilir.
OnlyAdminCanUninstall=Bu kurulum yaln�z y�netici haklar�na sahip bir kullan�c� taraf�ndan kald�r�labilir.
UninstallStatusLabel=L�tfen %1 yaz�l�m� bilgisayar�n�zdan kald�r�l�rken bekleyin.
UninstalledAll=%1 yaz�l�m� bilgisayar�n�zdan kald�r�ld�.
UninstalledMost=%1 yaz�l�m� kald�r�ld�.%n%nBaz� bile�enler kald�r�lamad�. Bunlar� el ile silebilirsiniz.
UninstalledAndNeedsRestart=%1 kald�rma i�lemini tamamlamak i�in bilgisayar�n�z yeniden ba�lat�lmal�.%n%n�imdi yeniden ba�latmak ister misiniz?
UninstallDataCorrupted="%1" dosyas� bozulmu�. Kald�r�lam�yor.

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Payla��lan Dosya Silinsin mi?
ConfirmDeleteSharedFile2=Sisteme g�re, payla��lan �u dosya ba�ka bir program taraf�ndan kullan�lm�yor ve kald�r�labilir. Bu payla��lm�� dosyay� silmek ister misiniz?%n%nBa�ka herhangi bir yaz�l�m bu dosyay� halen kullan�yor ise, sildi�inizde di�er yaz�l�m d�zg�n �al��mayabilir. Emin de�ilseniz Hay�r d��mesine t�klay�n. Dosyay� sisteminizde b�rakman�n bir zarar� olmaz.
SharedFileNameLabel=Dosya ad�:
SharedFileLocationLabel=Konum:
WizardUninstalling=Kald�rma Durumu
StatusUninstalling=%1 kald�r�l�yor...

; *** Shutdown block reasons
ShutdownBlockReasonInstallingApp=%1 kuruluyor.
ShutdownBlockReasonUninstallingApp=%1 kald�r�l�yor.

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 %2 s�r�m�
AdditionalIcons=Ek simgeler:
CreateDesktopIcon=Masa�st� simg&esi olu�turulsun
CreateQuickLaunchIcon=H�zl� Ba�lat simgesi &olu�turulsun
ProgramOnTheWeb=%1 Web Sitesi
UninstallProgram=%1 Yaz�l�m�n� Kald�r�n
LaunchProgram=%1 Yaz�l�m� �al��t�r�ls�n
AssocFileExtension=%1 y&az�l�m� ile %2 dosya uzant�s� ili�kilendirilsin
AssocingFileExtension=%1 y&az�l�m� ile %2 dosya uzant�s� ili�kilendiriliyor...
AutoStartProgramGroupDescription=Ba�lang��:
AutoStartProgram=%1 kendili�inden ba�lat�ls�n
AddonHostProgramNotFound=%1 se�ti�iniz klas�rde bulunamad�.%n%nYine de devam etmek istiyor musunuz?
