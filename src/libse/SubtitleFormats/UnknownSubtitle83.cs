using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle83 : SubtitleFormat
    {

        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+\)\s+\d+:\d+:\d+:\d+\s+\d+:\d+:\d+:\d+\s+Durée\s+\d+:\d+\s+Lis\s+:\s+\d+\s+Nbc\s+:\s+\d+$", RegexOptions.Compiled);

        public override string Extension => ".rtf";

        public override string Name => "Unknown 83";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string format = "{0}) {1} {2} Durée {3} Lis : {4} Nbc : {5}" + Environment.NewLine + "{6}" + Environment.NewLine;
            var sb = new StringBuilder();
            for (int index = 0; index < subtitle.Paragraphs.Count;)
            {
                var p = subtitle.Paragraphs[index++];
                var lis = (int)Math.Round(MillisecondsToFrames(p.Duration.TotalMilliseconds) / 2.0);
                sb.AppendLine(string.Format(format, index, p.StartTime.ToHHMMSSFF(), p.EndTime.ToHHMMSSFF(), p.Duration.ToSSFF(), lis, p.Text.Length, p.Text));
            }
            return sb.ToString().ToRtf();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string line in lines)
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
            foreach (var line in lines)
            {
                if (RegexTimeCodes.IsMatch(line)) // 1) 00:00:08:02 00:00:10:10 Durée 02:08 Lis : 28 Nbc : 19
                {
                    if (p != null)
                    {
                        subtitle.Paragraphs.Add(p);
                    }
                    try
                    {
                        var timeCodes = line.Substring(line.IndexOf(")", StringComparison.Ordinal) + 1, line.IndexOf("Durée", StringComparison.Ordinal) - 2).Trim().Split(" \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        p = new Paragraph(string.Empty, TimeCode.ParseHHMMSSFFToMilliseconds(timeCodes[0]), TimeCode.ParseHHMMSSFFToMilliseconds(timeCodes[1]));
                    }
                    catch (Exception)
                    {
                        _errorCount++;
                    }
                }
                else if (p != null && p.Text.Length < 1000)
                {
                    p.Text = (p.Text + Environment.NewLine + line).Trim();
                }
                else
                {
                    _errorCount++;
                    if (_errorCount > 50)
                    {
                        return;
                    }
                }
            }
            if (p != null)
            {
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
        }

    }
}
