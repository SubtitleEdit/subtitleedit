using Nikse.SubtitleEdit.Logic.Ocr;
using System;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

public class BinaryOcrAddHistoryItem
{
    public BinaryOcrBitmap BinaryOcrBitmap { get; set; }
    public NikseBitmap2? Bitmap { get; set; }
    public int LineIndex { get; set; }
    public DateTime DateTime { get; set; }

    public BinaryOcrAddHistoryItem(BinaryOcrBitmap binaryOcrBitmap, NikseBitmap2? bitmap, int lineIndex)
    {
        BinaryOcrBitmap = binaryOcrBitmap;
        Bitmap = bitmap;
        DateTime = DateTime.Now;
        LineIndex = lineIndex;
    }

    public override string ToString()
    {
        return $"{BinaryOcrBitmap.Text} - created at {DateTime.ToShortTimeString()}"; 
    }
}
