using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Chrome = Nikse.SubtitleEdit.Features.Ocr.OcrDialogChrome;

namespace Nikse.SubtitleEdit.Features.Shared;

/// <summary>
/// Downloads libmpv, in the editor download layout (see <see cref="Chrome"/>): title, progress,
/// status and error on the panel color, Retry (after a failure) and Cancel in the button bar.
/// </summary>
public class DownloadLibMpvWindow : Window
{
    public DownloadLibMpvWindow(DownloadLibMpvViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.General.DownloadingX, "libmpv");
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Chrome.Apply(this);

        DataContext = vm;

        var titleText = Chrome.DownloadTitle(string.Format(Se.Language.General.DownloadingX, "libmpv"));
        var progressBar = Chrome.DownloadProgressBar(nameof(DownloadLibMpvViewModel.Progress));
        var statusText = Chrome.DownloadStatus(nameof(DownloadLibMpvViewModel.StatusText));

        // Download/unpack failures (no data, file in use, ...) - otherwise the window just sits there.
        var errorText = Chrome.DownloadError(DownloadWindowUi.ErrorPropertyName);

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonRetry = DownloadWindowUi.MakeButtonRetry(vm.RetryCommand);
        var buttonBar = Chrome.ButtonBar(null, buttonRetry, buttonCancel);

        Content = Chrome.Root(Chrome.DownloadBody(titleText, progressBar, statusText, errorText), buttonBar);

        Loaded += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
            vm.StartDownload();
        };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
