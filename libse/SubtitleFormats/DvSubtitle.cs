using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class DvSubtitle : SubtitleFormat
    {
        private static readonly Regex LinePattern = new Regex(@"^c 1 \d{8} \d{8} .+", RegexOptions.Compiled);

        public override string Extension => ".dv";

        public override string Name => "DV Subtitle";

        public override bool IsTimeBased => false;

        public override bool IsMine(List<string> lines, string fileName)
        {
            var okCount = 0;
            var errors = 0;
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && line.StartsWith('c') && LinePattern.IsMatch(line))
                {
                    okCount++;
                }
                else
                {
                    errors++;
                }
            }
            return okCount > errors;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine($"c 1 {MillisecondsToFrames(p.StartTime.TotalMilliseconds):00000000} {MillisecondsToFrames(p.EndTime.TotalMilliseconds):00000000} {WriteText(p.Text)} 0");
            }
            return sb.ToString().Trim();
        }

        private static string WriteText(string text)
        {
            var sb = new StringBuilder();
            var lines = text.SplitToLines();
            foreach (var line in lines)
            {
                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }

                sb.Append('\'');
                sb.Append(line);
                sb.Append('\'');
            }

            if (lines.Count == 1)
            {
                sb.Append(" ''");
            }

            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && line.StartsWith('c') && LinePattern.IsMatch(line))
                {
                    var arr = line.Split();
                    if (long.TryParse(arr[2], out var start) && long.TryParse(arr[3], out var end))
                    {
                        var text = ReadText(line.Remove(0, 22));
                        subtitle.Paragraphs.Add(new Paragraph(text, FramesToMilliseconds(start), FramesToMilliseconds(end)));
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    _errorCount++;
                }

                if (_errorCount > 25)
                {
                    return;
                }
            }

            subtitle.Renumber();
        }

        private string ReadText(string input)
        {
            var arr = input.TrimEnd('0').Replace("' '", "\n").SplitToLines();
            var sb = new StringBuilder();
            for (var index = 0; index < arr.Count; index++)
            {
                var line = arr[index].Trim();
                if (index == 0 && line.StartsWith('\''))
                {
                    line = line.Remove(0, 1);
                }
                else if (index == arr.Count - 1 && line.EndsWith('\''))
                {
                    line = line.Remove(line.Length - 1);
                }

                sb.AppendLine(line);
            }

            return sb.ToString().Trim();
        }
    }
}
