using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.ErrorList;

/// <summary>The error classes the grid colours and "List errors" reports (Settings &gt; General).</summary>
public enum LineErrorType
{
    TooManyLines,
    CharactersPerSecond,
    DurationTooShort,
    DurationTooLong,
    LineTooLong,
    LineTooWide,
    Overlap,
    GapTooShort,
}

/// <summary>One error on one line: the class plus a short detail such as "27.3 > 25".</summary>
public record LineError(LineErrorType Type, string Detail)
{
    public string Label => GetLabel(Type);

    /// <summary>"Reading speed: 27.3 > 25" - the old single-string form, still used by the batch error list.</summary>
    public override string ToString() => $"{Label}: {Detail}";

    public static string GetLabel(LineErrorType type)
    {
        var l = Se.Language.ErrorList;
        return type switch
        {
            LineErrorType.TooManyLines => l.TooManyLines,
            LineErrorType.CharactersPerSecond => l.CharactersPerSecond,
            LineErrorType.DurationTooShort => l.DurationTooShort,
            LineErrorType.DurationTooLong => l.DurationTooLong,
            LineErrorType.LineTooLong => l.LineTooLong,
            LineErrorType.LineTooWide => l.LineTooWide,
            LineErrorType.Overlap => l.Overlap,
            LineErrorType.GapTooShort => l.GapTooShort,
            _ => type.ToString(),
        };
    }

    public static string GetHint(LineErrorType type)
    {
        var l = Se.Language.ErrorList;
        return type switch
        {
            LineErrorType.TooManyLines => l.TooManyLinesHint,
            LineErrorType.CharactersPerSecond => l.CharactersPerSecondHint,
            LineErrorType.DurationTooShort => l.DurationTooShortHint,
            LineErrorType.DurationTooLong => l.DurationTooLongHint,
            LineErrorType.LineTooLong => l.LineTooLongHint,
            LineErrorType.LineTooWide => l.LineTooWideHint,
            LineErrorType.Overlap => l.OverlapHint,
            LineErrorType.GapTooShort => l.GapTooShortHint,
            _ => string.Empty,
        };
    }

    private static readonly IBrush TooManyLinesBrush = new SolidColorBrush(Color.Parse("#E84393"));
    private static readonly IBrush CpsBrush = new SolidColorBrush(Color.Parse("#E8912D"));
    private static readonly IBrush TooShortBrush = new SolidColorBrush(Color.Parse("#F1C40F"));
    private static readonly IBrush TooLongBrush = new SolidColorBrush(Color.Parse("#9B59B6"));
    private static readonly IBrush LineTooLongBrush = new SolidColorBrush(Color.Parse("#3498DB"));
    private static readonly IBrush LineTooWideBrush = new SolidColorBrush(Color.Parse("#2980B9"));
    private static readonly IBrush OverlapBrush = new SolidColorBrush(Color.Parse("#E74C3C"));
    private static readonly IBrush GapBrush = new SolidColorBrush(Color.Parse("#1ABC9C"));
    public static readonly IBrush AllBrush = new SolidColorBrush(Color.Parse("#5D8AA8"));

    public static IBrush GetBrush(LineErrorType type)
    {
        return type switch
        {
            LineErrorType.TooManyLines => TooManyLinesBrush,
            LineErrorType.CharactersPerSecond => CpsBrush,
            LineErrorType.DurationTooShort => TooShortBrush,
            LineErrorType.DurationTooLong => TooLongBrush,
            LineErrorType.LineTooLong => LineTooLongBrush,
            LineErrorType.LineTooWide => LineTooWideBrush,
            LineErrorType.Overlap => OverlapBrush,
            LineErrorType.GapTooShort => GapBrush,
            _ => AllBrush,
        };
    }
}
