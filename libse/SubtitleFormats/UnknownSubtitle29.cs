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

        public override string Extension => ".txt";

        public override string Name => "Unknown 29";

        private static string MakeTimeCode(TimeCode tc)
        {
            return $"{tc.Hours:00}:{tc.Minutes:00}:{tc.Seconds:00}:{MillisecondsToFramesMaxFrameRate(tc.Milliseconds):00}";
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string text = HtmlUtil.RemoveHtmlTags(p.Text);
                sb.AppendLine($"{MakeTimeCode(p.StartTime)}\t{MakeTimeCode(p.EndTime)}\r\n{text}\r\n");
            }
            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            var sb = new StringBuilder();
            char[] splitChars = { ':', ';', ',' };
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
                        string[] arr = s.Split('\t');
                        if (arr.Length == 2)
                        {
                            p = new Paragraph(DecodeTimeCodeFrames(arr[0], splitChars), DecodeTimeCodeFrames(arr[1], splitChars), string.Empty);
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
