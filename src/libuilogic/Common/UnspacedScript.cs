using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.Common;

/// <summary>
/// Scripts written without spaces between words (Chinese, Japanese, Thai...). Joining two lines
/// of such text must not put a space at the break - that space would be a new character in the
/// text, not a restored layout. Korean is written with spaces, so it is not included.
/// </summary>
public static class UnspacedScript
{
    public static bool IsUnspacedScript(char c)
    {
        // Escapes, not literals: an editor or tool that normalizes text turns U+F900 into U+8C48.
        return c is >= '\u0E00' and <= '\u0EFF' // Thai, Lao
            or >= '\u1000' and <= '\u109F' // Myanmar
            or >= '\u1780' and <= '\u17FF' // Khmer
            or >= '\u2E80' and <= '\u2FDF' // CJK radicals
            or >= '\u3000' and <= '\u30FF' // CJK punctuation, Hiragana, Katakana
            or >= '\u3100' and <= '\u31FF' // Bopomofo, Katakana extensions
            or >= '\u3400' and <= '\u4DBF' // CJK extension A
            or >= '\u4E00' and <= '\u9FFF' // CJK unified ideographs
            or >= '\uF900' and <= '\uFAFF' // CJK compatibility ideographs
            or >= '\uFF00' and <= '\uFFEF'; // full-width and half-width forms
    }

    /// <summary>True when a space between <paramref name="previous"/> and <paramref name="next"/> belongs there.</summary>
    public static bool NeedsSpaceBetween(char previous, char next)
    {
        return !IsUnspacedScript(previous) && !IsUnspacedScript(next);
    }

    /// <summary>
    /// Joins lines with a space, except where the break sits next to a script written without
    /// spaces between words.
    /// </summary>
    public static string JoinLines(IReadOnlyList<string> lines)
    {
        if (lines.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder(lines[0]);
        for (var i = 1; i < lines.Count; i++)
        {
            var previous = sb.Length > 0 ? sb[sb.Length - 1] : ' ';
            var next = lines[i].Length > 0 ? lines[i][0] : ' ';
            if (NeedsSpaceBetween(previous, next))
            {
                sb.Append(' ');
            }

            sb.Append(lines[i]);
        }

        return sb.ToString();
    }
}
