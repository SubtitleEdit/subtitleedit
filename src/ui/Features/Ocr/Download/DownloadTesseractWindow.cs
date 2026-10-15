using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Chrome = Nikse.SubtitleEdit.Features.Ocr.OcrDialogChrome;

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
        Chrome.Apply(this);

        DataContext = vm;

        var titleText = Chrome.DownloadTitle(string.Format(Se.Language.General.DownloadingX, "Tesseract"));
        var progressBar = Chrome.DownloadProgressBar(nameof(DownloadFfmpegViewModel.Progress));
        var statusText = Chrome.DownloadStatus(nameof(DownloadFfmpegViewModel.StatusText));
        var errorText = Chrome.DownloadError(nameof(vm.Error));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonBar = Chrome.ButtonBar(null, buttonCancel);

        Content = Chrome.Root(Chrome.DownloadBody(titleText, progressBar, statusText, errorText), buttonBar);

        Loaded += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
            vm.StartDownload();
        };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
