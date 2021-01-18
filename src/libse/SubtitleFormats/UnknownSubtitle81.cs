using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    internal class UnknownSubtitle81 : SubtitleFormat
    {

        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+a?:\s+\**\d\d:\d\d:\d\d\.\d\d\s+\d\d:\d\d:\d\d\.\d\d\**\s+\d\d\.\d\d\s+\d+$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 81";

        public override string ToText(Subtitle subtitle, string title)
        {
            string pre = title + @"
Enigma
PAL
SDI Media Group
ENGLISH (US)
WB,GDMX,1:33,4x3
0000.00
0000.00
0000.00
0000.00
0000.00
0000.00
0000.00
#: Italics Text
@/: Force title
@+: reposition top
@|: reposition middle

";

            var sb = new StringBuilder();
            sb.AppendLine(pre);
            int count = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine($"     {count}:  {p.StartTime.ToHHMMSSPeriodFF()}  {p.EndTime.ToHHMMSSPeriodFF()}  {p.Duration.Seconds:00}.{MillisecondsToFrames(p.Duration.Milliseconds):00}  {p.Text.Length}");
                foreach (var line in EncodeText(p.Text).SplitToLines())
                {
                    sb.AppendLine("        " + line);
                }
                sb.AppendLine();
                count++;
            }
            sb.AppendLine();
            sb.AppendLine();
            return sb.ToString();
        }

        private static string EncodeText(string text)
        {
            var sb = new StringBuilder();
            int i = 0;
            text = text.Replace("#", string.Empty);
            while (i < text.Length)
            {
                if (text.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append("#");
                    i += 3;
                }
                else if (text.Substring(i).StartsWith("</i>", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append("#");
                    i += 4;
                }
                else
                {
                    sb.Append(text[i++]);
                }
            }
            return HtmlUtil.RemoveHtmlTags(sb.ToString().TrimEnd('#'), true);
        }

        private static readonly char[] TimeCodeSplitChars = { ':', '.' };
        private static readonly char[] LineSplitChars = { ' ', '\t' };

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            foreach (var line in lines)
            {
                string s = line.Trim();
                if (s.Length >= 38 && s.Length <= 47 && RegexTimeCodes.IsMatch(s))
                {
                    if (p != null)
                    {
                        subtitle.Paragraphs.Add(p);
                    }
                    s = s.Replace("*", string.Empty);
                    try
                    {
                        var arr = s.Split(LineSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        p = new Paragraph(DecodeTimeCodeFrames(arr[1], TimeCodeSplitChars), DecodeTimeCodeFrames(arr[2], TimeCodeSplitChars), string.Empty);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (p != null && s.Length > 0)
                {
                    if (p.Text.Length > 500)
                    {
                        _errorCount++;
                        return;
                    }
                    p.Text = (p.Text + Environment.NewLine + s).Trim();
                }
            }
            if (p != null)
            {
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
            foreach (var paragraph in subtitle.Paragraphs)
            {
                paragraph.Text = DecodeText(paragraph.Text);
            }
        }

        private static string DecodeText(string text)
        {
            var sb = new StringBuilder();
            int i = 0;
            bool italicOn = false;
            while (i < text.Length)
            {
                if (text[i] == '#')
                {
                    sb.Append(italicOn ? "</i>" : "<i>");
                    italicOn = !italicOn;
                    i++;
                }
                else
                {
                    sb.Append(text[i++]);
                }
            }
            if (italicOn)
            {
                sb.Append("</i>");
            }

            return sb.ToString().Replace("@+", string.Empty).Replace("@/", string.Empty).Replace("@|", string.Empty).Replace(" </i>", "</i> ");
        }

    }
}
