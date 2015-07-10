using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class SonicScenaristBitmaps : SubtitleFormat
    {
        private static readonly Regex regexTimeCodes = new Regex(@"^\d\d\d\d\s+\d\d:\d\d:\d\d:\d\d\s+\d\d:\d\d:\d\d:\d\d\s+.+\.(tif|tiff|png|bmp|TIF|TIFF|PNG|BMP)", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".sst"; }
        }

        public override string Name
        {
            get { return "Sonic Scenarist Bitmaps"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (System.IO.Path.GetExtension(fileName).ToLowerInvariant() != ".sst")
                return false;

            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //0001       00:49:26:22  00:49:27:13  t01_v001c001_22_0001.bmp
                sb.AppendLine(string.Format("{0:0000}       {1}  {2}  {3}", index + 1, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), HtmlUtil.RemoveHtmlTags(p.Text).Replace(Environment.NewLine, "\t")));
                index++;
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return time.ToHHMMSSFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:03:15:22 00:03:23:10 This is line one.
            //This is line two.
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            int index = 0;
            foreach (string line in lines)
            {
                if (regexTimeCodes.IsMatch(line))
                {
                    string temp = line.Substring(0, regexTimeCodes.Match(line).Length);

                    string[] parts = temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length > 3)
                    {
                        var sb = new StringBuilder();
                        for (int i = 3; i < parts.Length; i++)
                            sb.Append(parts[i] + " ");
                        string text = sb.ToString().Trim();
                        p = new Paragraph(DecodeTimeCode(parts[1].Split(':')), DecodeTimeCode(parts[2].Split(':')), text);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Display_Area") || line.StartsWith('#') || line.StartsWith("Color") || index < 10)
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    _errorCount++;
                }
                index++;
            }
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            int milliseconds = FramesToMillisecondsMax999(double.Parse(frames));
            if (milliseconds > 999)
                milliseconds = 999;

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), milliseconds);
        }

    }
}