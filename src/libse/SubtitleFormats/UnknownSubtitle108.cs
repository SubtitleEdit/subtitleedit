using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Tab separated subtitle list (e.g. exported from a Word table), two rows per subtitle:
    /// 1	00:02:10:01	[5:02]	If a daughter or a grand-daughter,
    /// 	00:02:15:03	[]	under the age of 25, fit for marriage
    /// First row: number, start time code, optional duration (seconds:frames), first text line.
    /// Second row: end time code, second text line.
    /// </summary>
    public class UnknownSubtitle108 : SubtitleFormat
    {
        private static readonly Regex RegexStart = new Regex(@"^(\d+) *\t[\t ]*(\d\d:\d\d:\d\d[:.]\d\d)(.*)$", RegexOptions.Compiled);
        private static readonly Regex RegexEnd = new Regex(@"^[\t ]+(\d\d:\d\d:\d\d[:.]\d\d)(.*)$", RegexOptions.Compiled);
        private static readonly Regex RegexStartRest = new Regex(@"^[\t ]*(?:W(?=[\t ])[\t ]*)?(?:\d{1,2}:\d\d(?=\t|$))?(.*)$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 108";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            var number = 1;
            foreach (var p in subtitle.Paragraphs)
            {
                var lines = HtmlUtil.RemoveHtmlTags(p.Text, true).SplitToLines();
                string line1;
                string line2;
                if (lines.Count <= 1)
                {
                    line1 = string.Empty;
                    line2 = lines.Count == 1 ? lines[0].Trim() : string.Empty;
                }
                else
                {
                    line1 = lines[0].Trim();
                    line2 = string.Join(" ", lines.GetRange(1, lines.Count - 1)).Trim();
                }

                sb.AppendLine(number + "\t" + p.StartTime.ToHHMMSSFF() + "\t" + line1);
                sb.AppendLine("\t" + p.EndTime.ToHHMMSSFF() + "\t" + line2);
                sb.AppendLine("\t\t");
                number++;
            }

            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            Paragraph p = null;
            var expectEnd = false;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var startMatch = RegexStart.Match(line);
                if (startMatch.Success)
                {
                    if (expectEnd)
                    {
                        _errorCount++;
                    }

                    var restMatch = RegexStartRest.Match(startMatch.Groups[3].Value);
                    var text = restMatch.Success ? restMatch.Groups[1].Value.Trim() : startMatch.Groups[3].Value.Trim();
                    try
                    {
                        p = new Paragraph(DecodeTimeCode(startMatch.Groups[2].Value), new TimeCode(), text);
                        expectEnd = true;
                    }
                    catch
                    {
                        p = null;
                        expectEnd = false;
                        _errorCount++;
                    }

                    continue;
                }

                var endMatch = RegexEnd.Match(line);
                if (endMatch.Success && p != null && expectEnd)
                {
                    try
                    {
                        p.EndTime = DecodeTimeCode(endMatch.Groups[1].Value);
                    }
                    catch
                    {
                        _errorCount++;
                        p = null;
                        expectEnd = false;
                        continue;
                    }

                    var text = endMatch.Groups[2].Value.Trim();
                    if (text.Length > 0)
                    {
                        p.Text = p.Text.Length > 0 ? p.Text + Environment.NewLine + text : text;
                    }

                    // skip empty placeholder rows like "579	00:00:00:00"
                    if (p.Text.Length > 0 || p.StartTime.TotalMilliseconds > 0 || p.EndTime.TotalMilliseconds > 0)
                    {
                        subtitle.Paragraphs.Add(p);
                    }

                    p = null;
                    expectEnd = false;
                    continue;
                }

                if (subtitle.Paragraphs.Count == 0 && p == null && line.TrimStart().StartsWith("No.", StringComparison.Ordinal))
                {
                    continue; // header row: "No.	Timecode	Er.	Dur.	Subtitle text"
                }

                _errorCount++;
            }

            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string timeCode)
        {
            return DecodeTimeCodeFramesFourParts(timeCode.Split(':', '.'));
        }
    }
}
