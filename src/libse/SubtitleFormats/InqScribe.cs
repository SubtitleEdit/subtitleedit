using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class InqScribe : SubtitleFormat
    {
        public override string Extension => ".inqscr";

        public override string Name => "InqScribe 1.1";

        public override string ToText(Subtitle subtitle, string title)
        {
            var template = @"app=InqScribe
cache.mediaend=[START]
cache.mediastart=[END]
file.data=
font.name=Arial
font.size=12
print.bottom=1.
print.left=1.
print.right=1.
print.top=1.
print.units=1
state.aspectratio=0.
state.leftcolwidth=890
state.mediaheight=720
state.mediawidth=1280
state.windowpos=327,155,1241,800
tc.format=[x]
tc.includesourcename=0
tc.omitframes=0
tc.unbracketed=0
text=[TEXT]
timecode.fps=30
type=file
version=1.1

";

            if (subtitle.Paragraphs.Count > 0)
            {
                template = template.Replace("[START]", EncodeTimeCode(subtitle.Paragraphs.First().StartTime));
                template = template.Replace("[END]", EncodeTimeCode(subtitle.Paragraphs.Last().EndTime));

                var sb = new StringBuilder();
                for (var index = 0; index < subtitle.Paragraphs.Count; index++)
                {
                    var paragraph = subtitle.Paragraphs[index];
                    sb.Append($"[{EncodeTimeCode(paragraph.StartTime)}]\\r");
                    var text = paragraph.Text.Replace(Environment.NewLine, "\\r");
                    sb.Append($"{text}\\r");
                    if (index < subtitle.Paragraphs.Count - 1)
                    {
                        var next = subtitle.Paragraphs[index + 1];
                        if (next.StartTime.TotalMilliseconds > paragraph.EndTime.TotalMilliseconds + 200)
                        {
                            sb.Append($"[{EncodeTimeCode(paragraph.EndTime)}]\\r\\r");
                        }
                    }
                }

                template = template.Replace("[TEXT]", sb.ToString());
            }

            return template;
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            return timeCode.ToHHMMSSPeriodFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var idFound = false;
            var textLine = string.Empty;
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index];
                var s = line.Trim();
                if (!idFound)
                {
                    if (line == "app=InqScribe")
                    {
                        idFound = true;
                    }
                    else if (index > 10)
                    {
                        return;
                    }
                }
                else if (s.StartsWith("text=", StringComparison.Ordinal))
                {
                    textLine = s;
                }
                else if (s.StartsWith("timecode.fps=", StringComparison.Ordinal))
                {
                    var fpsString = s.Remove(0, 13);
                    if (double.TryParse(fpsString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var fr) &&
                        fr > 20 && fr < 200)
                    {
                        Configuration.Settings.General.CurrentFrameRate = fr;
                    }
                }
            }

            var matches = new Regex(@"\[\d\d:\d\d:\d\d\.\d\d\]").Matches(textLine);
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var start = DecodeTimeCode(match.Value);
                string text;
                if (i >= matches.Count - 1)
                {
                    text = textLine.Substring(match.Index + match.Value.Length);
                }
                else
                {
                    var nextMatch = matches[i + 1];
                    text = textLine.Substring(match.Index + match.Value.Length, nextMatch.Index - match.Index - nextMatch.Value.Length);
                }

                text = text.Replace("\\r", Environment.NewLine).Trim();
                subtitle.Paragraphs.Add(new Paragraph(start, new TimeCode(), text));
            }

            if (subtitle.Paragraphs.Count == 0)
            {
                return;
            }

            for (var index = 0; index < subtitle.Paragraphs.Count - 1; index++)
            {
                var paragraph = subtitle.Paragraphs[index];
                var next = subtitle.Paragraphs[index + 1];
                if (string.IsNullOrEmpty(next.Text))
                {
                    paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds;
                }
                else
                {
                    paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                }
            }

            var last = subtitle.Paragraphs.Last();
            last.Duration.TotalMilliseconds = Utilities.GetOptimalDisplayMilliseconds(last.Text);
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string s)
        {
            return DecodeTimeCodeFrames(s.TrimStart('[').TrimEnd(']'), new[] { ':', '.' });
        }
    }
}
