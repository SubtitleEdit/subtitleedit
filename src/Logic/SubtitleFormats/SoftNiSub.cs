using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    /// <summary>
    /// SoftNi - http://www.softni.com/ 
    /// </summary>
    public class SoftNiSub : SubtitleFormat
    {
        static Regex regexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d\.\d\d\\\d\d:\d\d:\d\d\.\d\d$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".sub"; }
        }

        public override string Name
        {
            get { return "SoftNi sub"; }
        }

        public override bool HasLineNumber
        {
            get { return false; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            DoLoadSubtitle(subtitle, lines);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("*PART 1*");
            sb.AppendLine("00:00:00.00\\00:00:00.00");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = p.Text;
                if (text.StartsWith("<i>") && text.EndsWith("</i>"))
                    text = "[" + text;
                text = Utilities.RemoveHtmlTags(text);
                sb.AppendLine(string.Format("{0}{1}{2}\\{3}", text, Environment.NewLine, p.StartTime.ToHHMMSSPeriodFF(), p.EndTime.ToHHMMSSPeriodFF()));
            }
            sb.AppendLine(@"*END*
...........\...........
*CODE*
0000000000000000
*CAST*
*GENERATOR*
*FONTS*
*READ*
0,300 15,000 130,000 100,000 25,000
*TIMING*
1 25 0
*TIMED BACKUP NAME*
C:\
*FORMAT SAMPLE ÅåÉéÌìÕõÛûÿ*
*READ ADVANCED*
< > 1 1 0,300
*MARKERS*");
            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            DoLoadSubtitle(subtitle, lines);
        }

        private void DoLoadSubtitle(Subtitle subtitle, List<string> lines)
        {
            //—Peter.
            //—Estoy de licencia.
            //01:48:50.07\01:48:52.01
            var sb = new StringBuilder();
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (regexTimeCodes.IsMatch(s))
                {
                    var temp = s.Split('\\');
                    if (temp.Length > 1)
                    {
                        string start = temp[0];
                        string end = temp[1];

                        string[] startParts = start.Split(":.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string[] endParts = end.Split(":.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (startParts.Length == 4 && endParts.Length == 4)
                        {
                            try
                            {
                                p = new Paragraph();
                                p.StartTime = DecodeTimeCode(startParts);
                                p.EndTime = DecodeTimeCode(endParts);
                                string text = sb.ToString().Trim();
                                if (text.StartsWith("["))
                                    text = "<i>" + text.Remove(0, 1) + "</i>";
                                p.Text = text;
                                if (text.Length > 0)
                                    subtitle.Paragraphs.Add(p);
                                sb = new StringBuilder();
                            }
                            catch (Exception exception)
                            {
                                _errorCount++;
                                System.Diagnostics.Debug.WriteLine(exception.Message);
                            }
                        }
                    }
                }
                else if (line.Trim().Length == 0)
                {
                    // skip empty lines
                }
                else if (line.StartsWith("*"))
                {
                    // skip start
                }
                else if (line.Trim().Length > 0 && p != null)
                {
                    sb.AppendLine(line);
                }
            }

            subtitle.Renumber(1);
        }

        private TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            int milliseconds = (int)((1000.0 / Configuration.Settings.General.CurrentFrameRate) * int.Parse(frames));
            if (milliseconds > 999)
                milliseconds = 999;

            TimeCode tc = new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), milliseconds);
            return tc;
        }

    }
}

