using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Core.VobSub.Ocr.Service
{
    public interface IOCRStrategy
    {
        string GetName();
        string GetUrl();
        List<string> PerformOCR(string language, List<Bitmap> images);
        int GetMaxImageSize();
        int GetMaximumRequestArraySize();
    }
}
