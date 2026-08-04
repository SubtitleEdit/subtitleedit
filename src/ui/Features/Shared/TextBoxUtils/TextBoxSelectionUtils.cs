using System;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

public static class TextBoxSelectionUtils
{
    /// <summary>
    /// Moves leading/trailing white-space (spaces and new-lines) of a selection outside the
    /// tags/symbols to be added, so " 'word'" becomes " &lt;i&gt;'word'&lt;/i&gt;" instead of
    /// "&lt;i&gt; 'word'&lt;/i&gt;".
    /// </summary>
    public static string SplitOuterWhiteSpace(string selectedText, out string pre, out string post)
    {
        pre = string.Empty;
        post = string.Empty;

        while (selectedText.EndsWith(' ') || selectedText.EndsWith(Environment.NewLine, StringComparison.Ordinal) ||
               selectedText.StartsWith(' ') || selectedText.StartsWith(Environment.NewLine, StringComparison.Ordinal))
        {
            if (selectedText.EndsWith(' '))
            {
                post = " " + post;
                selectedText = selectedText.Remove(selectedText.Length - 1);
            }

            if (selectedText.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                post = Environment.NewLine + post;
                selectedText = selectedText.Remove(selectedText.Length - Environment.NewLine.Length);
            }

            if (selectedText.StartsWith(' '))
            {
                pre += " ";
                selectedText = selectedText.Remove(0, 1);
            }

            if (selectedText.StartsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                pre += Environment.NewLine;
                selectedText = selectedText.Remove(0, Environment.NewLine.Length);
            }
        }

        return selectedText;
    }
}
