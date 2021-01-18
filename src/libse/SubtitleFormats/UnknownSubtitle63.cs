using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle63 : SubtitleFormat
    {
        //3:       00:00:09:23 00:00:16:21 06:23
        //Alustame sellest...
        //Siin kajab kuidagi harjumatult.
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\s+\d\d:\d\d:\d\d\:\d\d \d\d:\d\d:\d\d\:\d\d \d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 63";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string format = "{0}:       {1} {2} {3:00}:{4:00}";
            var sb = new StringBuilder();
            int count = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                // to avoid rounding errors in duration
                var startFrame = MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds);
                var endFrame = MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds);
                var durationCalc = new Paragraph(
                        new TimeCode(p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, FramesToMillisecondsMax999(startFrame)),
                        new TimeCode(p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, FramesToMillisecondsMax999(endFrame)),
                        string.Empty);

                sb.AppendLine(string.Format(format, count, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), durationCalc.Duration.Seconds, MillisecondsToFramesMaxFrameRate(durationCalc.Duration.Milliseconds)));
                sb.AppendLine(p.Text);
                sb.AppendLine();
                count++;
            }
            return sb.ToString().Trim();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            bool expectStartTime = true;
            var p = new Paragraph();
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                string s = line.Trim().Replace("*", string.Empty);
                var match = RegexTimeCodes.Match(s);
                if (match.Success)
                {
                    string[] parts = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(p.Text))
                            {
                                subtitle.Paragraphs.Add(p);
                                p = new Paragraph();
                            }
                            p.StartTime = DecodeTimeCodeFrames(parts[1], SplitCharColon);
                            p.EndTime = DecodeTimeCodeFrames(parts[2], SplitCharColon);
                            expectStartTime = false;
                        }
                        catch (Exception exception)
                        {
                            _errorCount++;
                            System.Diagnostics.Debug.WriteLine(exception.Message);
                        }
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    if (Math.Abs(p.StartTime.TotalMilliseconds) < 0.001 && Math.Abs(p.EndTime.TotalMilliseconds) < 0.001)
                    {
                        _errorCount++;
                    }
                    else
                    {
                        subtitle.Paragraphs.Add(p);
                    }

                    p = new Paragraph();
                }
                else if (!expectStartTime)
                {
                    p.Text = (p.Text + Environment.NewLine + line).Trim();
                    if (p.Text.Length > 500)
                    {
                        _errorCount += 10;
                        return;
                    }
                    while (p.Text.Contains(Environment.NewLine + " "))
                    {
                        p.Text = p.Text.Replace(Environment.NewLine + " ", Environment.NewLine);
                    }
                }
            }
            if (!string.IsNullOrEmpty(p.Text))
            {
                subtitle.Paragraphs.Add(p);
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}
