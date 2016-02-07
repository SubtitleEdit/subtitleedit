using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class QubeMasterImport : SubtitleFormat
    {
        // ToText code by Tosil Velkoff, tosil@velkoff.net
        // Based on UnknownSubtitle44
        //SubLine1
        //SubLine2
        //10:01:04:12
        //10:01:07:09
        private readonly static Regex RegexTimeCodes1 = new Regex(@"^\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "QubeMasterPro Import"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();

            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);

            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            int index = 0;

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (index != 0) sb.AppendLine();
                index++;

                sb.AppendLine(HtmlUtil.RemoveHtmlTags(p.Text));
                sb.AppendLine(EncodeTimeCode(p.StartTime));
                sb.AppendLine(EncodeTimeCode(p.EndTime));
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return time.ToHHMMSSFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            bool expectStartTime = true;
            var p = new Paragraph();
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                string s = line.Trim();
                var match = RegexTimeCodes1.Match(s);
                if (match.Success)
                {
                    string[] parts = s.Split(':');
                    if (parts.Length == 4)
                    {
                        try
                        {
                            if (expectStartTime)
                            {
                                p.StartTime = DecodeTimeCodeFramesFourParts(parts);
                                expectStartTime = false;
                            }
                            else
                            {
                                if (p.EndTime.TotalMilliseconds < 0.01)
                                    _errorCount++;
                                p.EndTime = DecodeTimeCodeFramesFourParts(parts);
                            }
                        }
                        catch (Exception exception)
                        {
                            _errorCount++;
                            System.Diagnostics.Debug.WriteLine(exception.Message);
                        }
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    if (p.StartTime.TotalMilliseconds == 0 && p.EndTime.TotalMilliseconds == 0)
                        _errorCount++;
                    else
                        subtitle.Paragraphs.Add(p);
                    p = new Paragraph();
                }
                else
                {
                    expectStartTime = true;
                    p.Text = (p.Text + Environment.NewLine + line).Trim();
                    if (p.Text.Length > 500)
                    {
                        _errorCount += 10;
                        return;
                    }
                }
            }
            if (p.EndTime.TotalMilliseconds > 0)
                subtitle.Paragraphs.Add(p);

            bool allNullEndTime = true;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                if (subtitle.Paragraphs[i].EndTime.TotalMilliseconds != 0)
                    allNullEndTime = false;
            }
            if (allNullEndTime)
                subtitle.Paragraphs.Clear();

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}
