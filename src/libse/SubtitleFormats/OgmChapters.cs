using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class OgmChapters : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^CHAPTER\d+=\d\d:\d\d:\d\d[.,]\d\d\d$", RegexOptions.Compiled);
        private static readonly Regex RegexChapterNames = new Regex(@"^CHAPTER\d+NAME=[^\r\n]+$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "OGM Chapters";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var p = subtitle.Paragraphs[index];
                sb.AppendLine($"CHAPTER{index + 1:00}={EncodeTimeCode(p.StartTime)}");
                sb.AppendLine($"CHAPTER{index + 1:00}NAME={HtmlUtil.RemoveHtmlTags(p.Text, true).Replace(Environment.NewLine, " ")}");
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return time.ToString().Replace(",", ".");
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (var line in lines)
            {
                var s = line.Trim();
                if (s.StartsWith("CHAPTER", StringComparison.Ordinal))
                {
                    if (RegexTimeCodes.IsMatch(s))
                    {
                        var arr = line.Split('=');
                        if (arr.Length == 2)
                        {
                            string start = arr[1];
                            string[] startParts = start.Split(new[] { ':', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);
                            if (startParts.Length == 4)
                            {
                                p = new Paragraph { StartTime = DecodeTimeCodeMsFourParts(startParts) };
                                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 4000;
                                subtitle.Paragraphs.Add(p);
                            }
                        }
                    }
                    else if (p != null && RegexChapterNames.IsMatch(s))
                    {
                        p.Text = s.Remove(0, s.IndexOf('=') + 1).Trim();
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }
            subtitle.Renumber();
        }
    }
}