using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Ocr;

namespace Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;

/// <summary>
/// A line of an image based "other subtitle" (Blu-ray sup, PGS/VobSub in mkv/mp4) - it has no
/// text, so the grid shows its bitmap instead, the same way the OCR window does (issue #15911).
/// </summary>
public class PointSyncOtherImageLine : SubtitleLineViewModel
{
    public OcrSubtitleItem ImageItem { get; }

    public PointSyncOtherImageLine(OcrSubtitleItem imageItem)
    {
        ImageItem = imageItem;
        Number = imageItem.Number;
        StartTime = imageItem.StartTime;
        EndTime = imageItem.EndTime;
    }
}
