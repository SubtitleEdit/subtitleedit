using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Edl : SubtitleFormat
    {
        private static readonly Regex Regex = new Regex(@"^\d+\s+[a-zA-Z0-9_.-]{2,250}\s+[A-Z]\s+[A-Z]\s+\d\d:\d\d:\d\d:\d\d\s+\d\d:\d\d:\d\d:\d\d\s+\d\d:\d\d:\d\d:\d\d\s+\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);
        private const string TextPrefix = "* FROM CLIP NAME: ";

        public override string Extension => ".edl";

        public override string Name => "EDL";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("TITLE: " + title);
            // Time codes are written with ':' separators and no drop-frame arithmetic, which
            // is non-drop time code at every frame rate (the old header said DROP FRAME for
            // integer rates - exactly backwards, and NLEs trust this line).
            sb.AppendLine("FCM: NON-DROP FRAME");

            sb.AppendLine();
            const string writeFormat = "{0:000000}  {1}       {2}     {3}        {4} {5} {6} {7}";
            var eventNumber = 0;
            for (int index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var p = subtitle.Paragraphs[index];
                if (index == 0 && p.StartTime.TotalSeconds > 1)
                {
                    var start = new TimeCode(p.StartTime.TotalMilliseconds - 1000.0);
                    var end = new TimeCode(p.StartTime.TotalMilliseconds - 1);
                    eventNumber++;
                    sb.AppendLine(string.Format(writeFormat, eventNumber, "BL", "V", "C", EncodeTimeCode(start), EncodeTimeCode(end), EncodeTimeCode(start), EncodeTimeCode(end)));
                    sb.AppendLine();
                }
                var text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                eventNumber++;
                sb.AppendLine(string.Format(writeFormat, eventNumber, "AX", "V", "C", EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime)));
                sb.AppendLine(TextPrefix + text);
                sb.AppendLine();
                var next = subtitle.GetParagraphOrDefault(index + 1);
                if (next != null && next.StartTime.TotalMilliseconds > p.EndTime.TotalMilliseconds + 100)
                {
                    var start = new TimeCode(p.EndTime.TotalMilliseconds + 1);
                    var end = new TimeCode(start.TotalMilliseconds + 1000);
                    if (end.TotalMilliseconds >= next.StartTime.TotalMilliseconds)
                    {
                        end = new TimeCode(next.StartTime.TotalMilliseconds - 1);
                    }
                    eventNumber++;
                    sb.AppendLine(string.Format(writeFormat, eventNumber, "BL", "V", "C", EncodeTimeCode(start), EncodeTimeCode(end), EncodeTimeCode(start), EncodeTimeCode(end)));
                    sb.AppendLine();
                }
            }
            return sb.ToString().Trim() + Environment.NewLine;
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            return $"{timeCode.Hours:00}:{timeCode.Minutes:00}:{timeCode.Seconds:00}:{MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {   //002  AX       V     C        01:00:01:15 01:00:04:18 00:00:01:15 00:00:04:18
            //000002  AX V     C        01:00:04:00 01:00:05:00 00:00:02:05 00:00:03:05
            _errorCount = 0;
            Paragraph lastParagraph = null;
            int count = 0;
            var splitChar = new[] { ' ' };
            foreach (string rawLine in lines)
            {
                // NLEs pad event rows with trailing spaces (Premiere, Avid, Nucoda all do)
                var line = rawLine.TrimEnd();
                bool isTimeCode = false;
                if (line.Length > 0)
                {
                    bool success = false;
                    if (IsSkippableComment(line))
                    {
                        count++;
                        continue;
                    }

                    if (line.Length > 65 && line.Length < 500 && line.IndexOf(':') > 20)
                    {
                        var match = Regex.Match(line);
                        if (match.Success)
                        {
                            isTimeCode = true;
                            if (lastParagraph != null && Math.Abs(lastParagraph.StartTime.TotalMilliseconds + 1) > 0.001)
                            {
                                subtitle.Paragraphs.Add(lastParagraph);
                            }

                            var arr = line.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                            try
                            {
                                if (arr.Length == 8 && arr[1] != "BL")
                                {
                                    var start = DecodeTimeCodeFrames(arr[6], SplitCharColon);
                                    var end = DecodeTimeCodeFrames(arr[7], SplitCharColon);
                                    lastParagraph = new Paragraph(start, end, string.Empty);
                                    success = true;
                                }
                                else
                                {
                                    lastParagraph = new Paragraph(string.Empty, -1, -1);
                                }
                            }
                            catch
                            {
                                _errorCount++;
                            }
                        }
                    }
                    if (!isTimeCode && !string.IsNullOrWhiteSpace(line) && lastParagraph != null && Utilities.GetNumberOfLines(lastParagraph.Text) < 5)
                    {
                        lastParagraph.Text = (lastParagraph.Text + Environment.NewLine + line).Trim();
                        success = true;
                    }
                    if (!success && count > 9)
                    {
                        _errorCount++;
                    }
                }
                count++;
            }
            if (lastParagraph != null)
            {
                subtitle.Paragraphs.Add(lastParagraph);
            }
            foreach (var paragraph in subtitle.Paragraphs)
            {
                paragraph.Text = StripClipNamePrefix(paragraph.Text);
            }

            subtitle.Renumber();
        }

        /// <summary>
        /// EDL comment/metadata lines that must not leak into the cue text: any "*" comment
        /// except the clip name (Avid writes "* FROM CLIP: path", Nucoda "* FROM FILE: path",
        /// screening EDLs "*SOURCE FILE: ..."), and M2 motion-memory lines.
        /// </summary>
        private static bool IsSkippableComment(string line)
        {
            if (line.StartsWith("M2", StringComparison.Ordinal) && line.Length > 2 && line[2] == ' ')
            {
                return true;
            }

            if (!line.StartsWith('*'))
            {
                return false;
            }

            var body = line.TrimStart('*').TrimStart();
            return !body.StartsWith("FROM CLIP NAME:", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The clip name line appears in the wild as "* FROM CLIP NAME: ", "*FROM CLIP NAME: "
        /// and bare "FROM CLIP NAME: ".
        /// </summary>
        private static string StripClipNamePrefix(string text)
        {
            var s = text.TrimStart('*').TrimStart();
            if (s.StartsWith("FROM CLIP NAME:", StringComparison.OrdinalIgnoreCase))
            {
                return s.Remove(0, "FROM CLIP NAME:".Length).TrimStart();
            }

            return text;
        }

    }
}