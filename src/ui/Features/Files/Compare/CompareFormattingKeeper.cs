using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

/// <summary>
/// "Take text, keep formatting" (#16823): the reference's plain text inside the current line's
/// own formatting. A typical use is an nOCR file with the right italics but wrong letters, fixed
/// from an OCR of the same images that reads better but has no italics.
/// <para>
/// Only the tags that wrap the whole line move over - leading ASSA override blocks like
/// {\an8}{\i1}, trailing ones like {\i0}, and HTML tags like &lt;i&gt;...&lt;/i&gt; that open at
/// the start and close at the end. Tags inside the line cannot be placed once the words differ,
/// so they are dropped along with the reference's own tags.
/// </para>
/// </summary>
public static class CompareFormattingKeeper
{
    private static readonly string[] HtmlTagNames = { "i", "b", "u", "font" };

    public static string TakeText(string current, string reference)
    {
        var plain = HtmlUtil.RemoveHtmlTags(reference ?? string.Empty, alsoSsaTags: true).Trim();
        if (string.IsNullOrEmpty(current))
        {
            return plain;
        }

        var leading = new List<string>();
        var start = 0;
        while (start < current.Length)
        {
            var tag = ReadTagAt(current, start);
            if (tag == null || !(IsAssaBlock(tag) || IsHtmlOpenTag(tag)))
            {
                break;
            }

            leading.Add(tag);
            start += tag.Length;
        }

        var trailing = new List<string>();
        var end = current.Length;
        while (end > start)
        {
            var tag = ReadTagEndingAt(current, end);
            if (tag == null || end - tag.Length < start || !(IsAssaBlock(tag) || IsHtmlCloseTag(tag)))
            {
                break;
            }

            trailing.Insert(0, tag);
            end -= tag.Length;
        }

        var inner = current.Substring(start, end - start);
        var prefix = new List<string>();
        var suffix = new List<string>();
        var unmatchedClosers = new List<string>(trailing);

        foreach (var tag in leading)
        {
            if (IsAssaBlock(tag))
            {
                prefix.Add(tag);
                continue;
            }

            // An HTML tag is a wrapper only if it closes at the very end and is not closed
            // somewhere inside first - "<i>A</i> b <i>C</i>" is not an italic line.
            var name = GetHtmlTagName(tag);
            var closerIndex = unmatchedClosers.FindIndex(p => IsHtmlCloseTag(p) && GetHtmlTagName(p) == name);
            if (closerIndex >= 0 && IsBalanced(inner, name))
            {
                prefix.Add(tag);
                unmatchedClosers.RemoveAt(closerIndex);
            }
        }

        foreach (var tag in trailing)
        {
            if (IsAssaBlock(tag) || !unmatchedClosers.Contains(tag))
            {
                suffix.Add(tag);
            }
            else
            {
                unmatchedClosers.Remove(tag);
            }
        }

        return string.Concat(prefix) + plain + string.Concat(suffix);
    }

    private static string? ReadTagAt(string s, int index)
    {
        var close = s[index] switch
        {
            '{' => '}',
            '<' => '>',
            _ => '\0',
        };

        if (close == '\0')
        {
            return null;
        }

        var endIndex = s.IndexOf(close, index + 1);
        return endIndex < 0 ? null : s.Substring(index, endIndex - index + 1);
    }

    private static string? ReadTagEndingAt(string s, int end)
    {
        var last = s[end - 1];
        var open = last switch
        {
            '}' => '{',
            '>' => '<',
            _ => '\0',
        };

        if (open == '\0')
        {
            return null;
        }

        var startIndex = s.LastIndexOf(open, end - 1);
        return startIndex < 0 ? null : s.Substring(startIndex, end - startIndex);
    }

    private static bool IsAssaBlock(string tag) => tag.StartsWith("{\\", StringComparison.Ordinal) && tag.EndsWith('}');

    private static bool IsHtmlOpenTag(string tag)
    {
        return tag.StartsWith('<') && !tag.StartsWith("</", StringComparison.Ordinal) && Array.IndexOf(HtmlTagNames, GetHtmlTagName(tag)) >= 0;
    }

    private static bool IsHtmlCloseTag(string tag)
    {
        return tag.StartsWith("</", StringComparison.Ordinal) && Array.IndexOf(HtmlTagNames, GetHtmlTagName(tag)) >= 0;
    }

    private static string GetHtmlTagName(string tag)
    {
        var s = tag.TrimStart('<', '/');
        var endIndex = s.IndexOfAny(new[] { ' ', '>', '\t' });
        return (endIndex < 0 ? s : s.Substring(0, endIndex)).ToLowerInvariant();
    }

    private static bool IsBalanced(string inner, string name)
    {
        // "<i>One</i>\n<i>Two</i>" is an italic line too: closing and reopening at a line
        // break does not end the wrapper.
        inner = Regex.Replace(inner, "</" + name + @">\s*\n\s*<" + name + @"(\s[^>]*)?>", "\n", RegexOptions.IgnoreCase);
        var depth = 0;
        var index = 0;
        while ((index = inner.IndexOf('<', index)) >= 0)
        {
            var tag = ReadTagAt(inner, index);
            if (tag == null)
            {
                break;
            }

            if (IsHtmlOpenTag(tag) && GetHtmlTagName(tag) == name)
            {
                depth++;
            }
            else if (IsHtmlCloseTag(tag) && GetHtmlTagName(tag) == name && --depth < 0)
            {
                return false;
            }

            index += tag.Length;
        }

        return true;
    }
}
