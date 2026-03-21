using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleBluRay : IOcrSubtitle
{
    public int Count { get; private set; }

    private readonly List<BluRaySupParser.PcsData> _pcsDataList;

    public OcrSubtitleBluRay(List<BluRaySupParser.PcsData> pcsDataList)
    {
        _pcsDataList = pcsDataList;
        Count = pcsDataList.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        if (index < 0 || index >= _pcsDataList.Count)
        {
            return new SKBitmap(1, 1);
        }

        return _pcsDataList[index].GetBitmap();
    }

    public TimeSpan GetStartTime(int index)
    {
        return TimeSpan.FromMilliseconds(_pcsDataList[index].StartTime / 90.0);
    }

    public TimeSpan GetEndTime(int index)
    {
        return TimeSpan.FromMilliseconds(_pcsDataList[index].EndTime / 90.0);
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
        if (index < 0 || index >= _pcsDataList.Count)
        {
            return new SKPointI(-1, -1);
        }

        var position = _pcsDataList[index].GetPosition();
        return new SKPointI(position.Left, position.Top);
    }

    public SKSizeI GetScreenSize(int index)
    {
        if (index < 0 || index >= _pcsDataList.Count)
        {
            return new SKSizeI(-1, -1);
        }

        var size = _pcsDataList[index].GetScreenSize();
        return new SKSizeI((int)size.Width, (int)size.Height);
    }
}