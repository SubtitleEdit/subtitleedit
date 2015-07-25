using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class SpruceWithSpace : SubtitleFormat
    {
        private readonly static Regex regexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d, \d\d:\d\d:\d\d:\d\d,.+", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".stl"; }
        }

        public override string Name
        {
            get { return "Spruce Subtitle With Space"; }
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
            const string Header = @"$FontName           =   Arial
$FontSize               =   34
$HorzAlign          =   Left
$VertAlign          =   Bottom
$XOffset                =   0
$YOffset                =   0
$Bold                   =   FALSE
$UnderLined         =   FALSE
$Italic             =   FALSE
$TextContrast           =   15
$Outline1Contrast       =   15
$Outline2Contrast       =   15
$BackgroundContrast =   0
$ForceDisplay           =   FALSE
$FadeIn             =   0
$FadeOut                =   0
$TapeOffset         =   FALSE

\\Colour 0 = Black
\\Colour 1 = Red
\\Colour 2 = Green
\\Colour 3 = Yellow
\\Colour 4 = Blue
\\Colour 5 = Magenta
\\Colour 6 = Cyan
\\Colour 7 = White
";
            var sb = new StringBuilder();
            sb.AppendLine(Header);
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format("$HorzAlign     = Center\r\n{0}, {1}, {2}", EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), EncodeText(p.Text)));
            }
            return sb.ToString();
        }

        private static string EncodeText(string text)
        {
            text = text.Replace("<b>", "^B");
            text = text.Replace("</b>", string.Empty);
            text = text.Replace("<i>", "^I");
            text = text.Replace("</i>", string.Empty);
            text = text.Replace("<u>", "^U");
            text = text.Replace("</u>", string.Empty);
            return text.Replace(Environment.NewLine, "|");
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:01:54:19
            return time.ToHHMMSSFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:01:54:19,00:01:56:17,We should be thankful|they accepted our offer.
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (line.IndexOf(':') == 2 && regexTimeCodes.IsMatch(line))
                {
                    string start = line.Substring(0, 11);
                    string end = line.Substring(13, 11);

                    try
                    {
                        Paragraph p = new Paragraph(DecodeTimeCode(start), DecodeTimeCode(end), DecodeText(line.Substring(25).Trim()));
                        subtitle.Paragraphs.Add(p);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("//", StringComparison.Ordinal) && !line.StartsWith('$'))
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string time)
        {
            //00:01:54:19
            var hour = int.Parse(time.Substring(0, 2));
            var minutes = int.Parse(time.Substring(3, 2));
            var seconds = int.Parse(time.Substring(6, 2));
            var frames = int.Parse(time.Substring(9, 2));
            return new TimeCode(hour, minutes, seconds, FramesToMillisecondsMax999(frames));
        }

        private static string DecodeText(string text)
        {
            // TODO: Improve end tags
            text = text.Replace("|", Environment.NewLine);
            if (text.Contains("^B"))
                text = text.Replace("^B", "<b>") + "</b>";
            if (text.Contains("^I"))
                text = text.Replace("^I", "<i>") + "</i>";
            if (text.Contains("^U"))
                text = text.Replace("^U", "<u>") + "</u>";
            return text;
        }
    }
}