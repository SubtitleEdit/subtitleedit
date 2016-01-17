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

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 3"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            Subtitle subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            //150583||3968723||Rythme standard quatre-par-quatre.\~- Sûr... Accord d'entrée, D majeur?||
            //155822||160350||Rob n'y connait rien en claviers. Il\~commence chaque chanson en D majeur||

            const string paragraphWriteFormat = "{0}||{1}||{2}||";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (!subtitle.WasLoadedWithFrameNumbers)
                    p.CalculateFrameNumbersFromTimeCodes(Configuration.Settings.General.CurrentFrameRate);
                sb.AppendLine(string.Format(paragraphWriteFormat, p.StartFrame, p.EndFrame, p.Text.Replace(Environment.NewLine, "\\~")));
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
            string[] parts = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3)
            {
                int start;
                int end;
                if (int.TryParse(parts[0], out start) && int.TryParse(parts[1], out end))
                {
                    Paragraph p = new Paragraph(parts[2].Replace("\\~", Environment.NewLine), start, end);
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