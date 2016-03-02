using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle42 : SubtitleFormat
    {
        //SUB[0 I 01:00:09:10>01:00:12:10]
        //SUB[0 N 01:00:09:10>01:00:12:10]

        // Time code line can optionally contain "speaker"
        //SUB[0 N 01:02:02:03>01:02:03:06] VAL
        //e eu tenho maiô pra nadar?

        // or multiple "speakers" seperated with a "+"
        //SUB[0 N 01:02:12:26>01:02:14:19] FABINHO CRIANÇA + VAL
        //-Olha.
        //-Tô olhando!
        private static readonly Regex RegexTimeCodes = new Regex(@"^SUB\[\d [NI] \d\d:\d\d:\d\d:\d\d\>\d\d:\d\d:\d\d:\d\d\]", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 42"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string style = "N";
                if (p.Text.StartsWith("<i>", StringComparison.Ordinal) && p.Text.EndsWith("</i>", StringComparison.Ordinal))
                    style = "I";
                sb.AppendLine(string.Format("SUB[0 {0} {1}>{2}]{3}{4}", style, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), Environment.NewLine, HtmlUtil.RemoveHtmlTags(p.Text)));
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:50:39:13 (last is frame)
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //SUB[0 N 01:00:16:22>01:00:19:04]
            //a cerca de 65 km a norte
            //de Nova Iorque.

            _errorCount = 0;
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            bool italic = false;
            foreach (string line in lines)
            {
                string lineTrimmed = line.Trim();
                if (lineTrimmed.Length >= 32 && RegexTimeCodes.IsMatch(lineTrimmed))
                {
                    if (p != null && italic)
                        p.Text = "<i>" + p.Text.Trim() + "</i>";

                    italic = lineTrimmed[6] == 'I';
                    string start = lineTrimmed.Substring(8, 11);
                    string end = lineTrimmed.Substring(20, 11);
                    string[] startParts = start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        TimeCode startTime = DecodeTimeCodeFramesFourParts(startParts);
                        TimeCode endTime = DecodeTimeCodeFramesFourParts(endParts);
                        p = new Paragraph(startTime, endTime, string.Empty);
                        subtitle.Paragraphs.Add(p);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (p != null && (lineTrimmed.Length > 0 && !lineTrimmed.StartsWith('@')))
                {
                    if (string.IsNullOrEmpty(p.Text))
                        p.Text = lineTrimmed;
                    else
                        p.Text = p.Text.TrimEnd() + Environment.NewLine + lineTrimmed;
                }
            }
            if (p != null && italic)
                p.Text = "<i>" + p.Text.Trim() + "</i>";
            subtitle.Renumber();
        }

    }
}
