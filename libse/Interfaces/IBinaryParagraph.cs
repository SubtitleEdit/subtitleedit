using System.Drawing;

namespace Nikse.SubtitleEdit.Core.Interfaces
{
    public interface IBinaryParagraph
    {
        bool IsForced { get; }
        Bitmap GetBitmap();
    }
}
