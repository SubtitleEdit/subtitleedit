using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
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

        private static Regex regexTimeCodes1 = new Regex(@"^\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 60"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();

            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);

            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(EncodeTimeCode(p.StartTime));
                sb.AppendLine(EncodeTimeCode(p.EndTime));
                if (!string.IsNullOrEmpty(p.Actor))
                    sb.AppendLine(p.Actor.ToUpper());
                else
                    sb.AppendLine("UNKNOWN ACTOR");
                sb.AppendLine(Utilities.RemoveHtmlTags(p.Text));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
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
                var match = regexTimeCodes1.Match(s);
                if (match.Success && s.Length == 11)
                {
                    if (p.StartTime.TotalMilliseconds > 0)
                    {
                        subtitle.Paragraphs.Add(p);
                        if (string.IsNullOrEmpty(p.Text))
                            _errorCount++;
                    }

                    p = new Paragraph();
                    string[] parts = s.Split(':');
                    if (parts.Length == 4)
                    {
                        try
                        {
                            p.StartTime = DecodeTimeCode(parts);
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
                    if (line == line.ToUpper())
                        p.Actor = line;
                    else
                        _errorCount++;
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
                subtitle.Paragraphs.Add(p);

            bool allNullEndTime = true;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                p = subtitle.Paragraphs[i];
                if (p.EndTime.TotalMilliseconds != 0)
                    allNullEndTime = false;

                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text);
                if (i < subtitle.Paragraphs.Count - 2 && p.EndTime.TotalMilliseconds >= subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds)
                    p.EndTime.TotalMilliseconds = subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds - Configuration.Settings.General.MininumMillisecondsBetweenLines;
            }
            if (!allNullEndTime)
                subtitle.Paragraphs.Clear();

            subtitle.RemoveEmptyLines();
            subtitle.Renumber(1);
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(int.Parse(frames)));
        }

    }
}
