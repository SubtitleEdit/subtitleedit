using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

/// <summary>
/// When a number begins a sentence, it should always be spelled out.
/// </summary>
public class NetflixCheckStartNumberSpellOut : INetflixQualityChecker
{
    // The digits are captured so the replacement can use the group's own index/length - the
    // offsets used to be counted off the whole match, which broke as soon as the separator was
    // not exactly the assumed width. The line break alternation matters off Windows, where
    // paragraph text is separated by "\n" and the old "\r\n" pattern never matched at all.
    private static readonly Regex NumberStart = new Regex(@"^(\d+) [A-Za-z]", RegexOptions.Compiled);
    private static readonly Regex NumberStartInside = new Regex(@"[\.,!] (\d+) [A-Za-z]", RegexOptions.Compiled);
    private static readonly Regex NumberStartInside2 = new Regex(@"[\.,!](?:\r\n|\n|\r)(\d+) [A-Za-z]", RegexOptions.Compiled);

    public string Name { get; set; }

    public NetflixCheckStartNumberSpellOut(string name)
    {
        Name = name;
    }

    public void Check(Subtitle subtitle, NetflixQualityController controller)
    {
        foreach (var p in subtitle.Paragraphs)
        {
            var newText = p.Text;

            newText = SpellOutNumbers(NumberStart, newText, controller.Language);
            newText = SpellOutNumbers(NumberStartInside, newText, controller.Language);
            newText = SpellOutNumbers(NumberStartInside2, newText, controller.Language);

            if (newText != p.Text)
            {
                var fixedParagraph = new Paragraph(p, false) { Text = newText };
                var comment = Se.Language.Tools.NetflixCheckAndFix.StartNumberSpellOut;
                controller.AddRecord(p, fixedParagraph, comment, string.Empty, true);
            }
        }
    }

    private static string SpellOutNumbers(Regex regex, string text, string language)
    {
        var m = regex.Match(text);
        while (m.Success)
        {
            var digits = m.Groups[1];
            text = text.Remove(digits.Index, digits.Length)
                       .Insert(digits.Index, NetflixHelper.ConvertNumberToString(digits.Value, true, language));
            m = regex.Match(text, m.Index + 1);
        }

        return text;
    }
}
