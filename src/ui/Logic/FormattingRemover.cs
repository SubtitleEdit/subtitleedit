using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// "Remove all formatting" for a whole subtitle text.
/// </summary>
public static class FormattingRemover
{
    /// <summary>
    /// Removes HTML and ASSA tags. \N used as padding at the start/end (e.g. from Surround with \N
    /// to lift a subtitle) would only leave empty lines behind, so leading and trailing empty lines
    /// are dropped (#15531). Empty lines between text are kept - they may be an intended gap.
    /// </summary>
    public static string RemoveAll(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var s = HtmlUtil.RemoveHtmlTags(text, true);
        var lines = s.SplitToLines();
        var first = 0;
        while (first < lines.Count && string.IsNullOrWhiteSpace(lines[first]))
        {
            first++;
        }

        var last = lines.Count - 1;
        while (last >= first && string.IsNullOrWhiteSpace(lines[last]))
        {
            last--;
        }

        if (first == 0 && last == lines.Count - 1)
        {
            return s;
        }

        return string.Join(Environment.NewLine, lines.Skip(first).Take(last - first + 1));
    }
}
