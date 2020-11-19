using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class OcrAlphabet
    {
        public OcrAlphabet()
        {
            OcrCharacters = new List<OcrCharacter>();
        }

        public List<OcrCharacter> OcrCharacters { get; }

        public int CalculateMaximumSize()
        {
            int max = 0;
            foreach (OcrCharacter c in OcrCharacters)
            {
                foreach (OcrImage img in c.OcrImages)
                {
                    int size = img.Bmp.Width * img.Bmp.Height;
                    if (size > max)
                    {
                        max = size;
                    }
                }
            }
            return max;
        }

        public OcrCharacter GetOcrCharacter(string text, bool addIfNotExists)
        {
            foreach (var ocrCharacter in OcrCharacters)
            {
                if (ocrCharacter.Text == text)
                {
                    return ocrCharacter;
                }
            }

            if (addIfNotExists)
            {
                var ch = new OcrCharacter(text);
                OcrCharacters.Add(ch);
                return ch;
            }
            return null;
        }

    }
}
