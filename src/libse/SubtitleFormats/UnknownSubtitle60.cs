using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle60 : SubtitleFormat
    {
        //01:00:31:14
        //THE PRIME MINISTER
        //Thank you.

        //01:00:32:06
        //STIG
        //But first we'll go to our foreign guest, welcome to the programme. It’s a great pleasure having you here. Let me start with a standard question; is this your first time in Sweden?

        //01:00:44:16
        //FEMALE ARTIST
        //No, I was here once many years ago, and I was introduced to your ”surströmming” – is that..?

        private static readonly Regex RegexTimeCodes1 = new Regex(@"^\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 60";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(EncodeTimeCode(p.StartTime));
                sb.AppendLine(EncodeTimeCode(p.EndTime));
                if (!string.IsNullOrEmpty(p.Actor))
                {
                    sb.AppendLine(p.Actor.ToUpperInvariant());
                }
                else
                {
                    sb.AppendLine("UNKNOWN ACTOR");
                }

                sb.AppendLine(HtmlUtil.RemoveHtmlTags(p.Text));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            bool expectStartTime = true;
            bool expectActor = false;
            var p = new Paragraph();
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                string s = line.Trim();
                var match = RegexTimeCodes1.Match(s);
                if (match.Success && s.Length == 11)
                {
                    if (p.StartTime.TotalMilliseconds > 0)
                    {
                        subtitle.Paragraphs.Add(p);
                        if (string.IsNullOrEmpty(p.Text))
                        {
                            _errorCount++;
                        }
                    }

                    p = new Paragraph();
                    string[] parts = s.Split(':');
                    if (parts.Length == 4)
                    {
                        try
                        {
                            p.StartTime = DecodeTimeCodeFramesFourParts(parts);
                            expectActor = true;
                            expectStartTime = false;
                        }
                        catch (Exception exception)
                        {
                            _errorCount++;
                            System.Diagnostics.Debug.WriteLine(exception.Message);
                            expectStartTime = true;
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(line) && expectActor)
                {
                    if (line == line.ToUpperInvariant())
                    {
                        p.Actor = line;
                    }
                    else
                    {
                        _errorCount++;
                    }

                    expectActor = false;
                }
                else if (!string.IsNullOrWhiteSpace(line) && !expectActor && !expectStartTime)
                {
                    p.Text = (p.Text + Environment.NewLine + line).Trim();
                    if (p.Text.Length > 5000)
                    {
                        _errorCount += 10;
                        return;
                    }
                }
            }
            if (p.StartTime.TotalMilliseconds > 0)
            {
                subtitle.Paragraphs.Add(p);
            }

            bool allNullEndTime = true;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                p = subtitle.Paragraphs[i];
                if (p.EndTime.TotalMilliseconds != 0)
                {
                    allNullEndTime = false;
                }

                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text);
                if (i < subtitle.Paragraphs.Count - 2 && p.EndTime.TotalMilliseconds >= subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds)
                {
                    p.EndTime.TotalMilliseconds = subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                }
            }
            if (!allNullEndTime)
            {
                subtitle.Paragraphs.Clear();
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}
