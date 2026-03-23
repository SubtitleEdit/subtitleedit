using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle84 : SubtitleFormat
    {

        private static readonly Regex RegexTimeCodes = new Regex(@"^<\d\d:\d\d:\d\d:\d\d><\d\d:\d\d:\d\d:\d\d>$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 84";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string writeFormat = "{3}{2}<{0}><{1}>{2}";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(writeFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), Environment.NewLine, HtmlUtil.RemoveHtmlTags(p.Text, true)));
                //Var vi bedre end japanerne
                //eller bare mere heldige?
                //<12:03:29:03> <12:03:35:06>
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:50:39:13 (last is frame)
            return time.ToHHMMSSFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                var s = line.Trim();
                if (s.Length == 26 && RegexTimeCodes.IsMatch(s))
                {
                    string start = s.Substring(1, 11);
                    string end = s.Substring(14, 11);

                    string[] startParts = start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 4 && endParts.Length == 4)
                    {
                        var p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), sb.ToString().Trim());
                        subtitle.Paragraphs.Add(p);
                    }
                    sb.Clear();
                }
                else
                {
                    sb.AppendLine(line.Trim());
                }

                if (sb.Length > 1000)
                {
                    return;
                }
            }
            subtitle.Renumber();
        }

    }
}
