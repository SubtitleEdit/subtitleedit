using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class Spruce : SubtitleFormat
    {
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
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Header);
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format("{0},{1},{2}", EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), EncodeText(p.Text)));
            }
            return sb.ToString();
        }

        private string EncodeText(string text)
        {
            text = text.Replace("<b>", "^B");
            text = text.Replace("</b>", string.Empty);
            text = text.Replace("<i>", "^I");
            text = text.Replace("</i>", string.Empty);
            text = text.Replace("<u>", "^U");
            text = text.Replace("</u>", string.Empty);
            return text.Replace(Environment.NewLine, "|");
        }

        private string EncodeTimeCode(TimeCode time)
        {
            //00:01:54:19

            int frames = time.Milliseconds / (1000 / 25);

            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, frames);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:01:54:19,00:01:56:17,We should be thankful|they accepted our offer.
            subtitle.Paragraphs.Clear();
            var regexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d,\d\d:\d\d:\d\d:\d\d,.+", RegexOptions.Compiled);
            foreach (string line in lines)
            {
                if (regexTimeCodes.IsMatch(line))
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
                else if (line.Trim().Length > 0 && !line.StartsWith("//") && !line.StartsWith("$"))
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber(1);
        }

        private TimeCode DecodeTimeCode(string time)
        {
            //00:01:54:19

            string hour = time.Substring(0, 2);
            string minutes = time.Substring(3, 2);
            string seconds = time.Substring(6, 2);
            string frames = time.Substring(9, 2);

            int milliseconds = (int)((1000 / 25.0) * int.Parse(frames));
            if (milliseconds > 999)
                milliseconds = 999;

            TimeCode tc = new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), milliseconds);
            return tc;
        }

        private string DecodeText(string text)
        { //TODO: improve end tags
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
