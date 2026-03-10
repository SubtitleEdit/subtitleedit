; *** Tin nhắn tiếng Việt cho Inno Setup phiên bản 6.5.0+ ***
; Người dịch: Memecoder (memecoder17@gmail.com)
; Vui lòng báo cáo tất cả các lỗi chính tả/ngữ pháp và các nhận xét.
; To download user-contributed translations of this file, go to:
;   https://jrsoftware.org/files/istrans/
;
; Note: When translating this text, do not add periods (.) to the end of
; messages that didn't have them already, because on those messages Inno
; Setup adds the periods automatically (appending a period would result in
; two periods being displayed).

[LangOptions]
; The following three entries are very important. Be sure to read and 
; understand the '[LangOptions] section' topic in the help file.
LanguageName=Tiếng Việt
LanguageID=$042A
LanguageCodePage=1258
; Nếu ngôn ngữ bạn đang dịch yêu cầu các phông chữ hoặc
; kích thước đặc biệt, hãy bỏ ghi chú bất kỳ mục nào sau đây và thay đổi chúng cho phù hợp.
;DialogFontName=
;DialogFontSize=9
;DialogFontBaseScaleWidth=7
;DialogFontBaseScaleHeight=15
;WelcomeFontName=Segoe UI
;WelcomeFontSize=14

[Messages]

; *** Tiêu đề ứng dụng
SetupAppTitle=Cài đặt
SetupWindowTitle=Cài đặt - %1
UninstallAppTitle=Gỡ cài đặt
UninstallAppFullTitle=Gỡ cài đặt - %1

; *** Các mục chung khác
InformationTitle=Thông tin
ConfirmTitle=Xác nhận
ErrorTitle=Lỗi

; *** Tin nhắn của SetupLdr
SetupLdrStartupMessage=Chương trình này sẽ cài đặt %1 trên máy tính của bạn, bạn có muốn tiếp tục không?
LdrCannotCreateTemp=Không thể tạo tệp tạm thời. Việc cài đặt đã bị hủy bỏ
LdrCannotExecTemp=Không thể thực thi tệp trong thư mục tạm thời. Việc cài đặt đã bị hủy bỏ
HelpTextNote=

; *** Tin nhắn lỗi khởi động
LastErrorMessage=%1.%n%nLỗi %2: %3
SetupFileMissing=Tệp %1 không có trong thư mục cài đặt. Vui lòng sửa lỗi này hoặc nhận một bản sao mới của chương trình.
SetupFileCorrupt=Các tệp cài đặt đã bị hỏng. Vui lòng tải xuống một bản sao mới của chương trình.
SetupFileCorruptOrWrongVer=Các tệp cài đặt đã bị hỏng hoặc không tương thích với phiên bản trình cài đặt này. Vui lòng sửa lỗi này hoặc nhận một bản sao mới của chương trình.
InvalidParameter=Dòng lệnh chứa tham số không hợp lệ:%n%n%1
SetupAlreadyRunning=Trình cài đặt đã đang chạy.
WindowsVersionNotSupported=Chương trình này không hỗ trợ phiên bản Windows được cài đặt trên máy tính này.
WindowsServicePackRequired=Chương trình này yêu cầu %1 Service Pack %2 hoặc phiên bản mới hơn.
NotOnThisPlatform=Chương trình này không hoạt động trên %1.
OnlyOnThisPlatform=Chương trình này chỉ có thể chạy trên %1.
OnlyOnTheseArchitectures=Chương trình này chỉ có thể được cài đặt trên các máy tính chạy Windows cho các kiến trúc bộ xử lý sau:%n%n%1
WinVersionTooLowError=Chương trình này yêu cầu phiên bản %1 %2 hoặc mới hơn.
WinVersionTooHighError=Chương trình này không thể được cài đặt trên phiên bản %1 %2 hoặc mới hơn.
AdminPrivilegesRequired=Để cài đặt chương trình này, bạn phải đăng nhập với tư cách quản trị viên.
PowerUserPrivilegesRequired=Để cài đặt chương trình này, bạn phải đăng nhập với tư cách quản trị viên hoặc là thành viên của nhóm "Power Users".
SetupAppRunningError=%1 đã được phát hiện đang chạy.%n%nVui lòng đóng tất cả các bản sao của chương trình và nhấp vào "OK" để tiếp tục, hoặc "Hủy" để thoát.
UninstallAppRunningError=%1 đã được phát hiện đang chạy.%n%nVui lòng đóng tất cả các bản sao của chương trình và nhấp vào "OK" để tiếp tục, hoặc "Hủy" để thoát.

