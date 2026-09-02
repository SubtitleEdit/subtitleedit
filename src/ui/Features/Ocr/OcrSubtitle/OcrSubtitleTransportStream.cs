using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleTransportStream : IOcrSubtitle
{
    private readonly List<TransportStreamSubtitle> _subtitles;
    public int Count { get; private set; }

    public OcrSubtitleTransportStream(List<TransportStreamSubtitle> subtitles)
    {
        _subtitles = subtitles;
        Count = _subtitles.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        return _subtitles[index].GetBitmap();
    }

    public TimeSpan GetStartTime(int index)
    {
        return _subtitles[index].StartTimeCode.TimeSpan;
    }

    public TimeSpan GetEndTime(int index)
    {
        return _subtitles[index].EndTimeCode.TimeSpan;
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

    public bool GetIsForced(int index)
    {
        if (index < 0 || index >= _subtitles.Count)
        {
            return false;
        }

        return _subtitles[index].IsForced;
    }

    public SKPointI GetPosition(int index)
    {
        var position = _subtitles[index].GetPosition();
        return new SKPointI(position.Left, position.Top);
    }

    public SKSizeI GetScreenSize(int index)
    {
        var screenSize = _subtitles[index].GetScreenSize();
        return new SKSizeI((int)screenSize.Width, (int)screenSize.Height);
    }
}