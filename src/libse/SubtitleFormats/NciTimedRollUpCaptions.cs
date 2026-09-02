using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class NciTimedRollUpCaptions : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);
        private const int MaxDurationMs = 12000;

        private enum ExpectingLine
        {
            TimeCodes,
            Text
        }

        public override string Extension => ".flc";

        public override string Name => "NCI Timed Roll Up Captions";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName) && !fileName.EndsWith(".flc", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            for (int index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                Paragraph p = subtitle.Paragraphs[index];
                string text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                if (text.Trim().Length == 0)
                {
                    text = "\u0014\u002C";
                }
                else
                {
                    text = text.Replace(Environment.NewLine, "\u0011P");
                    text = text.Replace("♪", "\u00117");
                    text = text.Replace("'", "'\u0012\u0029");
                    text = "\u0014" + "\u0025" + "\u0014" + "\u002d" + text;
                }
                sb.AppendLine(p.StartTime.ToHHMMSSFF() + "\r\n" + text);

                Paragraph next = subtitle.GetParagraphOrDefault(index + 1);
                if (next == null || next.StartTime.TotalMilliseconds > p.StartTime.TotalMilliseconds + MaxDurationMs)
                {
                    sb.AppendLine(p.EndTime.ToHHMMSSFF() + "\r\n\u0014\u002C");
                }
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var paragraph = new Paragraph();
            var expecting = ExpectingLine.TimeCodes;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 4)
                    {
                        try
                        {
                            paragraph = new Paragraph();
                            int startHours = int.Parse(parts[0]);
                            int startMinutes = int.Parse(parts[1]);
                            int startSeconds = int.Parse(parts[2]);
                            int startFrames = int.Parse(parts[3]);
                            paragraph.StartTime = new TimeCode(startHours, startMinutes, startSeconds, FramesToMillisecondsMax999(startFrames));
                            expecting = ExpectingLine.Text;
                        }
                        catch
                        {
                            expecting = ExpectingLine.TimeCodes;
                        }
                    }
                }
                else
                {
                    if (expecting == ExpectingLine.Text || !string.IsNullOrEmpty(paragraph.Text))
                    {
                        if (line.Length > 0)
                        {
                            string text = line.Trim();

                            // The 0x14 control row is the "end of captions" marker ToText writes
                            // after the last cue; its time code is that cue's real end time, not
                            // the start of another caption. Treating it as a caption made the
                            // final cue end one minimum-gap early, and the shift accumulated.
                            if (text == "\u0014\u002C")
                            {
                                var lastParagraph = subtitle.Paragraphs.Count > 0
                                    ? subtitle.Paragraphs[subtitle.Paragraphs.Count - 1]
                                    : null;
                                if (lastParagraph != null)
                                {
                                    lastParagraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds;
                                }

                                expecting = ExpectingLine.TimeCodes;
                                continue;
                            }

                            paragraph.Text = (paragraph.Text + Environment.NewLine + text).Trim();
                            if (!subtitle.Paragraphs.Contains(paragraph))
                            {
                                subtitle.Paragraphs.Add(paragraph);
                            }

                            expecting = ExpectingLine.TimeCodes;
                        }
                    }
                }
            }
            for (int index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                Paragraph p = subtitle.Paragraphs[index];
                p.Text = FixText(p.Text);

                Paragraph next = subtitle.GetParagraphOrDefault(index + 1);
                if (next != null)
                {
                    if (next.StartTime.TotalMilliseconds - p.StartTime.TotalMilliseconds <= MaxDurationMs)
                    {
                        // A following entry with no text is the end-of-caption row ToText writes,
                        // and it carries this cue's exact end time. Subtracting the minimum gap
                        // from it made every cue slightly shorter on each save, accumulating.
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }
                    else
                    {
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text);
                    }
                }
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

        private static string FixText(string input)
        {
            var text = input.Replace("\u0011P", Environment.NewLine);
            text = text.Replace("\u00117", "♪");

            text = text.Replace("\u0012\u0029", string.Empty);

            text = text.Replace("\u0014\u0020", string.Empty);
            text = text.Replace("\u0014\u0021", string.Empty);
            text = text.Replace("\u0014\u0022", string.Empty);
            text = text.Replace("\u0014\u0023", string.Empty);
            text = text.Replace("\u0014\u0024", string.Empty);
            text = text.Replace("\u0014\u0025", string.Empty);
            text = text.Replace("\u0014\u0026", string.Empty);
            text = text.Replace("\u0014\u0027", string.Empty);
            text = text.Replace("\u0014\u0028", string.Empty);
            text = text.Replace("\u0014\u0029", string.Empty);
            text = text.Replace("\u0014\u002a", string.Empty);
            text = text.Replace("\u0014\u002b", string.Empty);
            text = text.Replace("\u0014\u002c", string.Empty);
            text = text.Replace("\u0014\u002d", string.Empty);
            text = text.Replace("\u0014\u002e", string.Empty);
            text = text.Replace("\u0014\u002f", string.Empty);
            return text.Trim();
        }

    }
}
