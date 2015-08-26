using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle29 : SubtitleFormat
    {

        //00:00:30:21   00:00:35:22
        //Et c'est là que nous devions
        //passer la nuit.

        //00:00:38:04   00:00:40:18
        //Un nouveau martyre a commencé.

        private static readonly Regex RegexTimeCode = new Regex(@"^\d\d:\d\d:\d\d:\d\d\t\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 29"; }
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

        private static string MakeTimeCode(TimeCode tc)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", tc.Hours, tc.Minutes, tc.Seconds, MillisecondsToFramesMaxFrameRate(tc.Milliseconds));
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string text = HtmlUtil.RemoveHtmlTags(p.Text);
                sb.AppendLine(string.Format("{0}\t{1}\r\n{2}\r\n", MakeTimeCode(p.StartTime), MakeTimeCode(p.EndTime), text));
            }
            return sb.ToString();
        }

        private static TimeCode DecodeTimeCode(string timeCode)
        {
            string[] arr = timeCode.Split(new[] { ':', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return new TimeCode(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), FramesToMillisecondsMax999(int.Parse(arr[3])));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                string s = line.TrimEnd();
                if (RegexTimeCode.IsMatch(s))
                {
                    try
                    {
                        if (p != null)
                        {
                            p.Text = sb.ToString().Trim();
                            subtitle.Paragraphs.Add(p);
                        }
                        sb = new StringBuilder();
                        string[] arr = s.Split('\t');
                        if (arr.Length == 2)
                            p = new Paragraph(DecodeTimeCode(arr[0]), DecodeTimeCode(arr[1]), string.Empty);
                    }
                    catch
                    {
                        _errorCount++;
                        p = null;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(s))
                {
                    sb.AppendLine(s);
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
        }

    }
}