; *** Các câu hỏi khởi động
PrivilegesRequiredOverrideTitle=Chọn chế độ cài đặt
PrivilegesRequiredOverrideInstruction=Chọn chế độ cài đặt
PrivilegesRequiredOverrideText1=%1 có thể được cài đặt cho tất cả người dùng (yêu cầu quyền quản trị viên), hoặc chỉ cho bạn.
PrivilegesRequiredOverrideText2=%1 chỉ có thể được cài đặt cho bạn, hoặc cho tất cả người dùng (yêu cầu quyền quản trị viên).
PrivilegesRequiredOverrideAllUsers=Cài đặt cho &tất cả người dùng
PrivilegesRequiredOverrideAllUsersRecommended=Cài đặt cho &tất cả người dùng (khuyên dùng)
PrivilegesRequiredOverrideCurrentUser=Chỉ cài đặt cho tôi
PrivilegesRequiredOverrideCurrentUserRecommended=Chỉ cài đặt cho &tôi (khuyên dùng)

; *** Các lỗi khác
ErrorCreatingDir=Trình cài đặt không thể tạo thư mục "%1"
ErrorTooManyFilesInDir=Trình cài đặt không thể tạo tệp trong thư mục "%1" vì có quá nhiều tệp trong đó

; *** Tin nhắn chung của trình cài đặt
ExitSetupTitle=Thoát trình cài đặt
ExitSetupMessage=Việc cài đặt chưa hoàn tất. Nếu bạn thoát bây giờ, chương trình sẽ không được cài đặt.%n%nBạn có thể mở lại trình cài đặt sau.%n%nThoát trình cài đặt?
AboutSetupMenuItem=&Giới thiệu trình cài đặt...
AboutSetupTitle=Giới thiệu trình cài đặt
AboutSetupMessage=%1 phiên bản %2%n%3%n%nTrang chủ của %1:%n%4
AboutSetupNote=
TranslatorNote=Bản dịch tiếng Việt bởi MemeCoder

; *** Các nút
ButtonBack=< &Quay lại
ButtonNext=&Tiếp theo >
ButtonInstall=&Cài đặt
ButtonOK=OK
ButtonCancel=Hủy
ButtonYes=&Có
ButtonYesToAll=Có cho &tất cả
ButtonNo=&Không
ButtonNoToAll=K&hông cho tất cả
ButtonFinish=&Hoàn thành
ButtonBrowse=&Duyệt...
ButtonWizardBrowse=&Duyệt...
ButtonNewFolder=&Tạo thư mục mới

; *** Tin nhắn hộp thoại "Chọn ngôn ngữ"
SelectLanguageTitle=Chọn ngôn ngữ cài đặt
SelectLanguageLabel=Chọn ngôn ngữ sẽ được sử dụng trong quá trình cài đặt.

; *** Văn bản trình hướng dẫn chung
ClickNext=Nhấp "Tiếp theo" để tiếp tục, hoặc "Hủy" để thoát trình cài đặt.
BeveledLabel=
BrowseDialogTitle=Duyệt thư mục
BrowseDialogLabel=Chọn một thư mục từ danh sách và nhấp "OK".
NewFolderName=Thư mục mới

