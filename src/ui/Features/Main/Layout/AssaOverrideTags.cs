using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

/// <summary>
/// The override tags of the block a line starts with ("{\an8\pos(640,80)}Text"), split into
/// single tags for the tag chips of layout 14. Only the leading block is handled: that is the
/// block that styles the whole line, and the one the chips edit.
/// </summary>
internal static class AssaOverrideTags
{
    /// <summary>
    /// The tags of the leading override block, each with its backslash ("\pos(640,80)"). A tag's
    /// parentheses may hold more tags (\t(0,500,\fs40)), so a backslash inside them does not
    /// start a new tag. Empty when the text does not start with a block.
    /// </summary>
    internal static List<string> GetLeadingTags(string? text)
    {
        return TryParseLeadingBlock(text, out _, out var tags, out _) ? tags : [];
    }

    /// <summary>
    /// The text with one tag of the leading block removed. A block left empty is removed with its
    /// braces; a block that also holds a comment (text before the first backslash) keeps it.
    /// </summary>
    internal static string RemoveLeadingTag(string text, int index)
    {
        if (!TryParseLeadingBlock(text, out var blockLength, out var tags, out var comment) || index < 0 || index >= tags.Count)
        {
            return text;
        }

        tags.RemoveAt(index);
        var rest = text.Substring(blockLength);
        if (tags.Count == 0 && comment.Length == 0)
        {
            return rest;
        }

        return "{" + comment + string.Concat(tags) + "}" + rest;
    }

    private static bool TryParseLeadingBlock(string? text, out int blockLength, out List<string> tags, out string comment)
    {
        blockLength = 0;
        tags = [];
        comment = string.Empty;
        if (string.IsNullOrEmpty(text) || text[0] != '{')
        {
            return false;
        }

        var end = text.IndexOf('}');
        if (end < 0)
        {
            return false;
        }

        blockLength = end + 1;
        var content = text.Substring(1, end - 1);
        var firstTag = content.IndexOf('\\');
        if (firstTag < 0)
        {
            comment = content;
            return true;
        }

        comment = content.Substring(0, firstTag);
        var current = new StringBuilder();
        var depth = 0;
        for (var i = firstTag; i < content.Length; i++)
        {
            var c = content[i];
            if (c == '\\' && depth == 0 && current.Length > 0)
            {
                tags.Add(current.ToString());
                current.Clear();
            }

            if (c == '(')
            {
                depth++;
            }
            else if (c == ')' && depth > 0)
            {
                depth--;
            }

            current.Append(c);
        }

        if (current.Length > 0)
        {
            tags.Add(current.ToString());
        }

        return true;
    }
}
