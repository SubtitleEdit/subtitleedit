using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared;

/// <summary>
/// Shared controls for the single-file download dialogs (ffmpeg, libmpv, libVLC, yt-dlp,
/// FFmpeg libraries, llama.cpp), so a failed download looks and behaves the same in all of
/// them: the error text is shown in red, and a Retry button appears next to Cancel.
/// Both are bound to the view model's <c>Error</c> property - non-empty means "failed".
/// </summary>
public static class DownloadWindowUi
{
    public const string ErrorPropertyName = "Error";

    public static Button MakeButtonRetry(IRelayCommand command)
    {
        var button = UiUtil.MakeButton(Se.Language.General.Retry, command);
        button.Bind(Button.IsVisibleProperty, new Binding(ErrorPropertyName)
        {
            Converter = StringConverters.IsNotNullOrEmpty,
        });
        return button;
    }
}
