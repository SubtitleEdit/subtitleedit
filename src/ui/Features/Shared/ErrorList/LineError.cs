using Avalonia.Media;
using Avalonia.Media.Immutable;
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
    GapInRange,
    CustomRule,
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
            LineErrorType.DurationTooShort => Se.Language.General.TooShort,
            LineErrorType.DurationTooLong => Se.Language.General.TooLong,
            LineErrorType.LineTooLong => l.LineTooLong,
            LineErrorType.LineTooWide => l.LineTooWide,
            LineErrorType.Overlap => Se.Language.General.Overlapping,
            LineErrorType.GapTooShort => l.GapTooShort,
            LineErrorType.GapInRange => l.GapInRange,
            LineErrorType.CustomRule => l.CustomRule,
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
            LineErrorType.GapInRange => l.GapInRangeHint,
            LineErrorType.CustomRule => l.CustomRuleHint,
            _ => string.Empty,
        };
    }

    private const string TooManyLinesColor = "#E84393";
    private const string CpsColor = "#E8912D";
    private const string TooShortColor = "#F1C40F";
    private const string TooLongColor = "#9B59B6";
    private const string LineTooLongColor = "#3498DB";
    private const string LineTooWideColor = "#2980B9";
    private const string OverlapColor = "#E74C3C";
    private const string GapColor = "#1ABC9C";
    private const string CustomRuleColor = "#FFA500";
    public const string AllColor = "#5D8AA8";

    private static readonly IBrush TooManyLinesBrush = new ImmutableSolidColorBrush(Color.Parse(TooManyLinesColor));
    private static readonly IBrush CpsBrush = new ImmutableSolidColorBrush(Color.Parse(CpsColor));
    private static readonly IBrush TooShortBrush = new ImmutableSolidColorBrush(Color.Parse(TooShortColor));
    private static readonly IBrush TooLongBrush = new ImmutableSolidColorBrush(Color.Parse(TooLongColor));
    private static readonly IBrush LineTooLongBrush = new ImmutableSolidColorBrush(Color.Parse(LineTooLongColor));
    private static readonly IBrush LineTooWideBrush = new ImmutableSolidColorBrush(Color.Parse(LineTooWideColor));
    private static readonly IBrush OverlapBrush = new ImmutableSolidColorBrush(Color.Parse(OverlapColor));
    private static readonly IBrush GapBrush = new ImmutableSolidColorBrush(Color.Parse(GapColor));
    private static readonly IBrush CustomRuleBrush = new ImmutableSolidColorBrush(Color.Parse(CustomRuleColor));
    public static readonly IBrush AllBrush = new ImmutableSolidColorBrush(Color.Parse(AllColor));

    /// <summary>The dot colour as "#RRGGBB" - the html export paints the same palette as the window.</summary>
    public static string GetColor(LineErrorType type)
    {
        return type switch
        {
            LineErrorType.TooManyLines => TooManyLinesColor,
            LineErrorType.CharactersPerSecond => CpsColor,
            LineErrorType.DurationTooShort => TooShortColor,
            LineErrorType.DurationTooLong => TooLongColor,
            LineErrorType.LineTooLong => LineTooLongColor,
            LineErrorType.LineTooWide => LineTooWideColor,
            LineErrorType.Overlap => OverlapColor,
            LineErrorType.GapTooShort or LineErrorType.GapInRange => GapColor,
            LineErrorType.CustomRule => CustomRuleColor,
            _ => AllColor,
        };
    }

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
            LineErrorType.GapTooShort or LineErrorType.GapInRange => GapBrush,
            LineErrorType.CustomRule => CustomRuleBrush,
            _ => AllBrush,
        };
    }
}
