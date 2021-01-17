using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class HtmlSamiArray : SubtitleFormat
    {
        public override string Extension => ".html";

        public override string Name => "Html javascript sami array";

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        private static string ConvertJavaSpecialCharacters(string s)
        {
            if (s.Contains("&#"))
            {
                for (int i = 33; i < 255; i++)
                {
                    string tag = @"&#" + i + @";";
                    if (s.Contains(tag))
                    {
                        s = s.Replace(tag, Convert.ToChar(i).ToString());
                    }
                }
            }
            return s;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                var pos0 = line.IndexOf("[0] = ", StringComparison.Ordinal);
                var pos1 = line.IndexOf("[1] = ", StringComparison.Ordinal);
                var pos2 = line.IndexOf("[2] = ", StringComparison.Ordinal);
                if (pos0 >= 0 && pos1 >= 0 && pos2 >= 0)
                {
                    var p = new Paragraph();
                    var sb = new StringBuilder();

                    for (int i = pos0 + 6; i < line.Length && char.IsDigit(line[i]); i++)
                    {
                        sb.Append(line[i]);
                    }
                    p.StartTime.TotalMilliseconds = int.Parse(sb.ToString());

                    sb.Clear();
                    for (int i = pos1 + 7; i < line.Length && line[i] != '\''; i++)
                    {
                        sb.Append(line[i]);
                    }
                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }

                    for (int i = pos2 + 7; i < line.Length && line[i] != '\''; i++)
                    {
                        sb.Append(line[i]);
                    }
                    p.Text = sb.ToString().Trim();
                    p.Text = WebUtility.HtmlDecode(p.Text);
                    p.Text = ConvertJavaSpecialCharacters(p.Text);
                    subtitle.Paragraphs.Add(p);
                }
            }
            for (int i = 1; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.GetParagraphOrDefault(i - 1);
                Paragraph next = subtitle.GetParagraphOrDefault(i);
                if (p != null && next != null)
                {
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds;
                }

                if (!string.IsNullOrEmpty(next.Text))
                {
                    p.EndTime.TotalMilliseconds--;
                }
            }
            for (int i = subtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                Paragraph p = subtitle.GetParagraphOrDefault(i);
                if (p != null && string.IsNullOrEmpty(p.Text))
                {
                    subtitle.Paragraphs.RemoveAt(i);
                }
            }
            subtitle.Renumber();
        }
    }
}
