using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle68 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^\d\d:\d\d:\d\dF\d\d", RegexOptions.Compiled); //10:00:02F00

        public override string Extension => ".txt";

        public override string Name => "Unknown 68";

        public override string ToText(Subtitle subtitle, string title)
        {
            //Mais l'image utilisée pour
            //nous vendre la nourriture
            //  10:00:44F11
            //
            //est restée celle d'une Amérique
            //bucolique et agraire.
            //  10:00:46F18   10:00:50F14
            var sb = new StringBuilder();
            if (subtitle.Header != null && subtitle.Header.Contains(";» DO NOT MODIFY THE FOLLOWING 3 LINES"))
            {
                sb.AppendLine(subtitle.Header.Trim());
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine(@";» Video file: C:\Documents and Settings\video.mpg
;» Last edited: 24 sept. 09 11:26
;» Timing model: NTSC (30 fps)
;» Drop frame timing: ON
;» Number of captions: 1348
;» Caption time codes: 10:00:02F00 - 11:32:07F00
;» Video start time: 09:59:15F12 (Forced)
;» Insertion disk created: NO
;» Reading speed: 300
;» Minimum display time: 30
;» Maximum display time (sec.): 5
;» Minimum erase time: 0
;» Tab stop value: 4
;» Sticky mode: OFF
;» Right justification ragged left: OFF
;» Parallelogram filter: OFF
;» Coding standard: EIA-608" + Environment.NewLine +
";» \"Bottom\" row: 15" + @"
;» Lines per caption: 15
;» Characters per line: 32
;» Default horizontal position: Center
;» Default vertical position: Bottom
;» Default mode: PopOn
;» Captioning channel: 1

;» DO NOT MODIFY THE FOLLOWING 3 LINES
;»»10<210>10000001000000040?000?000100030000100?:?8;;:400685701001000100210<1509274
;»»2000000000200500005>??????091000000014279616<6000000000000000000000000000000000000000000000000000000005000105=4641;?
;»»0020??000000900<0<0<008000000000000<0<0<0080000000??00<0??000=200>1000;5=83<:;
;»
;» ************************************************************************");
                sb.AppendLine();
            }

            const string paragraphWriteFormat = "{0}{1}  {2}   {3}{1}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = p.Text.Replace("♪", "|");
                if (text.StartsWith("<i>", StringComparison.Ordinal))
                {
                    text = ",b" + Environment.NewLine + text;
                }

                if (text.StartsWith("{\\an8}", StringComparison.Ordinal))
                {
                    text = ",12" + Environment.NewLine + text;
                }

                text = HtmlUtil.RemoveHtmlTags(text, true);
                sb.AppendLine(string.Format(paragraphWriteFormat, text, Environment.NewLine, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime)));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            var text = new StringBuilder();
            var header = new StringBuilder();
            Paragraph p = null;
            char[] splitChars = { ':', 'F' };
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (subtitle.Paragraphs.Count == 0 && line.StartsWith(';') || line.Length == 0)
                {
                    header.AppendLine(line);
                }
                else if (RegexTimeCode.IsMatch(line))
                {
                    var timeParts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (timeParts.Length == 1)
                    {
                        try
                        {
                            TimeCode start = DecodeTimeCodeFrames(timeParts[0].Substring(0, 11), splitChars);
                            if (p != null && Math.Abs(p.EndTime.TotalMilliseconds) < 0.001)
                            {
                                p.EndTime.TotalMilliseconds = start.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                            }

                            TimeCode end = new TimeCode();
                            p = MakeTextParagraph(text, start, end);
                            subtitle.Paragraphs.Add(p);
                            text.Clear();
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                    else if (timeParts.Length == 2)
                    {
                        try
                        {
                            TimeCode start = DecodeTimeCodeFrames(timeParts[0].Substring(0, 11), splitChars);
                            if (p != null && Math.Abs(p.EndTime.TotalMilliseconds) < 0.001)
                            {
                                p.EndTime.TotalMilliseconds = start.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                            }

                            TimeCode end = DecodeTimeCodeFrames(timeParts[1].Substring(0, 11), splitChars);
                            p = MakeTextParagraph(text, start, end);
                            subtitle.Paragraphs.Add(p);
                            text.Clear();
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    text.AppendLine(line.Trim().Replace("|", "♪"));
                    if (text.Length > 5000)
                    {
                        return;
                    }
                }
                else
                {
                    text.Clear();
                }
            }
            subtitle.Header = header.ToString();
            subtitle.Renumber();
        }

        private static Paragraph MakeTextParagraph(StringBuilder text, TimeCode start, TimeCode end)
        {
            var p = new Paragraph(start, end, text.ToString().Trim());
            if (p.Text.StartsWith(",b" + Environment.NewLine, StringComparison.Ordinal))
            {
                p.Text = "<i>" + p.Text.Remove(0, 2).Trim() + "</i>";
            }
            else if (p.Text.StartsWith(",1" + Environment.NewLine, StringComparison.Ordinal))
            {
                p.Text = "{\\an8}" + p.Text.Remove(0, 2).Trim();
            }
            else if (p.Text.StartsWith(",12" + Environment.NewLine, StringComparison.Ordinal))
            {
                p.Text = "{\\an8}" + p.Text.Remove(0, 3).Trim();
            }

            return p;
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}F{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

    }
}
