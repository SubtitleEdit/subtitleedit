using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class OcrCharacter : IDisposable
    {
        public string Text { get; private set; }
        public List<OcrImage> OcrImages { get; set; }

        public OcrCharacter(string text)
        {
            Text = text;
            OcrImages = new List<OcrImage>();
        }

        public void Dispose()
        {
            if (OcrImages != null && OcrImages.Count > 0)
            {
                foreach (OcrImage ocrImage in OcrImages)
                {
                    ocrImage.Dispose();
                }
                OcrImages = null;
            }
        }
    }
}