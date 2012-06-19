; *** Inno Setup version 5.5.0+ Greek messages ***
;
; Note: When translating this text, do not add periods (.) to the end of
; messages that didn't have them already, because on those messages Inno
; Setup adds the periods automatically (appending a period would result in
; two periods being displayed).
;
; Originally translated by Anastasis Chatzioglou
;                          http://anasto.go.to, baldycom@hotmail.com
; Updated by XhmikosR [XhmikosR, my_nickname at yahoo dot com]
; 29th October 2011 [XhmikosR]:
;   various cleanup and minor updates
; 30th May 2012/1st June 2012 [XhmikosR]:
;   updated strings for Inno Setup 5.5.0, updated older untranslated strings;
;   the translation should be complete now
;

[LangOptions]
; The following three entries are very important. Be sure to read and
; understand the '[LangOptions] section' topic in the help file.
LanguageName=<0395><03BB><03BB><03B7><03BD><03B9><03BA><03AC>
LanguageID=$408
LanguageCodePage=1253
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
SetupAppTitle=Εγκατάσταση
SetupWindowTitle=Εγκατάσταση - %1
UninstallAppTitle=Απεγκατάσταση
UninstallAppFullTitle=%1 Απεγκατάσταση

; *** Misc. common
InformationTitle=Πληροφορίες
ConfirmTitle=Επιβεβαίωση
ErrorTitle=Σφάλμα

; *** SetupLdr messages
SetupLdrStartupMessage=Θα εκτελεστεί η εγκατάσταση του %1. Θέλετε να συνεχίσετε;
LdrCannotCreateTemp=Σφάλμα στη δημιουργία προσωρινού αρχείου. Η εγκατάσταση τερματίστηκε.
LdrCannotExecTemp=Αδύνατη η εκτέλεση αρχείου στον φάκελο προσωρινών αρχείων. Η εγκατάσταση τερματίστηκε.

; *** Startup error messages
LastErrorMessage=%1.%n%nΣφάλμα %2: %3
SetupFileMissing=Δεν βρίσκεται το αρχείο %1 στον κατάλογο εγκατάστασης. Ίσως χρειάζεται να προμηθευτείτε ένα νέο αντίγραφο του προγράμματος.
SetupFileCorrupt=Το αρχείο εγκατάστασης είναι κατεστραμμένο. Ίσως χρειάζεται να προμηθευτείτε ένα νέο αντίγραφο του προγράμματος.
SetupFileCorruptOrWrongVer=Το αρχείο εγκατάστασης είναι κατεστραμμένο ή είναι σε λάθος έκδοση. Ίσως χρειάζεται να προμηθευτείτε ένα νέο αντίγραφο του προγράμματος.
InvalidParameter=Μία άκυρη παράμετρος χρησιμοποιήθηκε στη γραμμή εντολών:%n%n%1
SetupAlreadyRunning=Η εγκατάσταση τρέχει ήδη.
WindowsVersionNotSupported=Αυτό το πρόγραμμα δεν υποστηρίζει την έκδοση των Windows που τρέχει ο υπολογιστής σας.
WindowsServicePackRequired=Αυτό το πρόγραμμα χρειάζεται %1 Service Pack %2 ή νεότερο.
NotOnThisPlatform=Αυτό το πρόγραμμα δεν μπορεί να εκτελεστεί σε %1.
OnlyOnThisPlatform=Αυτό το πρόγραμμα εκτελείται μόνο σε %1.
OnlyOnTheseArchitectures=Αυτό το πρόγραμμα μπορεί να εγκατασταθεί μονό σε Windows σχεδιασμένα για επεξεργαστές με αρχιτεκτονική:%n%n%1
MissingWOW64APIs=Η έκδοση των Windows που εκτελείτε δεν διαθέτει λειτουργικότητα 64-bit. Για να διορθωθεί το πρόβλημα εγκαταστήσετε το Service Pack %1.
WinVersionTooLowError=Αυτό το πρόγραμμα απαιτεί %1 έκδοση ή νεότερη.
WinVersionTooHighError=Αυτό το πρόγραμμα δεν μπορεί να εκτελεστεί σε %1 έκδοση ή νεότερη.
AdminPrivilegesRequired=Πρέπει να είστε ο Διαχειριστής συστήματος για να εγκαταστήσετε αυτό το πρόγραμμα.
PowerUserPrivilegesRequired=Πρέπει να είστε ο Διαχειριστής συστήματος ή Power User για να εγκαταστήσετε αυτό το πρόγραμμα.
SetupAppRunningError=Η εγκατάσταση εντόπισε ότι εκτελείται η εφαρμογή %1.%n%nΠαρακαλώ κλείστε την εφαρμογή τώρα και πατήστε Εντάξει για να συνεχίσετε, ή ’κυρο για έξοδο.
UninstallAppRunningError=Η απεγκατάσταση εντόπισε ότι εκτελείται η εφαρμογή %1.%n%nΠαρακαλώ κλείστε την εφαρμογή τώρα και πατήστε Εντάξει για να συνεχίσετε, ή ’κυρο για έξοδο.

