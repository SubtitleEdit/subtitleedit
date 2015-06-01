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
            var buffer = FileUtil.ReadAllBytesShared(fileName);
            int index = startPosition;
            if (buffer[index] != 1)
            {
                return;
            }

            while (index + textPosition < buffer.Length)
            {
                int textLength = buffer[index + 16];
                if (textLength > 0 && index + textPosition + textLength < buffer.Length)
                {
                    string text = GetText(index + textPosition, textLength, buffer);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        int startFrames = GetFrames(index + 4, buffer);
                        int endFrames = GetFrames(index + 8, buffer);
                        subtitle.Paragraphs.Add(new Paragraph(text, FramesToMilliseconds(startFrames), FramesToMilliseconds(endFrames)));
                    }
                }
                index += textPosition + textLength;
            }
            subtitle.Renumber();
        }

        private static string GetText(int index, int length, byte[] buffer)
        {
            if (length < 1)
            {
                return string.Empty;
            }

            int offset = 0;
            if (buffer[index] == 7)
            {
                offset = 1;
            }
            else if (buffer[index + 1] == 7)
            {
                offset = 2;
            }
            else if (buffer[index + 2] == 7)
            {
                offset = 3;
            }

            if (length - offset < 1)
            {
                return string.Empty;
            }

            const string newline1 = ""; // unicode chars
            const string newline2 = ""; // unicode char
            var s = Encoding.UTF8.GetString(buffer, index + offset, length - offset);
            s = s.Replace(newline1, Environment.NewLine);
            s = s.Replace(newline2, Environment.NewLine);
            return s;
        }

        private static int GetFrames(int index, byte[] buffer)
        {
            return (buffer[index + 2] << 16) + (buffer[index + 1] << 8) + buffer[index];
        }

    }
}