using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class RealTime : SubtitleFormat
    {
        public override string Extension => ".rt";

        public override string Name => "RealTime";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<Window" + Environment.NewLine +
                "  Width    = \"640\"" + Environment.NewLine +
                "  Height   = \"480\"" + Environment.NewLine +
                "  WordWrap = \"true\"" + Environment.NewLine +
                "  Loop     = \"true\"" + Environment.NewLine +
                "  bgcolor  = \"black\"" + Environment.NewLine +
                ">" + Environment.NewLine +
                "<Font" + Environment.NewLine +
                "  Color = \"white\"" + Environment.NewLine +
                "  Face  = \"Arial\"" + Environment.NewLine +
                "  Size  = \"+2\"" + Environment.NewLine +
                ">" + Environment.NewLine +
                "<center>" + Environment.NewLine +
                "<b>" + Environment.NewLine);
            const string writeFormat = "<Time begin=\"{0}\" end=\"{1}\" /><clear/>{2}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //<Time begin="0:03:24.8" end="0:03:29.4" /><clear/>Man stjæler ikke fra Chavo, nej.
                sb.AppendLine(string.Format(writeFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), p.Text.Replace(Environment.NewLine, " ")));
            }
            sb.AppendLine("</b>");
            sb.AppendLine("</center>");
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //0:03:24.8
            return $"{time.Hours:0}:{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds / 100:0}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //<Time begin="0:03:24.8" end="0:03:29.4" /><clear/>Man stjæler ikke fra Chavo, nej.
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                try
                {
                    if (line.Contains("<Time ") && line.Contains(" begin=") && line.Contains("end="))
                    {
                        int indexOfBegin = line.IndexOf(" begin=", StringComparison.Ordinal);
                        int indexOfEnd = line.IndexOf(" end=", StringComparison.Ordinal);
                        string begin = line.Substring(indexOfBegin + 7, 11);
                        string end = line.Substring(indexOfEnd + 5, 11);

                        string[] startParts = begin.Split(new[] { ':', '.', '"' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] endParts = end.Split(new[] { ':', '.', '"' }, StringSplitOptions.RemoveEmptyEntries);
                        if (startParts.Length == 4 && endParts.Length == 4)
                        {
                            string text = line.Substring(line.LastIndexOf("/>", StringComparison.Ordinal) + 2);
                            var p = new Paragraph(DecodeTimeCode(startParts), DecodeTimeCode(endParts), text);
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                }
                catch
                {
                    _errorCount++;
                }
            }

            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            //[00:06:51.48]
            var hour = int.Parse(parts[0]);
            var minutes = int.Parse(parts[1]);
            var seconds = int.Parse(parts[2]);
            var millisesonds = int.Parse(parts[3]);

            return new TimeCode(hour, minutes, seconds, millisesonds * 10);
        }

    }
}
