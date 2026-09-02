using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

/// <summary>
/// From 1 to 10, numbers should be written out: one, two, three, etc.
/// </summary>
public class NetflixCheckNumbersOneToTenSpellOut : INetflixQualityChecker
{
    private static readonly Regex NumberOneToNine = new Regex(@"\b\d\b", RegexOptions.Compiled);
    private static readonly Regex NumberTen = new Regex(@"\b10\b", RegexOptions.Compiled);

    public string Name { get; set; }

    public NetflixCheckNumbersOneToTenSpellOut(string name)
    {
        Name = name;
    }

    public void Check(Subtitle subtitle, NetflixQualityController controller)
    {
        if (controller.Language == "ja" || controller.Language == "ar")
        {
            return;
        }

        foreach (var p in subtitle.Paragraphs)
        {
            string newText = p.Text;
            var m = NumberOneToNine.Match(newText);
            while (m.Success)
            {
                var ok = ShouldSpellOut(newText, m.Index, m.Length);

                if (ok)
                {
                    newText = newText.Remove(m.Index, 1).Insert(m.Index, NetflixHelper.ConvertNumberToString(m.Value.Substring(0, 1), false, controller.Language));
                }

                m = NumberOneToNine.Match(newText, m.Index + 1);
            }

            m = NumberTen.Match(newText);
            while (m.Success)
            {
                var ok = ShouldSpellOut(newText, m.Index, m.Length);

                if (ok)
                {
                    newText = newText.Remove(m.Index, 2).Insert(m.Index, NetflixHelper.ConvertNumberToString(m.Value, false, controller.Language));
                }

                m = NumberTen.Match(newText, m.Index + 1);
            }

            if (newText != p.Text)
            {
                var fixedParagraph = new Paragraph(p, false) { Text = newText };
                string comment = Se.Language.Tools.NetflixCheckAndFix.NumbersOneToTenSpellOut;
                controller.AddRecord(p, fixedParagraph, comment, string.Empty, true);
            }
        }
    }

    /// <summary>
    /// Whether the number matched at <paramref name="index"/> should be written out. Shared by
    /// both loops above: the 1-9 pass and the 10 pass used to make this decision separately, and
    /// the 10 pass only ever looked for a colon - so "Chapter 10. Introduction" became
    /// "Chapter ten. Introduction" while "Chapter 3. Introduction" was correctly left alone.
    /// </summary>
    private static bool ShouldSpellOut(string text, int index, int length)
    {
        var after = index + length;

        // A ':' or '.' next to the number means a time code, a decimal or a numbered heading -
        // unless the number ends the sentence, where the period belongs to the sentence.
        var ok = after >= text.Length || !IsColonOrPeriod(text[after]);
        if (!ok && IsSentenceEnding(text.Substring(after)))
        {
            ok = true;
        }

        // A comma followed by a digit is a thousands separator ("1,000"). Only the character
        // right after THIS comma decides that: the old test searched the whole rest of the line
        // for ",<digit>", so a later "4,000" stopped an earlier "3," being written out.
        if (ok && after < text.Length && text[after] == ',' &&
            after + 1 < text.Length && char.IsDigit(text[after + 1]))
        {
            ok = false;
        }

        if (ok && index > 0 && IsColonOrPeriod(text[index - 1]))
        {
            ok = false;
        }

        return ok;
    }

    private static bool IsSentenceEnding(string rest)
    {
        foreach (var ending in new[] { ".", "?", "!" })
        {
            if (rest == ending ||
                rest == ending + "</i>" ||
                rest == ending + Environment.NewLine ||
                rest == ending + "</i>" + Environment.NewLine)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Replaces <c>":.".Contains(c.ToString())</c>: the old shape boxed every neighbour
    /// character into a one-char string before searching a two-character literal.
    /// </summary>
    private static bool IsColonOrPeriod(char c) => c == ':' || c == '.';
}
