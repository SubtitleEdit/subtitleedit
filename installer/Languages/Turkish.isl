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
UninstallAppTitle=Kaldýrma
UninstallAppFullTitle=%1 Kaldýrma

; *** Misc. common
InformationTitle=Bilgi
ConfirmTitle=Onay
ErrorTitle=Hata

; *** SetupLdr messages
SetupLdrStartupMessage=%1 yazýlýmý kurulacak. Devam etmek istiyor musunuz?
LdrCannotCreateTemp=Geçici bir dosya oluþturulamadý. Kurulum iptal edildi
LdrCannotExecTemp=Geçici klasördeki dosya çalýþtýrýlamadýðýndan kurulum iptal edildi

; *** Startup error messages
LastErrorMessage=%1.%n%nHata %2: %3
SetupFileMissing=Kurulum klasöründeki %1 dosyasý eksik. Lütfen sorunu çözün ya da yazýlýmýn yeni bir kopyasýyla yeniden deneyin.
SetupFileCorrupt=Kurulum dosyalarý bozuk. Lütfen yazýlýmýn yeni bir kopyasýyla yeniden deneyin.
SetupFileCorruptOrWrongVer=Kurulum dosyalarý bozulmuþ ya da bu kurulum sürümü ile uyumlu deðil. Lütfen sorunu çözün ya da yazýlýmýn yeni bir kopyasýyla yeniden deneyin.
InvalidParameter=Komut satýrýndan geçersiz bir parametre gönderildi:%n%n%1
SetupAlreadyRunning=Kurulum zaten çalýþýyor.
WindowsVersionNotSupported=Bu yazýlým, bilgisayarýnýzda yüklü olan Windows sürümü ile uyumlu deðil.
WindowsServicePackRequired=Bu yazýlým, %1 Service Pack %2 ve üzerindeki sürümlerle çalýþýr.
NotOnThisPlatform=Bu yazýlým, %1 üzerinde çalýþmaz.
OnlyOnThisPlatform=Bu yazýlým, %1 üzerinde çalýþtýrýlmalýdýr.
OnlyOnTheseArchitectures=Bu yazýlým, yalnýz þu iþlemci mimarileri için tasarlanmýþ Windows sürümleriyle çalýþýr:%n%n%1
MissingWOW64APIs=Kullandýðýnýz Windows sürümü 64-bit kurulumu için gerekli iþlevlere sahip deðil. Bu sorunu çözmek için lütfen Hizmet Paketi %1 yükleyin.
WinVersionTooLowError=Bu yazýlým için %1 sürüm %2 ya da üzeri gereklidir.
WinVersionTooHighError=Bu yazýlým, '%1' sürüm '%2' ya da üzerine kurulamaz.
AdminPrivilegesRequired=Bu yazýlýmý kurmak için Yönetici olarak oturum açmýþ olmanýz gerekir.
PowerUserPrivilegesRequired=Bu yazýlýmý kurarken, Yönetici ya da Güçlü Kullanýcýlar grubunun bir üyesi olarak oturum açmýþ olmanýz gerekir.
SetupAppRunningError=Kurulum, %1 yazýlýmýnýn çalýþmakta olduðunu algýladý.%n%nLütfen yazýlýmýn çalýþan tüm kopyalarýný kapatýp, devam etmek için Tamam ya da kurulumdan çýkmak için Ýptal düðmesine týklayýn.
UninstallAppRunningError=Kaldýrma, %1 yazýlýmýnýn çalýþmakta olduðunu algýladý.%n%nLütfen yazýlýmýn çalýþan tüm kopyalarýný kapatýp, devam etmek için Tamam ya da  kurulumdan çýkmak için Ýptal düðmesine týklayýn.

; *** Misc. errors
ErrorCreatingDir=Kurulum "%1" klasörünü oluþturamadý.
ErrorTooManyFilesInDir="%1" klasörü içinde çok sayýda dosya olduðundan bir dosya oluþturulamadý

