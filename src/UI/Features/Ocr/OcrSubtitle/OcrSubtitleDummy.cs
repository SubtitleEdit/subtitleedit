using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleDummy : IOcrSubtitle
{
    public int Count { get; private set; }
    private readonly Subtitle _subtitle;

    public OcrSubtitleDummy(Subtitle subtitle)
    {
        _subtitle = new Subtitle(subtitle, false);
        Count = subtitle.Paragraphs.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        return new SKBitmap(1,1, true);
    }

    public TimeSpan GetStartTime(int index)
    {
        return _subtitle.Paragraphs[index].StartTime.TimeSpan;
    }

    public TimeSpan GetEndTime(int index)
    {
        return _subtitle.Paragraphs[index].EndTime.TimeSpan;
    }

    public List<OcrSubtitleItem> MakeOcrSubtitleItems()
    {
        var ocrSubtitleItems = new List<OcrSubtitleItem>(Count);
        for (var i = 0; i < Count; i++)
        {
            ocrSubtitleItems.Add(new OcrSubtitleItem(this, i));
        }

        return ocrSubtitleItems;
    }

    public SKPointI GetPosition(int index)
    {
        return new SKPointI(-1, -1);
    }

    public SKSizeI GetScreenSize(int index)
    {
        return new SKSizeI(-1, -1);
    }
}