using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Core.Interfaces
{
    public interface IBinaryParagraphWithPosition : IBinaryParagraph
    {
        SKSize GetScreenSize();
        Position GetPosition();
        TimeCode StartTimeCode { get; }
        TimeCode EndTimeCode { get; }
    }
}
