using System;
using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class EdiusMarkerList2Ms : SubtitleFormat
    {
        public override string Extension => ".csv";

        public override string Name => "Edius Marker List 2 Ms";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine($@"# EDIUS Marker list
# Format Version 2
# Created Date : {DateTime.Now:ddd MMM dd HH:mm:ss yyy}
#
# No, Position, Duration, Comment");

            var number = 1;
            foreach (var p in subtitle.Paragraphs)
            {
                sb.Append(number);
                sb.Append(",\"");
                sb.Append(EncodeTimeCode(p.StartTime));
                sb.Append("\",\"");
                sb.Append(EncodeTimeCode(p.Duration));
                sb.Append("\",\"");
                sb.Append(p.Text.Replace("\"", "\\\""));
                sb.AppendLine("\"");
                number++;
            }

            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            return timeCode.ToString(false).Replace(',', ':');
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            subtitle.Paragraphs.Clear();
            lines.ForEach(line => sb.AppendLine(line));
            var text = sb.ToString();
            if (!text.Contains("# EDIUS Marker list") || !text.Contains("# Format Version 2"))
            {
                _errorCount++;
                return;
            }

            var continuation = false;
            var oldParts = new List<string>();
            foreach (var line in lines)
            {
                var s = line.Trim();
                if (s.StartsWith('#') || s.Length == 0)
                {
                    continue;
                }

                var parts = new List<string>();
                if (continuation)
                {
                    parts.AddRange(oldParts);
                    oldParts = new List<string>();

                    var extraParts = GetCsvParts(s, ref continuation);
                    if (parts.Count > 0 && extraParts.Count > 0)
                    {
                        parts[parts.Count - 1] += Environment.NewLine + extraParts[0];
                        extraParts.RemoveAt(0);
                    }
                    parts.AddRange(extraParts);
                }
                else
                {
                    parts.AddRange(GetCsvParts(s, ref continuation));
                }

                if (!continuation)
                {
                    if (parts.Count == 4 &&
                            ParseTimeCode(parts[1], out var startMs) &&
                            ParseTimeCode(parts[2], out var durationMs))
                    {
                        var p = new Paragraph(parts[3], startMs, startMs + durationMs);
                        subtitle.Paragraphs.Add(p);
                    }
                    else
                    {
                        _errorCount++;
                    }

                    oldParts = new List<string>();
                }
                else
                {
                    oldParts.AddRange(parts);
                }
            }

            subtitle.Renumber();
        }

        private static List<string> GetCsvParts(string s, ref bool continuation)
        {
            var list = new List<string>();
            var quoteOn = continuation;
            var i = 0;
            var sb = new StringBuilder();
            while (i < s.Length)
            {
                var ch = s[i];
                if (ch == ',' && !quoteOn)
                {
                    list.Add(sb.ToString());
                    sb.Clear();
                }
                else if (ch == '"' && !quoteOn)
                {
                    quoteOn = true;
                }
                else if (quoteOn && ch == '\\' && s.Substring(i).StartsWith("\\\""))
                {
                    sb.Append("\"");
                    i++;
                }
                else if (quoteOn && ch == '"')
                {
                    quoteOn = false;
                }
                else
                {
                    sb.Append(ch);
                }

                i++;
            }

            if (sb.Length > 0)
            {
                list.Add(sb.ToString());
            }

            continuation = quoteOn;
            return list;
        }

        private static bool ParseTimeCode(string tc, out double ms)
        {
            var arr = tc.Split(':', '.');
            if (arr.Length == 4 && arr[3].Length == 3 &&
                int.TryParse(arr[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var hours) &&
                int.TryParse(arr[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var minute) &&
                int.TryParse(arr[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var seconds) &&
                int.TryParse(arr[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var milliseconds))
            {
                var timeCode = new TimeCode(hours, minute, seconds, milliseconds);
                ms = timeCode.TotalMilliseconds;
                return true;
            }

            ms = 0;
            return false;
        }
    }
}
