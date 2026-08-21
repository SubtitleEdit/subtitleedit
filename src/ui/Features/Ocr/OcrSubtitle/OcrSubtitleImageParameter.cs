using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleImageParameter : IOcrSubtitle
{
    public int Count { get; private set; }

    private readonly List<ImageParameter> _imageParameterList;

    public OcrSubtitleImageParameter(List<ImageParameter> imageParameters)
    {
        _imageParameterList = imageParameters;
        Count = imageParameters.Count;
    }

    public SKBitmap GetBitmap(int index)
    {
        if (index < 0 || index >= _imageParameterList.Count)
        {
            return new SKBitmap(1, 1);
        }

        return _imageParameterList[index].Bitmap;
    }

    public TimeSpan GetStartTime(int index)
    {
        return _imageParameterList[index].StartTime;
    }

    public TimeSpan GetEndTime(int index)
    {
        return _imageParameterList[index].EndTime;
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
        if (index < 0 || index >= _imageParameterList.Count)
        {
            return new SKPointI(-1, -1);
        }

        // Same placement math as the full-frame export. The hand-rolled version here had no
        // branch for center or bottom alignments, so the default bottom-center came out as
        // (0,0) - which the batch converter then promoted to an override position, pinning
        // batch text->image subtitles to the top-left corner of the frame.
        var param = _imageParameterList[index];
        return FullFrameImage.GetPosition(param, param.Bitmap.Width, param.Bitmap.Height);
    }

    public SKSizeI GetScreenSize(int index)
    {
        if (index < 0 || index >= _imageParameterList.Count)
        {
            return new SKSizeI(-1, -1);
        }

        return new SKSizeI(_imageParameterList[index].ScreenWidth, _imageParameterList[index].ScreenHeight);
    }
}