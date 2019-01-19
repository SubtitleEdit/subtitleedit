using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle9 : SubtitleFormat
    {
        //00:04:04.219
        //The city council of long beach

        public override string Extension => ".html";

        public override string Name => "Unknown 9";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div id=\"transcript\">");
            sb.AppendLine("  <div name=\"transcriptText\" id=\"transcriptText\">");
            sb.AppendLine("    <div id=\"transcriptPanel\">");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine($"      <a class=\"caption\" starttime=\"{((long)(Math.Round(p.StartTime.TotalMilliseconds))).ToString(CultureInfo.InvariantCulture)}\" duration=\"{((long)(Math.Round(p.Duration.TotalMilliseconds))).ToString(CultureInfo.InvariantCulture)}\">{p.Text.Replace(Environment.NewLine, "<br />")}</a>");
            }
            sb.AppendLine("    </div>");
            sb.AppendLine("  </div>");
            sb.AppendLine("</div>");
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //<a class="caption" starttime="0" duration="16000">[♪techno music♪]</a>

            var temp = new StringBuilder();
            foreach (string l in lines)
            {
                temp.Append(l);
            }

            string all = temp.ToString();
            if (!all.Contains("class=\"caption\""))
            {
                return;
            }

            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                var indexOfStart = line.IndexOf("starttime=", StringComparison.Ordinal);
                var indexOfDuration = line.IndexOf("duration=", StringComparison.Ordinal);
                if (line.Contains("class=\"caption\"") && indexOfStart > 0 && indexOfDuration > 0)
                {
                    string startTime = "0";
                    int index = indexOfStart + 10;
                    while (index < line.Length && @"0123456789""'.,".Contains(line[index]))
                    {
                        if (@"0123456789,.".Contains(line[index]))
                        {
                            startTime += line[index];
                        }

                        index++;
                    }

                    string duration = "0";
                    index = indexOfDuration + 9;
                    while (index < line.Length && @"0123456789""'.,".Contains(line[index]))
                    {
                        if (@"0123456789,.".Contains(line[index]))
                        {
                            duration += line[index];
                        }

                        index++;
                    }

                    string text = string.Empty;
                    index = line.IndexOf('>', indexOfDuration);
                    if (index > 0 && index + 1 < line.Length)
                    {
                        text = line.Substring(index + 1).Trim();
                        index = text.IndexOf("</", StringComparison.Ordinal);
                        if (index > 0)
                        {
                            text = text.Substring(0, index);
                        }

                        text = text.Replace("<br />", Environment.NewLine);
                    }

                    long startMilliseconds;
                    long durationMilliseconds;
                    if (text.Length > 0 && long.TryParse(startTime, out startMilliseconds) && long.TryParse(duration, out durationMilliseconds))
                    {
                        subtitle.Paragraphs.Add(new Paragraph(text, startMilliseconds, startMilliseconds + durationMilliseconds));
                    }
                }
            }
            subtitle.Renumber();
        }

    }
}
