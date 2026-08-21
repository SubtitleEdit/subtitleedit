using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Best-effort repair of slightly-malformed JSON emitted by external command-line tools so the
/// strict <see cref="System.Text.Json"/> parser can read it.
/// </summary>
public static class JsonRepair
{
    /// <summary>
    /// Escapes raw control characters (&lt; U+0020) that appear inside JSON string literals.
    /// Strict JSON requires these to be escaped (e.g. a literal newline must be <c>\n</c>); some
    /// tools — e.g. qwen3-asr-cli — write the raw character, which makes
    /// <c>System.Text.Json</c> throw "'0x0A' is invalid within a JSON string". Characters outside
    /// string literals (structural whitespace) are left untouched, so valid JSON is returned
    /// unchanged.
    /// </summary>
    public static string EscapeControlCharsInStrings(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return json;
        }

        var sb = new StringBuilder(json.Length + 16);
        var inString = false;
        var escaped = false;

        foreach (var c in json)
        {
            if (!inString)
            {
                sb.Append(c);
                if (c == '"')
                {
                    inString = true;
                }

                continue;
            }

            if (escaped)
            {
                // Previous char was a backslash; this char is part of an escape sequence — emit verbatim.
                sb.Append(c);
                escaped = false;
                continue;
            }

            switch (c)
            {
                case '\\':
                    sb.Append(c);
                    escaped = true;
                    break;
                case '"':
                    sb.Append(c);
                    inString = false;
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                case '\b':
                    sb.Append("\\b");
                    break;
                case '\f':
                    sb.Append("\\f");
                    break;
                default:
                    if (c < 0x20)
                    {
                        sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        sb.Append(c);
                    }

                    break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Replaces comma decimal separators in numbers outside string literals with dots, e.g.
    /// <c>"start": 1,840</c> becomes <c>"start": 1.840</c>. qwen3-asr-cli up to v0.1.7 formatted
    /// timestamps with the process locale, so on Windows with a comma-decimal regional format
    /// (French, German, ...) the JSON was invalid. Only a comma directly between two digits is
    /// rewritten — a structural comma in well-formed output is always followed by whitespace or
    /// a quote. NOTE: this makes the repair unsafe for JSON with arrays of bare numbers
    /// (<c>[1,2]</c>); the qwen3 output has none.
    /// </summary>
    public static string FixCommaDecimalSeparators(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return json;
        }

        var chars = json.ToCharArray();
        var inString = false;
        var escaped = false;

        for (var i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                }
                else if (c == '\\')
                {
                    escaped = true;
                }
                else if (c == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (c == '"')
            {
                inString = true;
            }
            else if (c == ',' && i > 0 && i + 1 < chars.Length &&
                     char.IsAsciiDigit(chars[i - 1]) && char.IsAsciiDigit(chars[i + 1]))
            {
                chars[i] = '.';
            }
        }

        return new string(chars);
    }
}
