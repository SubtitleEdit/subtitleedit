namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckMaxDuration : INetflixQualityChecker
    {

        /// <summary>
        /// Maximum duration: 7 seconds per subtitle event
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            const string comment = "Maximum duration: 7 seconds per subtitle event";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (p.Duration.TotalMilliseconds > 7000)
                {
                    var fixedParagraph = new Paragraph(p, false);
                    fixedParagraph.Duration.TotalMilliseconds = 7000;
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }

    }
}
