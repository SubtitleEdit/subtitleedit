﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class DvdStudioProSpace : TextFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d+:\d+[:;]\d+ , \d+:\d+:\d+[:;]\d+ , .*$", RegexOptions.Compiled);

        public override string Extension => ".STL";

        public override string Name => "DVD Studio Pro with space";


        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0} , {1} , {2}\r\n";
            const string timeFormat = "{0:00}:{1:00}:{2:00}:{3:00}";
            var header = Configuration.Settings.SubtitleSettings.DvdStudioProHeader.TrimEnd() + Environment.NewLine;

            var lastVerticalAlign = "$VertAlign = Bottom";
            var lastHorizontalcalAlign = "$HorzAlign = Center";
            var sb = new StringBuilder();
            sb.AppendLine(header);
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string startTime = string.Format(timeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds));
                string endTime = string.Format(timeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds));
                DvdStudioPro.ToTextAlignment(p, sb, ref lastVerticalAlign, ref lastHorizontalcalAlign);
                sb.AppendFormat(paragraphWriteFormat, startTime, endTime, DvdStudioPro.EncodeStyles(p.Text));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            int number = 0;
            var verticalAlign = "$VertAlign=Bottom";
            var horizontalAlign = "$HorzAlign=Center";
            bool italicOn = false;
            bool boldOn = false;
            bool underlineOn = false;
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && line[0] != '$' && !line.StartsWith("//", StringComparison.Ordinal))
                {
                    if (RegexTimeCodes.Match(line).Success)
                    {
                        string[] toPart = line.Substring(0, 25).Split(new[] { " ," }, StringSplitOptions.None);
                        Paragraph p = new Paragraph();
                        if (toPart.Length == 2 &&
                            DvdStudioPro.GetTimeCode(p.StartTime, toPart[0]) &&
                            DvdStudioPro.GetTimeCode(p.EndTime, toPart[1]))
                        {
                            number++;
                            p.Number = number;
                            string text = line.Substring(27).Trim();
                            p.Text = text.Replace(" | ", Environment.NewLine).Replace("|", Environment.NewLine);
                            p.Text = DvdStudioPro.DecodeStyles(p.Text);
                            if (p.Text.Trim().StartsWith("<<Graphic>>"))
                            {
                                _errorCount++;
                            }

                            if (italicOn && !p.Text.Contains("<i>"))
                            {
                                p.Text = "<i>" + p.Text + "</i>";
                            }
                            if (boldOn && !p.Text.Contains("<b>"))
                            {
                                p.Text = "<b>" + p.Text + "</b>";
                            }
                            if (underlineOn && !p.Text.Contains("<u>"))
                            {
                                p.Text = "<u>" + p.Text + "</u>";
                            }
                            p.Text = DvdStudioPro.GetAlignment(verticalAlign, horizontalAlign) + p.Text;
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else if (line != null && line.TrimStart().StartsWith("$VertAlign", StringComparison.OrdinalIgnoreCase))
                {
                    verticalAlign = line.RemoveChar(' ').RemoveChar('\t');
                }
                else if (line != null && line.TrimStart().StartsWith("$HorzAlign", StringComparison.OrdinalIgnoreCase))
                {
                    horizontalAlign = line.RemoveChar(' ').RemoveChar('\t');
                }
                else if (line.Replace(" ", string.Empty).Equals("$Italic=True", StringComparison.OrdinalIgnoreCase))
                {
                    italicOn = true;
                }
                else if (line.Replace(" ", string.Empty).Trim().Equals("$Italic=False", StringComparison.OrdinalIgnoreCase))
                {
                    italicOn = false;
                }
                else if (line.Replace(" ", string.Empty).Equals("$Bold=True", StringComparison.OrdinalIgnoreCase))
                {
                    boldOn = true;
                }
                else if (line.Replace(" ", string.Empty).Trim().Equals("$Bold=False", StringComparison.OrdinalIgnoreCase))
                {
                    boldOn = false;
                }
                else if (line.Replace(" ", string.Empty).Equals("$Underlined=True", StringComparison.OrdinalIgnoreCase))
                {
                    underlineOn = true;
                }
                else if (line.Replace(" ", string.Empty).Trim().Equals("$Underlined=False", StringComparison.OrdinalIgnoreCase))
                {
                    underlineOn = false;
                }
            }
        }

    }
}
