namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckMinDuration : INetflixQualityChecker
    {

        /// <summary>
        /// Minimum duration: 5/6 second (833 ms) - also see https://github.com/SubtitleEdit/plugins/issues/129
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            for (int index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                Paragraph p = subtitle.Paragraphs[index];
                var next = subtitle.GetParagraphOrDefault(index + 1);
                if (p.Duration.TotalMilliseconds < 833)
                {
                    Paragraph fixedParagraph = null;
                    if (next == null || next.StartTime.TotalMilliseconds > p.StartTime.TotalMilliseconds + 834)
                    {
                        // we can fix duration
                        fixedParagraph = new Paragraph(p, false);
                        fixedParagraph.Duration.TotalMilliseconds = 834;
                    }
                    string comment = "Minimum duration: 5/6 second (833 ms)";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }
    }
}
