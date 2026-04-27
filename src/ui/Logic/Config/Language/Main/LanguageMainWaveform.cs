namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageMainWaveform
{
    public string PlayPauseHint { get; set; }
    public string PlayNextHint { get; set; }
    public string PlaySelectionHint { get; set; }
    public string SetStartAndOffsetTheRestHint { get; set; }
    public string SetStartHint { get; set; }
    public string SetEndHint { get; set; }
    public string NewHint { get; set; }
    public string CenterWaveformHint { get; set; }
    public string ZoomHorizontalHint { get; set; }
    public string ZoomVerticalHint { get; set; }
    public string SelectCurrentLineWhilePlayingHint { get; set; }
    public string VideoPosition { get; set; }
    public string HideWaveformToolbar { get; set; }
    public string ResetZoomAndSpeed { get; set; }
    public string RemoveBlankLines { get; set; }
    public string PlaySelectedRepeatHint { get; set; }

    public LanguageMainWaveform()
    {
        PlayPauseHint = "Play / Pause {0}";
        PlayNextHint = "Play next {0}";
        PlaySelectionHint = "Play selection {0}";
        SetStartAndOffsetTheRestHint = "Set start of current subtitle and offset the rest {0}";
        SetStartHint = "Set start of current subtitle {0}";
        SetEndHint = "Set end of current subtitle {0}";
        NewHint = "Insert new subtitle at video position {0}";
        CenterWaveformHint = "Center waveform on current video position while playing {0}";
        ZoomHorizontalHint = "Zoom horizontal {0}";
        ZoomVerticalHint = "Zoom vertical {0}";
        SelectCurrentLineWhilePlayingHint = "Select current subtitle while playing {0}";
        VideoPosition = "Video position {0}";
        HideWaveformToolbar = "Hide toolbar {0}";
        ResetZoomAndSpeed = "Reset zoom & playback speed {0}";
        RemoveBlankLines = "Remove blank lines {0}";
        PlaySelectedRepeatHint = "Play selected subtitle(s) in repeat mode {0}";
    }
}