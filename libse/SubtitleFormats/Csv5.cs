using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Csv5 : SubtitleFormat
    {
        private const string Separator = ",";
        private static readonly Regex TimeCodeRegex = new Regex(@"^\d\d:\d\d:\d\d\.\d\d$", RegexOptions.Compiled);
        private static readonly char[] TimeCodeSplitChars = { ':', '.' };

        public override string Extension
        {
            get { return ".csv"; }
        }

        public override string Name
        {
            get { return "Csv5"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && !fileName.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > 0;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string format = "{1}{0}{2}{0}{3}";
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                string text = p.Text.Replace(Environment.NewLine, "///").Replace("\"", string.Empty);
                if (text.Contains(','))
                {
                    text = "\"" + text + "\"";
                }
                sb.AppendLine(string.Format(format, Separator, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), text));
            }
            return sb.ToString().Trim();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            foreach (string line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    var start = parts[0].Replace(" ", string.Empty).Replace("-", string.Empty);
                    var end = parts[1].Replace(" ", string.Empty).Replace("-", string.Empty);
                    string text = line.Remove(0, parts[0].Length + 1 + parts[1].Length).Replace("///", Environment.NewLine).Replace("\"", string.Empty).Trim();
                    if (text.StartsWith(",", StringComparison.Ordinal))
                    {
                        text = text.Remove(0, 1);
                        if (TimeCodeRegex.IsMatch(start) && TimeCodeRegex.IsMatch(end))
                        {
                            try
                            {
                                subtitle.Paragraphs.Add(new Paragraph(DecodeTimeCodeFrames(start, TimeCodeSplitChars), DecodeTimeCodeFrames(end, TimeCodeSplitChars), text));
                            }
                            catch
                            {
                                _errorCount++;
                            }
                        }
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    _errorCount++;
                    if (line.StartsWith("$", StringComparison.Ordinal))
                    {
                        _errorCount += 500;
                    }
                }
            }
            subtitle.Renumber();
        }

    }
}
