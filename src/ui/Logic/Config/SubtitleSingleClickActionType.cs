namespace Nikse.SubtitleEdit.Logic.Config;

public enum SubtitleSingleClickActionType
{
    None,
    GoToWaveformOnlyNoVideoPosition,
    GoToSubtitleAndPause,
    GoToSubtitleAndPlay,
    GoToSubtitleAndSetVideoPosition,
    GoToSubtitleAndPauseAndFocusTextBox,
    GoToSubtitleAndPlayAndFocusTextBox,

    /// <summary>
    /// Focus the text box with the caret at the end, without touching the video (#17218).
    /// </summary>
    FocusTextBox,
}
