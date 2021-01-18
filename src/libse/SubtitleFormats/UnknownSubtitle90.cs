using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle90 : SubtitleFormat
    {
        //data_000001 1 0.13 0.63 Hello
        private static readonly Regex RegexTimeCodes = new Regex(@"^data_\d+ \d \d+\.\d+ \d+\.\d+ ", RegexOptions.Compiled);

        public override string Extension => ".ctm";

        public override string Name => "Unknown 90";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            const string writeFormat = "data_000001 1 {0:0.00} {1:0.00} {2}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, writeFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.Duration), HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, "<br>"))));
            }
            return sb.ToString();
        }

        private static double EncodeTimeCode(TimeCode time)
        {
            return time.TotalSeconds;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                var match = RegexTimeCodes.Match(line);
                if (match.Success)
                {
                    var arr = match.Value.Split(' ');
                    string text = line.Remove(0, match.Length).Trim();
                    text = text.Replace("<br>", Environment.NewLine);
                    text = text.Replace("</br>", Environment.NewLine);
                    try
                    {
                        var dur = DecodeTimeCode(arr[3]);
                        var start = DecodeTimeCode(arr[2]);
                        var p = new Paragraph(start, new TimeCode(start.TotalMilliseconds + dur.TotalMilliseconds), text);
                        subtitle.Paragraphs.Add(p);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                    if (text.Length > 0 && char.IsDigit(text[0]))
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    _errorCount += 2;
                }
            }
            foreach (Paragraph p2 in subtitle.Paragraphs)
            {
                p2.Text = Utilities.AutoBreakLine(p2.Text);
            }
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string s)
        {
            var seconds = Convert.ToDouble(s, CultureInfo.InvariantCulture);
            return new TimeCode(seconds * 1000.0);
        }
    }
}
