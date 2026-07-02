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
        Text = p.Text;
        OriginalText = p.OriginalText;
        StartTime = p.StartTime;
        EndTime = p.EndTime;
        UpdateDuration();
        Language = p.Language;
        Region = p.Region;
        Style = p.Style;
        Actor = p.Actor;
        Layer = p.Layer;
        Number = p.Number;
        Extra = p.Extra;
        Effect = p.Effect;
        IsComment = p.IsComment;
        MarginL = p.MarginL;
        MarginR = p.MarginR;
        MarginV = p.MarginV;
        NewSection = p.NewSection;
        Forced = p.Forced;
        Bookmark = p.Bookmark;

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
        Id = Guid.TryParse(paragraph.Id, out var guid) ? guid : Guid.NewGuid();
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
            Text = Text,
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
            Text = OriginalText,
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

    public int PixelWidth
    {
        get
        {
            if (!Se.Settings.General.ShowColumnPixelWidth && !Se.Settings.General.ColorTextTooWide)
            {
                return 0;
            }

            var text = HtmlUtil.RemoveHtmlTags(Text, true);
            var lines = text.SplitToLines();
            var maxWidth = 0;
            foreach (var line in lines)
            {
                var width = CalculatePixelWidth(line);
                if (width > maxWidth)
                {
                    maxWidth = width;
                }
            }

            return maxWidth;
        }
    }

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

            return SubtitleTextInfoHelper.GetCharactersPerSecond(Text, StartTime, EndTime);
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

            return 60.0 / Duration.TotalSeconds * Text.CountWords();
        }
    }

    public IBrush TextBackgroundBrush
    {
        get
        {
            if (string.IsNullOrEmpty(Text))
            {
                return _transparentBrush;
            }

            // Avalonia re-evaluates this getter on every cell repaint; strip HTML once
            // and reuse across all settings-enabled branches below.
            string? stripped = null;

            if (Se.Settings.General.ColorTextTooLong)
            {
                stripped = SubtitleTextInfoHelper.StripHtml(Text);

                foreach (var line in stripped.SplitToLines())
                {
                    if (SubtitleTextInfoHelper.GetLineLength(line) > Se.Settings.General.SubtitleLineMaximumLength)
                    {
                        return _errorBrush;
                    }
                }
            }

            if (Se.Settings.General.ColorTextTooWide)
            {
                stripped ??= SubtitleTextInfoHelper.StripHtml(Text);
                foreach (var line in stripped.SplitToLines())
                {
                    if (CalculatePixelWidth(line) > Se.Settings.General.ColorTextTooWidePixels)
                    {
                        return _errorBrush;
                    }
                }
            }

            if (Se.Settings.General.ColorTextTooManyLines)
            {
                stripped ??= SubtitleTextInfoHelper.StripHtml(Text);
                if (stripped.SplitToLines().Count > Se.Settings.General.MaxNumberOfLines)
                {
                    return _errorBrush;
                }
            }

            return _transparentBrush;
        }
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
    }

    partial void OnPreviousGapChanged(double value)
    {
        OnPropertyChanged(nameof(StartTimeBackgroundBrush));
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
    }

    public void RefreshText()
    {
        OnPropertyChanged(nameof(Text));
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

    public string GetErrors(SubtitleLineViewModel? prev, SubtitleLineViewModel? next)
    {
        var errors = new StringBuilder();

        var general = Se.Settings.General;
        var strippedText = SubtitleTextInfoHelper.StripHtml(Text);
        var lines = strippedText.SplitToLines();
        var lineCount = lines.Count;

        if (lineCount > general.MaxNumberOfLines && Se.Settings.General.ColorTextTooManyLines)
        {
            errors.AppendLine("Max #lines: " + lineCount + " >" + general.MaxNumberOfLines);
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
            foreach (var line in lines)
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
            foreach (var line in lines)
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
