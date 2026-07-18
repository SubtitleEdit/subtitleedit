namespace Nikse.SubtitleEdit.Logic.Config;

// The type is stored as a number in Settings.json, so every member has an explicit value
// that must never change or be reused. New members get the next free number but can be
// declared wherever they fit logically.
public enum SeWaveformToolbarItemType
{
    Play = 0,
    PlayNext = 1,
    PlaySelection = 2,
    Repeat = 3,

    // SE 4 "Translate tab" style text buttons: play a single line and stop at its end.
    TextPrevious = 17,
    TextPlay = 18,
    TextPause = 19,
    TextNext = 20,

    RemoveBlankLines = 4,
    New = 5,
    SetStart = 6,
    SetEnd = 7,
    SetStartAndOffsetTheRest = 8,
    VerticalZoom = 9,
    HorizontalZoom = 10,
    VideoPositionSlider = 11,
    PlaybackSpeed = 12,
    AutoSelectOnPlay = 13,
    Center = 14,
    VideoSeek = 15,
    More = 16
}