; *** Misc. errors
ErrorCreatingDir=Η εγκατάσταση δεν μπορεί να δημιουργήσει τον φάκελο %1
ErrorTooManyFilesInDir=Δεν μπορεί να δημιουργηθεί ένα αρχείο στον φάκελο "%1" επειδή ήδη περιέχει πολλά αρχεία

; *** Setup common messages
ExitSetupTitle=Τέλος Εγκατάστασης
ExitSetupMessage=Η εγκατάσταση δεν έχει τελειώσει. Αν τη σταματήσετε τώρα, το πρόγραμμα δεν θα εγκατασταθεί.%n%nΜπορείτε να εκτελέσετε ξανά την εγκατάσταση αργότερα.%n%nΈξοδος;
AboutSetupMenuItem=&Σχετικά με την Εγκατάσταση...
AboutSetupTitle=Σχετικά με την Εγκατάσταση
AboutSetupMessage=%1 έκδοση %2%n%3%n%n%1 προσωπική σελίδα%n%4
AboutSetupNote=
TranslatorNote=Αρχική μετάφραση από τον Anastasis Chatzioglou.%nΕνημερώσεις και βελτιώσεις από τον XhmikosR, my_nickname at yahoo dot com

; *** Buttons
ButtonBack=< &Πίσω
ButtonNext=&Επόμενο >
ButtonInstall=&Εγκατάσταση
ButtonOK=Ε&ντάξει
ButtonCancel=&Ακυρο
ButtonYes=Ν&αι
ButtonYesToAll=Ναι σε &Ολα
ButtonNo=Ό&χι
ButtonNoToAll=Όχι &σε όλα
ButtonFinish=&Τέλος
ButtonBrowse=&Αναζήτηση...
ButtonWizardBrowse=&Εύρεση...
ButtonNewFolder=&Δημιουργία νέου φακέλου

; *** "Select Language" dialog messages
SelectLanguageTitle=Επιλογή της γλώσσας εγκατάστασης
SelectLanguageLabel=Επιλέξτε τη γλώσσα που θα χρησιμοποιηθεί κατά τη διάρκεια της εγκατάστασης:

; *** Common wizard text
ClickNext=Πατήστε Επόμενο για να συνεχίσετε ή ’κυρο για να τερματίσετε την εγκατάσταση.
BeveledLabel=
BrowseDialogTitle=Εύρεση φακέλου
BrowseDialogLabel=Επιλέξτε ένα φάκελο από την ακόλουθη λίστα και μετά πατήστε OK.
NewFolderName=Νέος φάκελος

; *** "Welcome" wizard page
WelcomeLabel1=Καλωσορίσατε στην εγκατάσταση του [name]
WelcomeLabel2=Θα γίνει εγκατάσταση του [name/ver] στον υπολογιστή σας.%n%nΠριν συνεχίσετε σας συνιστούμε να κλείσετε κάθε άλλη εφαρμογή πριν συνεχίσετε.

; *** "Password" wizard page
WizardPassword=Εισαγωγή Κωδικού
PasswordLabel1=Αυτή η εγκατάσταση προστατεύεται με κωδικό.
PasswordLabel3=Παρακαλώ εισάγετε τον κωδικό και πατήστε Επόμενο.
PasswordEditLabel=&Κωδικός:
IncorrectPassword=Ο κωδικός που εισάγατε είναι λάθος. Παρακαλώ, προσπαθήστε ξανά.

