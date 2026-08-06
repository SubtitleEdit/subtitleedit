using Avalonia.Controls;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

/// <summary>
/// Pre-boxed results for the converters that only ever return one of a handful of values.
/// A converter returns <see cref="object"/>, so every <c>return true</c> / <c>return 1.0</c> /
/// <c>return FlowDirection.RightToLeft</c> otherwise boxes - one small allocation per bound
/// property per row, on every grid repaint.
/// </summary>
internal static class ConverterBoxes
{
    internal static readonly object True = true;
    internal static readonly object False = false;

    internal static readonly object ZeroDouble = 0.0;
    internal static readonly object OneDouble = 1.0;

    internal static readonly object StarGridLength = new GridLength(1, GridUnitType.Star);
    internal static readonly object ZeroGridLength = new GridLength(0);

    internal static readonly object FontStyleNormal = FontStyle.Normal;
    internal static readonly object FontStyleItalic = FontStyle.Italic;

    internal static readonly object FlowDirectionLeftToRight = FlowDirection.LeftToRight;
    internal static readonly object FlowDirectionRightToLeft = FlowDirection.RightToLeft;

    internal static object Bool(bool value) => value ? True : False;
}
