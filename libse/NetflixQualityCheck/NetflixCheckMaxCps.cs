using System;
using System.Globalization;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckMaxCps : INetflixQualityChecker
    {
        /// <summary>
        /// Speed - max 17 (for most languages) characters per second
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            var oldIgnoreWhiteSpace = Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace;
            try
            {
                Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace = false;
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    var jp = new Paragraph(p);
                    if (controller.Language == "ja")
                    {
                        jp.Text = HtmlUtil.RemoveHtmlTags(jp.Text, true);
                        jp.Text = NetflixImsc11Japanese.RemoveTags(jp.Text);
                    }
                    var charactersPerSeconds = Utilities.GetCharactersPerSecond(jp);
                    if (charactersPerSeconds > controller.CharactersPerSecond)
                    {
                        var fixedParagraph = new Paragraph(p, false);
                        while (Utilities.GetCharactersPerSecond(fixedParagraph) > controller.CharactersPerSecond)
                        {
                            fixedParagraph.EndTime.TotalMilliseconds++;
                        }
                        string comment = "Maximum " + controller.CharactersPerSecond  + " characters per second";
                        controller.AddRecord(p, fixedParagraph, comment, charactersPerSeconds.ToString(CultureInfo.InvariantCulture));
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