; *** Setup common messages
ExitSetupTitle=Kurulumdan Çýkýn
ExitSetupMessage=Kurulum tamamlanmadý. Þimdi çýkarsanýz, yazýlým yüklenmeyecek.%n%nYüklemeyi tamamlamak için istediðiniz zaman kurulum programýný yeniden çalýþtýrabilirsiniz.%n%nKurulumdan çýkýlsýn mý?
AboutSetupMenuItem=Kurulum H&akkýnda...
AboutSetupTitle=Kurulum Hakkýnda
AboutSetupMessage=%1 %2 sürümü%n%3%n%n%1 anasayfa:%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< G&eri
ButtonNext=Ý&leri >
ButtonInstall=&Kurun
ButtonOK=Tamam
ButtonCancel=Ýptal
ButtonYes=E&vet
ButtonYesToAll=&Tümüne Evet
ButtonNo=&Hayýr
ButtonNoToAll=Tümüne Ha&yýr
ButtonFinish=&Bitti
ButtonBrowse=&Gözatýn...
ButtonWizardBrowse=Göza&týn...
ButtonNewFolder=Ye&ni Klasör Oluþturun

; *** "Select Language" dialog messages
SelectLanguageTitle=Kurulum Dilini Seçin
SelectLanguageLabel=Kurulum süresince kullanýlacak dili seçin:

; *** Common wizard text
ClickNext=Devam etmek için Ýleri, çýkmak için Ýptal düðmesine basýn.
BeveledLabel=
BrowseDialogTitle=Klasöre Gözatýn
BrowseDialogLabel=Aþaðýdaki listeden bir klasör seçip, Tamam düðmesine týklayýn.
NewFolderName=Yeni Klasör

; *** "Welcome" wizard page
WelcomeLabel1=[name] Kurulum Yardýmcýsýna Hoþgeldiniz.
WelcomeLabel2=Bilgisayarýnýza [name/ver] yazýlýmý kurulacak.%n%nDevam etmeden önce çalýþan diðer tüm programlarý kapatmanýz önerilir.

; *** "Password" wizard page
WizardPassword=Parola
PasswordLabel1=Bu kurulum parola korumalýdýr.
PasswordLabel3=Lütfen parolayý yazýn ve devam etmek için Ýleri düðmesine týklayýn. Parolalar büyük küçük harflere duyarlýdýr.
PasswordEditLabel=&Parola:
IncorrectPassword=Yazdýðýnýz parola doðru deðil. Lütfen yeniden deneyin.

; *** "License Agreement" wizard page
WizardLicense=Lisans Anlaþmasý
LicenseLabel=Lütfen devam etmeden önce aþaðýdaki önemli bilgileri okuyun.
LicenseLabel3=Lütfen Aþaðýdaki Lisans Anlaþmasýný okuyun. Kuruluma devam edebilmek için bu anlaþmayý kabul etmelisiniz.
LicenseAccepted=Anlaþmayý kabul &ediyorum.
LicenseNotAccepted=Anlaþmayý kabul et&miyorum.

; *** "Information" wizard pages
WizardInfoBefore=Bilgiler
InfoBeforeLabel=Lütfen devam etmeden önce aþaðýdaki önemli bilgileri okuyun.
InfoBeforeClickLabel=Kuruluma devam etmeye hazýr olduðunuzda Ýleri düðmesine týklayýn.
WizardInfoAfter=Bilgiler
InfoAfterLabel=Lütfen devam etmeden önce aþaðýdaki önemli bilgileri okuyun.
InfoAfterClickLabel=Kuruluma devam etmeye hazýr olduðunuzda Ýleri düðmesine týklayýn.

; *** "User Information" wizard page
WizardUserInfo=Kullanýcý Bilgileri
UserInfoDesc=Lütfen bilgilerinizi yazýn.
UserInfoName=K&ullanýcý Adý:
UserInfoOrg=Ku&rum:
UserInfoSerial=&Seri Numarasý:
UserInfoNameRequired=Bir ad yazmalýsýnýz.

