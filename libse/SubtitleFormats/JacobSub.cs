using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Implementation of JacobSub. Specs: (http://unicorn.us.com/jacosub/jscripts.html)
    /// </summary>
    public class JacobSub : SubtitleFormat
    {
        // H:MM:SS.FF H:MM:SS.FF directive {comment} text {comment} more text...
        // 0:30:57.22 0:30:59.46 vm {opening credit} A Film By Akira Kurosawa

        private static readonly Regex RegexTimeCode = new Regex(@"^(\d:\d\d:\d\d\.\d\d) (\d:\d\d:\d\d\.\d\d)", RegexOptions.Compiled);

        /// <summary>
        /// Each character code begins with an alphabet character followed by arguments
        /// made up of other alphabet characters and numbers
        /// </summary>
        private static readonly Regex RegexDirectives = new Regex("^[a-z\\d]+(?= )", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        public override string Extension => ".jss";

        public override string Name => "JacobSub";

        public override bool IsTimeBased => true;

        public override bool IsMine(List<string> lines, string fileName)
        {
            // only validate/check file extension if file exists
            if (File.Exists(fileName) && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            // reset members
            subtitle.Paragraphs.Clear();
            _errorCount = 0;

            // must only be used with matched value of RegexTimeCode
            char[] timeSplitChar = { ':', '.', ' ' };

            int lineCount = lines.Count;
            var paragraph = new Paragraph();
            for (int i = 0; i < lineCount; i++)
            {
                string line = lines[i].Trim();
                string lineNext = string.Empty;

                if (i + 1 < lineCount)
                {
                    lineNext = lines[i + 1].Trim();
                }

                Match match = null;
                if (line.Length >= 21)
                {
                    match = RegexTimeCode.Match(line);
                }

                if (match?.Success == true)
                {
                    // save previous read paragraph
                    if (paragraph?.Text.Length > 0)
                    {
                        subtitle.Paragraphs.Add(paragraph);
                    }

                    int len = match.Value.Length;
                    paragraph = new Paragraph()
                    {
                        StartTime = DecodeTime(match.Groups[1].Value, timeSplitChar),
                        EndTime = DecodeTime(match.Groups[2].Value, timeSplitChar),
                        Text = DecodeText(line.Substring(len))
                    };
                }
                else
                {
                    if (paragraph.Text.Length == 0)
                    {
                        _errorCount++;
                    }
                    else
                    {
                        paragraph.Text += (Environment.NewLine + DecodeText(line)).TrimEnd();
                    }
                }

                // read last line
                if (i + 1 == lineCount && !RegexTimeCode.IsMatch(lineNext))
                {
                    paragraph.Text = paragraph.Text.Trim();
                    subtitle.Paragraphs.Add(paragraph);
                }
            }

            subtitle.Renumber();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            // 0:30:57.22 0:30:59.46 vm {opening credit} A Film By Akira Kurosawa
            string writeFormat = "{0} {1} D {2}" + Environment.NewLine;

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string startTime = EncodeTime(p.StartTime);
                string endTime = EncodeTime(p.EndTime);
                string text = HtmlUtil.RemoveHtmlTags(p.Text, true);

                sb.AppendFormat(writeFormat, startTime, endTime, text);
            }
            return sb.ToString();
        }

        private static TimeCode DecodeTime(string timestamp, char[] splitChars)
        {
            // H:MM:SS.FF H:MM:SS.FF
            string[] tokens = timestamp.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);

            // parse tokens
            int hours = int.Parse(tokens[0]);
            int minutes = int.Parse(tokens[1]);
            int seconds = int.Parse(tokens[2]);
            int milliseconds = FramesToMilliseconds(double.Parse(tokens[3]));

            return new TimeCode(hours, minutes, seconds, milliseconds);
        }

        /// <summary>
        /// Returns time code encoded in this format: H:MM:SS.FF H:MM:SS.FF
        /// </summary>
        private static string EncodeTime(TimeCode tc) => $"{tc.Hours:#0}:{tc.Minutes:00}:{tc.Seconds:00}.{MillisecondsToFrames(tc.Milliseconds):00}";

        private static string DecodeText(string input)
        {
            input = input.Trim();

            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // remove all comment
            int idx = input.IndexOf('{');

            // You don't even need this directive at all, if your text begins with a
            // non -alphabetic character (such as a {comment}, number, etc.).
            bool startsWithComment = idx == 0;

            while (idx >= 0)
            {
                int endIdx = input.IndexOf('}');
                if (endIdx < idx)
                {
                    break;
                }
                input = input.Remove(idx, endIdx - idx + 1);
                idx = input.IndexOf('{', idx);
            }

            // remove leading chars
            input = input.Replace("~", string.Empty);
            input = input.FixExtraSpaces();
            input = input.TrimStart();

            // do not include directives
            Match matchDirective = RegexDirectives.Match(input);
            if (startsWithComment || !(matchDirective.Success && IsDirective(matchDirective.Value)))
            {
                return input.Trim();
            }
            return input.Substring(matchDirective.Value.Length).Trim();
        }

        /// <summary>
        ///  A directive determines a subtitle's position, font, style, color, and so forth.
        ///  Each character code begins with an alphabet character followed by arguments made up
        ///  of other alphabet characters and numbers.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static bool IsDirective(string input)
        {
            // default directives
            if (input.Equals("d", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // vertival positioning

            // horizontal positioning

            // fonts

            // genlock fader control (amiga only)

            // iff graphic files

            // special effects

            //argument directives

            // time track

            return false;
        }
    }
}
