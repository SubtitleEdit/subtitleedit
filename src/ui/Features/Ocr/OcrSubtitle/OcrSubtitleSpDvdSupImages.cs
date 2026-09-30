using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleSpDvdSupImages : IOcrSubtitle
{
    public int Count { get; private set; }
    private readonly List<SpHeader> _spList;

    public OcrSubtitleSpDvdSupImages(string fileName)
    {
        _spList = SpDvdSupParser.Parse(fileName);
        Count = _spList.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        return _spList[index].Picture.GetBitmap(null, SKColors.Transparent, SKColors.White, SKColors.Black, SKColors.Black, false);
    }

    public TimeSpan GetStartTime(int index)
    {
        return _spList[index].StartTime;
    }

    public TimeSpan GetEndTime(int index)
    {
        return _spList[index].StartTime + _spList[index].Picture.Delay;
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
        // GetBitmap crops to the ink, so pair it with the cropped position rather than the
        // display area's origin (which can be the whole frame on some discs).
        return _spList[index].Picture.ImagePosition;
    }

    public SKSizeI GetScreenSize(int index)
    {
        // The video frame, not the subtitle image's own rectangle: GetPosition returns that
        // rectangle's offset, and the alignment capture divides one by the other - returning the
        // image size made every line score as right/top and prepend a bogus {\anN}.
        // DVD is 720x480 (NTSC) or 720x576 (PAL); pick by the display area we were given.
        var picture = _spList[index].Picture;
        var height = picture.ImageDisplayArea.Bottom > 480 ? 576 : 480;
        return new SKSizeI(720, height);
    }
}