; *** "License Agreement" wizard page
WizardLicense=’δεια Χρήσης
LicenseLabel=Παρακαλώ διαβάστε προσεκτικά τις παρακάτω πληροφορίες πριν συνεχίσετε.
LicenseLabel3=Παρακαλώ διαβάστε προσεκτικά τις παρακάτω πληροφορίες πριν συνεχίσετε. Πρέπει να αποδεχθείτε τους όρους της ’δειας Χρήσης πριν συνεχίσετε την εγκατάσταση.
LicenseAccepted=&Αποδέχομαι τους όρους της ’δειας Χρήσης
LicenseNotAccepted=Δεν &αποδέχομαι τους όρους της ’δειας Χρήσης

; *** "Information" wizard pages
WizardInfoBefore=Πληροφορίες
InfoBeforeLabel=Παρακαλώ διαβάστε προσεκτικά τις παρακάτω πληροφορίες πριν συνεχίσετε.
InfoBeforeClickLabel=Όταν είστε έτοιμοι να συνεχίσετε με την Εγκατάσταση, πατήστε Επόμενο.
WizardInfoAfter=Πληροφορίες
InfoAfterLabel=Παρακαλώ διαβάστε προσεκτικά τις παρακάτω πληροφορίες πριν συνεχίσετε.
InfoAfterClickLabel=Όταν είστε έτοιμοι να συνεχίσετε με την Εγκατάσταση, πατήστε Επόμενο.

; *** "User Information" wizard page
WizardUserInfo=Πληροφορίες Χρήστη
UserInfoDesc=Παρακαλώ εισάγετε τις πληροφορίες σας.
UserInfoName=&Ονομα Χρήστη:
UserInfoOrg=&Εταιρεία:
UserInfoSerial=&Σειριακός Αριθμός:
UserInfoNameRequired=Πρέπει να εισάγετε ένα όνομα.

; *** "Select Destination Location" wizard page
WizardSelectDir=Επιλέξτε τον φάκελο στον οποίο θα εγκατασταθεί το πρόγραμμα
SelectDirDesc=Πού θα εγκατασταθεί το [name];
SelectDirLabel3=Το [name] θα εγκατασταθεί στον ακόλουθο φάκελο.
SelectDirBrowseLabel=Για συνέχεια πατήστε Επόμενο. Αν θέλετε άλλο φάκελο, πατήστε Εύρεση.
DiskSpaceMBLabel=Αυτό το πρόγραμμα χρειάζεται [mb] MB χώρο στον δίσκο.
CannotInstallToNetworkDrive=Η εγκατάσταση δεν μπορεί να γίνει σε δίσκο δικτύου.
CannotInstallToUNCPath=Η εγκατάσταση δεν μπορεί να γίνει σε διαδρομή UNC.
InvalidPath=Πρέπει να δώσετε την πλήρη διαδρομή με το γράμμα δίσκου: π.χ. %n%nC:\APP%n%nή μια διαδρομή UNC της μορφής:%n%n\\server\share
InvalidDrive=Ο τοπικός δίσκος ή ο δίσκος δικτύου που επιλέξατε δεν υπάρχει ή δεν είναι προσβάσιμος. Επιλέξτε άλλον.
DiskSpaceWarningTitle=Δεν υπάρχει αρκετός χώρος στο δίσκο
DiskSpaceWarning=Η εγκατάσταση χρειάζεται τουλάχιστον %1 KB ελεύθερο χώρο στο δίσκο αλλά ο επιλεγμένος οδηγός διαθέτει μόνον %2 KB.%n%nΘέλετε να συνεχίσετε οπωσδήποτε;
DirNameTooLong=Το όνομα του φακέλου είναι πολύ μεγάλο.
InvalidDirName=Λάθος όνομα φακέλου.
BadDirName32=Τα ονόματα φακέλων δεν μπορούν να περιέχουν κάποιον από τους παρακάτω χαρακτήρες:%n%n%1
DirExistsTitle=Ο φάκελος υπάρχει
DirExists=Ο φάκελος:%n%n%1%n%nυπάρχει ήδη. Θέλετε να γίνει η εγκατάσταση σε αυτόν τον φάκελο;
DirDoesntExistTitle=Ο φάκελος δεν υπάρχει
DirDoesntExist=Ο φάκελος:%n%n%1%n%nδεν υπάρχει. Θέλετε να δημιουργηθεί;