; *** Trang trình hướng dẫn "Chào mừng"
WelcomeLabel1=Chào mừng bạn đến với trình cài đặt [name]
WelcomeLabel2=Chương trình này sẽ cài đặt [name/ver] trên máy tính của bạn.%n%nBạn nên đóng tất cả các chương trình khác trước khi tiếp tục.

; *** Trang trình hướng dẫn "Mật khẩu"
WizardPassword=Mật khẩu
PasswordLabel1=Trình cài đặt này được bảo vệ bằng mật khẩu.
PasswordLabel3=Vui lòng nhập mật khẩu và nhấp "Tiếp theo" để tiếp tục. Mật khẩu có phân biệt chữ hoa chữ thường.
PasswordEditLabel=&Mật khẩu:
IncorrectPassword=Bạn đã nhập sai mật khẩu. Vui lòng thử lại.

; *** Trang trình hướng dẫn "Thỏa thuận cấp phép"
WizardLicense=Thỏa thuận cấp phép
LicenseLabel=Vui lòng đọc thỏa thuận cấp phép.
LicenseLabel3=Vui lòng đọc thỏa thuận cấp phép. Bạn phải chấp nhận các điều khoản của thỏa thuận này trước khi tiếp tục cài đặt.
LicenseAccepted=Tôi &chấp nhận các điều khoản của thỏa thuận
LicenseNotAccepted=Tôi &không chấp nhận các điều khoản của thỏa thuận

; *** Các trang trình hướng dẫn "Thông tin"
WizardInfoBefore=Thông tin
InfoBeforeLabel=Vui lòng đọc thông tin quan trọng sau đây trước khi tiếp tục.
InfoBeforeClickLabel=Nếu bạn đã sẵn sàng tiếp tục cài đặt, hãy nhấp vào "Tiếp theo".
WizardInfoAfter=Thông tin
InfoAfterLabel=Vui lòng đọc thông tin quan trọng sau đây trước khi tiếp tục.
InfoAfterClickLabel=Nếu bạn đã sẵn sàng tiếp tục cài đặt, hãy nhấp vào "Tiếp theo".

; *** Trang trình hướng dẫn "Thông tin người dùng"
WizardUserInfo=Thông tin người dùng
UserInfoDesc=Vui lòng nhập thông tin của bạn.
UserInfoName=&Tên người dùng:
UserInfoOrg=&Tổ chức:
UserInfoSerial=&Số sê-ri:
UserInfoNameRequired=Bạn phải nhập tên.

; *** Trang trình hướng dẫn "Chọn vị trí đích"
WizardSelectDir=Chọn đường dẫn cài đặt
SelectDirDesc=Bạn muốn cài đặt [name] ở đâu?
SelectDirLabel3=Chương trình sẽ cài đặt [name] vào thư mục sau.
SelectDirBrowseLabel=Nhấp "Tiếp theo" để tiếp tục. Nếu bạn muốn chọn một thư mục khác, hãy nhấp vào "Duyệt".
DiskSpaceGBLabel=Cần ít nhất [gb] GB dung lượng đĩa trống.
DiskSpaceMBLabel=Cần ít nhất [mb] MB dung lượng đĩa trống.
CannotInstallToNetworkDrive=Không thể cài đặt vào ổ đĩa mạng.
CannotInstallToUNCPath=Không thể cài đặt qua đường dẫn mạng (UNC).
InvalidPath=Bạn phải chỉ định một đường dẫn đầy đủ có ký tự ổ đĩa, ví dụ:%n%nC:\APP%n%nhoặc ở định dạng UNC:%n%n\\server\share
InvalidDrive=Ổ đĩa hoặc đường dẫn mạng bạn đã chọn không tồn tại hoặc không thể truy cập được. Vui lòng chọn một đường dẫn khác.
DiskSpaceWarningTitle=Không đủ dung lượng đĩa
DiskSpaceWarning=Việc cài đặt yêu cầu ít nhất %1 KB dung lượng trống, nhưng trên ổ đĩa đã chọn chỉ có %2 KB.%n%nBạn có muốn tiếp tục không?
DirNameTooLong=Tên thư mục hoặc đường dẫn quá dài.
InvalidDirName=Tên thư mục được chỉ định không hợp lệ.
BadDirName32=Tên thư mục không được chứa các ký tự sau:%n%n%1
DirExistsTitle=Thư mục đã tồn tại
DirExists=Thư mục:%n%n%1%n%nđã tồn tại. Bạn có muốn cài đặt vào thư mục này không?
DirDoesntExistTitle=Thư mục không tồn tại
DirDoesntExist=Thư mục:%n%n%1%n%nkhông tồn tại. Bạn có muốn tạo nó không?

