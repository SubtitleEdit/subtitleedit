using System.Globalization;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckMaxLineLength : INetflixQualityChecker
    {

        /// <summary>
        /// Maximum duration: 7 seconds per subtitle event
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                foreach (var line in p.Text.SplitToLines())
                {
                    if (line.Length > controller.SingleLineMaxLength)
                    {
                        var fixedParagraph = new Paragraph(p, false);
                        fixedParagraph.Text = Utilities.AutoBreakLine(fixedParagraph.Text, controller.SingleLineMaxLength, controller.SingleLineMaxLength - 3, controller.Language);
                        string comment = "Single line length > " + controller.SingleLineMaxLength;
                        controller.AddRecord(p, fixedParagraph, comment, line.Length.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
        }

    }
}
