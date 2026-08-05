using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Globalization;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// The value converters the subtitle grid runs per visible row on every repaint.
/// One iteration = one repaint of a 200-row viewport (the grid virtualizes, but a fast
/// scroll re-converts every recycled row, and the whole set is re-converted on sort,
/// selection paint, theme change and settings change).
/// </summary>
[MemoryDiagnoser]
public class GridCellConverterBenchmarks
{
    private const int Rows = 200;

    private static bool _avaloniaInitialized;

    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    private TimeSpanToDisplayFullConverter _fullTime = null!;
    private TimeSpanToDisplayShortConverter _shortTime = null!;
    private DoubleToDisplayShortConverter _gap = null!;
    private DoubleToOneDecimalHideMaxConverter _cpsWpm = null!;
    private TextToFlowDirectionConverter _flowDirection = null!;
    private DurationToBackgroundConverter _durationBackground = null!;

    private TimeSpan[] _startTimes = null!;
    private TimeSpan[] _durations = null!;
    private double[] _gaps = null!;
    private double[] _cps = null!;
    private string[] _texts = null!;

    [GlobalSetup]
    public void Setup()
    {
        EnsureAvalonia();

        _fullTime = new TimeSpanToDisplayFullConverter();
        _shortTime = new TimeSpanToDisplayShortConverter();
        _gap = new DoubleToDisplayShortConverter();
        _cpsWpm = new DoubleToOneDecimalHideMaxConverter();
        _flowDirection = new TextToFlowDirectionConverter();
        _durationBackground = new DurationToBackgroundConverter();

        _startTimes = new TimeSpan[Rows];
        _durations = new TimeSpan[Rows];
        _gaps = new double[Rows];
        _cps = new double[Rows];
        _texts = new string[Rows];

        var sentences = ConverterBenchmarkData.GridTexts;
        for (var i = 0; i < Rows; i++)
        {
            _startTimes[i] = TimeSpan.FromMilliseconds(i * 2400 + 137);
            _durations[i] = TimeSpan.FromMilliseconds(1500 + i % 900);
            // Every 10th row has the "no gap" sentinel the last-line cell uses.
            _gaps[i] = i % 10 == 0 ? double.MaxValue : 200 + i % 700;
            _cps[i] = i % 13 == 0 ? double.MaxValue : 8.5 + i % 11;
            _texts[i] = sentences[i % sentences.Length];
        }
    }

    internal static void EnsureAvalonia()
    {
        if (_avaloniaInitialized)
        {
            return;
        }

        _avaloniaInitialized = true;
        AppBuilder.Configure<Application>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .UseSkia()
            .WithInterFont()
            .SetupWithoutStarting();
    }

    /// <summary>Start and end time cells - two full time-code conversions per row.</summary>
    [Benchmark]
    public object FullTimeCells()
    {
        object last = null!;
        for (var i = 0; i < Rows; i++)
        {
            last = _fullTime.Convert(_startTimes[i], typeof(string), null, _culture);
            last = _fullTime.Convert(_startTimes[i] + _durations[i], typeof(string), null, _culture);
        }

        return last;
    }

    /// <summary>Duration cell.</summary>
    [Benchmark]
    public object ShortTimeCells()
    {
        object last = null!;
        for (var i = 0; i < Rows; i++)
        {
            last = _shortTime.Convert(_durations[i], typeof(string), null, _culture);
        }

        return last;
    }

    /// <summary>Gap column (double milliseconds, MaxValue = no gap).</summary>
    [Benchmark]
    public object GapCells()
    {
        object last = null!;
        for (var i = 0; i < Rows; i++)
        {
            last = _gap.Convert(_gaps[i], typeof(string), null, _culture);
        }

        return last;
    }

    /// <summary>CPS and WPM columns.</summary>
    [Benchmark]
    public object CpsWpmCells()
    {
        object last = null!;
        for (var i = 0; i < Rows; i++)
        {
            last = _cpsWpm.Convert(_cps[i], typeof(string), null, _culture);
            last = _cpsWpm.Convert(_cps[i] * 7, typeof(string), null, _culture);
        }

        return last;
    }

    /// <summary>Text and original-text flow direction.</summary>
    [Benchmark]
    public object FlowDirectionCells()
    {
        object last = null!;
        for (var i = 0; i < Rows; i++)
        {
            last = _flowDirection.Convert(_texts[i], typeof(object), null, _culture);
        }

        return last;
    }

    /// <summary>Duration cell background brush.</summary>
    [Benchmark]
    public object DurationBackgroundCells()
    {
        object last = null!;
        for (var i = 0; i < Rows; i++)
        {
            last = _durationBackground.Convert(_durations[i], typeof(object), null, _culture);
        }

        return last;
    }
}