; *** Trang trình hướng dẫn "Chọn thành phần"
WizardSelectComponents=Chọn thành phần
SelectComponentsDesc=Bạn muốn cài đặt những thành phần nào?
SelectComponentsLabel2=Chọn các thành phần bạn muốn cài đặt; bỏ chọn các thành phần bạn không muốn cài đặt. Nhấp "Tiếp theo" để tiếp tục.
FullInstallation=Cài đặt đầy đủ
CompactInstallation=Cài đặt gọn nhẹ
CustomInstallation=Cài đặt tùy chỉnh
NoUninstallWarningTitle=Các thành phần đã tồn tại
NoUninstallWarning=Các thành phần sau đã được cài đặt trên máy tính của bạn:%n%n%1%n%nViệc bỏ chọn các thành phần này sẽ không gỡ bỏ chúng.%n%nBạn có muốn tiếp tục không?
ComponentSize1=%1 KB
ComponentSize2=%1 MB
ComponentsDiskSpaceGBLabel=Lựa chọn này yêu cầu ít nhất [gb] GB dung lượng đĩa.
ComponentsDiskSpaceMBLabel=Lựa chọn này yêu cầu ít nhất [mb] MB dung lượng đĩa.

; *** Trang trình hướng dẫn "Chọn tác vụ bổ sung"
WizardSelectTasks=Chọn tác vụ bổ sung
SelectTasksDesc=Bạn muốn thực hiện những tác vụ bổ sung nào?
SelectTasksLabel2=Chọn các tác vụ bổ sung mà trình cài đặt [name] nên thực hiện, sau đó nhấp vào "Tiếp theo".

; *** Trang trình hướng dẫn "Chọn thư mục trong menu Start"
WizardSelectProgramGroup=Chọn thư mục trong menu Start
SelectStartMenuFolderDesc=Bạn muốn tạo các lối tắt ở đâu?
SelectStartMenuFolderLabel3=Trình cài đặt sẽ tạo các lối tắt trong thư mục menu Start sau.
SelectStartMenuFolderBrowseLabel=Nhấp "Tiếp theo" để tiếp tục. Nếu bạn muốn chọn một thư mục khác, hãy nhấp vào "Duyệt".
MustEnterGroupName=Bạn phải nhập tên thư mục.
GroupNameTooLong=Tên thư mục hoặc đường dẫn quá dài.
InvalidGroupName=Tên thư mục được chỉ định không hợp lệ.
BadGroupName=Tên thư mục không được chứa các ký tự sau:%n%n%1
NoProgramGroupCheck2=&Không tạo thư mục trong menu Start

; *** Trang trình hướng dẫn "Sẵn sàng cài đặt"
WizardReady=Sẵn sàng cài đặt
ReadyLabel1=Chương trình đã sẵn sàng để bắt đầu cài đặt [name] trên máy tính của bạn.
ReadyLabel2a=Nhấp "Cài đặt" để tiếp tục cài đặt, hoặc "Quay lại" nếu bạn muốn xem lại hoặc thay đổi cài đặt.
ReadyLabel2b=Nhấp "Cài đặt" để tiếp tục.
ReadyMemoUserInfo=Thông tin người dùng:
ReadyMemoDir=Đường dẫn cài đặt:
ReadyMemoType=Loại cài đặt:
ReadyMemoComponents=Các thành phần đã chọn:
ReadyMemoGroup=Thư mục trong menu Start:
ReadyMemoTasks=Các tác vụ bổ sung:

