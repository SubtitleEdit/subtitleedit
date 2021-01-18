using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle100 : SubtitleFormat
    {
        public override string Extension => ".vid";

        public override string Name => "Unknown 100";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            foreach (var p in subtitle.Paragraphs)
            {
                sb.Append($"{GetTimeCodeString(p)}{Environment.NewLine}DG @015 A {p.Text}{Environment.NewLine}");
            }
            sb.Append("\u001a\u001a");

            return sb.ToString().Trim();
        }

        private static string GetTimeCodeString(Paragraph paragraph)
        {
            return $"#{paragraph.StartTime.TotalMilliseconds:00000000}{paragraph.Duration.TotalMilliseconds:000000}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            foreach (var line in lines)
            {
                var s = line.Trim();
                if (s.StartsWith('#'))
                {
                    s = s.TrimStart('#');
                    if (s.Length == 14)
                    {
                        var start = s.Substring(0, 8);
                        var duration = s.Remove(0, 8);
                        if (long.TryParse(start, out var startMs) && long.TryParse(duration, out var durationMs))
                        {
                            paragraph = new Paragraph(string.Empty, startMs, startMs + durationMs);
                            subtitle.Paragraphs.Add(paragraph);
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

                }
                else if (paragraph != null && s.StartsWith("DG @015 A"))
                {
                    paragraph.Text = (paragraph.Text + Environment.NewLine + line.Remove(0, 9).Trim()).Trim();
                }
                else
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }
    }
}
