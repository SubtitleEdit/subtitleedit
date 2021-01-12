using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    /// <summary>
    /// Maximum duration: 7 seconds per subtitle event.
    /// </summary>
    public class NetflixCheckMaxDuration : INetflixQualityChecker
    {
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (p.Duration.TotalMilliseconds > 7000)
                {
                    var fixedParagraph = new Paragraph(p, false);
                    fixedParagraph.EndTime.TotalMilliseconds = fixedParagraph.StartTime.TotalMilliseconds + 7000;
                    string comment = "Maximum duration: 7 seconds per subtitle event";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }
    }
}
