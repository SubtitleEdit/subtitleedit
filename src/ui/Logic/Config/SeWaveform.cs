using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeWaveform
{
    public bool ShowToolbar { get; set; }
    public bool CenterVideoPosition { get; set; }

    // SE 4 parity for the "Center" toggle: keep the play-head centered (and keep
    // "select current subtitle" working) also while paused, so mouse-wheel
    // scrubbing walks the waveform as one continuous strip.
    public bool CenterVideoPositionAlsoWhenPaused { get; set; }
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
    public string WaveformShotChangeColor { get; set; }
    public string WaveformParagraphLeftColor { get; set; }
    public string WaveformParagraphRightColor { get; set; }
    public string WaveformFancyHighColor { get; set; }
    public string ParagraphBackground { get; set; }
    public string ParagraphSelectedBackground { get; set; }
    public bool InvertMouseWheel { get; set; }

    // SE 4 parity: when true, the mouse wheel over the waveform seeks the video position
    // forward/backward (in steps) instead of scrolling the view. The view follows the
    // play-head so it stays visible. Defaults to false (SE 5 scroll behavior). Issue #12134.
    public bool MouseWheelSetsVideoPosition { get; set; }

    // Step size per wheel notch when MouseWheelSetsVideoPosition is on.
    // 0 = one frame (uses General.CurrentFrameRate); otherwise the step in milliseconds.
    public int MouseWheelVideoPositionStepMs { get; set; }

    // Last value picked in the waveform toolbar's "seek video" combo box (the << / >> buttons),
    // stored as the raw option string. A trailing "f" means frames, "s" (or no suffix) means
    // seconds. Remembered across sessions. See SeWaveformToolbarItemType.VideoSeek.
    public string VideoSeekAmount { get; set; }
    public double ShotChangesSensitivity { get; set; }
    public string ShotChangesImportTimeCodeFormat { get; set; }
    public bool SnapToShotChanges { get; set; }
    public bool SnapToFrames { get; set; }
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

    // Hidden setting (Settings.json only, no UI yet): when false, the waveform/peaks are not
    // generated automatically when a video is opened. Cached peaks still load, so previously
    // generated waveforms keep showing; new ones are only made on demand. Defaults to true.
    public bool WaveformAutoGenerate { get; set; }

    public string SpectrogramStyle { get; set; }
    public string WaveformDrawStyle { get; set; }
    public string LastDisplayMode { get; set; }
    public bool RightClickSelectsSubtitle { get; set; }
    public bool AllowOverlap { get; set; }
    public bool SetVideoPositionOnMoveStartEnd { get; set; }
    public string SingleClickAction { get; set; }
    public string DoubleClickAction { get; set; }
    public bool WaveformUnwrapText { get; set; }
    public int WaveformMinimumSampleRate { get; set; }

    // Defaults for the waveform "Extract audio..." context-menu action. The STT
    // audio-clip path stays at 16 kHz mono regardless of these; these only apply
    // to the user-initiated extract (issues #11235 / #11237).
    // ExtractAudioFormat: container/extension without the dot ("wav", "mp3", "m4a", "flac").
    // ExtractAudioSampleRate: 0 = keep the source sample rate; otherwise e.g. 48000.
    // ExtractAudioBitRate: bitrate for lossy formats (mp3/m4a), e.g. "192k"; ignored for wav/flac.
    public string ExtractAudioFormat { get; set; }
    public int ExtractAudioSampleRate { get; set; }
    public string ExtractAudioBitRate { get; set; }

    // SE 4 parity: small footer at the bottom-left of each paragraph rectangle showing
    // the subtitle number, duration, and characters-per-second. Defaults on so SE 5
    // matches the SE 4 out-of-box look; users can hide them via Settings.json.
    public bool WaveformShowNumberAndDuration { get; set; }
    public bool WaveformShowCps { get; set; }

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
        WaveformShotChangeColor = Colors.AntiqueWhite.FromColorToHex();
        WaveformParagraphLeftColor = Color.FromArgb(90, 0, 255, 0).FromColorToHex();
        WaveformParagraphRightColor = Color.FromArgb(90, 255, 0, 0).FromColorToHex();
        WaveformFancyHighColor = Colors.Orange.FromColorToHex();
        ParagraphBackground = Color.FromArgb(90, 70, 70, 70).FromColorToHex();
        ParagraphSelectedBackground = Color.FromArgb(90, 70, 70, 120).FromColorToHex();
        ShotChangesSensitivity = 0.4;
        ShotChangesImportTimeCodeFormat = "Seconds";
        SnapToShotChangesPixels = 8;
        SnapToShotChanges = true;
        SnapToFrames = false;
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
        WaveformMinimumSampleRate = 126;
        WaveformAutoGenerate = true;

        ExtractAudioFormat = "wav";
        ExtractAudioSampleRate = 0; // keep source sample rate
        ExtractAudioBitRate = "192k";

        WaveformShowNumberAndDuration = true;
        WaveformShowCps = true;

        MouseWheelVideoPositionStepMs = 500;
        VideoSeekAmount = "2s";

        ToolbarItems =
        [
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.TextPrevious, IsVisible = false, SortOrder = 2 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.TextPlay, IsVisible = false, SortOrder = 4 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.TextPause, IsVisible = false, SortOrder = 6 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.TextNext, IsVisible = false, SortOrder = 8 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.Play, IsVisible = true, SortOrder = 10 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.PlayNext, IsVisible = false, SortOrder = 20 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.PlaySelection, IsVisible = false, SortOrder = 30 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.Repeat, IsVisible = true, SortOrder = 40 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.RemoveBlankLines, IsVisible = false, SortOrder = 50 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.New, IsVisible = true, SortOrder = 60 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.SetStart, IsVisible = true, SortOrder = 70 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.SetEnd, IsVisible = true, SortOrder = 80 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.SetStartAndOffsetTheRest, IsVisible = true, SortOrder = 90 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.VideoSeek, IsVisible = false, SortOrder = 95 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.VerticalZoom, IsVisible = true, SortOrder = 100 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.HorizontalZoom, IsVisible = true, SortOrder = 110 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.VideoPositionSlider, IsVisible = true, SortOrder = 120 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.PlaybackSpeed, IsVisible = true, SortOrder = 130 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.AutoSelectOnPlay, IsVisible = true, SortOrder = 140 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.Center, IsVisible = true, SortOrder = 150 },
            new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.More, IsVisible = true, SortOrder = 160 },
        ];
    }

    /// <summary>
    /// Adds any toolbar item types missing from a loaded (older) settings file so newly added
    /// items (e.g. VideoSeek) appear for existing users and InitWaveform's per-type lookup can't
    /// throw. Missing items take their sort order from the defaults, so when the user enables one
    /// it shows up at its intended spot (the configure dialog rewrites sort orders as 10, 20, ...,
    /// so a default below 10 lands left of everything, one above the maximum lands at the end).
    /// </summary>
    public void EnsureAllToolbarItems()
    {
        if (ToolbarItems == null)
        {
            ToolbarItems = new List<SeWaveformToolbarItem>();
        }

        List<SeWaveformToolbarItem>? defaults = null;
        foreach (SeWaveformToolbarItemType type in System.Enum.GetValues(typeof(SeWaveformToolbarItemType)))
        {
            if (ToolbarItems.Exists(p => p.Type == type))
            {
                continue;
            }

            defaults ??= new SeWaveform().ToolbarItems;
            var defaultItem = defaults.Find(p => p.Type == type);
            var sortOrder = defaultItem?.SortOrder ??
                            (ToolbarItems.Count > 0 ? ToolbarItems.Max(p => p.SortOrder) + 10 : 10);

            // Missing items were introduced after this settings file was written; add them hidden
            // so an upgrade never silently changes a toolbar the user has already tuned.
            ToolbarItems.Add(new SeWaveformToolbarItem { Type = type, IsVisible = false, SortOrder = sortOrder });
        }
    }
}