namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckNumberOfLines : INetflixQualityChecker
    {

        /// <summary>
        /// Two lines maximum
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            const string comment = "Two lines maximum";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (p.NumberOfLines > 2)
                {
                    var fixedParagraph = new Paragraph(p, false);
                    fixedParagraph.Text = Utilities.AutoBreakLine(fixedParagraph.Text, controller.SingleLineMaxLength, controller.SingleLineMaxLength - 3, controller.Language);
                    if (fixedParagraph.NumberOfLines > 2)
                    {
                        fixedParagraph = null; // cannot fix text
                    }
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }

    }
}
