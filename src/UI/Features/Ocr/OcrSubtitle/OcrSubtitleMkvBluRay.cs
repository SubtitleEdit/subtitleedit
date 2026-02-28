using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleMkvBluRay : IOcrSubtitle
{
    public int Count { get; private set; }
    private readonly MatroskaTrackInfo _matroskaSubtitleInfo;
    private readonly List<BluRaySupParser.PcsData> _pcsDataList;

    public OcrSubtitleMkvBluRay(MatroskaTrackInfo matroskaSubtitleInfo, List<BluRaySupParser.PcsData> pcsDataList)
    {
        _matroskaSubtitleInfo = matroskaSubtitleInfo;
        _pcsDataList = pcsDataList;
        Count = _pcsDataList.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        return _pcsDataList[index].GetBitmap();
    }

    public TimeSpan GetStartTime(int index)
    {
        return TimeSpan.FromMilliseconds(_pcsDataList[index].StartTimeCode.TotalMilliseconds);
    }

    public TimeSpan GetEndTime(int index)
    {
        return TimeSpan.FromMilliseconds(_pcsDataList[index].EndTimeCode.TotalMilliseconds);
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
        var position = _pcsDataList[index].GetPosition();
        return new SKPointI(position.Left, position.Top);
    }

    public SKSizeI GetScreenSize(int index)
    {
        var screenSize = _pcsDataList[index].GetScreenSize();   
        return new SKSizeI((int)screenSize.Width, (int)screenSize.Height);
    }
}