using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Json : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON";

        public static string EncodeJsonText(string text, string newLineCharacter = "<br />")
        {
            var sb = new StringBuilder(text.Length);
            foreach (var c in text)
            {
                switch (c)
                {
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '"':
                        sb.Append("\\\"");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString().Replace(Environment.NewLine, newLineCharacter);
        }

        public static string DecodeJsonText(string text)
        {
            text = text.Replace("<br />", Environment.NewLine);
            text = text.Replace("<br>", Environment.NewLine);
            text = text.Replace("<br/>", Environment.NewLine);
            text = text.Replace("\\n", Environment.NewLine);
            bool keepNext = false;
            var sb = new StringBuilder(text.Length);
            foreach (var c in text)
            {
                if (c == '\\' && !keepNext)
                {
                    keepNext = true;
                }
                else
                {
                    sb.Append(c);
                    keepNext = false;
                }
            }
            return sb.ToString();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder(@"[");
            int count = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.Append(',');
                }

                sb.Append("{\"start\":");
                sb.Append(p.StartTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"end\":");
                sb.Append(p.EndTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"text\":\"");
                sb.Append(EncodeJsonText(p.Text));
                sb.Append("\"}");
                count++;
            }
            sb.Append(']');
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            foreach (var s in lines)
            {
                sb.Append(s);
            }

            if (!sb.ToString().TrimStart().StartsWith("[{\"", StringComparison.Ordinal))
            {
                return;
            }

            foreach (string line in sb.ToString().Replace("},{", Environment.NewLine).SplitToLines())
            {
                string s = line.Trim() + "}";
                string start = ReadTag(s, "start");
                string end = ReadTag(s, "end");
                string text = ReadTag(s, "text");
                if (start != null && end != null && text != null && !IsTagArray(s, "text"))
                {
                    if (double.TryParse(start, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var startSeconds) &&
                        double.TryParse(end, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var endSeconds))
                    {
                        subtitle.Paragraphs.Add(new Paragraph(DecodeJsonText(text), startSeconds * TimeCode.BaseUnit, endSeconds * TimeCode.BaseUnit));
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
            subtitle.Renumber();
        }

        private static bool IsTagArray(string content, string tag)
        {
            var startIndex = content.IndexOfAny(new[] { "\"" + tag + "\"", "'" + tag + "'" }, StringComparison.Ordinal);
            if (startIndex < 0)
            {
                return false;
            }

            return content.Substring(startIndex + 3 + tag.Length).Trim().TrimStart(':').TrimStart().StartsWith('[');
        }

        public static string ConvertJsonSpecialCharacters(string s)
        {
            if (s.Contains("\\u00"))
            {
                for (int i = 33; i < 200; i++)
                {
                    var tag = "\\u" + i.ToString("x4");
                    if (s.Contains(tag))
                    {
                        s = s.Replace(tag, Convert.ToChar(i).ToString());
                    }
                }
            }
            return s;
        }

        private static readonly char[] CommaAndEndCurlyBracket = { ',', '}' };

        public static string ReadTag(string s, string tag)
        {
            var startIndex = s.IndexOfAny(new[] { "\"" + tag + "\"", "'" + tag + "'" }, StringComparison.Ordinal);
            if (startIndex < 0)
            {
                return null;
            }

            var res = s.Substring(startIndex + 3 + tag.Length).Trim().TrimStart(':').TrimStart();
            if (res.StartsWith('"'))
            { // text
                res = ConvertJsonSpecialCharacters(res);
                res = res.Replace("\\\"", "@__1");
                int endIndex = res.IndexOf("\"}", StringComparison.Ordinal);
                if (endIndex == -1)
                {
                    endIndex = res.LastIndexOf('"');
                }
                int endAlternate = res.IndexOf("\",", StringComparison.Ordinal);
                if (endIndex < 0)
                {
                    endIndex = endAlternate;
                }
                else if (endAlternate > 0 && endAlternate < endIndex)
                {
                    endIndex = endAlternate;
                }

                if (endIndex < 0 && res.EndsWith("\"", StringComparison.Ordinal))
                {
                    endIndex = res.Length - 1;
                }

                if (endIndex <= 0)
                {
                    return null;
                }

                if (res.Length > 1)
                {
                    return res.Substring(1, endIndex - 1).Replace("@__1", "\\\"");
                }

                return string.Empty;
            }
            else
            { // number
                var endIndex = res.IndexOfAny(CommaAndEndCurlyBracket);
                if (endIndex < 0)
                {
                    return null;
                }

                return res.Substring(0, endIndex);
            }
        }

        public static List<string> ReadArray(string s, string tag)
        {
            var list = new List<string>();

            var startIndex = s.IndexOfAny(new[] { "\"" + tag + "\"", "'" + tag + "'" }, StringComparison.Ordinal);
            if (startIndex < 0)
            {
                return list;
            }

            startIndex += tag.Length + 2;
            string res = s.Substring(startIndex).TrimStart().TrimStart(':').TrimStart();
            int tagLevel = 1;
            int oldStart = 0;
            if (oldStart < res.Length && res[oldStart] == '[')
            {
                oldStart++;
            }
            int nextTag = oldStart;
            while (tagLevel >= 1 && nextTag >= 0 && nextTag + 1 < res.Length)
            {
                while (oldStart < res.Length && res[oldStart] == ' ')
                {
                    oldStart++;
                }

                if (oldStart < res.Length && res[oldStart] == '"')
                {
                    nextTag = res.IndexOf('"', oldStart + 1);

                    while (nextTag > 0 && nextTag + 1 < res.Length && res[nextTag - 1] == '\\')
                    {
                        nextTag = res.IndexOf('"', nextTag + 1);
                    }

                    if (nextTag > 0)
                    {
                        string newValue = res.Substring(oldStart, nextTag - oldStart);
                        list.Add(newValue.Remove(0, 1));
                        oldStart = nextTag + 1;
                        while (oldStart < res.Length && "\r\n\t ".Contains(res[oldStart]))
                        {
                            oldStart++;
                        }
                        if (oldStart < res.Length && res[oldStart] == ']')
                        {
                            oldStart++;
                        }
                        while (oldStart < res.Length && "\r\n\t ".Contains(res[oldStart]))
                        {
                            oldStart++;
                        }
                        if (oldStart < res.Length && res[oldStart] == ',')
                        {
                            oldStart++;
                        }
                        while (oldStart < res.Length && "\r\n\t ".Contains(res[oldStart]))
                        {
                            oldStart++;
                        }
                        if (oldStart < res.Length && res[oldStart] == '[')
                        {
                            oldStart++;
                        }
                        while (oldStart < res.Length && "\r\n\t ".Contains(res[oldStart]))
                        {
                            oldStart++;
                        }
                    }
                }
                else if (oldStart < res.Length && res[oldStart] != '[' && res[oldStart] != ']')
                {
                    nextTag = res.IndexOf(',', oldStart + 1);
                    if (nextTag > 0)
                    {
                        string newValue = res.Substring(oldStart, nextTag - oldStart);
                        if (newValue.EndsWith(']'))
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
                            if (res[nextTag] == ']')
                            {
                                tagLevel--;
                            }

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
            if (text.StartsWith('[') && text.EndsWith(']'))
            {
                text = text.Trim('[', ']');
                text = text.Trim();

                text = text.Replace("<br />", Environment.NewLine);
                text = text.Replace("<br>", Environment.NewLine);
                text = text.Replace("<br/>", Environment.NewLine);
                text = text.Replace("\\n", Environment.NewLine);

                bool keepNext = false;
                var sb = new StringBuilder();
                foreach (var c in text)
                {
                    if (c == '\\' && !keepNext)
                    {
                        keepNext = true;
                    }
                    else if (!keepNext && c == ',')
                    {
                        list.Add(sb.ToString());
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(c);
                        keepNext = false;
                    }
                }
                if (sb.Length > 0)
                {
                    list.Add(sb.ToString());
                }
            }
            return list;
        }

        public static List<string> ReadObjectArray(string text)
        {
            var list = new List<string>();
            text = text.Trim();
            if (text.StartsWith('[') && text.EndsWith(']'))
            {
                text = text.Trim('[', ']').Trim();
                int onCount = 0;
                bool keepNext = false;
                var sb = new StringBuilder();
                foreach (var c in text)
                {
                    if (keepNext)
                    {
                        sb.Append(c);
                        keepNext = false;
                    }
                    else if (c == '\\')
                    {
                        sb.Append(c);
                        keepNext = true;
                    }
                    else if (c == '{')
                    {
                        sb.Append(c);
                        onCount++;
                    }
                    else if (c == '}')
                    {
                        sb.Append(c);
                        onCount--;
                    }
                    else if (c == ',' && onCount == 0)
                    {
                        list.Add(sb.ToString().Trim());
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                if (sb.Length > 0)
                {
                    list.Add(sb.ToString().Trim());
                }
            }
            return list;
        }
    }
}
