using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    //Subtitle number: 1
    //Start time (or frames): 00:00:48,862:0000001222
    //End time (or frames): 00:00:50,786:0000001270
    //Subtitle text: In preajma lacului Razel,
    public class UnknownSubtitle3 : SubtitleFormat
    {
        private static readonly char[] SplitChar = { '|' };

        public override string Extension => ".txt";

        public override string Name => "Unknown 3";

        public override string ToText(Subtitle subtitle, string title)
        {
            //150583||3968723||Rythme standard quatre-par-quatre.\~- Sûr... Accord d'entrée, D majeur?||
            //155822||160350||Rob n'y connait rien en claviers. Il\~commence chaque chanson en D majeur||

            const string paragraphWriteFormat = "{0}||{1}||{2}||";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(paragraphWriteFormat, MillisecondsToFrames(p.StartTime.TotalMilliseconds), MillisecondsToFrames(p.EndTime.TotalMilliseconds), p.Text.Replace(Environment.NewLine, "\\~")));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            foreach (string line in lines)
            {
                ReadLine(subtitle, line);
            }
            subtitle.Renumber();
        }

        private void ReadLine(Subtitle subtitle, string line)
        {
            // 150583||3968723||Rythme standard quatre-par-quatre.\~- Sûr... Accord d'entrée, D majeur?||
            // 155822||160350||Rob n'y connait rien en claviers. Il\~commence chaque chanson en D majeur||
            string[] parts = line.Split(SplitChar, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3)
            {
                int start;
                int end;
                if (int.TryParse(parts[0], out start) && int.TryParse(parts[1], out end))
                {
                    var p = new Paragraph(parts[2].Replace("\\~", Environment.NewLine), start, end);
                    subtitle.Paragraphs.Add(p);
                }
                else
                {
                    _errorCount++;
                }
            }
            else
            {
                _errorCount++;
            }
        }

    }

}
