# Workflow Chuẩn - Subtitle Edit (Avalonia C#)

Tài liệu quy tắc và luồng xử lý bắt buộc cho dự án Subtitle Edit.

## 1. Quy tắc cốt lõi
- Luôn trả lời bằng tiếng Việt.
- Luôn sử dụng skill `teamwork-preview-protocol` và `prompt-optimizer` cho mọi tác vụ code.
- YAGNI & KISS: Sửa tối thiểu, đúng trọng tâm, tận dụng tối đa API/Helper có sẵn của Subtitle Edit (`UiUtil`, `FfmpegGenerator`, `MessageBox`, `IFileHelper`, `IWindowService`...).
- Tuân thủ MVVM (CommunityToolkit.Mvvm) và pattern UI code của Subtitle Edit Avalonia.
- Build dứt điểm đạt `0 error, 0 warning`.
- Sau khi code xong: tự động commit local và cung cấp mã hash commit cho người dùng.
- TUYỆT ĐỐI CHỈ COMMIT LOCAL, KHÔNG ĐƯỢC PUSH GITHUB trừ khi người dùng yêu cầu rõ ràng.
- Luôn luôn trả về trạng thái chuẩn cuối mỗi phản hồi:
  - commit local: *<mã hash>*
  - commit github: "không"
  - path exe: *<đường dẫn file exe sau build>*

## 2. Kiến trúc dự án
- Nền tảng: Avalonia UI, .NET 10.0 (`net10.0`).
- Solution: `SubtitleEdit.sln`.
- Các project chính:
  - `src/libse/LibSE.csproj`: Thư viện lõi xử lý định dạng phụ đề, codec, stream.
  - `src/libuilogic/LibUiLogic.csproj`: Logic dùng chung cho UI.
  - `src/ui/UI.csproj`: Giao diện người dùng Avalonia chính (`SubtitleEdit.exe`).
- Build command: `dotnet build -c Release src/ui/UI.csproj` (File exe khi build bắt buộc ở trong `\src\ui\bin\Release\net10.0\SubtitleEdit.exe`).

## 3. Tiêu chuẩn giao diện & Menu
- Xây dựng UI bằng C# code thông qua helper `UiUtil` (theo đúng chuẩn hiện có của Subtitle Edit: `UiUtil.MakeLabel`, `UiUtil.MakeTextBox`, `UiUtil.MakeButtonBrowse`, `UiUtil.MakeButtonBar`, `Grid`, `StackPanel`...).
- Menu Video:
  - Menu chính tại `src/ui/Features/Main/Layout/InitMenu.cs`.
  - Native Mac menu tại `src/ui/Features/Main/Layout/InitNativeMacMenu.cs`.
- Hỗ trợ đa ngôn ngữ thông qua `Se.Language.Video` và các file `src/ui/Assets/Languages/*.json`.
