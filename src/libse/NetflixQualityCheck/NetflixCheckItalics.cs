using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckItalics : INetflixQualityChecker
    {

        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
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
                            string comment = "Fixed italics";
                            controller.AddRecord(p, fixedParagraph, comment);
                        }
                    }
                    else
                    {
                        var fixedParagraph = new Paragraph(p, false);
                        fixedParagraph.Text = HtmlUtil.RemoveHtmlTags(fixedParagraph.Text);
                        if (fixedParagraph.Text != p.Text)
                        {
                            string comment = "Italics not allowed";
                            controller.AddRecord(p, fixedParagraph, comment);
                        }
                    }
                }
            }
        }

    }
}
