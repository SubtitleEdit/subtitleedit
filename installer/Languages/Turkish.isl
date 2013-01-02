; *** Inno Setup version 5.5.3+ Turkish messages ***
; Language	"Turkce" Turkish Translate by "Ceviren"	Adil YILDIZ	adilyildiz@gmail.com
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
SetupAppTitle=Kur
SetupWindowTitle=%1 - Kur
UninstallAppTitle=Kaldýr
UninstallAppFullTitle=%1 Kaldýr

; *** Misc. common
InformationTitle=Bilgi
ConfirmTitle=Sorgu
ErrorTitle=Hata

; *** SetupLdr messages
SetupLdrStartupMessage=Bu kurulum %1 programýný yükleyecektir. Devam etmek istiyor musunuz?
LdrCannotCreateTemp=Geçici bir dosya oluþturulamadý. Kurulum iptal edildi
LdrCannotExecTemp=Geçici dizindeki dosya çalýþtýrýlamadý. Kurulum iptal edildi

; *** Startup error messages
LastErrorMessage=%1.%n%nHata %2: %3
SetupFileMissing=%1 adlý dosya kurulum dizininde bulunamadý. Lütfen problemi düzeltiniz veya programýn yeni bir kopyasýný edininiz.
SetupFileCorrupt=Kurulum dosyalarý bozulmuþ. Lütfen programýn yeni bir kopyasýný edininiz.
SetupFileCorruptOrWrongVer=Kurulum dosyalarý bozulmuþ veya kurulumun bu sürümü ile uyuþmuyor olabilir. Lütfen problemi düzeltiniz veya Programýn yeni bir kopyasýný edininiz.
InvalidParameter=Komut satýrýna geçersiz bir parametre girildi:%n%n%1
SetupAlreadyRunning=Kur zaten çalýþýyor.
WindowsVersionNotSupported=Bu program bilgisayarýnýzda çalýþan Windows sürümünü desteklemiyor.
WindowsServicePackRequired=Bu program için %1 Service Pack %2 veya sonrasý gerekmektedir.
NotOnThisPlatform=Bu program %1 üzerinde çalýþtýrýlamaz.
OnlyOnThisPlatform=Bu program sadece %1 üzerinde çalýþtýrýlmalýdýr.
OnlyOnTheseArchitectures=Bu program sadece aþaðýdaki mimarilere sahip Windows sürümlerinde çalýþýr:%n%n%1
MissingWOW64APIs=Kullandýðýnýz Windows sürümü Kur'un 64-bit yükleme yapabilmesi için gerekli olan özelliklere sahip deðildir. Bu problemi ortadan kaldýrmak için lütfen Service Pack %1 yükleyiniz.
WinVersionTooLowError=Bu programý çalýþtýrabilmek için %1 %2 sürümü veya daha sonrasý yüklü olmalýdýr.
WinVersionTooHighError=Bu program %1 %2 sürümü veya sonrasýnda çalýþmaz.
AdminPrivilegesRequired=Bu program kurulurken yönetici olarak oturum açýlmýþ olmak gerekmektedir.
PowerUserPrivilegesRequired=Bu program kurulurken Yönetici veya Güç Yöneticisi Grubu üyesi olarak giriþ yapýlmýþ olmasý gerekmektedir.
SetupAppRunningError=Kur %1 programýnýn çalýþtýðýný tespit etti.%n%nLütfen bu programýn çalýþan bütün parçalarýný þimdi kapatýnýz, daha sonra devam etmek için Tamam'a veya çýkmak için Ýptal'e basýnýz.
UninstallAppRunningError=Kaldýr %1 programýnýn çalýþtýðýný tespit etti.%n%nLütfen bu programýn çalýþan bütün parçalarýný þimdi kapatýnýz, daha sonra devam etmek için Tamam'a veya çýkmak için Ýptal'e basýnýz.

; *** Misc. errors
ErrorCreatingDir=Kur " %1 " dizinini oluþturamadý.
ErrorTooManyFilesInDir=" %1 " dizininde bir dosya oluþturulamadý. Çünkü dizin çok fazla dosya içeriyor