/// <summary>
/// The syntax highlighting converter: the most expensive per-row work in the grid, run for
/// the text cell and (when shown) the original-text cell of every visible row.
/// </summary>
[MemoryDiagnoser]
public class SyntaxHighlightingConverterBenchmarks
{
    private const int Rows = 200;

    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    private TextWithSubtitleSyntaxHighlightingConverter _converter = null!;
    private string[] _texts = null!;

    [GlobalSetup]
    public void Setup()
    {
        GridCellConverterBenchmarks.EnsureAvalonia();
        _converter = new TextWithSubtitleSyntaxHighlightingConverter();

        var sentences = ConverterBenchmarkData.GridTexts;
        _texts = new string[Rows];
        for (var i = 0; i < Rows; i++)
        {
            _texts[i] = sentences[i % sentences.Length];
        }
    }

    private object RunAll()
    {
        object last = null!;
        for (var i = 0; i < Rows; i++)
        {
            last = _converter.Convert(_texts[i], typeof(InlineCollection), null, _culture);
        }

        return last;
    }

    [Benchmark]
    public object NoFormatting()
    {
        Se.Settings.Appearance.SubtitleGridFormattingType = (int)SubtitleGridFormattingTypes.NoFormatting;
        return RunAll();
    }

    [Benchmark(Baseline = true)]
    public object ShowFormatting()
    {
        Se.Settings.Appearance.SubtitleGridFormattingType = (int)SubtitleGridFormattingTypes.ShowFormatting;
        return RunAll();
    }

    [Benchmark]
    public object ShowTags()
    {
        Se.Settings.Appearance.SubtitleGridFormattingType = (int)SubtitleGridFormattingTypes.ShowTags;
        return RunAll();
    }
}

/// <summary>
/// The small converters. Each is trivial on its own, but the grid runs a handful per row and
/// every one of them boxes its result for the binding system.
/// </summary>
[MemoryDiagnoser]
public class SmallConverterBenchmarks
{
    private const int Rows = 200;

    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    private InverseBooleanConverter _inverseBoolean = null!;
    private NotNullConverter _notNull = null!;
    private NullToOpacityConverter _nullToOpacity = null!;
    private BooleanToGridLengthConverter _booleanToGridLength = null!;
    private BoolToFontStyleConverter _boolToFontStyle = null!;
    private BooleanToCheckMarkConverter _booleanToCheckMark = null!;
    private BooleanAndConverter _booleanAnd = null!;
    private ColorToBrushConverter _colorToBrush = null!;
    private MiddleEllipsisConverter _middleEllipsis = null!;
    private TextOneLineShortConverter _textOneLineShort = null!;
    private BatchConvertStatusColorConverter _statusColor = null!;

    private object?[] _bookmarks = null!;
    private object?[] _booleans = null!;
    private object?[] _booleanPairs = null!;
    private string[] _paths = null!;
    private string[] _statuses = null!;

    [GlobalSetup]
    public void Setup()
    {
        GridCellConverterBenchmarks.EnsureAvalonia();

        _inverseBoolean = new InverseBooleanConverter();
        _notNull = new NotNullConverter();
        _nullToOpacity = new NullToOpacityConverter();
        _booleanToGridLength = new BooleanToGridLengthConverter();
        _boolToFontStyle = new BoolToFontStyleConverter();
        _booleanToCheckMark = new BooleanToCheckMarkConverter();
        _booleanAnd = BooleanAndConverter.Instance;
        _colorToBrush = new ColorToBrushConverter();
        _middleEllipsis = new MiddleEllipsisConverter();
        _textOneLineShort = new TextOneLineShortConverter();
        _statusColor = new BatchConvertStatusColorConverter();

        _bookmarks = new object?[Rows];
        _booleans = new object?[Rows];
        _paths = new string[Rows];
        _statuses = new string[Rows];
        for (var i = 0; i < Rows; i++)
        {
            _bookmarks[i] = i % 7 == 0 ? "A bookmark comment" : null;
            _booleans[i] = i % 3 != 0;
            _paths[i] = $"/Users/someone/Movies/season 3/episode {i:D2}/the.long.file.name.S03E{i:D2}.1080p.WEB-DL.mkv";
            _statuses[i] = ConverterBenchmarkData.BatchStatuses[i % ConverterBenchmarkData.BatchStatuses.Length];
        }

        _booleanPairs = new object?[] { true, true, false };
    }

    /// <summary>The row's IsHidden / Bookmark / column-visibility bindings.</summary>
    [Benchmark]
    public object BooleanAndNullConverters()
    {
        object last = null!;
        for (var i = 0; i < Rows; i++)
        {
            last = _inverseBoolean.Convert(_booleans[i], typeof(bool), null, _culture);
            last = _notNull.Convert(_bookmarks[i], typeof(bool), null, _culture);
            last = _nullToOpacity.Convert(_bookmarks[i], typeof(double), null, _culture);
            last = _booleanToGridLength.Convert(_booleans[i], typeof(object), null, _culture);
            last = _boolToFontStyle.Convert(_booleans[i], typeof(object), null, _culture);
            last = _booleanToCheckMark.Convert(_booleans[i], typeof(string), null, _culture);
        }

