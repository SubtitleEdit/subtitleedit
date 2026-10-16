using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>
/// The llama.cpp download in the editor look (see <see cref="EditorChrome"/>), like the OCR engine
/// downloads: title, progress, status and error on the panel, Retry and Cancel in the button bar.
/// </summary>
public class DownloadLlamaCppWindow : Window
{
    public DownloadLlamaCppWindow(DownloadLlamaCppViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.General.DownloadingX, "llama.cpp");
        Width = 500;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.EngineDialog);
        DataContext = vm;

        var titleText = EditorChrome.MakeDownloadTitle();
        titleText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.TitleText)));

        var progressBar = EditorChrome.MakeDownloadProgressBar(nameof(vm.ProgressValue));
        var statusText = EditorChrome.MakeDownloadStatus(nameof(vm.ProgressText));
        var errorText = EditorChrome.MakeDownloadError(nameof(vm.Error));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonRetry = EditorChrome.AsPrimary(DownloadWindowUi.MakeButtonRetry(vm.RetryCommand));
        var buttonBar = EditorChrome.MakeFlushButtonBar(null, buttonCancel, buttonRetry);

        Content = EditorChrome.MakeRoot(EditorChrome.MakeDownloadBody(titleText, progressBar, statusText, errorText), buttonBar, stretch: false);

        UiUtil.FocusOnFirstActivation(this, buttonCancel);
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}
