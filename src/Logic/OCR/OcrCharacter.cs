using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class OcrCharacter
    {
        public string Text { get; }
        public List<OcrImage> OcrImages { get; set; }

        public OcrCharacter(string text)
        {
            Text = text;
            OcrImages = new List<OcrImage>();
        }
    }
}
