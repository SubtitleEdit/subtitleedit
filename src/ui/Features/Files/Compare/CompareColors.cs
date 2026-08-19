using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

/// <summary>
/// The three highlight colors of the compare view - "only in one file", "text or time
/// difference" and "number difference" - in one place, so the row backgrounds, the color
/// legend and the exported HTML page cannot drift apart.
///
/// The light pastels were the only palette for a long time, which left them unreadable in
/// the dark theme: a near-white row background under the dark theme's near-white text
/// (issue #13435). Each color now has a dark counterpart in the same family as the run
/// highlighting of <see cref="TextDiffHighlighter"/>.
/// </summary>
internal static class CompareColors
{
    private const byte RowAlpha = 180;

    // The light palette doubles as the export palette: the generated HTML page has a white
    // background and black text, so it must keep the pastels whatever theme is active.
    internal static readonly Color OnlyInOneFileLight = Color.FromRgb(255, 235, 233);
    internal static readonly Color TextOrTimeDifferenceLight = Color.FromRgb(230, 255, 237);
    internal static readonly Color NumberDifferenceLight = Color.FromRgb(255, 248, 220);

    private static readonly Color OnlyInOneFileDark = Color.FromRgb(90, 35, 35);
    private static readonly Color TextOrTimeDifferenceDark = Color.FromRgb(35, 75, 40);
    private static readonly Color NumberDifferenceDark = Color.FromRgb(85, 70, 25);

    private static readonly IBrush OnlyInOneFileRowLight = MakeRowBrush(OnlyInOneFileLight);
    private static readonly IBrush TextOrTimeDifferenceRowLight = MakeRowBrush(TextOrTimeDifferenceLight);
    private static readonly IBrush NumberDifferenceRowLight = MakeRowBrush(NumberDifferenceLight);

    private static readonly IBrush OnlyInOneFileRowDark = MakeRowBrush(OnlyInOneFileDark);
    private static readonly IBrush TextOrTimeDifferenceRowDark = MakeRowBrush(TextOrTimeDifferenceDark);
    private static readonly IBrush NumberDifferenceRowDark = MakeRowBrush(NumberDifferenceDark);

    /// <summary>Opaque color for the legend swatches.</summary>
    internal static Color OnlyInOneFile => UiTheme.IsDarkThemeEnabled() ? OnlyInOneFileDark : OnlyInOneFileLight;

    internal static Color TextOrTimeDifference => UiTheme.IsDarkThemeEnabled() ? TextOrTimeDifferenceDark : TextOrTimeDifferenceLight;

    internal static Color NumberDifference => UiTheme.IsDarkThemeEnabled() ? NumberDifferenceDark : NumberDifferenceLight;

    /// <summary>Row background brush; the same instance per theme, so brushes stay reference comparable.</summary>
    internal static IBrush OnlyInOneFileRow => UiTheme.IsDarkThemeEnabled() ? OnlyInOneFileRowDark : OnlyInOneFileRowLight;

    internal static IBrush TextOrTimeDifferenceRow => UiTheme.IsDarkThemeEnabled() ? TextOrTimeDifferenceRowDark : TextOrTimeDifferenceRowLight;

    internal static IBrush NumberDifferenceRow => UiTheme.IsDarkThemeEnabled() ? NumberDifferenceRowDark : NumberDifferenceRowLight;

    /// <summary>
    /// Light-palette color for a row brush, or null when the cell is not highlighted. The
    /// exported page is always light, so a dark-theme brush must export as its light twin -
    /// otherwise the export carries dark cells behind the page's black text.
    /// </summary>
    internal static Color? GetExportColor(IBrush? brush)
    {
        if (ReferenceEquals(brush, OnlyInOneFileRowLight) || ReferenceEquals(brush, OnlyInOneFileRowDark))
        {
            return OnlyInOneFileLight;
        }

        if (ReferenceEquals(brush, TextOrTimeDifferenceRowLight) || ReferenceEquals(brush, TextOrTimeDifferenceRowDark))
        {
            return TextOrTimeDifferenceLight;
        }

        if (ReferenceEquals(brush, NumberDifferenceRowLight) || ReferenceEquals(brush, NumberDifferenceRowDark))
        {
            return NumberDifferenceLight;
        }

        return null;
    }

    private static IBrush MakeRowBrush(Color color)
        => new SolidColorBrush(Color.FromArgb(RowAlpha, color.R, color.G, color.B));
}
