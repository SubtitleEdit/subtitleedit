using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaProgressBar;

public partial class AssaProgressBarViewModel : ObservableObject
{
    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    // Progress bar settings
    [ObservableProperty] private bool _positionTop;
    [ObservableProperty] private bool _positionBottom = true;
    [ObservableProperty] private int _barHeight = 40;
    [ObservableProperty] private Color _foregroundColor = Colors.LimeGreen;
    [ObservableProperty] private Color _backgroundColor = Color.FromRgb(64, 64, 64);
    [ObservableProperty] private Color _textColor = Colors.White;
    [ObservableProperty] private int _cornerStyleIndex; // 0 = square, 1 = rounded
    [ObservableProperty] private ObservableCollection<string> _cornerStyles = [];

    // Chapters settings
    [ObservableProperty] private int _splitterWidth = 2;
    [ObservableProperty] private int _splitterHeight = 10;
    [ObservableProperty] private string _fontName = "Arial";
    [ObservableProperty] private int _fontSize = 12;
    [ObservableProperty] private int _xAdjustment;
    [ObservableProperty] private int _yAdjustment;
    [ObservableProperty] private int _textAlignmentIndex; // 0 = left, 1 = center, 2 = right
    [ObservableProperty] private ObservableCollection<string> _textAlignments = [];
    [ObservableProperty] private ObservableCollection<string> _fonts = [];

    // Chapters list
    [ObservableProperty] private ObservableCollection<ProgressBarChapter> _chapters = [];
    [ObservableProperty] private ProgressBarChapter? _selectedChapter;
    [ObservableProperty] private string _chapterText = string.Empty;
    [ObservableProperty] private TimeSpan _chapterStartTime;

    // Preview
    [ObservableProperty] private string _previewCode = string.Empty;

    // Video info
    private int _videoWidth = 1920;
    private int _videoHeight = 1080;
    private double _videoDurationMs = 3600000; // 1 hour default

    private Subtitle _subtitle = new();
    private Subtitle _progressBarSubtitle = new();
    private string? _videoFileName;
    private LibMpvDynamicPlayer? _mpvPlayer;
    private string _oldSubtitleText = string.Empty;
    private DispatcherTimer _positionTimer = new DispatcherTimer();
    private readonly SubtitleFormat _assaFormat = new AdvancedSubStationAlpha();
    private readonly string _tempSubtitleFileName;
    private bool _isSubtitleLoaded;

    public Subtitle ResultSubtitle => _subtitle;
    public VideoPlayerControl? VideoPlayerControl { get; set; }

    public AssaProgressBarViewModel()
    {
        CornerStyles.Add(Se.Language.Assa.ProgressBarSquareCorners);
        CornerStyles.Add(Se.Language.Assa.ProgressBarRoundedCorners);

        TextAlignments.Add(Se.Language.General.Left);
        TextAlignments.Add(Se.Language.General.Center);
        TextAlignments.Add(Se.Language.General.Right);

        // Load fonts
        foreach (var fontFamily in FontManager.Current.SystemFonts.OrderBy(f => f.Name))
        {
            Fonts.Add(fontFamily.Name);
        }

        if (Fonts.Contains("Arial"))
        {
            FontName = "Arial";
        }
        else if (Fonts.Count > 0)
        {
            FontName = Fonts[0];
        }
        
        _tempSubtitleFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ass");

        BarHeight = Se.Settings.Assa.ProgressBarHeight;
        ForegroundColor = Se.Settings.Assa.ProgressBarForegroundColor.FromHexToColor();
        BackgroundColor = Se.Settings.Assa.ProgressBarBackgroundColor.FromHexToColor();
        if (Se.Settings.Assa.ProgressBarCornerStyleIndex >= 0 && Se.Settings.Assa.ProgressBarCornerStyleIndex < CornerStyles.Count)
        {
            CornerStyleIndex = Se.Settings.Assa.ProgressBarCornerStyleIndex;
        }
    }

