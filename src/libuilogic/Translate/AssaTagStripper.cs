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
public readonly record struct StrippedLine(string Prefix, string Text, string Suffix)
{
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

    public string Restore(string newText)
    {
        return Prefix + newText + Suffix;
    }

    /// <summary>Removes every override block - for read-only context the model must not edit.</summary>
    public static string RemoveAllBlocks(string? input)
    {
        return AnyBlock.Replace(input ?? string.Empty, string.Empty);
    }
}
