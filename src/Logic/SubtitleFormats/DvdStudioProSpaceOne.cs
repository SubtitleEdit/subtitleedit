using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class DvdStudioProSpaceOne : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d+:\d+:\d+,\d+:\d+:\d+:\d+, .*$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".STL"; }
        }

        public override string Name
        {
            get { return "DVD Studio Pro with one space"; }
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
            const string paragraphWriteFormat = "{0},{1}, {2}\r\n";
            const string timeFormat = "{0:00}:{1:00}:{2:00}:{3:00}";
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
                double factor = (TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate);
                string startTime = string.Format(timeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, (int)Math.Round(p.StartTime.Milliseconds / factor));
                string endTime = string.Format(timeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, (int)Math.Round(p.EndTime.Milliseconds / factor));
                sb.AppendFormat(paragraphWriteFormat, startTime, endTime, DvdStudioPro.EncodeStyles(p.Text));
            }
            return sb.ToString().Trim();
        }

        public static byte GetFrameFromMilliseconds(int milliseconds, double frameRate)
        {
            return (byte)Math.Round(milliseconds / (TimeCode.BaseUnit / frameRate));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            int number = 0;
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && line[0] != '$' && !line.StartsWith("//"))
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
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }
        }

    }
}
