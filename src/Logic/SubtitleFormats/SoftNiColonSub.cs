﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    /// <summary>
    /// SoftNi - http://www.softni.com/
    /// </summary>
    public class SoftNicolonSub : SubtitleFormat
    {
        private static Regex regexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d\\\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".sub"; }
        }

        public override string Name
        {
            get { return "SoftNi colon sub"; }
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
            sb.AppendLine("00:00:00:00\\00:00:00:00");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = p.Text;
                bool positionTop = false;

                // If text starts with {\an8}, subtitle appears at the top
                if (text.StartsWith("{\\an8}"))
                {
                    positionTop = true;
                    // Remove the tag {\an8}.
                    text = text.Remove(0, 6);
                }

                // Split lines (split a subtitle into its lines)
                var lines = text.SplitToLines();
                int count = 0;
                var lineSb = new StringBuilder();
                string tempLine = string.Empty;
                Boolean nextLineInItalics = false;
                foreach (string line in lines)
                {
                    // Append line break in every line except the first one
                    if (count > 0)
                        lineSb.Append(Environment.NewLine);

                    tempLine = line;

                    // This line should be in italics (it was detected in previous line)
                    if (nextLineInItalics)
                    {
                        tempLine = "<i>" + tempLine;
                        nextLineInItalics = false;
                    }

                    if (tempLine.StartsWith("<i>") && tempLine.EndsWith("</i>"))
                    {
                        // Whole line is in italics
                        // Remove <i> from the beginning
                        tempLine = tempLine.Remove(0, 3);
                        // Remove </i> from the end
                        tempLine = tempLine.Remove(tempLine.Length - 4, 4);
                        // Add new italics tag at the beginning
                        tempLine = "[" + tempLine;
                    }
                    else if (tempLine.StartsWith("<i>") && Utilities.CountTagInText(tempLine, "<i>") > Utilities.CountTagInText(tempLine, "</i>"))
                    {
                        // Line starts with <i> but italics are not closed. So the next line should be in italics
                        nextLineInItalics = true;
                    }
                    lineSb.Append(tempLine);
                    count++;

                    text = lineSb.ToString();
                    // Replace remaining italics tags
                    text = text.Replace("<i>", @"[");
                    text = text.Replace("</i>", @"]");
                    text = HtmlUtil.RemoveHtmlTags(text);
                }

                // Add top-position SoftNI marker "}" at the beginning of first line.
                if (positionTop)
                    text = "}" + text;

                sb.AppendLine(string.Format("{0}{1}{2}\\{3}", text, Environment.NewLine, p.StartTime.ToHHMMSSPeriodFF().Replace(".", ":"), p.EndTime.ToHHMMSSPeriodFF().Replace(".", ":")));
            }
            sb.AppendLine(@"*END*");
            sb.AppendLine(@"...........\...........");
            sb.AppendLine(@"*CODE*");
            sb.AppendLine(@"0000000000000000");
            sb.AppendLine(@"*CAST*");
            sb.AppendLine(@"*GENERATOR*");
            sb.AppendLine(@"*FONTS*");
            sb.AppendLine(@"*READ*");
            sb.AppendLine(@"0,300 15,000 130,000 100,000 25,000");
            sb.AppendLine(@"*TIMING*");
            sb.AppendLine(@"1 25 0");
            sb.AppendLine(@"*TIMED BACKUP NAME*");
            sb.AppendLine(@"C:\");
            sb.AppendLine(@"*FORMAT SAMPLE ÅåÉéÌìÕõÛûÿ*");
            sb.AppendLine(@"*READ ADVANCED*");
            sb.AppendLine(@"< > 1 1 0,300");
            sb.AppendLine(@"*MARKERS*");
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
                    // Start and end time separated by "\"
                    var temp = s.Split('\\');
                    if (temp.Length > 1)
                    {
                        string start = temp[0];
                        string end = temp[1];

                        string[] startParts = start.Split(new[] { ':', '.' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] endParts = end.Split(new[] { ':', '.' }, StringSplitOptions.RemoveEmptyEntries);
                        if (startParts.Length == 4 && endParts.Length == 4)
                        {
                            try
                            {
                                p = new Paragraph();
                                p.StartTime = DecodeTimeCode(startParts);
                                p.EndTime = DecodeTimeCode(endParts);
                                string text = sb.ToString().Trim();

                                Boolean positionTop = false;
                                // If text starts with "}", subtitle appears at the top
                                if (text.StartsWith('}'))
                                {
                                    positionTop = true;
                                    // Remove the tag "}"
                                    text = text.Remove(0, 1);
                                }
                                // Replace tags
                                text = text.Replace("[", @"<i>");
                                text = text.Replace("]", @"</i>");

                                // Split subtitle lines (one subtitle has one or more lines)
                                var subtitleLines = text.SplitToLines();
                                int count = 0;
                                var lineSb = new StringBuilder();
                                string tempLine = string.Empty;
                                foreach (string subtitleLine in subtitleLines)
                                {
                                    // Append line break in every line except the first one
                                    if (count > 0)
                                        lineSb.Append(Environment.NewLine);
                                    tempLine = subtitleLine;
                                    // Close italics in every line (if next line is in italics, SoftNI will use "[" at the beginning)
                                    if (Utilities.CountTagInText(tempLine, "<i>") > Utilities.CountTagInText(tempLine, "</i>"))
                                        tempLine = tempLine + "</i>";

                                    lineSb.Append(tempLine);
                                    count++;
                                }
                                text = lineSb.ToString();

                                // Replace "</i>line break<i>" with just a line break (SubRip does not need to close italics and open them again in the next line).
                                text = text.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);

                                // Subtitle appears at the top (add tag)
                                if (positionTop)
                                    text = "{\\an8}" + text;

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
                else if (string.IsNullOrWhiteSpace(line) || line.StartsWith('*'))
                {
                    // skip empty lines or start
                }
                else if (p != null)
                {
                    sb.AppendLine(line);
                }
            }

            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            TimeCode tc = new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(int.Parse(frames)));
            return tc;
        }

    }
}
