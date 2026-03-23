using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// SoftNi - http://www.softni.com/
    /// </summary>
    public class SoftNiSub : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d\.\d\d\\\d\d:\d\d:\d\d\.\d\d$", RegexOptions.Compiled);

        public override string Extension => ".sub";

        public override string Name => "SoftNi sub";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            var lineSb = new StringBuilder();
            sb.AppendLine("*PART 1*");
            sb.AppendLine("00:00:00.00\\00:00:00.00");
            const string writeFormat = "{0}{1}{2}\\{3}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = p.Text;
                bool positionTop = false;

                // If text starts with {\an8}, subtitle appears at the top
                if (text.StartsWith("{\\an8}", StringComparison.Ordinal))
                {
                    positionTop = true;
                    // Remove the tag {\an8}.
                    text = text.Remove(0, 6);
                }

                // Split lines (split a subtitle into its lines)
                var lines = text.SplitToLines();
                int count = 0;
                lineSb.Clear();
                bool nextLineInItalics = false;
                foreach (var line in lines)
                {
                    // Append line break in every line except the first one
                    if (count > 0)
                    {
                        lineSb.Append(Environment.NewLine);
                    }

                    var tempLine = line;

                    // This line should be in italics (it was detected in previous line)
                    if (nextLineInItalics)
                    {
                        tempLine = $"<i>{tempLine}";
                        nextLineInItalics = false;
                    }

                    if (tempLine.StartsWith("<i>", StringComparison.Ordinal) && tempLine.EndsWith("</i>", StringComparison.Ordinal))
                    {
                        // Whole line is in italics
                        // Remove <i> from the beginning
                        tempLine = tempLine.Remove(0, 3);
                        // Remove </i> from the end
                        tempLine = tempLine.Remove(tempLine.Length - 4, 4);
                        // Add new italics tag at the beginning
                        tempLine = $"[{tempLine}]";
                    }
                    else if (tempLine.StartsWith("<i>", StringComparison.Ordinal) && Utilities.CountTagInText(tempLine, "<i>") > Utilities.CountTagInText(tempLine, "</i>"))
                    {
                        // Line starts with <i> but italics are not closed. So the next line should be in italics
                        nextLineInItalics = true;
                        tempLine += "]";
                    }
                    lineSb.Append(tempLine);
                    count++;

                    text = lineSb.ToString();
                    // Replace remaining italics tags
                    text = text.Replace("<i>", @"[");
                    text = text.Replace("</i>]", @"]");
                    text = text.Replace("</i>", @"]");
                    text = HtmlUtil.RemoveHtmlTags(text);
                }

                // Add top-position SoftNI marker "}" at the beginning of first line.
                if (positionTop)
                {
                    text = "}" + text;
                }

                sb.AppendLine(string.Format(writeFormat, text, Environment.NewLine, p.StartTime.ToHHMMSSPeriodFF(), p.EndTime.ToHHMMSSPeriodFF()));
            }

            var last = subtitle.Paragraphs.Last();
            if (last == null)
            {
                sb.AppendLine("*END*");
                sb.AppendLine(@"...........\...........");
            }
            else if (!last.Text.StartsWith("*END", StringComparison.Ordinal))
            {
                var endTime = new TimeCode(last.EndTime.TotalMilliseconds + 1000.0) { Milliseconds = 0 };
                sb.AppendLine("*END*");
                sb.AppendLine($"{endTime.ToHHMMSSPeriodFF()}\\{endTime.ToHHMMSSPeriodFF()}");
            }

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
            //—Peter.
            //—Estoy de licencia.
            //01:48:50.07\01:48:52.01
            var sb = new StringBuilder();
            var lineSb = new StringBuilder();
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            char[] splitChars = { ':', '.' };
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (RegexTimeCodes.IsMatch(s))
                {
                    // Start and end time separated by "\"
                    var temp = s.Split('\\');
                    if (temp.Length > 1)
                    {
                        string start = temp[0];
                        string end = temp[1];

                        string[] startParts = start.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                        string[] endParts = end.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                        if (startParts.Length == 4 && endParts.Length == 4)
                        {
                            try
                            {
                                p = new Paragraph();
                                p.StartTime = DecodeTimeCodeFramesFourParts(startParts);
                                p.EndTime = DecodeTimeCodeFramesFourParts(endParts);
                                var text = sb.ToString().Trim();

                                var positionTop = false;
                                // If text starts with "}", subtitle appears at the top
                                if (text.StartsWith('}'))
                                {
                                    positionTop = true;
                                    // Remove the tag "{"
                                    text = text.Remove(0, 1);
                                }
                                // Replace tags
                                text = text.Replace("[", @"<i>");
                                text = text.Replace("]", @"</i>");

                                // Split subtitle lines (one subtitle has one or more lines)
                                var subtitleLines = text.SplitToLines();
                                int count = 0;
                                lineSb.Clear();
                                foreach (string subtitleLine in subtitleLines)
                                {
                                    // Append line break in every line except the first one
                                    if (count > 0)
                                    {
                                        lineSb.Append(Environment.NewLine);
                                    }

                                    var tempLine = subtitleLine;
                                    // Close italics in every line (if next line is in italics, SoftNI will use "[" at the beginning)
                                    if (Utilities.CountTagInText(tempLine, "<i>") > Utilities.CountTagInText(tempLine, "</i>"))
                                    {
                                        tempLine = tempLine + "</i>";
                                    }

                                    lineSb.Append(tempLine);
                                    count++;
                                }
                                text = lineSb.ToString();

                                // Replace "</i>line break<i>" with just a line break (SubRip does not need to close italics and open them again in the next line).
                                text = text.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);

                                // Subtitle appears at the top (add tag)
                                if (positionTop)
                                {
                                    text = "{\\an8}" + text;
                                }

                                p.Text = text;
                                if (text.Length > 0)
                                {
                                    subtitle.Paragraphs.Add(p);
                                }

                                sb.Clear();
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
                    // skip empty lines
                }
                else if (p != null)
                {
                    sb.AppendLine(line);
                }
            }

            subtitle.Renumber();
        }
    }
}
