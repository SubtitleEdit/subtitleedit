using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle52 : SubtitleFormat
    {
        //#00001    10:00:02.00 10:00:04.13 00:00:02.13 #F CC00000D0    #C
        private static readonly Regex RegexTimeCodes = new Regex(@"^\#\d\d\d\d\d\t\d\d:\d\d:\d\d\.\d\d\t\d\d:\d\d:\d\d\.\d\d\t\d\d:\d\d:\d\d\.\d\d\t.*$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 52";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines.Count > 0 && lines[0] != null && lines[0].StartsWith("{\\rtf1", StringComparison.Ordinal))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string paragraphWriteFormat = "#{0:00000}\t{1}\t{2}\t{3}\t#F\tCC00000D0\t#C " + Environment.NewLine + "{4}";
            const string timeFormat = "{0:00}:{1:00}:{2:00}.{3:00}";
            var sb = new StringBuilder();
            string header = @"FILE_INFO_BEGIN
VIDEOFILE:
ORIG_TITLE: [TITLE]
PGM_TITLE:
EP_TITLE: 03
PROD:
TRANSL: SDI Media
CLIENT: FIC-HD
COMMENT:
TAPE#: TN10179565
CRE_DATE:
REP_DATE:
TR_DATE:
PROG_LEN:
SOM: 09:59:35:00
TRA_FONT:
LANG_CO: English
LIST_FONT: Arial Unicode MS 450
TV_SYS: 625/50
TV_FPS: EBU 625/50
LINE_LEN: 43.2
SW_VER: 2.25
FILE_INFO_END";
            if (subtitle.Header != null && subtitle.Header.Contains("FILE_INFO_BEGIN"))
            {
                header = subtitle.Header;
            }

            sb.AppendLine(header);
            int number = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var startFrame = MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds);
                string startTime = string.Format(timeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, startFrame);

                var endFrame = MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds);
                string endTime = string.Format(timeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, endFrame);

                // to avoid rounding errors in duration
                var durationCalc = new Paragraph(
                        new TimeCode(p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, FramesToMillisecondsMax999(startFrame)),
                        new TimeCode(p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, FramesToMillisecondsMax999(endFrame)),
                        string.Empty);
                string duration = string.Format(timeFormat, durationCalc.Duration.Hours, durationCalc.Duration.Minutes, durationCalc.Duration.Seconds, MillisecondsToFramesMaxFrameRate(durationCalc.Duration.Milliseconds));

                sb.AppendLine(string.Format(paragraphWriteFormat, number, startTime, endTime, duration, HtmlUtil.RemoveHtmlTags(p.Text)));
                number++;
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            bool started = false;
            var header = new StringBuilder();
            var text = new StringBuilder();
            char[] splitChar = { ':', ',', '.' };
            foreach (string line in lines)
            {
                try
                {
                    if (RegexTimeCodes.Match(line).Success)
                    {
                        started = true;
                        if (p != null)
                        {
                            p.Text = text.ToString().Trim();
                        }

                        text.Clear();
                        string start = line.Substring(7, 11);
                        string end = line.Substring(19, 11);
                        p = new Paragraph(DecodeTimeCodeFrames(start, splitChar), DecodeTimeCodeFrames(end, splitChar), string.Empty);
                        subtitle.Paragraphs.Add(p);
                    }
                    else if (!started)
                    {
                        header.AppendLine(line);
                    }
                    else if (p != null && p.Text.Length < 200)
                    {
                        text.AppendLine(line);
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                catch
                {
                    _errorCount++;
                }
            }
            if (p != null)
            {
                p.Text = text.ToString().Trim();
            }

            subtitle.Header = header.ToString();
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}