; *** "Select Destination Directory" wizard page
WizardSelectDir=Hedef Klasörü Seçin
SelectDirDesc=[name] nereye kurulsun?
SelectDirLabel3=[name] yazýlýmý þu klasöre kurulacak.
SelectDirBrowseLabel=Devam etmek icin Ýleri düðmesine týklayýn. Farklý bir klasör seçmek için Gözatýn düðmesine týklayýn.
DiskSpaceMBLabel=En az [mb] MB disk alaný gereklidir.
CannotInstallToNetworkDrive=Yazýlým bir að sürücüsü üzerine kurulamaz.
CannotInstallToUNCPath=Yazýlým bir UNC yolu üzerine (\\yol gibi) kurulamaz.
InvalidPath=Sürücü adý ile tam yolu yazmalýsýnýz; örneðin: %n%nC:\APP%n%n ya da þu biçimde bir UNC yolu:%n%n\\sunucu\paylaþým
InvalidDrive=Sürücü ya da UNC paylaþýmý yok ya da eriþilemiyor. Lütfen baþka bir tane seçin.
DiskSpaceWarningTitle=Yeterli Disk Alaný Yok
DiskSpaceWarning=Kurulum için %1 KB boþ alan gerekli, ancak seçilmiþ sürücüde yalnýz %2 KB boþ alan var.%n%nGene de devam etmek istiyor musunuz?
DirNameTooLong=Klasör adý ya da yol çok uzun.
InvalidDirName=Klasör adý geçersiz.
BadDirName32=Klasör adlarýnda þu karakterler bulunamaz:%n%n%1
DirExistsTitle=Klasör Zaten Var"
DirExists=Klasör:%n%n%1%n%zaten var. Kurulum için bu klasörü kullanmak ister misiniz?
DirDoesntExistTitle=Klasör Bulunamadý
DirDoesntExist=Klasör:%n%n%1%n%nbulunamadý.Klasörün oluþturmasýný ister misiniz?

; *** "Select Components" wizard page
WizardSelectComponents=Bileþenleri Seçin
SelectComponentsDesc=Hangi bileþenler kurulacak?
SelectComponentsLabel2=Kurmak istediðiniz bileþenleri seçin; kurmak istemediðiniz bileþenlerin iþaretini kaldýrýn. Devam etmeye hazýr olduðunuzda Ýleri düðmesine týklayýn.
FullInstallation=Tam Kurulum
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Normal kurulum
CustomInstallation=Özel kurulum
NoUninstallWarningTitle=Varolan Bileþenler
NoUninstallWarning=Kur þu bileþenlerin bilgisayarýnýza zaten kurulmuþ olduðunu algýladý:%n%n%1%n%n Bu bileþenlerin iþaretlerinin kaldýrýlmasý bileþenleri kaldýrmaz.%n%nGene de devam etmek istiyor musunuz?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Seçili bileþenler için diskte en az [mb] MB bos alan gerekli.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Ek Ýþlemleri Seçin
SelectTasksDesc=Baþka hangi ek iþlemler yapýlsýn?
SelectTasksLabel2=[name] kurulurken yapýlmasýný istediðiniz ek iþleri seçin ve Ýleri düðmesine týklayýn.

; *** "Baþlat Menüsü Dizini Seç" sihirbaz sayfasý
WizardSelectProgramGroup=Baþlat Menüsü Klasörünü Seçin
SelectStartMenuFolderDesc=Yazýlýmýn kýsayollarý nereye kurulsun?
SelectStartMenuFolderLabel3=Kur yazýlým kýsayollarýný aþaðýdaki Baþlat Menüsü klasöründe oluþturacak.
SelectStartMenuFolderBrowseLabel=Devam etmek için Ýleri düðmesine týklayýn. Farklý bir klasör seçmek için Gözatýn düðmesine týklayýn.
MustEnterGroupName=Bir klasör adý yazmalýsýnýz.
GroupNameTooLong=Klasör adý ya da yol çok uzun.
InvalidGroupName=Klasör adý geçersiz.
BadGroupName=Klasör adýnda þu karakterler bulunamaz:%n%n%1
NoProgramGroupCheck2=Baþlat Menüsü klasörü &oluþturulmasýn

; *** "Ready to Install" wizard page
WizardReady=Kurulmaya Hazýr
ReadyLabel1=[name] bilgisayarýnýza kurulmaya hazýr.
ReadyLabel2a=Kuruluma devam etmek için Ýleri düðmesine, ayarlarý gözden geçirip deðiþtirmek için Geri düðmesine týklayýn.
ReadyLabel2b=Kuruluma devam etmek için Ýleri düðmesine týklayýn.
ReadyMemoUserInfo=Kullanýcý bilgileri:
ReadyMemoDir=Hedef konumu:
ReadyMemoType=Kurulum tipi:
ReadyMemoComponents=Seçilmiþ bileþenler:
ReadyMemoGroup=Baþlat Menüsü klasörü:
ReadyMemoTasks=Ek iþlemler:

