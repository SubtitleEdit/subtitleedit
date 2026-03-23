using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// MacSub (reading/writing).
    /// http://devel.aegisub.org/wiki/SubtitleFormats/Macsub
    /// </summary>
    public class MacSub : SubtitleFormat
    {
        private enum Expecting
        {
            StartFrame,
            Text,
            EndFrame
        }

        public override string Extension => ".txt";

        public override bool IsTimeBased => false;

        public override string Name => "MacSub";

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var expecting = Expecting.StartFrame;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            char[] trimChar = { '/' };
            var p = new Paragraph();
            for (int i = 0, lineNumber = 1; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                string nextLine = null;
                if (i + 1 < lines.Count)
                {
                    nextLine = lines[i + 1].Trim();
                }
                try
                {
                    switch (expecting)
                    {
                        case Expecting.StartFrame:
                            if (ContainsOnlyNumber(line))
                            {
                                p.StartTime.TotalMilliseconds = FramesToMilliseconds(int.Parse(line.TrimStart(trimChar)));
                                expecting = Expecting.Text;
                            }
                            else
                            {
                                _errorCount++;
                            }
                            break;

                        case Expecting.Text:
                            line = HtmlUtil.RemoveHtmlTags(line, true);
                            p.Text += string.IsNullOrEmpty(p.Text) ? line : Environment.NewLine + line;
                            // Next reading is going to be endframe if next line starts with (/) delimeter which indicates frame start.
                            if ((nextLine == null) || (nextLine.Length > 0 && nextLine[0] == '/'))
                            {
                                expecting = Expecting.EndFrame;
                                p.Number = lineNumber++;
                            }
                            break;

                        case Expecting.EndFrame:
                            if (ContainsOnlyNumber(line))
                            {
                                p.EndTime.TotalMilliseconds = FramesToMilliseconds(int.Parse(line.TrimStart(trimChar)));
                                subtitle.Paragraphs.Add(p);
                                // Prepare for next reading.
                                p = new Paragraph();
                                expecting = Expecting.StartFrame;
                            }
                            else
                            {
                                _errorCount++;
                            }
                            break;
                    }
                }
                catch
                {
                    _errorCount++;
                }
            }

        }

        public override string ToText(Subtitle subtitle, string title)
        {
            // Startframe
            // Text
            // Endframe.
            const string writeFormat = "/{0}{3}{1}{3}/{2}{3}";
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendFormat(writeFormat, MillisecondsToFrames(p.StartTime.TotalMilliseconds), HtmlUtil.RemoveHtmlTags(p.Text, true),
                    MillisecondsToFrames(p.EndTime.TotalMilliseconds), Environment.NewLine);
            }
            return sb.ToString();
        }

        public static bool ContainsOnlyNumber(string input)
        {
            int len = input.Length;
            // 10 = length of int.MaxValue (2147483647); +1 if starts with '/'
            if (len == 0 || len > 11 || input[0] != '/')
            {
                return false;
            }

            int halfLen = len / 2;
            for (int i = 1; i <= halfLen; i++) // /10.0 (Do not parse double)
            {
                if (!(CharUtils.IsDigit(input[i]) && CharUtils.IsDigit(input[len - i])))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
