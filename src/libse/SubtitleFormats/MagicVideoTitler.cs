using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class MagicVideoTitler : SubtitleFormat
    {
        public override string Extension => ".mvt";
        public override string Name => "Magic Video Titler";
        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("`ABOUT=Subtitle Edit export to Magic Video Titler III Professional");
            sb.AppendLine();

            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine($"`TITLIN={MillisecondsToFrames(p.StartTime.TotalMilliseconds):00000000}");
                sb.AppendLine($"`TITLOUT={MillisecondsToFrames(p.EndTime.TotalMilliseconds):00000000}");

                var lineLetters = new[] { 'A', 'B', 'C', 'D', 'E' };
                var list = p.Text.SplitToLines();
                for (var index = 0; index < list.Count; index++)
                {
                    if (index < lineLetters.Length)
                    {
                        var line = list[index];
                        var lineLetter = lineLetters[index];
                        sb.AppendLine($"`TITL{lineLetter}=" + line);
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var p = new Paragraph();
            foreach (var line in lines)
            {
                var s = line.Trim();

                if (s.StartsWith("`TITLIN=", StringComparison.Ordinal))
                {
                    if (p.EndTime.TotalMilliseconds > 0 && !string.IsNullOrEmpty(p.Text))
                    {
                        subtitle.Paragraphs.Add(p);
                    }

                    p = new Paragraph();
                    var framesText = s.Remove(0, 8).Trim();
                    if (long.TryParse(framesText, out var frames))
                    {
                        p.StartTime.TotalMilliseconds = FramesToMilliseconds(frames);
                    }
                }
                else if (s.StartsWith("`TITLOUT=", StringComparison.Ordinal))
                {
                    var framesText = s.Remove(0, 9).Trim();
                    if (long.TryParse(framesText, out var frames))
                    {
                        p.EndTime.TotalMilliseconds = FramesToMilliseconds(frames);
                    }
                }
                else if (s.Length > 7 && s[6] == '=' && s.StartsWith("`TITL", StringComparison.Ordinal))
                {
                    p.Text = (p.Text + Environment.NewLine + s.Remove(0, 7)).Trim();
                }
            }

            if (p.EndTime.TotalMilliseconds > 0 && !string.IsNullOrEmpty(p.Text))
            {
                subtitle.Paragraphs.Add(p);
            }
        }
    }
}
