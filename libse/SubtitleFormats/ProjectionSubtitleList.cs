using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class ProjectionSubtitleList : SubtitleFormat
    {
        public override string Extension => ".psl";
        public override string Name => "Projection Subtitle List";
        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                sb.Append($"{EncodeTime(p.StartTime)} {EncodeTime(p.EndTime)} {{\\uc0\\pard\\qc");
                foreach (var line in p.Text.SplitToLines())
                {
                    sb.Append("{");
                    sb.Append(line); //TODO: add RTF tags
                    sb.Append("}\\par");
                }
                sb.AppendLine("}");
            }
            sb.AppendLine();
            return sb.ToString();
        }

        private static string EncodeTime(TimeCode time)
        {
            return MillisecondsToFrames(time.TotalMilliseconds).ToString(CultureInfo.InvariantCulture).PadLeft(8, ' ');
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            foreach (string line in lines)
            {
                int startText = line.IndexOf("{\\", StringComparison.Ordinal);
                if (startText < 3)
                {
                    _errorCount++;
                    if (_errorCount > 20)
                    {
                        break;
                    }
                    continue;
                }

                var frameArray = line.Substring(0, startText).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (frameArray.Length != 2)
                {
                    _errorCount++;
                    if (_errorCount > 20)
                    {
                        break;
                    }
                    continue;
                }

                if (long.TryParse(frameArray[0], NumberStyles.None, CultureInfo.InvariantCulture, out var startFrame) &&
                    long.TryParse(frameArray[1], NumberStyles.None, CultureInfo.InvariantCulture, out var endFrame))

                {
                    var text = line.Remove(0, startText);
                    //TODO: remove RTF tags
                    subtitle.Paragraphs.Add(new Paragraph(text, FramesToMilliseconds(startFrame), FramesToMilliseconds(endFrame)));
                }
            }
            subtitle.Renumber();
        }
    }
}
