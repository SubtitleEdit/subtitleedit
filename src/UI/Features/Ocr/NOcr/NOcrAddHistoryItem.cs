using Nikse.SubtitleEdit.Logic.Ocr;
using System;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public class NOcrAddHistoryItem
{
    public NOcrChar NOcrChar { get; set; }
    public NikseBitmap2? PreviewBitmap { get; set; }
    public int LineIndex { get; set; }
    public DateTime DateTime { get; set; }

    public NOcrAddHistoryItem(NOcrChar nOcrChar, NikseBitmap2? previewBitmap, int lineIndex)
    {
        NOcrChar = nOcrChar;
        PreviewBitmap = previewBitmap == null ? null : new NikseBitmap2(previewBitmap);
        DateTime = DateTime.Now;
        LineIndex = lineIndex;
    }

    public override string ToString()
    {
        return $"{NOcrChar.Text} - created at {DateTime.ToShortTimeString()}"; 
    }
}
