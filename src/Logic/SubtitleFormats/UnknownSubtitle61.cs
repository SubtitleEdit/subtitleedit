﻿using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class UnknownSubtitle61 : SubtitleFormat
    {
        //00:02:23.59
        //קרוליין: פשוט תשימי את זה בפייסבוק או משהו.

        //00:02:25.78
        //הם בטוח יאהבו את זה.

        //00:02:27.78
        //ליזי: אוקיי. אז אני מניחה שזה זמן הנתינה
        private static Regex regexTimeCodes1 = new Regex(@"^\d\d:\d\d:\d\d\.\d\d$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 61"; }
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
                sb.AppendLine(HtmlUtil.RemoveHtmlTags(p.Text));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds / 10);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            bool expectStartTime = true;
            var p = new Paragraph();
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                string s = line.Trim();
                var match = regexTimeCodes1.Match(s);
                if (match.Success && s.Length == 11)
                {
                    if (!expectStartTime)
                        _errorCount++;

                    if (p.StartTime.TotalMilliseconds > 0)
                    {
                        subtitle.Paragraphs.Add(p);
                        if (string.IsNullOrEmpty(p.Text))
                            _errorCount++;
                    }

                    p = new Paragraph();
                    string[] parts = s.Split(new[] { ':', '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        try
                        {
                            p.StartTime = DecodeTimeCode(parts);
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
                else if (string.IsNullOrWhiteSpace(line))
                {
                    expectStartTime = true;
                }
                else if (!expectStartTime)
                {
                    p.Text = (p.Text + Environment.NewLine + line).Trim();
                    if (p.Text.Length > 5000)
                    {
                        _errorCount += 10;
                        return;
                    }
                }
                else
                {
                    _errorCount++;
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
                    p.EndTime.TotalMilliseconds = subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
            }
            if (!allNullEndTime)
                subtitle.Paragraphs.Clear();

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string msDiv10 = parts[3];

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), int.Parse(msDiv10) * 10);
        }

    }
}
