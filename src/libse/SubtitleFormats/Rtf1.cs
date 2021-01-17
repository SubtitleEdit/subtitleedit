using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{

    public class Rtf1 : SubtitleFormat
    {
        // 0001 00:00:30:10 00:00:33:11 27 (same as 86, but with "reading speed")
        private static readonly Regex RegexTimeCode1 = new Regex(@"\d+ \d\d:\d\d:\d\d:\d\d \d\d:\d\d:\d\d:\d\d \d+", RegexOptions.Compiled);

        public override string Extension => ".rtf";

        public override string Name => "RTF 1";

        private static string MakeTimeCode(TimeCode tc)
        {
            return $"{tc.Hours:00}:{tc.Minutes:00}:{tc.Seconds:00}:{MillisecondsToFramesMaxFrameRate(tc.Milliseconds):00}";
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1039{\fonttbl{\f0\fnil Arial;}}
{\colortbl ;\red0\green0\blue0;}
\viewkind4\uc1\pard\b\f0\fs24 [Header]\par
Max Characters Per Line: 36\par
Frame Rate: " + Configuration.Settings.General.CurrentFrameRate + @" fps\par
TimeCode Format: " + Configuration.Settings.General.CurrentFrameRate + @" frames/sec\par
[/Header]\par
\par
\par");
            sb.AppendLine();
            int count = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = HtmlUtil.RemoveHtmlTags(p.Text);
                var readingSpeed = CalculateReadingSpeed(text);
                sb.AppendLine($"\\pard {count:0000} {MakeTimeCode(p.StartTime)} {MakeTimeCode(p.EndTime)} 27 \\par"); // \pard 0001 00:00:30:10 00:00:33:11 27\par
                sb.AppendLine("\\par");
                sb.AppendLine($"\\pard\\qc\\cf1\\outl {text.ToRtfPart()}\\par");
                sb.AppendLine("\\cf0\\outl0\\par");
                count++;
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        private object CalculateReadingSpeed(string text)
        {
            return 0; // Don't know how this is calculated
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            string rtf = sb.ToString().Trim();
            if (!rtf.StartsWith("{\\rtf", StringComparison.Ordinal))
            {
                return;
            }

            lines = rtf.FromRtf().SplitToLines();
            Paragraph p = null;
            char[] splitChars = { ':', ';', ',' };
            foreach (string line in lines)
            {
                string s = line.TrimEnd();
                if (RegexTimeCode1.IsMatch(s))
                {
                    try
                    {
                        if (p != null)
                        {
                            subtitle.Paragraphs.Add(p);
                        }

                        string[] arr = s.Split(' ');
                        p = new Paragraph(DecodeTimeCodeFrames(arr[1], splitChars), DecodeTimeCodeFrames(arr[2], splitChars), string.Empty);
                    }
                    catch
                    {
                        _errorCount++;
                        p = null;
                    }
                }
                else if (s.Length > 0 && p != null)
                {
                    p.Text = (p.Text + Environment.NewLine + s.TrimStart()).Trim();
                }
                else if (s.Length > 0)
                {
                    _errorCount++;
                }
            }
            if (p != null)
            {
                subtitle.Paragraphs.Add(p);
            }

            if (subtitle.Paragraphs.Count > 0)
            {
                p = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1];
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text);
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}
