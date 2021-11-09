using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    /// <summary>
    /// Close gaps between subtitles of 3-11 frames (inclusive) to 2 frames.
    /// https://partnerhelp.netflixstudios.com/hc/en-us/articles/360051554394-Timed-Text-Style-Guide-Subtitle-Timing-Guidelines
    /// The 11 frames can change for other frame rates.
    /// </summary>
    public class NetflixCheckBridgeGaps : INetflixQualityChecker
    {
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            if (controller.Language == "ja")
            {
                return;
            }

            double twoFramesGap = 1000.0 / controller.FrameRate * 2.0;
            int halfSecGap = (int)Math.Round(controller.FrameRate / 2);

            for (int index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var p = subtitle.Paragraphs[index];
                var next = subtitle.GetParagraphOrDefault(index + 1);
                if (next == null)
                {
                    continue;
                }

                var gapInFrames = SubtitleFormat.MillisecondsToFrames(next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds);
                if (gapInFrames > 2 && gapInFrames < halfSecGap && !p.StartTime.IsMaxTime)
                {
                    var fixedParagraph = new Paragraph(p, false) { EndTime = { TotalMilliseconds = next.StartTime.TotalMilliseconds - twoFramesGap } };
                    string comment = $"3-{halfSecGap - 1} frames gap => 2 frames gap";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }
    }
}
