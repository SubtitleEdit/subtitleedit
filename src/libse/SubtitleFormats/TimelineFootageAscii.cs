using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Timeline Ascii export - THE MOVIE TITRE EDITOR - http://www.pld.ttu.ee/~priidu/timeline/ by priidu@pld.ttu.ee
    ///
    /// Sample:
    /// 1.
    ///    41,10
    ///    46,10
    /// ±NE/SEVÎ
    /// ³ÂÍÅ/ÑÅÁß
    ///
    /// 2.
    ///    49,05
    ///    51,09
    /// ±Viòð ir klât.
    /// ³Îí ïðèøåë.
    /// </summary>
    public class TimeLineFootageAscii : SubtitleFormat
    {

        private static readonly Regex RegexTimeCode = new Regex(@"^\s*\d+,\d\d$", RegexOptions.Compiled);

        private enum ExpectingLine
        {
            Number,
            TimeStart,
            TimeEnd,
            Text
        }

        public override string Extension => ".asc";

        public override string Name => "Timeline footage ascii";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName == null || !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return string.Empty;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            var expecting = ExpectingLine.Number;
            _errorCount = 0;
            byte firstLineCode = 0;
            byte secondLineCode = 0;

            subtitle.Paragraphs.Clear();
            IEnumerable<byte[]> byteLines = TimeLineAscii.SplitBytesToLines(File.ReadAllBytes(fileName));
            foreach (byte[] bytes in byteLines)
            {
                var line = Encoding.ASCII.GetString(bytes);
                if (line.EndsWith('.') && Utilities.IsInteger(line.TrimEnd('.')))
                {
                    if (paragraph != null && !string.IsNullOrEmpty(paragraph.Text))
                    {
                        subtitle.Paragraphs.Add(paragraph);
                    }

                    paragraph = new Paragraph();
                    expecting = ExpectingLine.TimeStart;
                }
                else if (paragraph != null && expecting == ExpectingLine.TimeStart && RegexTimeCode.IsMatch(line))
                {
                    string[] parts = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        try
                        {
                            var tc = DecodeTimeCode(parts);
                            paragraph.StartTime = tc;
                            expecting = ExpectingLine.TimeEnd;
                        }
                        catch
                        {
                            _errorCount++;
                            expecting = ExpectingLine.Number;
                        }
                    }
                }
                else if (paragraph != null && expecting == ExpectingLine.TimeEnd && RegexTimeCode.IsMatch(line))
                {
                    string[] parts = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        try
                        {
                            var tc = DecodeTimeCode(parts);
                            paragraph.EndTime = tc;
                            expecting = ExpectingLine.Text;
                        }
                        catch
                        {
                            _errorCount++;
                            expecting = ExpectingLine.Number;
                        }
                    }
                }
                else
                {
                    if (paragraph != null && expecting == ExpectingLine.Text)
                    {
                        if (bytes.Length > 1)
                        {
                            // get text from encoding
                            var enc = GetEncodingFromLanguage(bytes[0]);
                            string s = enc.GetString(bytes, 1, bytes.Length - 1).Trim();

                            // italic text
                            if (s.StartsWith('#'))
                            {
                                s = "<i>" + s.Remove(0, 1) + "</i>";
                            }

                            paragraph.Text = (paragraph.Text + Environment.NewLine + s).Trim();
                            if (paragraph.Text.Length > 2000)
                            {
                                _errorCount += 100;
                                return;
                            }

                            if (paragraph.Text.Contains(Environment.NewLine))
                            {
                                if (secondLineCode == 0)
                                {
                                    secondLineCode = bytes[0];
                                }

                                if (secondLineCode != bytes[0])
                                {
                                    _errorCount++;
                                }
                            }
                            else
                            {
                                if (firstLineCode == 0)
                                {
                                    firstLineCode = bytes[0];
                                }

                                if (firstLineCode != bytes[0])
                                {
                                    _errorCount++;
                                }
                            }
                        }
                    }
                }
            }
            if (paragraph != null && !string.IsNullOrEmpty(paragraph.Text))
            {
                subtitle.Paragraphs.Add(paragraph);
            }

            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            int frames16 = int.Parse(parts[0]);
            int frames = int.Parse(parts[1]);
            return new TimeCode(FramesToMilliseconds(16 * frames16 + (frames * 16.0 / 24.0)));
        }

        private Encoding GetEncodingFromLanguage(byte language)
        {
            if (language == 179) // Russian
            {
                return Encoding.GetEncoding(1251);
            }

            if (language == 177) // Baltic
            {
                return Encoding.GetEncoding(1257);
            }

            return Encoding.GetEncoding(1252);
        }

    }
}