; *** Trang trình hướng dẫn TDownloadWizardPage và DownloadTemporaryFile
DownloadingLabel2=Đang tải tệp...
ButtonStopDownload=&Dừng tải xuống
StopDownload=Bạn có thực sự muốn dừng tải xuống không?
ErrorDownloadAborted=Tải xuống đã bị hủy
ErrorDownloadFailed=Lỗi tải xuống: %1 %2
ErrorDownloadSizeFailed=Lỗi khi lấy kích thước: %1 %2
ErrorProgress=Lỗi thực thi: %1 trên %2
ErrorFileSize=Kích thước tệp không hợp lệ: mong đợi %1, nhận được %2

; *** Trang trình hướng dẫn TExtractionWizardPage và ExtractArchive
ExtractingLabel=Đang giải nén tệp...
ButtonStopExtraction=&Dừng giải nén
StopExtraction=Bạn có thực sự muốn dừng giải nén không?
ErrorExtractionAborted=Giải nén đã bị hủy
ErrorExtractionFailed=Lỗi giải nén: %1

; *** Chi tiết lỗi giải nén tệp lưu trữ
ArchiveIncorrectPassword=Mật khẩu không đúng
ArchiveIsCorrupted=Tệp lưu trữ bị hỏng
ArchiveUnsupportedFormat=Định dạng tệp lưu trữ không được hỗ trợ

; *** Trang trình hướng dẫn "Chuẩn bị cài đặt"
WizardPreparing=Chuẩn bị cài đặt
PreparingDesc=Trình cài đặt đang chuẩn bị để cài đặt [name] trên máy tính của bạn.
PreviousInstallNotCompleted=Việc cài đặt hoặc gỡ cài đặt của chương trình trước đó chưa được hoàn tất. Bạn cần khởi động lại máy tính để hoàn tất việc cài đặt trước đó.%n%nSau khi khởi động lại, hãy mở lại trình cài đặt để hoàn tất việc cài đặt [name].
CannotContinue=Không thể tiếp tục cài đặt. Vui lòng nhấp vào "Hủy" để thoát.
ApplicationsFound=Các ứng dụng sau đang sử dụng các tệp cần được cập nhật bởi trình cài đặt. Bạn nên cho phép trình cài đặt tự động đóng các ứng dụng này.
ApplicationsFound2=Các ứng dụng sau đang sử dụng các tệp cần được cập nhật bởi trình cài đặt. Bạn nên cho phép trình cài đặt tự động đóng các ứng dụng này. Sau khi cài đặt hoàn tất, trình cài đặt sẽ cố gắng khởi động lại chúng.
CloseApplications=&Tự động đóng các ứng dụng
DontCloseApplications=&Không đóng các ứng dụng
ErrorCloseApplications=Trình cài đặt không thể tự động đóng tất cả các ứng dụng. Bạn nên đóng tất cả các ứng dụng đang sử dụng các tệp cần được cập nhật bởi trình cài đặt trước khi tiếp tục.
PrepareToInstallNeedsRestart=Trình cài đặt cần khởi động lại máy tính của bạn. Sau khi khởi động lại, hãy chạy lại trình cài đặt để hoàn tất việc cài đặt [name]%n%nBạn có muốn khởi động lại ngay bây giờ không?

; *** Trang trình hướng dẫn "Đang cài đặt"
WizardInstalling=Đang cài đặt
InstallingLabel=Vui lòng đợi trong khi [name] được cài đặt trên máy tính của bạn.

