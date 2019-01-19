using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AdobeEncore : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d \d\d:\d\d:\d\d:\d\d ", RegexOptions.Compiled);
        private int _maxMsDiv10;

        public override string Extension => ".txt";

        public override string Name => "Adobe Encore";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();

            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            if (sb.ToString().Contains("#INPOINT OUTPOINT PATH"))
            {
                return false; // Pinnacle Impression
            }

            LoadSubtitle(subtitle, lines, fileName);

            bool containsNewLine = false;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (p.Text.Contains(Environment.NewLine))
                {
                    containsNewLine = true;
                }
            }
            if (sb.ToString().Contains("//") && !containsNewLine)
            {
                return false; // "DVD Subtitle System" format
            }

            if (_maxMsDiv10 > 90 && !containsNewLine)
            {
                return false; // "DVD Subtitle System" format (frame rate should not go higher than 90...)
            }

            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //00:03:15:22 00:03:23:10 This is line one.
                //This is line two.
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)} {EncodeTimeCode(p.EndTime)} {HtmlUtil.RemoveHtmlTags(p.Text, true)}");
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return time.ToHHMMSSFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:03:15:22 00:03:23:10 This is line one.
            //This is line two.
            Paragraph p = null;
            _maxMsDiv10 = 0;
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                Match match = RegexTimeCodes.Match(line);
                if (match.Success)
                {
                    try
                    {
                        string temp = line.Substring(0, match.Value.Length);
                        string start = temp.Substring(0, 11);
                        string end = temp.Substring(12, 11);

                        string[] startParts = start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                        string[] endParts = end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                        if (startParts.Length == 4 && endParts.Length == 4)
                        {
                            string text = line.Remove(0, RegexTimeCodes.Match(line).Length - 1).Trim();
                            p = new Paragraph(DecodeTimeCode(startParts), DecodeTimeCode(endParts), text);
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                    catch
                    {
                        _errorCount += 10;
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    if (string.IsNullOrEmpty(p.Text))
                    {
                        p.Text = line;
                    }
                    else
                    {
                        p.Text += Environment.NewLine + line;
                    }
                }
                else
                {
                    _errorCount++;
                }
            }

            subtitle.Renumber();
        }

        private TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            int frames = int.Parse(parts[3]);

            if (frames > _maxMsDiv10)
            {
                _maxMsDiv10 = frames;
            }

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(frames));
        }

    }
}
