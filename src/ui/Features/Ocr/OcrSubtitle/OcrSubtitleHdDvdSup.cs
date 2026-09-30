using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleHdDvdSup : IOcrSubtitle
{
    public int Count { get; private set; }
    private readonly List<HdDvdSubPicture> _pictures;

    public OcrSubtitleHdDvdSup(string fileName)
    {
        _pictures = HdDvdSupParser.Parse(fileName);
        Count = _pictures.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        return _pictures[index].GetBitmap();
    }

    public TimeSpan GetStartTime(int index)
    {
        return _pictures[index].StartTime;
    }

    public TimeSpan GetEndTime(int index)
    {
        return _pictures[index].EndTime;
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

    public bool GetIsForced(int index) => _pictures[index].Forced;

    public SKPointI GetPosition(int index)
    {
        // GetBitmap crops to the ink - pair it with the cropped position.
        return _pictures[index].ImagePosition;
    }

    public SKSizeI GetScreenSize(int index)
    {
        // HD-DVD video is 1920x1080; the stream itself doesn't say.
        return new SKSizeI(1920, 1080);
    }
}