; *** Setup common messages
ExitSetupTitle=Kur'dan Çýk
ExitSetupMessage=Kurulum tamamlanmadý. Þimdi çýkarsanýz program kurulmuþ olmayacak.%n%nDaha sonra Kur'u tekrar çalýþtýrarak kurulumu tamamlayabilirsiniz.%n%nKur'dan çýkmak istediðinizden emin misiniz?
AboutSetupMenuItem=Kur H&akkýnda...
AboutSetupTitle=Kur Hakkýnda
AboutSetupMessage=%1 %2 sürümü%n%3%n%n%1 internet:%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< G&eri
ButtonNext=Ý&leri >
ButtonInstall=&Kur
ButtonOK=Tamam
ButtonCancel=Ýptal
ButtonYes=E&vet
ButtonYesToAll=Tümüne E&vet
ButtonNo=&Hayýr
ButtonNoToAll=Tümüne Ha&yýr
ButtonFinish=&Son
ButtonBrowse=&Gözat...
ButtonWizardBrowse=Göza&t...
ButtonNewFolder=Ye&ni Dizin Oluþtur

; *** "Select Language" dialog messages
SelectLanguageTitle=Kur Dilini Seçiniz
SelectLanguageLabel=Lütfen kurulum sýrasýnda kullanacaðýnýz dili seçiniz:

; *** Common wizard text
ClickNext=Devam etmek için Ýleri'ye , çýkmak için Ýptal 'e basýnýz.
BeveledLabel=
BrowseDialogTitle=Dizine Gözat
BrowseDialogLabel=Aþaðýdaki listeden bir dizin seçip, daha sonra Tamam tuþuna basýnýz.
NewFolderName=Yeni Dizin

; *** "Welcome" wizard page
WelcomeLabel1=[name] Kurulum Sihirbazýna Hoþgeldiniz.
WelcomeLabel2=Kur þimdi [name/ver] programýný bilgisayarýnýza yükleyecektir.%n%nDevam etmeden önce çalýþan diðer bütün programlarý kapatmanýz tavsiye edilir.

; *** "Password" wizard page
WizardPassword=Þifre
PasswordLabel1=Bu kurulum þifre korumalýdýr.
PasswordLabel3=Lütfen þifreyi giriniz. Daha sonra devam etmek için Ýleri'ye basýnýz. Lütfen þifreyi girerken Büyük-Küçük harflere dikkat ediniz.
PasswordEditLabel=&Þifre:
IncorrectPassword=Girdiðiniz þifre hatalý. Lütfen tekrar deneyiniz.

; *** "License Agreement" wizard page
WizardLicense=Lisans Anlaþmasý
LicenseLabel=Lütfen devam etmeden önce aþaðýdaki önemli bilgileri okuyunuz.
LicenseLabel3=Lütfen Aþaðýdaki Lisans Anlaþmasýný okuyunuz. Kuruluma devam edebilmek için bu anlaþmanýn koþullarýný kabul etmiþ olmalýsýnýz.
LicenseAccepted=Anlaþmayý Kabul &Ediyorum.
LicenseNotAccepted=Anlaþmayý Kabul Et&miyorum.

; *** "Information" wizard pages
WizardInfoBefore=Bilgi
InfoBeforeLabel=Lütfen devam etmeden önce aþaðýdaki önemli bilgileri okuyunuz.
InfoBeforeClickLabel=Kur ile devam etmeye hazýr olduðunuzda Ýleri'yi týklayýnýz.
WizardInfoAfter=Bilgi
InfoAfterLabel=Lütfen devam etmeden önce aþaðýdaki önemli bilgileri okuyunuz.
InfoAfterClickLabel=Kur ile devam etmeye hazýr olduðunuzda Ýleri'yi týklayýnýz.

; *** "User Information" wizard page
WizardUserInfo=Kullanýcý Bilgileri
UserInfoDesc=Lütfen bilgilerinizi giriniz.
UserInfoName=K&ullanýcý Adý:
UserInfoOrg=Þi&rket:
UserInfoSerial=&Seri Numarasý:
UserInfoNameRequired=Bir isim girmelisiniz.

