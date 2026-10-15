using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Chrome = Nikse.SubtitleEdit.Features.Ocr.OcrDialogChrome;

namespace Nikse.SubtitleEdit.Features.Ocr.Download;

public class DownloadCrispEmbedWindow : Window
{
    public DownloadCrispEmbedWindow(DownloadCrispEmbedViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.General.DownloadingX, "CrispEmbed");
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Chrome.Apply(this);

        DataContext = vm;

        var titleText = Chrome.DownloadTitle().WithBindText(vm, nameof(vm.TitleText));
        var progressBar = Chrome.DownloadProgressBar(nameof(vm.ProgressValue));
        var statusText = Chrome.DownloadStatus(nameof(vm.ProgressText));
        var errorText = Chrome.DownloadError(nameof(vm.Error));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonBar = Chrome.ButtonBar(null, buttonCancel);

        Content = Chrome.Root(Chrome.DownloadBody(titleText, progressBar, statusText, errorText), buttonBar);

        Loaded += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
        };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
