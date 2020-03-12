using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle96 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^\d\d\d\d:\d\d\s*[-–]\s*\d\d\d\d:\d\d\s+", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 96";

        public override string ToText(Subtitle subtitle, string title)
        {
            //0201:37 -0202:15                Thank you.
            //0207:26 - 0209:13             Well, now, what does she think I’m
            //    Gonna do with all these papers here.

            const string writeFormat = "{0:0000}:{1:00} - {2:0000}:{3:00} {4}";
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(writeFormat, p.StartTime.TotalSeconds, MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds),
                    p.EndTime.TotalSeconds, MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds),
                    p.Text));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (var line in lines)
            {
                var s = line.Trim();
                if (string.IsNullOrEmpty(s))
                {
                    // ignore
                }
                else
                {
                    var match = RegexTimeCode.Match(s);
                    if (match.Success)
                    {
                        var arr = match.Value.Split('-', '–');
                        if (arr.Length == 2)
                        {
                            var start = GetTimeCode(arr[0].Trim());
                            var end = GetTimeCode(arr[1].Trim());
                            var text = s.Remove(0, match.Value.Length).Trim();
                            paragraph = new Paragraph(start, end, text);
                            subtitle.Paragraphs.Add(paragraph);
                        }
                    }
                    else if (paragraph != null)
                    {
                        paragraph.Text = (paragraph.Text + Environment.NewLine + s).Trim();
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }
            subtitle.Renumber();
        }

        private static TimeCode GetTimeCode(string code)
        {
            var arr = code.Split(':');
            if (int.TryParse(arr[0], out int seconds) && int.TryParse(arr[1], out int frames))
            {
                return new TimeCode(seconds * TimeCode.BaseUnit + FramesToMillisecondsMax999(frames));
            }
            return new TimeCode();
        }
    }
}
