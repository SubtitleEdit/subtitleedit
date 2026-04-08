using Nikse.SubtitleEdit.Logic.Ocr;
using System;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

public class BinaryOcrAddHistoryItem
{
    public BinaryOcrBitmap BinaryOcrBitmap { get; set; }
    public NikseBitmap2? PreviewBitmap { get; set; }
    public int PreviewTopMargin { get; set; }
    public int LineIndex { get; set; }
    public DateTime DateTime { get; set; }

    public BinaryOcrAddHistoryItem(BinaryOcrBitmap binaryOcrBitmap, NikseBitmap2? previewBitmap, int previewTopMargin, int lineIndex)
    {
        BinaryOcrBitmap = binaryOcrBitmap;
        PreviewBitmap = previewBitmap == null ? null : new NikseBitmap2(previewBitmap);
        PreviewTopMargin = previewTopMargin;
        DateTime = DateTime.Now;
        LineIndex = lineIndex;
    }

    public override string ToString()
    {
        return $"{BinaryOcrBitmap.Text} - created at {DateTime.ToShortTimeString()}"; 
    }
}