; *** "Kurulmaya Hazýr" sihirbaz sayfasý
WizardPreparing=Kuruluma Hazýrlanýlýyor
PreparingDesc=[name] bilgisayarýnýza kurulmaya hazýrlanýyor.
PreviousInstallNotCompleted=Önceki yazýlým kurulumu ya da kaldýrýlmasý tamamlanmamýþ. Bu kurulumun tamamlanmasý için bilgisayarýnýzý yeniden baþlatmalýsýnýz.%n%nBilgisayarýnýzý yeniden baþlattýktan sonra iþlemi tamamlamak için [name] kurulumunu yeniden çalýþtýrýn.
CannotContinue=Kuruluma devam edilemiyor. Çýkmak için Ýptal düðmesine týklayýn.
ApplicationsFound=Þu uygulamalar, kurulum tarafýndan güncellenmesi gereken dosyalarý kullanýyor. Kurulumun bu uygulamalarý kendiliðinden kapatmasýna izin vermeniz önerilir.
ApplicationsFound2=Þu uygulamalar, kurulum tarafýndan güncellenmesi gereken dosyalarý kullanýyor. Kurulumun bu uygulamalarý kendiliðinden kapatmasýna izin vermeniz önerilir. Tamamlandýktan sonra kurulum, uygulamalarý yeniden baþlatmayý deneyecek.
CloseApplications=&Uygulamalar kapatýlsýn
DontCloseApplications=Uygulamalar &kapatýlmasýn
ErrorCloseApplications=Kurulum, uygulamalarý kapatamadý. Kurulum tarafýndan güncellenmesi gereken dosyalarý kullanan uygulamalarý el ile kapatmanýz önerilir.

; *** "Kuruluyor" sihirbaz
WizardInstalling=Kuruluyor
InstallingLabel=Lütfen [name] bilgisayarýnýza kurulurken bekleyin.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=[name] kurulum yardýmcýsý tamamlanýyor
FinishedLabelNoIcons=Bilgisayarýnýza [name] kurulumu tamamlandý.
FinishedLabel=Bilgisayarýnýza [name] kurulumu tamamlandý. Simgeleri yüklemeyi seçtiyseniz, uygulamayý simgelere týklayarak baþlatabilirsiniz.
ClickFinish=Kurulumdan çýkmak için Bitti düðmesine týklayýn.
FinishedRestartLabel=[name] kurulumunun tamamlanmasý için, bilgisayarýnýz yeniden baþlatýlmalý. Þimdi yeniden baþlatmak ister misiniz?
FinishedRestartMessage=[name] kurulumunun tamamlanmasý için, bilgisayarýnýz yeniden baþlatýlmalý.%n%nÞimdi yeniden baþlatmak ister misiniz?
ShowReadmeCheck=Evet README dosyasýna bakmak istiyorum
YesRadio=&Evet, bilgisayar þimdi yeniden baþlatýlsýn
NoRadio=&Hayýr, bilgisayarý daha sonra yeniden baþlatacaðým
; used for example as 'Run MyProg.exe'
RunEntryExec=%1 çalýþtýrýlsýn
; used for example as 'View Readme.txt'
RunEntryShellExec=%1 görüntülensin

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Kurulum için Sýradaki Disk Gerekli
SelectDiskLabel2=Lütfen %1. diski takýp Tamam düðmesine týklayýn.%n%nDiskteki dosyalar aþaðýdakinden farklý bir klasörde bulunuyorsa, doðru yolu yazýn ya da Gözatýn düðmesine týklayarak doðru klasörü seçin.
PathLabel=&Yol:
FileNotInDir2="%1" dosyasý "%2" içinde yok. Lütfen doðru diski takýn ya da baþka bir klasör seçin.
SelectDirectoryLabel=Lütfen sonraki diskin konumunu belirtin.

