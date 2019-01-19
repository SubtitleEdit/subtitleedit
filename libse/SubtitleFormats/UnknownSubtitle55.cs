using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle55 : SubtitleFormat
    {

        //  338:  00:24:34.00  00:24:37.10   [51]
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\s+\d\d:\d\d:\d\d\.\d\d\s+\d\d:\d\d:\d\d\.\d\d\s+\[\d+\]$", RegexOptions.Compiled);

        public override string Extension => ".rtf";

        public override string Name => "Unknown 55";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string format = "{0}:  {1}  {2}   [{3}]";
            var sb = new StringBuilder();
            int count = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(format, count, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), p.Text.Length));
                sb.AppendLine(p.Text);
                sb.AppendLine();
                count++;
            }

            return sb.ToString().ToRtf();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}.{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
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

            var arr = rtf.FromRtf().SplitToLines();
            bool expectStartTime = true;
            var p = new Paragraph();
            subtitle.Paragraphs.Clear();
            char[] splitChars = { '.', ':' };
            foreach (string line in arr)
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
                            p.StartTime = DecodeTimeCodeFrames(parts[1], splitChars);
                            p.EndTime = DecodeTimeCodeFrames(parts[2], splitChars);
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
