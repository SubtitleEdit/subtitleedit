using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle99 : SubtitleFormat
    {
        //07:00:01:27
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\:\d\d\:\d\d\:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".rtf";

        public override string Name => "Unknown 99";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)}{Environment.NewLine}{p.Text}{Environment.NewLine}");
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
            var p = new Paragraph { StartTime = { TotalMilliseconds = -1 } };
            var text = new StringBuilder();
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (RegexTimeCodes.Match(s).Success)
                {
                    try
                    {
                        if (p.StartTime.TotalMilliseconds >= 0 && text.Length > 0)
                        {
                            p.Text = text.ToString().Trim();
                            subtitle.Paragraphs.Add(p);
                        }

                        text = new StringBuilder();
                        var arr = s.Split(':');
                        if (arr.Length == 4)
                        {
                            p = new Paragraph { StartTime = DecodeTimeCodeFramesFourParts(arr) };
                        }
                        else
                        {
                            _errorCount++;
                            p = new Paragraph { StartTime = { TotalMilliseconds = -1 } };
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (s.Length > 0)
                {
                    text.AppendLine(s);
                    if (text.Length > 2000)
                    {
                        _errorCount++;
                        return;
                    }
                }
            }
            if (p.StartTime.TotalMilliseconds >= 0 && text.Length > 0)
            {
                p.Text = text.ToString().Trim();
                subtitle.Paragraphs.Add(p);
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
