using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    /// <summary>
    /// When including ellipses in subtitles, use the single smart character (U+2026) as opposed to three dots/periods in a row.
    /// </summary>
    public class NetflixCheckEllipsesNotThreeDots : INetflixQualityChecker
    {
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            string comment = "Use the single smart character (U+2026) as opposed to three dots/periods in a row";

            foreach (var paragraph in subtitle.Paragraphs)
            {
                if (paragraph.Text.Contains("..."))
                {
                    var fixedParagraph = new Paragraph(paragraph, false) { Text = paragraph.Text.Replace("...", "…") };
                    controller.AddRecord(paragraph, fixedParagraph, comment);
                }
            }
        }
    }
}
