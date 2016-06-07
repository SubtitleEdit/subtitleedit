using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TSB4 : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".sub"; }
        }

        public override string Name
        {
            get { return "TSB4"; }
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
            return "Not supported!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
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
            for (int i = 0; i < array.Length - 20; i++)
            {
                if (array[i] == 84 && array[i + 1] == 73 && array[i + 2] == 84 && array[i + 3] == 76 && array[i + 8] == 84 && array[i + 9] == 73 && array[i + 10] == 77 && array[i + 11] == 69) // TITL + TIME
                {
                    int endOfText = array[i + 4];

                    int start = array[i + 16] + array[i + 17] * 256;
                    if (array[i + 18] != 32)
                        start += array[i + 18] * 256 * 256;

                    int end = array[i + 20] + array[i + 21] * 256;
                    if (array[i + 22] != 32)
                        end += array[i + 22] * 256 * 256;

                    int textStart = i;
                    while (textStart < i + endOfText && !(array[textStart] == 0x4C && array[textStart + 1] == 0x49 && array[textStart + 2] == 0x4E && array[textStart + 3] == 0x45)) // LINE
                    {
                        textStart++;
                    }
                    int length = i + endOfText - textStart - 2;
                    textStart += 8;

                    string text = Encoding.Default.GetString(array, textStart, length);
                    // text = Encoding.Default.GetString(array, i + 53, endOfText - 47);
                    text = text.Trim('\0').Replace("\0", " ").Trim();
                    var item = new Paragraph(text, FramesToMilliseconds(start), FramesToMilliseconds(end));
                    subtitle.Paragraphs.Add(item);
                    i += endOfText + 5;
                }
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}
