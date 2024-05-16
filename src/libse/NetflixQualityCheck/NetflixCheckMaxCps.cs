using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    /// <summary>
    /// Reading speed - depends on the language.
    /// </summary>
    public class NetflixCheckMaxCps : INetflixQualityChecker
    {
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            ICalcLength calc = CalcFactory.MakeCalculator(nameof(CalcAll));
            var charactersPerSecond = controller.CharactersPerSecond;
            var comment = "Maximum " + charactersPerSecond + " characters per second";
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

                var charactersPerSeconds = Utilities.GetCharactersPerSecond(jp, calc);
                if (charactersPerSeconds > charactersPerSecond && !p.StartTime.IsMaxTime)
                {
                    var fixedParagraph = new Paragraph(p, false);
                    while (Utilities.GetCharactersPerSecond(fixedParagraph) > charactersPerSecond)
                    {
                        fixedParagraph.EndTime.TotalMilliseconds++;
                    }

                    controller.AddRecord(p, fixedParagraph, comment, FormattableString.Invariant($"CPS={charactersPerSeconds:0.##}"));
                }
            }
        }
    }
}
