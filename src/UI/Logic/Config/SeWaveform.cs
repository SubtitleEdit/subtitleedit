using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeWaveform
{
    public bool ShowToolbar { get; set; }
    public bool CenterVideoPosition { get; set; }
    public bool DrawGridLines { get; set; }
    public bool FocusTextBoxAfterInsertNew { get; set; }
    public int SpectrogramCombinedWaveformHeight { get; set; }

    public bool ShowToolbarPlay { get; set; }
    public bool ShowToolbarPlayNext { get; set; }
    public bool ShowToolbarPlaySelection { get; set; }
    public bool ShowToolbarRepeat { get; set; }
    public bool ShowToolbarRemoveBlankLines { get; set; }
    public bool ShowToolbarNew { get; set; }
    public bool ShowToolbarSetStart { get; set; }
    public bool ShowToolbarSetEnd { get; set; }
    public bool ShowToolbarSetStartAndOffsetTheRest { get; set; }
    public bool ShowToolbarVerticalZoom { get; set; }
    public bool ShowToolbarHorizontalZoom { get; set; }
    public bool ShowToolbarVideoPositionSlider { get; set; }
    public bool ShowToolbarPlaybackSpeed { get; set; }

    public string WaveformColor { get; set; }
    public string WaveformBackgroundColor { get; set; }
    public string WaveformSelectedColor { get; set; }
    public string WaveformCursorColor { get; set; }
    public string WaveformFancyHighColor { get; set; }
    public string ParagraphBackground { get; set; }
    public string ParagraphSelectedBackground { get; set; }
    public bool InvertMouseWheel { get; set; }
    public double ShotChangesSensitivity { get; set; }
    public string ShotChangesImportTimeCodeFormat { get; set; }
    public bool SnapToShotChanges { get; set; }
    public bool ShotChangesAutoGenerate { get; set; }
    public int SnapToShotChangesPixels { get; set; }
    public bool FocusOnMouseOver { get; set; }
    public bool GuessTimeCodeStartFromBeginning { get; set; }
    public int GuessTimeCodeScanBlockSize { get; set; }
    public int GuessTimeCodeScanBlockAverageMin { get; set; }
    public int GuessTimeCodeScanBlockAverageMax { get; set; }
    public int GuessTimeCodeSplitLongSubtitlesAtMs { get; set; }
    public double SeekSilenceMinDurationSeconds { get; set; }
    public double SeekSilenceMaxVolume { get; set; }
    public bool SeekSilenceSeekForward { get; set; }
    public bool GenerateSpectrogram { get; set; }
    public string SpectrogramStyle { get; set; }
    public string WaveformDrawStyle { get; set; }
    public string LastDisplayMode { get; set; }
    public bool RightClickSelectsSubtitle { get; set; }
    public bool AllowOverlap { get; set; }
    public string SingleClickAction { get; set; }
    public string DoubleClickAction { get; set; }

    public SeWaveform()
    {
        ShowToolbar = true;
        DrawGridLines = false;
        FocusTextBoxAfterInsertNew = true;
        SpectrogramCombinedWaveformHeight = 50;
        WaveformColor = Color.FromArgb(255, 0, 70, 0).FromColorToHex();
        WaveformBackgroundColor = Color.FromArgb(255, 0, 0, 0).FromColorToHex();
        WaveformSelectedColor = Color.FromArgb(150, 0, 120, 255).FromColorToHex();
        WaveformCursorColor = Colors.Cyan.FromColorToHex();
        WaveformFancyHighColor = Colors.Orange.FromColorToHex();
        ParagraphBackground = Color.FromArgb(90, 70, 70, 70).FromColorToHex();
        ParagraphSelectedBackground = Color.FromArgb(90, 70, 70, 120).FromColorToHex();
        ShotChangesSensitivity = 0.4;
        ShotChangesImportTimeCodeFormat = "Seconds";
        SnapToShotChangesPixels = 8;
        SnapToShotChanges = true;
        ShotChangesAutoGenerate = false;
        ShowToolbarPlay = true;
        ShowToolbarRepeat = true;
        ShowToolbarRemoveBlankLines = false;
        ShowToolbarNew = true;
        ShowToolbarSetStart = true;
        ShowToolbarSetEnd = true;
        ShowToolbarSetStartAndOffsetTheRest = true;
        ShowToolbarVerticalZoom = true;
        ShowToolbarHorizontalZoom = true;
        ShowToolbarVideoPositionSlider = true;
        ShowToolbarPlaybackSpeed = true;
        ShowToolbarVerticalZoom = true;
        ShowToolbarHorizontalZoom = true;
        ShowToolbarVideoPositionSlider = true;
        ShowToolbarPlaybackSpeed = true;
        SpectrogramStyle = nameof(SeSpectrogramStyle.Classic);
        LastDisplayMode = nameof(WaveformDisplayMode.OnlyWaveform);
        WaveformDrawStyle = Controls.AudioVisualizerControl.WaveformDrawStyle.Fancy.ToString();
        RightClickSelectsSubtitle = true;
        SingleClickAction = WaveformSingleClickActionType.SetVideoPositionAndPauseAndSelectSubtitle.ToString();
        DoubleClickAction = nameof(WaveformDoubleClickActionType.None);

        GuessTimeCodeStartFromBeginning = false;
        GuessTimeCodeScanBlockSize = 100;
        GuessTimeCodeScanBlockAverageMin = 35;
        GuessTimeCodeScanBlockAverageMax = 70;
        GuessTimeCodeSplitLongSubtitlesAtMs = 3500;

        SeekSilenceSeekForward = true;
        SeekSilenceMinDurationSeconds = 0.3;
        SeekSilenceMaxVolume = 0.1;
    }
}