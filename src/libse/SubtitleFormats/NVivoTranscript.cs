using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class NVivoTranscript : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes1 = new Regex(@"^\d\d\:\d\d\t[A-Z\d]+:\t", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodes2 = new Regex(@"^\d\d\:\d\d\:\d\d\t[A-Z\d]+:\t", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "nVivo transcript";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)}\t{GetActor(p)}\t{p.Text.Replace(Environment.NewLine, " ")}");
            }

            return sb.ToString().Trim();
        }

        private static string GetActor(Paragraph paragraph)
        {
            return string.IsNullOrEmpty(paragraph.Actor) ? "UNKNOWN1:" : paragraph.Actor;
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            if (timeCode.Hours > 0)
            {
                return $"{timeCode.Hours}:{timeCode.Minutes:00}:{timeCode.Seconds:00}";
            }

            return $"{timeCode.Minutes:00}:{timeCode.Seconds:00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            foreach (var line in lines)
            {
                var s = line.Trim();
                if (RegexTimeCodes1.Match(s).Success)
                {
                    var timeCode = $"00:{s.Substring(0, 5)}:00";
                    var arr = s.Split('\t');
                    if (arr.Length == 3)
                    {
                        var speaker = arr[1].Trim();
                        var text = arr[2].Trim();
                        subtitle.Paragraphs.Add(GetParagraph(timeCode, speaker, text));
                    }
                }
                else if (RegexTimeCodes2.Match(s).Success)
                {
                    var timeCode = $"{s.Substring(0, 8)}:00";
                    var arr = s.Split('\t');
                    if (arr.Length == 3)
                    {
                        var speaker = arr[1].Trim();
                        var text = arr[2].Trim();
                        subtitle.Paragraphs.Add(GetParagraph(timeCode, speaker, text));
                    }
                }
                else if (s.Length > 0)
                {
                    _errorCount++;
                }

                if (_errorCount > 10)
                {
                    return;
                }
            }

            var index = 1;
            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            foreach (var paragraph in subtitle.Paragraphs)
            {
                paragraph.Text = Utilities.RemoveUnneededSpaces(paragraph.Text, language);
                paragraph.Text = Utilities.AutoBreakLine(paragraph.Text, language);
                paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(paragraph.Text);

                var next = subtitle.GetParagraphOrDefault(index);
                if (next != null)
                {
                    paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                }
                else
                {
                    paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + Configuration.Settings.General.NewEmptyDefaultMs;
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

        private static Paragraph GetParagraph(string timeCode, string speaker, string text)
        {
            var tc = DecodeTimeCodeFramesFourParts(timeCode.Split(':'));
            return new Paragraph
            {
                StartTime = tc,
                Text = text,
                Actor = speaker,
            };
        }
    }
}
