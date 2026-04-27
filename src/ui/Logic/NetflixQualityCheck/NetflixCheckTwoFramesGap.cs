using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

/// <summary>
/// Two frames gap minimum.
/// </summary>
public class NetflixCheckTwoFramesGap : INetflixQualityChecker
{
    public string Name { get; set; }

    public NetflixCheckTwoFramesGap(string name)
    {
        Name = name;
    }

    public void Check(Subtitle subtitle, NetflixQualityController controller)
    {
        if (controller.Language == "ja")
        {
            return;
        }

        double twoFramesGap = 1000.0 / controller.FrameRate * 2.0;

        for (int index = 0; index < subtitle.Paragraphs.Count; index++)
        {
            Paragraph p = subtitle.Paragraphs[index];
            var next = subtitle.GetParagraphOrDefault(index + 1);
            if (next != null && SubtitleFormat.MillisecondsToFrames(next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds) < 2 && !p.StartTime.IsMaxTime)
            {
                var fixedParagraph = new Paragraph(p, false) { EndTime = { TotalMilliseconds = next.StartTime.TotalMilliseconds - twoFramesGap } };
                string comment;

                if (p.EndTime.TotalMilliseconds > next.StartTime.TotalMilliseconds)
                {
                    comment = Se.Language.Tools.NetflixCheckAndFix.MinimumTwoFramesGapOverlapping;
                }
                else
                {
                    comment = Se.Language.Tools.NetflixCheckAndFix.MinimumTwoFramesGap;
                }

                controller.AddRecord(p, fixedParagraph, comment, string.Empty, true);
            }
        }
    }
}
