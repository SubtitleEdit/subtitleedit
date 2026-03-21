using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings.WaveformToolbarItems;

public partial class ToolbarItemDisplay : ObservableObject
{
    [ObservableProperty] private bool _isVisible;
    [ObservableProperty] private int _fontSize;
    [ObservableProperty] private int _leftMargin;
    [ObservableProperty] private int _rightMargin;

    public string Name { get; }
    public SeWaveformToolbarItemType Type { get; }

    public ToolbarItemDisplay(SeWaveformToolbarItemType type, bool isVisible, int fontSize, int leftMargin, int rightMargin)
    {
        Type = type;
        Name = GetDisplayName(type);
        _isVisible = isVisible;
        _fontSize = fontSize;
        _leftMargin = leftMargin;
        _rightMargin = rightMargin;
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
            SeWaveformToolbarItemType.SetStartAndOffsetTheRest => Format(w.SetStartAndOffsetTheRestHint),
            SeWaveformToolbarItemType.VerticalZoom => Format(w.ZoomVerticalHint),
            SeWaveformToolbarItemType.HorizontalZoom => Format(w.ZoomHorizontalHint),
            SeWaveformToolbarItemType.VideoPositionSlider => Format(w.VideoPosition),
            SeWaveformToolbarItemType.PlaybackSpeed => "Playback speed",
            SeWaveformToolbarItemType.AutoSelectOnPlay => Format(w.SelectCurrentLineWhilePlayingHint),
            SeWaveformToolbarItemType.Center => Format(w.CenterWaveformHint),
            SeWaveformToolbarItemType.More => "More",
            _ => type.ToString(),
        };
    }

    private static string Format(string hint) => string.Format(hint, string.Empty).TrimEnd();
}