        return last;
    }

    [Benchmark]
    public object? BooleanAnd()
    {
        object? last = null;
        for (var i = 0; i < Rows; i++)
        {
            last = _booleanAnd.Convert(_booleanPairs, typeof(bool), null, _culture);
        }

        return last;
    }

    [Benchmark]
    public object? ColorToBrush()
    {
        object? last = null;
        for (var i = 0; i < Rows; i++)
        {
            last = _colorToBrush.Convert(ConverterBenchmarkData.Colors[i % ConverterBenchmarkData.Colors.Length], typeof(object), null, _culture);
        }

        return last;
    }

    [Benchmark]
    public object MiddleEllipsis()
    {
        object last = null!;
        for (var i = 0; i < Rows; i++)
        {
            last = _middleEllipsis.Convert(_paths[i], typeof(string), "60", _culture);
        }

        return last;
    }

    [Benchmark]
    public object TextOneLineShort()
    {
        object last = null!;
        for (var i = 0; i < Rows; i++)
        {
            last = _textOneLineShort.Convert(ConverterBenchmarkData.GridTexts[i % ConverterBenchmarkData.GridTexts.Length], typeof(string), null, _culture);
        }

        return last;
    }

    /// <summary>Batch convert's status column - foreground and background per row.</summary>
    [Benchmark]
    public object? BatchConvertStatusColor()
    {
        object? last = null;
        for (var i = 0; i < Rows; i++)
        {
            last = _statusColor.Convert(_statuses[i], typeof(object), "background", _culture);
            last = _statusColor.Convert(_statuses[i], typeof(object), null, _culture);
        }

        return last;
    }
}

/// <summary>
/// Why the converters stopped handing out <see cref="SolidColorBrush"/>: it is an AvaloniaObject
/// with a property store behind it, for a value that never changes after construction.
/// </summary>
[MemoryDiagnoser]
public class BrushCreationBenchmarks
{
    private const int Count = 200;

    private static readonly Dictionary<Color, SolidColorBrush> Cache = new();

    [GlobalSetup]
    public void Setup() => GridCellConverterBenchmarks.EnsureAvalonia();

    [Benchmark(Baseline = true)]
    public object SolidColorBrushPerCall()
    {
        object last = null!;
        for (var i = 0; i < Count; i++)
        {
            last = new SolidColorBrush(ConverterBenchmarkData.Colors[i % ConverterBenchmarkData.Colors.Length]);
        }

        return last;
    }

    [Benchmark]
    public object ImmutableSolidColorBrushPerCall()
    {
        object last = null!;
        for (var i = 0; i < Count; i++)
        {
            last = new ImmutableSolidColorBrush(ConverterBenchmarkData.Colors[i % ConverterBenchmarkData.Colors.Length]);
        }

        return last;
    }

    [Benchmark]
    public object CachedSolidColorBrush()
    {
        object last = null!;
        for (var i = 0; i < Count; i++)
        {
            var color = ConverterBenchmarkData.Colors[i % ConverterBenchmarkData.Colors.Length];
            if (!Cache.TryGetValue(color, out var brush))
            {
                brush = new SolidColorBrush(color);
                Cache[color] = brush;
            }

            last = brush;
        }

        return last;
    }
}

internal static class ConverterBenchmarkData
{
    /// <summary>
    /// Subtitle lines with the mix a real file has: plain text, two-liners, italics,
    /// font tags, ASSA override tags and one right-to-left line.
    /// </summary>
    internal static readonly string[] GridTexts =
    {
        "It was the best of times, it was the worst of times.",
        "Are you coming with us?",
        "- No." + "\r\n" + "- Then stay here and wait.",
        "<i>I told you already, this is not going to work out</i>" + "\r\n" + "<i>the way you think it will.</i>",
        "Hello.",
        "<font color=\"#ff8800\" face=\"Arial\" size=\"20\">Somewhere in Denmark</font>",
        "{\\an8}Chapter one",
        "{\\i1\\c&H00FF00&\\fnArial\\fs28}Do you have any idea{\\i0}" + "\r\n" + "what you have done?",
        "مرحبا بالعالم",
        "He said: <b>\"Never again.\"</b>" + "\r\n" + "And he meant it.",
    };

    internal static readonly string[] BatchStatuses =
    {
        "-",
        "Converted",
        "Error: file not found",
        "OCR 42%",
        "Cancelled",
        "-",
    };

    internal static readonly Avalonia.Media.Color[] Colors =
    {
        Avalonia.Media.Color.FromRgb(255, 255, 255),
        Avalonia.Media.Color.FromRgb(255, 200, 0),
        Avalonia.Media.Color.FromRgb(0, 128, 255),
        Avalonia.Media.Color.FromRgb(255, 255, 255),
    };
}
