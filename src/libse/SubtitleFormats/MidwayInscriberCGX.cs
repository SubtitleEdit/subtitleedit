using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class MidwayInscriberCGX : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"<\d\d:\d\d:\d\d:\d\d> <\d\d:\d\d:\d\d:\d\d>$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Midway Inscriber CG-X";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string writeFormat = "{3} <{0}> <{1}>{2}";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(writeFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), Environment.NewLine, HtmlUtil.RemoveHtmlTags(p.Text, true)));
                //Var vi bedre end japanerne
                //eller bare mere heldige? <12:03:29:03> <12:03:35:06>
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

            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.Append(line);
            }

            if (!sb.ToString().Contains("> <"))
            {
                return;
            }

            //Var vi bedre end japanerne
            //eller bare mere heldige? <12:03:29:03> <12:03:35:06>

            subtitle.Paragraphs.Clear();
            sb.Clear();
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (RegexTimeCodes.IsMatch(line))
                    {
                        int idx = RegexTimeCodes.Match(line).Index;
                        string temp = line.Substring(0, idx).Trim();
                        sb.AppendLine(temp);

                        string start = line.Substring(idx + 1, 11);
                        string end = line.Substring(idx + 15, 11);

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
