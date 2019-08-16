using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonType6 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 6";

        private static readonly char[] CharSpace = { ' ' };

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.Append("{\"words\":[");
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];

                //split words
                string text = p.Text.Replace(Environment.NewLine, " ").Replace("  ", " ");
                var words = text.Split(CharSpace, StringSplitOptions.RemoveEmptyEntries);
                var times = GenerateTimes(words, text, p.StartTime, p.EndTime);
                for (int j = 0; j < words.Length; j++)
                {
                    sb.Append("[\"");
                    sb.Append(times[j]);
                    sb.Append("\",\"");
                    sb.Append(Json.EncodeJsonText(words[j]));
                    sb.Append("\"]");
                    sb.Append(',');
                }
                var next = subtitle.GetParagraphOrDefault(i + 1);
                if (next == null || next.StartTime.TotalMilliseconds - 200 < p.EndTime.TotalMilliseconds)
                {
                    sb.Append("[\"");
                    sb.Append(Convert.ToInt64(p.EndTime.TotalMilliseconds));
                    sb.Append("\",\"");
                    sb.Append("\"]");
                    sb.Append(',');
                }
            }
            return sb.ToString().Trim().TrimEnd(',') + "],\"paragraphs\":[],\"speakers\":{}}";
        }

        private static List<string> GenerateTimes(string[] words, string text, TimeCode start, TimeCode end)
        {
            var list = new List<string>();

            double total = 0.0;
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                double percent = (word.Length + 1.0) * 100 / text.Length;
                list.Add(Convert.ToInt64(start.TotalMilliseconds + (total * (end.TotalMilliseconds - start.TotalMilliseconds) / 100.0)).ToString());
                total += percent;
            }
            return list;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            foreach (string s in lines)
            {
                sb.Append(s);
            }

            string allText = sb.ToString();
            if (!allText.Contains("\"words\"") && !allText.Contains("'words'"))
            {
                return;
            }

            var words = Json.ReadArray(allText, "words");

            foreach (string word in words)
            {
                var elements = Json.ReadArray(word);
                if (elements.Count == 2)
                {
                    string milliseconds = elements[0].Trim('"').Trim();
                    string text = elements[1].Trim();
                    if (text.StartsWith('"'))
                    {
                        text = text.Remove(0, 1);
                    }

                    if (text.EndsWith('"'))
                    {
                        text = text.Remove(text.Length - 1, 1);
                    }

                    long number;
                    if (long.TryParse(milliseconds, out number))
                    {
                        subtitle.Paragraphs.Add(new Paragraph(text, number, number));
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }

            sb.Clear();
            var sub = new Subtitle();
            double startMilliseconds = 0;
            if (subtitle.Paragraphs.Count > 0)
            {
                startMilliseconds = subtitle.Paragraphs[0].StartTime.TotalMilliseconds;
            }

            for (int i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                Paragraph next = subtitle.Paragraphs[i + 1];
                Paragraph prev = subtitle.GetParagraphOrDefault(i - 1);
                if (sb.Length + p.Text.Length > (Configuration.Settings.General.SubtitleLineMaximumLength * 2) - 15) // text too big
                {
                    var newParagraph = new Paragraph(sb.ToString(), startMilliseconds, prev.EndTime.TotalMilliseconds);
                    sub.Paragraphs.Add(newParagraph);
                    sb.Clear();
                    if (!string.IsNullOrWhiteSpace(p.Text))
                    {
                        sb.Append(p.Text);
                        startMilliseconds = p.StartTime.TotalMilliseconds;
                    }
                }
                else if (next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds > 2000) // long time to next sub
                {
                    if (!string.IsNullOrWhiteSpace(p.Text))
                    {
                        sb.Append(' ');
                        sb.Append(p.Text);
                    }
                    var newParagraph = new Paragraph(sb.ToString(), startMilliseconds, next.StartTime.TotalMilliseconds);
                    sub.Paragraphs.Add(newParagraph);
                    sb.Clear();
                    startMilliseconds = next.StartTime.TotalMilliseconds;
                }
                else if (string.IsNullOrWhiteSpace(p.Text)) // empty text line
                {
                    if (string.IsNullOrWhiteSpace(next.Text) && sb.Length > 0)
                    {
                        var newParagraph = new Paragraph(sb.ToString(), startMilliseconds, next.StartTime.TotalMilliseconds);
                        sub.Paragraphs.Add(newParagraph);
                        sb.Clear();
                    }
                }
                else // just add word to current sub
                {
                    if (sb.Length == 0)
                    {
                        startMilliseconds = p.StartTime.TotalMilliseconds;
                    }

                    sb.Append(' ');
                    sb.Append(p.Text);

                }
            }
            if (sb.Length > 0)
            {
                var newParagraph = new Paragraph(sb.ToString().Trim(), startMilliseconds, Utilities.GetOptimalDisplayMilliseconds(sb.ToString()));
                sub.Paragraphs.Add(newParagraph);
            }

            subtitle.Paragraphs.Clear();
            foreach (Paragraph p in sub.Paragraphs)
            {
                p.Text = Utilities.AutoBreakLine(p.Text);
                subtitle.Paragraphs.Add(new Paragraph(p));
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}
