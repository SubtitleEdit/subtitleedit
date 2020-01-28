using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle93 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\[\d+\.\d+ +\d+\.\d+\].*", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 93";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //[13.560 23.900] Heute geht es...
                sb.AppendLine($"[{EncodeTimeCode(p.StartTime)} {EncodeTimeCode(p.EndTime)}] {HtmlUtil.RemoveHtmlTags(p.Text, true).Replace(Environment.NewLine, "\r")}");
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.000}", time.TotalSeconds);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                var s = line.Trim();
                if (RegexTimeCodes.IsMatch(s))
                {
                    var indexOfSeparator = s.IndexOf(']');
                    var timeCodes = s.Substring(1, indexOfSeparator - 2);
                    var text = s.Remove(0, indexOfSeparator + 1).TrimStart();
                    var arr = timeCodes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string start = arr[0];
                    string end = arr[1];
                    text = text.Replace("\r", Environment.NewLine);
                    p = new Paragraph(DecodeTimeCode(start), DecodeTimeCode(end), text);
                    subtitle.Paragraphs.Add(p);
                }
                else
                {
                    _errorCount++;
                    if (_errorCount > 20)
                    {
                        return;
                    }
                }
            }
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string secondsAndMilliseconds)
        {
            return new TimeCode(double.Parse(secondsAndMilliseconds, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) * TimeCode.BaseUnit);
        }
    }
}
