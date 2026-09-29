using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

/// <summary>
/// A format that keeps its images in the file itself and hands them out by index
/// (<see cref="IBinaryParagraphList"/>, e.g. PlayStation subs), with the timing in a loaded subtitle.
/// </summary>
public class OcrSubtitleBinaryParagraphList : IOcrSubtitle
{
    private static readonly SKPointI NoPosition = new SKPointI(-1, -1);
    private static readonly SKSizeI NoScreenSize = new SKSizeI(-1, -1);

    private readonly IBinaryParagraphList _binaryParagraphList;
    private readonly Subtitle _subtitle;

    public int Count { get; private set; }

    public OcrSubtitleBinaryParagraphList(IBinaryParagraphList binaryParagraphList, Subtitle subtitle)
    {
        _binaryParagraphList = binaryParagraphList;
        _subtitle = subtitle;
        Count = subtitle.Paragraphs.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        return _binaryParagraphList.GetSubtitleBitmap(index) ?? new SKBitmap(1, 1, true);
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

    public bool GetIsForced(int index)
    {
        return index >= 0 && index < Count && _binaryParagraphList.GetIsForced(index);
    }

    public SKPointI GetPosition(int index)
    {
        return NoPosition;
    }

    public SKSizeI GetScreenSize(int index)
    {
        return NoScreenSize;
    }
}
