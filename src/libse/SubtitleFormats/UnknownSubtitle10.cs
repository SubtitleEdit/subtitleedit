using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle10 : SubtitleFormat
    {
        public override string Extension => ".txt";

        public override string Name => "Unknown 10";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.Append("{\"language_code\":\"en\",\"subtitles\":[");
            int i = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }

                sb.Append('{');
                sb.AppendFormat("\"content\":\"{0}\",\"start_time\":{1},\"end_time\":{2}", p.Text.Replace(Environment.NewLine, " <br> "), ((long)(Math.Round(p.StartTime.TotalMilliseconds))).ToString(CultureInfo.InvariantCulture), ((long)(Math.Round(p.EndTime.TotalMilliseconds))).ToString(CultureInfo.InvariantCulture));
                sb.Append('}');
                i++;
            }
            sb.Append("]}");
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var temp = new StringBuilder();
            foreach (string l in lines)
            {
                temp.Append(l);
            }

            string all = temp.ToString();
            if (!all.Contains("{\"content\":\""))
            {
                return;
            }

            var arr = all.Replace("\n", string.Empty).Replace("{\"content\":\"", "\n").Split('\n');

            _errorCount = 0;
            subtitle.Paragraphs.Clear();

            // {"content":"La ce se gandeste  Oh Ha Ni a noastra <br> de la inceputul dimineti?","start_time":314071,"end_time":317833},
            for (int i = 0; i < arr.Length; i++)
            {
                string line = arr[i].Trim();

                int indexStartTime = line.IndexOf("\"start_time\":", StringComparison.Ordinal);
                int indexEndTime = line.IndexOf("\"end_time\":", StringComparison.Ordinal);
                if (indexStartTime > 0 && indexEndTime > 0)
                {
                    int indexEndText = indexStartTime;
                    if (indexStartTime > indexEndTime)
                    {
                        indexEndText = indexEndTime;
                    }

                    string text = line.Substring(0, indexEndText - 1).Trim().TrimEnd('\"');
                    text = text.Replace("<br>", Environment.NewLine).Replace("<BR>", Environment.NewLine);
                    text = text.Replace("<br/>", Environment.NewLine).Replace("<BR/>", Environment.NewLine);
                    text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                    text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                    text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                    text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                    text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                    text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                    try
                    {
                        string start = line.Substring(indexStartTime);
                        string end = line.Substring(indexEndTime);
                        var paragraph = new Paragraph
                        {
                            Text = text,
                            StartTime = { TotalMilliseconds = GetMilliseconds(start) },
                            EndTime = { TotalMilliseconds = GetMilliseconds(end) }
                        };
                        subtitle.Paragraphs.Add(paragraph);
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                        _errorCount++;
                    }
                }
            }
            subtitle.Renumber();
        }

        private static double GetMilliseconds(string start)
        {
            while (start.Length > 1 && !start.StartsWith(':'))
            {
                start = start.Remove(0, 1);
            }

            start = start.Trim().Trim(':').Trim('"').Trim();

            int i = 0;
            while (i < start.Length && char.IsDigit(start[i]))
            {
                i++;
            }

            return int.Parse(start.Substring(0, i));
        }

    }
}
