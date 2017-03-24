namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckNumberOfLines : INetflixQualityChecker
    {

        /// <summary>
        /// Two lines maximum
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (p.Text.SplitToLines().Length > 2)
                {
                    var fixedParagraph = new Paragraph(p, false);
                    fixedParagraph.Text = Utilities.AutoBreakLine(fixedParagraph.Text, 43, 40, controller.Language);
                    if (fixedParagraph.Text.SplitToLines().Length > 2)
                    {
                        fixedParagraph = null; // cannot fix text
                    }
                    string comment = "Two lines maximum";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }

    }
}
