using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle79 : SubtitleFormat
    {

        // SUB [0 N 00:00:00:00>00:00:00:00]
        private static readonly Regex RegexTimeCode = new Regex(@"^SUB \[\d [NIB] \d\d:\d\d:\d\d:\d\d>\d\d:\d\d:\d\d\:\d\d\]", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 79";

        public override string ToText(Subtitle subtitle, string title)
        {
            //SUB [0 N 00:00:00:00>00:00:00:00]
            //Subtitle 1
            //Subtitle 2

            //0 = Height (0=bottom, 5=middle, 9=top)
            //N or I or B = Normal or Italic or Bold

            const string paragraphWriteFormat = "SUB [{0} {1} {2}>{3}]{4}{5}";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var text = p.Text.Trim();

                string verticalAlignment = "0";
                if (text.StartsWith("{\\an7}", StringComparison.Ordinal) || text.StartsWith("{\\an8}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal))
                {
                    verticalAlignment = "9";
                }
                else if (text.StartsWith("{\\an4}", StringComparison.Ordinal) || text.StartsWith("{\\an5}", StringComparison.Ordinal) || text.StartsWith("{\\an6}", StringComparison.Ordinal))
                {
                    verticalAlignment = "5";
                }

                text = Utilities.RemoveSsaTags(text);
                string formatting = "N";
                if (text.StartsWith("<i>", StringComparison.Ordinal))
                {
                    formatting = "I";
                }
                else if (text.StartsWith("<b>", StringComparison.Ordinal))
                {
                    formatting = "B";
                }

                text = HtmlUtil.RemoveHtmlTags(text);

                sb.AppendLine(string.Format(paragraphWriteFormat, verticalAlignment, formatting, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), Environment.NewLine, text));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            _errorCount = 0;
            string formatting = "N";
            string verticalAglinment = "0";

            subtitle.Paragraphs.Clear();
            char[] splitChars = { ' ', '>' };
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (s.Length == 33 && RegexTimeCode.IsMatch(s))
                {
                    var parts = s.Split(splitChars);
                    if (parts.Length == 5)
                    {
                        AddParagraph(subtitle, paragraph, formatting, verticalAglinment);

                        try
                        {
                            verticalAglinment = parts[1].TrimStart('[');
                            formatting = parts[2];
                            paragraph = new Paragraph { StartTime = DecodeTimeCodeFrames(parts[3], SplitCharColon), EndTime = DecodeTimeCodeFrames(parts[4].TrimEnd(']'), SplitCharColon) };
                        }
                        catch (Exception)
                        {
                            _errorCount++;
                        }
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else if (paragraph != null && Utilities.GetNumberOfLines(paragraph.Text) < 15)
                {
                    paragraph.Text = (paragraph.Text + Environment.NewLine + line).Trim();
                }
                else
                {
                    _errorCount++;
                    if (_errorCount > 10)
                    {
                        return;
                    }
                }
            }
            AddParagraph(subtitle, paragraph, formatting, verticalAglinment);
            subtitle.Renumber();
        }

        private static void AddParagraph(Subtitle subtitle, Paragraph paragraph, string formatting, string verticalAglinment)
        {
            if (paragraph != null)
            {
                if (formatting == "I")
                {
                    paragraph.Text = "<i>" + paragraph.Text + "</i>";
                }
                else if (formatting == "B")
                {
                    paragraph.Text = "<b>" + paragraph.Text + "</b>";
                }

                if (verticalAglinment == "7" || verticalAglinment == "8" || verticalAglinment == "9")
                {
                    paragraph.Text = "{\\an8}" + paragraph.Text;
                }
                else if (verticalAglinment == "3" || verticalAglinment == "4" || verticalAglinment == "5" || verticalAglinment == "6")
                {
                    paragraph.Text = "{\\an5}" + paragraph.Text;
                }

                subtitle.Paragraphs.Add(paragraph);
            }
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return time.ToHHMMSSFF();
        }
    }
}
