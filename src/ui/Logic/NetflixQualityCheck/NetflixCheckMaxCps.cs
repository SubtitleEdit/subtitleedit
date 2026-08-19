using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

/// <summary>
/// Reading speed - depends on the language.
/// </summary>
public class NetflixCheckMaxCps : INetflixQualityChecker
{
    public string Name { get; set; }

    public NetflixCheckMaxCps(string name)
    {
        Name = name;
    }

    public void Check(Subtitle subtitle, NetflixQualityController controller)
    {
        ICalcLength calc = CalcFactory.MakeCalculator(nameof(CalcAll));
        var charactersPerSecond = controller.CharactersPerSecond;
        var comment = string.Format(Se.Language.Tools.NetflixCheckAndFix.MaximumXCharactersPerSecond, charactersPerSecond);
        foreach (var p in subtitle.Paragraphs)
        {
            var jp = new Paragraph(p);
            if (controller.Language == "ja")
            {
                jp.Text = HtmlUtil.RemoveHtmlTags(jp.Text, true);
                jp.Text = NetflixImsc11Japanese.RemoveTags(jp.Text);
            }

            if (controller.Language == "ko")
            {
                calc = CalcFactory.MakeCalculator(nameof(CalcCjk));
            }

            var charactersPerSeconds = jp.GetCharactersPerSecond(calc);
            if (charactersPerSeconds > charactersPerSecond && !p.StartTime.IsMaxTime)
            {
                // Count with the same calculator used for the check above, so the new duration
                // actually lands on the Netflix limit for this language.
                var numberOfCharacters = (double)calc.CountCharacters(jp.Text, true);
                var maxDurationMilliseconds = numberOfCharacters / charactersPerSecond * 1000.0;
                var fixedParagraph = new Paragraph(p, false);
                fixedParagraph.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + maxDurationMilliseconds;

                controller.AddRecord(p, fixedParagraph, comment, FormattableString.Invariant($"CPS={charactersPerSeconds:0.##}"), true);
            }
        }
    }
}