; *** "Select Destination Directory" wizard page
WizardSelectDir=Kurulacak Dizini Seçiniz
SelectDirDesc=[name] hangi dizine kurulsun?
SelectDirLabel3=Kur [name] programýný aþaðýdaki dizine kuracaktýr.
SelectDirBrowseLabel=Devam etmek için Ýleri'ye basýnýz. Baþka bir dizin seçmek istiyorsanýz, Gözat'a basýnýz.
DiskSpaceMBLabel=Bu program en az [mb] MB disk alaný gerektirmektedir.
CannotInstallToNetworkDrive=Kur bir að sürücüsüne kurulum yapamaz.
CannotInstallToUNCPath=Kur UNC tipindeki dizin yollarýna (Örnek: \\yol vb.) kurulum yapamaz.
InvalidPath=Sürücü ismi ile birlikte tam yolu girmelisiniz; Örneðin %nC:\APP%n%n veya bir UNC yolunu %n%n\\sunucu\paylaþým%n%n þeklinde girmelisiniz.
InvalidDrive=Seçtiðiniz sürücü bulunamadý veya ulaþýlamýyor. Lütfen baþka bir sürücü seçiniz.
DiskSpaceWarningTitle=Yetersiz Disk Alaný
DiskSpaceWarning=Kur en az %1 KB kullanýlabilir disk alaný gerektirmektedir. Ancak seçili diskte %2 KB boþ alan bulunmaktadýr.%n%nYine de devam etmek istiyor musunuz?
DirNameTooLong=Dizin adý veya yolu çok uzun.
InvalidDirName=Dizin adý geçersiz.
BadDirName32=Dizin adý takib eden karakterlerden her hangi birini içeremez:%n%n%1
DirExistsTitle=Dizin Bulundu
DirExists=Dizin:%n%n%1%n%n zaten var. Yine de bu dizine kurmak istediðinizden emin misiniz?
DirDoesntExistTitle=Dizin Bulunamadý
DirDoesntExist=Dizin:%n%n%1%n%nbulunmamaktadýr. Bu dizini oluþturmak ister misiniz?

; *** "Select Components" wizard page
WizardSelectComponents=Bileþen Seç
SelectComponentsDesc=Hangi bileþenler kurulsun?
SelectComponentsLabel2=Kurmak istediðiniz bileþenleri seçiniz; istemediklerinizi temizleyiniz.Devam etmeye hazýr olduðunuz zaman Ýleri'ye týklayýnýz.
FullInstallation=Tam Kurulum
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Normal Kurulum
CustomInstallation=Özel Kurulum
NoUninstallWarningTitle=Mevcut Bileþenler
NoUninstallWarning=Kur aþaðýdaki bileþenlerin kurulu olduðunu tespit etti:%n%n%1%n%nBu bileþenlerin seçimini kaldýrmak bileþenleri silmeyecek.%n%nYine de devam etmek istiyor musunuz?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Seçili bileþenler için en az [mb] MB disk alaný gerekmektedir.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Ek Görevleri Seçiniz
SelectTasksDesc=Hangi görevler yerine getirilsin?
SelectTasksLabel2=[name] kurulurken istediðiniz ek görevleri seçip Ýleri'ye týklayýnýz.

; *** "Baþlat Menüsü Dizini Seç" sihirbaz sayfasý
WizardSelectProgramGroup=Baþlat Menüsü Dizinini Seçiniz
SelectStartMenuFolderDesc=Kur program kýsayollarýný nereye yerleþtirsin?
SelectStartMenuFolderLabel3=Kur programýn kýsayollarýný aþaðýdaki Baþlat Menüsü dizinine kuracak.
SelectStartMenuFolderBrowseLabel=Devam etmek için, Ýleri'ye basýnýz. Baþka bir dizin seçmek istiyorsanýz, Gözat'a basýnýz.
MustEnterGroupName=Bir dizin ismi girmelisiniz.
GroupNameTooLong=Dizin adý veya yolu çok uzun.
InvalidGroupName=Dizin adý geçersiz.
BadGroupName=Dizin adý, takip eden karakterlerden her hangi birini içeremez:%n%n%1
NoProgramGroupCheck2=&Baþlat menüsünde kýsayol oluþturma