; *** "Select Components" wizard page
WizardSelectComponents=Επιλογή Συστατικών
SelectComponentsDesc=Ποια συστατικά θέλετε να εγκατασταθούν;
SelectComponentsLabel2=Επιλέξτε τα συστατικά που θέλετε να εγκαταστήσετε και πατήστε Επόμενο για συνέχεια της εγκατάστασης.
FullInstallation=Πλήρης Εγκατάσταση
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Περιορισμένη Εγκατάσταση
CustomInstallation=Προσαρμοσμένη Εγκατάσταση
NoUninstallWarningTitle=Τα συστατικά υπάρχουν
NoUninstallWarning=Η εγκατάσταση εντόπισε ότι τα ακόλουθα συστατικά είναι ήδη εγκατεστημένα στον υπολογιστή σας:%n%n%1%n%nΑποεπιλέγοντας αυτά τα συστατικά δεν θα απεγκατασταθούν.%n%nΘέλετε να συνεχίσετε παρόλα αυτά;
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=Η συγκεκριμένη επιλογή απαιτεί τουλάχιστον [mb] MB ελεύθερο χώρο στον δίσκο.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Επιλογή Επιπλέον Ενεργειών
SelectTasksDesc=Ποιες επιπλέον ενέργειες θέλετε να γίνουν;
SelectTasksLabel2=Επιλέξτε τις επιπλέον ενέργειες που θέλετε να γίνουν κατά την εγκατάσταση του [name] και πατήστε Επόμενο.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Επιλογή Καταλόγου Στο Μενού Εκκίνηση
SelectStartMenuFolderDesc=Πού θα τοποθετηθούν οι συντομεύσεις του προγράμματος;
SelectStartMenuFolderLabel3=Η εγκατάσταση θα δημιουργήσει τις συντομεύσεις του προγράμματος στην ακόλουθη ομάδα.
SelectStartMenuFolderBrowseLabel=Για συνέχεια, πατήστε Επόμενο. Αν θέλετε άλλη ομάδα, πατήστε Εύρεση.
MustEnterGroupName=Πρέπει να δώσετε το όνομα ενός καταλόγου.
GroupNameTooLong=Το όνομα του καταλόγου είναι πολύ μεγάλο.
InvalidGroupName=Το όνομα του καταλόγου δεν είναι σωστό.
BadGroupName=Ονόματα καταλόγων δεν μπορούν να περιέχουν κάποιον από τους παρακάτω χαρακτήρες:%n%n%1
NoProgramGroupCheck2=&Χωρίς δημιουργία καταλόγου στο μενού Έναρξης.

; *** "Ready to Install" wizard page
WizardReady=Έτοιμος για εγκατάσταση
ReadyLabel1=Η εγκατάσταση του [name] είναι έτοιμη να εκτελεστεί στον υπολογιστή σας.
ReadyLabel2a=Πατήστε Εγκατάσταση για να συνεχίσετε ή Πίσω αν θέλετε να αλλάξετε κάποιες ρυθμίσεις.
ReadyLabel2b=Πατήστε Εγκατάσταση για να συνεχίσετε.
ReadyMemoUserInfo=Πληροφορίες Χρήστη:
ReadyMemoDir=Φάκελος προορισμού:
ReadyMemoType=Είδος εγκατάστασης:
ReadyMemoComponents=Επιλεγμένα συστατικά:
ReadyMemoGroup=Κατάλογος στο μενού Προγράμματα:
ReadyMemoTasks=Επιπλέον Ενέργειες:

; *** "Preparing to Install" wizard page
WizardPreparing=Προετοιμασία Εγκατάστασης
PreparingDesc=Η εγκατάσταση προετοιμάζεται να εγκαταστήσει το πρόγραμμα [name] στον υπολογιστή σας.
PreviousInstallNotCompleted=Η εγκατάσταση/αφαίρεση ενός προηγούμενου προγράμματος δεν ολοκληρώθηκε. Θα χρειαστεί να επανεκκινήσετε τον υπολογιστή σας ώστε να ολοκληρώσετε εκείνη την εγκατάσταση.%n%nΜετά την επανεκκίνηση του υπολογιστή σας, εκτελέστε την Εγκατάσταση πάλι για να ολοκληρώσετε την εγκατάσταση του [name].
CannotContinue=Η εγκατάσταση δε μπορεί να συνεχίσει. Παρακαλώ πατήστε ’κυρο για τερματισμό.
ApplicationsFound=Οι ακόλουθες εφαρμογές χρησιμοποιούν αρχεία τα οποία πρέπει να ενημερωθούν από την Εγκατάσταση. Προτείνεται να επιτρέψετε στην Εγκατάσταση να κλείσει αυτές τις εφαρμογές αυτόματα.
ApplicationsFound2=Οι ακόλουθες εφαρμογές χρησιμοποιούν αρχεία τα οποία πρέπει να ενημερωθούν από την Εγκατάσταση. Προτείνεται να επιτρέψετε στην Εγκατάσταση να κλείσει αυτές τις εφαρμογές αυτόματα. Μετά την ολοκλήρωση της διαδικασίας, η Εγκατάσταση θα προσπαθήσει να επανεκκινήσει τις εφαρμογές.
CloseApplications=&Αυτόματο κλείσιμο των εφαρμογών
DontCloseApplications=&Χωρίς κλείσιμο των εφαρμογών

