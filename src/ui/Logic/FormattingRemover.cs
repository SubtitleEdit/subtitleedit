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
    /// Removes HTML and ASSA tags. A \N between text stays a line break, but \N used as padding
    /// (at the start/end or several in a row, e.g. to lift a subtitle) would only leave empty lines
    /// behind, so empty lines are dropped (#15531).
    /// </summary>
    public static string RemoveAll(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var s = HtmlUtil.RemoveHtmlTags(text, true);
        var lines = s.SplitToLines();
        if (!lines.Any(string.IsNullOrWhiteSpace))
        {
            return s;
        }

        return string.Join(Environment.NewLine, lines.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}
