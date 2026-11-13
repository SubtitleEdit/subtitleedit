using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.UiLogic.Translate;

/// <summary>
/// Keeps ASSA override blocks ("{\pos(946.5,250.8)\fs54\1c&amp;HFDF9AA&amp;}") away from the model
/// (#13927): leading and trailing blocks are cut off before a line is sent and glued back on
/// verbatim around the model's answer. They carry no meaning for proofreading, cost a lot of
/// tokens, and small models "normalize" them so often that the tag guard used to drop the
/// whole correction. Blocks in the middle of the text are left in place - they are rare, and
/// the guard still verifies them.
/// </summary>
public readonly record struct StrippedLine(string Prefix, string Text, string Suffix, bool HasWrappers = false)
{
    private static readonly Regex WholeLineTag = new Regex(@"^<([ibu])>(.*)</\1>$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private static readonly Regex LeadingMusic = new Regex(@"^[♪♫]+\s*", RegexOptions.Compiled);
    private static readonly Regex TrailingMusic = new Regex(@"\s*[♪♫]+$", RegexOptions.Compiled);
    private static readonly Regex LeadingBlocks = new Regex(@"^(\s*\{\\[^}]*\})+", RegexOptions.Compiled);
    private static readonly Regex TrailingBlocks = new Regex(@"(\{\\[^}]*\}\s*)+$", RegexOptions.Compiled);
    private static readonly Regex AnyBlock = new Regex(@"\{\\[^}]*\}", RegexOptions.Compiled);
    private static readonly Regex DrawingMode = new Regex(@"\\p[1-9]", RegexOptions.Compiled);

    public static StrippedLine Strip(string? input)
    {
        var text = input ?? string.Empty;
        var prefix = string.Empty;
        var suffix = string.Empty;

        var lead = LeadingBlocks.Match(text);
        if (lead.Success)
        {
            if (DrawingMode.IsMatch(lead.Value))
            {
                // vector drawing ("{\p1}m 0 0 l 100 0 ...") - the "text" is shape commands, not words
                return new StrippedLine(text, string.Empty, string.Empty);
            }

            prefix = lead.Value;
            text = text.Substring(lead.Length);
        }

        var trail = TrailingBlocks.Match(text);
        if (trail.Success)
        {
            suffix = trail.Value;
            text = text.Substring(0, trail.Index);
        }

        // keep the whitespace next to the blocks with the blocks, so the restored line is byte-identical
        var trimmedStart = text.TrimStart();
        prefix += text.Substring(0, text.Length - trimmedStart.Length);
        var trimmed = trimmedStart.TrimEnd();
        suffix = trimmedStart.Substring(trimmed.Length) + suffix;

        return new StrippedLine(prefix, trimmed, suffix);
    }

    /// <summary>
    /// <see cref="Strip"/> plus the markup that wraps the whole line: an italic/bold/underline
    /// tag around all of it ("&lt;i&gt;...&lt;/i&gt;") and leading/trailing music symbols ("♪ ... ♪").
    /// Local translation models drop such wrappers often (#16869: &lt;i&gt; lost in 11 of 20
    /// italic lines), and since they apply to the whole line they can be put back around any
    /// translation. Tags that cover only part of the line stay in the text for the model.
    /// </summary>
    public static StrippedLine StripForTranslation(string? input)
    {
        var stripped = Strip(input);
        var peeled = PeelWrappers(stripped.Text);
        if (peeled.Text.Length == 0 || peeled.Text.Length == stripped.Text.Length)
        {
            return stripped; // nothing wrapped, or only markup (nothing to translate without it)
        }

        return new StrippedLine(stripped.Prefix + peeled.Prefix, peeled.Text, peeled.Suffix + stripped.Suffix, HasWrappers: true);
    }

    public string Restore(string newText)
    {
        if (HasWrappers)
        {
            // The model kept (some of) the wrappers anyway - drop its copy so ours are not doubled.
            var peeled = PeelWrappers(newText);
            if (peeled.Text.Length > 0)
            {
                newText = peeled.Text;
            }
        }

        return Prefix + newText + Suffix;
    }

    /// <summary>Peels whole-line tags and music symbols (in any nesting order) off trimmed text.</summary>
    private static StrippedLine PeelWrappers(string text)
    {
        var prefix = string.Empty;
        var suffix = string.Empty;
        text = text.Trim();
        while (true)
        {
            var tag = WholeLineTag.Match(text);
            if (tag.Success && IsSingleWrap(tag.Groups[2].Value, tag.Groups[1].Value))
            {
                prefix += text.Substring(0, tag.Groups[2].Index);
                suffix = text.Substring(tag.Groups[2].Index + tag.Groups[2].Length) + suffix;
                text = tag.Groups[2].Value;
            }
            else
            {
                var lead = LeadingMusic.Match(text);
                var trail = TrailingMusic.Match(text);
                if (!lead.Success && !trail.Success)
                {
                    break;
                }

                if (lead.Success)
                {
                    prefix += lead.Value;
                    text = text.Substring(lead.Length);
                }

                trail = TrailingMusic.Match(text);
                if (trail.Success)
                {
                    suffix = trail.Value + suffix;
                    text = text.Substring(0, trail.Index);
                }
            }

            // keep inner whitespace with the wrappers so an untouched line restores byte-identical
            var trimmedStart = text.TrimStart();
            prefix += text.Substring(0, text.Length - trimmedStart.Length);
            var trimmed = trimmedStart.TrimEnd();
            suffix = trimmedStart.Substring(trimmed.Length) + suffix;
            text = trimmed;
            if (text.Length == 0)
            {
                break;
            }
        }

        return new StrippedLine(prefix, text, suffix);
    }

    /// <summary>
    /// True when "&lt;i&gt;" + <paramref name="inner"/> + "&lt;/i&gt;" is one wrap, not
    /// "&lt;i&gt;One&lt;/i&gt; and &lt;i&gt;two&lt;/i&gt;" (the same tag closes and reopens inside).
    /// </summary>
    private static bool IsSingleWrap(string inner, string tag)
    {
        var depth = 0;
        foreach (Match m in Regex.Matches(inner, "<(/?)" + tag + ">", RegexOptions.IgnoreCase))
        {
            depth += m.Groups[1].Length == 0 ? 1 : -1;
            if (depth < 0)
            {
                return false;
            }
        }

        return depth == 0;
    }

    /// <summary>Removes every override block - for read-only context the model must not edit.</summary>
    public static string RemoveAllBlocks(string? input)
    {
        return AnyBlock.Replace(input ?? string.Empty, string.Empty);
    }
}
