using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.Download;

public class DownloadPaddleOcrWindow : Window
{
    public DownloadPaddleOcrWindow(DownloadPaddleOcrViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.General.DownloadingX, "Paddle OCR");
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.EngineDialog);

        DataContext = vm;

        var titleText = EditorChrome.MakeDownloadTitle().WithBindText(vm, nameof(vm.StatusText));

        var progressBar = EditorChrome.MakeDownloadProgressBar(nameof(vm.ProgressValue));
        progressBar.Bind(ProgressBar.OpacityProperty, new Binding(nameof(vm.ProgressOpacity)));

        var statusText = EditorChrome.MakeDownloadStatus(nameof(vm.ProgressText));
        var errorText = EditorChrome.MakeDownloadError(nameof(vm.Error));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonBar = EditorChrome.MakeFlushButtonBar(null, buttonCancel);

        Content = EditorChrome.MakeRoot(EditorChrome.MakeDownloadBody(titleText, progressBar, statusText, errorText), buttonBar, stretch: false);

        Loaded += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
            vm.StartDownload();
        };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
