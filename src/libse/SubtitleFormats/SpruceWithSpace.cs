using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SpruceWithSpace : SubtitleFormat
    {

        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d, \d\d:\d\d:\d\d:\d\d,.+", RegexOptions.Compiled);

        public override string Extension => ".stl";

        public override string Name => "Spruce Subtitle With Space";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string header = @"$FontName           =   Arial
$FontSize               =   34
$HorzAlign          =   Left
$VertAlign          =   Bottom
$XOffset                =   0
$YOffset                =   0
$Bold                   =   FALSE
$UnderLined         =   FALSE
$Italic             =   FALSE
$TextContrast           =   15
$Outline1Contrast       =   15
$Outline2Contrast       =   15
$BackgroundContrast =   0
$ForceDisplay           =   FALSE
$FadeIn             =   0
$FadeOut                =   0
$TapeOffset         =   FALSE

\\Colour 0 = Black
\\Colour 1 = Red
\\Colour 2 = Green
\\Colour 3 = Yellow
\\Colour 4 = Blue
\\Colour 5 = Magenta
\\Colour 6 = Cyan
\\Colour 7 = White
";

            var lastVerticalAlign = "$VertAlign     = Bottom";
            var lastHorizontalcalAlign = "$HorzAlign     = Center";
            var sb = new StringBuilder();
            sb.AppendLine(header);
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb = ToTextAlignment(p, sb, ref lastVerticalAlign, ref lastHorizontalcalAlign);
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)}, {EncodeTimeCode(p.EndTime)}, {EncodeText(p.Text)}");
            }
            return sb.ToString();
        }

        private static StringBuilder ToTextAlignment(Paragraph p, StringBuilder sb, ref string lastVerticalAlign, ref string lastHorizontalAlign)
        {
            string verticalAlign;
            string horizontalAlign;
            bool verticalTopAlign = p.Text.StartsWith("{\\an7}", StringComparison.Ordinal) ||
                                    p.Text.StartsWith("{\\an8}", StringComparison.Ordinal) ||
                                    p.Text.StartsWith("{\\an9}", StringComparison.Ordinal);
            bool verticalCenterAlign = p.Text.StartsWith("{\\an4}", StringComparison.Ordinal) ||
                                       p.Text.StartsWith("{\\an5}", StringComparison.Ordinal) ||
                                       p.Text.StartsWith("{\\an6}", StringComparison.Ordinal);
            if (verticalTopAlign)
            {
                verticalAlign = "$VertAlign     = Top";
            }
            else if (verticalCenterAlign)
            {
                verticalAlign = "$VertAlign     = Center";
            }
            else
            {
                verticalAlign = "$VertAlign     = Bottom";
            }

            if (lastVerticalAlign != verticalAlign)
            {
                sb.AppendLine(verticalAlign);
            }

            bool horizontalLeftAlign = p.Text.StartsWith("{\\an1}", StringComparison.Ordinal) ||
                                       p.Text.StartsWith("{\\an4}", StringComparison.Ordinal) ||
                                       p.Text.StartsWith("{\\an7}", StringComparison.Ordinal);
            bool horizontalRightAlign = p.Text.StartsWith("{\\an3}", StringComparison.Ordinal) ||
                                        p.Text.StartsWith("{\\an6}", StringComparison.Ordinal) ||
                                        p.Text.StartsWith("{\\an9}", StringComparison.Ordinal);
            if (horizontalLeftAlign)
            {
                horizontalAlign = "$HorzAlign     = Left";
            }
            else if (horizontalRightAlign)
            {
                horizontalAlign = "$HorzAlign     = Right";
            }
            else
            {
                horizontalAlign = "$HorzAlign     = Center";
            }

            sb.AppendLine(horizontalAlign);

            lastVerticalAlign = verticalAlign;
            lastHorizontalAlign = horizontalAlign;
            return sb;
        }

        private static string EncodeText(string input)
        {
            var text = input.Replace("<b>", "^B")
                            .Replace("</b>", string.Empty)
                            .Replace("<i>", "^I")
                            .Replace("</i>", string.Empty)
                            .Replace("<u>", "^U")
                            .Replace("</u>", string.Empty);
            return HtmlUtil.RemoveHtmlTags(text, true).Replace(Environment.NewLine, "|");
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:01:54:19
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:01:54:19,00:01:56:17,We should be thankful|they accepted our offer.
            _errorCount = 0;
            var verticalAlign = "$VertAlign=Bottom";
            var horizontalAlign = "$HorzAlign=Center";
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (line.IndexOf(':') == 2 && RegexTimeCodes.IsMatch(line))
                {
                    string start = line.Substring(0, 11);
                    string end = line.Substring(13, 11);

                    try
                    {
                        var text = DecodeText(line.Substring(25).Trim());
                        text = DvdStudioPro.GetAlignment(verticalAlign, horizontalAlign) + text;
                        var startTime = DecodeTimeCodeFramesFourParts(start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries));
                        var endTime = DecodeTimeCodeFramesFourParts(end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries));
                        var p = new Paragraph(startTime, endTime, text);
                        subtitle.Paragraphs.Add(p);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (line.TrimStart().StartsWith("$VertAlign", StringComparison.OrdinalIgnoreCase))
                {
                    verticalAlign = line.RemoveChar(' ', '\t');
                }
                else if (line.TrimStart().StartsWith("$HorzAlign", StringComparison.OrdinalIgnoreCase))
                {
                    horizontalAlign = line.RemoveChar(' ', '\t');
                }
                else if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("//", StringComparison.Ordinal) && !line.StartsWith('$'))
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

        private static string DecodeText(string text)
        { // TODO: Improve end tags
            text = text.Replace("|", Environment.NewLine);
            if (text.Contains("^B"))
            {
                text = text.Replace("^B", "<b>") + "</b>";
            }

            if (text.Contains("^I"))
            {
                text = text.Replace("^I", "<i>") + "</i>";
            }

            if (text.Contains("^U"))
            {
                text = text.Replace("^U", "<u>") + "</u>";
            }

            return text;
        }
    }
}
