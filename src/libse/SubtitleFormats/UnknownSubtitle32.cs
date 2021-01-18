using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle32 : SubtitleFormat
    {
        //1:  02:25:07.24  02:25:10.19
        private static readonly Regex RegexTimeCode = new Regex(@"^\d+:\s+\d\d:\d\d:\d\d\.\d\d  \d\d:\d\d:\d\d\.\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 32";

        public override string ToText(Subtitle subtitle, string title)
        {
            string paragraphWriteFormat = string.Empty.PadLeft(5, ' ') + "{4}:  {0}  {1}{3}{2}{3}";
            var sb = new StringBuilder();
            sb.AppendLine(@"STTRIO
Theatrical
PAL
SDI Media
ENGLISH
Sony,Sony DVD/UMD,1:85,16x9
0000.00
0000.00
0000.00
0000.00
0000.00
0000.00
0000.00
#: Italics Text
@/: Force title
@+: reposition top
@|: reposition middle
");
            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                count++;

                string text = p.Text;
                text = text.Replace("<i>", "#");
                text = text.Replace("</i>", "#");
                text = HtmlUtil.RemoveHtmlTags(text);
                if (text.StartsWith("{\\an8}"))
                {
                    text = text.Remove(0, 6) + "@+";
                }

                if (text.StartsWith("{\\an5}"))
                {
                    text = text.Remove(0, 6) + "@|";
                }

                if (text.StartsWith("{\\an") && text.Length > 6 && text[5] == '}')
                {
                    text = text.Remove(0, 6);
                }

                text = text.Replace(Environment.NewLine, Environment.NewLine.PadRight(Environment.NewLine.Length + 8, ' '));
                text = text.PadLeft(text.Length + 8, ' ');

                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), text, Environment.NewLine, count));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            char[] splitChars = { ':', ';', ',', '.' };
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (RegexTimeCode.IsMatch(s.Replace("*", string.Empty)))
                {
                    s = s.Replace("*", string.Empty);
                    if (paragraph != null && !string.IsNullOrEmpty(paragraph.Text))
                    {
                        subtitle.Paragraphs.Add(paragraph);
                    }

                    string[] parts = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    paragraph = new Paragraph();
                    try
                    {
                        paragraph.StartTime = DecodeTimeCodeFrames(parts[1], splitChars);
                        paragraph.EndTime = DecodeTimeCodeFrames(parts[2], splitChars);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (paragraph != null && s.Length > 0)
                {
                    bool italicOn = false;
                    while (s.Contains('#'))
                    {
                        int index = s.IndexOf('#');
                        if (italicOn)
                        {
                            s = s.Remove(index, 1).Insert(index, "</i>");
                        }
                        else
                        {
                            s = s.Remove(index, 1).Insert(index, "<i>");
                        }

                        italicOn = !italicOn;
                    }

                    // force title
                    s = s.Replace("@/", string.Empty);

                    paragraph.Text = (paragraph.Text + Environment.NewLine + s).Trim();
                    if (paragraph.Text.Length > 2000)
                    {
                        _errorCount += 100;
                        return;
                    }
                }
            }
            if (paragraph != null && !string.IsNullOrEmpty(paragraph.Text))
            {
                subtitle.Paragraphs.Add(paragraph);
            }

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //@+: reposition top
                //@|: reposition middle
                if (p.Text.Contains("@+"))
                {
                    p.Text = "{\\an8}" + p.Text.Replace("@+", string.Empty).Replace("@|", string.Empty);
                }
                else if (p.Text.Contains("@|"))
                {
                    p.Text = "{\\an5}" + p.Text.Replace("@+", string.Empty).Replace("@|", string.Empty);
                }
            }

            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15.22 (last is frame)
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}.{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

    }
}
