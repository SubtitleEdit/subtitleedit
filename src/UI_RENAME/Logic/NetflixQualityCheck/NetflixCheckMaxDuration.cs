using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

/// <summary>
/// Maximum duration: 7 seconds per subtitle event.
/// </summary>
public class NetflixCheckMaxDuration : INetflixQualityChecker
{
    public string Name { get; set; }

    public NetflixCheckMaxDuration(string name)
    {
        Name = name;
    }

    public void Check(Subtitle subtitle, NetflixQualityController controller)
    {
        foreach (Paragraph p in subtitle.Paragraphs)
        {
            if (p.DurationTotalMilliseconds > 7000)
            {
                var fixedParagraph = new Paragraph(p, false);
                fixedParagraph.EndTime.TotalMilliseconds = fixedParagraph.StartTime.TotalMilliseconds + 7000;
                string comment = Se.Language.Tools.NetflixCheckAndFix.MaximumDuration;
                controller.AddRecord(p, fixedParagraph, comment, string.Empty, true);
            }
        }
    }
}

