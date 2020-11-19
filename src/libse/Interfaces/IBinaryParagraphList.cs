using System.Drawing;

namespace Nikse.SubtitleEdit.Core.Interfaces
{
    public interface IBinaryParagraphList
    {
        Bitmap GetSubtitleBitmap(int index, bool crop = true);
        bool GetIsForced(int index);
    }
}
