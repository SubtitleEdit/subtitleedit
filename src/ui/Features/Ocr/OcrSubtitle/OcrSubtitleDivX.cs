using Nikse.SubtitleEdit.Core.ContainerFormats;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleDivX : IOcrSubtitle
{
    public int Count { get; private set; }
    private readonly List<XSub> _list;
    private readonly string _fileName;

    public OcrSubtitleDivX(List<XSub> list, string fileName)
    {
        _list = list;
        _fileName = fileName;
        Count = _list.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        return _list[index].GetImage();
    }

    public TimeSpan GetStartTime(int index)
    {
        return _list[index].Start.TimeSpan;
    }

    public TimeSpan GetEndTime(int index)
    {
        return _list[index].End.TimeSpan;
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