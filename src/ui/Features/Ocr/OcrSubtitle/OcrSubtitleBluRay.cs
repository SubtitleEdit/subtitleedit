using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleBluRay : IOcrSubtitle
{
    public int Count { get; private set; }

    private readonly List<BluRaySupParser.PcsData> _pcsDataList;

    public OcrSubtitleBluRay(List<BluRaySupParser.PcsData> pcsDataList)
    {
        _pcsDataList = pcsDataList;
        Count = pcsDataList.Count;
    }

    private double? _frameRate;

    /// <summary>
    /// The frame rate the cues are timed at: the one the first PCS declares when the start times
    /// are on its frame grid, else the standard rate they fit (the declared byte is often wrong,
    /// e.g. 25 over 23.976 timing). 0 when no rate fits.
    /// </summary>
    public double FrameRate
    {
        get
        {
            if (_frameRate == null)
            {
                var startTimes = new List<long>(_pcsDataList.Count);
                foreach (var pcsData in _pcsDataList)
                {
                    startTimes.Add(pcsData.StartTime);
                }

                _frameRate = _pcsDataList.Count > 0
                    ? BluRaySupFrameRateDetector.Detect(startTimes, _pcsDataList[0].FramesPerSecondType)
                    : 0;
            }

            return _frameRate.Value;
        }
    }

    /// <summary>
    /// The frame rate the first PCS declares, 0 when there is none or the code is unknown.
    /// </summary>
    public double DeclaredFrameRate => _pcsDataList.Count > 0
        ? BluRaySupPicture.GetFrameRate(_pcsDataList[0].FramesPerSecondType)
        : 0;

    public SKBitmap GetBitmap(int index)
    {
        if (index < 0 || index >= _pcsDataList.Count)
        {
            return new SKBitmap(1, 1);
        }

        return _pcsDataList[index].GetBitmap();
    }

    public TimeSpan GetStartTime(int index)
    {
        // 90 kHz PTS is a fractional millisecond; round to the whole ms subtitle formats store (#14056).
        return TimeSpanExtensions.FromMillisecondsWholeMilliseconds(_pcsDataList[index].StartTime / 90.0);
    }

    public TimeSpan GetEndTime(int index)
    {
        return TimeSpanExtensions.FromMillisecondsWholeMilliseconds(_pcsDataList[index].EndTime / 90.0);
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
        if (index < 0 || index >= _pcsDataList.Count)
        {
            return false;
        }

        return _pcsDataList[index].IsForced;
    }

    public SKPointI GetPosition(int index)
    {
        if (index < 0 || index >= _pcsDataList.Count)
        {
            return new SKPointI(-1, -1);
        }

        var position = _pcsDataList[index].GetPosition();
        return new SKPointI(position.Left, position.Top);
    }

    public SKSizeI GetScreenSize(int index)
    {
        if (index < 0 || index >= _pcsDataList.Count)
        {
            return new SKSizeI(-1, -1);
        }

        var size = _pcsDataList[index].GetScreenSize();
        return new SKSizeI((int)size.Width, (int)size.Height);
    }
}