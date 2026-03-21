using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

/// <summary>
/// Maximum 42 chars per line for the majority of languages.
/// </summary>
public class NetflixCheckMaxLineLength : INetflixQualityChecker
{
    public void Check(Subtitle subtitle, NetflixQualityController controller)
    {
        foreach (var p in subtitle.Paragraphs)
        {
            foreach (var line in p.Text.SplitToLines())
            {
                if (controller.Language == "ja")
                {
                    var vertical = p.Text.Contains("{\\an7", StringComparison.Ordinal) || p.Text.Contains("{\\an9", StringComparison.Ordinal);
                    var text = HtmlUtil.RemoveHtmlTags(line, true);
                    text = NetflixImsc11Japanese.RemoveTags(text);
                    if (vertical) // Vertical subtitles - Maximum 11 full-width characters per line
                    {
                        if (CalculateJapaneseLength(text) > 11)
                        {
                            var comment = "Single vertical line length > 11";
                            controller.AddRecord(p, p.StartTime.ToHHMMSSFF(), line.Length.ToString(CultureInfo.InvariantCulture), comment, false);
                        }
                    }
                    else // Horizontal subtitles - Maximum 13 full-width characters per line
                    {
                        if (CalculateJapaneseLength(text) > 13)
                        {
                            var comment = "Single horizontal line length > 13";
                            controller.AddRecord(p, p.StartTime.ToHHMMSSFF(), line.Length.ToString(CultureInfo.InvariantCulture), comment);
                        }
                    }
                }
                else if (controller.Language == "ko" && line.CountCharacters(nameof(CalcCjk), false) > controller.SingleLineMaxLength)
                {
                    var fixedParagraph = new Paragraph(p, false);
                    fixedParagraph.Text = Utilities.AutoBreakLine(fixedParagraph.Text, controller.SingleLineMaxLength, controller.SingleLineMaxLength - 3, controller.Language);
                    var comment = "Single line length > " + controller.SingleLineMaxLength;
                    controller.AddRecord(p, fixedParagraph, comment, line.CountCharacters(nameof(CalcCjk), false).ToString(CultureInfo.InvariantCulture), true);
                }
                else if (line.CountCharacters(false) > controller.SingleLineMaxLength)
                {
                    var fixedParagraph = new Paragraph(p, false);
                    fixedParagraph.Text = Utilities.AutoBreakLine(fixedParagraph.Text, controller.SingleLineMaxLength, controller.SingleLineMaxLength - 3, controller.Language);
                    var comment = "Single line length > " + controller.SingleLineMaxLength;
                    controller.AddRecord(p, fixedParagraph, comment, line.Length.ToString(CultureInfo.InvariantCulture), true   );
                }
            }
        }
    }

    private static int CalculateJapaneseLength(string text)
    {
        return text.Length;
    }
}
