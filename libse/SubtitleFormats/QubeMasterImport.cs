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
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "QubeMasterPro Import";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(HtmlUtil.RemoveHtmlTags(p.Text));
                sb.AppendLine(EncodeTimeCode(p.StartTime));
                sb.AppendLine(EncodeTimeCode(p.EndTime));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time) => time.ToHHMMSSFF();

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            bool expectStartTime = true;
            var p = new Paragraph();
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                string s = line.Trim();
                Match match = null;

                // try to match using regex is length of current line is exactly 11
                if (s.Length == 11)
                {
                    match = RegexTimeCodes.Match(s);
                }
                if (match?.Success == true)
                {
                    string[] tokens = s.Split(':');
                    try
                    {
                        (expectStartTime ? p.StartTime : p.EndTime).TotalMilliseconds = DecodeTimeCodeFramesFourParts(tokens).TotalMilliseconds;
                    }
                    catch (Exception exception)
                    {
                        _errorCount++;
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    if (IsValidParagraph(p))
                    {
                        p.Number = subtitle.Paragraphs.Count + 1;
                        subtitle.Paragraphs.Add(p);
                    }
                    else
                    {
                        _errorCount++;
                    }
                    p = new Paragraph();
                }
                else
                {
                    expectStartTime = true;
                    p.Text = (p.Text + Environment.NewLine + s).TrimStart();
                    if (p.Text.Length > 500)
                    {
                        _errorCount += 10;
                        return;
                    }
                }
            }

            if (IsValidParagraph(p))
            {
                subtitle.Paragraphs.Add(p);
            }
            else
            {
                _errorCount++;
            }
        }

        protected static bool IsValidParagraph(Paragraph p)
        {
            // empty text (uncomment if empty text shouldn't be allowed)
            //if (string.IsNullOrWhiteSpace(HtmlUtil.RemoveHtmlTags(p.Text, true)))
            //{
            //    return false;
            //}

            // shouldn't have zero duration time
            if (p.EndTime.TotalMilliseconds <= p.StartTime.TotalMilliseconds)
            {
                return false;
            }

            double minTime = Math.Min(p.EndTime.TotalMilliseconds, p.StartTime.TotalMilliseconds);

            // one of the paragraphs contains negative time
            if (minTime < 0)
            {
                return false;
            }

            return true;
        }

    }
}
