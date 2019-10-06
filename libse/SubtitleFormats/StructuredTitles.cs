using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class StructuredTitles : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\d\d[ a-z]: \d\d:\d\d:\d\d:\d\d,\d\d:\d\d:\d\d:\d\d,\d{1,2}", RegexOptions.Compiled);
        private static readonly Regex RegexSomeCodes = new Regex(@"^\d\d \d\d \d\d", RegexOptions.Compiled);
        private static readonly Regex RegexText = new Regex(@"^[A-Z]\d[A-Z]\d\d ", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Structured titles";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Structured titles");
            int index = 0;

            //0001 : 01:07:25:08,01:07:29:00,10
            //80 80 80
            //C1Y00 Niemand zal je helpen ontsnappen.
            //C1Y00 - Een agent heeft me geholpen.
            foreach (var p in subtitle.Paragraphs)
            {
                // 1=first line, 11=bottom line, 10=bottom of two lines subtitle
                string verticalAlignment;
                if (p.Text.StartsWith("{\\an8}", StringComparison.Ordinal))
                {
                    verticalAlignment = "1"; // one line
                }
                else
                {
                    verticalAlignment = (12 - Utilities.GetNumberOfLines(p.Text)).ToString(CultureInfo.InvariantCulture);
                }

                sb.AppendLine($"{index + 1:0000} : {EncodeTimeCode(p.StartTime)},{EncodeTimeCode(p.EndTime)},{verticalAlignment}");
                sb.AppendLine("80 80 80");
                var italic = Utilities.RemoveSsaTags(p.Text).StartsWith("<i>", StringComparison.OrdinalIgnoreCase) &&
                             p.Text.EndsWith("</i>", StringComparison.OrdinalIgnoreCase);
                var pre = string.Empty;
                var post = string.Empty;
                if (italic)
                {
                    pre = "<";
                    post = ">";
                }
                var lines = HtmlUtil.RemoveHtmlTags(p.Text, true).SplitToLines();
                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];
                    sb.AppendLine(GetPositionCode(i, p.Extra) + " " + pre + line.Trim() + post);
                }
                sb.AppendLine();
                index++;
            }
            return sb.ToString();
        }

        private static string GetPositionCode(int lineNumber, string extra)
        {
            if (!string.IsNullOrWhiteSpace(extra))
            {
                var arr = extra.Split(':');
                if (lineNumber < arr.Length && arr[lineNumber].Length == 5)
                {
                    return arr[lineNumber];
                }
            }
            return "C1Y00";
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //0001 : 01:07:25:08,01:07:29:00,10
            _errorCount = 0;
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (line.IndexOf(':') == 5 && RegexTimeCodes.IsMatch(line))
                {
                    if (p != null)
                    {
                        subtitle.Paragraphs.Add(p);
                    }

                    string start = line.Substring(7, 11);
                    string end = line.Substring(19, 11);

                    string[] startParts = start.Split(SplitCharColon);
                    string[] endParts = end.Split(SplitCharColon);
                    if (startParts.Length == 4 && endParts.Length == 4)
                    {
                        p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), string.Empty);
                        if (line.EndsWith(",1", StringComparison.Ordinal) || line.EndsWith(",2", StringComparison.Ordinal))
                        {
                            p.Text = "{\\an8}";
                        }
                    }
                }
                else if (p != null && RegexText.IsMatch(line))
                {
                    if (string.IsNullOrEmpty(p.Text))
                    {
                        p.Extra = line.Substring(0, 5);
                        p.Text = line.Substring(5).Trim();
                    }
                    else if (p.Text == "{\\an8}")
                    {
                        p.Extra = line.Substring(0, 5);
                        p.Text += line.Substring(5).Trim();
                    }
                    else
                    {
                        p.Extra += ":" + line.Substring(0, 5);
                        p.Text += Environment.NewLine + line.Substring(5).Trim();
                    }
                }
                else if (string.IsNullOrWhiteSpace(line) || line.Length < 10 && RegexSomeCodes.IsMatch(line))
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    if (p.Text != null && Utilities.GetNumberOfLines(p.Text) > 3)
                    {
                        _errorCount++;
                    }
                    else
                    {
                        if (!line.TrimEnd().EndsWith(": --:--:--:--,--:--:--:--,-1", StringComparison.Ordinal))
                        {
                            if (string.IsNullOrEmpty(p.Text))
                            {
                                p.Text = line.Trim();
                            }
                            else
                            {
                                p.Text += Environment.NewLine + line.Trim();
                            }
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(p?.Text))
            {
                subtitle.Paragraphs.Add(p);
            }

            FixItalics(subtitle);

            subtitle.Renumber();
        }

        private static void FixItalics(Subtitle subtitle)
        {
            foreach (var p in subtitle.Paragraphs)
            {
                if (p.Text.Contains('<') && p.Text.Contains('>'))
                {
                    var sb = new StringBuilder();
                    foreach (var line in p.Text.SplitToLines())
                    {
                        if (line.StartsWith('<') && line.EndsWith('>'))
                        {
                            sb.AppendLine("<i>" + line.TrimStart('<').TrimEnd('>') + "</i>");
                        }
                        else
                        {
                            sb.AppendLine(line);
                        }
                    }
                    p.Text = HtmlUtil.FixInvalidItalicTags(sb.ToString().TrimEnd());
                }
            }
        }
    }
}
