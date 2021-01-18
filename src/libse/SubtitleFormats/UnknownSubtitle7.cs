using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Reported by dipa nuswantara
    /// </summary>
    public class UnknownSubtitle7 : SubtitleFormat
    {
        private enum ExpectingLine
        {
            TimeStart,
            Text,
            TimeEndOrText,
        }

        public override string Extension => ".txt";

        public override string Name => "Unknown 7";

        public override string ToText(Subtitle subtitle, string title)
        {
            //00:00:54:16 Bisakah kalian diam,Tolong!
            //00:00:56:07
            //00:00:57:16 Benar, tepatnya saya tidak memiliki "Anda
            //sudah mendapat 24 jam" adegan... tapi
            //00:01:02:03

            const string paragraphWriteFormat = "{0} {2}{3}{1}\t";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), p.Text, Environment.NewLine));
            }
            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var regexTimeCode = new Regex(@"^\d\d:\d\d:\d\d:\d\d ", RegexOptions.Compiled);
            var regexTimeCodeEnd = new Regex(@"^\d\d:\d\d:\d\d:\d\d\t$", RegexOptions.Compiled);

            var paragraph = new Paragraph();
            var expecting = ExpectingLine.TimeStart;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            int count = 0;
            foreach (string line in lines)
            {
                count++;
                if (regexTimeCode.IsMatch(line))
                {
                    string[] parts = line.Substring(0, 11).Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        try
                        {
                            var tc = DecodeTimeCodeFramesFourParts(parts);
                            if (expecting == ExpectingLine.TimeStart)
                            {
                                paragraph = new Paragraph();
                                paragraph.StartTime = tc;
                                expecting = ExpectingLine.Text;

                                if (line.Length > 12)
                                {
                                    paragraph.Text = line.Substring(12).Trim();
                                }
                            }
                        }
                        catch
                        {
                            _errorCount++;
                            expecting = ExpectingLine.TimeStart;
                        }
                    }
                }
                else if (regexTimeCodeEnd.IsMatch(line) || (count == lines.Count && regexTimeCodeEnd.IsMatch(line + "\t")))
                {
                    string[] parts = line.Substring(0, 11).Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        var tc = DecodeTimeCodeFramesFourParts(parts);
                        paragraph.EndTime = tc;
                        subtitle.Paragraphs.Add(paragraph);
                        paragraph = new Paragraph();
                        expecting = ExpectingLine.TimeStart;
                    }
                }
                else
                {
                    if (expecting == ExpectingLine.Text)
                    {
                        if (line.Length > 0)
                        {
                            string text = line.Replace("|", Environment.NewLine);
                            paragraph.Text += Environment.NewLine + text;
                            expecting = ExpectingLine.TimeEndOrText;

                            if (paragraph.Text.Length > 2000)
                            {
                                _errorCount += 100;
                                return;
                            }
                        }
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }
            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return time.ToHHMMSSFF();
        }

    }
}
