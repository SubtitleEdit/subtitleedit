using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle26 : SubtitleFormat
    {

        //L'Enfant d'en haut
        //1ab

        //10/01/2012
        //10/01/2012
        //01:00:22.09
        //01:00:30.09
        //**:**:**.**
        //**:**:**.**
        //01:00:51.09
        //01:00:55.22
        //01:01:10.09
        //**:**:**.**
        //**:**:**.**
        //01:13:23.09
        //
        //1:  01:01:28.22  01:01:33.09*
        //     SISTER
        //
        //2:
        //01:02:58.15 01:03:00.00
        //I'm pooping, sir.
        //Were they easy to get?

        private static readonly Regex RegexTimeCode = new Regex(@"^\d+:  \d\d:\d\d:\d\d.\d\d  \d\d:\d\d:\d\d.\d\d[\*]*$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 26"; }
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
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", tc.Hours, tc.Minutes, tc.Seconds, MillisecondsToFramesMaxFrameRate(tc.Milliseconds));
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(title);
            sb.AppendLine(@"1ab

10/01/2012
10/01/2012
01:00:22.09
01:00:30.09
**:**:**.**
**:**:**.**
01:00:51.09
01:00:55.22
01:01:10.09
**:**:**.**
**:**:**.**
01:13:23.09");
            sb.AppendLine();
            int count = 1;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string text = HtmlUtil.RemoveHtmlTags(p.Text);
                sb.AppendLine(string.Format("{0}:  {1}  {2}\r\n{3}\r\n", count, MakeTimeCode(p.StartTime), MakeTimeCode(p.EndTime), text));
                count++;
            }

            return sb.ToString();
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
                        sb.Clear();
                        string[] arr = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        char[] splitChars = { ':', ';', ',', '.' };
                        if (arr.Length == 3)
                            p = new Paragraph(DecodeTimeCodeFrames(arr[1].TrimEnd('*'), splitChars), DecodeTimeCodeFrames(arr[2].TrimEnd('*'), splitChars), string.Empty);
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
