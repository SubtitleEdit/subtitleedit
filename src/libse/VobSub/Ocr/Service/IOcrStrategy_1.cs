using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Core.VobSub.Ocr.Service
{
    public interface IOcrStrategy_1
    {
        string GetName();
        string GetUrl();
        List<string> PerformOcr(string language, List<Bitmap> images);
        int GetMaxImageSize();
        int GetMaximumRequestArraySize();
    }
}
