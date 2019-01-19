using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle31 : SubtitleFormat
    {

        private static readonly Regex RegexTimeCode = new Regex(@"^\d+\.\d\d$", RegexOptions.Compiled);

        private enum ExpectingLine
        {
            Number,
            TimeStart,
            TimeEnd,
            Text
        }

        public override string Extension => ".txt";

        public override string Name => "Unknown 31";

        public override string ToText(Subtitle subtitle, string title)
        {
            //1.
            //8.03
            //10.06
            //- Labai aèiû.
            //- Jûs rimtai?

            //2.
            //16.00
            //19.06
            //Kaip reikalai ðunø grobimo versle?

            const string paragraphWriteFormat = "{4}.{3}{0}{3}{1}{3}{2}{3}";
            var sb = new StringBuilder();
            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                count++;
                var text = HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagFont);
                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), text, Environment.NewLine, count));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            var expecting = ExpectingLine.Number;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            char[] splitChar = { '.' };
            foreach (string line in lines)
            {
                if (line.EndsWith('.') && Utilities.IsInteger(line.TrimEnd('.')))
                {
                    if (!string.IsNullOrEmpty(paragraph?.Text))
                    {
                        subtitle.Paragraphs.Add(paragraph);
                    }

                    paragraph = new Paragraph();
                    expecting = ExpectingLine.TimeStart;
                }
                else if (paragraph != null && expecting == ExpectingLine.TimeStart && RegexTimeCode.IsMatch(line))
                {
                    string[] parts = line.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        try
                        {
                            var tc = DecodeTimeCode(parts);
                            paragraph.StartTime = tc;
                            expecting = ExpectingLine.TimeEnd;
                        }
                        catch
                        {
                            _errorCount++;
                            expecting = ExpectingLine.Number;
                        }
                    }
                }
                else if (paragraph != null && expecting == ExpectingLine.TimeEnd && RegexTimeCode.IsMatch(line))
                {
                    string[] parts = line.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        try
                        {
                            var tc = DecodeTimeCode(parts);
                            paragraph.EndTime = tc;
                            expecting = ExpectingLine.Text;
                        }
                        catch
                        {
                            _errorCount++;
                            expecting = ExpectingLine.Number;
                        }
                    }
                }
                else
                {
                    if (paragraph != null && expecting == ExpectingLine.Text)
                    {
                        if (line.Length > 0)
                        {
                            string s = line;
                            paragraph.Text = (paragraph.Text + Environment.NewLine + s).Trim();
                            if (paragraph.Text.Length > 2000)
                            {
                                _errorCount += 100;
                                return;
                            }
                        }
                    }
                    else if (line.Length > 0 && line != "@ headers")
                    {
                        _errorCount++;
                    }
                }
            }
            if (!string.IsNullOrEmpty(paragraph?.Text))
            {
                subtitle.Paragraphs.Add(paragraph);
            }

            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            int frames = MillisecondsToFrames(time.TotalMilliseconds);
            int footage = frames / 16;
            int rest = (int)Math.Round(frames % 16.0 / 16.0 * Configuration.Settings.General.CurrentFrameRate);
            return $"{footage}.{rest:00}";
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            var frames16 = int.Parse(parts[0]);
            var frames = int.Parse(parts[1]);
            return new TimeCode(0, 0, 0, FramesToMilliseconds(16 * frames16 + frames));
        }

    }
}
