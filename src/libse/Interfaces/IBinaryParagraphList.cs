using SkiaSharp;

namespace Nikse.SubtitleEdit.Core.Interfaces
{
    public interface IBinaryParagraphList
    {
        SKBitmap GetSubtitleBitmap(int index, bool crop = true);
        bool GetIsForced(int index);
    }
}
