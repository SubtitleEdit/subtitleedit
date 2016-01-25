using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle20 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode1 = new Regex(@"^     \d\d:\d\d:\d\d:\d\d     \d\d\d\d            ", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCode1Empty = new Regex(@"^     \d\d:\d\d:\d\d:\d\d     \d\d\d\d$", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCode2 = new Regex(@"^     \d\d:\d\d:\d\d:\d\d     \d\d\d\-\d\d          ", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".C"; }
        }

        public override string Name
        {
            get { return "Unknown 20"; }
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
            var sb = new StringBuilder();
            int number = 1;
            int number2 = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string line1 = string.Empty;
                string line2 = string.Empty;
                string[] lines = p.Text.Replace(Environment.NewLine, "\r").Split('\r');
                if (lines.Length > 2)
                    lines = Utilities.AutoBreakLine(p.Text).Replace(Environment.NewLine, "\r").Split('\r');
                if (lines.Length == 1)
                {
                    line2 = lines[0];
                }
                else
                {
                    line1 = lines[0];
                    line2 = lines[1];
                }

                // line 1
                sb.Append(string.Empty.PadLeft(5, ' '));
                sb.Append(p.StartTime.ToHHMMSSFF());
                sb.Append(string.Empty.PadLeft(5, ' '));
                sb.Append(number.ToString("D4"));
                sb.Append(string.Empty.PadLeft(12, ' '));
                sb.AppendLine(line1);

                //line 2
                sb.Append(string.Empty.PadLeft(5, ' '));
                sb.Append(p.EndTime.ToHHMMSSFF());
                sb.Append(string.Empty.PadLeft(5, ' '));
                sb.Append((number2 / 7 + 1).ToString("D3"));
                sb.Append('-');
                sb.Append((number2 % 7 + 1).ToString("D2"));
                sb.Append(string.Empty.PadLeft(10, ' '));
                sb.AppendLine(line2);
                sb.AppendLine();

                number++;
                number2++;
            }
            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            char[] splitChar = { ':' };
            foreach (string line in lines)
            {
                string s = line.TrimEnd();
                if (RegexTimeCode1.IsMatch(s))
                {
                    try
                    {
                        if (p != null)
                            subtitle.Paragraphs.Add(p);
                        p = new Paragraph(DecodeTimeCode(s.Substring(5, 11), splitChar), new TimeCode(0, 0, 0, 0), s.Remove(0, 37).Trim());
                    }
                    catch
                    {
                        _errorCount++;
                        p = null;
                    }
                }
                else if (RegexTimeCode1Empty.IsMatch(s))
                {
                    try
                    {
                        if (p != null)
                            subtitle.Paragraphs.Add(p);
                        p = new Paragraph(DecodeTimeCode(s.Substring(5, 11), splitChar), new TimeCode(0, 0, 0, 0), string.Empty);
                    }
                    catch
                    {
                        _errorCount++;
                        p = null;
                    }
                }
                else if (RegexTimeCode2.IsMatch(s))
                {
                    try
                    {
                        if (p != null)
                        {
                            p.EndTime = DecodeTimeCode(s.Substring(5, 11), splitChar);
                            if (string.IsNullOrWhiteSpace(p.Text))
                                p.Text = s.Remove(0, 37).Trim();
                            else
                                p.Text = p.Text + Environment.NewLine + s.Remove(0, 37).Trim();
                        }
                    }
                    catch
                    {
                        _errorCount++;
                        p = null;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(s))
                {
                    _errorCount++;
                }
            }
            if (p != null)
                subtitle.Paragraphs.Add(p);
            subtitle.Renumber();
        }

    }
}
