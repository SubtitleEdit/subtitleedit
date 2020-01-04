using System.Globalization;

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
                var p = subtitle.Paragraphs[index];

                if (controller.Language == "ja")
                {
                    if (p.Duration.TotalMilliseconds < 500)
                    {
                        string comment = "Minimum duration: 0.5 second";
                        controller.AddRecord(p, p.StartTime.ToHHMMSSFF(), comment, p.Duration.TotalSeconds.ToString(CultureInfo.InvariantCulture));
                    }
                    continue;
                }

                var next = subtitle.GetParagraphOrDefault(index + 1);
                if (p.Duration.TotalMilliseconds < 833)
                {
                    Paragraph fixedParagraph = null;
                    if (next == null || next.StartTime.TotalMilliseconds > p.StartTime.TotalMilliseconds + 834)
                    {
                        // we can fix duration
                        fixedParagraph = new Paragraph(p, false);
                        fixedParagraph.EndTime.TotalMilliseconds = fixedParagraph.StartTime.TotalMilliseconds + 834;
                    }
                    string comment = "Minimum duration: 5/6 second (833 ms)";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }
    }
}
