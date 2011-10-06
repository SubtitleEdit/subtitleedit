using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
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

        public override bool HasLineNumber
        {
            get { return false; }
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
            //I'd forgotten.
            //#0:02:58.21,0:03:00.16

            //Were you somewhere far away?
            //- Yes, four years in Switzerland.
            //#0:03:02.15,0:03:06.14

            const string paragraphWriteFormat = "{0}{1}#{2},{3}{1}";

            StringBuilder sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = p.Text;
                sb.AppendLine(string.Format(paragraphWriteFormat, p.Text, Environment.NewLine, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime)));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            string text = string.Empty;
            for (int i=0; i<lines.Count; i++)
            {
                string line = lines[i].Trim();

                if (line.StartsWith("#"))
                {
                    var timeParts = line.Split("#,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (timeParts.Length == 2)
                    {
                        try
                        {
                            TimeCode start = DecodeTimeCode(timeParts[0]);
                            TimeCode end = DecodeTimeCode(timeParts[1]);
                            subtitle.Paragraphs.Add(new Paragraph(start, end, text));
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
                else if (line.Length > 0)
                {
                    text = (text + Environment.NewLine + line).Trim();
                }
                else
                {
                    text = string.Empty;
                }
            }
            subtitle.Renumber(1);
        }

        private string EncodeTimeCode(TimeCode time)
        {
            //0:03:02.15
            int frames = (int)(time.Milliseconds / (1000.0 / Configuration.Settings.General.CurrentFrameRate));
            return string.Format("{0}:{1:00}:{2:00}.{3:00}", time.Hours, time.Minutes, time.Seconds, frames);
        }

        private TimeCode DecodeTimeCode(string timePart)
        {
            //0:03:02.15
            var parts = timePart.Split(":.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            int hours = int.Parse(parts[0]);
            int minutes = int.Parse(parts[1]);
            int seconds = int.Parse(parts[2]);
            int milliseconds = (int)((1000.0 / Configuration.Settings.General.CurrentFrameRate) * int.Parse(parts[3]));
            return new TimeCode(hours, minutes, seconds, milliseconds);
        }

    }
}