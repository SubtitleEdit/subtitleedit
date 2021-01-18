using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
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
                                string comment = "Single vertical line length > 11";
                                controller.AddRecord(p, p.StartTime.ToHHMMSSFF(), comment, line.Length.ToString(CultureInfo.InvariantCulture));
                            }
                        }
                        else // Horizontal subtitles - Maximum 13 full-width characters per line
                        {
                            if (CalculateJapaneseLength(text) > 13)
                            {
                                string comment = "Single horizontal line length > 13";
                                controller.AddRecord(p, p.StartTime.ToHHMMSSFF(), comment, line.Length.ToString(CultureInfo.InvariantCulture));
                            }
                        }
                    }
                    else if (line.CountCharacters(false, Configuration.Settings.General.IgnoreArabicDiacritics) > controller.SingleLineMaxLength)
                    {
                        var fixedParagraph = new Paragraph(p, false);
                        fixedParagraph.Text = Utilities.AutoBreakLine(fixedParagraph.Text, controller.SingleLineMaxLength, controller.SingleLineMaxLength - 3, controller.Language);
                        string comment = "Single line length > " + controller.SingleLineMaxLength;
                        controller.AddRecord(p, fixedParagraph, comment, line.Length.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        private static int CalculateJapaneseLength(string text)
        {
            return text.Length;
        }
    }
}