; *** Installation phase messages
SetupAborted=Kurulum tamamlanamadý.%n%nLütfen sorunu düzelterek kurulumu yeniden çalýþtýrýn.
EntryAbortRetryIgnore=Yeniden denemek için Yeniden Deneyin düðmesine, devam etmek için Yoksayýn düðmesine, kurulumu iptal etmek için Vazgeçin düðmesine týklayýn.

; *** Installation status messages
StatusClosingApplications=Uygulamalar kapatýlýyor...
StatusCreateDirs=Klasörler oluþturuluyor...
StatusExtractFiles=Dosyalar ayýklanýyor...
StatusCreateIcons=Kýsayollar oluþturuluyor...
StatusCreateIniEntries=INI kayýtlarý oluþturuluyor...
StatusCreateRegistryEntries=Kayýt Defteri kayýtlarý oluþturuluyor...
StatusRegisterFiles=Dosyalar kaydediliyor...
StatusSavingUninstall=Kaldýrma bilgileri kaydediliyor...
StatusRunProgram=Kurulum tamamlanýyor...
StatusRestartingApplications=Uygulamalar yeniden baþlatýlýyor...
StatusRollback=Deðiþiklikler geri alýnýyor...

; *** Misc. errors
ErrorInternal2=Ýç hata: %1
ErrorFunctionFailedNoCode=%1 tamamlanamadý.
ErrorFunctionFailed=%1 tamamlanamadý; kod %2
ErrorFunctionFailedWithMessage=%1 tamamlanamadý; kod %2.%n%3
ErrorExecutingProgram=Þu dosya yürütülemedi:%n%1

; *** Registry errors
ErrorRegOpenKey=Kayýt defteri anahtarý açýlýrken bir hata oluþtu:%n%1%2
ErrorRegCreateKey=Kayýt defteri anahtarý oluþturulurken bir hata oluþtu:%n%1%2
ErrorRegWriteKey=Kayýt defteri anahtarý yazýlýrken bir hata oluþtu:%n%1%2

; *** INI errors
ErrorIniEntry="%1" dosyasýna INI kaydý eklenirken bir hata oluþtu.

; *** File copying errors
FileAbortRetryIgnore=Yeniden denemek için Yeniden Deneyin düðmesine, bu dosyayý atlamak için (önerilmez) Yoksayýn düðmesine, kurulumu iptal etmek için Vazgeçin düðmesine týklayýn.
FileAbortRetryIgnore2=Yeniden denemek için Yeniden Deneyin düðmesine, devam etmek için (önerilmez) Yoksayýn düðmesine, kurulumu iptal etmek için Vazgeçin düðmesine týklayýn.
SourceIsCorrupted=Kaynak dosya bozuk
SourceDoesntExist="%1" kaynak dosyasý bulunamadý
ExistingFileReadOnly=Varolan dosya salt okunabilir olarak iþaretlenmiþ.%n%nSalt okunur özniteliðini kaldýrýp yeniden denemek için Yeniden Deneyin düðmesine, bu dosyayý atlamak için Yoksayýn düðmesine, kurulumunu iptal etmek için Vazgeçin düðmesine týklayýn.
ErrorReadingExistingDest=Varolan dosya okunmaya çalýþýlýrken bir hata oluþtu.
FileExists=Dosya zaten var.%n%nKurulum bu dosyanýn üzerine yazsýn mý?
ExistingFileNewer=Varolan dosya, kurulum tarafýndan yazýlmaya çalýþýlandan daha yeni.Varolan dosyayý korumanýz önerilir %n%nVarolan dosya korunsun mu?
ErrorChangingAttr=Varolan dosyanýn öznitelikleri deðiþtirilirken bir hata oluþtu:
ErrorCreatingTemp=Hedef klasörde dosya oluþturulurken bir hata oluþtu:
ErrorReadingSource=Kaynak dosya okunurken bir hata oluþtu:
ErrorCopying=Dosya kopyalanýrken bir hata oluþtu:
ErrorReplacingExistingFile=Varolan dosya deðiþtirilirken bir hata oluþtu.
ErrorRestartReplace=Yeniden baþlatmada deðiþtirme tamamlanamadý:
ErrorRenamingTemp=Hedef klasördeki dosyanýn adý deðiþtirilirken bir hata oluþtu:
ErrorRegisterServer=DLL/OCX kayýt edilemedi: %1
ErrorRegSvr32Failed=RegSvr32 þu kod ile iþlemi tamamlayamadý: %1
ErrorRegisterTypeLib=Tip kitaplýðý kaydedilemedi: %1