; *** "Ready to Install" wizard page
WizardReady=Yükleme için Hazýr
ReadyLabel1=Kur [name] programýný bilgisayarýnýza kurmak için hazýr.
ReadyLabel2a=Kuruluma devam etmek için Kur'a , ayarlarýnýzý kontrol etmek veya deðiþtirmek için Geri'ye týklayýnýz.
ReadyLabel2b=Kuruluma devam etmek için Kur'a týklayýnýz.
ReadyMemoUserInfo=Kullanýcý bilgisi:
ReadyMemoDir=Hedef dizin:
ReadyMemoType=Kurulum tipi:
ReadyMemoComponents=Seçili bileþenler:
ReadyMemoGroup=Baþlat Menüsü :
ReadyMemoTasks=Ek görevler:

; *** "Kur Hazýlanýyor" sihirbaz sayfasý
WizardPreparing=Kurulum Hazýrlanýyor
PreparingDesc=Kur [name] programýný bilgisayarýnýza kurmak için hazýrlanýyor.
PreviousInstallNotCompleted=Bir önceki Kurulum/Kaldýr programýna ait iþlem tamamlanmamýþ.Önceki kurulum iþleminin tamamlanmasý için bilgisayarýnýzý yeniden baþlatmalýsýnýz.%n%nBilgisayarýnýz tekrar baþladýktan sonra,Kurulum'u tekrar çalýþtýrarak [name] programýný kurma iþlemine devam edebilirsiniz.
CannotContinue=Kur devam edemiyor. Lütfen Ýptal'e týklayýp Çýkýn.
ApplicationsFound=Aþaðýdaki uygulamalar, Kur tarafýndan güncelleþtirilmesi gereken dosyalarý kullanýyor. Kur tarafýndan, bu uygulamalarýn otomatik kapatýlmasýna izin vermenizi öneririz.
ApplicationsFound2=Aþaðýdaki uygulamalar, Kur tarafýndan güncelleþtirilmesi gereken dosyalarý kullanýyor. Kur tarafýndan, bu uygulamalarýn otomatik kapatýlmasýna izin vermenizi öneririz. Yükleme tamamlandýktan sonra, Kur uygulamalarý yeniden baþlatmaya çalýþacaktýr.
CloseApplications=&Uygulamalarý otomatik kapat
DontCloseApplications=Uygulamalarý &kapatma
ErrorCloseApplications=Kurulum otomatik olarak tüm programlarý kapatmakta baþarýsýz oldu. Devam etmeden önce Kurulum tarafýndan güncellenmesi gereken dosyalarý kullanan uygulamalarý kapatmanýz önerilir.

; *** "Kuruluyor" sihirbaz
WizardInstalling=Kuruluyor
InstallingLabel=Lütfen [name] bilgisayarýnýza kurulurken bekleyiniz.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=[name] Kur Sihirbazý tamamlanýyor
FinishedLabelNoIcons=Kur [name] programýný bilgisayarýnýza kurma iþlemini tamamladý.
FinishedLabel=Kur [name] programýný bilgisayarýnýza kurma iþlemini tamamladý. Program yüklenen kýsayol simgesine týklanarak çalýþtýrýlabilir.
ClickFinish=Kur'dan çýkmak için Son'a týklayýnýz.
FinishedRestartLabel=[name] programýnýn kurulumunu bitirmek için, Kur bilgisayarýnýzý yeniden baþlatacak. Bilgisayarýnýz yeniden baþlatýlsýn mý?
FinishedRestartMessage=[name] kurulumunu bitirmek için, bilgisayarýnýzýn yeniden baþlatýlmasý gerekmektedir. %n%nBiligisayarýnýz yeniden baþlatýlsýn mý?
ShowReadmeCheck=Beni Oku dosyasýný okumak istiyorum.
YesRadio=&Evet , bilgisayar yeniden baþlatýlsýn.
NoRadio=&Hayýr, daha sonra yeniden baþlatýrým.
; used for example as 'Run MyProg.exe'
RunEntryExec=%1 uygulamasýný Çalýþtýr
; used for example as 'View Readme.txt'
RunEntryShellExec=%1 dosyasýný görüntüle

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Bir Sonraki Diski Takýnýz
SelectDiskLabel2=%1 numaralý diski takýp, Tamam'ý týklayýnýz.%n%nEðer dosyalar baþka bir yerde bulunuyor ise doðru yolu yazýnýz veya Gözat'ý týklayýnýz.
PathLabel=&Yol:
FileNotInDir2=" %1 " adlý dosya " %2 " dizininde bulunamadý. Lütfen doðru diski veya dosyayý seçiniz.
SelectDirectoryLabel=Lütfen sonraki diskin yerini belirleyiniz.

