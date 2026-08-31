using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;

public class OcrSubtitleSpDvdSupImages : IOcrSubtitle
{
    public int Count { get; private set; }
    private readonly string _fileName;
    private List<SpHeader> _spList = new List<SpHeader>();

    public OcrSubtitleSpDvdSupImages(string fileName)
    {
        using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var buffer = new byte[SpHeader.SpHeaderLength];
            int bytesRead = fs.Read(buffer, 0, buffer.Length);
            var header = new SpHeader(buffer);

            while (header.Identifier == "SP" && bytesRead > 0 && header.NextBlockPosition > 4)
            {
                buffer = new byte[header.NextBlockPosition];
                bytesRead = fs.Read(buffer, 0, buffer.Length);
                if (bytesRead == buffer.Length)
                {
                    header.AddPicture(buffer);
                    _spList.Add(header);
                }

                buffer = new byte[SpHeader.SpHeaderLength];
                bytesRead = fs.Read(buffer, 0, buffer.Length);
                while (bytesRead == buffer.Length && Encoding.ASCII.GetString(buffer, 0, 2) != "SP")
                {
                    fs.Seek(fs.Position - buffer.Length + 1, SeekOrigin.Begin);
                    bytesRead = fs.Read(buffer, 0, buffer.Length);
                }

                header = new SpHeader(buffer);
            }
        }

        _fileName = fileName;
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
        return new SKPointI(_spList[index].Picture.ImageDisplayArea.Left, _spList[index].Picture.ImageDisplayArea.Top);
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