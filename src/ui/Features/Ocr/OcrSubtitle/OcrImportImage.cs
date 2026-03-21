using Nikse.SubtitleEdit.Features.Files.ImportImages;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrImportImage : IOcrSubtitle
{
    private List<ImportImageItem> _images;
    public int Count { get; private set; }

    public OcrImportImage(List<ImportImageItem> images)
    {
        _images = images;
        Count = images.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        return _images[index].GetBitmap();
    }

    public TimeSpan GetStartTime(int index)
    {
        return _images[index].Start;
    }

    public TimeSpan GetEndTime(int index)
    {
        return _images[index].End;
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
        return new SKPointI(0, 0);
    }

    public SKSizeI GetScreenSize(int index)
    {
        return new SKSizeI(0, 0);
    }
}