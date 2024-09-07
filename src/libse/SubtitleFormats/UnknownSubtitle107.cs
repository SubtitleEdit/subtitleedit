using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle107 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d;\d\d;\d\d;\d\d \d\d;\d\d;\d\d;\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 107";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("@ This file written with Closed Caption Converter V3");
            sb.AppendLine();
            sb.AppendLine("<begin subtitles>");
            const string writeFormat = "{0} {1}{2}{3}{2}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(writeFormat, EncodeTimeCode(p.StartTime), EncodeEndTimeCode(p.EndTime), Environment.NewLine, HtmlUtil.RemoveHtmlTags(p.Text, true)));
            }
            sb.AppendLine("<end subtitles>");
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return time.ToHHMMSSFF().Replace(":", ";");
        }

        private static string EncodeEndTimeCode(TimeCode time)
        {
            //00;50;39;13 (last is frame)

            //Bugfix for Avid - On 23.976 FPS and 24 FPS projects, when the End time of a subtitle ends in 02, 07, 12, 17, 22, 27 frames, the subtitle won't import.
            if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 23.976) < 0.01 ||
                Math.Abs(Configuration.Settings.General.CurrentFrameRate - 24) < 0.01)
            {
                var frames = MillisecondsToFramesMaxFrameRate(time.Milliseconds);
                if (frames == 2 || frames == 7 || frames == 12 || frames == 17 || frames == 22 || frames == 27)
                {
                    frames--;
                }

                return $"{time.Hours:00};{time.Minutes:00};{time.Seconds:00};{frames:00}";
            }

            return EncodeTimeCode(time);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            var beginFound = false;
            var endFound = false;
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.Equals("<begin subtitles>", StringComparison.OrdinalIgnoreCase))
                {
                    beginFound = true;
                }
                else if (trimmedLine.Equals("<end subtitles>", StringComparison.OrdinalIgnoreCase))
                {
                    endFound = true;
                    break;
                }

                if (line.IndexOf(';') == 2 && RegexTimeCodes.IsMatch(line))
                {
                    var temp = line.Substring(0, RegexTimeCodes.Match(line).Length);
                    var start = temp.Substring(0, 11);
                    var end = temp.Substring(12, 11);

                    var startParts = start.Split(';');
                    var endParts = end.Split(';');
                    if (startParts.Length == 4 && endParts.Length == 4)
                    {
                        p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), string.Empty);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (trimmedLine.Length == 0 || trimmedLine[0] == '@')
                {
                    // skip these lines
                }
                else if (trimmedLine.Length > 0 && p != null)
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
                        p.Text = p.Text.TrimEnd() + Environment.NewLine + line;
                    }
                }
            }

            if (!beginFound)
            {
                _errorCount++;
            }

            if (!endFound)
            {
                _errorCount++;
            }

            subtitle.Renumber();
        }
    }
}
