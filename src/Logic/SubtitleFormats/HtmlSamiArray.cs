using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class HtmlSamiArray : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".html"; }
        }

        public override string Name
        {
            get { return "Html javascript sami array"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > 0;
        }

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
                    string tag = "&#" + i.ToString() + ";";
                    if (s.Contains(tag))
                        s = s.Replace(tag, Convert.ToChar(i).ToString());
                }
            }
            return s;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (line.Contains("[0] = ") && line.Contains("[1] = '") && line.Contains("[2] = '"))
                {
                    var p = new Paragraph();
                    var sb = new StringBuilder();

                    int pos = line.IndexOf("[0] = ");
                    for (int i = pos + 6; i < line.Length && Utilities.IsInteger(line[i].ToString()); i++)
                    {
                        sb.Append(line.Substring(i, 1));
                    }
                    p.StartTime.TotalMilliseconds = int.Parse(sb.ToString());

                    pos = line.IndexOf("[1] = '");
                    sb = new StringBuilder();
                    for (int i = pos + 7; i<line.Length &&line[i] != '\''; i++)
                    {
                        sb.Append(line.Substring(i, 1));
                    }
                    if (sb.Length > 0)
                        sb.AppendLine();
                    pos = line.IndexOf("[2] = '");
                    for (int i = pos + 7; i<line.Length &&line[i] != '\''; i++)
                    {
                        sb.Append(line.Substring(i, 1));
                    }
                    p.Text = sb.ToString().Trim();
                    p.Text = Utilities.HtmlDecode(p.Text);
                    p.Text = ConvertJavaSpecialCharacters(p.Text);
                    subtitle.Paragraphs.Add(p);
                }
            }
            for (int i = 1; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.GetParagraphOrDefault(i - 1);
                Paragraph next = subtitle.GetParagraphOrDefault(i);
                if (p != null && next != null)
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds;
                if (!string.IsNullOrEmpty(next.Text))
                    p.EndTime.TotalMilliseconds--;
            }
            for (int i = subtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                Paragraph p = subtitle.GetParagraphOrDefault(i);
                if (p != null && string.IsNullOrEmpty(p.Text))
                    subtitle.Paragraphs.RemoveAt(i);
            }
            subtitle.Renumber(1);
        }
    }
}
