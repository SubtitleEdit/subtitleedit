using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle80 : SubtitleFormat
    {

        // 1<HT>01033902/01034028<HT>xxx
        private static readonly Regex RegexTimeCode = new Regex(@"^\d+\t\d+\/\d+\t", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".cap"; }
        }

        public override string Name
        {
            get { return "Unknown 80"; }
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
            const string paragraphWriteFormat = "{0}\t{1}/{2}\t{3}";
            var sb = new StringBuilder();
            sb.AppendLine("Lambda字幕V4\tDF1+1\tSCENE\"和文標準\"");
            sb.AppendLine();
            int count = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var text = HtmlUtil.RemoveHtmlTags(p.Text.Trim()).Replace(Environment.NewLine, Environment.NewLine + "\t\t\t\t");
                sb.AppendLine(string.Format(paragraphWriteFormat, count, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), text));
                count++;
            }
            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            var text = new StringBuilder();
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (s.Length > 19 && RegexTimeCode.IsMatch(s))
                {
                    var lineParts = s.Split('\t');
                    var parts = lineParts[1].Split('/');
                    if (parts.Length == 2)
                    {
                        if (text.Length > 0 && paragraph != null)
                        {
                            paragraph.Text = text.ToString().Trim();
                        }
                        try
                        {
                            paragraph = new Paragraph { StartTime = DecodeTimeCode(parts[0]), EndTime = DecodeTimeCode(parts[1]) };
                            subtitle.Paragraphs.Add(paragraph);
                            text = new StringBuilder();
                            s = s.Remove(0, 18 + lineParts[0].Length).Trim();
                            var idxA = s.IndexOf("＠");
                            if (idxA > 0)
                            {
                                s = s.Substring(0, idxA - 1).Trim();
                            }
                            text.Append(s);
                        }
                        catch (Exception)
                        {
                            _errorCount++;
                        }
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else if (paragraph != null && text.Length < 150)
                {
                    var idxA = s.IndexOf("＠");
                    if (idxA > 0)
                    {
                        s = s.Substring(0, idxA - 1).Trim();
                    }
                    text.Append(Environment.NewLine + s);
                }
                else
                {
                    _errorCount++;
                    if (_errorCount > 10)
                        return;
                }
            }
            if (text.Length > 0 && paragraph != null)
            {
                paragraph.Text = text.ToString().Trim();
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return time.ToHHMMSSFF().Replace(":", string.Empty);
        }

        private static TimeCode DecodeTimeCode(string part)
        {
            part = part.Trim();
            string hour = part.Substring(0,2);
            string minutes = part.Substring(2, 2);
            string seconds = part.Substring(4, 2);
            string frames = part.Substring(6, 2);
            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(int.Parse(frames)));
        }

    }
}