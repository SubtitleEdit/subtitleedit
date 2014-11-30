using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class Ayato : SubtitleFormat
    {
        public override string Extension
        {
            get { return "aya"; }
        }

        public override string Name
        {
            get { return "Ayato"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length >= 3000 && fi.Length < 1024000) // not too small or too big
                {
                    if (!fileName.EndsWith(".aya", StringComparison.OrdinalIgnoreCase))
                        return false;

                    var sub = new Subtitle();
                    LoadSubtitle(sub, lines, fileName);
                    return sub.Paragraphs.Count > 0;
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            const int startPosition = 0xa99;
            const int textPosition = 72;

            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            var bytes = FileUtil.ReadAllBytesShared(fileName);
            int index = startPosition;
            if (bytes[index] != 1)
            {
                return;
            }

            while (index + textPosition < bytes.Length)
            {
                int textLength = bytes[index + 16];
                if (textLength == 0)
                {
                    return;
                }
                if (index + textPosition + textLength < bytes.Length)
                {
                    string text = GetText(index + textPosition, textLength, bytes);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        int startFrames = GetFrames(index + 4, bytes);
                        int endFrames = GetFrames(index + 8, bytes);
                        subtitle.Paragraphs.Add(new Paragraph(text, FramesToMilliseconds(startFrames), FramesToMilliseconds(endFrames)));
                    }
                }
                index += textPosition + textLength;
            }
            subtitle.Renumber(1);
        }

        private string GetText(int index, int length, byte[] bytes)
        {
            const string newline1 = ""; // unicode chars
            const string newline2 = ""; // unicode chars
            var s = Encoding.UTF8.GetString(bytes, index + 2, length - 2);
            s = s.Replace(newline1, Environment.NewLine);
            s = s.Replace(newline2, Environment.NewLine);
            return s;
        }

        private int GetFrames(int index, byte[] bytes)
        {
            return (bytes[index + 2] << 16) + (bytes[index + 1] << 8) + bytes[index + 0];
        }

    }
}