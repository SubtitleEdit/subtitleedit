using Nikse.SubtitleEdit.Core.Interfaces;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleIBinaryParagraph : IOcrSubtitle
{
    public int Count { get; private set; }
    private readonly IList<IBinaryParagraphWithPosition> _list;

    public OcrSubtitleIBinaryParagraph(IList<IBinaryParagraphWithPosition> list)
    {
        _list = list;
        Count = _list.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        return _list[index].GetBitmap();
    }

    public TimeSpan GetStartTime(int index)
    {
        return _list[index].StartTimeCode.TimeSpan;
    }

    public TimeSpan GetEndTime(int index)
    {
        return _list[index].EndTimeCode.TimeSpan;
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
        var position = _list[index].GetPosition();
        return new SKPointI(position.Left, position.Top);
    }

    public SKSizeI GetScreenSize(int index)
    {
        var screenSize = _list[index].GetScreenSize();
        return new SKSizeI((int)screenSize.Width, (int)screenSize.Height);
    }
}