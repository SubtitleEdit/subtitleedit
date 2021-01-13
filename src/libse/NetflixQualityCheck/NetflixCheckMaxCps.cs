using Nikse.SubtitleEdit.Core.Common;
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
            var oldIgnoreWhiteSpace = Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace;
            try
            {
                Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace = false;

                int charactersPerSecond = controller.CharactersPerSecond;
                string comment = "Maximum " + charactersPerSecond + " characters per second";
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    var jp = new Paragraph(p);
                    if (controller.Language == "ja")
                    {
                        jp.Text = HtmlUtil.RemoveHtmlTags(jp.Text, true);
                        jp.Text = NetflixImsc11Japanese.RemoveTags(jp.Text);
                    }

                    var charactersPerSeconds = Utilities.GetCharactersPerSecond(jp);
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
            finally
            {
                Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace = oldIgnoreWhiteSpace;
            }
        }

    }
}
