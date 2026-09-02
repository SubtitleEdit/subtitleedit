using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Subtitle script for the DVD Junior / ImgTool DVD authoring tools:
    /// 00:00:05:25&amp;00:00:08:20#come here
    /// It is Spruce STL with "&amp;" and "#" instead of the two commas, so it shares
    /// the "|" line break and the ^I/^B/^U style toggles.
    /// </summary>
    public class Spc : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d&\d\d:\d\d:\d\d:\d\d#", RegexOptions.Compiled);

        public override string Extension => ".spc";

        public override string Name => "DVD Junior SPC";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)}&{EncodeTimeCode(p.EndTime)}#{EncodeText(p.Text)}");
            }

            return sb.ToString();
        }

        private static string EncodeText(string input)
        {
            var text = HtmlUtil.RemoveHtmlTags(input, true);
            return text.Replace(Environment.NewLine, "|");
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:01:54:19
            var frames = MillisecondsToFramesMaxFrameRate(time.Milliseconds);
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{frames:00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();

            foreach (var line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    try
                    {
                        var start = line.Substring(0, 11);
                        var end = line.Substring(12, 11);
                        var text = Spruce.DecodeStyleToggles(line.Substring(24).Replace("|", Environment.NewLine));
                        subtitle.Paragraphs.Add(new Paragraph(DecodeTimeCode(start), DecodeTimeCode(end), text));
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    _errorCount++;
                }
            }

            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string time)
        {
            //00:01:54:19
            var parts = time.Split(':');
            var milliseconds = FramesToMillisecondsMax999(int.Parse(parts[3]));
            return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), milliseconds);
        }
    }
}
