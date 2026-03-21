using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
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

    public SKPointI GetPosition(int index)
    {
        if (index < 0 || index >= _imageParameterList.Count)
        {
            return new SKPointI(-1, -1);
        }

        var left = 0;
        var top = 0;
        var param = _imageParameterList[index];

        if (param.Alignment == ExportAlignment.BottomLeft || param.Alignment == ExportAlignment.MiddleLeft || param.Alignment == ExportAlignment.TopLeft)
        {
            left = param.LeftRightMargin;
        }
        else if (param.Alignment == ExportAlignment.BottomRight || param.Alignment == ExportAlignment.MiddleRight || param.Alignment == ExportAlignment.TopRight)
        {
            left = param.ScreenWidth - param.Bitmap.Width - param.LeftRightMargin;
        }

        if (param.Alignment == ExportAlignment.TopLeft || param.Alignment == ExportAlignment.TopCenter || param.Alignment == ExportAlignment.TopRight)
        {
            top = param.BottomTopMargin;
        }

        if (param.Alignment == ExportAlignment.MiddleLeft || param.Alignment == ExportAlignment.MiddleCenter || param.Alignment == ExportAlignment.MiddleRight)
        {
            top = param.ScreenHeight - (param.Bitmap.Height / 2);
        }

        if (param.OverridePosition != null &&
            param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < param.ScreenWidth &&
            param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < param.ScreenHeight)
        {
            left = param.OverridePosition.Value.X;
            top = param.OverridePosition.Value.Y;
        }

        return new SKPointI(left, top);
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