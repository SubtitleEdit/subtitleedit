using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SonicScenaristBitmaps : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d{1,4}\s+\d\d:\d\d:\d\d:\d\d\s+\d\d:\d\d:\d\d:\d\d\s+.+\.(tif|tiff|png|bmp|TIF|TIFF|PNG|BMP)", RegexOptions.Compiled);

        public override string Extension => ".sst";

        public override string Name => "Sonic Scenarist Bitmaps";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var extension = System.IO.Path.GetExtension(fileName);
            if (extension != null && extension.ToLowerInvariant() != ".sst")
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            int index = 0;
            const string writeFormat = "{0:0000}       {1}  {2}  {3}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //0001       00:49:26:22  00:49:27:13  t01_v001c001_22_0001.bmp
                sb.AppendLine(string.Format(writeFormat, index + 1, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), HtmlUtil.RemoveHtmlTags(p.Text).Replace(Environment.NewLine, "\t")));
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
            char[] splitChar = { ' ' };
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    string temp = line.Substring(0, RegexTimeCodes.Match(line).Length).Replace("\t", " ");

                    string[] parts = temp.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length > 3)
                    {
                        sb.Clear();
                        for (int i = 3; i < parts.Length; i++)
                        {
                            sb.Append(parts[i] + " ");
                        }

                        string text = sb.ToString().Trim();
                        p = new Paragraph(DecodeTimeCodeFramesFourParts(parts[1].Split(SplitCharColon)), DecodeTimeCodeFramesFourParts(parts[2].Split(SplitCharColon)), text);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Display_Area", StringComparison.Ordinal) || line.StartsWith('#') || line.StartsWith("Color", StringComparison.Ordinal) || index < 10)
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

    }
}
