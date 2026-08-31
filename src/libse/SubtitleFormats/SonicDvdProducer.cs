using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Subtitle script for Sonic DVD Producer, as documented in its user guide:
    ///
    /// 1 00:01:00:00        00:01:19:00 Subtitle line 1
    ///                                  Subtitle line 2
    ///
    /// Line number, in point, out point and the text, padded into fixed columns so that a
    /// second text line lines up under the first one.
    /// </summary>
    public class SonicDvdProducer : SubtitleFormat
    {
        //                                                number     sep       in point               sep        out point              sep      text
        private static readonly Regex RegexEntry = new Regex(@"^[ \t]*(\d+)([ \t]+)(\d\d:\d\d:\d\d:\d\d)([ \t]+)(\d\d:\d\d:\d\d:\d\d)([ \t]*)(.*)$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Sonic DVD Producer";

        public override bool IsMine(List<string> lines, string fileName)
        {
            // "Adobe Encore w. line#" is the same layout with single spaces and is checked first,
            // so only claim the column-padded files DVD Producer actually writes - otherwise this
            // format would take over every Adobe Encore file with line numbers.
            var columnPadded = false;
            foreach (var line in lines)
            {
                var match = RegexEntry.Match(line);
                if (match.Success &&
                    (match.Groups[2].Value.Length > 1 || match.Groups[4].Value.Length > 1 || match.Groups[6].Value.Length > 1))
                {
                    columnPadded = true;
                    break;
                }
            }

            return columnPadded && base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            var index = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                index++;
                var prefix = $"{index,-4} {EncodeTimeCode(p.StartTime)}  {EncodeTimeCode(p.EndTime)}  ";
                var textLines = HtmlUtil.RemoveHtmlTags(p.Text, true).SplitToLines();
                for (var i = 0; i < textLines.Count; i++)
                {
                    sb.AppendLine((i == 0 ? prefix : new string(' ', prefix.Length)) + textLines[i]);
                }

                if (textLines.Count == 0)
                {
                    sb.AppendLine(prefix.TrimEnd());
                }
            }

            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:01:54:19
            return time.ToHHMMSSFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();

            Paragraph p = null;
            foreach (var line in lines)
            {
                var match = RegexEntry.Match(line);
                if (match.Success)
                {
                    var startParts = match.Groups[3].Value.Split(':');
                    var endParts = match.Groups[5].Value.Split(':');
                    p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), match.Groups[7].Value.Trim());
                    subtitle.Paragraphs.Add(p);
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // entries may be separated by blank lines
                }
                else if (p != null)
                {
                    p.Text = string.IsNullOrEmpty(p.Text) ? line.Trim() : p.Text + Environment.NewLine + line.Trim();
                }
                else
                {
                    _errorCount++;
                }
            }

            subtitle.Renumber();
        }
    }
}
