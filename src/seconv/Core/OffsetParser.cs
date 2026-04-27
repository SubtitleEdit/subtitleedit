using System.Globalization;

namespace SeConv.Core;

/// <summary>
/// Parses offset strings in the legacy Subtitle Edit CLI format. Accepts
/// a plain integer (milliseconds) or colon/comma/period separated integer
/// tokens treated as (hh, mm, ss, ms) — 4 tokens = hh:mm:ss:ms,
/// 3 = mm:ss:ms, 2 = ss:ms, 1 = ms. Optional leading +/- repeated.
/// </summary>
internal static class OffsetParser
{
    public static TimeSpan Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new FormatException("Offset is empty.");
        }

        var s = input.Trim();
        var negate = false;
        while (s.Length > 0 && (s[0] == '-' || s[0] == '+'))
        {
            if (s[0] == '-')
            {
                negate = !negate;
            }
            s = s[1..];
        }

        if (s.Length == 0)
        {
            throw Bad(input);
        }

        if (TryParseInt(s, out var ms))
        {
            var result = TimeSpan.FromMilliseconds(ms);
            return negate ? result.Negate() : result;
        }

        var parts = s.Split([':', ',', '.'], StringSplitOptions.RemoveEmptyEntries).ToList();
        if (parts.Count is < 1 or > 4)
        {
            throw Bad(input);
        }

        var total = TimeSpan.Zero;
        if (parts.Count == 4)
        {
            if (!TryParseInt(parts[0], out var hh)) throw Bad(input);
            total = total.Add(TimeSpan.FromHours(hh));
            parts.RemoveAt(0);
        }
        if (parts.Count == 3)
        {
            if (!TryParseInt(parts[0], out var mm)) throw Bad(input);
            total = total.Add(TimeSpan.FromMinutes(mm));
            parts.RemoveAt(0);
        }
        if (parts.Count == 2)
        {
            if (!TryParseInt(parts[0], out var ss)) throw Bad(input);
            total = total.Add(TimeSpan.FromSeconds(ss));
            parts.RemoveAt(0);
        }
        if (parts.Count == 1)
        {
            if (!TryParseInt(parts[0], out var msPart)) throw Bad(input);
            total = total.Add(TimeSpan.FromMilliseconds(msPart));
        }

        return negate ? total.Negate() : total;
    }

    private static bool TryParseInt(string s, out int value)
        => int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out value);

    private static FormatException Bad(string input)
        => new($"The offset value '{input}' is invalid.");
}
