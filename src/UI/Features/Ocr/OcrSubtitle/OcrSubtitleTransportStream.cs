using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleTransportStream : IOcrSubtitle
{
    private TransportStreamParser _tsParser;
    private List<TransportStreamSubtitle> _subtitles;
    private string _fileName;
    public int Count { get; private set; }

    public OcrSubtitleTransportStream(TransportStreamParser tsParser, List<TransportStreamSubtitle> subtitles, string fileName)
    {
        _tsParser = tsParser;
        _subtitles = subtitles;
        _fileName = fileName;
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