; *** Installation phase messages
SetupAborted=Kurulum tamamlanamadý.%n%nLütfen problemi düzeltiniz veya Kurulum'u tekrar çalýþtýrýnýz.
EntryAbortRetryIgnore=Tekrar denemek için "Tekrar Dene" ye , yine de devam etmek için Yoksay'a , kurulumu iptal etmek için ise Ýptal'e týklayýnýz.

; *** Installation status messages
StatusClosingApplications=Uygulamalar kapatýlýyor...
StatusCreateDirs=Dizinler oluþturuluyor...
StatusExtractFiles=Dosyalar çýkartýlýyor...
StatusCreateIcons=Program kýsayollarý oluþturuluyor...
StatusCreateIniEntries=INI girdileri oluþturuluyor...
StatusCreateRegistryEntries=Kayýt Defteri girdileri oluþturuluyor...
StatusRegisterFiles=Dosyalar sisteme kaydediliyor...
StatusSavingUninstall=Kaldýr bilgileri kaydediliyor...
StatusRunProgram=Kurulum sonlandýrýlýyor...
StatusRestartingApplications=Uygulamalar baþlatýlýyor...
StatusRollback=Deðiþiklikler geri alýnýyor...

; *** Misc. errors
ErrorInternal2=Ýç hata: %1
ErrorFunctionFailedNoCode=%1 baþarýsýz oldu.
ErrorFunctionFailed=%1 baþarýsýz oldu; kod  %2
ErrorFunctionFailedWithMessage=%1 baþarýsýz oldu ; kod  %2.%n%3
ErrorExecutingProgram=%1 adlý dosya çalýþtýrýlamadý.

; *** Registry errors
ErrorRegOpenKey=Aþaðýdaki Kayýt Defteri anahtarý açýlýrken hata oluþtu:%n%1\%2
ErrorRegCreateKey=Aþaðýdaki Kayýt Defteri anahtarý oluþturulurken hata oluþtu:%n%1\%2
ErrorRegWriteKey=Aþaðýdaki Kayýt Defteri anahtarýna yazýlýrken hata oluþtu:%n%1\%2

; *** INI errors
ErrorIniEntry=" %1 " adlý dosyada INI girdisi yazma hatasý.

; *** File copying errors
FileAbortRetryIgnore=Yeniden denemek için "Yeniden Dene" ye, dosyayý atlamak için Yoksay'a (önerilmez), Kurulumu iptal etmek için Ýptal'e týklayýnýz.
FileAbortRetryIgnore2=Yeniden denemek için "Yeniden Dene" ye , yine de devam etmek için Yoksay'a (önerilmez), Kurulumu Ýptal etmek için Ýptal'e týklayýnýz.
SourceIsCorrupted=Kaynak dosya bozulmuþ
SourceDoesntExist=%1 adlý kaynak dosya bulunamadý.
ExistingFileReadOnly=Dosya Salt Okunur.%n%nSalt Okunur özelliðini kaldýrýp yeniden denemek için Yeniden Dene'yi , dosyasý atlamak için Yoksay'ý , Kurulumu iptal etmek için Ýptal'i týklayýnýz.
ErrorReadingExistingDest=Dosyayý okurken bir hata oluþtu :
FileExists=Dosya zaten var.%n%nKurulum'un üzerine yazmasýný ister misiniz?
ExistingFileNewer=Zaten var olan dosya Kurulum'un yüklemek istediði dosyadan daha yeni. Var olan dosyayý saklamanýz önerilir.%n%nVar olan dosya saklansýn mý?
ErrorChangingAttr=Zaten var olan dosyanýn özelliði deðiþtirilirken bir hata oluþtu:
ErrorCreatingTemp=Hedef dizinde dosya oluþturulurken bir hata oluþtu:
ErrorReadingSource=Kaynak dosya okunurken bir hata oluþtu:
ErrorCopying=Bir dosya kopyalanýrken bir hata oluþtu:
ErrorReplacingExistingFile=Zaten var olan dosya deðiþtirilirken bir hata oluþtu:
ErrorRestartReplace=RestartReplace baþarýsýz oldu:
ErrorRenamingTemp=Hedef dizinde bulunan dosyanýn adý deðiþtirilirken hata oldu:
ErrorRegisterServer=%1 adlý DLL/OCX sisteme tanýtýlamadý.
ErrorRegSvr32Failed=RegSvr32 çýkýþ hatasý %1 ile baþarýsýz oldu
ErrorRegisterTypeLib=%1 adlý tip kütüphanesi (Type Library) sisteme tanýtýlamadý

