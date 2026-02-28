using SkiaSharp;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.VobSub.Ocr.Service
{
    public interface IOcrStrategy
    {
        string GetName();
        string GetUrl();
        List<string> PerformOcr(string language, List<SKBitmap> images);
        int GetMaxImageSize();
        int GetMaximumRequestArraySize();
        List<OcrLanguage> GetLanguages();
    }
}
