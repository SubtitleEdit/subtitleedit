using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleMp4VobSub : IOcrSubtitle
{
    public int Count { get; private set; }

    private readonly Trak _mp4SubtitleTrack;
    private readonly List<Paragraph> _paragraphs;

    public OcrSubtitleMp4VobSub(Trak mp4SubtitleTrack, List<Paragraph> paragraphs)
    {
        _mp4SubtitleTrack = mp4SubtitleTrack;
        _paragraphs = paragraphs;
        Count = _paragraphs.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        return _mp4SubtitleTrack.Mdia.Minf.Stbl.SubPictures[index].GetBitmap(null, SKColors.Transparent, SKColors.Black, SKColors.White, SKColors.Black, false);
    }

    public TimeSpan GetStartTime(int index)
    {
        return _paragraphs[index].StartTime.TimeSpan;
    }

    public TimeSpan GetEndTime(int index)
    {
        return _paragraphs[index].EndTime.TimeSpan;
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