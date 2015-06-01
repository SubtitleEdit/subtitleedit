using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    /// <summary>
    /// http://forum.videohelp.com/threads/365786-Converting-Subtitles-%28XML-PNG%29-to-idx-sub
    /// </summary>
    public class SatBoxPng : SubtitleFormat
    {

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "SatBox png"; }
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
            return "Not implemented";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //<I s="0.600" e="3.720" x="268" y="458" w="218" h="58" i="AYZ1.png" />
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            if (string.IsNullOrEmpty(fileName))
                return;
            string path = Path.GetDirectoryName(fileName);
            foreach (string line in lines)
            {
                string s = line;
                if (line.Contains(" s=\"") && line.Contains(" e=\"") && line.Contains(" i=\"") && line.Contains(".png") && (line.Contains("<I ") || line.Contains("&lt;I ")))
                {
                    string start = GetTagValue("s", line);
                    string end = GetTagValue("e", line);
                    string text = GetTagValue("i", line);
                    try
                    {
                        if (File.Exists(Path.Combine(path, text)))
                            text = Path.Combine(path, text);
                        p = new Paragraph(DecodeTimeCode(start), DecodeTimeCode(end), text);
                        subtitle.Paragraphs.Add(p);
                    }
                    catch (Exception exception)
                    {
                        _errorCount++;
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(line) && p != null)
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

        private static string GetTagValue(string tag, string line)
        {
            var start = line.IndexOf(tag + "=\"", StringComparison.Ordinal);
            if (start > 0 && line.Length > start + 4)
            {
                int end = line.IndexOf('"', start + 3);
                if (end > 0 && line.Length > end + 3)
                {
                    string value = line.Substring(start + 3, end - start - 3);
                    return value;
                }

            }
            return string.Empty;
        }

        private static TimeCode DecodeTimeCode(string s)
        {
            return TimeCode.FromSeconds(double.Parse(s, CultureInfo.InvariantCulture));
        }

    }
}
