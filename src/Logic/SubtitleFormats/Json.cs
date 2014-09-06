﻿using System;
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

        public static string EncodeJsonText(string text)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                string s = text.Substring(i, 1);
                if (s == "\"")
                {
                    sb.Append("\\\"");
                }
                else if (s == "\\")
                {
                    sb.Append("\\\\");
                }
                else
                {
                    sb.Append(s);
                }
            }
            return sb.ToString().Replace(Environment.NewLine, "<br />");
        }

        public static string DecodeJsonText(string text)
        {
            var sb = new StringBuilder();
            text = text.Replace("<br />", Environment.NewLine);
            text = text.Replace("<br>", Environment.NewLine);
            text = text.Replace("<br/>", Environment.NewLine);
            text = text.Replace("\\n", Environment.NewLine);
            bool keepNext = false;
            for (int i = 0; i < text.Length; i++)
            {
                string s = text.Substring(i, 1);
                if (s == "\\" && !keepNext)
                {
                    keepNext = true;
                }
                else
                {
                    sb.Append(s);
                    keepNext = false;
                }
            }
            return sb.ToString();
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
                sb.Append(EncodeJsonText(p.Text));
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
                        subtitle.Paragraphs.Add(new Paragraph(DecodeJsonText(text), startSeconds * 1000.0, endSeconds * 1000.0));
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
            int startIndex = s.IndexOf("\"" + tag + "\"", StringComparison.Ordinal);
            if (startIndex == -1)
                startIndex = s.IndexOf("'" + tag + "'", StringComparison.Ordinal);
            if (startIndex == -1)
                return null;
            string res = s.Substring(startIndex + 3 + tag.Length).Trim().TrimStart(':').TrimStart();
            if (res.StartsWith("\""))
            { // text
                res = Json.ConvertJsonSpecialCharacters(res);
                res = res.Replace("\\\"", "@__1");
                int endIndex = res.IndexOf("\"}", StringComparison.Ordinal);
                int endAlternate = res.IndexOf("\",", StringComparison.Ordinal);
                if (endIndex == -1)
                    endIndex = endAlternate;
                else if (endAlternate > 0 && endAlternate < endIndex)
                    endIndex = endAlternate;
                if (endIndex == -1)
                    return null;
                if (res.Length > 1)
                    return res.Substring(1, endIndex - 1).Replace("@__1", "\\\"");
                return string.Empty;
            }
            else
            { // number
                int endIndex = res.IndexOf(',');
                if (endIndex == -1)
                    endIndex = res.IndexOf('}');
                if (endIndex == -1)
                    return null;
                return res.Substring(0, endIndex);
            }
        }

        public static List<string> ReadArray(string s, string tag)
        {
            var list = new List<string>();

            int startIndex = s.IndexOf("\"" + tag + "\"", StringComparison.Ordinal);
            if (startIndex == -1)
                startIndex = s.IndexOf("'" + tag + "'", StringComparison.Ordinal);
            if (startIndex == -1)
                return list;

            startIndex += tag.Length + 4;

            string res = s.Substring(startIndex);

            int tagLevel = 1;
            int nextTag = 0;
            int oldStart = 0;
            while (tagLevel >= 1 && nextTag >= 0 && nextTag+1 < res.Length)
            {
                if (res.Substring(oldStart, 1) == "\"")
                {
                    nextTag = res.IndexOf('"', oldStart + 1);

                    while (nextTag > 0 && nextTag + 1 < res.Length &&  res.Substring(nextTag - 1, 1) == "\\")
                        nextTag = res.IndexOf('"', nextTag + 1);

                    if (nextTag > 0)
                    {
                        string newValue = res.Substring(oldStart, nextTag - oldStart);
                        list.Add(newValue.Remove(0, 1));
                        oldStart = nextTag + 2;
                    }
                }
                else if (res.Substring(oldStart, 1) != "[" && res.Substring(oldStart, 1) != "]")
                {
                    nextTag = res.IndexOf(',', oldStart + 1);
                    if (nextTag > 0)
                    {
                        string newValue = res.Substring(oldStart, nextTag - oldStart);
                        if (newValue.EndsWith("]"))
                        {
                            newValue = newValue.TrimEnd(']');
                            tagLevel = -10; // return
                        }
                        list.Add(newValue.Trim());
                        oldStart = nextTag + 1;
                    }
                }
                else
                {
                    int nextBegin = res.IndexOf('[', nextTag);
                    int nextEnd = res.IndexOf(']', nextTag);
                    if (nextBegin < nextEnd && nextBegin != -1)
                    {
                        nextTag = nextBegin + 1;
                        tagLevel++;
                    }
                    else
                    {
                        nextTag = nextEnd + 1;
                        tagLevel--;
                        if (tagLevel == 1)
                        {
                            string newValue = res.Substring(oldStart, nextTag - oldStart);
                            list.Add(newValue);
                            if (res.Substring(nextTag, 1) == "]")
                                tagLevel--;
                            oldStart = nextTag + 1;
                        }
                    }
                }

            }
            return list;
        }

        internal static List<string> ReadArray(string text)
        {
            var list = new List<string>();
            text = text.Trim();
            if (text.StartsWith("[") && text.EndsWith("]"))
            {
                text = text.Trim('[');
                text = text.Trim(']');
                text = text.Trim();

                text = text.Replace("<br />", Environment.NewLine);
                text = text.Replace("<br>", Environment.NewLine);
                text = text.Replace("<br/>", Environment.NewLine);
                text = text.Replace("\\n", Environment.NewLine);

                bool keepNext = false;
                var sb = new StringBuilder();
                for (int i = 0; i < text.Length; i++)
                {
                    string s = text.Substring(i, 1);
                    if (s == "\\" && !keepNext)
                    {
                        keepNext = true;
                    }
                    else if (!keepNext && s == ",")
                    {
                        list.Add(sb.ToString());
                        sb = new StringBuilder();
                        keepNext = false;
                    }
                    else
                    {
                        sb.Append(s);
                        keepNext = false;
                    }
                }
                if (sb.Length > 0)
                    list.Add(sb.ToString());
            }
            return list;
        }

    }
}
