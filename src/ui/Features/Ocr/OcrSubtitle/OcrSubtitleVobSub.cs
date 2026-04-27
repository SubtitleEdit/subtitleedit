using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleVobSub : IOcrSubtitle
{
    public int Count { get; private set; }

    private readonly List<VobSubMergedPack> _vobSubMergedPack;
    private List<SKColor>? _palette;

    public OcrSubtitleVobSub(List<VobSubMergedPack> vobSubMergedPack, List<SKColor>? palette = null)
    {
        _vobSubMergedPack = vobSubMergedPack;
        _palette = palette;
        Count = _vobSubMergedPack.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        if (_palette != null)
        {
            _vobSubMergedPack[index].Palette = _palette;
        }

        return _vobSubMergedPack[index].GetBitmap();
    }

    public TimeSpan GetStartTime(int index)
    {
        return _vobSubMergedPack[index].StartTime;
    }

    public TimeSpan GetEndTime(int index)
    {
        return _vobSubMergedPack[index].EndTime;
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
        var item = _vobSubMergedPack[index];
        var left = item.SubPicture.ImageDisplayArea.Left;
        var top = item.SubPicture.ImageDisplayArea.Top;
        var bmp = item.SubPicture.GetBitmap(_palette, SKColors.Transparent, SKColors.Black, SKColors.White, SKColors.Black, false, false);
        var nbmp = new NikseBitmap(bmp);
        var topCropped = nbmp.CropTopTransparent(0);
        top += topCropped;
        var bottomCropped = nbmp.CalcBottomTransparent();
        var width = bmp.Width;
        var height = bmp.Height;
        height -= topCropped;
        height -= bottomCropped;

        left += nbmp.CalcLeftCroppingTransparent();

        return new SKPointI(left, top);

        //var position = _vobSubMergedPack[index].GetPosition();  
        //return new SKPointI(position.Left, position.Top);
    }

    public SKSizeI GetScreenSize(int index)
    {
        var screenSize = _vobSubMergedPack[index].GetScreenSize();  
        return new SKSizeI((int)screenSize.Width, (int)screenSize.Height);
    }
}