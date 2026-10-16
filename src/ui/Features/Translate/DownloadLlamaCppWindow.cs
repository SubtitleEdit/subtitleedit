using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Chrome = Nikse.SubtitleEdit.Features.Ocr.OcrDialogChrome;

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
        Chrome.Apply(this);
        DataContext = vm;

        var titleText = Chrome.DownloadTitle();
        titleText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.TitleText)));

        var progressBar = Chrome.DownloadProgressBar(nameof(vm.ProgressValue));
        var statusText = Chrome.DownloadStatus(nameof(vm.ProgressText));
        var errorText = Chrome.DownloadError(nameof(vm.Error));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonRetry = Chrome.MakePrimary(DownloadWindowUi.MakeButtonRetry(vm.RetryCommand));
        var buttonBar = Chrome.ButtonBar(null, buttonCancel, buttonRetry);

        Content = Chrome.Root(Chrome.DownloadBody(titleText, progressBar, statusText, errorText), buttonBar);

        UiUtil.FocusOnFirstActivation(this, buttonCancel);
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}
