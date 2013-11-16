using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
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
            Subtitle subtitle = new Subtitle();
            this.LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > this._errorCount;
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
                array = File.ReadAllBytes(fileName);
            }
            catch
            {
                _errorCount++;
                return;
            }
            if (array.Length < 100)
            {
                return;
            }
            if (array[0] != 84 || array[1] != 83 || array[2] != 66 || array[3] != 52)
            {
                return;
            }
            for (int i = 0; i < array.Length - 20; i++)
            {
                if (array[i] == 84 && array[i + 1] == 73 && array[i + 2] == 84 && array[i + 3] == 76 && array[i + 8] == 84 && array[i + 9] == 73 && array[i + 10] == 77 && array[i + 11] == 69) // TITL + TIME
                {
                    int endOfText = (int)array[i + 4];

                    bool hasTimeCode = true;
                    int start = array[i + 16] + array[i + 17] * 256;
                    if (hasTimeCode && array[i + 17] == 32)
                        start = (int)array[i + 16];
                    else
                        hasTimeCode = false;

                    int end = array[i + 20] + array[i + 21] * 256;
                    if (hasTimeCode && array[i + 21] == 32)
                        start = (int)array[i + 20];
                    else
                        hasTimeCode = false;

                    string text = Encoding.Default.GetString(array, i + 53, endOfText - 47);
                    text = text.Trim('\0').Trim();
                    var item = new Paragraph(text, (double)SubtitleFormat.FramesToMilliseconds((double)start), (double)SubtitleFormat.FramesToMilliseconds((double)end));
                    subtitle.Paragraphs.Add(item);
                    i += endOfText + 5;
                }
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber(1);
        }

    }
}