; *** Post-installation errors
ErrorOpeningReadme=README dosyasý açýlýrken bir hata oluþtu.
ErrorRestartingComputer=Kurulum bilgisayarýnýzý yeniden baþlatamýyor. Lütfen bilgisayarýnýzý yeniden baþlatýn.

; *** Uninstaller messages
UninstallNotFound="%1" dosyasý bulunamadý. Yazýlým kaldýrýlamýyor.
UninstallOpenError="%1" dosyasý açýlamadý. Yazýlým kaldýrýlamýyor.
UninstallUnsupportedVer="%1" kaldýrma günlük dosyasýnýn biçimi, bu kaldýrýcý sürümü tarafýndan anlaþýlamadý. Yazýlým kaldýrýlamýyor.
UninstallUnknownEntry=Kaldýrma günlüðünde bilinmeyen bir kayýt (%1) bulundu.
ConfirmUninstall=%1 yazýlýmýný tüm bileþenleri ile birlikte tamamen kaldýrmak istediðinize emin misiniz?
UninstallOnlyOnWin64=Bu kurulum yalnýz 64-bit Windows üzerinden kaldýrýlabilir.
OnlyAdminCanUninstall=Bu kurulum yalnýz yönetici haklarýna sahip bir kullanýcý tarafýndan kaldýrýlabilir.
UninstallStatusLabel=Lütfen %1 yazýlýmý bilgisayarýnýzdan kaldýrýlýrken bekleyin.
UninstalledAll=%1 yazýlýmý bilgisayarýnýzdan kaldýrýldý.
UninstalledMost=%1 yazýlýmý kaldýrýldý.%n%nBazý bileþenler kaldýrýlamadý. Bunlarý el ile silebilirsiniz.
UninstalledAndNeedsRestart=%1 kaldýrma iþlemini tamamlamak için bilgisayarýnýz yeniden baþlatýlmalý.%n%nÞimdi yeniden baþlatmak ister misiniz?
UninstallDataCorrupted="%1" dosyasý bozulmuþ. Kaldýrýlamýyor.

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Paylaþýlan Dosya Silinsin mi?
ConfirmDeleteSharedFile2=Sisteme göre, paylaþýlan þu dosya baþka bir program tarafýndan kullanýlmýyor ve kaldýrýlabilir. Bu paylaþýlmýþ dosyayý silmek ister misiniz?%n%nBaþka herhangi bir yazýlým bu dosyayý halen kullanýyor ise, sildiðinizde diðer yazýlým düzgün çalýþmayabilir. Emin deðilseniz Hayýr düðmesine týklayýn. Dosyayý sisteminizde býrakmanýn bir zararý olmaz.
SharedFileNameLabel=Dosya adý:
SharedFileLocationLabel=Konum:
WizardUninstalling=Kaldýrma Durumu
StatusUninstalling=%1 kaldýrýlýyor...

; *** Shutdown block reasons
ShutdownBlockReasonInstallingApp=%1 kuruluyor.
ShutdownBlockReasonUninstallingApp=%1 kaldýrýlýyor.

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 %2 sürümü
AdditionalIcons=Ek simgeler:
CreateDesktopIcon=Masaüstü simg&esi oluþturulsun
CreateQuickLaunchIcon=Hýzlý Baþlat simgesi &oluþturulsun
ProgramOnTheWeb=%1 Web Sitesi
UninstallProgram=%1 Yazýlýmýný Kaldýrýn
LaunchProgram=%1 Yazýlýmý Çalýþtýrýlsýn
AssocFileExtension=%1 y&azýlýmý ile %2 dosya uzantýsý iliþkilendirilsin
AssocingFileExtension=%1 y&azýlýmý ile %2 dosya uzantýsý iliþkilendiriliyor...
AutoStartProgramGroupDescription=Baþlangýç:
AutoStartProgram=%1 kendiliðinden baþlatýlsýn
AddonHostProgramNotFound=%1 seçtiðiniz klasörde bulunamadý.%n%nYine de devam etmek istiyor musunuz?
