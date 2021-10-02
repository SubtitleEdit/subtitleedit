using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle104 : SubtitleFormat
    {
        public override string Extension => ".txt";
        public override string Name => "Unknown 104";
        private static readonly Regex RegexTimeCode = new Regex(@"\[\d\d:\d\d:\d\d\]", RegexOptions.Compiled);

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                sb.Append($"{EncodeTime(p.StartTime)} {p.Text} ");
            }
            sb.AppendLine();
            return sb.ToString();
        }

        private static string EncodeTime(TimeCode timeCode)
        {
            return "[" + timeCode.ToHHMMSS() + "]";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            var allText = string.Join(Environment.NewLine, lines);
            var matches = RegexTimeCode.Matches(allText);
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var timeCodeString = match.Value.Trim('[', ']');
                var ms = TimeCode.ParseHHMMSSToMilliseconds(timeCodeString);
                if (i < matches.Count - 1)
                {
                    var nextMatch = matches[i + 1];
                    var text = allText.Substring(match.Index + match.Value.Length, nextMatch.Index - match.Index - match.Value.Length).Trim();
                    if (text.Length > 0)
                    {
                        var p = new Paragraph(text, ms, ms);
                        subtitle.Paragraphs.Add(p);
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    var text = allText.Substring(match.Index + match.Value.Length).Trim();
                    if (text.Length > 0 && text.Length < 200)
                    {
                        var p = new Paragraph(text, ms, ms);
                        subtitle.Paragraphs.Add(p);
                    }
                }
            }

            subtitle.RecalculateDisplayTimes(Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds, null, Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds, true);
            subtitle.Renumber();
        }
    }
}
