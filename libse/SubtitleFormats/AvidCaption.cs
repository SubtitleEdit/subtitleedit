using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AvidCaption : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d \d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Avid Caption";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("@ This file written with the Avid Caption plugin, version 1");
            sb.AppendLine();
            sb.AppendLine("<begin subtitles>");
            const string writeFormat = "{0} {1}{2}{3}{2}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(writeFormat, p.StartTime.ToHHMMSSFF(), EncodeEndTimeCode(p.EndTime), Environment.NewLine, HtmlUtil.RemoveHtmlTags(p.Text, true)));
                //00:50:34:22 00:50:39:13
                //Ich muss dafür sorgen,
                //dass die Epsteins weiterleben
            }
            sb.AppendLine("<end subtitles>");
            return sb.ToString();
        }

        private static string EncodeEndTimeCode(TimeCode time)
        {
            //00:50:39:13 (last is frame)

            //Bugfix for Avid - On 23.976 FPS and 24 FPS projects, when the End time of a subtitle ends in 02, 07, 12, 17, 22, 27 frames, the subtitle won't import.
            if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 23.976) < 0.01 ||
                Math.Abs(Configuration.Settings.General.CurrentFrameRate - 24) < 0.01)
            {
                var frames = MillisecondsToFramesMaxFrameRate(time.Milliseconds);
                if (frames == 2 || frames == 7 || frames == 12 || frames == 17 || frames == 22 || frames == 27)
                {
                    frames--;
                }

                return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{frames:00}";
            }
            else
            {
                return time.ToHHMMSSFF();
            }
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:03:15:22  00:03:23:10 This is line one.
            //This is line two.
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            bool beginFound = false;
            bool endFound = false;
            foreach (string line in lines)
            {
                string tline = line.Trim();
                if (tline.Equals("<begin subtitles>", StringComparison.OrdinalIgnoreCase))
                {
                    beginFound = true;
                }
                else if (tline.Equals("<end subtitles>", StringComparison.OrdinalIgnoreCase))
                {
                    endFound = true;
                    break;
                }

                if (line.IndexOf(':') == 2 && RegexTimeCodes.IsMatch(line))
                {
                    string temp = line.Substring(0, RegexTimeCodes.Match(line).Length);
                    string start = temp.Substring(0, 11);
                    string end = temp.Substring(12, 11);

                    string[] startParts = start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 4 && endParts.Length == 4)
                    {
                        p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), string.Empty);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (tline.Length == 0 || tline[0] == '@')
                {
                    // skip these lines
                }
                else if (tline.Length > 0 && p != null)
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
