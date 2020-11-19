using System;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckNumberOfLines : INetflixQualityChecker
    {

        /// <summary>
        /// Two lines maximum
        /// Text should be on one line if it fits
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (p.Text.SplitToLines().Count > 2)
                {
                    var fixedParagraph = new Paragraph(p, false);
                    fixedParagraph.Text = Utilities.AutoBreakLine(fixedParagraph.Text, controller.SingleLineMaxLength, controller.SingleLineMaxLength - 3, controller.Language);
                    if (fixedParagraph.Text.SplitToLines().Count > 2)
                    {
                        fixedParagraph = null; // cannot fix text
                    }
                    string comment = "Two lines maximum";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
                else if (p.Text.SplitToLines().Count == 2 && p.Text.Contains(Environment.NewLine) &&
                    p.Text.Replace(Environment.NewLine, " ").Replace("  ", " ").CountCharacters(false, Configuration.Settings.General.IgnoreArabicDiacritics) <= controller.SingleLineMaxLength &&
                    p.Text != Utilities.AutoBreakLine(p.Text, controller.Language))
                {
                    var fixedParagraph = new Paragraph(p, false);
                    fixedParagraph.Text = Utilities.AutoBreakLine(fixedParagraph.Text, controller.SingleLineMaxLength, controller.SingleLineMaxLength - 3, controller.Language);
                    string comment = "Text can fit on one line";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }

    }
}
