﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class UnknownSubtitle33 : SubtitleFormat
    {

        private static Regex regexTimeCodes = new Regex(@"^\d\d\:\d\d\:\d\d\s+\d+    ", RegexOptions.Compiled);
        private static Regex regexNumberAndText = new Regex(@"^\d+    [^ ]+", RegexOptions.Compiled);
        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 33"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            Subtitle subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            //08:59:51  3    ON THE PANEL THIS WEEK WE HAVE EMILY LAWLER AND ZACH
            //08:59:54  4    GORCHOW, ALONG WITH RON DZWONKOWSKI.
            //          5    HERE IS THE RUNDOWN.
            //          6    A POSSIBLE REDO OF THE EM LAW IF VOTERS REJECT IT.
            //09:00:03  7    AND MIKE DUGAN AND LATER GENE CLEM IS DISCUSSING THIS

            const string paragraphWriteFormat = "{0} {1}    {2}";

            var sb = new StringBuilder();
            int count = 1;
            int count2 = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var lines = HtmlUtil.RemoveHtmlTags(p.Text).SplitToLines();
                if (lines.Length > 0)
                {
                    sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), count.ToString().PadLeft(2, ' '), lines[0]));
                    for (int i = 1; i < lines.Length; i++)
                    {
                        count++;
                        if (count > 26)
                        {
                            sb.Append(string.Empty.PadLeft(38, ' ') + count2);
                            sb.AppendLine();
                            sb.AppendLine();
                            count = 1;
                            count2++;
                        }
                        sb.AppendLine(string.Format(paragraphWriteFormat, string.Empty, count.ToString().PadLeft(10, ' '), lines[i]));
                    }

                    count++;
                    if (count > 26)
                    {
                        sb.Append(string.Empty.PadLeft(38, ' ') + count2);
                        sb.AppendLine();
                        sb.AppendLine();
                        count = 1;
                        count2++;
                    }
                }
            }
            return sb.ToString().Trim();
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            int seconds = (int)(timeCode.Seconds + timeCode.Milliseconds / 1000 + 0.5);
            return string.Format("{0:00}:{1:00}:{2:00}", timeCode.Hours, timeCode.Minutes, seconds);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (s.Length > 4 && s[2] == ':' && regexTimeCodes.Match(s).Success)
                {
                    if (p != null && !string.IsNullOrEmpty(p.Text))
                        subtitle.Paragraphs.Add(p);
                    p = new Paragraph();

                    try
                    {
                        string[] arr = s.Substring(0, 8).Split(':');
                        if (arr.Length == 3)
                        {
                            int hours = int.Parse(arr[0]);
                            int minutes = int.Parse(arr[1]);
                            int seconds = int.Parse(arr[2]);
                            p.StartTime = new TimeCode(hours, minutes, seconds, 0);
                            string text = s.Remove(0, 12).Trim();
                            p.Text = text;
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (p != null && regexNumberAndText.Match(s).Success)
                {
                    if (p.Text.Length > 1000)
                    {
                        _errorCount += 100;
                        return;
                    }
                    string text = s.Remove(0, 2).Trim();
                    p.Text = (p.Text + Environment.NewLine + text).Trim();
                }
                else if (s.Length > 0 && !Utilities.IsInteger(s))
                {
                    _errorCount++;
                }
            }
            if (p != null && !string.IsNullOrEmpty(p.Text))
                subtitle.Paragraphs.Add(p);

            int index = 1;
            foreach (Paragraph paragraph in subtitle.Paragraphs)
            {
                Paragraph next = subtitle.GetParagraphOrDefault(index);
                if (next != null)
                {
                    paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                }
                index++;
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}