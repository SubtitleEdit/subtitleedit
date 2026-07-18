namespace Nikse.SubtitleEdit.Logic.Config;

public enum SeWaveformToolbarItemType
{
    Play,
    PlayNext,
    PlaySelection,
    Repeat,
    RemoveBlankLines,
    New,
    SetStart,
    SetEnd,
    SetStartAndOffsetTheRest,
    VerticalZoom,
    HorizontalZoom,
    VideoPositionSlider,
    PlaybackSpeed,
    AutoSelectOnPlay,
    Center,
    VideoSeek,
    More,

    // New values must be appended here - the type is stored as a number in Settings.json,
    // so inserting above would shift existing users' saved toolbar items.

    // SE 4 "Translate tab" style text buttons: play a single line and stop at its end.
    TextPrevious,
    TextPlay,
    TextPause,
    TextNext
}