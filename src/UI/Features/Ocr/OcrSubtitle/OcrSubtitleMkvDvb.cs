using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleMkvDvb : IOcrSubtitle
{
    public int Count { get; private set; }
    private readonly MatroskaTrackInfo _matroskaSubtitleInfo;
    private Subtitle _subtitle;
    private List<DvbSubPes> _subtitleImages;

    public OcrSubtitleMkvDvb(MatroskaTrackInfo matroskaSubtitleInfo, Subtitle subtitle, List<DvbSubPes> subtitleImages)
    {
        _matroskaSubtitleInfo = matroskaSubtitleInfo;
        _subtitle = subtitle;
        _subtitleImages = subtitleImages;
        Count = _subtitleImages.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        return _subtitleImages[index].GetImageFull();
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
        var position = _subtitleImages[index].GetPosition();    
        return new SKPointI(position.Left, position.Top);
    }

    public SKSizeI GetScreenSize(int index)
    {
        var screenSize = _subtitleImages[index].GetScreenSize();
        return new SKSizeI((int)screenSize.Width, (int)screenSize.Height);
    }
}