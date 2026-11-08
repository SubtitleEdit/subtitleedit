using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.Download;

public class DownloadTesseractWindow : Window
{
    public DownloadTesseractWindow(DownloadTesseractViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.General.DownloadingX, "Tesseract");
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.EngineDialog);

        DataContext = vm;

        var titleText = EditorChrome.MakeDownloadTitle(string.Format(Se.Language.General.DownloadingX, "Tesseract"));
        var progressBar = EditorChrome.MakeDownloadProgressBar(nameof(DownloadFfmpegViewModel.Progress));
        var statusText = EditorChrome.MakeDownloadStatus(nameof(DownloadFfmpegViewModel.StatusText));
        var errorText = EditorChrome.MakeDownloadError(nameof(vm.Error));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonBar = EditorChrome.MakeButtonBar(null, buttonCancel);

        Content = EditorChrome.MakeRoot(EditorChrome.MakeDownloadBody(titleText, progressBar, statusText, errorText), buttonBar, stretch: false);

        Loaded += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
            vm.StartDownload();
        };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
