using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class PowerkaraokeText : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+(:\d+){0,1}\.\d+ : \d+(:\d+){0,1}\.\d+ : .+", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Powerkaraoke text";

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        private static string EncodeTimeCode(TimeCode ts)
        {
            if (ts.Hours == 0 && ts.Minutes == 0 && ts.Seconds == 0)
            {
                return string.Format("{0:0}.{1:000}", ts.Seconds, ts.Milliseconds);
            }
            return string.Format("{0:0}:{1:00}.{2:000}", ts.Minutes, ts.Seconds, ts.Milliseconds);
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
                    if ((startParts.Length >= 2 && startParts.Length <= 4) && (endParts.Length >= 2 && endParts.Length <= 4))
                    {
                        try
                        {
                            string text = line.Remove(0, temp[0].Length + temp[1].Length).Trim().TrimStart(':');
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
            subtitle.Renumber();
        }

        private TimeCode DecodeTimeCode(string[] parts)
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
