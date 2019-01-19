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

        public override string Extension => ".txt";

        public override string Name => "Unknown 26";

        private static string MakeTimeCode(TimeCode tc)
        {
            return $"{tc.Hours:00}:{tc.Minutes:00}:{tc.Seconds:00}.{MillisecondsToFramesMaxFrameRate(tc.Milliseconds):00}";
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
                sb.AppendLine($"{count}:  {MakeTimeCode(p.StartTime)}  {MakeTimeCode(p.EndTime)}\r\n{text}\r\n");
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
                        {
                            p = new Paragraph(DecodeTimeCodeFrames(arr[1].TrimEnd('*'), splitChars), DecodeTimeCodeFrames(arr[2].TrimEnd('*'), splitChars), string.Empty);
                        }
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
