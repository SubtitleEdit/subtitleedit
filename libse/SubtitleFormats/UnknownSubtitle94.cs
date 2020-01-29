using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle94 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d+\t+\d+:\d+\t.*", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 94";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                //0:13	0:14	I'm from Londrina, Paraná, Brasil.
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)}\t{EncodeTimeCode(p.EndTime)}\t{HtmlUtil.RemoveHtmlTags(p.Text, true).Replace(Environment.NewLine, " ")}");
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{(int)(time.TotalSeconds / 60):0}:{time.Seconds:00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                var s = line.Trim();
                if (RegexTimeCodes.IsMatch(s))
                {
                    var arr = s.Split('\t');
                    string start = arr[0];
                    string end = arr[1];
                    var text = arr[2];
                    var p = new Paragraph(DecodeTimeCode(start), DecodeTimeCode(end), text);
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
            var arr = secondsAndMilliseconds.Split(':');

            // minutes to milliseconds
            var totalMilliseconds = double.Parse(arr[0], NumberStyles.None, CultureInfo.InvariantCulture) * TimeCode.BaseUnit * 60;

            // seconds to milliseconds
            totalMilliseconds += double.Parse(arr[1], NumberStyles.None, CultureInfo.InvariantCulture) * TimeCode.BaseUnit;
            return new TimeCode(totalMilliseconds);
        }
    }
}
