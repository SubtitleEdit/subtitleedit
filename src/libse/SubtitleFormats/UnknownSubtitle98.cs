using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle98 : SubtitleFormat
    {
        //07:00:01:27AM
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\:\d\d\:\d\d\:\d\d\t", RegexOptions.Compiled);

        public override string Extension => ".rtf";

        public override string Name => "Unknown 98";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)}\t{p.Text.Replace(Environment.NewLine, " ")}");
            }
            return sb.ToString().Trim().ToRtf();
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            return $"{timeCode.Hours:00}:{timeCode.Minutes:00}:{timeCode.Seconds:00}:{MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            var rtf = sb.ToString().Trim();
            if (!rtf.StartsWith("{\\rtf", StringComparison.Ordinal))
            {
                return;
            }

            lines = rtf.FromRtf().SplitToLines();
            _errorCount = 0;
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (RegexTimeCodes.Match(s).Success)
                {
                    try
                    {
                        var arr = s.Substring(0, 11).Split(':');
                        if (arr.Length == 4)
                        {
                            var p = new Paragraph
                            {
                                StartTime = DecodeTimeCodeFramesFourParts(arr),
                                Text = s.Remove(0, 11).Trim()
                            };
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (s.Length > 0)
                {
                    _errorCount++;
                }
            }

            int index = 1;
            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            foreach (var paragraph in subtitle.Paragraphs)
            {
                paragraph.Text = Utilities.AutoBreakLine(paragraph.Text, language);
                var next = subtitle.GetParagraphOrDefault(index);
                if (next != null)
                {
                    paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                }
                if (paragraph.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(paragraph.Text);
                }
                index++;
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }
    }
}
