using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Core.VobSub.Ocr.Service
{
    public class GoogleOCRService : IOCRService
    {

        private readonly IOCRStrategy _ocrStrategy;

        public GoogleOCRService(IOCRStrategy translationStrategy)
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

        public string GetUrl()
        {
            return "";
        }

        public List<string> PerformOCR(string language, List<Bitmap> images)
        {
            return _ocrStrategy.PerformOCR(language, images);
        }
    }
}