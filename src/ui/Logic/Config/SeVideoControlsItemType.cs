namespace Nikse.SubtitleEdit.Logic.Config;

// The type is stored as a number in Settings.json, so every member has an explicit value
// that must never change or be reused. New members get the next free number.
public enum SeVideoControlsItemType
{
    // The controls row under the video, laid out left to right in list order. The position
    // slider takes the remaining width.
    Play = 0,
    Stop = 1,
    FullScreen = 2,
    PositionSlider = 3,
    Volume = 4,

    // Captions drawn at the bottom of the controls row, in list order: the first is placed to
    // the left of the second (#15286). The position/duration text is centered under the position
    // slider when it comes first, and right-aligned when it follows the file name.
    PositionText = 5,
    VideoFileName = 6,

    // Small label in the top right corner (e.g. "libmpv 2.5 sw").
    PlayerName = 7,
}
