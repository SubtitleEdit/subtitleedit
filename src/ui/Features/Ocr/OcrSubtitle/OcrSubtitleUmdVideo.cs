using Nikse.SubtitleEdit.Core.ContainerFormats.ProgramStream;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

/// <summary>
/// The pictures of one PSP UMD Video subtitle stream (.MPS/.PMF, or a ".subs" dump of one).
/// </summary>
public class OcrSubtitleUmdVideo : IOcrSubtitle
{
    public int Count { get; private set; }
    private readonly List<UmdVideoSubtitle> _pictures;

    public OcrSubtitleUmdVideo(List<UmdVideoSubtitle> pictures)
    {
        _pictures = pictures;
        Count = _pictures.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        return _pictures[index].GetBitmap() ?? new SKBitmap(1, 1);
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

    public bool GetIsForced(int index) => false;

    public SKPointI GetPosition(int index)
    {
        return new SKPointI(_pictures[index].X, _pictures[index].Y);
    }

    public SKSizeI GetScreenSize(int index)
    {
        return new SKSizeI(UmdVideoSubtitle.ScreenWidth, UmdVideoSubtitle.ScreenHeight);
    }
}
