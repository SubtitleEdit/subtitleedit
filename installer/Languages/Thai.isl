; Translation made with Stonevoice Translator 2.2 (http://www.stonevoice.com/auto/translator)
; $Translator:NL=%n:TB=%t
; Suwat Yangfuang, Ekachai Omkaew
; suwat.yang@gmail.com, ekaomk@gmail.com
; 
; *** Inno Setup version 5.5.3+ Thai messages ***
;
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
LanguageName=Thai
LanguageID=$041E
LanguageCodePage=874
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
SetupAppTitle=การติดตั้ง
SetupWindowTitle=การติดตั้ง - %1
UninstallAppTitle=ยกเลิกการติดตั้ง
UninstallAppFullTitle=%1 ยกเลิกการติดตั้ง

; *** Misc. common
InformationTitle=คำอธิบาย
ConfirmTitle=การยืนยัน
ErrorTitle=ความผิดพลาด

; *** SetupLdr messages
SetupLdrStartupMessage=จะเริ่มการติดตั้ง %1. คุณต้องการติดตั้งโปรแกรมหรือไม่?
LdrCannotCreateTemp=ไม่สามารถ สร้างแฟ้มข้อมูลชั่วคราวได้ การติดตั้งยกเลิก
LdrCannotExecTemp=ไม่สามารถ ใช้ไฟล์ใน ที่เก็บข้อมูลชั่วคราวได้ การติดตั้งยกเลิก

; *** Startup error messages
LastErrorMessage=%1.%n%nความผิดพลาดเรื่อง %2: %3
SetupFileMissing=ไฟล์ %1 ในโฟลเดอร์ที่ติดตั้งไม่สมบูรณ์ กรุณาแก้ไขใช้ไฟล์การติดตั้งที่สมบูรณ์
SetupFileCorrupt=ไฟล์การติดตั้งเสียหาย กรุณาแก้ไขใช้ไฟล์การติดตั้งที่สมบูรณ์
SetupFileCorruptOrWrongVer=เวอร์ชั่นไฟล์การติดตั้งไม่ถูกต้อง กรุณาแก้ไขใช้ไฟล์การติดตั้งที่สมบูรณ์
InvalidParameter=พารามิเตอร์ผิดพลาดที่ชุดคำสั่ง:%n%n%1
SetupAlreadyRunning=การติดตั้งกำลังดำเนินการอยู่แล้ว
WindowsVersionNotSupported=โปรแกรมไม่รองรับเวอร์ชันของวินโดวน์ที่คุณใช้งานอยู่
WindowsServicePackRequired=โปรแกรมต้องการ %1 Service Pack %2 ขึ้นไป.
NotOnThisPlatform=โปรแกรมนี้ไม่ทำงาน ในระบบ %1.
OnlyOnThisPlatform=โปรแกรมนี้ ต้องทำงานในระบบ %1.
OnlyOnTheseArchitectures=โปรแกรมนี้สามารถติดตั้งใน Windows รุ่นที่ออกแบบมาสำหรับสถาปัตยกรรมของหน่วยประมวลผลดังต่อไปนี้:%n%n%1
MissingWOW64APIs=รุ่นของ Windows ที่คุณใช้อยู่ไม่จำเป็นต้องมีการใช้งานโดยติดตั้งเพื่อทำการติดตั้งแบบ 64 - bit เมื่อต้องการแก้ไขปัญหานี้โปรดติดตั้ง Service Pack %1.
WinVersionTooLowError=โปรแกรมนี้ต้องการระบบ %1 เวอร์ชั่น %2 หรือใหม่กว่า
WinVersionTooHighError=โปรแกรมนี้ไม่สามารถ ติดตั้งในระบบ %1 เวอร์ชั่น %2 หรือใหม่กว่า
AdminPrivilegesRequired=คุณต้องใช้ USER ของผู้ดูแลระบบเช่น administrator เพี่อติดตั้งโปรแกรมนี้.
PowerUserPrivilegesRequired=คุณต้องเข้าสู่ระบบ ด้วยผู้ใช้งาน ที่เป็นผู้ดูแลระบบ เช่น administrator หรือ เป็นผู้ใช้งาน ที่อยู่ในกลุ่ม Power Users เมื่อต้องการติดตั้งโปรแกรมนี้
SetupAppRunningError=มีโปรแกรม %1 กำลังทำงานอยู่%n%nกรุณาปิดโปรแกรม และคลิก ตกลง เพื่อทำงานต่อ หรือ ยกเลิก เพื่อจบการติดตั้ง
UninstallAppRunningError=พบว่าโปรแกรม %1 การกำลังทำงานอยู่%n%nกรุณาปิดโปรแกรม และคลิก ตกลง เพื่อทำงานต่อ หรือ ยกเลิก เพื่อจบการทำงาน

; *** Misc. errors
ErrorCreatingDir=ไม่สามารถสร้างโฟลเดอร์ "%1" ได้
ErrorTooManyFilesInDir=ไม่สามารถสร้างไฟล์ในโฟลเดอร์  "%1" เพราะมีไฟล์จำนวนมากเกินไป

; *** Setup common messages
ExitSetupTitle=ออกจาก การติดตั้ง
ExitSetupMessage=การติดตั้งจะไม่สมบูรณ์ ถ้าคุณจบการทำงานในเวลานี้%n%nคุณจะต้องทำการติดตั้งโปรแกรมใหม่อีกครั้ง เพื่อให้การติดตั้งสมบูรณ์%n%nคุณต้องการจบการติดตั้ง?
AboutSetupMenuItem=เ&กี่ยวกับ การติดตั้ง...
AboutSetupTitle=เกี่ยวกับ โปรแกรมการติดตั้ง
AboutSetupMessage=%1 เวอร์ชั่น %2%n%3%n%n%1 โฮมเพจ:%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< &ย้อนกลับ
ButtonNext=&ทำต่อ >
ButtonInstall=&ติดตั้ง
ButtonOK=ตกลง
ButtonCancel=ยกเลิก
ButtonYes=ใ&ช่
ButtonYesToAll=ใช่ &ทั้งหมด
ButtonNo=ไ&ม่
ButtonNoToAll=ไม่ &ทั้งหมด
ButtonFinish=เ&สร็จ
ButtonBrowse=เ&ลือก...
ButtonWizardBrowse=เ&ลือก...
ButtonNewFolder=&สร้างโฟลเดอร์ใหม่

; *** "Select Language" dialog messages
SelectLanguageTitle=เลือกภาษาที่ต้องการ
SelectLanguageLabel=เลือกภาษา ที่ต้องการใช้ระหว่าง การติดตั้งโปรแกรม

; *** Common wizard text
ClickNext=คลิก ทำต่อ > เพื่อทำงาน หรือ คลิก ยกเลิก จบการทำงาน
BeveledLabel=
BrowseDialogTitle=เลือกโฟลเดอร์
BrowseDialogLabel=เลือกโฟลเดอร์ในรายการด้านล่างจากนั้นคลิก ตกลง.
NewFolderName=โฟลเดอร์ใหม่

; *** "Welcome" wizard page
WelcomeLabel1=ขอต้อนรับสู่ โปรแกรมการติดตั้ง [name]
WelcomeLabel2=ก่อนการติดตั้งโปรแกรม [name/ver] สู่เครื่องคอมพิวเตอร์ของคุณ%n%nเราขอแนะนำคุณให้ ปิดโปรแกรมที่ไม่เกี่ยวข้องทั้งหมด ก่อนการติดตั้งเพื่อป้องกันการเกิดปํญหาในการติดตั้งโปรแกรม

; *** "Password" wizard page
WizardPassword=รหัสผ่าน
PasswordLabel1=การติดตั้งนี้ ถูกป้องกันด้วยรหัสผ่าน
PasswordLabel3=กรุณาใส่รหัสผ่านที่ใช้ติดตั้งโปรแกรม คลิก ทำต่อ เพื่อทำงาน  (Passwords are case-sensitive)
PasswordEditLabel=&รหัสผ่าน:
IncorrectPassword=รหัสผ่านที่ใช้ไม่ถูกต้อง กรูณาทดลองใส่อีกครั้ง

; *** "License Agreement" wizard page
WizardLicense=สัญญา ข้อตกลง
LicenseLabel=กรุณาอ่าน สัญญาข้อตกลง ที่สำคัญนี้ก่อน การติดตั้งโปรแกรม
LicenseLabel3=กรุณาอ่าน สัญญาข้อตกลง ที่จะแสดงต่อไป คุณต้องยอมรับ เงื่อนไขระยะเวลา ที่กำหนดในสัญญา ก่อนเริ่มการติดตั้งโปรแกรม ต่อไป
LicenseAccepted=ฉัน &ยอมรับ ข้อสัญญา
LicenseNotAccepted=ฉัน &ไม่ยอมรับ ข้อสัญญา

; *** "Information" wizard pages
WizardInfoBefore=คำอธิบาย
InfoBeforeLabel=กรุณาอ่านข้อมูลสำคัญ ก่อนทำการติดตั้งโปรแกรม
InfoBeforeClickLabel=ถ้าคุณพร้อมจะติดตั้งโปรแกรม คลิกที่ปุ่ม ทำต่อ >
WizardInfoAfter=คำอธิบาย
InfoAfterLabel=กรุณาอ่านข้อมูลสำคัญ ก่อนทำงานขั้นตอนต่อไป
InfoAfterClickLabel=ถ้าคุณพร้อมจะทำขั้นตอนต่อไป คลิกที่ปุ่ม ทำต่อ >

; *** "User Information" wizard page
WizardUserInfo=ข้อมูล สำหรับผู้ใช้งาน
UserInfoDesc=กรุณา กรอกข้อมูล ของคุณ
UserInfoName=&ผู้ใช้งาน
UserInfoOrg=&หน่วยงาน
UserInfoSerial=รหัส &Serial Number
UserInfoNameRequired=คุณต้อง ใส่ชื่อของคุณ

; *** "Select Destination Location" wizard page
WizardSelectDir=เลือกโฟลเดอร์ที่ต้องการติดตั้งโปรแกรม
SelectDirDesc=โฟลเดอร์ที่ติดตั้งโปรแกรมคือ [name] ?
SelectDirLabel3=การติดตั้งจะติดตั้ง [name] ลงในโฟลเดอร์ต่อไปนี้
SelectDirBrowseLabel=ดำเนินการต่อไปให้คลิกที่ปุ่ม ทำต่อ > ถ้าคุณต้องการเลือกโฟลเดอร์อื่นให้คลิกที่ปุ่ม เลือก...
DiskSpaceMBLabel=การติดตั้งต้องการเนื้อที่ว่างไม่น้อยกว่า [mb] MB
CannotInstallToNetworkDrive=การติดตั้งไม่สามารถติดตั้งไปยังไดร์เครือข่ายได้
CannotInstallToUNCPath=ไม่สามารถติดตั้งลงไปยังตำแหน่ง UNC ได้
InvalidPath=คุณต้องใช้ full path with drive letter; ตามตัวอย่าง:%nC:\APP
InvalidDrive=ไม่มีไดรว์ที่คุณเลือก กรุณาเลือกไดรว์อื่น
DiskSpaceWarningTitle=เนื้อที่ว่างในดิสก์ไม่เพียงพอ
DiskSpaceWarning=การติดตั้งต้องการเนื้อที่ว่าง %1 KB แต่ไดรว์ที่คุณเลือกมีเนื้อที่ว่าง %2 KB%n%nคุณต้องการติดตั้งต่อไปหรือไม่?
DirNameTooLong=ชื่อโฟลเดอร์หรือ path ยาวเกินไป.
InvalidDirName=ชื่อโฟลเดอร์ไม่ถูกต้อง.
BadDirName32=ชื่อโฟลเดอร์ไม่มีสามารถใช้ตัวอักษรใดๆหลังจากเครื่องหมาย :%n%n%1
DirExistsTitle=มีโฟลเดอร์อยู่แล้ว
DirExists=โฟลเดอร์:%n%n%1%n%nในขณะนี้มีอยู่แล้ว คุณต้องการติดตั้งโปรแกรมในโฟลเดอร์นี้เลยหรือไม่?
DirDoesntExistTitle=ไม่พบโฟลเดอร์
DirDoesntExist=โฟลเดอร์:%n%n%1%n%nไม่มีอยู่ในขณะนี้ คุณต้องการสร้างโฟลเดอร์นี้เลยหรือไม่?

; *** "Select Components" wizard page
WizardSelectComponents=เลือกส่วนประกอบ
SelectComponentsDesc=ส่วนประกอบไหนที่คุณต้องการติดตั้ง?
SelectComponentsLabel2=ทำเครื่องหมายเลือกส่วนประกอบที่คุณต้องการติดตั้ง; ลบเครื่องหมายส่วนประกอบที่คุณไม่ต้องการ คลิกทำต่อ เมื่อคุณเลือกเสร็จแล้ว
FullInstallation=ติดตั้งทุกอย่าง
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=ติดตั้งน้อยที่สุด
CustomInstallation=กำหนดส่วนประกอบเอง
NoUninstallWarningTitle=ส่วนประกอบเดิมมีอยู่แล้ว
NoUninstallWarning=พบส่วนประกอบ ที่ต้องการติดตั้งในคอมพิวเตอร์ของคุณแล้ว%n%n%1
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceMBLabel=การติดตั้งต้องการเนื้อที่ว่าง [mb] MB

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=เลือกการทำงาน เพิ่มเติม
SelectTasksDesc=กำหนดการทำงานเพิ่มเติม
SelectTasksLabel2=เลือกการทำงานเพิ่มเติม เพื่อการติดตั้งโปรแกรม [name] และคลิกที่ปุ่ม ทำต่อ >

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=เลือกโฟลเดอร์ Start Menu
SelectStartMenuFolderDesc=กำหนดการติดตั้ง program's shortcuts?
SelectStartMenuFolderLabel3=การติดตั้งจะสร้าง program's shortcuts ต่อไปนี้ในโฟลเดอร์ Start Menu.
SelectStartMenuFolderBrowseLabel=ดำเนินการต่อไปให้คลิกที่ปุ่ม ทำต่อ > ถ้าคุณต้องการเลือกโฟลเดอร์อื่นให้คลิกที่ปุ่ม เลือก...
MustEnterGroupName=คุณต้องใส่ชื่อโฟลเดอร์
GroupNameTooLong=ชื่อโฟลเดอร์หรือ path ยาวเกินไป.
InvalidGroupName=ชื่อโฟลเดอร์ไม่ถูกต้อง.
BadGroupName=ชื่อโฟลเดอร์ไม่สามารถ ใช้ตัวอักษรนี้ในชื่อได้:%n%n%1
NoProgramGroupCheck2=ไ&ม่มีการสร้าง Start Menu folder

; *** "Ready to Install" wizard page
WizardReady=พร้อมที่จะติดตั้ง
ReadyLabel1=กำลังจะเริ่มการติดตั้งโปรแกรม [name] ในเครื่องคอมพิวเตอร์แล้ว
ReadyLabel2a=คลิกที่ปุ่ม ติดตั้ง เพื่อติดตั้งโปรแกรม หรือ คลิกที่ปุ่ม < ย้อนกลับ เพื่อดูการกำหนดค่า ที่ใช้ในการติดตั้งโปรแกรม
ReadyLabel2b=คลิกที่ ติดตั้ง เพื่อติดตั้งโปรแกรม
ReadyMemoUserInfo=ข้อมูล สำหรับผู้ใช้
ReadyMemoDir=โฟลเดอร์ที่ติดตั้งโปรแกรม
ReadyMemoType=ประเภทการติดตั้ง
ReadyMemoComponents=ส่วนประกอบที่เลิอกติดตั้ง
ReadyMemoGroup=โฟลเดอร์ Start Menu
ReadyMemoTasks=การทำงาน เพิ่มเติม:

; *** "Preparing to Install" wizard page
WizardPreparing=การเตรียมการ ที่จะติดตั้งโปรแกรม
PreparingDesc=โปรแกรม กำลังเตรียมการ เพื่อติดตั้ง [name] ที่เครื่องคอมพิวเตอร์ของคุณ
PreviousInstallNotCompleted=การติดตั้ง หรือถอดถอน โปรแกรมเดิมไม่สมบูรณ์ คุณต้อง restart เครื่องคอมพิวเตอร์ เพื่อให้การติดตั้ง ถูกต้องสมบูรณ์%n%nหลังจากการ restart เครื่องคอมพิวเตอร์แล้ว เรียกโปรแกรม Setup นี้อีกครั้ง เพื่อให้การติดตั้ง [name] ถูกต้องสมบูรณ์
CannotContinue=โปรแกรมติดตั้งไม่สามารถทำงานต่อไปได้ กรุณาคลิกปุ่ม ยกเลิก เพื่อออกจากโปรแกรม
ApplicationsFound=The following applications are using files that need to be updated by Setup. It is recommended that you allow Setup to automatically close these applications.
ApplicationsFound2=The following applications are using files that need to be updated by Setup. It is recommended that you allow Setup to automatically close these applications. After the installation has completed, Setup will attempt to restart the applications.
CloseApplications=&Automatically close the applications
DontCloseApplications=&Do not close the applications
ErrorCloseApplications=Setup was unable to automatically close all applications. It is recommended that you close all applications using files that need to be updated by Setup before continuing.

; *** "Installing" wizard page
WizardInstalling=การติดตั้ง
InstallingLabel=กรุณารอสักครู่ กำลังติดตั้งโปรแกรม [name] ในคอมพิวเตอร์

; *** "Setup Completed" wizard page
FinishedHeadingLabel=การติดตั้ง [name] เสร็จสมบูรณ์
FinishedLabelNoIcons=การติดตั้งโปรแกรม [name] ในเครื่องคอมพิวเตอร์ เสร็จสมบูรณ์
FinishedLabel=การติดตั้งโปรแกรม [name] ในเครื่องคอมพิวเตอร์ เสร็จสมบูรณ์ คุณสามารถเรียกใช้โปรแกรมได้จาก Icons ได้แล้ว
ClickFinish=คลิก เสร็จ เพื่อจบการติดตั้งโปรแกรม
FinishedRestartLabel=การติดตั้งโปรแกรม [name] จำเป็นต้อง Restart เครื่องคอมพิวเตอร์ คุณต้องการ Restart เครื่องคอมพิวเตอร์ ในขณะนี้หรือไม่?
FinishedRestartMessage=การติดตั้งโปรแกรม [name] จำเป็นต้อง Restart เครื่องคอมพิวเตอร์%n%n คุณต้องการ Restart เครื่องคอมพิวเตอร์ ในขณะนี้หรือไม่?
ShowReadmeCheck=ใช่ คุณต้องการอ่านไฟล์ README
YesRadio=ใ&ช่, restart คอมพิวเตอร์ทันที
NoRadio=ไ&ม่ คุณต้องการ Restart คอมพิวเตอร์เอง หลังจากนี้
; used for example as 'Run MyProg.exe'
RunEntryExec=Run %1
; used for example as 'View Readme.txt'
RunEntryShellExec=View %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=การติดตั้ง ต้องดิสก์แผ่นต่อไป
SelectDiskLabel2=กรุณาใส่ดิสก์ %1 และ คลิก ตกลง%n%nถ้าไม่มีไฟล์ในดิสก์ แต่มีในโฟลเดอร์อื่น กรุณาใส่โฟลเดอร์ที่ถูกต้อง หรือ คลิก เลือก
PathLabel=&Path:
FileNotInDir2=ไฟล์ "%1" ไม่พบใน "%2"กรุณาใส่แผ่นดิสก์ที่ถูกต้อง หรือ เลือกโฟลเดอร์ที่มีไฟล์สำหรับติดตั้ง
SelectDirectoryLabel=กรุณากำหนดที่อยู่ ของดิสก์แผ่นต่อไป

; *** Installation phase messages
SetupAborted=การติดตั้งไม่สมบูรณ์%n%nกรุณาเรียก ไฟล์การติดตั้งโปรแกรม อีกครั้งหนึ่ง
EntryAbortRetryIgnore=คลิก Retry เพื่อลองทำงานอีกครั้ง, คลิก Ignore เพื่อข้ามการทำงานที่ผิดพลาด, หรือ คลิก Abort ยกเลิกการติดตั้ง

; *** Installation status messages
StatusClosingApplications=กำลังปิดแอปพลิเคชัน...
StatusCreateDirs=การสร้าง โฟลเดอร์ ..
StatusExtractFiles=การขยาย ไฟล์ต่างๆ...
StatusCreateIcons=การสร้าง Program icons...
StatusCreateIniEntries=การสร้าง INI entries...
StatusCreateRegistryEntries=การสร้าง registry entries...
StatusRegisterFiles=ไฟล์ Registering ต่างๆ...
StatusSavingUninstall=การบันทึกข้อมูล การยกเลิกการติดตั้งโปรแกรม...
StatusRunProgram=เสร็จสิ้น การติดตั้งโปรแกรม...
StatusRestartingApplications=กำลังรีสตาร์ทแอปพลิเคชัน...
StatusRollback=เรียกคืน การแก้ไขทั้งหมด...

; *** Misc. errors
ErrorInternal2=เกิดข้อผิดพลาด (Internal error: %1)
ErrorFunctionFailedNoCode=%1 failed
ErrorFunctionFailed=%1 failed; code %2
ErrorFunctionFailedWithMessage=%1 failed; code %2.%n%3
ErrorExecutingProgram=ไม่สามารถขยายไฟล์ได้:%n%1

; *** Registry errors
ErrorRegOpenKey=ผิดพลาดในการเปิด registry key:%n%1\%2
ErrorRegCreateKey=ผิดพลาดในการสร้าง registry key:%n%1\%2
ErrorRegWriteKey=ผิดพลาดในการเขียน registry key:%n%1\%2

; *** INI errors
ErrorIniEntry=เกิดข้อผิดพลาดในการสร้างรายการ INI ไฟล์ "%1".

; *** File copying errors
FileAbortRetryIgnore=คลิก Retry เพื่อลองทำอีกครั้ง คลิก Ignore เพื่อข้ามการติดตั้งไฟล์นี้ (ไม่แนะนำให้ทำ) หรือ คลิก Abort เพื่อยกเลิกการติดตั้ง
FileAbortRetryIgnore2=คลิก Retry เพื่อลองทำอีกครั้ง คลิก Ignore เพื่อข้ามการทำงานนี้ (ไม่แนะนำให้ทำ) หรือ คลิก Abort เพื่อยกเลิกการติดตั้ง
SourceIsCorrupted=ไฟล์ต้นฉบับไม่ถูกต้อง
SourceDoesntExist=ไฟล์ "%1" ไม่มี
ExistingFileReadOnly=ไฟล์ถูกกำหนดคุณสมบัติไว้ เป็นแบบอ่านได้อย่างเดียว( read-only)%n%nคลิก Retry เพื่อยกเลิกคุณสมบัตินี้ และลองทำอีกครั้ง คลิก Ignore เพื่อข้ามการติดตั้งไฟล์นี้ หรือ คลิก Abort เพื่อยกเลิกการติดตั้ง
ErrorReadingExistingDest=เกิดความผิดพลาดขณะอ่านไฟล์:
FileExists=มีไฟล์นี้อยู่แล้ว%n%nคุณต้องเขียนทับไฟล์นี้หรือไม่?
ExistingFileNewer=ไฟล์ในระบบที่มีอยู่เป็น เวอร์ชั่นใหม่กว่า ไฟล์ที่จะติดตั้งลงไป เราแนะนำให้คุณไม่ให้เขียนทับไฟล์ที่มีอยู่นี้%n%nคุณไม่ต้องการเขียนทับไฟล์นี้?
ErrorChangingAttr=เกิดความผิดพลาดจาก การแก้ไขคุณสมบัติไฟล์ที่มีอยู่:
ErrorCreatingTemp=เกิดความผิดพลาดจาก การสร้างไฟล์ในโฟลเดอร์ที่จะทำการติดตั้งโปรแกรม:
ErrorReadingSource=เกิดความผิดพลาดจาก จากการอ่านข้อมูลไฟล์ต้นฉบับ:
ErrorCopying=เกิดความผิดพลาดจาก จากการทำสำเนาไฟล์นี้:
ErrorReplacingExistingFile=เกิดความผิดพลาดจาก จากการเขียนทับ ไฟล์ที่มีอยู่:
ErrorRestartReplace=RestartReplace failed:
ErrorRenamingTemp=เกิดความผิดพลาดจาก จากการเปลี่ยนชื่อไฟล์ ในโฟลเดอร์ที่จะติดตั้งโปรแกรม:
ErrorRegisterServer=ไม่สามารถลงทะเบียน DLL/OCX: %1
ErrorRegSvr32Failed=RegSvr32 failed with exit code %1
ErrorRegisterTypeLib=Unable to register the type library: %1

; *** Post-installation errors
ErrorOpeningReadme=เกิดความผิดพลาดจาก จากการเปิดไฟล์ README:
ErrorRestartingComputer=ไม่สามารถ Restart คอมพิวเตอร์ได้ กรุณาลอง Restart เองอีกครั้งภายหลัง

; *** Uninstaller messages
UninstallNotFound=ไฟล์ "%1" ไม่มี ไม่สามารถยกเลิกการติดตั้งได้
UninstallOpenError=ไฟล์ "%1" ไม่สามารถเปิดใช้ได้ ทำให้ไม่สามารถ ยกเลิกการลงโปรแกรมได้
UninstallUnsupportedVer=ถอนการติดตั้งแฟ้มบันทึก "%1" อยู่ในรูปแบบไม่ได้รับการยอมรับจากรุ่น uninstaller นี้ ไม่สามารถถอนการติดตั้ง
UninstallUnknownEntry=รายการที่ไม่รู้จัก (%1) ได้ถูกพบในแฟ้มบันทึกของการถอนการติดตั้ง
ConfirmUninstall=คุณต้องการยกเลิกการติดตั้ง  %1 และส่วนประกอบทั้งหมด?
UninstallOnlyOnWin64=การติดตั้งนี้สามารถถอนการติดตั้งบน 64 - bit Windows เท่านั้น.
OnlyAdminCanUninstall=คุณต้องเป็น ผู้ดูแลระบบ (Administrator) จึงจะสามารถยกเลิกการติดตั้งโปรแกรมนี้ได้
UninstallStatusLabel=กรุณารอสักครู่  %1 กำลังยกเลิกออกจากคอมพิวเตอร์
UninstalledAll=%1 ได้ยกเลิกการติดตั้ง จากคอมพิวเตอร์ของคุณแล้ว
UninstalledMost=%1 การยกเลิกการติดตั้งเสร็จสมบูรณ์%n%nส่วนประกอบบ้างอย่างไม่สามารถเอาออกได้ คุณจำเป็นต้องลบออกเอง
UninstalledAndNeedsRestart=การยกเลิก การติดตั้งโปรแกรม %1 คุณต้อง restart เครื่องคอมพิวเตอร์%n%nคุณต้องการ restart เครื่องเดี๋ยวนี้?
UninstallDataCorrupted="%1" ไฟล์ไม่ถูกต้อง ไม่สามารถยกเลิกการติดตั้งได้

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=ลบ ไฟล์ที่ใช้ร่วมกัน?
ConfirmDeleteSharedFile2=ไฟล์ที่จะลบ พบว่าอาจมีโปรแกรมอื่นๆ ใช้งานอยู่%nคุณต้องการ ลบไฟล์นี้หรือไม่?%n%nถ้ามีโปรแกรมที่ใช้ไฟล์นี้ จะทำให้โปรแกรมนั้นทำงานไม่ถูกต้อง ถ้าคุณไม่แน่ใจ คลิก ไม่ เพื่อข้ามการลบไฟล์นี้
SharedFileNameLabel=ชื่อไฟล์ :
SharedFileLocationLabel=ตำแหน่ง:
WizardUninstalling=สถานะ การยกเลิก
StatusUninstalling=กำลังยกเลิก %1...

; *** Shutdown block reasons
ShutdownBlockReasonInstallingApp=กำลังติดตั้ง %1.
ShutdownBlockReasonUninstallingApp=กำลังยกเลิก %1.

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 เวอร์ชัน %2
AdditionalIcons=Additional icons:
CreateDesktopIcon=สร้าง &ไอคอนบนเดสท็อป
CreateQuickLaunchIcon=สร้าง &ไอคอนแถบด่วน
ProgramOnTheWeb=%1 บนเว็บ
UninstallProgram=ยกเลิก %1
LaunchProgram=เปิด %1
AssocFileExtension=&Associate %1 with the %2 file extension
AssocingFileExtension=Associating %1 with the %2 file extension...
