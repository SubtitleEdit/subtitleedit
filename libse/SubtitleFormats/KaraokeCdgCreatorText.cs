using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class KaraokeCdgCreatorText : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+(:\d+){0,1}\.\d+ : \d+(:\d+){0,1}\.\d+ : .+", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Karaoke CDG Creator text";

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //59.874 : 1:00.113 : Toa*
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;

            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    var temp = line.Replace(" : ", "\0").Split('\0');
                    string start = temp[0];
                    string end = temp[1];
                    char[] splitChars = { ':', '.', ',' };
                    string[] startParts = start.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length >= 2 && startParts.Length <= 4 && (endParts.Length >= 2 && endParts.Length <= 4))
                    {
                        try
                        {
                            string text = line.Remove(0, temp[0].Length + temp[1].Length + 6).Trim().TrimStart('\0');
                            text = text.Replace("\\n", Environment.NewLine);
                            p = new Paragraph(DecodeTimeCode(startParts), DecodeTimeCode(endParts), text);
                            subtitle.Paragraphs.Add(p);
                        }
                        catch (Exception exception)
                        {
                            _errorCount++;
                            System.Diagnostics.Debug.WriteLine(exception.Message);
                        }
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip empty lines
                }
                else if (p != null)
                {
                    _errorCount++;
                }
            }

            // merge lines
            int i = 0;
            while (i < subtitle.Paragraphs.Count - 1)
            {
                p = subtitle.Paragraphs[i];
                var next = subtitle.Paragraphs[i + 1];
                if (next.StartTime.TotalMilliseconds - 1000 < p.EndTime.TotalMilliseconds &&
                    !(next.Text.StartsWith(Environment.NewLine, StringComparison.Ordinal) || p.Text.EndsWith(Environment.NewLine, StringComparison.Ordinal)))
                {
                    p.Text += " " + next.Text;
                    p.EndTime.TotalMilliseconds = next.EndTime.TotalMilliseconds;
                    subtitle.Paragraphs.RemoveAt(i + 1);
                }
                else
                {
                    i++;
                }
            }

            // remove "*" (syllable separator) and trim
            foreach (var paragraph in subtitle.Paragraphs)
            {
                paragraph.Text = paragraph.Text.Replace("* ", string.Empty).RemoveChar('*').Trim();
            }

            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            if (parts.Length == 4)
            {
                return DecodeTimeCodeMsFourParts(parts);
            }
            else if (parts.Length == 3)
            {
                return new TimeCode(0, int.Parse(parts[1]), int.Parse(parts[1]), int.Parse(parts[2]));
            }
            return new TimeCode(0, 0, int.Parse(parts[0]), int.Parse(parts[1]));
        }
    }
}
