using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class DvdStudioPro : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d+:\d+[:;]\d+\t,\t\d+:\d+:\d+[:;]\d+\t,\t.*$", RegexOptions.Compiled);

        public override string Extension => ".STL";

        public override string Name => "DVD Studio Pro";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0}\t,\t{1}\t,\t{2}\r\n";
            const string timeFormat = "{0:00}:{1:00}:{2:00}:{3:00}";
            const string header = @"$VertAlign          =   Bottom
$Bold               =   FALSE
$Underlined         =   FALSE
$Italic             =   FALSE
$XOffset                =   0
$YOffset                =   -5
$TextContrast           =   15
$Outline1Contrast           =   15
$Outline2Contrast           =   13
$BackgroundContrast     =   0
$ForceDisplay           =   FALSE
$FadeIn             =   0
$FadeOut                =   0
$HorzAlign          =   Center
";

            var lastVerticalAlign = "$VertAlign = Bottom";
            var lastHorizontalcalAlign = "$HorzAlign = Center";
            var sb = new StringBuilder();
            sb.AppendLine(header);
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string startTime = string.Format(timeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds));
                string endTime = string.Format(timeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds));
                sb = ToTextAlignment(p, sb, ref lastVerticalAlign, ref lastHorizontalcalAlign);
                sb.AppendFormat(paragraphWriteFormat, startTime, endTime, EncodeStyles(p.Text));
            }
            return sb.ToString().Trim();
        }

        internal static StringBuilder ToTextAlignment(Paragraph p, StringBuilder sb, ref string lastVerticalAlign, ref string lastHorizontalAlign)
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
                verticalAlign = "$VertAlign = Top";
            else if (verticalCenterAlign)
                verticalAlign = "$VertAlign = Center";
            else
                verticalAlign = "$VertAlign = Bottom";
            if (lastVerticalAlign != verticalAlign)
                sb.AppendLine(verticalAlign);

            bool horizontalLeftAlign = p.Text.StartsWith("{\\an1}", StringComparison.Ordinal) ||
                                       p.Text.StartsWith("{\\an4}", StringComparison.Ordinal) ||
                                       p.Text.StartsWith("{\\an7}", StringComparison.Ordinal);
            bool horizontalRightAlign = p.Text.StartsWith("{\\an3}", StringComparison.Ordinal) ||
                                        p.Text.StartsWith("{\\an6}", StringComparison.Ordinal) ||
                                        p.Text.StartsWith("{\\an9}", StringComparison.Ordinal);
            if (horizontalLeftAlign)
                horizontalAlign = "$HorzAlign = Left";
            else if (horizontalRightAlign)
                horizontalAlign = "$HorzAlign = Right";
            else
                horizontalAlign = "$HorzAlign = Center";
            if (lastHorizontalAlign != horizontalAlign)
                sb.AppendLine(horizontalAlign);

            lastVerticalAlign = verticalAlign;
            lastHorizontalAlign = horizontalAlign;
            return sb;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            int number = 0;
            var verticalAlign = "$VertAlign=Bottom";
            var horizontalAlign = "$HorzAlign=Center";
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && line[0] != '$')
                {
                    if (RegexTimeCodes.Match(line).Success)
                    {
                        string[] threePart = line.Split(new[] { "\t,\t" }, StringSplitOptions.None);
                        var p = new Paragraph();
                        if (threePart.Length == 3 &&
                            GetTimeCode(p.StartTime, threePart[0]) &&
                            GetTimeCode(p.EndTime, threePart[1]))
                        {
                            number++;
                            p.Number = number;
                            p.Text = threePart[2].TrimEnd().Replace(" | ", Environment.NewLine).Replace("|", Environment.NewLine);
                            p.Text = DecodeStyles(p.Text);
                            p.Text = GetAlignment(verticalAlign, horizontalAlign) + p.Text;
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else if (line != null && line.TrimStart().StartsWith("$VertAlign", StringComparison.OrdinalIgnoreCase))
                {
                    verticalAlign = line.RemoveChar(' ').RemoveChar('\t');
                }
                else if (line != null && line.TrimStart().StartsWith("$HorzAlign", StringComparison.OrdinalIgnoreCase))
                {
                    horizontalAlign = line.RemoveChar(' ').RemoveChar('\t');
                }
            }
        }

        internal static string GetAlignment(string verticalAlign, string horizontalAlign)
        {
            if (verticalAlign.Equals("$VertAlign=Top"))
            {
                if (horizontalAlign.Equals("$HorzAlign=Left"))
                    return "{\\an7}";
                if (horizontalAlign.Equals("$HorzAlign=Right"))
                    return "{\\an9}";
                return "{\\an8}";
            }

            if (verticalAlign.Equals("$VertAlign=Center"))
            {
                if (horizontalAlign.Equals("$HorzAlign=Left"))
                    return "{\\an4}";
                if (horizontalAlign.Equals("$HorzAlign=Right"))
                    return "{\\an6}";
                return "{\\an5}";
            }

            if (horizontalAlign.Equals("$HorzAlign=Left"))
                return "{\\an1}";
            if (horizontalAlign.Equals("$HorzAlign=Right"))
                return "{\\an3}";
            return string.Empty;
        }

        internal static string DecodeStyles(string text)
        {
            var sb = new StringBuilder();
            bool italicOn = false;
            bool boldOn = false;
            bool skipNext = false;
            for (int i = 0; i < text.Length; i++)
            {
                if (skipNext)
                {
                    skipNext = false;
                }
                else
                {
                    if (text.Substring(i).StartsWith("^I", StringComparison.Ordinal))
                    {
                        if (!italicOn)
                            sb.Append("<i>");
                        else
                            sb.Append("</i>");
                        italicOn = !italicOn;
                        skipNext = true;
                    }
                    else if (text.Substring(i).StartsWith("^B", StringComparison.Ordinal))
                    {
                        if (!boldOn)
                            sb.Append("<b>");
                        else
                            sb.Append("</b>");
                        boldOn = !boldOn;
                        skipNext = true;
                    }
                    else
                    {
                        sb.Append(text[i]);
                    }
                }
            }
            return sb.ToString();
        }

        internal static string EncodeStyles(string text)
        {
            text = Utilities.RemoveSsaTags(text);
            text = text.Replace("<I>", "<i>").Replace("</I>", "</i>");
            bool allItalic = text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(text, "<i>") == 1;

            text = text.Replace("<i>", "^I");
            text = text.Replace("<I>", "^I");
            text = text.Replace("</i>", "^I");
            text = text.Replace("</I>", "^I");

            text = text.Replace("<b>", "^B");
            text = text.Replace("<B>", "^B");
            text = text.Replace("</b>", "^B");
            text = text.Replace("</B>", "^B");

            if (allItalic)
                return text.Replace(Environment.NewLine, "^I|^I");
            return text.Replace(Environment.NewLine, "|");
        }

        internal static bool GetTimeCode(TimeCode timeCode, string timeString)
        {
            try
            {
                string[] timeParts = timeString.Split(':', ';');
                timeCode.Hours = int.Parse(timeParts[0]);
                timeCode.Minutes = int.Parse(timeParts[1]);
                timeCode.Seconds = int.Parse(timeParts[2]);
                timeCode.Milliseconds = FramesToMillisecondsMax999(int.Parse(timeParts[3]));
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
