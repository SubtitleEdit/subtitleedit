using System.Globalization;

namespace SeConv.Core;

/// <summary>
/// Parses <c>WIDTHxHEIGHT</c> strings (e.g. <c>1920x1080</c>) used by <c>--resolution</c>.
/// Both lowercase <c>x</c> and uppercase <c>X</c> are accepted.
/// </summary>
internal static class ResolutionParser
{
    public static (int Width, int Height) Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new FormatException("Resolution is empty.");
        }

        var s = input.Trim();
        var idx = s.IndexOfAny(['x', 'X']);
        if (idx <= 0 || idx == s.Length - 1)
        {
            throw new FormatException($"Resolution '{input}' is invalid. Expected WIDTHxHEIGHT, e.g. 1920x1080.");
        }

        var wPart = s[..idx];
        var hPart = s[(idx + 1)..];

        if (!int.TryParse(wPart, NumberStyles.None, CultureInfo.InvariantCulture, out var w) ||
            !int.TryParse(hPart, NumberStyles.None, CultureInfo.InvariantCulture, out var h) ||
            w <= 0 || h <= 0)
        {
            throw new FormatException($"Resolution '{input}' is invalid. Width and height must be positive integers.");
        }

        return (w, h);
    }
}
