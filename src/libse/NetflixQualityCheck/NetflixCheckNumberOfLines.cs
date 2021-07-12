using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    /// <summary>
    /// Two lines maximum.
    /// Text should usually be kept to one line, unless it exceeds the character limitation.
    /// </summary>
    public class NetflixCheckNumberOfLines : INetflixQualityChecker
    {
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
                    p.Text != Utilities.AutoBreakLine(p.Text, controller.SingleLineMaxLength, controller.SingleLineMaxLength + 1, controller.Language))
                {
                    var fixedParagraph = new Paragraph(p, false);
                    fixedParagraph.Text = Utilities.AutoBreakLine(fixedParagraph.Text, controller.SingleLineMaxLength, controller.SingleLineMaxLength + 1, controller.Language);
                    string comment = "Text can fit on one line";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }

    }
}
