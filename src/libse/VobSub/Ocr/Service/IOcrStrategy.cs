using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Core.VobSub.Ocr.Service
{
    public interface IOcrStrategy
    {
        string GetName();
        string GetUrl();
        List<string> PerformOcr(string language, List<Bitmap> images);
        int GetMaxImageSize();
        int GetMaximumRequestArraySize();
    }
}
