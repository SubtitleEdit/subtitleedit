namespace Nikse.SubtitleEdit.UiLogic.Grammar;

/// <summary>A replacement for the text at [<see cref="Offset"/>, <see cref="Offset"/> + <see cref="Length"/>).</summary>
public readonly struct LanguageToolFixItem
{
    public LanguageToolFixItem(int offset, int length, string replacement)
    {
        Offset = offset;
        Length = length;
        Replacement = replacement ?? string.Empty;
    }

    public int Offset { get; }
    public int Length { get; }
    public string Replacement { get; }
}

public static class LanguageToolFix
{
    /// <summary>
    /// Applies replacements to one line. Right to left, so the offsets of the fixes still waiting stay
    /// valid, and fixes overlapping one already applied are skipped rather than corrupting the line.
    /// </summary>
    public static string Apply(string text, IEnumerable<LanguageToolFixItem> fixes)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text ?? string.Empty;
        }

        var ordered = fixes
            .Where(f => f.Offset >= 0 && f.Length > 0 && f.Offset + f.Length <= text.Length)
            .OrderByDescending(f => f.Offset)
            .ThenByDescending(f => f.Length)
            .ToList();

        var result = text;
        var appliedStart = text.Length;
        foreach (var item in ordered)
        {
            if (item.Offset + item.Length > appliedStart)
            {
                continue;
            }

            result = result.Remove(item.Offset, item.Length).Insert(item.Offset, item.Replacement);
            appliedStart = item.Offset;
        }

        return result;
    }
}
