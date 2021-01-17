using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle75 : SubtitleFormat
    {

        //BOSTA - English
        //REEL 1
        //0001:   124+12    127+12
        //Get going, you idiot!
        //0002:   139+13    143+07
        //They scorn it, but they don't
        //know its true value.
        //0003:   143+12    146+09
        //Move on, move on!
        //0004:   147+04    151+00
        //In 1943, it was one of a kind.
        //0005:   159+00    161+15
        //...
        //1083:  1575+05   1583+09
        //THE END IS THE BEGINNING
        //1084:  1918+12      0+00
        //END OF THE FILM

        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\d\d:\s+\d+\+\d+\s+\d+\+\d+$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 75";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0:0000}:   {1}    {2}\r\n{3}";

            var sb = new StringBuilder();
            sb.AppendLine(title);
            sb.AppendLine("REEL 1                                ");
            for (int index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                Paragraph p = subtitle.Paragraphs[index];
                sb.AppendFormat(paragraphWriteFormat, index + 1, GetTimeCode(p.StartTime), GetTimeCode(p.EndTime), p.Text);
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }

        private static string GetTimeCode(TimeCode tc)
        {
            var seconds = (int)tc.TotalSeconds;
            var frames = MillisecondsToFrames(tc.Milliseconds);
            return $"{seconds:00}+{frames:00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            int i = 0;
            Paragraph paragraph = null;
            while (i < lines.Count)
            {
                string line = lines[i].TrimEnd();
                if (line.Length > 8 && line[4] == ':' && RegexTimeCodes.IsMatch(line))
                {
                    if (paragraph != null)
                    {
                        if (!string.IsNullOrWhiteSpace(paragraph.Text))
                        {
                            subtitle.Paragraphs.Add(paragraph);
                        }
                        else
                        {
                            _errorCount++;
                        }
                    }

                    paragraph = new Paragraph();
                    var arr = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 3)
                    {
                        try
                        {
                            paragraph.StartTime = TryReadTimeCodesLine(arr[1]);
                            paragraph.EndTime = TryReadTimeCodesLine(arr[2]);
                        }
                        catch (Exception)
                        {
                            _errorCount++;
                        }
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    if (paragraph != null && paragraph.Text.Length < 500)
                    {
                        paragraph.Text = (paragraph.Text + Environment.NewLine + line).Trim();
                    }
                    else if (paragraph != null && paragraph.Text.Length > 500)
                    {
                        _errorCount++;
                        return;
                    }
                }
                i++;
            }
            if (paragraph != null && !string.IsNullOrWhiteSpace(paragraph.Text) && paragraph.Text != "END OF THE FILM")
            {
                subtitle.Paragraphs.Add(paragraph);
            }
            subtitle.Renumber();
        }

        private static TimeCode TryReadTimeCodesLine(string line)
        {
            string[] parts = line.Split('+');
            return new TimeCode(0, 0, int.Parse(parts[0]), FramesToMillisecondsMax999(int.Parse(parts[1])));
        }

    }
}
