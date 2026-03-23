using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Implements https://web.archive.org/web/20080213213927/http://trac.annodex.net/wiki/PJS
    /// https://github.com/SubtitleEdit/subtitleedit/pull/1964#issuecomment-249379407
    /// </summary>
    public class PhoenixSubtitle : SubtitleFormat
    {
        //2447,   2513, "You should come to the Drama Club, too."
        //2513,   2594, "Yeah. The Drama Club is worried|that you haven't been coming."
        //2603,   2675, "I see. Sorry, I'll drop by next time."

        private static readonly Regex RegexTimeCodes = new Regex(@"^(\d+),\s*(\d+),", RegexOptions.Compiled);
        private static readonly char[] TrimChars = { ' ', '"' };

        public override string Extension => ".pjs";

        public override bool IsTimeBased => false;

        public override string Name => "Phoenix Subtitle";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName?.EndsWith(".pjs", StringComparison.OrdinalIgnoreCase) == false)
            {
                return false;
            }
            return base.IsMine(lines, fileName);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                // too short line
                if (line.Length < 4)
                {
                    _errorCount++;
                    continue;
                }

                var match = RegexTimeCodes.Match(line);
                if (match.Success)
                {
                    try
                    {
                        var startMs = (double)FramesToMilliseconds(int.Parse(match.Groups[1].Value));
                        var endMs = (double)FramesToMilliseconds(int.Parse(match.Groups[2].Value));
                        var paragraph = new Paragraph
                        {
                            Number = subtitle.Paragraphs.Count + 1,
                            StartTime = new TimeCode(startMs),
                            EndTime = new TimeCode(endMs)
                        };

                        // Decode text.
                        line = line.Substring(match.Value.Length).Trim();
                        // Read quote (") if there are more than one.
                        if (line.Length > 2 && line[0] == '"' && line[1] == '"')
                        {
                            line = "\"" + line.Trim(TrimChars) + "\"";
                        }
                        else
                        {
                            line = line.Trim(TrimChars);
                        }

                        paragraph.Text = string.Join(Environment.NewLine, line.Split('|'));
                        subtitle.Paragraphs.Add(paragraph);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    _errorCount++;
                }
            }
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string writeFormat = "{0},{1},\"{2}\"{3}";
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                string text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                // Pipe character for forced line breaks.
                text = text.Replace(Environment.NewLine, "|");
                sb.AppendFormat(writeFormat, MillisecondsToFrames(p.StartTime.TotalMilliseconds), MillisecondsToFrames(p.EndTime.TotalMilliseconds), text, Environment.NewLine);
            }
            return sb.ToString();
        }

    }
}