    public void Initialize(Subtitle subtitle, int videoWidth, int videoHeight, double videoDurationMs, string? videoFileName)
    {
        _subtitle = new Subtitle(subtitle, false);
        _videoWidth = videoWidth > 0 ? videoWidth : 1920;
        _videoHeight = videoHeight > 0 ? videoHeight : 1080;
        _videoDurationMs = videoDurationMs > 0 ? videoDurationMs : 3600000;
        _videoFileName = videoFileName;

        LoadExistingSettings();
        _subtitle.Paragraphs.Clear();
        GeneratePreview();
        
        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(videoFileName) && VideoPlayerControl != null)
            {
                _ = VideoPlayerControl.Open(videoFileName);
            }

            StartTitleTimer();
        });
    }
    
    private void StartTitleTimer()
    {
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _positionTimer.Tick += (s, e) =>
        {
            if (_mpvPlayer == null)
            {
                return;
            }

            ApplyProgressBar();
            var text = _assaFormat.ToText(_subtitle, string.Empty);
            if (text == _oldSubtitleText)
            {
                return;
            }
            
            var subtitle = new Subtitle();
            
            File.WriteAllText(_tempSubtitleFileName, text);
            if (!_isSubtitleLoaded)
            {
                _isSubtitleLoaded = true;
                _mpvPlayer.SubAdd(_tempSubtitleFileName);
            }
            else
            {
                _mpvPlayer.SubReload();
            }
            
            _oldSubtitleText = text;
        };

        _positionTimer.Start();
    }

    private void LoadExistingSettings()
    {
        // Load existing chapters from subtitle
        foreach (var p in _subtitle.Paragraphs)
        {
            if (p.Extra == "SE-progress-bar-text")
            {
                var chapter = new ProgressBarChapter
                {
                    Text = Utilities.RemoveSsaTags(p.Text)
                };

                if (double.TryParse(p.Actor, NumberStyles.Float, CultureInfo.InvariantCulture, out var ms))
                {
                    chapter.StartTimeMs = ms;
                }

                Chapters.Add(chapter);
            }
        }

        // Load style settings
        foreach (var style in AdvancedSubStationAlpha.GetSsaStylesFromHeader(_subtitle.Header))
        {
            if (style.Name == "SE-progress-bar-text")
            {
                FontName = style.FontName;
                FontSize = (int)style.FontSize;
            }
            else if (style.Name == "SE-progress-bar-bg")
            {
                PositionTop = style.Alignment == "7";
                PositionBottom = !PositionTop;
            }
        }
    }

    partial void OnPositionTopChanged(bool value)
    {
        if (value)
        {
            PositionBottom = false;
        }
        GeneratePreview();
    }

    partial void OnPositionBottomChanged(bool value)
    {
        if (value)
        {
            PositionTop = false;
        }
        GeneratePreview();
    }

    partial void OnBarHeightChanged(int value) => GeneratePreview();
    partial void OnForegroundColorChanged(Color value) => GeneratePreview();
    partial void OnBackgroundColorChanged(Color value) => GeneratePreview();
    partial void OnTextColorChanged(Color value) => GeneratePreview();
    partial void OnCornerStyleIndexChanged(int value) => GeneratePreview();
    partial void OnSplitterWidthChanged(int value) => GeneratePreview();
    partial void OnSplitterHeightChanged(int value) => GeneratePreview();
    partial void OnFontNameChanged(string value) => GeneratePreview();
    partial void OnFontSizeChanged(int value) => GeneratePreview();
    partial void OnXAdjustmentChanged(int value) => GeneratePreview();
    partial void OnYAdjustmentChanged(int value) => GeneratePreview();
    partial void OnTextAlignmentIndexChanged(int value) => GeneratePreview();

    partial void OnSelectedChapterChanged(ProgressBarChapter? value)
    {
        if (value != null)
        {
            ChapterText = value.Text;
            ChapterStartTime = TimeSpan.FromMilliseconds(value.StartTimeMs);
        }
    }

    partial void OnChapterTextChanged(string value)
    {
        if (SelectedChapter != null)
        {
            SelectedChapter.Text = value;
            GeneratePreview();
        }
    }

    partial void OnChapterStartTimeChanged(TimeSpan value)
    {
        if (SelectedChapter != null)
        {
            SelectedChapter.StartTimeMs = value.TotalMilliseconds;
            GeneratePreview();
        }
    }

    [RelayCommand]
    private void AddChapter()
    {
        double startTimeMs = 0;
        if (Chapters.Count > 0)
        {
            startTimeMs = Chapters.Last().StartTimeMs + 300000; // 5 minutes after last
            if (startTimeMs > _videoDurationMs)
            {
                startTimeMs = _videoDurationMs - 60000;
            }
        }

        var chapter = new ProgressBarChapter
        {
            Text = $"Chapter {Chapters.Count + 1}",
            StartTimeMs = Math.Max(0, startTimeMs)
        };

        Chapters.Add(chapter);
        SelectedChapter = chapter;
        GeneratePreview();
    }

    [RelayCommand]
    private void RemoveChapter()
    {
        if (SelectedChapter != null)
        {
            var index = Chapters.IndexOf(SelectedChapter);
            Chapters.Remove(SelectedChapter);

            if (Chapters.Count > 0)
            {
                SelectedChapter = Chapters[Math.Min(index, Chapters.Count - 1)];
            }
            else
            {
                SelectedChapter = null;
            }

            GeneratePreview();
        }
    }

    [RelayCommand]
    private void RemoveAllChapters()
    {
        Chapters.Clear();
        SelectedChapter = null;
        GeneratePreview();
    }

    private void GeneratePreview()
    {
        _progressBarSubtitle = new Subtitle();

        var alignment = PositionTop ? "7" : "1";
        var textAlignment = TextAlignmentIndex switch
        {
            1 => PositionTop ? "8" : "2", // center
            2 => PositionTop ? "9" : "3", // right
            _ => PositionTop ? "7" : "1"  // left
        };

        var foreColor = ToAssaColor(ForegroundColor);
        var backColor = ToAssaColor(BackgroundColor);
        var txtColor = ToAssaColor(TextColor);

        var script = $@"[Script Info]
; ASSA Progress Bar
ScriptType: v4.00+
ScaledBorderAndShadow: Yes
PlayResX: {_videoWidth}
PlayResY: {_videoHeight}

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: SE-progress-bar-splitter,Arial,20,{txtColor},&H0000FFFF,&H00000000,&H00000000,-1,0,0,0,100,100,0,0,1,0,0,7,0,0,0,1
Style: SE-progress-bar-bg,Arial,20,{foreColor},{backColor},&H00FFFFFF,&H00000000,0,0,0,0,100,100,0,0,1,0,0,{alignment},0,0,0,1
Style: SE-progress-bar-text,{FontName},{FontSize},{txtColor},&H0000FFFF,&H00000000,&H00000000,-1,0,0,0,100,100,0,0,1,0,0,{textAlignment},0,0,0,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
";

        var durationInCs = (int)Math.Round(_videoDurationMs / 10.0);
        var pbDrawing = GenerateProgressBarDrawing(durationInCs);

        script += $"Dialogue: -255,0:00:00.00,{TimeSpan.FromMilliseconds(_videoDurationMs):hh\\:mm\\:ss\\.ff},SE-progress-bar-bg,,0,0,0,,{pbDrawing}\n";

        new AdvancedSubStationAlpha().LoadSubtitle(_progressBarSubtitle, script.SplitToLines(), string.Empty);

        // Add chapters
        if (Chapters.Count > 0)
        {
            var layer = -254;
            for (int i = 0; i < Chapters.Count; i++)
            {
                var chapter = Chapters[i];
                var percentOfDuration = chapter.StartTimeMs * 100.0 / _videoDurationMs;
                var position = (int)Math.Round(_videoWidth * percentOfDuration / 100.0);
                var textPosition = GetTextPosition(position, i, percentOfDuration);

                string splitterText;
                if (PositionTop)
                {
                    var splitterTop = 0;
                    if (SplitterHeight < BarHeight)
                    {
                        splitterTop += (BarHeight - SplitterHeight) / 2;
                    }
                    splitterText = $"{{\\p1}}m {position} {splitterTop} l " +
                        $"{position + SplitterWidth} {splitterTop} " +
                        $"{position + SplitterWidth} {splitterTop + SplitterHeight} " +
                        $"{position} {splitterTop + SplitterHeight} " +
                        $"{position} {splitterTop}" +
                        $"{{\\p0}}";
                }
                else
                {
                    var splitterTop = _videoHeight - BarHeight;
                    if (SplitterHeight < BarHeight)
                    {
                        splitterTop += (BarHeight - SplitterHeight) / 2;
                    }
                    splitterText = $"{{\\p1}}m {position} {splitterTop} l " +
                        $"{position + SplitterWidth} {splitterTop} " +
                        $"{position + SplitterWidth} {splitterTop + SplitterHeight} " +
                        $"{position} {splitterTop + SplitterHeight} " +
                        $"{position} {splitterTop}" +
                        $"{{\\p0}}";
                }

                // Add splitter (except for first chapter)
                if (i > 0)
                {
                    var splitter = new Paragraph(splitterText, 0, _videoDurationMs)
                    {
                        Extra = "SE-progress-bar-splitter",
                        Layer = layer++,
                    };
                    _progressBarSubtitle.Paragraphs.Add(splitter);
                }

                // Add chapter text
                var yPos = PositionTop
                    ? (int)Math.Round(BarHeight / 2.0 + YAdjustment)
                    : (int)Math.Round(_videoHeight - BarHeight / 2.0 + YAdjustment);

                var posTag = $"{{\\pos({textPosition}, {yPos})}}";
                var chapterInfo = new Paragraph(posTag + chapter.Text, 0, _videoDurationMs)
                {
                    Extra = "SE-progress-bar-text",
                    Layer = -1,
                    Actor = chapter.StartTimeMs.ToString(CultureInfo.InvariantCulture),
                };
                _progressBarSubtitle.Paragraphs.Add(chapterInfo);
            }
        }

        PreviewCode = new AdvancedSubStationAlpha().ToText(_progressBarSubtitle, string.Empty);
    }

    private string GenerateProgressBarDrawing(int durationInCs)
    {
        if (CornerStyleIndex == 1) // Rounded corners
        {
            var barEnd = _videoWidth - BarHeight;
            return $@"{{\K{durationInCs}\p1}}m {BarHeight} 0 b 0 0 0 {BarHeight} {BarHeight} {BarHeight} l {barEnd} {BarHeight} b {_videoWidth} {BarHeight} {_videoWidth} 0 {barEnd} 0 l {barEnd} 0 {BarHeight} 0{{\p0}}";
        }

        return $"{{\\K{durationInCs}\\p1}}m 0 0 l {_videoWidth} 0 {_videoWidth} {BarHeight} 0 {BarHeight}{{\\p0}}";
    }

    private int GetTextPosition(int position, int chapterIndex, double percentOfDuration)
    {
        if (TextAlignmentIndex == 1) // Center
        {
            var positionNextX = _videoWidth;
            if (chapterIndex + 1 < Chapters.Count)
            {
                var next = Chapters[chapterIndex + 1];
                var percentNext = next.StartTimeMs * 100.0 / _videoDurationMs;
                positionNextX = (int)Math.Round(_videoWidth * percentNext / 100.0);
            }
            return (int)Math.Round((position + positionNextX) / 2.0 + XAdjustment);
        }
        else if (TextAlignmentIndex == 2) // Right
        {
            var positionNextX = _videoWidth;
            if (chapterIndex + 1 < Chapters.Count)
            {
                var next = Chapters[chapterIndex + 1];
                var percentNext = next.StartTimeMs * 100.0 / _videoDurationMs;
                positionNextX = (int)Math.Round(_videoWidth * percentNext / 100.0);
            }

            if (CornerStyleIndex == 1 && chapterIndex == Chapters.Count - 1)
            {
                return (int)Math.Round((double)(positionNextX - BarHeight + XAdjustment));
            }

            return (int)Math.Round((double)(positionNextX) - 10.0 + (double)XAdjustment);
        }

        // Left
        if (CornerStyleIndex == 1 && chapterIndex == 0)
        {
            return (int)Math.Round((double)position + (double)BarHeight + (double)SplitterWidth + (double)XAdjustment);
        }

        return (int)Math.Round((double)position + 8.0 + (double)SplitterWidth + (double)XAdjustment);
    }

    private static string ToAssaColor(Color color)
    {
        var alpha = 255 - color.A;
        return $"&H{alpha:X2}{color.B:X2}{color.G:X2}{color.R:X2}";
    }

    [RelayCommand]
    private void Ok()
    {
        Se.Settings.Assa.ProgressBarHeight = BarHeight;
        Se.Settings.Assa.ProgressBarForegroundColor = ForegroundColor.FromColorToHex();
        Se.Settings.Assa.ProgressBarBackgroundColor = BackgroundColor.FromColorToHex();
        Se.Settings.Assa.ProgressBarCornerStyleIndex = CornerStyleIndex;
        
        ApplyProgressBar();
        OkPressed = true;
        Close();
    }

    private void ApplyProgressBar()
    {
        if (string.IsNullOrEmpty(_subtitle.Header))
        {
            _subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
        }

        // Get new styles and merge
        var newStyles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(_progressBarSubtitle.Header);
        var ignoreStyleNames = new[] { "SE-progress-bar-text", "SE-progress-bar-splitter", "SE-progress-bar-bg" };
        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(_subtitle.Header)
            .Where(p => !ignoreStyleNames.Contains(p.Name))
            .ToList();
        styles.AddRange(newStyles);
        _subtitle.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(_subtitle.Header, styles);

        // Remove existing progress bar paragraphs
        for (var i = _subtitle.Paragraphs.Count - 1; i >= 0; i--)
        {
            if (ignoreStyleNames.Contains(_subtitle.Paragraphs[i].Extra))
            {
                _subtitle.Paragraphs.RemoveAt(i);
            }
        }

        // Add new progress bar paragraphs
        _subtitle.Paragraphs.AddRange(_progressBarSubtitle.Paragraphs);

        // Update PlayRes
        _subtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + _videoWidth.ToString(CultureInfo.InvariantCulture), "[Script Info]", _subtitle.Header);
        _subtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + _videoHeight.ToString(CultureInfo.InvariantCulture), "[Script Info]", _subtitle.Header);
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private void Reset()
    {
        PositionBottom = true;
        PositionTop = false;
        BarHeight = 20;
        ForegroundColor = Colors.LimeGreen;
        BackgroundColor = Color.FromRgb(64, 64, 64);
        TextColor = Colors.White;
        CornerStyleIndex = 0;
        SplitterWidth = 2;
        SplitterHeight = 10;
        FontName = "Arial";
        FontSize = 12;
        XAdjustment = 0;
        YAdjustment = 0;
        TextAlignmentIndex = 0;
        Chapters.Clear();
        SelectedChapter = null;
        GeneratePreview();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    public async void LoadVideoAndSubtitle()
    {
        if (VideoPlayerControl == null)
        {
            return;
        }
        
        // Wait a bit for video players to finish opening the file (or until they report a duration)
        await VideoPlayerControl.WaitForPlayersReadyAsync();
        Dispatcher.UIThread.Post(() =>
        {
            _mpvPlayer = VideoPlayerControl.VideoPlayerInstance as LibMpvDynamicPlayer;
        });
    }
}