; *** Post-installation errors
ErrorOpeningReadme=Beni Oku dosyasý açýlýrken hata oluþtu.
ErrorRestartingComputer=Kurulum bilgisayarý yeniden baþlatamadý. Lütfen kendiniz kapatýnýz.

; *** Uninstaller messages
UninstallNotFound=%1 adlý dosya bulunamadý. Kaldýrma programý çalýþtýrýlamadý.
UninstallOpenError="%1" dosyasý açýlamýyor. Kaldýrma programý çalýþtýrýlamadý.
UninstallUnsupportedVer=%1 adlý Kaldýr bilgi dosyasý kaldýrma programýnýn bu sürümü ile uyuþmuyor. Kaldýrma programý çalýþtýrýlamadý.
UninstallUnknownEntry=Kaldýr Bilgi dosyasýndaki %1 adlý satýr anlaþýlamadý
ConfirmUninstall=%1 ve bileþenlerini kaldýrmak istediðinizden emin misiniz?
UninstallOnlyOnWin64=Bu kurulum sadece 64-bit Windows'lardan kaldýrýlabilir.
OnlyAdminCanUninstall=Bu kurulum sadece yönetici yetkisine sahip kullanýcýlar tarafýndan kaldýrabilir.
UninstallStatusLabel=Lütfen %1 programý bilgisayarýnýzdan kaldýrýlýrken bekleyin...
UninstalledAll=%1 programý bilgisayarýnýzdan tamamen kaldýrýldý.
UninstalledMost=%1 programýnýn kaldýrýlma iþlemi sona erdi.%n%nBazý bileþenler kaldýrýlamadý. Bu dosyalarý kendiniz silebilirsiniz.
UninstalledAndNeedsRestart=%1 programýnýn kaldýrýlmasý tamamlandý, Bilgisayarýnýzý yeniden baþlatmalýsýnýz.%n%nÞimdi yeniden baþlatýlsýn mý?
UninstallDataCorrupted="%1" adlý dosya bozuk. . Kaldýrma programý çalýþtýrýlamadý.

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Paylaþýlan Dosya Kaldýrýlsýn Mý?
ConfirmDeleteSharedFile2=Sistemde paylaþýlan bazý dosyalarýn artýk hiçbir program tarafýndan kullanýlmadýðýný belirtiyor. Kaldýr bu paylaþýlan dosyalarý silsin mi?%n%n Bu dosya bazý programlar tafarýndan kullanýlýyorsa ve silinmesini isterseniz, bu programalar düzgün çalýþmayabilir. Emin deðilseniz, Hayýr'a týklayýnýz. Dosyanýn sisteminizde durmasý hiçbir zarar vermez.
SharedFileNameLabel=Dosya adý:
SharedFileLocationLabel=Yol:
WizardUninstalling=Kaldýrma Durumu
StatusUninstalling=%1 Kaldýrýlýyor...

; *** Shutdown block reasons
ShutdownBlockReasonInstallingApp=%1 kuruluyor.
ShutdownBlockReasonUninstallingApp=%1 kaldýrýlýyor.

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 %2 sürümü
AdditionalIcons=Ek simgeler:
CreateDesktopIcon=Masaüstü simg&esi oluþtur
CreateQuickLaunchIcon=Hýzlý Baþlat simgesi &oluþtur
ProgramOnTheWeb=%1 Web Sitesi
UninstallProgram=%1 Programýný Kaldýr
LaunchProgram=%1 Programýný Çalýþtýr
AssocFileExtension=%2 dosya uzantýlarýný %1 ile iliþkilendir
AssocingFileExtension=%2 dosya uzantýlarý %1 ile iliþkilendiriliyor...
AutoStartProgramGroupDescription=Baþlangýç:
AutoStartProgram=%1 otomatik baþlat
AddonHostProgramNotFound=%1 seçtiðiniz klasörde bulunamadý.%n%nYine de devam etmek istiyor musunuz?
