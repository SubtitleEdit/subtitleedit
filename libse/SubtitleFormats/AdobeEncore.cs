using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AdobeEncore : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d \d\d:\d\d:\d\d:\d\d ", RegexOptions.Compiled);
        private int _maxMsDiv10;

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Adobe Encore"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();

            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);
            if (sb.ToString().Contains("#INPOINT OUTPOINT PATH"))
                return false; // Pinnacle Impression

            LoadSubtitle(subtitle, lines, fileName);

            bool containsNewLine = false;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (p.Text.Contains(Environment.NewLine))
                {
                    containsNewLine = true;
                    break;
                }
            }
            if (!containsNewLine)
            {
                if (_maxMsDiv10 > 90)
                    return false; // "DVD Subtitle System" format (frame rate should not go higher than 90...)
                if (sb.ToString().Contains("//"))
                    return false; // "DVD Subtitle System" format
            }
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //00:03:15:22 00:03:23:10 This is line one.
                //This is line two.
                sb.AppendLine(string.Format("{0} {1} {2}", EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), HtmlUtil.RemoveHtmlTags(p.Text, true)));
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return time.ToHHMMSSFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:03:15:22 00:03:23:10 This is line one.
            //This is line two.
            Paragraph p = null;
            _maxMsDiv10 = 0;
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            Match match = null;
            foreach (string line in lines)
            {
                if (line.Length >= 23 && (match = RegexTimeCodes.Match(line)).Success)
                {
                    string temp = line.Substring(0, match.Value.Length);
                    string start = temp.Substring(0, 11);
                    string end = temp.Substring(12, 11);

                    string[] startParts = start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    string text = line.Substring(match.Length).Trim();
                    p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), text);
                    subtitle.Paragraphs.Add(p);
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    if (p != null)
                    {
                        if (string.IsNullOrEmpty(p.Text))
                            p.Text = line;
                        else
                            p.Text += Environment.NewLine + line;
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }

            subtitle.Renumber();
        }

    }
}