; *** "Installing" wizard page
WizardInstalling=Πρόοδος Εγκατάστασης
InstallingLabel=Παρακαλώ περιμένετε να ολοκληρωθεί η εγκατάσταση του [name] στον υπολογιστή σας.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=Ολοκληρώνοντας τον Οδηγό Εγκατάστασης του [name]
FinishedLabelNoIcons=Η εγκατάσταση του [name] στον υπολογιστή σας ολοκληρώθηκε με επιτυχία.
FinishedLabel=Η εγκατάσταση του [name] στον υπολογιστή σας ολοκληρώθηκε με επιτυχία. Μπορείτε να ξεκινήσετε το πρόγραμμα επιλέγοντας τα εικονίδια που δημιουργήθηκαν.
ClickFinish=Πατήστε Τέλος για να τερματίσετε το πρόγραμμα εγκατάστασης.
FinishedRestartLabel=Για να ολοκληρωθεί η εγκατάσταση του [name] πρέπει να γίνει επανεκκίνηση του υπολογιστή σας. Θέλετε να γίνει επανεκκίνηση τώρα;
FinishedRestartMessage=Για να ολοκληρωθεί η εγκατάσταση του [name] πρέπει να γίνει επανεκκίνηση του υπολογιστή σας.%n%nΘέλετε να γίνει επανεκκίνηση τώρα;
ShowReadmeCheck=Ναι, θέλω να διαβάσω τις πληροφορίες του προγράμματος
YesRadio=&Ναι, να γίνει επανεκκίνηση τώρα
NoRadio=&Οχι, θα κάνω επανεκκίνηση αργότερα
; used for example as 'Run MyProg.exe'
RunEntryExec=Να εκτελεστεί το πρόγραμμα %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Να εκτελεστεί το %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Η Εγκατάσταση χρειάζεται τον επόμενο δίσκο/δισκέτα
SelectDiskLabel2=Παρακαλώ, εισάγετε το Δίσκο/Δισκέτα %1 και πατήστε OK.%n%nΑν τα αρχεία αυτού του δίσκου/δισκέτας βρίσκονται σε κάποιον φάκελο εκτός αυτού που φαίνεται παρακάτω, εισάγετε τη σωστή διαδρομή ή πατήστε Εύρεση.
PathLabel=&Διαδρομή:
FileNotInDir2=Το αρχείο "%1" δε βρέθηκε στο "%2". Παρακαλώ εισάγετε το σωστό δίσκο/δισκέτα ή επιλέξτε κάποιον άλλο φάκελο.
SelectDirectoryLabel=Παρακαλώ καθορίσετε την τοποθεσία του επόμενου δίσκου/δισκέτας.

; *** Installation phase messages
SetupAborted=Η εγκατάσταση δεν ολοκληρώθηκε.%n%nΠαρακαλώ, διορθώστε το πρόβλημα και εκτελέστε ξανά την Εγκατάσταση.
EntryAbortRetryIgnore=Πατήστε Retry για να ξαναπροσπαθήσετε, Ignore για να συνεχίσετε ή Abort για να τερματίσετε την Εγκατάσταση.

; *** Installation status messages
StatusClosingApplications=Κλείσιμο εφαρμογών...
StatusCreateDirs=Δημιουργία φακέλων...
StatusExtractFiles=Αποσυμπίεση αρχείων...
StatusCreateIcons=Δημιουργία εικονιδίων...
StatusCreateIniEntries=Καταχώρηση στο ΙΝΙ αρχείο...
StatusCreateRegistryEntries=Καταχώρηση στο μητρώο συστήματος...
StatusRegisterFiles=Καταχώρηση αρχείων...
StatusSavingUninstall=Πληροφορίες απεγκατάστασης...
StatusRunProgram=Τελειώνοντας την εγκατάσταση...
StatusRestartingApplications=Επανεκκίνηση εφαρμογών...
StatusRollback=Επαναφορά αλλαγών...

