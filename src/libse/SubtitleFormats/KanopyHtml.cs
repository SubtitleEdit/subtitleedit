using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class KanopyHtml : SubtitleFormat
    {
        //<a href='#' begin="5.706" end="8.289"><span class='ts'>00:05</span> (music)</a>
        //<a href='#' begin="13.037" end="14.961"><span class='ts'>00:13</span> - A Swiss scientist</a>
        //<a href='#' begin="14.961" end="17.128"><span class='ts'>00:14</span> had a marvelous statement,</a>

        public override string Extension => ".html";

        public override string Name => "Kanopy Html";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div>");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var start = $"{p.StartTime.Minutes:00}:{p.StartTime.Seconds:00}";
                if (p.StartTime.Hours > 0)
                {
                    start = $"{p.StartTime.Hours:00}:{start}";
                }

                sb.AppendLine($"      <a href='#' begin=\"{p.StartTime.TotalSeconds:0.000}\" end=\"{p.EndTime.TotalSeconds:0.000}\"><span class='ts'>{start}</span> {p.Text.Replace(Environment.NewLine, " <br />")}</a>");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var temp = new StringBuilder();
            foreach (string l in lines)
            {
                temp.Append(l);
            }

            string all = temp.ToString();
            if (!all.Contains(" begin=") || !all.Contains(" end=") || all.Contains("http://www.w3.org/ns/ttml") || all.Contains("http://www.w3.org/20"))
            {
                return;
            }

            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                var indexOfBegin = line.IndexOf("begin=", StringComparison.Ordinal);
                var indexOfEnd = line.IndexOf("end=", StringComparison.Ordinal);
                if (indexOfBegin > 0 && indexOfEnd > 0)
                {
                    string startTime = "0";
                    int index = indexOfBegin + 6;
                    while (index < line.Length && @"0123456789""'.".Contains(line[index]))
                    {
                        if ("0123456789.".Contains(line[index]))
                        {
                            startTime += line[index];
                        }

                        index++;
                    }

                    string end = "0";
                    index = indexOfEnd + 4;
                    while (index < line.Length && @"0123456789""'.".Contains(line[index]))
                    {
                        if ("0123456789.".Contains(line[index]))
                        {
                            end += line[index];
                        }

                        index++;
                    }

                    string text = string.Empty;
                    index = line.IndexOf("</span>", indexOfEnd, StringComparison.Ordinal);
                    if (index > 0 && index + 7 < line.Length)
                    {
                        text = line.Substring(index + 7).Trim().Replace("</p>", string.Empty);
                        index = text.IndexOf("</", StringComparison.Ordinal);
                        if (index > 0)
                        {
                            text = text.Substring(0, index);
                        }

                        text = text.Replace("<br />", Environment.NewLine);
                    }

                    double startSeconds;
                    double endSeconds;
                    if (text.Length > 0 && double.TryParse(startTime, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out startSeconds) &&
                                           double.TryParse(end, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out endSeconds))
                    {
                        subtitle.Paragraphs.Add(new Paragraph(text, startSeconds * TimeCode.BaseUnit, endSeconds * TimeCode.BaseUnit));
                    }
                }
            }
            subtitle.Renumber();
        }

    }
}
