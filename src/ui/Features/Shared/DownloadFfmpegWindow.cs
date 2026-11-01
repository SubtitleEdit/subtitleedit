using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared;

/// <summary>
/// Downloads ffmpeg, in the editor download layout (see <see cref="Chrome"/>): title, progress,
/// status and error on the panel color, Retry (after a failure) and Cancel in the button bar.
/// </summary>
public class DownloadFfmpegWindow : Window
{
    public DownloadFfmpegWindow(DownloadFfmpegViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.General.DownloadingX, "ffmpeg");
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.EngineDialog);

        DataContext = vm;

        var titleText = EditorChrome.MakeDownloadTitle(string.Format(Se.Language.General.DownloadingX, "ffmpeg"));
        var progressBar = EditorChrome.MakeDownloadProgressBar(nameof(DownloadFfmpegViewModel.Progress));
        var statusText = EditorChrome.MakeDownloadStatus(nameof(DownloadFfmpegViewModel.StatusText));

        // Download/unpack failures (no data, file in use, ...) - otherwise the window just sits there.
        var errorText = EditorChrome.MakeDownloadError(DownloadWindowUi.ErrorPropertyName);

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonRetry = DownloadWindowUi.MakeButtonRetry(vm.RetryCommand);
        var buttonBar = EditorChrome.MakeButtonBar(null, buttonRetry, buttonCancel);

        Content = EditorChrome.MakeRoot(EditorChrome.MakeDownloadBody(titleText, progressBar, statusText, errorText), buttonBar, stretch: false);

        Loaded += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
            vm.StartDownload();
        };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
