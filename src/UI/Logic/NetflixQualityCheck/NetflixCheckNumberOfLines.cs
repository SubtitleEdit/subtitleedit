using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

/// <summary>
/// Two lines maximum.
/// Text should usually be kept to one line, unless it exceeds the character limitation.
/// </summary>
public class NetflixCheckNumberOfLines : INetflixQualityChecker
{
    public string Name { get; set; }

    public NetflixCheckNumberOfLines(string name)
    {
        Name = name;
    }

    public void Check(Subtitle subtitle, NetflixQualityController controller)
    {
        foreach (var p in subtitle.Paragraphs)
        {
            if (p.Text.SplitToLines().Count > 2)
            {
                var fixedParagraph = new Paragraph(p, false);
                fixedParagraph.Text = Utilities.AutoBreakLine(fixedParagraph.Text, controller.SingleLineMaxLength, controller.SingleLineMaxLength - 3, controller.Language);
                if (fixedParagraph.Text.SplitToLines().Count > 2)
                {
                    fixedParagraph = null; // cannot fix text
                }
                var comment = Se.Language.Tools.NetflixCheckAndFix.TwoLinesMaximum;
                controller.AddRecord(p, fixedParagraph, comment);
            }
            else if (p.Text.SplitToLines().Count == 2 && p.Text.Contains(Environment.NewLine) &&
                p.Text.Replace(Environment.NewLine, " ").Replace("  ", " ").CountCharacters(false) <= controller.SingleLineMaxLength &&
                p.Text != Utilities.AutoBreakLine(p.Text, controller.SingleLineMaxLength, controller.SingleLineMaxLength + 1, controller.Language))
            {
                var fixedParagraph = new Paragraph(p, false);
                fixedParagraph.Text = Utilities.AutoBreakLine(fixedParagraph.Text, controller.SingleLineMaxLength, controller.SingleLineMaxLength + 1, controller.Language);
                var comment = Se.Language.Tools.NetflixCheckAndFix.TextCanFitOnOneLine;
                controller.AddRecord(p, fixedParagraph, comment, string.Empty, true);
            }
        }
    }

}
