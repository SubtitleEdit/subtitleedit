using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleWebVttImages : IOcrSubtitle
{
    public int Count { get; private set; }
    private readonly Subtitle _subtitle;
    private readonly string _fileName;
    private WebVttThumbnail _formatWebVttImages = new WebVttThumbnail();

    public OcrSubtitleWebVttImages(Subtitle subtitle, string fileName)
    {
        _subtitle = subtitle;
        _fileName = fileName;
        Count = subtitle.Paragraphs.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        return _formatWebVttImages.GetBitmap(_fileName, _subtitle, index);
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