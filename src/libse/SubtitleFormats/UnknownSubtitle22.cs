using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle22 : SubtitleFormat
    {

        //25    10:03:20:23 02:07   10:03:23:05
        //I see, on my way.
        //
        //26    10:03:31:18 02:07   10:03:34:00
        //Panessa, why didn't they give them
        //an escape route ?

        private static readonly Regex RegexTimeCode = new Regex(@"^\d+\t\d\d:\d\d:\d\d:\d\d\t\d\d:\d\d\t\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 22";

        private static string MakeTimeCode(TimeCode tc)
        {
            return $"{tc.Hours:00}:{tc.Minutes:00}:{tc.Seconds:00}:{MillisecondsToFramesMaxFrameRate(tc.Milliseconds):00}";
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("#\tAppearance\tCaption\t");
            sb.AppendLine();
            int count = 1;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string text = HtmlUtil.RemoveHtmlTags(p.Text);

                // to avoid rounding errors in duration
                var startFrame = MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds);
                var endFrame = MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds);
                var durationCalc = new Paragraph(
                        new TimeCode(p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, FramesToMillisecondsMax999(startFrame)),
                        new TimeCode(p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, FramesToMillisecondsMax999(endFrame)),
                        string.Empty);

                sb.AppendLine($"{count}\t{MakeTimeCode(p.StartTime)}\t{durationCalc.Duration.Seconds:00}:{MillisecondsToFramesMaxFrameRate(durationCalc.Duration.Milliseconds):00}\t{MakeTimeCode(p.EndTime)}\r\n{text}\r\n");
                count++;
            }

            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            var sb = new StringBuilder();
            char[] splitChars = { ':', ';', ',' };
            foreach (string line in lines)
            {
                string s = line.TrimEnd();
                if (RegexTimeCode.IsMatch(s))
                {
                    try
                    {
                        if (p != null)
                        {
                            p.Text = sb.ToString().Trim();
                            subtitle.Paragraphs.Add(p);
                        }
                        sb = new StringBuilder();
                        string[] arr = s.Split('\t');
                        if (arr.Length == 4)
                        {
                            p = new Paragraph(DecodeTimeCodeFrames(arr[1], splitChars), DecodeTimeCodeFrames(arr[3], splitChars), string.Empty);
                        }
                    }
                    catch
                    {
                        _errorCount++;
                        p = null;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(s))
                {
                    sb.AppendLine(s);
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
        }

    }
}