; *** Misc. errors
ErrorInternal2=Εσωτερικό σφάλμα: %1
ErrorFunctionFailedNoCode=%1 Σφάλμα
ErrorFunctionFailed=%1 απέτυχε, κωδικός %2
ErrorFunctionFailedWithMessage=%1 απέτυχε, κωδικός %2.%n%3
ErrorExecutingProgram=Αδύνατη η εκτέλεση του αρχείου:%n%1

; *** Registry errors
ErrorRegOpenKey=Δεν μπορεί να διαβαστεί το κλειδί μητρώου συστήματος:%n%1\%2
ErrorRegCreateKey=Δεν μπορεί να δημιουργηθεί το κλειδί μητρώου συστήματος:%n%1\%2
ErrorRegWriteKey=Δεν μπορεί να γίνει καταχώρηση στο κλειδί μητρώου συστήματος:%n%1\%2

; *** INI errors
ErrorIniEntry=Δεν μπορεί να γίνει καταχώρηση στο αρχείο ΙΝΙ "%1".

; *** File copying errors
FileAbortRetryIgnore=Πατήστε Retry για να ξαναπροσπαθήσετε, Ignore για να παραλείψετε το αρχείο (δεν προτείνεται) ή Abort για να τερματίσετε την εγκατάσταση.
FileAbortRetryIgnore2=Πατήστε Retry για να ξαναπροσπαθήσετε, Ignore για να παραλείψετε το αρχείο (δεν προτείνεται) ή Abort για να τερματίσετε την εγκατάσταση.
SourceIsCorrupted=Το αρχείο προέλευσης είναι κατεστραμμένο
SourceDoesntExist=Το αρχείο προέλευσης "%1" δεν υπάρχει
ExistingFileReadOnly=Το αρχείο είναι μαρκαρισμένο μόνο για ανάγνωση.%n%nΠατήστε Retry για να το ξεμαρκάρετε και να προσπαθήσετε πάλι, Ignore για να παραλείψετε το αρχείο ή Abort για να τερματίσετε την εγκατάσταση.
ErrorReadingExistingDest=Παρουσιάστηκε σφάλμα κατά την ανάγνωση του αρχείου:
FileExists=Το αρχείο υπάρχει ήδη.%n%nΘέλετε να ξαναγραφτεί;
ExistingFileNewer=Ένα αρχείο που βρέθηκε στον υπολογιστή σας είναι νεότερης έκδοσης από εκείνο της εγκατάστασης. Προτείνεται να κρατήσετε το υπάρχον αρχείο.%n%nΘέλετε να κρατήσετε το υπάρχον αρχείο;
ErrorChangingAttr=Προέκυψε σφάλμα στην προσπάθεια να αλλαχτούν τα χαρακτηριστικά του αρχείου:
ErrorCreatingTemp=Προέκυψε σφάλμα στην προσπάθεια να δημιουργηθεί ένα αρχείο στον κατάλογο προορισμού:
ErrorReadingSource=Προέκυψε σφάλμα στην προσπάθεια ανάγνωσης του αρχείου προέλευσης:
ErrorCopying=Προέκυψε σφάλμα στην προσπάθεια να αντιγραφεί το αρχείο:
ErrorReplacingExistingFile=Προέκυψε σφάλμα στην προσπάθεια να αντικατασταθεί το υπάρχον αρχείο:
ErrorRestartReplace=Προέκυψε σφάλμα στην προσπάθεια να γίνει επανεκκίνηση και αντικατάσταση αρχείου:
ErrorRenamingTemp=Προέκυψε σφάλμα στην προσπάθεια μετονομασίας ενός αρχείου στον κατάλογο προορισμού:
ErrorRegisterServer=Προέκυψε σφάλμα στην προσπάθεια καταχώρησης DLL/OCX: %1
ErrorRegSvr32Failed=Το RegSvr32 απέτυχε με κωδικό εξόδου %1
ErrorRegisterTypeLib=Αδύνατη η καταχώρηση της βιβλιοθήκης τύπων: %1

