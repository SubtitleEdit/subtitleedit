using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckBridgeGaps : INetflixQualityChecker
    {
        /// <summary>
        /// Two frames gap minimum
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            if (controller.Language == "ja")
            {
                return;
            }

            for (int index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var p = subtitle.Paragraphs[index];
                var next = subtitle.GetParagraphOrDefault(index + 1);
                if (next == null)
                {
                    continue;
                }

                double twoFramesGap = 1000.0 / controller.FrameRate * 2.0;
                var gapInFrames = SubtitleFormat.MillisecondsToFrames(next.StartTime.TotalMilliseconds) - SubtitleFormat.MillisecondsToFrames(p.EndTime.TotalMilliseconds);
                if (gapInFrames >= 3 && gapInFrames <= 11)
                {
                    var fixedParagraph = new Paragraph(p, false) { EndTime = { TotalMilliseconds = next.StartTime.TotalMilliseconds - twoFramesGap } };
                    string comment = "3-11 frames gap => 2 frames gap";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }
    }
}
