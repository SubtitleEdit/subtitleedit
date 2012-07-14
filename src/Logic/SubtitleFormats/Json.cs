using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class Json : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".json"; }
        }

        public override string Name
        {
            get { return "JSON"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (count > 0)
                    sb.Append(",");
                sb.Append("{\"start\":");
                sb.Append(p.StartTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"end\":");
                sb.Append(p.EndTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"text\":\"");
                sb.Append(p.Text.Replace("\\", string.Empty).Replace("{", string.Empty).Replace("{", string.Empty).Replace("\"", "\\\"").Replace(Environment.NewLine, "<br />"));
                sb.Append("\"}");
                count++;
            }
            sb.Append("]");
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);
            if (!sb.ToString().Trim().StartsWith("[{\"start"))
                return;

            foreach (string line in sb.ToString().Replace("},{", Environment.NewLine).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                string s = line.Trim() + "}";
                string start = ReadTag(s, "start");
                string end = ReadTag(s, "end");
                string text = ReadTag(s, "text");
                if (start != null && end != null && text != null)
                {
                    double startSeconds;
                    double endSeconds;
                    if (double.TryParse(start, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out startSeconds) &&
                        double.TryParse(end, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out endSeconds) &&
                        text != null)
                    {
                        text = text.Replace("<br />", Environment.NewLine);
                        text = text.Replace("<br>", Environment.NewLine);
                        text = text.Replace("<br/>", Environment.NewLine);
                        text = text.Replace("\\n", Environment.NewLine);
                        subtitle.Paragraphs.Add(new Paragraph(text, startSeconds * 1000.0, endSeconds * 1000.0));
                    }
                    else
                    {
                        _errorCount ++;
                    }
                }
                else
                {
                    _errorCount ++;
                }
            }
            subtitle.Renumber(1);
        }

        public static string ConvertJsonSpecialCharacters(string s)
        {
            if (s.Contains("\\u00"))
            {
                for (int i = 33; i < 200; i++)
                {
                    string tag = "\\u" + i.ToString("X4").ToLower();
                    if (s.Contains(tag))
                        s = s.Replace(tag, Convert.ToChar(i).ToString());
                }
            }
            return s;
        }


        public static string ReadTag(string s, string tag)
        {
            int startIndex = s.IndexOf("\"" + tag + "\"");
            if (startIndex == -1)
                startIndex = s.IndexOf("'" + tag + "'");
            if (startIndex == -1)
                return null;
            string res = s.Substring(startIndex + 3 + tag.Length).Trim().TrimStart(':').TrimStart();
            if (res.StartsWith("\""))
            { // text
                res = Json.ConvertJsonSpecialCharacters(res);
                res = res.Replace("\\\"", "@__1");
                int endIndex = res.IndexOf("\"}");
                if (endIndex == -1)
                    endIndex = res.IndexOf("\",");
                if (endIndex == -1)
                    return null;
                if (res.Length > 1)
                    return res.Substring(1, endIndex - 1).Replace("@__1", "\"");
                return string.Empty;
            }
            else
            { // number
                int endIndex = res.IndexOf(",");
                if (endIndex == -1)
                    endIndex = res.IndexOf("}");
                if (endIndex == -1)
                    return null;
                return res.Substring(0, endIndex);
            }
        }

    }
}
