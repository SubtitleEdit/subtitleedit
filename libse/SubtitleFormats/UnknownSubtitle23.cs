using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle23 : SubtitleFormat
    {
        //1:  01:00:19.04  01:00:21.05
        private static readonly Regex RegexTimeCode1 = new Regex(@"^\s*\d+:\s+\d\d:\d\d:\d\d.\d\d\s+\d\d:\d\d:\d\d.\d\d\s*$", RegexOptions.Compiled);

        public override string Extension => ".rtf";

        public override string Name => "Unknown 23";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(title);
            sb.AppendLine(@"1ab

23/03/2012
03/05/2012
**:**:**.**
01:00:00.00
**:**:**.**
**:**:**.**
01:01:01.12
01:02:30.00
01:02:54.01
**:**:**.**
**:**:**.**
01:19:33.08
");

            int count = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine($"{count.ToString(CultureInfo.InvariantCulture).PadLeft(9, ' ')}:  {MakeTimeCode(p.StartTime)}  {MakeTimeCode(p.EndTime)}\r\n{p.Text}");
                count++;
            }

            return sb.ToString().Trim().ToRtf();
        }

        private static string MakeTimeCode(TimeCode timeCode)
        {
            return timeCode.ToHHMMSSPeriodFF();
        }

        private static TimeCode DecodeTimeCode(string timeCode)
        {
            string[] arr = timeCode.Split(new[] { ':', ';', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);
            return new TimeCode(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), FramesToMillisecondsMax999(int.Parse(arr[3])));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            string rtf = sb.ToString().Trim();
            if (!rtf.StartsWith("{\\rtf", StringComparison.Ordinal))
            {
                return;
            }

            lines = rtf.FromRtf().SplitToLines();
            _errorCount = 0;
            Paragraph p = null;
            sb.Clear();
            foreach (string line in lines)
            {
                string s = line.TrimEnd();
                if (RegexTimeCode1.IsMatch(s))
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
                        if (arr.Length == 3)
                        {
                            p = new Paragraph(DecodeTimeCode(arr[1]), DecodeTimeCode(arr[2]), string.Empty);
                        }
                    }
                    catch
                    {
                        _errorCount++;
                        p = null;
                    }
                }
                else if (p != null && s.Length > 0)
                {
                    sb.AppendLine(s.Trim());
                }
                else if (!string.IsNullOrWhiteSpace(s))
                {
                    _errorCount++;
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
