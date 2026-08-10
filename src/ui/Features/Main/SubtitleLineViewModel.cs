using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class SubtitleLineViewModel : ObservableObject
{
    [ObservableProperty]
    private int _number;

    [ObservableProperty]
    private string? _bookmark;

    [ObservableProperty]
    private TimeSpan _startTime;

    [ObservableProperty]
    private TimeSpan _endTime;

    [ObservableProperty]
    private TimeSpan _duration;

    [ObservableProperty]
    private string _text;

    [ObservableProperty]
    private string _originalText;

    [ObservableProperty]
    private string _style;

    [ObservableProperty]
    private string _actor;

    [ObservableProperty]
    private int _layer;

    [ObservableProperty]
    private double _gap;

    [ObservableProperty]
    private double _previousGap;

    [ObservableProperty]
    private bool _isHidden;

    /// <summary>
    /// Re-raises change notification for the text columns so the grid re-runs its converters
    /// (e.g. after a theme change, where the syntax highlighting colors differ).
    /// </summary>
    public void RefreshTextRendering()
    {
        OnPropertyChanged(nameof(Text));
        OnPropertyChanged(nameof(OriginalText));
    }

    public Paragraph? Paragraph { get; set; }
    public string Extra { get; set; }
    public string Language { get; set; }
    public string Region { get; set; }
    public string Effect { get; set; }
    public bool IsComment { get; set; }
    public string MarginL { get; set; }
    public string MarginR { get; set; }
    public string MarginV { get; set; }
    public bool NewSection { get; set; }
    public bool Forced { get; set; }
    public Guid Id { get; set; }
    public bool IsCpsColumnVisible { get; set; } = true;
    public bool IsDefault => Text == string.Empty && Number == 0 && Duration == TimeSpan.Zero && StartTime == TimeSpan.Zero;


    private bool _skipUpdate = false;

    private static SolidColorBrush _errorBrush = new SolidColorBrush(Se.Settings.General.ErrorColor.FromHexToColor());
    private static SolidColorBrush _transparentBrush = new SolidColorBrush(Colors.Transparent);
    public static Color ErrorColor
    {
        get => field;
        set
        {
            field = value;
            _errorBrush = new SolidColorBrush(value);
        }
    } = Se.Settings.General.ErrorColor.FromHexToColor();

    public SubtitleLineViewModel()
    {
        Text = string.Empty;
        OriginalText = string.Empty;
        Extra = string.Empty;
        Language = string.Empty;
        Region = string.Empty;
        Effect = string.Empty;
        MarginL = string.Empty;
        MarginR = string.Empty;
        MarginV = string.Empty;
        Style = string.Empty;
        Actor = string.Empty;
        Layer = 0;
        Id = Guid.NewGuid();
    }

    public SubtitleLineViewModel(SubtitleLineViewModel p, bool generateNewId = false)
    {
        // The observable properties are written as backing fields, not through their setters.
        // Nothing can be subscribed to an object still inside its own constructor, so every
        // notification the setters raise is discarded - but ObservableObject allocates a
        // PropertyChanging/PropertyChangedEventArgs for each one anyway, and the Text and
        // StartTime/EndTime setters fan out to a dozen more raises via their partial hooks.
        // That was ~40 dead allocations per line, and undo snapshots this whole collection
        // (issue #13234). The hooks are notification-only apart from UpdateDuration, which is
        // what _duration is set to below.
        _text = p.Text;
        _originalText = p.OriginalText;
        _startTime = p.StartTime;
        _endTime = p.EndTime;
        _duration = p.EndTime - p.StartTime;
        _style = p.Style;
        _actor = p.Actor;
        _layer = p.Layer;
        _number = p.Number;
        Language = p.Language;
        Region = p.Region;
        Extra = p.Extra;
        Effect = p.Effect;
        IsComment = p.IsComment;
        MarginL = p.MarginL;
        MarginR = p.MarginR;
        MarginV = p.MarginV;
        NewSection = p.NewSection;
        Forced = p.Forced;
        _bookmark = p.Bookmark;

        Id = generateNewId ? Guid.NewGuid() : p.Id;

        if (p.Paragraph != null)
        {
            Paragraph = new Paragraph(p.Paragraph, generateNewId);
        }
    }

    public SubtitleLineViewModel(Paragraph paragraph, SubtitleFormat subtitleFormat)
    {
        Text = paragraph.Text;
        OriginalText = string.Empty;
        Extra = paragraph.Extra;
        Language = paragraph.Language;
        Region = paragraph.Region;
        Effect = paragraph.Effect;
        IsComment = paragraph.IsComment;
        MarginL = paragraph.MarginL;
        MarginR = paragraph.MarginR;
        MarginV = paragraph.MarginV;
        NewSection = paragraph.NewSection;
        Forced = paragraph.Forced;
        Style = paragraph.Style;
        Actor = paragraph.Actor;
        Layer = paragraph.Layer;
        Number = paragraph.Number;
        StartTime = TimeSpan.FromMilliseconds(paragraph.StartTime.TotalMilliseconds);
        EndTime = TimeSpan.FromMilliseconds(paragraph.EndTime.TotalMilliseconds);
        UpdateDuration();
        Id = paragraph.Id ?? Guid.NewGuid();
        Paragraph = paragraph;
        Bookmark = paragraph.Bookmark;

        if (subtitleFormat is AdvancedSubStationAlpha or SubStationAlpha)
        {
            Style = paragraph.Extra;
        }
    }

    public Paragraph ToParagraph(SubtitleFormat? subtitleFormat = null)
    {
        var p = new Paragraph()
        {
            Number = Number,
            StartTime = new TimeCode(StartTime),
            EndTime = new TimeCode(EndTime),
            // TrimEnd: the edit text box is bound raw, so a trailing Enter lives in Text
            // until the row loses selection - it must never reach saved files or tools
            // (SE4 kept the same invariant by trimming in the TextChanged handler) - #13389.
            Text = Text.TrimEnd(),
            Actor = Actor,
            Style = Style,
            Language = Language,
            Region = Region,
            Effect = Effect,
            IsComment = IsComment,
            MarginL = MarginL,
            MarginR = MarginR,
            MarginV = MarginV,
            NewSection = NewSection,
            Forced = Forced,
            Layer = Layer,
            Bookmark = Bookmark,
        };

        if (subtitleFormat is AdvancedSubStationAlpha or SubStationAlpha)
        {
            p.Extra = Style;
        }

        return p;
    }

    public Paragraph ToParagraphOriginal(SubtitleFormat? subtitleFormat = null)
    {
        var p = new Paragraph
        {
            Number = Number,
            StartTime = new TimeCode(StartTime),
            EndTime = new TimeCode(EndTime),
            Text = OriginalText.TrimEnd(),
            Actor = Actor,
            Style = Style,
            Language = Language,
            Region = Region,
            Effect = Effect,
            IsComment = IsComment,
            MarginL = MarginL,
            MarginR = MarginR,
            MarginV = MarginV,
            NewSection = NewSection,
            Forced = Forced,
            Layer = Layer,
            Bookmark = Bookmark,
        };

        if (subtitleFormat is AdvancedSubStationAlpha or SubStationAlpha)
        {
            p.Extra = Style;
        }

        return p;
    }

    // Read-time memo for the html-stripped, line-split text: the pixel width column, the text
    // error verdict, GetErrors and the edit box's line-length panel all need it, and each used
    // to strip and split the text again - three times per line for a single error scan. Keyed
    // on the text instance like the memos below. The returned string/list are shared, so
    // callers must only read them.
    private string? _strippedLinesCacheText;
    private string? _strippedTextCacheValue;
    private List<string>? _strippedLinesCacheValue;

    private void EnsureStrippedCache()
    {
        if (_strippedLinesCacheValue == null || !ReferenceEquals(_strippedLinesCacheText, Text))
        {
            _strippedTextCacheValue = SubtitleTextInfoHelper.StripHtml(Text);
            _strippedLinesCacheValue = _strippedTextCacheValue.SplitToLines();
            _strippedLinesCacheText = Text;
        }
    }

    internal string GetStrippedText()
    {
        EnsureStrippedCache();
        return _strippedTextCacheValue!;
    }

    internal List<string> GetStrippedLines()
    {
        EnsureStrippedCache();
        return _strippedLinesCacheValue!;
    }

    // Read-time memos for the two WebVTT grid columns below, keyed on the text instance like
    // the memos around them - both parse the text, and a cell binding re-reads its value on
    // every repaint.
    private string? _webVttStyleCacheText;
    private string? _webVttStyleCacheValue;
    private string? _webVttVoiceCacheText;
    private string? _webVttVoiceCacheValue;

    /// <summary>
    /// The WebVTT cue classes of this line ("&lt;c.loud.red&gt;" shows as "loud, red"), for the
    /// grid's Style column in WebVTT. WebVTT keeps them inside the cue text rather than in a
    /// field of its own, so unlike the ASSA style this is derived from <see cref="Text"/>.
    /// </summary>
    public string WebVttStyle
    {
        get
        {
            if (!ReferenceEquals(_webVttStyleCacheText, Text) || _webVttStyleCacheValue == null)
            {
                var styles = WebVttHelper.GetParagraphStyles(Text);
                _webVttStyleCacheValue = string.Join(", ", styles.Select(p => p.TrimStart('.')));
                _webVttStyleCacheText = Text;
            }

            return _webVttStyleCacheValue;
        }
    }

    /// <summary>
    /// The WebVTT voice of this line (the "&lt;v Name&gt;" tag), for the grid's Voice column -
    /// WebVTT's counterpart of the ASSA actor. Derived from <see cref="Text"/>, see
    /// <see cref="WebVttStyle"/>.
    /// </summary>
    public string WebVttVoice
    {
        get
        {
            if (!ReferenceEquals(_webVttVoiceCacheText, Text) || _webVttVoiceCacheValue == null)
            {
                _webVttVoiceCacheValue = WebVTT.GetVoice(Text);
                _webVttVoiceCacheText = Text;
            }

            return _webVttVoiceCacheValue;
        }
    }

    // Read-time memo, see CharactersPerSecond below: the pixel-width column binding re-reads this
    // on every cell repaint and each read shapes every line with HarfBuzz.
    private string? _pixelWidthCacheText;
    private string? _pixelWidthCacheFontName;
    private int _pixelWidthCacheFontSize;
    private int _pixelWidthCacheValue;

    public int PixelWidth
    {
        get
        {
            var general = Se.Settings.General;
            if (!general.ShowColumnPixelWidth && !general.ColorTextTooWide)
            {
                return 0;
            }

            if (ReferenceEquals(_pixelWidthCacheText, Text) &&
                _pixelWidthCacheFontName == general.ColorTextTooWideFontName &&
                _pixelWidthCacheFontSize == general.ColorTextTooWideFontSize)
            {
                return _pixelWidthCacheValue;
            }

            var lines = GetStrippedLines();
            var maxWidth = 0;
            foreach (var line in lines)
            {
                var width = CalculatePixelWidth(line);
                if (width > maxWidth)
                {
                    maxWidth = width;
                }
            }

            _pixelWidthCacheText = Text;
            _pixelWidthCacheFontName = general.ColorTextTooWideFontName;
            _pixelWidthCacheFontSize = general.ColorTextTooWideFontSize;
            _pixelWidthCacheValue = maxWidth;
            return maxWidth;
        }
    }

    // Read-time memo for CPS/WPM: each value is read by its own column binding AND by the
    // Cps/Wpm/Duration background brushes during the same row repaint, and every computation
    // strips html tags and walks the text. Keyed on the exact inputs, so any Text or time
    // change simply recomputes on the next read - no invalidation wiring to get wrong.
    private string? _cpsCacheText;
    private TimeSpan _cpsCacheStart;
    private TimeSpan _cpsCacheEnd;
    private double _cpsCacheValue;

    private string? _wpmCacheText;
    private TimeSpan _wpmCacheStart;
    private TimeSpan _wpmCacheEnd;
    private double _wpmCacheValue;

    public double CharactersPerSecond
    {
        get
        {
            if (string.IsNullOrEmpty(Text))
            {
                return 0;
            }

            if (Duration.TotalMilliseconds <= 1.0)
            {
                return 999.0;
            }

            if (!ReferenceEquals(_cpsCacheText, Text) || _cpsCacheStart != StartTime || _cpsCacheEnd != EndTime)
            {
                _cpsCacheText = Text;
                _cpsCacheStart = StartTime;
                _cpsCacheEnd = EndTime;
                _cpsCacheValue = SubtitleTextInfoHelper.GetCharactersPerSecond(Text, StartTime, EndTime);
            }

            return _cpsCacheValue;
        }
    }

    public double WordsPerMinute // WPM
    {
        get
        {
            if (string.IsNullOrEmpty(Text))
            {
                return 0;
            }

            if (Duration.TotalMilliseconds <= 1.0)
            {
                return 999.0;
            }

            if (!ReferenceEquals(_wpmCacheText, Text) || _wpmCacheStart != StartTime || _wpmCacheEnd != EndTime)
            {
                _wpmCacheText = Text;
                _wpmCacheStart = StartTime;
                _wpmCacheEnd = EndTime;
                _wpmCacheValue = 60.0 / Duration.TotalSeconds * Text.CountWords();
            }

            return _wpmCacheValue;
        }
    }

    // Read-time memo for the text error highlight, same idea as the CPS/WPM memos above: the
    // grid re-reads this getter on every cell repaint, and each evaluation strips html, splits
    // into lines and (with ColorTextTooWide on) shapes every line with HarfBuzz. The key is the
    // text plus every setting the answer depends on, so a settings change simply recomputes on
    // the next read. Only the verdict is memoized, never the brush - the error brush instance is
    // replaced when the user picks another error color.
    private string? _textErrorCacheText;
    private TextErrorSettings _textErrorCacheSettings;
    private bool _textErrorCacheValue;

    private readonly record struct TextErrorSettings(
        bool ColorTextTooLong,
        int MaxLineLength,
        bool ColorTextTooWide,
        int MaxPixelWidth,
        string FontName,
        int FontSize,
        bool ColorTextTooManyLines,
        int MaxNumberOfLines,
        string? LengthStrategy)
    {
        public static TextErrorSettings Current()
        {
            var general = Se.Settings.General;
            return new TextErrorSettings(
                general.ColorTextTooLong,
                general.SubtitleLineMaximumLength,
                general.ColorTextTooWide,
                general.ColorTextTooWidePixels,
                general.ColorTextTooWideFontName,
                general.ColorTextTooWideFontSize,
                general.ColorTextTooManyLines,
                general.MaxNumberOfLines,
                // GetLineLength counts through this strategy, so it belongs in the key too.
                Configuration.Settings.General.CpsLineLengthStrategy);
        }
    }

    public IBrush TextBackgroundBrush => HasTextRuleError() ? _errorBrush : _transparentBrush;

    /// <summary>
    /// The memoized "text too long / too wide / too many lines" verdict behind
    /// <see cref="TextBackgroundBrush"/>. Also read by <see cref="AccessibleErrorText"/> and
    /// <see cref="HasErrors"/>, so scanning the file for errors twice (list errors, go to
    /// next error) never re-strips or re-shapes a line whose text is unchanged.
    /// </summary>
    private bool HasTextRuleError()
    {
        if (string.IsNullOrEmpty(Text))
        {
            return false;
        }

        var settings = TextErrorSettings.Current();
        if (!ReferenceEquals(_textErrorCacheText, Text) || !_textErrorCacheSettings.Equals(settings))
        {
            _textErrorCacheText = Text;
            _textErrorCacheSettings = settings;
            _textErrorCacheValue = HasTextError(settings);
        }

        return _textErrorCacheValue;
    }

    private bool HasTextError(TextErrorSettings settings)
    {
        // The stripped lines are memoized (GetStrippedLines), so the enabled branches share them.
        if (settings.ColorTextTooLong)
        {
            foreach (var line in GetStrippedLines())
            {
                if (SubtitleTextInfoHelper.GetLineLength(line) > settings.MaxLineLength)
                {
                    return true;
                }
            }
        }

        if (settings.ColorTextTooWide)
        {
            foreach (var line in GetStrippedLines())
            {
                if (CalculatePixelWidth(line) > settings.MaxPixelWidth)
                {
                    return true;
                }
            }
        }

        if (settings.ColorTextTooManyLines)
        {
            if (GetStrippedLines().Count > settings.MaxNumberOfLines)
            {
                return true;
            }
        }

        return false;
    }

    private static readonly Dictionary<(string name, int size), (SKFont font, SKShaper shaper)> _fontCache = [];
    private static bool _skipCalculatePixelWidth = false;

    private static int CalculatePixelWidth(string line)
    {
        if (string.IsNullOrEmpty(line) || _skipCalculatePixelWidth)
        {
            return 0;
        }

        try
        {
            var general = Se.Settings.General;
            var key = (name: general.ColorTextTooWideFontName, size: general.ColorTextTooWideFontSize);

            if (!_fontCache.TryGetValue(key, out var entry))
            {
                var typeface = SKTypeface.FromFamilyName(key.name) ?? SKTypeface.Default;
                entry = (new SKFont(typeface, key.size), new SKShaper(typeface));
                _fontCache[key] = entry;
            }

            var result = entry.shaper.Shape(line, entry.font);
            if (result.Points.Length == 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(result.Points.Last().X + entry.font.Size);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Error calculating pixel width for line: " + line);
            _skipCalculatePixelWidth = true;
            return 0;
        }
    }

    public IBrush DurationBackgroundBrush
    {
        get
        {
            var general = Se.Settings.General;
            if ((general.ColorDurationTooShort && Duration.TotalMilliseconds < general.SubtitleMinimumDisplayMilliseconds) ||
                (general.ColorDurationTooLong && Duration.TotalMilliseconds > general.SubtitleMaximumDisplayMilliseconds) ||
                // SE4 fallback: when the CPS column is hidden, surface CPS-too-high on the Duration cell instead
                ((!general.ShowColumnCps || !IsCpsColumnVisible) && general.ColorCharactersPerSecond && CharactersPerSecond > general.SubtitleMaximumCharactersPerSeconds))
            {
                return _errorBrush;
            }

            return _transparentBrush;
        }
    }

    public IBrush EndTimeBackgroundBrush =>
        Se.Settings.General.ColorTimeCodeOverlap && Gap < 0 ? _errorBrush : _transparentBrush;

    public IBrush StartTimeBackgroundBrush =>
        Se.Settings.General.ColorTimeCodeOverlap && PreviousGap < 0 ? _errorBrush : _transparentBrush;

    public IBrush CpsBackgroundBrush
    {
        get
        {
            if (Se.Settings.General.ColorCharactersPerSecond &&
                CharactersPerSecond > Se.Settings.General.SubtitleMaximumCharactersPerSeconds)
            {
                return _errorBrush;
            }

            return _transparentBrush;
        }
    }

    public IBrush WpmBackgroundBrush
    {
        get
        {
            if (Se.Settings.General.ColorWordsPerMinute &&
                WordsPerMinute > Se.Settings.General.SubtitleMaximumWordsPerMinute)
            {
                return _errorBrush;
            }

            return _transparentBrush;
        }
    }

    partial void OnGapChanged(double value)
    {
        if (_skipUpdate)
        {
            return;
        }

        OnPropertyChanged(nameof(GapBackgroundBrush));
        OnPropertyChanged(nameof(EndTimeBackgroundBrush));
        OnPropertyChanged(nameof(AccessibleErrorText));
    }

    partial void OnPreviousGapChanged(double value)
    {
        OnPropertyChanged(nameof(StartTimeBackgroundBrush));
        OnPropertyChanged(nameof(AccessibleErrorText));
    }

    public IBrush GapBackgroundBrush
    {
        get
        {
            if (Se.Settings.General.ColorGapTooShort &&
                Gap < Se.Settings.General.MinimumBetweenLines.GetMilliseconds())
            {
                return _errorBrush;
            }

            return _transparentBrush;
        }
    }

    /// <summary>
    /// Screen-reader text for the rule violations the grid shows as cell tints.
    /// The tints are color-only, so this mirrors the *BackgroundBrush conditions
    /// and is appended to the row's AutomationProperties.Name binding (empty when
    /// the line is clean). English on purpose, same as GetErrors/the error list.
    /// </summary>
    public string AccessibleErrorText
    {
        get
        {
            var general = Se.Settings.General;
            StringBuilder? errors = null;

            void Add(string error)
            {
                errors ??= new StringBuilder(" - ");
                if (errors.Length > 3)
                {
                    errors.Append(", ");
                }

                errors.Append(error);
            }

            if (general.ColorTimeCodeOverlap && PreviousGap < 0)
            {
                Add("overlap with previous");
            }

            if (general.ColorTimeCodeOverlap && Gap < 0)
            {
                Add("overlap with next");
            }
            else if (general.ColorGapTooShort && Gap < general.MinimumBetweenLines.GetMilliseconds())
            {
                Add("gap too short");
            }

            if (general.ColorDurationTooShort && Duration.TotalMilliseconds < general.SubtitleMinimumDisplayMilliseconds)
            {
                Add("duration too short");
            }

            if (general.ColorDurationTooLong && Duration.TotalMilliseconds > general.SubtitleMaximumDisplayMilliseconds)
            {
                Add("duration too long");
            }

            if (general.ColorCharactersPerSecond && CharactersPerSecond > general.SubtitleMaximumCharactersPerSeconds)
            {
                Add("CPS " + Math.Round(CharactersPerSecond, 1));
            }

            if (general.ColorWordsPerMinute && WordsPerMinute > general.SubtitleMaximumWordsPerMinute)
            {
                Add("WPM " + Math.Round(WordsPerMinute));
            }

            // Memoized by (Text, settings) - the same verdict the Text cell tint uses.
            if (HasTextRuleError())
            {
                Add("text too long or wide");
            }

            return errors?.ToString() ?? string.Empty;
        }
    }

    public TimeSpan StartTimeOnly
    {
        get => StartTime;
        set
        {
            if (StartTime == value)
            {
                return;
            }

            if (_skipUpdate)
            {
                return;
            }

            _skipUpdate = true;
            SetStartTimeOnly(value);
            _skipUpdate = false;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Binding target for the start-time editor in "keep duration" mode (used when
    /// no separate end-time editor is shown). Programmatic callers should use
    /// <see cref="SetStartTimeKeepDuration"/> so the side effect reads as an action.
    /// </summary>
    public TimeSpan StartTimeKeepDuration
    {
        get => StartTime;
        set
        {
            if (StartTime == value || _skipUpdate)
            {
                return;
            }

            SetStartTimeKeepDuration(value);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Sets the start time and shifts the end time by the same amount, preserving
    /// the line's duration ("move the whole line"). Plain <see cref="StartTime"/>
    /// assignment and <see cref="SetStartTimeOnly"/>, by contrast, keep the end fixed.
    /// </summary>
    internal void SetStartTimeKeepDuration(TimeSpan timeSpan)
    {
        // SetTimes applies start/end atomically (no transient start > end exposed
        // to the bound editor controls); the span comes from the live times.
        SetTimes(timeSpan, timeSpan + (EndTime - StartTime));
    }

    partial void OnStartTimeChanged(TimeSpan value)
    {
        OnPropertyChanged(nameof(StartTimeOnly));
        OnPropertyChanged(nameof(StartTimeKeepDuration));

        if (_skipUpdate)
        {
            return;
        }

        // Plain, safe default: move the start and recompute duration, leaving the
        // end time fixed. To move the whole line keeping its duration, assign
        // StartTimeKeepDuration instead.
        _skipUpdate = true;
        UpdateDuration();
        _skipUpdate = false;
    }

    partial void OnEndTimeChanged(TimeSpan value)
    {
        if (_skipUpdate)
        {
            return;
        }

        // UpdateDuration raises the timing-derived notifications (CPS/WPM/brushes);
        // raising them here as well made every EndTime assignment notify twice, and the
        // grid re-evaluates the bound getters per notification.
        _skipUpdate = true;
        UpdateDuration();
        _skipUpdate = false;
    }

    partial void OnDurationChanged(TimeSpan value)
    {
        if (_skipUpdate)
        {
            return;
        }

        var newEndTime = StartTime + value;
        if (Math.Abs(newEndTime.TotalMilliseconds - EndTime.TotalMilliseconds) > 0.001)
        {
            EndTime = newEndTime;

            OnPropertyChanged(nameof(CharactersPerSecond));
            OnPropertyChanged(nameof(DurationBackgroundBrush));
            OnPropertyChanged(nameof(CpsBackgroundBrush));
            OnPropertyChanged(nameof(WordsPerMinute));
            OnPropertyChanged(nameof(WpmBackgroundBrush));
            OnPropertyChanged(nameof(AccessibleErrorText));
        }
    }

    internal void UpdateDuration()
    {
        var newDuration = EndTime - StartTime;
        if (Math.Abs(newDuration.TotalMilliseconds - Duration.TotalMilliseconds) > 0.001)
        {
            Duration = EndTime - StartTime;

            // Single raise site for everything derived from the times. TextBackgroundBrush
            // is deliberately absent: it depends only on Text (see the getter), and its
            // getter shapes the text with HarfBuzz - raising it at waveform-drag rate is
            // expensive. CpsBackgroundBrush/WpmBackgroundBrush are raised here so start-time
            // drags repaint them too (previously only end-time changes did).
            OnPropertyChanged(nameof(CharactersPerSecond));
            OnPropertyChanged(nameof(WordsPerMinute));
            OnPropertyChanged(nameof(DurationBackgroundBrush));
            OnPropertyChanged(nameof(CpsBackgroundBrush));
            OnPropertyChanged(nameof(WpmBackgroundBrush));
            OnPropertyChanged(nameof(AccessibleErrorText));
        }
    }

    partial void OnTextChanged(string value)
    {
        OnPropertyChanged(nameof(CharactersPerSecond));
        OnPropertyChanged(nameof(TextBackgroundBrush));
        OnPropertyChanged(nameof(CpsBackgroundBrush));
        // DurationBackgroundBrush now also reacts to CPS-too-high, and text edits change
        // CPS (numerator), so the Duration cell must repaint on text changes too.
        OnPropertyChanged(nameof(DurationBackgroundBrush));
        OnPropertyChanged(nameof(WordsPerMinute));
        OnPropertyChanged(nameof(WpmBackgroundBrush));
        OnPropertyChanged(nameof(PixelWidth));
        OnPropertyChanged(nameof(AccessibleErrorText));
        // WebVTT keeps the cue classes and the voice inside the text, so those two columns
        // change with it.
        OnPropertyChanged(nameof(WebVttStyle));
        OnPropertyChanged(nameof(WebVttVoice));
    }

    public void RefreshText()
    {
        OnPropertyChanged(nameof(Text));
    }

    /// <summary>
    /// Removes trailing whitespace - typically an empty line left by pressing Enter at the
    /// end of the text - from <see cref="Text"/> and <see cref="OriginalText"/>. Called when
    /// the row loses selection, so the line count/CPS shown in the grid match what
    /// <see cref="ToParagraph"/> commits (#13389). Not safe to run while the row is still
    /// bound to the edit text box: the TwoWay binding would push the trimmed value back and
    /// delete a newline the user just typed.
    /// </summary>
    public void TrimTrailingTextWhitespace()
    {
        var trimmed = Text.TrimEnd();
        if (trimmed.Length != Text.Length)
        {
            Text = trimmed;
        }

        var trimmedOriginal = OriginalText.TrimEnd();
        if (trimmedOriginal.Length != OriginalText.Length)
        {
            OriginalText = trimmedOriginal;
        }
    }

    /// <summary>
    /// Raises change notifications for properties whose values depend on
    /// <see cref="Se.Settings"/> (CPS strategy, line-length limit, colour
    /// toggles, error colour, etc.). The grid's per-cell bindings cache the
    /// last value, so when a setting changes mid-session the rows keep
    /// showing stale CPS numbers and stale error highlights until something
    /// on the row itself changes. Call this once per row after
    /// <see cref="Se.Settings"/> is updated.
    /// </summary>
    public void RefreshAfterSettingsChanged()
    {
        OnPropertyChanged(nameof(CharactersPerSecond));
        OnPropertyChanged(nameof(WordsPerMinute));
        OnPropertyChanged(nameof(TextBackgroundBrush));
        OnPropertyChanged(nameof(DurationBackgroundBrush));
        OnPropertyChanged(nameof(CpsBackgroundBrush));
        OnPropertyChanged(nameof(WpmBackgroundBrush));
        OnPropertyChanged(nameof(GapBackgroundBrush));
        OnPropertyChanged(nameof(PixelWidth));
        OnPropertyChanged(nameof(AccessibleErrorText));

        // The grid Text/OriginalText columns render through a converter that honors the
        // "single line" + separator appearance settings, so re-notify them too; otherwise
        // toggling single-line (or applying the SE4 look) wouldn't refresh the grid live.
        OnPropertyChanged(nameof(Text));
        OnPropertyChanged(nameof(OriginalText));
    }

    /// <summary>Updates all display properties from a fixed <see cref="Paragraph"/> in-place.</summary>
    internal void UpdateFrom(Paragraph p, SubtitleFormat? subtitleFormat)
    {
        Paragraph = p;
        Text = p.Text;
        Actor = p.Actor;
        Style = p.Style;
        Layer = p.Layer;
        Extra = p.Extra;
        Language = p.Language;
        Region = p.Region;
        Effect = p.Effect;
        IsComment = p.IsComment;
        MarginL = p.MarginL;
        MarginR = p.MarginR;
        MarginV = p.MarginV;
        NewSection = p.NewSection;
        Forced = p.Forced;
        Bookmark = p.Bookmark;
        if (subtitleFormat is AdvancedSubStationAlpha or SubStationAlpha)
        {
            Style = p.Extra;
        }
        _skipUpdate = true;
        StartTime = TimeSpan.FromMilliseconds(p.StartTime.TotalMilliseconds);
        EndTime = TimeSpan.FromMilliseconds(p.EndTime.TotalMilliseconds);
        _skipUpdate = false;
        UpdateDuration();
    }

    /// <summary>Updates all display properties from another <see cref="SubtitleLineViewModel"/> in-place.</summary>
    internal void UpdateFrom(SubtitleLineViewModel src)
    {
        if (src.Paragraph != null)
        {
            Paragraph = src.Paragraph;
        }
        Text = src.Text;
        Actor = src.Actor;
        Style = src.Style;
        Layer = src.Layer;
        Extra = src.Extra;
        Language = src.Language;
        Region = src.Region;
        Effect = src.Effect;
        IsComment = src.IsComment;
        MarginL = src.MarginL;
        MarginR = src.MarginR;
        MarginV = src.MarginV;
        NewSection = src.NewSection;
        Forced = src.Forced;
        Bookmark = src.Bookmark;
        _skipUpdate = true;
        StartTime = src.StartTime;
        EndTime = src.EndTime;
        _skipUpdate = false;
        UpdateDuration();
    }

    internal void SetStartTimeOnly(TimeSpan timeSpan)
    {
        _skipUpdate = true;
        StartTime = timeSpan;
        _skipUpdate = false;

        UpdateDuration();
    }

    /// <summary>
    /// Sets start and end time together without ever publishing an invalid
    /// intermediate state (e.g. start &gt; end / negative duration). Updating
    /// the two times separately can briefly expose such a state to the live
    /// editor controls bound to the selected line; the duration up/down clamps
    /// the negative value and writes it back, corrupting the end time. Suppress
    /// notifications while both values are assigned, then recompute duration once.
    /// </summary>
    internal void SetTimes(TimeSpan startTime, TimeSpan endTime)
    {
        _skipUpdate = true;
        StartTime = startTime;
        EndTime = endTime;
        _skipUpdate = false;

        UpdateDuration();
    }

    internal void Adjust(double factor, double adjustmentInSeconds)
    {
        if (StartTime.IsMaxTime())
        {
            return;
        }

        // Set both times atomically via SetTimes; updating start then end
        // separately can briefly expose start > end to the bound editor
        // controls, which clamp the negative duration and corrupt the end time.
        var newStart = TimeSpan.FromMilliseconds(StartTime.TotalMilliseconds * factor + adjustmentInSeconds * TimeCode.BaseUnit);
        var newEnd = TimeSpan.FromMilliseconds(EndTime.TotalMilliseconds * factor + adjustmentInSeconds * TimeCode.BaseUnit);
        SetTimes(newStart, newEnd);
    }

    internal double GetCharactersPerSecond()
    {
        if (Duration.TotalMilliseconds < 1)
        {
            return 999;
        }

        return SubtitleTextInfoHelper.GetCharactersPerSecond(Text, StartTime, EndTime);
    }

    /// <summary>
    /// Whether <see cref="GetErrors"/> would report anything, without building the message.
    /// Same rules, but allocation-free and short-circuiting, and the text rules go through the
    /// memoized verdict - the error scans (list errors, go to next/previous error) only need
    /// the yes/no answer, and they ask it for every line of the file.
    /// </summary>
    public bool HasErrors(SubtitleLineViewModel? prev, SubtitleLineViewModel? next)
    {
        var general = Se.Settings.General;

        if (general.ColorCharactersPerSecond &&
            Math.Round(CharactersPerSecond, 2, MidpointRounding.AwayFromZero) > general.SubtitleMaximumCharactersPerSeconds)
        {
            return true;
        }

        var durMsRounded = Math.Round(Duration.TotalMilliseconds, 3, MidpointRounding.AwayFromZero);
        if (general.ColorDurationTooShort && durMsRounded < general.SubtitleMinimumDisplayMilliseconds)
        {
            return true;
        }

        if (general.ColorDurationTooLong && durMsRounded > general.SubtitleMaximumDisplayMilliseconds)
        {
            return true;
        }

        if (prev != null && HasGapError(general, (StartTime - prev.EndTime).TotalMilliseconds))
        {
            return true;
        }

        if (next != null && HasGapError(general, (next.StartTime - EndTime).TotalMilliseconds))
        {
            return true;
        }

        // Last, because these are the only rules that touch the text (and, with
        // "text too wide" on, shape every line with HarfBuzz).
        return HasTextRuleError();
    }

    private static bool HasGapError(SeGeneral general, double gapMs)
        => gapMs < 0
            ? general.ColorTimeCodeOverlap
            : general.ColorGapTooShort && gapMs < general.MinimumBetweenLines.GetMilliseconds();

    public string GetErrors(SubtitleLineViewModel? prev, SubtitleLineViewModel? next)
    {
        var errors = new StringBuilder();

        var general = Se.Settings.General;

        if (Se.Settings.General.ColorTextTooManyLines)
        {
            var lineCount = GetStrippedLines().Count;
            if (lineCount > general.MaxNumberOfLines)
            {
                errors.AppendLine("Max #lines: " + lineCount + " >" + general.MaxNumberOfLines);
            }
        }

        var cpsRounded = Math.Round(CharactersPerSecond, 2, MidpointRounding.AwayFromZero);
        if (cpsRounded > general.SubtitleMaximumCharactersPerSeconds && Se.Settings.General.ColorCharactersPerSecond)
        {
            errors.AppendLine("Cps: " + cpsRounded + " > " + general.SubtitleMaximumCharactersPerSeconds);
        }

        var durMsRounded = Math.Round(Duration.TotalMilliseconds, 3, MidpointRounding.AwayFromZero);
        if (durMsRounded < general.SubtitleMinimumDisplayMilliseconds)
        {
            if (Se.Settings.General.ColorDurationTooShort)
            {
                errors.AppendLine("Min duration: " + durMsRounded + " < " + general.SubtitleMinimumDisplayMilliseconds);
            }
        }
        if (durMsRounded > general.SubtitleMaximumDisplayMilliseconds)
        {
            if (Se.Settings.General.ColorDurationTooLong)
            {
                errors.AppendLine("Max duration: " + durMsRounded + " > " + general.SubtitleMaximumDisplayMilliseconds);
            }
        }

        if (Se.Settings.General.ColorTextTooLong)
        {
            foreach (var line in GetStrippedLines())
            {
                var lineLength = SubtitleTextInfoHelper.GetLineLength(line);
                if (lineLength > general.SubtitleLineMaximumLength)
                {
                    errors.AppendLine("Max line length: " + lineLength + " > " + general.SubtitleLineMaximumLength);
                }
            }
        }

        if (Se.Settings.General.ColorTextTooWide)
        {
            foreach (var line in GetStrippedLines())
            {
                var pixelWidth = CalculatePixelWidth(line);
                if (pixelWidth > general.ColorTextTooWidePixels)
                {
                    errors.AppendLine("Max width (px): " + pixelWidth + " > " + general.ColorTextTooWidePixels);
                }
            }
        }

        if (prev != null)
        {
            var gapPrev = (StartTime - prev.EndTime).TotalMilliseconds;
            if (gapPrev < 0)
            {
                if (Se.Settings.General.ColorTimeCodeOverlap)
                {
                    errors.AppendLine("Overlap from previous: " + Math.Round(-gapPrev, 3));
                }
            }
            else if (gapPrev < general.MinimumBetweenLines.GetMilliseconds())
            {
                if (Se.Settings.General.ColorGapTooShort)
                {
                    errors.AppendLine("Min gap to previous: " + Math.Round(gapPrev, 3) + " < " + general.MinimumBetweenLines.GetMilliseconds());
                }
            }
        }

        if (next == null)
        {
            return errors.ToString();
        }

        var gapNext = (next.StartTime - EndTime).TotalMilliseconds;
        if (gapNext < 0)
        {
            if (Se.Settings.General.ColorTimeCodeOverlap)
            {
                errors.AppendLine("Overlap to next: " + Math.Round(-gapNext, 3));
            }
        }
        else if (gapNext < general.MinimumBetweenLines.GetMilliseconds())
        {
            if (Se.Settings.General.ColorGapTooShort)
            {
                errors.AppendLine("Min gap to next: " + Math.Round(gapNext, 3) + " < " + general.MinimumBetweenLines.GetMilliseconds());
            }
        }

        return errors.ToString();
    }

    public void RefreshTimeCodes()
    {
        OnPropertyChanged(nameof(StartTime));
        OnPropertyChanged(nameof(EndTime));
        OnPropertyChanged(nameof(Duration));
    }
}
