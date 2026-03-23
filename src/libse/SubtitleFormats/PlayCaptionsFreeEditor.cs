using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class PlayCaptionsFreeEditor : SubtitleFormat
    {
        public override string Extension => ".tmm";
        public override string Name => "Play Captions FreeEditor";

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not Implemented";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName) || Path.GetExtension(fileName).ToLowerInvariant() != Extension)
            {
                return;
            }

            try
            {
                ReadTimeCodes(subtitle, fileName);
                var path = Path.GetDirectoryName(fileName);
                if (path == null)
                {
                    return;
                }

                var texts = ReadTexts(Path.Combine(path, Path.GetFileNameWithoutExtension(fileName)) + ".rtf");
                FillTexts(subtitle, texts);
            }
            catch
            {
                // ignore
            }

            subtitle.Renumber();
        }

        private void ReadTimeCodes(Subtitle subtitle, string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var bytes = new byte[26];
                while (fs.Read(bytes, 0, bytes.Length) == 26)
                {
                    if (bytes[24] == 13 && bytes[25] == 10)
                    {
                        var startFrames = bytes[0] + bytes[1] * byte.MaxValue + bytes[2] * byte.MaxValue * byte.MaxValue;
                        var endFrames = bytes[4] + bytes[5] * byte.MaxValue + bytes[6] * byte.MaxValue * byte.MaxValue;
                        subtitle.Paragraphs.Add(new Paragraph(string.Empty, FramesToMilliseconds(startFrames), FramesToMilliseconds(endFrames)));
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }

            while (subtitle.Paragraphs.Count > 0 && Math.Abs(subtitle.Paragraphs.Last().EndTime.TotalMilliseconds) < 0.001)
            {
                subtitle.Paragraphs.RemoveAt(subtitle.Paragraphs.Count - 1);
            }
        }

        private static List<string> ReadTexts(string fileName)
        {
            var result = new List<string>();
            if (!File.Exists(fileName))
            {
                return result;
            }

            var rtf = FileUtil.ReadAllTextShared(fileName, Encoding.ASCII);
            return rtf.FromRtf().SplitToLines();
        }

        private void FillTexts(Subtitle subtitle, List<string> lines)
        {
            var counter = 0;
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    if (sb.Length > 0)
                    {
                        if (counter < subtitle.Paragraphs.Count)
                        {
                            subtitle.Paragraphs[counter].Text = sb.ToString().Trim();
                            counter++;
                            sb.Clear();
                        }
                        else
                        {
                            _errorCount++;
                        }
                    }
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
        }
    }
}
