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

        public override string Extension => ".C";

        public override string Name => "Unknown 20";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            int number = 1;
            int number2 = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string line1 = string.Empty;
                string line2 = string.Empty;
                var lines = p.Text.SplitToLines();
                if (lines.Count > 2)
                {
                    lines = Utilities.AutoBreakLine(p.Text).SplitToLines();
                }

                if (lines.Count == 1)
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
            foreach (string line in lines)
            {
                string s = line.TrimEnd();
                if (RegexTimeCode1.IsMatch(s))
                {
                    try
                    {
                        if (p != null)
                        {
                            subtitle.Paragraphs.Add(p);
                        }

                        p = new Paragraph(DecodeTimeCodeFrames(s.Substring(5, 11), SplitCharColon), new TimeCode(), s.Remove(0, 37).Trim());
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
                        {
                            subtitle.Paragraphs.Add(p);
                        }

                        p = new Paragraph(DecodeTimeCodeFrames(s.Substring(5, 11), SplitCharColon), new TimeCode(), string.Empty);
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
                            p.EndTime = DecodeTimeCodeFrames(s.Substring(5, 11), SplitCharColon);
                            if (string.IsNullOrWhiteSpace(p.Text))
                            {
                                p.Text = s.Remove(0, 37).Trim();
                            }
                            else
                            {
                                p.Text = p.Text + Environment.NewLine + s.Remove(0, 37).Trim();
                            }
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
            {
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber();
        }

    }
}
