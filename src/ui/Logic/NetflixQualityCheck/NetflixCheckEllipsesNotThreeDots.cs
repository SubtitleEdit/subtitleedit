using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

/// <summary>
/// When including ellipses in subtitles, use the single smart character (U+2026) as opposed to three dots/periods in a row.
/// </summary>
public class NetflixCheckEllipsesNotThreeDots : INetflixQualityChecker
{
    public string Name { get; set; }

    public NetflixCheckEllipsesNotThreeDots(string name)
    {
        Name = name;
    }

    public void Check(Subtitle subtitle, NetflixQualityController controller)
    {
        string comment = Se.Language.Tools.NetflixCheckAndFix.EllipsesUseSmartCharacter;

        foreach (var paragraph in subtitle.Paragraphs)
        {
            if (paragraph.Text.Contains("..."))
            {
                var fixedParagraph = new Paragraph(paragraph, false) { Text = paragraph.Text.Replace("...", "…") };
                controller.AddRecord(paragraph, fixedParagraph, comment, string.Empty, true);
            }
        }
    }
}
