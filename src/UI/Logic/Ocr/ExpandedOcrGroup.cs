using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Ocr;

public sealed class ExpandedOcrGroup
{
    public IReadOnlyList<ImageSplitterItem2> Items { get; }
    public ImageSplitterItem2 FirstItem => Items[0];
    public SKRectI Bounds { get; }
    public NikseBitmap2 PreviewBitmap { get; }
    public int MinTop { get; }
    public int MinY { get; }
    public int Count => Items.Count;
    public int PreviewTopMargin => FirstItem.Top - MinTop;

    private ExpandedOcrGroup(
        List<ImageSplitterItem2> items,
        SKRectI bounds,
        NikseBitmap2 previewBitmap,
        int minTop,
        int minY)
    {
        Items = items;
        Bounds = bounds;
        PreviewBitmap = previewBitmap;
        MinTop = minTop;
        MinY = minY;
    }

    public static ExpandedOcrGroup? Create(NikseBitmap2 sourceBitmap, IReadOnlyList<ImageSplitterItem2> letters, int startIndex, int count)
    {
        if (count <= 1 || startIndex < 0 || startIndex + count > letters.Count)
        {
            return null;
        }

        var items = new List<ImageSplitterItem2>(count);
        var minTop = int.MaxValue;
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;

        for (var i = startIndex; i < startIndex + count; i++)
        {
            var letter = letters[i];
            if (letter.NikseBitmap == null)
            {
                return null;
            }

            items.Add(letter);
            minTop = Math.Min(minTop, letter.Top);
            minX = Math.Min(minX, letter.X);
            minY = Math.Min(minY, letter.Y);
            maxX = Math.Max(maxX, letter.X + letter.NikseBitmap.Width);
            maxY = Math.Max(maxY, letter.Y + letter.NikseBitmap.Height);
        }

        var bounds = new SKRectI(minX, minY, maxX, maxY);
        var previewBitmap = sourceBitmap.CopyRectangle(new NikseRectangle(minX, minY, maxX - minX, maxY - minY));
        return new ExpandedOcrGroup(items, bounds, previewBitmap, minTop, minY);
    }

    public NOcrChar CreateNOcrChar()
    {
        return new NOcrChar
        {
            Width = PreviewBitmap.Width,
            Height = PreviewBitmap.Height,
            MarginTop = PreviewTopMargin,
            ExpandCount = Count,
        };
    }

    public BinaryOcrBitmap CreateBinaryOcrBitmap()
    {
        var firstBitmap = FirstItem.NikseBitmap ?? throw new InvalidOperationException("Expanded OCR group is missing the first bitmap.");
        var binaryOcrBitmap = new BinaryOcrBitmap(firstBitmap)
        {
            X = FirstItem.X,
            Y = FirstItem.Top,
            ExpandCount = Count,
            ExpandedList = new List<BinaryOcrBitmap>(),
        };

        for (var i = 1; i < Items.Count; i++)
        {
            var item = Items[i];
            if (item.NikseBitmap == null)
            {
                throw new InvalidOperationException("Expanded OCR group is missing a child bitmap.");
            }

            binaryOcrBitmap.ExpandedList.Add(new BinaryOcrBitmap(item.NikseBitmap)
            {
                X = item.X,
                Y = item.Top,
            });
        }

        return binaryOcrBitmap;
    }
}
