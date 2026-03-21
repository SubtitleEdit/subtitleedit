using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public interface IOcrSubtitle
{
    int Count { get; }
    SKBitmap GetBitmap(int index);
    TimeSpan GetStartTime(int index);
    TimeSpan GetEndTime(int index);
    List<OcrSubtitleItem> MakeOcrSubtitleItems();
    SKPointI GetPosition(int index);
    SKSizeI GetScreenSize(int index);
}