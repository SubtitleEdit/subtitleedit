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

    public SKPointI GetPosition(int index)
    {
        return new SKPointI(_spList[index].Picture.ImageDisplayArea.Left, _spList[index].Picture.ImageDisplayArea.Top);
    }

    public SKSizeI GetScreenSize(int index)
    {
        return new SKSizeI(_spList[index].Picture.ImageDisplayArea.Width, _spList[index].Picture.ImageDisplayArea.Height);
    }
}