using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class DvdStudioProSpaceOneSemicolon : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d+:\d+;\d+,\d+:\d+:\d+;\d+, .*$", RegexOptions.Compiled); // ";" is drop frame

        public override string Extension => ".STL";

        public override string Name => "DVD Studio Pro with one space/semicolon";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0},{1}, {2}\r\n";
            const string timeFormat = "{0:00}:{1:00}:{2:00};{3:00}";
            const string header = @"$VertAlign          =   Bottom
$Bold               =   FALSE
$Underlined         =   FALSE
$Italic             =   0
$XOffset                =   0
$YOffset                =   -5
$TextContrast           =   15
$Outline1Contrast           =   15
$Outline2Contrast           =   13
$BackgroundContrast     =   0
$ForceDisplay           =   FALSE
$FadeIn             =   0
$FadeOut                =   0
$HorzAlign          =   Center
";

            var sb = new StringBuilder();
            sb.AppendLine(header);
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string startTime = string.Format(timeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds));
                string endTime = string.Format(timeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds));
                sb.AppendFormat(paragraphWriteFormat, startTime, endTime, DvdStudioPro.EncodeStyles(p.Text));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            int number = 0;
            bool italicOn = false;
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                }
                else if (line[0] != '$' && !line.StartsWith("//", StringComparison.Ordinal))
                {
                    if (RegexTimeCodes.Match(line).Success)
                    {
                        string[] toPart = line.Substring(0, 24).Trim(',').Split(',');
                        var p = new Paragraph();
                        if (toPart.Length == 2 &&
                            DvdStudioPro.GetTimeCode(p.StartTime, toPart[0]) &&
                            DvdStudioPro.GetTimeCode(p.EndTime, toPart[1]))
                        {
                            number++;
                            p.Number = number;
                            string text = line.Substring(25).Trim();
                            p.Text = text.Replace(" | ", Environment.NewLine).Replace("|", Environment.NewLine);
                            p.Text = DvdStudioPro.DecodeStyles(p.Text);
                            if (italicOn && !p.Text.Contains("<i>"))
                            {
                                p.Text = "<i>" + p.Text + "</i>";
                            }
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else if (line.StartsWith("// Format: NTSC - 29.97", StringComparison.OrdinalIgnoreCase))
                {
                    Configuration.Settings.General.CurrentFrameRate = 29.97;
                }
                else if (line.StartsWith("// Format: NTSC - 23.9", StringComparison.OrdinalIgnoreCase))
                {
                    Configuration.Settings.General.CurrentFrameRate = 23.976;
                }
                else if (line.StartsWith("$Italic = True", StringComparison.OrdinalIgnoreCase))
                {
                    italicOn = true;
                }
                else if (line.StartsWith("$Italic = False", StringComparison.OrdinalIgnoreCase))
                {
                    italicOn = false;
                }
            }
        }

    }
}
