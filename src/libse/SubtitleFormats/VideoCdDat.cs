using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class VideoCdDat : SubtitleFormat
    {
        public override string Extension => ".dat";

        public override string Name => "Video CD DAT";
        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[2048];
                var bytesRead = fs.Read(buffer, 0, buffer.Length);
                while (bytesRead == 2048)
                {
                    ReadSegment(subtitle, buffer);

                    bytesRead = fs.Read(buffer, 0, buffer.Length);
                }
            }

            MergeLinesWithSameTimeCodes(subtitle);
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

        private static void MergeLinesWithSameTimeCodes(Subtitle subtitle)
        {
            for (var index = 0; index < subtitle.Paragraphs.Count-1; index++)
            {
                var p = subtitle.Paragraphs[index];
                var next = subtitle.GetParagraphOrDefault(index + 1);
                if (Math.Abs(p.StartTime.TotalMilliseconds - next.StartTime.TotalMilliseconds) < 0.01)
                {
                    p.Text = p.Text + Environment.NewLine + next.Text;
                    p.EndTime.TotalMilliseconds = next.EndTime.TotalMilliseconds;
                    next.Text = string.Empty;
                }
            }
        }

        private static void ReadSegment(Subtitle subtitle, byte[] buffer)
        {
            var iso88511 = Encoding.GetEncoding("ISO-8859-1");
            var index = 0;
            var firstTimeIn = ReadFourByteInt(buffer, ref index);
            var lastTimeOut = ReadFourByteInt(buffer, ref index);
            Paragraph lastP = null;
            var lastMultiline = false;
            while (index < buffer.Length - 18)
            {
                var zeroByte = buffer[index++];
                var multipleLineFlag = buffer[index++];
                var xPosition = ReadTwoByteInt(buffer, ref index);
                var yPosition = ReadTwoByteInt(buffer, ref index);
                var startTime = ReadFourByteInt(buffer, ref index);
                var endTime = ReadFourByteInt(buffer, ref index);

                var textLength = ReadTwoByteInt(buffer, ref index);
                if (index + textLength >= buffer.Length)
                {
                    break;
                }

                if (textLength > 200 || startTime == -1 && endTime == -1)
                {
                    break;
                }

                if (buffer[index + textLength] == 0) // remove padding
                {
                    textLength--;
                }

                var text = iso88511.GetString(buffer, index, textLength);
                index += textLength;

                var last = buffer[index++];

                if (lastMultiline)
                {
                    lastP.Text += Environment.NewLine + text;
                    lastP.EndTime.TotalMilliseconds = endTime / 22500.0 * 1000.0;
                }
                else
                {
                    var p = new Paragraph(text, startTime / 22500.0 * 1000.0, endTime / 22500.0 * 1000.0);
                    subtitle.Paragraphs.Add(p);
                    lastP = p;

                    if (yPosition <= 200)
                    {
                        p.Text = @"{\an8}" + p.Text;
                    }
                }

                lastMultiline = multipleLineFlag == 1;
            }
        }

        private static int ReadFourByteInt(byte[] buffer, ref int index)
        {
            var result = (buffer[index] << 24) +
                   (buffer[index + 1] << 16) +
                   (buffer[index + 2] << 8) +
                   buffer[index + 3];
            index += 4;
            return result;
        }

        private static int ReadTwoByteInt(byte[] buffer, ref int index)
        {
            var result = (buffer[index] << 8) + buffer[index + 1];
            index += 2;
            return result;
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }
    }
}
