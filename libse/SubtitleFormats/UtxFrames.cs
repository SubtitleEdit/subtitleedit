using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UtxFrames : SubtitleFormat
    {
        public override string Extension => ".utx";

        public override string Name => "UTX (frames)";

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
                    {
                        return;
                    }
                }
                else
                {
                    text.Clear();
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
            int milliseconds = (int)Math.Round(TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate * int.Parse(timePart));
            return new TimeCode(milliseconds);
        }

    }
}
