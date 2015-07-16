using System;
using System.Collections.Generic;
using System.Text;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class UtxFrames : SubtitleFormat
    {

        public override string Extension
        {
            get { return ".utx"; }
        }

        public override string Name
        {
            get { return "UTX (frames)"; }
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
            //#2060,2188

            //Were you somewhere far away?
            //- Yes, four years in Switzerland.
            //#3885,3926

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
            long frames = (long)(time.TotalMilliseconds / (TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate));
            return frames.ToString();
        }

        private static TimeCode DecodeTimeCode(string timePart)
        {
            int milliseconds = (int)((TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate) * int.Parse(timePart));
            return new TimeCode(milliseconds);
        }

    }
}