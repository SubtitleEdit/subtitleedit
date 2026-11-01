using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.EngineDialog);

        DataContext = vm;

        var titleText = EditorChrome.MakeDownloadTitle(Se.Language.General.ExtractingAudioClips);
        var progressBar = EditorChrome.MakeDownloadProgressBar(nameof(GetAudioClipsViewModel.Progress));
        var statusText = EditorChrome.MakeDownloadStatus(nameof(GetAudioClipsViewModel.StatusText));
        var errorText = EditorChrome.MakeDownloadError(nameof(GetAudioClipsViewModel.Error));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonBar = EditorChrome.MakeButtonBar(null, buttonCancel);

        Content = EditorChrome.MakeRoot(EditorChrome.MakeDownloadBody(titleText, progressBar, statusText, errorText), buttonBar, stretch: false);

        UiUtil.FocusOnFirstActivation(this, buttonCancel); // hack to make OnKeyDown work

        // Loaded, not Activated: Activated fires again every time the window regains focus, which
        // started a second extraction loop from line one on top of the running one (#13777).
        Loaded += delegate { vm.StartAudioExtract(); };
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}
