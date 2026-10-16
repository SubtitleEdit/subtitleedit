using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Chrome = Nikse.SubtitleEdit.Features.Ocr.OcrDialogChrome;

namespace Nikse.SubtitleEdit.Features.Shared.GetAudioClips;

/// <summary>
/// Extracts the audio clips, in the editor download layout (see <see cref="Chrome"/>): title,
/// progress and status on the panel color, Cancel in the button bar.
/// </summary>
public class GetAudioClipsWindow : Window
{
    public GetAudioClipsWindow(GetAudioClipsViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.GetAudioClips;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Chrome.Apply(this);

        DataContext = vm;

        var titleText = Chrome.DownloadTitle(Se.Language.General.ExtractingAudioClips);
        var progressBar = Chrome.DownloadProgressBar(nameof(GetAudioClipsViewModel.Progress));
        var statusText = Chrome.DownloadStatus(nameof(GetAudioClipsViewModel.StatusText));
        var errorText = Chrome.DownloadError(nameof(GetAudioClipsViewModel.Error));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonBar = Chrome.ButtonBar(null, buttonCancel);

        Content = Chrome.Root(Chrome.DownloadBody(titleText, progressBar, statusText, errorText), buttonBar);

        UiUtil.FocusOnFirstActivation(this, buttonCancel); // hack to make OnKeyDown work

        // Loaded, not Activated: Activated fires again every time the window regains focus, which
        // started a second extraction loop from line one on top of the running one (#13777).
        Loaded += delegate { vm.StartAudioExtract(); };
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}
