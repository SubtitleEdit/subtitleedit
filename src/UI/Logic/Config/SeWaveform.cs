using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeWaveform
{
    public bool ShowToolbar { get; set; }
    public bool CenterVideoPosition { get; set; }
    public bool DrawGridLines { get; set; }
    public bool FocusTextBoxAfterInsertNew { get; set; }
    public int SpectrogramCombinedWaveformHeight { get; set; }

    public List<SeWaveformToolbarItem> ToolbarItems { get; set; }

    public int WaveformTextFontSize { get; set; }
    public bool WaveformTextFontBold { get; set; }
    public string WaveformTextColor { get; set; }
    public string WaveformColor { get; set; }
    public string WaveformBackgroundColor { get; set; }
    public string WaveformSelectedColor { get; set; }
    public string WaveformCursorColor { get; set; }
    public string WaveformParagraphLeftColor { get; set; }
    public string WaveformParagraphRightColor { get; set; }
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
        WaveformTextFontSize = 10;
        WaveformTextFontBold = false;
        WaveformTextColor = Colors.White.FromColorToHex();
        WaveformColor = Color.FromArgb(255, 0, 70, 0).FromColorToHex();
        WaveformBackgroundColor = Color.FromArgb(255, 0, 0, 0).FromColorToHex();
        WaveformSelectedColor = Color.FromArgb(150, 0, 120, 255).FromColorToHex();
        WaveformCursorColor = Colors.Cyan.FromColorToHex();
        WaveformParagraphLeftColor = Color.FromArgb(90, 0, 255, 0).FromColorToHex();
        WaveformParagraphRightColor = Color.FromArgb(90, 255, 0, 0).FromColorToHex();
        WaveformFancyHighColor = Colors.Orange.FromColorToHex();
        ParagraphBackground = Color.FromArgb(90, 70, 70, 70).FromColorToHex();
        ParagraphSelectedBackground = Color.FromArgb(90, 70, 70, 120).FromColorToHex();
        ShotChangesSensitivity = 0.4;
        ShotChangesImportTimeCodeFormat = "Seconds";
        SnapToShotChangesPixels = 8;
        SnapToShotChanges = true;
        ShotChangesAutoGenerate = false;

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

        ToolbarItems =
        [
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.Play, IsVisible = true, SortOrder = 10 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.PlayNext, IsVisible = false, SortOrder = 20 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.PlaySelection, IsVisible = false, SortOrder = 30 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.Repeat, IsVisible = true, SortOrder = 40 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.RemoveBlankLines, IsVisible = false, SortOrder = 50 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.New, IsVisible = true, SortOrder = 60 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.SetStart, IsVisible = true, SortOrder = 70 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.SetEnd, IsVisible = true, SortOrder = 80 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.SetStartAndOffsetTheRest, IsVisible = true, SortOrder = 90 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.VerticalZoom, IsVisible = true, SortOrder = 100 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.HorizontalZoom, IsVisible = true, SortOrder = 110 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.VideoPositionSlider, IsVisible = true, SortOrder = 120 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.PlaybackSpeed, IsVisible = true, SortOrder = 130 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.AutoSelectOnPlay, IsVisible = true, SortOrder = 140 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.Center, IsVisible = true, SortOrder = 150 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.More, IsVisible = true, SortOrder = 160 },
        ];
    }
}