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
}
