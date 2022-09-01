using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Core.VobSub.Ocr.Service
{
    public class GoogleOcrService : IOcrStrategy
    {

        private readonly IOcrStrategy _ocrStrategy;

        public GoogleOcrService(IOcrStrategy translationStrategy)
        {
            _ocrStrategy = translationStrategy;
        }

        public string GetName()
        {
            return _ocrStrategy.GetName();
        }

        public override string ToString()
        {
            return GetName();
        }

        public int GetMaxImageSize()
        {
            return _ocrStrategy.GetMaxImageSize();
        }

        public int GetMaximumRequestArraySize()
        {
            return _ocrStrategy.GetMaximumRequestArraySize();
        }

        public List<OcrLanguage> GetLanguages()
        {
            return _ocrStrategy.GetLanguages();
        }

        public string GetUrl()
        {
            return string.Empty;
        }

        public List<string> PerformOcr(string language, List<Bitmap> images)
        {
            return _ocrStrategy.PerformOcr(language, images);
        }
    }
}