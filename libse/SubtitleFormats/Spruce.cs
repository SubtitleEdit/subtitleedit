using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Spruce : SubtitleFormat
    {
        private const string Italic = "^I";
        private const string Bold = "^B";
        private const string Underline = "^U";

        private static readonly Regex RegexTimeCodes1 = new Regex(@"^\d\d:\d\d:\d\d:\d\d,\d\d:\d\d:\d\d:\d\d,.+", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodes2 = new Regex(@"^\d\d:\d\d:\d\d:\d\d,\d\d:\d\d:\d\d:\d\d,", RegexOptions.Compiled); // Matches time-only.

        public override string Extension
        {
            get { return ".stl"; }
        }

        public override string Name
        {
            get { return "Spruce Subtitle File"; }
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
            const string Header = @"//Font select and font size
$FontName       = Arial
$FontSize       = 30

//Character attributes (global)
$Bold           = FALSE
$UnderLined     = FALSE
$Italic         = FALSE

//Position Control
$HorzAlign      = Center
$VertAlign      = Bottom
$XOffset        = 0
$YOffset        = 0

//Contrast Control
$TextContrast           = 15
$Outline1Contrast       = 8
$Outline2Contrast       = 15
$BackgroundContrast     = 0

//Effects Control
$ForceDisplay   = FALSE
$FadeIn         = 0
$FadeOut        = 0

//Other Controls
$TapeOffset          = FALSE
//$SetFilePathToken  = <<:>>

//Colors
$ColorIndex1    = 0
$ColorIndex2    = 1
$ColorIndex3    = 2
$ColorIndex4    = 3

//Subtitles";
            var sb = new StringBuilder();
            sb.AppendLine(Header);
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format("{0},{1},{2}", EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), EncodeText(p.Text)));
            }
            return sb.ToString();
        }

        private static string EncodeText(string text)
        {
            text = HtmlUtil.FixUpperTags(text);
            bool allItalic = text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(text, "<i>") == 1;
            text = text.Replace("<b>", Bold);
            text = text.Replace("</b>", Bold);
            text = text.Replace("<i>", Italic);
            text = text.Replace("</i>", Italic);
            text = text.Replace("<u>", Underline);
            text = text.Replace("</u>", Underline);
            if (allItalic)
                return text.Replace(Environment.NewLine, "|^I");
            return text.Replace(Environment.NewLine, "|");
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:01:54:19
            int frames = (int)(time.Milliseconds / (TimeCode.BaseUnit / 25));
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, frames);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:01:54:19,00:01:56:17,We should be thankful|they accepted our offer.
            _errorCount = 0;
            subtitle.Paragraphs.Clear();

            // Copy reference of static compiled regex (RegexTimeCodes1).
            Regex timeCodeRegex = RegexTimeCodes1;
            if (fileName != null && fileName.EndsWith(".stl", StringComparison.OrdinalIgnoreCase)) // allow empty text if extension is ".stl"...
                timeCodeRegex = RegexTimeCodes2;

            foreach (string line in lines)
            {
                if (line.IndexOf(':') == 2 && timeCodeRegex.IsMatch(line))
                {
                    string start = line.Substring(0, 11);
                    string end = line.Substring(12, 11);

                    try
                    {
                        Paragraph p = new Paragraph(DecodeTimeCode(start), DecodeTimeCode(end), DecodeText(line.Substring(24)));
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
            string hour = time.Substring(0, 2);
            string minutes = time.Substring(3, 2);
            string seconds = time.Substring(6, 2);
            string frames = time.Substring(9, 2);

            int milliseconds = (int)((TimeCode.BaseUnit / 25.0) * int.Parse(frames));
            if (milliseconds > 999)
                milliseconds = 999;
            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), milliseconds);
        }

        private static string DecodeText(string text)
        {
            text = text.Replace("|", Environment.NewLine);

            //^IBrillstein^I
            if (text.Contains(Bold))
            {
                text = DecoderTextExtension(text, Bold, "<b>");
            }
            if (text.Contains(Italic))
            {
                text = DecoderTextExtension(text, Italic, "<i>");
            }
            if (text.Contains(Underline))
            {
                text = DecoderTextExtension(text, Underline, "<u>");
            }

            return text;
        }

        private static string DecoderTextExtension(string text, string SpruceTag, string htmlOpenTag)
        {
            var htmlCloseTag = htmlOpenTag.Insert(1, "/");

            var idx = text.IndexOf(SpruceTag, StringComparison.Ordinal);
            var c = Utilities.CountTagInText(text, SpruceTag);
            if (c == 1)
            {
                var l = idx + SpruceTag.Length;
                if (l < text.Length)
                {
                    text = text.Replace(SpruceTag, htmlOpenTag) + htmlCloseTag;
                }
                else if (l == text.Length) // Brillstein^I
                {
                    text = text.Remove(text.Length - Italic.Length);
                }
            }
            else if (c > 1)
            {
                var isOpen = true;
                while (idx >= 0)
                {
                    var htmlTag = isOpen ? htmlOpenTag : htmlCloseTag;
                    text = text.Remove(idx, SpruceTag.Length).Insert(idx, htmlTag);
                    isOpen = !isOpen;
                    idx = text.IndexOf(SpruceTag, idx + htmlTag.Length);
                }
            }
            return text;
        }
    }
}
