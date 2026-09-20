using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings.WaveformToolbarItems;

public partial class ToolbarItemDisplay : ObservableObject
{
    [ObservableProperty] private bool _isVisible;
    [ObservableProperty] private int _fontSize;
    [ObservableProperty] private int _leftMargin;
    [ObservableProperty] private int _rightMargin;
    [ObservableProperty] private int _width;

    public string Name { get; }
    public SeWaveformToolbarItemType Type { get; }

    public ToolbarItemDisplay(SeWaveformToolbarItemType type, bool isVisible, int fontSize, int leftMargin, int rightMargin, int width)
    {
        Type = type;
        Name = GetDisplayName(type);
        _isVisible = isVisible;
        _fontSize = fontSize;
        _leftMargin = leftMargin;
        _rightMargin = rightMargin;
        _width = width;
    }

    private static string GetDisplayName(SeWaveformToolbarItemType type)
    {
        var w = Se.Language.Main.Waveform;
        return type switch
        {
            SeWaveformToolbarItemType.Play => Format(w.PlayPauseHint),
            SeWaveformToolbarItemType.PlayNext => Format(w.PlayNextHint),
            SeWaveformToolbarItemType.PlaySelection => Format(w.PlaySelectionHint),
            SeWaveformToolbarItemType.Repeat => Format(w.PlaySelectedRepeatHint),
            SeWaveformToolbarItemType.RemoveBlankLines => Format(w.RemoveBlankLines),
            SeWaveformToolbarItemType.New => Format(w.NewHint),
            SeWaveformToolbarItemType.SetStart => Format(w.SetStartHint),
            SeWaveformToolbarItemType.SetEnd => Format(w.SetEndHint),
            SeWaveformToolbarItemType.SetEndAndGoToNext => Se.Language.General.SetEndAndGoToNext,
            SeWaveformToolbarItemType.PlayFromJustBeforeText => Se.Language.General.PlayFromJustBeforeText,
            SeWaveformToolbarItemType.SetStartAndOffsetTheRest => Format(w.SetStartAndOffsetTheRestHint),
            SeWaveformToolbarItemType.MoveSelectedLines => w.MoveSelectedLines,
            SeWaveformToolbarItemType.MoveSelectedLinesAndFollowing => w.MoveSelectedLinesAndFollowing,
            SeWaveformToolbarItemType.MoveAllLines => w.MoveAllLines,
            SeWaveformToolbarItemType.VerticalZoom => Format(w.ZoomVerticalHint),
            SeWaveformToolbarItemType.HorizontalZoom => Format(w.ZoomHorizontalHint),
            SeWaveformToolbarItemType.VideoPositionSlider => Format(w.VideoPosition),
            SeWaveformToolbarItemType.VideoPositionText => Format(w.VideoPositionTextBox),
            SeWaveformToolbarItemType.PlaybackSpeed => Se.Language.General.PlaybackSpeed,
            SeWaveformToolbarItemType.AutoSelectOnPlay => Format(w.SelectCurrentLineWhilePlayingHint),
            SeWaveformToolbarItemType.Center => Format(w.CenterWaveformHint),
            SeWaveformToolbarItemType.VideoSeek => w.SeekVideo,
            SeWaveformToolbarItemType.AudioTrackPicker => Se.Language.Main.Menu.AudioTracks.Replace("_", string.Empty),
            SeWaveformToolbarItemType.TextPrevious => Format(w.TextPreviousHint),
            SeWaveformToolbarItemType.TextPlay => Format(w.TextPlayHint),
            SeWaveformToolbarItemType.TextPause => Format(w.TextPauseHint),
            SeWaveformToolbarItemType.TextNext => Format(w.TextNextHint),
            SeWaveformToolbarItemType.More => Se.Language.General.More,
            SeWaveformToolbarItemType.LineBreak1 => string.Format(w.ToolbarLineBreakX, 1),
            SeWaveformToolbarItemType.LineBreak2 => string.Format(w.ToolbarLineBreakX, 2),
            SeWaveformToolbarItemType.InitialText => w.InitialText,
            SeWaveformToolbarItemType.TimelineTrackGrouping => Se.Language.Waveform.TimelineGroupTracksBy,
            _ => type.ToString(),
        };
    }

    private static string Format(string hint) => string.Format(hint, string.Empty).TrimEnd();

    // The list box item's screen-reader name falls back to ToString(), which announced the
    // type name "...WaveformToolbarItems.ToolbarItemDisplay" for every row (#12087).
    public override string ToString() => Name;
}
