using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Utx : SubtitleFormat
    {

        public override string Extension
        {
            get { return ".utx"; }
        }

        public override string Name
        {
            get { return "UTX"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

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
                string text = p.Text;
                sb.AppendLine(string.Format(paragraphWriteFormat, p.Text, Environment.NewLine, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime)));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            var text = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                if (line.StartsWith('#'))
                {
                    var timeParts = line.Split(new[] { '#', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (timeParts.Length == 2)
                    {
                        try
                        {
                            TimeCode start = DecodeTimeCode(timeParts[0]);
                            TimeCode end = DecodeTimeCode(timeParts[1]);
                            subtitle.Paragraphs.Add(new Paragraph(start, end, text.ToString().Trim()));
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
                else if (line.Length > 0)
                {
                    text.AppendLine(line.Trim());
                    if (text.Length > 5000)
                        return;
                }
                else
                {
                    text = new StringBuilder();
                }
            }
            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //0:03:02.15
            return string.Format("{0}:{1:00}:{2:00}.{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        private static TimeCode DecodeTimeCode(string timePart)
        {
            //0:03:02.15
            var parts = timePart.Split(new[] { ':', '.' }, StringSplitOptions.RemoveEmptyEntries);

            int hours = int.Parse(parts[0]);
            int minutes = int.Parse(parts[1]);
            int seconds = int.Parse(parts[2]);
            int milliseconds = (int)((TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate) * int.Parse(parts[3]));
            return new TimeCode(hours, minutes, seconds, milliseconds);
        }

    }
}