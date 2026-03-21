using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Nikse.SubtitleEdit.Logic.Ocr;

public class NiksePoint
{
    public int X { get; set; }
    public int Y { get; set; }

    public NiksePoint(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class NikseBitmapImageSplitter2
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsColorClose(SKColor a, SKColor b, int tolerance)
    {
        // Both transparent
        if (a.Alpha < 120 && b.Alpha < 120)
        {
            return true;
        }

        // Different alpha levels
        if (Math.Abs(a.Alpha - b.Alpha) > 50)
        {
            return false;
        }

        // Both dark and non-transparent
        if (a.Alpha > 250 && b.Alpha > 250 &&
            a.Red > 90 && a.Green > 90 && a.Blue > 90 &&
            b.Red > 90 && b.Green > 90 && b.Blue > 90)
        {
            return true;
        }

        var diff = a.Red + a.Green + a.Blue - (b.Red + b.Green + b.Blue);
        return (uint)(diff + tolerance) <= (uint)(tolerance * 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsColorClose(byte aa, byte ar, byte ag, byte ab, SKColor b, int tolerance)
    {
        // Both transparent
        if (aa < 120 && b.Alpha < 120)
        {
            return true;
        }

        // Both dark and non-transparent
        if (aa > 250 && b.Alpha > 250 &&
            ar > 90 && ag > 90 && ab > 90 &&
            b.Red > 90 && b.Green > 90 && b.Blue > 90)
        {
            return true;
        }

        var diff = ar + ag + ab - (b.Red + b.Green + b.Blue);
        return (uint)(diff + tolerance) <= (uint)(tolerance * 2);
    }

    public static NikseBitmap2 CropTopAndBottom(NikseBitmap2 bmp, out int topCropping)
    {
        var startTop = 0;
        var maxTop = bmp.Height - 2;
        if (maxTop > bmp.Height)
        {
            maxTop = bmp.Height;
        }

        for (var y = 0; y < maxTop; y++)
        {
            var allTransparent = true;
            for (var x = 0; x < bmp.Width; x++)
            {
                var a = bmp.GetAlpha(x, y);
                if (a != 0)
                {
                    allTransparent = false;
                    break;
                }
            }
            if (!allTransparent)
            {
                break;
            }

            startTop++;
        }
        topCropping = startTop;

        var h = bmp.Height;
        var bottomCroppingDone = false;
        for (var y = bmp.Height - 1; y > 3; y--)
        {
            for (var x = 0; x < bmp.Width; x++)
            {
                var a = bmp.GetAlpha(x, y);
                if (a != 0)
                {
                    bottomCroppingDone = true;
                    break;
                }
            }
            h = y;
            if (bottomCroppingDone)
            {
                break;
            }
        }
        if (h - startTop + 1 <= 0)
        {
            return new NikseBitmap2(0, 0);
        }

        return bmp.CopyRectangle(new NikseRectangle(0, startTop, bmp.Width, h - startTop + 1));
    }

    public static NikseBitmap2 CropTopAndBottom(NikseBitmap2 bmp, out int topCropping, int maxDifferentPixelsOnLine)
    {
        var startTop = 0;
        var maxTop = bmp.Height - 2;
        if (maxTop > bmp.Height)
        {
            maxTop = bmp.Height;
        }

        for (var y = 0; y < maxTop; y++)
        {
            var difference = 0;
            var allTransparent = true;
            for (var x = 1; x < bmp.Width - 1; x++)
            {
                var a = bmp.GetAlpha(x, y);
                if (a != 0)
                {
                    difference++;
                    if (difference >= maxDifferentPixelsOnLine)
                    {
                        allTransparent = false;
                        break;
                    }
                }
            }
            if (!allTransparent)
            {
                break;
            }

            startTop++;
        }
        if (startTop > 9)
        {
            startTop -= 5; // if top space > 9, then always leave blank 5 pixels on top (so . is not confused with ').
        }

        topCropping = startTop;

        for (var y = bmp.Height - 1; y > 3; y--)
        {
            var difference = 0;
            var allTransparent = true;
            for (var x = 1; x < bmp.Width - 1; x++)
            {
                var a = bmp.GetAlpha(x, y);
                if (a != 0)
                {
                    difference++;
                    if (difference >= maxDifferentPixelsOnLine)
                    {
                        allTransparent = false;
                        break;
                    }
                }
            }
            if (allTransparent == false)
            {
                return bmp.CopyRectangle(new NikseRectangle(0, startTop, bmp.Width - 1, y - startTop + 1));
            }
        }
        return bmp;
    }

    public static List<ImageSplitterItem2> SplitToLinesTransparentOrBlack(NikseBitmap2 bmp)
    {
        var startY = 0;
        var size = 0;
        var parts = new List<ImageSplitterItem2>();
        for (var y = 0; y < bmp.Height; y++)
        {
            var allTransparent = true;
            for (var x = 0; x < bmp.Width; x++)
            {
                var c = bmp.GetPixel(x, y);
                if (c.Alpha > 20 && c.Red + c.Green + c.Blue > 20)
                {
                    allTransparent = false;
                    break;
                }
            }
            if (allTransparent)
            {
                if (size > 2 && size <= 15)
                {
                    size++; // at least 15 pixels, like top of 'i' or top of 'È'
                }
                else
                {
                    if (size > 8)
                    {
                        var part = bmp.CopyRectangle(new NikseRectangle(0, startY, bmp.Width, size + 1));
                        parts.Add(new ImageSplitterItem2(0, startY, part));
                    }
                    size = 0;
                    startY = y;
                }
            }
            else
            {
                size++;
            }
        }
        if (size > 2)
        {
            parts.Add(size == bmp.Height ? new ImageSplitterItem2(0, startY, bmp) : new ImageSplitterItem2(0, startY, bmp.CopyRectangle(new NikseRectangle(0, startY, bmp.Width, size + 1))));
        }
        return parts;
    }

    /// <summary>
    /// split into lines
    /// </summary>
    public static List<ImageSplitterItem2> SplitToLines(NikseBitmap2 bmp, int minLineHeight, double averageLineHeight = -1)
    {
        var startY = 0;
        var size = 0;
        var parts = new List<ImageSplitterItem2>();
        for (var y = 0; y < bmp.Height; y++)
        {
            if (bmp.IsLineTransparent(y))
            {

                // check for appendix below text
                var appendix = y >= bmp.Height - minLineHeight;
                if (!appendix && y < bmp.Height - 10 && size > minLineHeight && minLineHeight > 15)
                {
                    var yp1 = bmp.IsLineTransparent(y + 1);
                    var yp2 = bmp.IsLineTransparent(y + 2);
                    var yp3 = bmp.IsLineTransparent(y + 3);
                    var yp4 = bmp.IsLineTransparent(y + 4);
                    var yp5 = bmp.IsLineTransparent(y + 5);
                    if (!yp1 || !yp2 || !yp3 || !yp4 || !yp5)
                    {
                        var yp6 = bmp.IsLineTransparent(y + 6);
                        var yp7 = bmp.IsLineTransparent(y + 7);
                        var yp8 = bmp.IsLineTransparent(y + 8);
                        var yp9 = bmp.IsLineTransparent(y + 9);
                        if (yp6 && yp7 && yp8 && yp9)
                        {
                            appendix = true;
                        }
                    }
                }

                if (appendix || size > 1 && size <= minLineHeight)
                {
                    size++; // at least 'lineMinHeight' pixels, like top of 'i'
                }
                else
                {
                    if (size > 1)
                    {
                        var part = bmp.CopyRectangle(new NikseRectangle(0, startY, bmp.Width, size + 1));
                        parts.Add(new ImageSplitterItem2(0, startY, part));
                    }
                    size = 0;
                    startY = y;
                }
            }
            else
            {
                size++;
            }
        }
        if (size > 1)
        {
            if (size == bmp.Height)
            {
                if (size > 100)
                {
                    return SplitToLinesTransparentOrBlack(bmp);
                }

                parts.Add(new ImageSplitterItem2(0, startY, bmp));
            }
            else
            {
                parts.Add(new ImageSplitterItem2(0, startY, bmp.CopyRectangle(new NikseRectangle(0, startY, bmp.Width, size + 1))));
            }
        }
        if (parts.Count == 1 && averageLineHeight > 5 && bmp.Height > averageLineHeight * 3)
        {
            return SplitToLinesAggressive(bmp, minLineHeight, averageLineHeight);
        }
        return parts;
    }

    private static List<ImageSplitterItem2> SplitToLinesAggressive(NikseBitmap2 bmp, int minLineHeight, double averageLineHeight)
    {
        var startY = 0;
        var size = 0;
        var parts = new List<ImageSplitterItem2>();
        for (var y = 0; y < bmp.Height; y++)
        {
            int a;
            var allTransparent = bmp.IsLineTransparent(y);

            if (size > 5 && size >= minLineHeight && size > averageLineHeight && !allTransparent && bmp.Width > 50 && y < bmp.Height - 5)
            {
                var leftX = 0;
                while (leftX < bmp.Width)
                {
                    a = bmp.GetAlpha(leftX, y);
                    if (a != 0)
                    {
                        break;
                    }
                    leftX++;
                }
                var rightX = bmp.Width;
                while (rightX > 0)
                {
                    a = bmp.GetAlpha(rightX, y - 1);
                    if (a != 0)
                    {
                        break;
                    }
                    rightX--;
                }
                if (leftX >= rightX)
                {
                    allTransparent = true;
                }

                leftX = 0;
                while (leftX < bmp.Width)
                {
                    a = bmp.GetAlpha(leftX, y - 1);
                    if (a != 0)
                    {
                        break;
                    }
                    leftX++;
                }
                rightX = bmp.Width;
                while (rightX > 0)
                {
                    a = bmp.GetAlpha(rightX, y);
                    if (a != 0)
                    {
                        break;
                    }
                    rightX--;
                }
                if (leftX >= rightX)
                {
                    allTransparent = true;
                }
            }

            if (allTransparent)
            {
                if (size > 2 && size <= minLineHeight)
                {
                    size++; // at least 'lineMinHeight' pixels, like top of 'i'
                }
                else
                {
                    if (size > 2)
                    {
                        var part = bmp.CopyRectangle(new NikseRectangle(0, startY, bmp.Width, size + 1));
                        parts.Add(new ImageSplitterItem2(0, startY, part));
                    }
                    size = 0;
                    startY = y;
                }
            }
            else
            {
                size++;
            }
        }
        if (size > 2)
        {
            if (size == bmp.Height)
            {
                if (size > 100)
                {
                    return SplitToLinesTransparentOrBlack(bmp);
                }

                parts.Add(new ImageSplitterItem2(0, startY, bmp));
            }
            else
            {
                parts.Add(new ImageSplitterItem2(0, startY, bmp.CopyRectangle(new NikseRectangle(0, startY, bmp.Width, size + 1))));
            }
        }
        if (parts.Count == 1 && averageLineHeight > 5 && bmp.Height > averageLineHeight * 3)
        {
            return parts;
        }
        return parts;
    }

    public static int IsBitmapsAlike(NikseBitmap2 bmp1, NikseBitmap2 bmp2)
    {
        var different = 0;
        var maxDiff = bmp1.Width * bmp1.Height / 5;
        for (var y = 0; y < bmp1.Height; y++)
        {
            for (var x = 0; x < bmp1.Width; x++)
            {
                if (!IsColorClose(bmp1.GetPixel(x, y), bmp2.GetPixel(x, y), 20))
                {
                    different++;
                }
            }
        }
        if (different > maxDiff)
        {
            return different + 10;
        }

        return different;
    }

    public static List<ImageSplitterItem2> SplitBitmapToLines(NikseBitmap2 bmp, int xOrMorePixelsMakesSpace, bool rightToLeft, bool topToBottom, int minLineHeight, bool autoHeight, double averageLineHeight = -1)
    {
        var list = new List<ImageSplitterItem2>();

        // split into separate lines
        var splitOld = SplitToLines(bmp, minLineHeight, averageLineHeight);
        if (!autoHeight)
        {
            return splitOld;
        }

        // fast horizontal split by x number of whole lines (3-4)
        var splitThreeBlankLines = SplitToLinesByMinTransparentHorizontalLines(bmp, minLineHeight, 3);
        var splitFourBlankLines = SplitToLinesByMinTransparentHorizontalLines(bmp, minLineHeight, 4);
        var splitBlankLines = splitThreeBlankLines.Count == splitFourBlankLines.Count ? splitFourBlankLines : splitThreeBlankLines;

        var lineBitmaps = splitOld.Count > splitBlankLines.Count ? splitOld : splitBlankLines;

        if (lineBitmaps.Count == 1 && lineBitmaps[0].NikseBitmap?.Height > minLineHeight * 2.2)
        {
            lineBitmaps = SplitToLinesNew(lineBitmaps[0], minLineHeight, averageLineHeight); // more advanced split (allows for up/down)
        }

        return lineBitmaps;
    }

    public static List<ImageSplitterItem2> SplitBitmapToLettersNew(NikseBitmap2 bmp, int xOrMorePixelsMakesSpace, bool rightToLeft, bool topToBottom, int minLineHeight, bool autoHeight, double averageLineHeight = -1)
    {
        // Split into separate lines
        var splitOld = SplitToLines(bmp, minLineHeight, averageLineHeight);

        List<ImageSplitterItem2> lineBitmaps;
        if (autoHeight)
        {
            // Fast horizontal split by x number of whole lines (3-4)
            var splitThreeBlankLines = SplitToLinesByMinTransparentHorizontalLines(bmp, minLineHeight, 3);
            var splitFourBlankLines = SplitToLinesByMinTransparentHorizontalLines(bmp, minLineHeight, 4);
            var splitBlankLines = splitThreeBlankLines.Count == splitFourBlankLines.Count ? splitFourBlankLines : splitThreeBlankLines;

            lineBitmaps = splitOld.Count > splitBlankLines.Count ? splitOld : splitBlankLines;

            if (lineBitmaps.Count == 1 && lineBitmaps[0].NikseBitmap?.Height > minLineHeight * 2.2)
            {
                lineBitmaps = SplitToLinesNew(lineBitmaps[0], minLineHeight, averageLineHeight);
            }
        }
        else
        {
            lineBitmaps = splitOld;
        }

        if (!topToBottom)
        {
            lineBitmaps.Reverse();
        }

        // Pre-allocate list with estimated capacity
        var estimatedCapacity = lineBitmaps.Count * 20; // Rough estimate
        var list = new List<ImageSplitterItem2>(estimatedCapacity);

        // Split into letters
        for (var index = 0; index < lineBitmaps.Count; index++)
        {
            var b = lineBitmaps[index];
            if (index > 0)
            {
                list.Add(new ImageSplitterItem2(Environment.NewLine));
            }

            var horizontalSplit = SplitHorizontalNew(b, xOrMorePixelsMakesSpace);
            if (rightToLeft)
            {
                // Process in reverse order directly without creating intermediate list
                var items = horizontalSplit.ToList();
                for (var i = items.Count - 1; i >= 0; i--)
                {
                    var item = items[i];
                    item.Top = index > 0 ? item.Y - b.Y : item.Y;
                    item.ParentY = item.Y;
                    list.Add(item);
                }
            }
            else
            {
                foreach (var item in horizontalSplit)
                {
                    item.Top = index > 0 ? item.Y - b.Y : item.Y;
                    item.ParentY = item.Y;
                    list.Add(item);
                }
            }
        }

        return list;
    }


    /// <summary>
    /// split into lines
    /// </summary>
    public static List<ImageSplitterItem2> SplitToLinesByMinTransparentHorizontalLines(NikseBitmap2 bmp, int minLineHeight, int minTransparentLines)
    {
        var parts = new List<ImageSplitterItem2>();
        var startY = 0;
        var lastTransparentY = -1;
        var keysInSequence = 0;
        for (var y = minLineHeight; y < bmp.Height - minLineHeight; y++)
        {
            var isLineTransparent = bmp.IsLineTransparent(y);
            if (startY == y && isLineTransparent)
            {
                startY++;
                continue; // skip start
            }

            if (isLineTransparent)
            {
                if (lastTransparentY == y - 1)
                {
                    if (keysInSequence == 0)
                    {
                        keysInSequence++;
                    }

                    keysInSequence++;
                }

                if (keysInSequence > 2 && lastTransparentY - startY > minLineHeight)
                {
                    var part = bmp.CopyRectangle(new NikseRectangle(0, startY, bmp.Width, lastTransparentY - startY - 1));
                    if (!part.IsImageOnlyTransparent() && part.GetNonTransparentHeight() + keysInSequence * 0.4 >= minLineHeight)
                    {
                        var croppedTop = part.CropTopTransparent(0);
                        parts.Add(new ImageSplitterItem2(0, startY + croppedTop, part));
                        startY = lastTransparentY + 1;
                    }
                }
                lastTransparentY = y;
            }
            else
            {
                keysInSequence = 0;
                lastTransparentY = -1;
            }
        }

        if (bmp.Height - startY > 1)
        {
            var part = bmp.CopyRectangle(new NikseRectangle(0, startY, bmp.Width, bmp.Height - startY));
            if (!part.IsImageOnlyTransparent())
            {
                var croppedTop = part.CropTopTransparent(0);
                parts.Add(new ImageSplitterItem2(0, startY + croppedTop, part));
            }
        }

        return parts;
    }


    /// <summary>
    /// split into lines
    /// </summary>
    public static List<ImageSplitterItem2> SplitToLinesNew(ImageSplitterItem2 item, int minLineHeight, double averageLineHeight = -1)
    {
        var bmp = new NikseBitmap2(item.NikseBitmap!);
        var parts = new List<ImageSplitterItem2>();
        var splitLines = new Dictionary<int, List<NiksePoint>>();
        var startY = 0;
        var bmpWidth = bmp.Width;
        var bmpHeight = bmp.Height;
        var minLineHeightDiv2 = minLineHeight / 2;

        for (var y = minLineHeight; y < bmpHeight - minLineHeight; y++)
        {
            if (startY == y && bmp.IsLineTransparent(y))
            {
                startY++;
                continue;
            }

            var points = new List<NiksePoint>();
            var yChange = 0;
            var completed = false;
            var backJump = 0;
            var x = 0;
            var maxUp = Math.Min(10, minLineHeightDiv2);
            var started = false;

            while (x < bmpWidth)
            {
                var currentY = y + yChange;
                var a1 = bmp.GetAlpha(x, currentY);
                var a2 = currentY + 1 < bmpHeight ? bmp.GetAlpha(x, currentY + 1) : 0;

                if (a1 > 150 || a2 > 150)
                {
                    // Try moving down (2-4 pixels)
                    if (x > 1 && yChange < 8 && currentY + 3 < bmpHeight &&
                        CanMoveDown(bmp, x, currentY, 2))
                    {
                        yChange += 2;
                    }
                    else if (x > 1 && yChange < 8 && currentY + 4 < bmpHeight &&
                             CanMoveDown(bmp, x, currentY, 3))
                    {
                        yChange += 3;
                    }
                    else if (x > 1 && yChange < 7 && currentY + 5 < bmpHeight &&
                             CanMoveDown(bmp, x, currentY, 4))
                    {
                        yChange += 4;
                    }
                    // Try moving up (2-4 pixels)
                    else if (x > 1 && yChange > -7 && currentY - 3 >= 0 &&
                             CanMoveUp(bmp, x, currentY, 2))
                    {
                        yChange -= 2;
                    }
                    else if (x > 1 && yChange > -7 && currentY - 4 >= 0 &&
                             CanMoveUp(bmp, x, currentY, 3))
                    {
                        yChange -= 3;
                    }
                    else if (x > 1 && yChange > -7 && currentY - 5 >= 0 &&
                             CanMoveUp(bmp, x, currentY, 4))
                    {
                        yChange -= 4;
                    }
                    else if (x > 10 && backJump < 3 && yChange > -7) // go left + up + check 12 pixels right
                    {
                        var done = false;
                        for (var i = 1; i < maxUp && !done; i++)
                        {
                            for (var k = 1; k < 9; k++)
                            {
                                if (CanGoUpAndRight(bmp, i, 12, x - k, currentY, minLineHeight))
                                {
                                    backJump++;
                                    x -= k;
                                    points.RemoveAll(p => p.X > x);
                                    yChange -= i + 1;
                                    done = true;
                                    break;
                                }
                            }
                        }

                        if (!done)
                        {
                            started = true;
                            break;
                        }
                    }
                    else
                    {
                        started = true;
                        break;
                    }
                }

                if (started)
                {
                    points.Add(new NiksePoint(x, currentY));
                }

                completed = x == bmpWidth - 1;
                x++;
            }

            if (completed)
            {
                splitLines.Add(y, points);
            }
        }

        var transparentColor = new SKColor(0, 0, 0, 0);
        foreach (var line in splitLines)
        {
            var key = line.Key;
            if (key - startY > minLineHeight && line.Value.Count > 0)
            {
                var linePoints = line.Value;
                var maxY = linePoints[0].Y;
                var minY = linePoints[0].Y;

                // Find min/max Y in single pass
                for (var i = 1; i < linePoints.Count; i++)
                {
                    var py = linePoints[i].Y;
                    if (py > maxY) maxY = py;
                    if (py < minY) minY = py;
                }

                var part = bmp.CopyRectangle(new NikseRectangle(0, startY, bmpWidth, maxY - startY));

                foreach (var point in linePoints)
                {
                    // Delete down
                    for (var y = point.Y - 1; y < startY + part.Height; y++)
                    {
                        part.SetPixel(point.X, y - startY, transparentColor);
                    }
                }

                if (!part.IsImageOnlyTransparent() && part.GetNonTransparentHeight() >= minLineHeight)
                {
                    foreach (var point in linePoints)
                    {
                        // Delete up
                        for (var y = point.Y; y >= minY; y--)
                        {
                            bmp.SetPixel(point.X, y, transparentColor);
                        }
                    }

                    var croppedTop = part.CropTopTransparent(0);
                    parts.Add(new ImageSplitterItem2(item.X, startY + croppedTop + item.Y, part));
                    startY = key + 1;
                }
            }
        }

        if (bmpHeight - startY > 1 && parts.Count > 0)
        {
            var part = bmp.CopyRectangle(new NikseRectangle(0, startY, bmpWidth, bmpHeight - startY));
            if (!part.IsImageOnlyTransparent())
            {
                var croppedTop = part.CropTopTransparent(0);
                parts.Add(new ImageSplitterItem2(item.X, startY + croppedTop + item.Y, part));
            }
        }

        return parts.Count <= 1 ? [item] : parts;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CanMoveDown(NikseBitmap2 bmp, int x, int y, int steps)
    {
        // Check if we can move down by 'steps' pixels
        // All pixels at x-1 and x must be transparent for the range
        var xMinus1 = x - 1;
        for (var i = 0; i <= steps + 1; i++)
        {
            if (bmp.GetAlpha(xMinus1, y + i) >= 150)
                return false;
        }

        for (var i = steps; i <= steps + 1; i++)
        {
            if (bmp.GetAlpha(x, y + i) >= 150)
                return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CanMoveUp(NikseBitmap2 bmp, int x, int y, int steps)
    {
        // Check if we can move up by 'steps' pixels
        // All pixels at x-1 and x must be transparent for the range
        var xMinus1 = x - 1;
        for (var i = 0; i <= steps + 1; i++)
        {
            if (bmp.GetAlpha(xMinus1, y - i) >= 150)
                return false;
        }

        for (var i = steps; i <= steps + 1; i++)
        {
            if (bmp.GetAlpha(x, y - i) >= 150)
                return false;
        }

        return true;
    }

    private static bool CanGoUpAndRight(NikseBitmap2 bmp, int up, int right, int x, int y, int minLineHeight)
    {
        if (y - up < 0 || x + right >= bmp.Width || y + minLineHeight > bmp.Height)
        {
            return false;
        }

        for (var myY = y; myY > y - up && myY > 1; myY--)
        {
            var a = bmp.GetAlpha(x, myY);
            if (a > 150)
            {
                return false;
            }
        }

        for (var myX = x; x < x + right && myX < bmp.Width; myX++)
        {
            var a = bmp.GetAlpha(myX, y - up);
            if (a > 150)
            {
                return false;
            }
        }

        return true;
    }

    private static List<ImageSplitterItem2> SplitHorizontalNew(ImageSplitterItem2 lineSplitterItem, int xOrMorePixelsMakesSpace)
    {
        var bmp = new NikseBitmap2(lineSplitterItem.NikseBitmap!);
        bmp.AddTransparentLineRight();
        var parts = new List<ImageSplitterItem2>();
        var bmpWidth = bmp.Width;
        var bmpHeight = bmp.Height;
        var startX = 0;
        var width = 0;
        var spacePixels = 0;
        var subtractSpacePixels = 0;

        for (var x = 0; x < bmpWidth; x++)
        {
            var points = IsVerticalLineTransparent(bmp, x, out var right, out var clean);

            if (points != null && clean)
            {
                spacePixels++;
            }

            if (right && points != null)
            {
                var add = FindMaxX(points, x) - x;
                width += add;
                subtractSpacePixels = add;
            }

            var newStartX = points != null ? FindMinX(points, x) : 0;
            if (points == null)
            {
                width++;
            }
            else if (width > 0 && newStartX > startX + 1)
            {
                var bmp0 = new NikseBitmap2(bmp);
                // Remove pixels after current
                for (var index = 0; index < points.Count; index++)
                {
                    var p = points[index];
                    bmp0.MakeVerticalLinePartTransparent(p.X, p.X + index, p.Y);
                }
                width = FindMaxX(points, x) - startX - 1;
                startX++;
                var b1 = bmp0.CopyRectangle(new NikseRectangle(startX, 0, width, bmpHeight));

                b1 = CropTopAndBottom(b1, out var addY);

                if (spacePixels >= xOrMorePixelsMakesSpace && parts.Count > 0)
                {
                    parts.Add(new ImageSplitterItem2(" ") { Y = addY + lineSplitterItem.Y, SpacePixels = spacePixels });
                }

                if (b1.Width > 0 && b1.Height > 0)
                {
                    parts.Add(new ImageSplitterItem2(startX + lineSplitterItem.X, addY + lineSplitterItem.Y, b1));
                }

                // Remove pixels before next letter
                foreach (var p in points)
                {
                    bmp.MakeVerticalLinePartTransparent(0, p.X, p.Y);
                }
                width = 1;
                startX = FindMinX(points, x);
                spacePixels = -subtractSpacePixels;
                subtractSpacePixels = 0;
            }
            else if (clean)
            {
                width = 1;
                startX = newStartX;
            }
        }

        return parts;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FindMinX(List<NiksePoint> points, int x)
    {
        var count = points.Count;
        for (var index = 0; index < count; index++)
        {
            var px = points[index].X;
            if (px < x)
            {
                x = px;
            }
        }
        return x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FindMaxX(List<NiksePoint> points, int x)
    {
        var count = points.Count;
        for (var index = 0; index < count; index++)
        {
            var px = points[index].X;
            if (px > x)
            {
                x = px;
            }
        }
        return x;
    }

    private static List<NiksePoint>? IsVerticalLineTransparent(NikseBitmap2 bmp, int x, out bool right, out bool clean)
    {
        right = false;
        var left = false;
        var leftCount = 0;
        var rightCount = 0;
        clean = true;
        var points = new List<NiksePoint>();
        var y = 0;
        var bmpHeight = bmp.Height;
        var bmpWidth = bmp.Width;
        var maxSlide = bmpHeight / 4;

        while (y < bmpHeight)
        {
            if (bmp.GetAlpha(x, y) > 100)
            {
                clean = false;
                if (x == 0)
                {
                    return null;
                }

                if (x < bmpWidth - 1 && y < bmpHeight - 1 && bmp.GetAlpha(x + 1, y) == 0 && bmp.GetAlpha(x + 1, y + 1) == 0)
                {
                    // If pixels to the left - move right?
                    if (bmp.GetAlpha(x - 1, y) > 0)
                    {
                        x++;
                        right = true;
                    }
                    else if (bmp.GetAlpha(x - 1, y) == 0)
                    {
                        x--;
                        left = true;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (x < bmpWidth - 1 && y == bmpHeight - 1 && bmp.GetAlpha(x + 1, y) == 0 && bmp.GetAlpha(x + 1, y - 1) == 0)
                {
                    // If pixels to the left - move right?
                    if (bmp.GetAlpha(x - 1, y) > 0)
                    {
                        x++;
                        right = true;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (bmp.GetAlpha(x - 1, y) == 0)
                {
                    x--;
                    left = true;
                }
                else if (y > 5 && bmp.GetAlpha(x - 1, y - 1) == 0)
                {
                    x--;
                    y--;
                    left = true;
                    // Remove points with Y > current Y
                    while (points.Count > 0 && points[^1].Y > y)
                    {
                        points.RemoveAt(points.Count - 1);
                    }
                }
                else if (y > 5 && bmp.GetAlpha(x - 1, y - 2) == 0)
                {
                    x--;
                    y -= 2;
                    left = true;
                    // Remove points with Y > current Y
                    while (points.Count > 0 && points[^1].Y > y)
                    {
                        points.RemoveAt(points.Count - 1);
                    }
                }
                else
                {
                    return null;
                }

                if (left)
                {
                    leftCount++;
                }

                if (right)
                {
                    rightCount++;
                }

                if (leftCount > maxSlide || rightCount > maxSlide)
                {
                    return null;
                }
            }
            else
            {
                points.Add(new NiksePoint(x, y));
                y++;
            }
        }

        return points;
    }

    //internal static int IsBitmapsAlike(ManagedBitmap2 bmp1, NikseBitmap2 bmp2)
    //{
    //    var different = 0;
    //    var maxDiff = bmp1.Width * bmp1.Height / 5;

    //    for (var x = 1; x < bmp1.Width; x++)
    //    {
    //        for (var y = 1; y < bmp1.Height; y++)
    //        {
    //            if (!IsColorClose(bmp1.GetPixel(x, y), bmp2.GetPixel(x, y), 20))
    //            {
    //                different++;
    //            }
    //        }
    //        if (different > maxDiff)
    //        {
    //            return different + 10;
    //        }
    //    }
    //    return different;
    //}

    //internal static int IsBitmapsAlike(NikseBitmap2 bmp1, ManagedBitmap2 bmp2)
    //{
    //    var different = 0;
    //    var maxDiff = bmp1.Width * bmp1.Height / 5;

    //    for (var x = 1; x < bmp1.Width; x++)
    //    {
    //        for (var y = 1; y < bmp1.Height; y++)
    //        {
    //            if (!IsColorClose(bmp1.GetPixel(x, y), bmp2.GetPixel(x, y), 20))
    //            {
    //                different++;
    //            }
    //        }
    //        if (different > maxDiff)
    //        {
    //            return different + 10;
    //        }
    //    }
    //    return different;
    //}

    internal static int IsBitmapsAlike(BinaryOcrBitmap bmp1, NikseBitmap2 bmp2)
    {
        var different = 0;
        var width = bmp1.Width;
        var height = bmp1.Height;
        var maxDiff = width * height / 5;
        var w4 = bmp2.Width * 4;

        for (var y = 0; y < height; y++)
        {
            var alpha = y * w4 + 3;
            var pixel = y * width;
            var lineEnd = pixel + width;

            while (pixel < lineEnd)
            {
                if (bmp1.GetPixel(pixel) > 0 && bmp2.GetAlpha(alpha) < 100)
                {
                    different++;
                    if (different > maxDiff)
                    {
                        return different + 10;
                    }
                }

                pixel++;
                alpha += 4;
            }
        }
        return different;
    }

    internal static int IsBitmapsAlike(BinaryOcrBitmap bmp1, BinaryOcrBitmap bmp2)
    {
        var different = 0;
        var width = bmp1.Width;
        var height = bmp1.Height;
        var maxDiff = width * height / 5;

        for (var y = 0; y < height; y++)
        {
            var pixel = y * width;
            var lineEnd = pixel + width;

            while (pixel < lineEnd)
            {
                if (bmp1.GetPixel(pixel) != bmp2.GetPixel(pixel))
                {
                    different++;
                    if (different > maxDiff)
                    {
                        return different + 10;
                    }
                }
                pixel++;
            }
        }
        return different;
    }

    internal static int IsBitmapsAlike(NikseBitmap2 bmp1, BinaryOcrBitmap bmp2)
    {
        var different = 0;
        var width = bmp1.Width;
        var height = bmp1.Height;
        var maxDiff = width * height / 5;
        var w4 = width * 4;
        var bmp2Width = bmp2.Width;

        for (var y = 1; y < height; y++)
        {
            var alpha = y * w4 + 7;
            var pixel = y * bmp2Width + 1;
            var lineEnd = pixel + width - 1;

            while (pixel < lineEnd)
            {
                if (bmp1.GetAlpha(alpha) < 100 && bmp2.GetPixel(pixel) > 0)
                {
                    different++;
                    if (different > maxDiff)
                    {
                        return different + 10;
                    }
                }

                pixel++;
                alpha += 4;
            }
        }
        return different;
    }

}