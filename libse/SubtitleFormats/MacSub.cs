using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Implements MacSub (reading/writing).
    /// http://devel.aegisub.org/wiki/SubtitleFormats/Macsub
    /// </summary>
    public class MacSub : SubtitleFormat
    {
        /// <summary>
        /// Enum expecting line.
        /// </summary>
        private enum Expecting
        {
            StartFrame,
            Text,
            EndFrame
        }

        public override string Extension
        {
            get
            {
                return ".txt";
            }
        }

        public override bool IsTimeBased
        {
            get
            {
                return false;
            }
        }

        public override string Name
        {
            get
            {
                return "MacSub";
            }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            // Filter by extension, do not allow (.json, .xml, .srt...)
            if (fileName == null || !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            var macSub = new Subtitle();
            LoadSubtitle(macSub, lines, fileName);
            return macSub.Paragraphs.Count > _errorCount;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var expecting = Expecting.StartFrame;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            char[] trimChar = { '/' };
            var p = new Paragraph();
            for (int i = 0; i < lines.Count; i++)
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
                            // 10  = length of int.MaxValues (2147483647); +1 = if contain '/'
                            if (line.Length <= 10 + 1 && (CharUtils.IsDigit(line[0]) || line[0] == '/'))
                            {
                                p.StartFrame = int.Parse(line.TrimStart(trimChar));
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
                                p.Number = i;
                            }
                            break;

                        case Expecting.EndFrame:
                            p.EndFrame = int.Parse(line.TrimStart(trimChar));
                            subtitle.Paragraphs.Add(p);
                            // Prepare for next reading.
                            p = new Paragraph();
                            expecting = Expecting.StartFrame;
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
    }
}