; *** Trang trình hướng dẫn "Hoàn tất cài đặt"
FinishedHeadingLabel=Hoàn tất cài đặt [name]
FinishedLabelNoIcons=Việc cài đặt [name] trên máy tính của bạn đã hoàn tất.
FinishedLabel=Việc cài đặt [name] trên máy tính của bạn đã hoàn tất. Các chương trình đã cài đặt có thể được mở bằng các lối tắt đã tạo.
ClickFinish=Nhấp "Hoàn thành" để thoát trình cài đặt.
FinishedRestartLabel=Để hoàn tất việc cài đặt [name], máy tính của bạn cần được khởi động lại. Khởi động lại máy tính ngay bây giờ?
FinishedRestartMessage=Để hoàn tất việc cài đặt [name], máy tính của bạn cần được khởi động lại.%n%nKhởi động lại máy tính ngay bây giờ?
ShowReadmeCheck=Có, tôi muốn xem tệp README
YesRadio=&Có, khởi động lại máy tính ngay bây giờ
NoRadio=&Không, tôi sẽ khởi động lại máy tính sau
RunEntryExec=Chạy %1
RunEntryShellExec=Xem %1

; *** Nội dung "Trình cài đặt cần đĩa tiếp theo"
ChangeDiskTitle=Cần lắp đĩa tiếp theo
SelectDiskLabel2=Vui lòng lắp đĩa %1 và nhấp "OK".%n%nNếu các tệp cần thiết có thể ở trong một thư mục khác với thư mục được chỉ định bên dưới, hãy nhập đường dẫn chính xác hoặc nhấp vào "Duyệt".
PathLabel=&Đường dẫn:
FileNotInDir2=Tệp "%1" không được tìm thấy trong "%2". Vui lòng lắp đúng đĩa hoặc chỉ định một thư mục khác.
SelectDirectoryLabel=Vui lòng chỉ định đường dẫn đến đĩa tiếp theo.

; *** Tin nhắn giai đoạn cài đặt
SetupAborted=Việc cài đặt chưa hoàn tất.%n%nVui lòng khắc phục sự cố và mở lại trình cài đặt.
AbortRetryIgnoreSelectAction=Chọn một hành động
AbortRetryIgnoreRetry=&Thử lại
AbortRetryIgnoreIgnore=&Bỏ qua lỗi và tiếp tục
AbortRetryIgnoreCancel=Hủy cài đặt
RetryCancelSelectAction=Chọn một hành động
RetryCancelRetry=&Thử lại
RetryCancelCancel=Hủy

; *** Tin nhắn trạng thái cài đặt
StatusClosingApplications=Đang đóng các ứng dụng...
StatusCreateDirs=Đang tạo các thư mục...
StatusExtractFiles=Đang giải nén tệp...
StatusDownloadFiles=Đang tải xuống tệp...
StatusCreateIcons=Đang tạo các lối tắt...
StatusCreateIniEntries=Đang tạo các mục INI...
StatusCreateRegistryEntries=Đang tạo các mục registry...
StatusRegisterFiles=Đang đăng ký các tệp...
StatusSavingUninstall=Đang lưu thông tin để gỡ cài đặt...
StatusRunProgram=Đang hoàn tất cài đặt...
StatusRestartingApplications=Đang khởi động lại các ứng dụng...
StatusRollback=Đang hoàn tác các thay đổi...

; *** Các lỗi khác
ErrorInternal2=Lỗi nội bộ: %1
ErrorFunctionFailedNoCode=%1 không thành công
ErrorFunctionFailed=%1 không thành công; mã %2
ErrorFunctionFailedWithMessage=%1 không thành công; mã %2.%n%3
ErrorExecutingProgram=Không thể thực thi tệp:%n%1

; *** Lỗi registry
ErrorRegOpenKey=Lỗi khi mở khóa registry:%n%1\%2
ErrorRegCreateKey=Lỗi khi tạo khóa registry:%n%1\%2
ErrorRegWriteKey=Lỗi khi ghi vào khóa registry:%n%1\%2

; *** Lỗi INI
ErrorIniEntry=Lỗi khi tạo mục trong tệp INI "%1".

