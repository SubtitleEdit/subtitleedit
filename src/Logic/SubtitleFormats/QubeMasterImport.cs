using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class QubeMasterImport : SubtitleFormat
    {
// ToText code by Tosil Velkoff, tosil@velkoff.net
// Based on UnknownSubtitle44
    //SubLine1
    //SubLine2
    //10:01:04:12
    //10:01:07:09
        static Regex regexTimeCodes1 = new Regex(@"^\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "QubeMasterPro Import"; }
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
            int index = 0;

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (index != 0) sb.AppendLine();
                index++;

                StringBuilder text = new StringBuilder();
                sb.AppendLine(Utilities.RemoveHtmlTags(p.Text));
                sb.AppendLine(EncodeTimeCode(p.StartTime));
                sb.AppendLine(EncodeTimeCode(p.EndTime));

            }
            return sb.ToString();
        }

        private string EncodeTimeCode(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            bool expectStartTime = true;
            var p = new Paragraph();
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                string s = line.Trim();
                var match = regexTimeCodes1.Match(s);
                if (match.Success)
                {
                    string[] parts = s.Split(':');
                    if (parts.Length == 4)
                    {
                        try
                        {
                            if (expectStartTime)
                            {
                                p.StartTime = DecodeTimeCode(parts);
                                expectStartTime = false;
                            }
                            else
                            {
                                if (p.EndTime.TotalMilliseconds < 0.01)
                                    _errorCount++;
                                p.EndTime = DecodeTimeCode(parts);
                            }
                        }
                        catch (Exception exception)
                        {
                            _errorCount++;
                            System.Diagnostics.Debug.WriteLine(exception.Message);
                        }
                    }
                }
                else if (line.Trim().Length == 0)
                {
                    if (p != null)
                    {
                        if (p.StartTime.TotalMilliseconds == 0 && p.EndTime.TotalMilliseconds == 0)
                            _errorCount++;
                        else
                            subtitle.Paragraphs.Add(p);
                        p = new Paragraph();
                    }
                }
                else if (line.Trim().Length > 0 && p != null)
                {
                    expectStartTime = true;
                    p.Text = (p.Text + Environment.NewLine + line).Trim();
                    if (p.Text.Length > 500)
                    {
                        _errorCount+=10;
                        return;
                    }
                }
            }
            if (p != null && p.EndTime.TotalMilliseconds > 0)
                subtitle.Paragraphs.Add(p);

            subtitle.RemoveEmptyLines();
            subtitle.Renumber(1);
        }

        private TimeCode DecodeTimeCode(string[] parts)
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

