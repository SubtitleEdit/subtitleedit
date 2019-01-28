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
            var header = Configuration.Settings.SubtitleSettings.DvdStudioProHeader.TrimEnd() + Environment.NewLine;

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
            {
                verticalAlign = "$VertAlign = Top";
            }
            else if (verticalCenterAlign)
            {
                verticalAlign = "$VertAlign = Center";
            }
            else
            {
                verticalAlign = "$VertAlign = Bottom";
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
                horizontalAlign = "$HorzAlign = Left";
            }
            else if (horizontalRightAlign)
            {
                horizontalAlign = "$HorzAlign = Right";
            }
            else
            {
                horizontalAlign = "$HorzAlign = Center";
            }

            if (lastHorizontalAlign != horizontalAlign)
            {
                sb.AppendLine(horizontalAlign);
            }

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
            bool italicOn = false;
            bool boldOn = false;
            bool underlineOn = false;

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
                            if (italicOn && !p.Text.Contains("<i>"))
                            {
                                p.Text = "<i>" + p.Text + "</i>";
                            }
                            if (boldOn && !p.Text.Contains("<b>"))
                            {
                                p.Text = "<b>" + p.Text + "</b>";
                            }
                            if (underlineOn && !p.Text.Contains("<u>"))
                            {
                                p.Text = "<u>" + p.Text + "</u>";
                            }
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
                else if (line.Replace(" ", string.Empty).Equals("$Italic=True", StringComparison.OrdinalIgnoreCase))
                {
                    italicOn = true;
                }
                else if (line.Replace(" ", string.Empty).Trim().Equals("$Italic=False", StringComparison.OrdinalIgnoreCase))
                {
                    italicOn = false;
                }
                else if (line.Replace(" ", string.Empty).Equals("$Bold=True", StringComparison.OrdinalIgnoreCase))
                {
                    boldOn = true;
                }
                else if (line.Replace(" ", string.Empty).Trim().Equals("$Bold=False", StringComparison.OrdinalIgnoreCase))
                {
                    boldOn = false;
                }
                else if (line.Replace(" ", string.Empty).Equals("$Underlined=True", StringComparison.OrdinalIgnoreCase))
                {
                    underlineOn = true;
                }
                else if (line.Replace(" ", string.Empty).Trim().Equals("$Underlined=False", StringComparison.OrdinalIgnoreCase))
                {
                    underlineOn = false;
                }
            }
        }

        internal static string GetAlignment(string verticalAlign, string horizontalAlign)
        {
            if (verticalAlign.Equals("$VertAlign=Top", StringComparison.OrdinalIgnoreCase))
            {
                if (horizontalAlign.Equals("$HorzAlign=Left", StringComparison.OrdinalIgnoreCase))
                {
                    return "{\\an7}";
                }

                if (horizontalAlign.Equals("$HorzAlign=Right", StringComparison.OrdinalIgnoreCase))
                {
                    return "{\\an9}";
                }

                return "{\\an8}";
            }

            if (verticalAlign.Equals("$VertAlign=Center", StringComparison.OrdinalIgnoreCase))
            {
                if (horizontalAlign.Equals("$HorzAlign=Left", StringComparison.OrdinalIgnoreCase))
                {
                    return "{\\an4}";
                }

                if (horizontalAlign.Equals("$HorzAlign=Right", StringComparison.OrdinalIgnoreCase))
                {
                    return "{\\an6}";
                }

                return "{\\an5}";
            }

            if (horizontalAlign.Equals("$HorzAlign=Left", StringComparison.OrdinalIgnoreCase))
            {
                return "{\\an1}";
            }

            if (horizontalAlign.Equals("$HorzAlign=Right", StringComparison.OrdinalIgnoreCase))
            {
                return "{\\an3}";
            }

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
                        sb.Append(!italicOn ? "<i>" : "</i>");
                        italicOn = !italicOn;
                        skipNext = true;
                    }
                    else if (text.Substring(i).StartsWith("^B", StringComparison.Ordinal))
                    {
                        sb.Append(!boldOn ? "<b>" : "</b>");
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

        internal static string EncodeStyles(string input)
        {
            var text = Utilities.RemoveSsaTags(input);
            text = text.Replace("<I>", "<i>").Replace("</I>", "</i>");
            bool allItalic = text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(text, "<i>") == 1;
            bool allBold = text.StartsWith("<b>", StringComparison.Ordinal) && text.EndsWith("</b>", StringComparison.Ordinal) && Utilities.CountTagInText(text, "<b>") == 1;
            bool allUnderline = text.StartsWith("<u>", StringComparison.Ordinal) && text.EndsWith("</u>", StringComparison.Ordinal) && Utilities.CountTagInText(text, "<u>") == 1;
            bool allUnderlineBoldItalic = text.StartsWith("<u><b><i>", StringComparison.Ordinal) && text.EndsWith("</i></b></u>", StringComparison.Ordinal) && Utilities.CountTagInText(text, "<u>") == 1;
            bool allBoldItalic = text.StartsWith("<b><i>", StringComparison.Ordinal) && text.EndsWith("</i></b>", StringComparison.Ordinal) && Utilities.CountTagInText(text, "<i>") == 1 && Utilities.CountTagInText(text, "<b>") == 1;

            text = text.Replace("<i>", "^I");
            text = text.Replace("<I>", "^I");
            text = text.Replace("</i>", "^I");
            text = text.Replace("</I>", "^I");

            text = text.Replace("<b>", "^B");
            text = text.Replace("<B>", "^B");
            text = text.Replace("</b>", "^B");
            text = text.Replace("</B>", "^B");

            text = text.Replace("<u>", "^U");
            text = text.Replace("<U>", "^U");
            text = text.Replace("</u>", "^U");
            text = text.Replace("</U>", "^U");

            if (allUnderlineBoldItalic)
            {
                return text.Replace(Environment.NewLine, "^U^B^I|^I^B^U");
            }

            if (allBoldItalic)
            {
                return text.Replace(Environment.NewLine, "^U^B^I|^I^B^U");
            }

            if (allItalic)
            {
                return text.Replace(Environment.NewLine, "^I|^I");
            }

            if (allBold)
            {
                return text.Replace(Environment.NewLine, "^B|^B");
            }

            if (allUnderline)
            {
                return text.Replace(Environment.NewLine, "^U|^U");
            }

            return text.Replace(Environment.NewLine, "|");
        }

        internal static bool GetTimeCode(TimeCode timeCode, string timeString)
        {
            try
            {
                var timeParts = timeString.Split(':', ';');
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
