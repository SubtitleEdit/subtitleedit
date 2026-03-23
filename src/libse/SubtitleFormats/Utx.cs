using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Utx : SubtitleFormat
    {
        // #0:03:03.23,0:03:08.05
        private static readonly Regex RegexTimeCode = new Regex(@"^#\d\d?:\d\d:\d\d\.\d\d,\d:\d\d:\d\d\.\d\d$", RegexOptions.Compiled);

        public override string Extension => ".utx";

        public override string Name => "UTX";

        public override string ToText(Subtitle subtitle, string title)
        {
            //I'd forgotten.
            //#0:02:58.21,0:03:00.16

            //Were you somewhere far away?
            //- Yes, four years in Switzerland.
            //#0:03:02.15,0:03:06.14

            const string paragraphWriteFormat = "{0}{1}#{2},{3}{1}";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(paragraphWriteFormat, p.Text, Environment.NewLine, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime)));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            var sb = new StringBuilder();
            char[] splitChars = { '#', ',' };
            char[] splitChars2 = { ':', '.' };
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith('#') && RegexTimeCode.IsMatch(line))
                {
                    var timeParts = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        string[] startParts = timeParts[0].Split(splitChars2, StringSplitOptions.RemoveEmptyEntries);
                        string[] endParts = timeParts[1].Split(splitChars2, StringSplitOptions.RemoveEmptyEntries);
                        TimeCode start = DecodeTimeCodeFramesFourParts(startParts);
                        TimeCode end = DecodeTimeCodeFramesFourParts(endParts);
                        subtitle.Paragraphs.Add(new Paragraph(start, end, sb.ToString().Trim()));
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (line.Length > 0)
                {
                    sb.AppendLine(line);
                    if (sb.Length > 5000)
                    {
                        return;
                    }
                }
                else
                {
                    sb.Clear();
                }
            }
            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //0:03:02.15
            return $"{time.Hours}:{time.Minutes:00}:{time.Seconds:00}.{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

    }
}
