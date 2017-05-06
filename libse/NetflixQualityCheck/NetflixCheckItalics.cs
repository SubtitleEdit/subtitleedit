using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckItalics : INetflixQualityChecker
    {

        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            // comments
            const string fixedItalic = "Fixed italics";
            const string italicNotAllowed = "Italics not allowed";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (p.Text.Contains("i>", StringComparison.OrdinalIgnoreCase))
                {
                    if (controller.AllowItalics)
                    {
                        var fixedParagraph = new Paragraph(p, false);
                        fixedParagraph.Text = HtmlUtil.FixInvalidItalicTags(fixedParagraph.Text);
                        if (fixedParagraph.Text != p.Text)
                        {
                            controller.AddRecord(p, fixedParagraph, fixedItalic);
                        }
                    }
                    else
                    {
                        var fixedParagraph = new Paragraph(p, false);
                        fixedParagraph.Text = HtmlUtil.RemoveHtmlTags(fixedParagraph.Text);
                        if (fixedParagraph.Text != p.Text)
                        {
                            controller.AddRecord(p, fixedParagraph, italicNotAllowed);
                        }
                    }
                }
            }
        }

    }
}
