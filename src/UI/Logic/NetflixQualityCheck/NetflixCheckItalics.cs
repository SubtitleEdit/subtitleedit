using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

public class NetflixCheckItalics : INetflixQualityChecker
{
    public string Name { get; set; }

    public NetflixCheckItalics(string name)
    {
        Name = name;
    }

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
                        string comment = Se.Language.Tools.NetflixCheckAndFix.ItalicsFixed;
                        controller.AddRecord(p, fixedParagraph, comment);
                    }
                }
                else
                {
                    var fixedParagraph = new Paragraph(p, false);
                    fixedParagraph.Text = HtmlUtil.RemoveHtmlTags(fixedParagraph.Text);
                    if (fixedParagraph.Text != p.Text)
                    {
                        string comment = Se.Language.Tools.NetflixCheckAndFix.ItalicsNotAllowed;
                        controller.AddRecord(p, fixedParagraph, comment, string.Empty, true);
                    }
                }
            }
        }
    }
}
