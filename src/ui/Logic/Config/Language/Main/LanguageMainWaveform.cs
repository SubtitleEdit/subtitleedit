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
    public string VideoPositionTextBox { get; set; }
    public string HideWaveformToolbar { get; set; }
    public string ResetZoomAndSpeed { get; set; }
    public string RemoveBlankLines { get; set; }
    public string PlaySelectedRepeatHint { get; set; }
    public string SeekBackHint { get; set; }
    public string SeekForwardHint { get; set; }
    public string SeekAmountHint { get; set; }
    public string SeekVideo { get; set; }
    public string ConfigureToolbarItems { get; set; }
    public string TextPreviousHint { get; set; }
    public string TextPlayHint { get; set; }
    public string TextPauseHint { get; set; }
    public string TextNextHint { get; set; }
    public string GuessStartNoLineSelected { get; set; }
    public string GuessStartTimeCodesLocked { get; set; }
    public string GuessStartNoWaveform { get; set; }
    public string GuessStartNoRoomBeforePreviousLineX { get; set; }
    public string GuessStartLineXAlreadyAtBoundary { get; set; }
    public string GuessStartNoSilenceFoundNearLineX { get; set; }
    public string GuessStartMovedLineXByYMs { get; set; }
    public string GuessEndNoLineSelected { get; set; }
    public string GuessEndTimeCodesLocked { get; set; }
    public string GuessEndNoWaveform { get; set; }
    public string GuessEndNoRoomBeforeNextLineX { get; set; }
    public string GuessEndLineXAlreadyAtBoundary { get; set; }
    public string GuessEndNoSilenceFoundNearLineX { get; set; }
    public string GuessEndMovedLineXByYMs { get; set; }

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
        VideoPositionTextBox = "Video position text box {0}";
        HideWaveformToolbar = "Hide toolbar {0}";
        ResetZoomAndSpeed = "Reset zoom & playback speed {0}";
        RemoveBlankLines = "Remove blank lines {0}";
        PlaySelectedRepeatHint = "Play selected subtitle(s) in repeat mode {0}";
        SeekBackHint = "Seek video backward {0}";
        SeekForwardHint = "Seek video forward {0}";
        SeekAmountHint = "Seek amount {0}";
        SeekVideo = "Seek video (<< >>)";
        ConfigureToolbarItems = "Configure toolbar items...";
        TextPreviousHint = "Play previous subtitle and stop at end {0}";
        TextPlayHint = "Play current subtitle and stop at end {0}";
        TextPauseHint = "Pause playback {0}";
        TextNextHint = "Play next subtitle and stop at end {0}";
        GuessStartNoLineSelected = "Guess start: no line selected";
        GuessStartTimeCodesLocked = "Guess start: time codes are locked";
        GuessStartNoWaveform = "Guess start: no waveform";
        GuessStartNoRoomBeforePreviousLineX = "Guess start: line {0} has no room before the previous line";
        GuessStartLineXAlreadyAtBoundary = "Guess start: line {0} already starts where the speech begins";
        GuessStartNoSilenceFoundNearLineX = "Guess start: no silence found near line {0}";
        GuessStartMovedLineXByYMs = "Guess start: line {0} start moved {1} ms";
        GuessEndNoLineSelected = "Guess end: no line selected";
        GuessEndTimeCodesLocked = "Guess end: time codes are locked";
        GuessEndNoWaveform = "Guess end: no waveform";
        GuessEndNoRoomBeforeNextLineX = "Guess end: line {0} has no room before the next line";
        GuessEndLineXAlreadyAtBoundary = "Guess end: line {0} already ends where the speech stops";
        GuessEndNoSilenceFoundNearLineX = "Guess end: no silence found near line {0}";
        GuessEndMovedLineXByYMs = "Guess end: line {0} end moved {1} ms";
    }
}