; *** Post-installation errors
ErrorOpeningReadme=Προέκυψε σφάλμα στην προσπάθεια να φορτωθεί το αρχείο πληροφοριών.
ErrorRestartingComputer=Προέκυψε σφάλμα στην προσπάθεια επανεκκίνησης του υπολογιστή. Παρακαλώ επανεκκινήσετε τον υπολογιστή σας μόνοι σας.

; *** Uninstaller messages
UninstallNotFound=Το αρχείο "%1" δεν βρέθηκε. Η απεγκατάσταση δεν μπορεί να γίνει.
UninstallOpenError=Το αρχείο "%1" δεν μπόρεσε να φορτωθεί. Η απεγκατάσταση δεν μπορεί να γίνει
UninstallUnsupportedVer=Το αρχείο "%1" δεν αναγνωρίζεται από αυτή την έκδοση της εγκατάστασης, Η απεγκατάσταση δεν μπορεί να εκτελεστεί
UninstallUnknownEntry=Το αρχείο "%1" δεν αναγνωρίζεται από αυτή την έκδοση της εγκατάστασης, Η απεγκατάσταση δεν μπορεί να εκτελεστεί
ConfirmUninstall=Είστε σίγουροι ότι θέλετε να διαγράψετε το %1 και όλα τα συστατικά του;
UninstallOnlyOnWin64=Αυτή η εφαρμογή μπορεί να απεγκατασταθεί μόνο σε 64-bit Windows.
OnlyAdminCanUninstall=Η απεγκατάσταση μπορεί να εκτελεστεί μόνο από τον Διαχειριστή συστήματος.
UninstallStatusLabel=Παρακαλώ περιμένετε όσο το %1 διαγράφεται από τον υπολογιστή σας.
UninstalledAll=Η απεγκατάσταση του %1 έγινε με επιτυχία.
UninstalledMost=Η απεγκατάσταση του %1 έγινε με επιτυχία.%n%nΚάποια συστατικά δεν ήταν δυνατόν να διαγραφούν. Αυτά μπορούν να διαγραφούν από εσάς.
UninstalledAndNeedsRestart=Για να ολοκληρώσετε την απεγκατάσταση του %1, θα πρέπει να επανεκκινήσετε τον υπολογιστή σας.%n%nΘα θέλατε να κάνετε επανεκκίνηση τώρα;
UninstallDataCorrupted="%1" Αυτό το αρχείο είναι κατεστραμμένο. Δεν μπορεί να γίνει απεγκατάσταση

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Θέλετε να διαγραφούν τα κοινά αρχεία;
ConfirmDeleteSharedFile2=Τα κοινά αρχεία δεν χρησιμοποιούνται από κάποιο πρόγραμμα. Θέλετε να διαγραφούν;%n%nΑν κάποιο πρόγραμμα τα χρησιμοποιεί ίσως δεν εκτελείται σωστά αν τα διαγράψετε. Αν δεν είστε σίγουροι, επιλέξτε Όχι. Αφήνοντάς τα στο σύστημά σας δεν προκαλούν κάποιο πρόβλημα.
SharedFileNameLabel=Όνομα Αρχείου:
SharedFileLocationLabel=Τοποθεσία:
WizardUninstalling=Πρόοδος Απεγκατάστασης
StatusUninstalling=Απεγκατάσταση του %1...

; *** Shutdown block reasons
ShutdownBlockReasonInstallingApp=Εγκατάσταση του %1.
ShutdownBlockReasonUninstallingApp=Απεγκατάσταση του %1.

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 έκδοση %2
AdditionalIcons=Επιπλέον εικονίδια:
CreateDesktopIcon=Δημιουργία ενός &εικονιδίου στην επιφάνεια εργασίας
CreateQuickLaunchIcon=Δημιουργία ενός εικονιδίου στη &Γρήγορη Εκκίνηση
ProgramOnTheWeb=Το %1 στο Internet
UninstallProgram=Απεγκατάσταση του %1
LaunchProgram=Εκκίνηση του %1
AssocFileExtension=%Αντιστοίχιση του %1 με την %2 επέκταση αρχείου
AssocingFileExtension=Γίνεται αντιστοίχηση του %1 με την %2 επέκταση αρχείου...
AutoStartProgramGroupDescription=Εκκίνηση:
AutoStartProgram=Αυτόματη εκκίνηση του %1
AddonHostProgramNotFound=Το %1 δε βρέθηκε στο φάκελο που επιλέξατε.%n%nΘέλετε να συνεχίσετε παρόλα αυτά;
