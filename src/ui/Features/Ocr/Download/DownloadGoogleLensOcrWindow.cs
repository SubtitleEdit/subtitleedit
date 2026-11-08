using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.Download;

public class DownloadGoogleLensOcrWindow : Window
{
    public DownloadGoogleLensOcrWindow(DownloadGoogleLensOcrViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.General.DownloadingX, "Google Lens OCR");
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.EngineDialog);

        DataContext = vm;

        var titleText = EditorChrome.MakeDownloadTitle().WithBindText(vm, nameof(vm.StatusText));
        var progressBar = EditorChrome.MakeDownloadProgressBar(nameof(vm.ProgressValue));
        var statusText = EditorChrome.MakeDownloadStatus(nameof(vm.ProgressText));
        var errorText = EditorChrome.MakeDownloadError(nameof(vm.Error));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonBar = EditorChrome.MakeButtonBar(null, buttonCancel);

        Content = EditorChrome.MakeRoot(EditorChrome.MakeDownloadBody(titleText, progressBar, statusText, errorText), buttonBar, stretch: false);

        Loaded += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
            vm.StartDownload();
        };
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}