; *** Lỗi sao chép tệp
FileAbortRetryIgnoreSkipNotRecommended=&Bỏ qua tệp này (không khuyến nghị)
FileAbortRetryIgnoreIgnoreNotRecommended=&Bỏ qua lỗi và tiếp tục (không khuyến nghị)
SourceIsCorrupted=Tệp nguồn bị hỏng
SourceDoesntExist=Tệp nguồn "%1" không tồn tại
SourceVerificationFailed=Xác minh tệp nguồn không thành công: %1
VerificationSignatureDoesntExist=Tệp chữ ký "%1" không tồn tại
VerificationSignatureInvalid=Tệp chữ ký "%1" không hợp lệ
VerificationKeyNotFound=Tệp chữ ký "%1" sử dụng một khóa không xác định
VerificationFileNameIncorrect=Tên tệp không chính xác
VerificationFileTagIncorrect=Thẻ tệp không chính xác
VerificationFileSizeIncorrect=Kích thước tệp không chính xác
VerificationFileHashIncorrect=Giá trị băm của tệp không chính xác
ExistingFileReadOnly2=Không thể thay thế tệp hiện có vì nó được đánh dấu chỉ đọc.
ExistingFileReadOnlyRetry=&Xóa thuộc tính "chỉ đọc" và thử lại
ExistingFileReadOnlyKeepExisting=&Giữ lại tệp hiện có
ErrorReadingExistingDest=Đã xảy ra lỗi khi cố gắng đọc tệp hiện có:
FileExistsSelectAction=Chọn một hành động
FileExists2=Tệp đã tồn tại.
FileExistsOverwriteExisting=&Ghi đè lên tệp hiện có
FileExistsKeepExisting=&Giữ lại tệp hiện có
FileExistsOverwriteOrKeepAll=&Lặp lại hành động cho tất cả các xung đột sau này
ExistingFileNewerSelectAction=Chọn một hành động
ExistingFileNewer2=Tệp hiện có mới hơn tệp đang được cài đặt.
ExistingFileNewerOverwriteExisting=&Ghi đè lên tệp hiện có
ExistingFileNewerKeepExisting=&Giữ lại tệp hiện có (khuyên dùng)
ExistingFileNewerOverwriteOrKeepAll=&Lặp lại hành động cho tất cả các xung đột sau này
ErrorChangingAttr=Đã xảy ra lỗi khi cố gắng thay đổi thuộc tính của tệp hiện có:
ErrorCreatingTemp=Đã xảy ra lỗi khi cố gắng tạo tệp trong thư mục cài đặt:
ErrorReadingSource=Đã xảy ra lỗi khi cố gắng đọc tệp nguồn:
ErrorCopying=Đã xảy ra lỗi khi cố gắng sao chép tệp:
ErrorDownloading=Đã xảy ra lỗi khi cố gắng tải xuống tệp:
ErrorExtracting=Đã xảy ra lỗi khi cố gắng giải nén tệp lưu trữ:
ErrorReplacingExistingFile=Đã xảy ra lỗi khi cố gắng thay thế tệp hiện có:
ErrorRestartReplace=Lỗi RestartReplace:
ErrorRenamingTemp=Đã xảy ra lỗi khi cố gắng đổi tên tệp trong thư mục cài đặt:
ErrorRegisterServer=Không thể đăng ký DLL/OCX: %1
ErrorRegSvr32Failed=Lỗi khi thực thi RegSvr32, mã trả về %1
ErrorRegisterTypeLib=Không thể đăng ký thư viện kiểu: %1

; *** Đánh dấu tên hiển thị gỡ cài đặt
UninstallDisplayNameMark=%1 (%2)
UninstallDisplayNameMarks=%1 (%2, %3)
UninstallDisplayNameMark32Bit=32-bit
UninstallDisplayNameMark64Bit=64-bit
UninstallDisplayNameMarkAllUsers=Tất cả người dùng
UninstallDisplayNameMarkCurrentUser=Người dùng hiện tại

; *** Lỗi sau khi cài đặt
ErrorOpeningReadme=Đã xảy ra lỗi khi cố gắng mở tệp README.
ErrorRestartingComputer=Trình cài đặt không thể khởi động lại máy tính. Vui lòng tự thực hiện.

