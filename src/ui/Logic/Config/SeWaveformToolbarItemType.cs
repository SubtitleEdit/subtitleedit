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

    // SE 4 "Adjust tab" buttons that were shortcut-only in SE 5 (#15034).
    SetEndAndGoToNext = 26,
    PlayFromJustBeforeText = 27,

    // "Move lines X ms" button groups (#14789): back/forward buttons for the global step and the
    // two custom-milliseconds slots of one scope, the same commands as the shortcuts.
    MoveSelectedLines = 23,
    MoveSelectedLinesAndFollowing = 24,
    MoveAllLines = 25,

    VerticalZoom = 9,
    HorizontalZoom = 10,
    VideoPositionSlider = 11,

    // SE 4's editable video position box (#12266): the slider's exact-time counterpart - type,
    // step or copy the current position as a time code.
    VideoPositionText = 22,

    // Picks which audio track the waveform is extracted from; only rendered when the
    // open video has more than one audio track.
    AudioTrackPicker = 21,

    PlaybackSpeed = 12,
    AutoSelectOnPlay = 13,
    Center = 14,
    VideoSeek = 15,
    More = 16,

    // Not buttons: each forces the toolbar's WrapPanel onto a new row at its position, so the
    // user decides where a toolbar wider than the waveform pane wraps.
    LineBreak1 = 28,
    LineBreak2 = 29,

    // Read-only box holding the selected line's text as it was when the line was selected, so a
    // machine translation stays readable (and copyable) while it is typed over (#14541, #15035).
    InitialText = 30,

    // Picks how the editor-style layout (layout 14) splits the subtitles over rows - one row, or
    // a row per layer, actor or style. Only rendered in that layout, like the audio-track picker
    // only renders for a multi-track video.
    TimelineTrackGrouping = 31
}
