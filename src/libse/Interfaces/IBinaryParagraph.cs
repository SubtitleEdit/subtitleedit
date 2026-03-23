
using SkiaSharp;

namespace Nikse.SubtitleEdit.Core.Interfaces
{
    public interface IBinaryParagraph
    {
        bool IsForced { get; }
        SKBitmap GetBitmap();
    }
}
