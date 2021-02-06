using Nikse.SubtitleEdit.Core.Common;
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

        public override string Extension => ".stl";

        public override string Name => "Spruce Subtitle File";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string header = @"//Font select and font size
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

            var lastVerticalAlign = "$VertAlign = Bottom";
            var lastHorizontalcalAlign = "$HorzAlign = Center";
            var sb = new StringBuilder();
            sb.AppendLine(header);
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                DvdStudioPro.ToTextAlignment(p, sb, ref lastVerticalAlign, ref lastHorizontalcalAlign);
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)},{EncodeTimeCode(p.EndTime)},{EncodeText(p.Text)}");
            }
            return sb.ToString();
        }

        private static string EncodeText(string input)
        {
            var text = HtmlUtil.FixUpperTags(input);
            bool allItalic = text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(text, "<i>") == 1;
            text = text.Replace("<b>", Bold);
            text = text.Replace("</b>", Bold);
            text = text.Replace("<i>", Italic);
            text = text.Replace("</i>", Italic);
            text = text.Replace("<u>", Underline);
            text = text.Replace("</u>", Underline);
            text = HtmlUtil.RemoveHtmlTags(text, true);
            if (allItalic)
            {
                return text.Replace(Environment.NewLine, "^I|^I");
            }

            return text.Replace(Environment.NewLine, "|");
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:01:54:19
            int frames = MillisecondsToFramesMaxFrameRate(time.Milliseconds);
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{frames:00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:01:54:19,00:01:56:17,We should be thankful|they accepted our offer.
            _errorCount = 0;
            subtitle.Paragraphs.Clear();

            // Copy reference of static compiled regex (RegexTimeCodes1).
            Regex timeCodeRegex = RegexTimeCodes1;
            if (fileName != null && fileName.EndsWith(".stl", StringComparison.OrdinalIgnoreCase)) // allow empty text if extension is ".stl"...
            {
                timeCodeRegex = RegexTimeCodes2;
            }

            var verticalAlign = "$VertAlign=Bottom";
            var horizontalAlign = "$HorzAlign=Center";
            foreach (string line in lines)
            {
                if (line.IndexOf(':') == 2 && timeCodeRegex.IsMatch(line))
                {
                    string start = line.Substring(0, 11);
                    string end = line.Substring(12, 11);

                    try
                    {
                        var text = DecodeText(line.Substring(24));
                        text = DvdStudioPro.GetAlignment(verticalAlign, horizontalAlign) + text;
                        Paragraph p = new Paragraph(DecodeTimeCode(start), DecodeTimeCode(end), text);
                        subtitle.Paragraphs.Add(p);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (line.TrimStart().StartsWith("$VertAlign", StringComparison.OrdinalIgnoreCase))
                {
                    verticalAlign = line.RemoveChar(' ', '\t');
                }
                else if (line.TrimStart().StartsWith("$HorzAlign", StringComparison.OrdinalIgnoreCase))
                {
                    horizontalAlign = line.RemoveChar(' ', '\t');
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

            int milliseconds = FramesToMillisecondsMax999(int.Parse(frames));
            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), milliseconds);
        }

        private static string DecodeText(string input)
        {
            var text = input.Replace("|", Environment.NewLine);

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

        private static string DecoderTextExtension(string input, string spruceTag, string htmlOpenTag)
        {
            var htmlCloseTag = htmlOpenTag.Insert(1, "/");
            var text = input;
            var idx = text.IndexOf(spruceTag, StringComparison.Ordinal);
            var c = Utilities.CountTagInText(text, spruceTag);
            if (c == 1)
            {
                var l = idx + spruceTag.Length;
                if (l < text.Length)
                {
                    text = text.Replace(spruceTag, htmlOpenTag) + htmlCloseTag;
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
                    text = text.Remove(idx, spruceTag.Length).Insert(idx, htmlTag);
                    isOpen = !isOpen;
                    idx = text.IndexOf(spruceTag, idx + htmlTag.Length, StringComparison.Ordinal);
                }
            }
            return text;
        }
    }
}
