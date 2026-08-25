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

    // Was constructed inside the per-match loop below, so every "3," in the file paid for a
    // fresh Regex parse. Compiled once here instead.
    private static readonly Regex CommaDigit = new Regex(@",\d", RegexOptions.Compiled);

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
                bool ok = newText.Length <= m.Index + 1 || newText.Length > m.Index + 1 && !IsColonOrPeriod(newText[m.Index + 1]);
                if (!ok && newText.Length > m.Index + 1)
                {
                    var rest = newText.Substring(m.Index + m.Length);
                    if (rest == "." || rest == "?" || rest == "!" ||
                        rest == ".</i>" || rest == "?</i>" || rest == "!</i>" ||
                        rest == "." + Environment.NewLine || rest == "?" + Environment.NewLine || rest == "!" + Environment.NewLine ||
                        rest == ".</i>" + Environment.NewLine || rest == "?</i>" + Environment.NewLine || rest == "!</i>" + Environment.NewLine)
                    {
                        ok = true;
                    }
                }

                if (ok && m.Index + m.Length < newText.Length && newText[m.Index + m.Length] == ',')
                {
                    if (CommaDigit.IsMatch(newText, m.Index + 1))
                    {
                        ok = false;
                    }
                }

                if (ok && m.Index > 0 && IsColonOrPeriod(newText[m.Index - 1]))
                {
                    ok = false;
                }

                if (ok)
                {
                    newText = newText.Remove(m.Index, 1).Insert(m.Index, NetflixHelper.ConvertNumberToString(m.Value.Substring(0, 1), false, controller.Language));
                }

                m = NumberOneToNine.Match(newText, m.Index + 1);
            }

            m = NumberTen.Match(newText);
            while (m.Success)
            {
                bool ok = newText.Length <= m.Index + 2 || newText.Length > m.Index + 2 && newText[m.Index + 2] != ':';
                if (ok && m.Index > 0 && IsColonOrPeriod(newText[m.Index - 1]))
                {
                    ok = false;
                }

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
    /// Replaces <c>":.".Contains(c.ToString())</c>: the old shape boxed every neighbour
    /// character into a one-char string before searching a two-character literal.
    /// </summary>
    private static bool IsColonOrPeriod(char c) => c == ':' || c == '.';
}