; *** Tin nhắn của trình gỡ cài đặt
UninstallNotFound=Tệp "%1" không tồn tại, không thể gỡ cài đặt.
UninstallOpenError=Không thể mở tệp "%1". Không thể gỡ cài đặt
UninstallUnsupportedVer=Tệp nhật ký gỡ cài đặt "%1" không được nhận dạng bởi phiên bản này của trình gỡ cài đặt. Không thể gỡ cài đặt
UninstallUnknownEntry=Mục không xác định (%1) trong tệp nhật ký gỡ cài đặt
ConfirmUninstall=Bạn có chắc chắn muốn gỡ cài đặt %1 và tất cả các thành phần của nó không?
UninstallOnlyOnWin64=Chương trình này chỉ có thể được gỡ cài đặt trong môi trường Windows 64-bit.
OnlyAdminCanUninstall=Chương trình này chỉ có thể được gỡ cài đặt bởi người dùng có quyền quản trị viên.
UninstallStatusLabel=Vui lòng đợi trong khi %1 được gỡ bỏ khỏi máy tính của bạn.
UninstalledAll=%1 đã được gỡ bỏ thành công khỏi máy tính của bạn.
UninstalledMost=Việc gỡ cài đặt %1 đã hoàn tất.%n%nMột số mục không thể bị xóa. Bạn có thể xóa chúng thủ công.
UninstalledAndNeedsRestart=Để hoàn tất việc gỡ cài đặt %1, máy tính của bạn cần được khởi động lại.%n%nKhởi động lại máy tính ngay bây giờ?
UninstallDataCorrupted=Tệp "%1" bị hỏng. Không thể gỡ cài đặt

; *** Tin nhắn giai đoạn gỡ cài đặt
ConfirmDeleteSharedFileTitle=Xóa các tệp được chia sẻ?
ConfirmDeleteSharedFile2=Hệ thống cho thấy tệp được chia sẻ sau không còn được sử dụng bởi các chương trình khác. Bạn có muốn xóa tệp được chia sẻ này không?%n%nNếu các chương trình khác vẫn đang sử dụng tệp này và nó bị xóa, các chương trình đó có thể hoạt động không đúng. Nếu bạn không chắc chắn, hãy chọn "Không". Tệp còn lại sẽ không gây hại cho hệ thống của bạn.
SharedFileNameLabel=Tên tệp:
SharedFileLocationLabel=Vị trí:
WizardUninstalling=Trạng thái gỡ cài đặt
StatusUninstalling=Đang gỡ cài đặt %1...

; *** Lý do chặn tắt máy
ShutdownBlockReasonInstallingApp=Đang cài đặt %1.
ShutdownBlockReasonUninstallingApp=Đang gỡ cài đặt %1.

; Các tin nhắn tùy chỉnh bên dưới không được sử dụng bởi chính trình cài đặt, nhưng nếu bạn
; sử dụng chúng trong các kịch bản của mình, bạn sẽ muốn dịch chúng.

[CustomMessages]

NameAndVersion=%1, phiên bản %2
AdditionalIcons=Các lối tắt bổ sung:
CreateDesktopIcon=Tạo lối tắt trên &Màn hình chính
CreateQuickLaunchIcon=Tạo lối tắt trên thanh &Khởi động nhanh
ProgramOnTheWeb=Trang web của %1
UninstallProgram=Gỡ cài đặt %1
LaunchProgram=Chạy %1
AssocFileExtension=&Liên kết %1 với phần mở rộng tệp %2
AssocingFileExtension=Đang liên kết %1 với phần mở rộng tệp %2...
AutoStartProgramGroupDescription=Tự động khởi động:
AutoStartProgram=Tự động khởi động %1
AddonHostProgramNotFound=%1 không được tìm thấy trong thư mục bạn đã chỉ định.%n%nBạn có muốn tiếp tục không?
