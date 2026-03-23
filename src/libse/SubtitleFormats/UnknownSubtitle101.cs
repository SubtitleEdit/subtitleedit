using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle101 : SubtitleFormat
    {
        public override string Extension => ".rtf";

        public override string Name => "Unknown 101";

        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d.\d\d\d ===> \d\d:\d\d:\d\d.\d\d\d\s{19,21}.+$", RegexOptions.Compiled);

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            const string writeFormat = "{0} ===> {1}                    {4}{2}{3}{2}";
            var last = subtitle.Paragraphs.Last();
            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var p = subtitle.Paragraphs[index];
                string count = (index + 1).ToString().PadLeft(4, '0');
                if (p == last)
                {
                    var lower = p.Text.ToLowerInvariant();
                    if (lower.Contains("author:") ||
                        lower.Contains("www.") ||
                        lower.Contains(".com") ||
                        lower.Contains(".org") ||
                        lower.Contains("text by") ||
                        lower.Contains("subtitles by") ||
                        lower.Contains("created by ") ||
                        lower.Contains("translated by "))
                    {
                        count = "CREDIT";
                    }
                }

                var start = p.StartTime.ToString().Replace(",", ".");
                var end = p.EndTime.ToString().Replace(",", ".");
                var text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                sb.AppendLine(string.Format(writeFormat, start, end, Environment.NewLine, text, count));
            }

            return sb.ToString().ToRtf();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.AppendLine(line);
            }

            string rtf = sb.ToString().Trim();
            if (!rtf.StartsWith("{\\rtf", StringComparison.Ordinal))
            {
                return;
            }

            lines = rtf.FromRtf().SplitToLines();
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            var timeCodeSplitChars = new[] { ':', '.' };
            foreach (string line in lines)
            {
                if (line.IndexOf(':') == 2 && RegexTimeCodes.IsMatch(line))
                {
                    string start = line.Substring(0, 12);
                    string end = line.Substring(18, 12);

                    string[] startParts = start.Split(timeCodeSplitChars, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(timeCodeSplitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 4 && endParts.Length == 4)
                    {
                        p = new Paragraph(DecodeTimeCodeMsFourParts(startParts), DecodeTimeCodeMsFourParts(endParts), string.Empty);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (line.Length == 0)
                {
                    // skip these lines
                }
                else if (line.Length > 0 && p != null)
                {
                    if (string.IsNullOrEmpty(p.Text))
                    {
                        p.Text = line;
                    }
                    else
                    {
                        if (Utilities.IsInteger(line))
                        {
                            _errorCount++;
                        }
                        else if (p.Text.Length > 2000)
                        {
                            _errorCount++;
                            return;
                        }
                        p.Text = p.Text.TrimEnd() + Environment.NewLine + line;
                    }
                }
            }
            subtitle.Renumber();
        }
    }
}
