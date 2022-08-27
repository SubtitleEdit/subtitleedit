using System.Drawing;

namespace Nikse.SubtitleEdit.Core.Interfaces
{
    public interface IBinaryParagraphList
    {
        Bitmap GetSubtitleBitmap(int index, bool crop = true, bool preCheck = false);
        bool GetIsForced(int index);
    }
}
