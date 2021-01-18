using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// SubtitleNext
    /// </summary>
    public class TSB4 : SubtitleFormat
    {
        public override string Extension => ".sub";

        public override string Name => "TSB4";

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var spaceMode = true;
            subtitle.Paragraphs.Clear();
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }
            byte[] array;
            try
            {
                array = FileUtil.ReadAllBytesShared(fileName);
            }
            catch
            {
                _errorCount++;
                return;
            }
            if (array.Length < 100 || array[0] != 84 || array[1] != 83 || array[2] != 66 || array[3] != 52)
            {
                return;
            }

            int codePage = 0; // use default code page if not found

            for (int i = 0; i < array.Length - 20; i++)
            {
                if (array[i] == 67 && array[i + 1] == 80 && array[i + 2] == 65 && array[i + 3] == 71 && array[i + 4] == 4 && array[i + 5] == 0 && array[i + 6] == 0 && array[i + 7] == 0) // CPAG
                {
                    codePage = array[i + 8] + array[i + 9] * 256;
                    i += 12;
                }
                if (array[i] == 84 && array[i + 1] == 73 && array[i + 2] == 84 && array[i + 3] == 76 && array[i + 8] == 84 && array[i + 9] == 73 && array[i + 10] == 77 && array[i + 11] == 69) // TITL + TIME
                {
                    if (array[i + 5] != 32)
                    {
                        spaceMode = false;
                    }

                    int endOfText = array[i + 4];
                    if (!spaceMode)
                    {
                        endOfText += array[i + 5] * 256;
                    }

                    int start = array[i + 16] + array[i + 17] * 256;
                    if (array[i + 18] != 32 || !spaceMode)
                    {
                        start += array[i + 18] * 256 * 256;
                    }

                    int end = array[i + 20] + array[i + 21] * 256;
                    if (array[i + 22] != 32 || !spaceMode)
                    {
                        end += array[i + 22] * 256 * 256;
                    }

                    int textStart = i;
                    while (textStart <= i + endOfText + 4 && !(array[textStart] == 0x4C && array[textStart + 1] == 0x49 && array[textStart + 2] == 0x4E && array[textStart + 3] == 0x45))  // LINE
                    {
                        textStart++;
                    }
                    int length = array[textStart + 4];
                    if (array[textStart + 5] != 32 || !spaceMode)
                    {
                        length += array[textStart + 5] * 256;
                    }

                    textStart += 8;

                    string text = string.Empty;
                    if (textStart + length > array.Length)
                    {
                        if (textStart < array.Length)
                        {
                            text = Encoding.GetEncoding(codePage).GetString(array, textStart, array.Length - textStart);
                        }
                    }
                    else
                    {
                        text = Encoding.GetEncoding(codePage).GetString(array, textStart, length);
                    }
                    text = FixItalicsAndBinaryZero(text);
                    text = string.Join(Environment.NewLine, text.SplitToLines()); //conform to CRLF
                    text = text.Replace(" " + Environment.NewLine, Environment.NewLine).Trim();
                    var item = new Paragraph(text, FramesToMilliseconds(start), FramesToMilliseconds(end));
                    subtitle.Paragraphs.Add(item);
                    i += endOfText + 5;
                }
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

        private static string FixItalicsAndBinaryZero(string text)
        {
            var sb = new StringBuilder(text.Length + 5);
            var italicOn = false;
            for (int i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                switch (ch)
                {
                    case '\u007f':
                        sb.Append(italicOn ? "</i>" : "<i>");
                        italicOn = !italicOn;
                        break;
                    case '\r':
                    case '\n':
                        if (italicOn)
                        {
                            sb.Append("</i>");
                            italicOn = false;
                        }
                        sb.Append(ch);
                        break;
                    case '\0':
                        sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString().Trim();
        }

    }
}
