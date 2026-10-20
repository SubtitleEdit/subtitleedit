using SkiaSharp;
using System.Runtime.CompilerServices;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

/// <summary>
/// A struct, not a class: IsVerticalLineTransparent pushes roughly one of these per row for
/// every column of a line bitmap, so a subtitle line used to cost on the order of Width*Height
/// heap allocations - most of them discarded the moment that method returns null. Nothing ever
/// mutates a point after construction or compares one by reference, so this is a drop-in change
/// (measured ~3x faster and ~56% less allocated on a 700x60 line).
/// </summary>
public readonly struct NiksePoint
{
    public int X { get; }
    public int Y { get; }

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
        var data1 = bmp1.GetPixelData();
        var data2 = bmp2.GetPixelData();
        var stride1 = bmp1.Width * 4;
        var stride2 = bmp2.Width * 4;
        for (var y = 0; y < bmp1.Height; y++)
        {
            var row1 = y * stride1;
            var row2 = y * stride2;
            for (var offset = 0; offset < stride1; offset += 4)
            {
                if (!IsColorCloseBgra(data1, row1 + offset, data2, row2 + offset, 20))
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

    /// <summary>
    /// IsColorClose over two raw BGRA quads. Replicates the channel mapping the
    /// SKColor-based overload sees through NikseBitmap2.GetPixel, which constructs
    /// SKColor(red: byte3, green: byte2, blue: byte1, alpha: byte0) - i.e. the
    /// "alpha" it tests is the B byte and the RGB sum is over the A, R and G bytes.
    /// Both operands are read identically, so the comparison result is unchanged.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsColorCloseBgra(ReadOnlySpan<byte> data1, int i1, ReadOnlySpan<byte> data2, int i2, int tolerance)
    {
        var alpha1 = data1[i1];
        var alpha2 = data2[i2];

        // Both transparent
        if (alpha1 < 120 && alpha2 < 120)
        {
            return true;
        }

        // Different alpha levels
        if (Math.Abs(alpha1 - alpha2) > 50)
        {
            return false;
        }

        // Both dark and non-transparent
        if (alpha1 > 250 && alpha2 > 250 &&
            data1[i1 + 3] > 90 && data1[i1 + 2] > 90 && data1[i1 + 1] > 90 &&
            data2[i2 + 3] > 90 && data2[i2 + 2] > 90 && data2[i2 + 1] > 90)
        {
            return true;
        }

        var diff = data1[i1 + 3] + data1[i1 + 2] + data1[i1 + 1] - (data2[i2 + 3] + data2[i2 + 2] + data2[i2 + 1]);
        return (uint)(diff + tolerance) <= (uint)(tolerance * 2);
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

            // Gate on the height of the actual content, not the bitmap: the VobSub decoder
            // leaves up to seven transparent rows under the text, and one DVD-sized line plus
            // that margin clears 2.2 x minLineHeight - which sent one-line images through the
            // up/down splitter, whose cut then eroded the leftmost glyph and left its crumbs
            // as a bogus second line (#14292).
            if (lineBitmaps.Count == 1 && lineBitmaps[0].NikseBitmap?.GetNonTransparentHeight() > minLineHeight * 2.2)
            {
                // A single line of large text (tall parentheses) clears that height too; the cut
                // then shaved the tip off a parenthesis into a bogus second line, read as "*".
                // A "line" of crumbs isn't one: keep the text whole then.
                var split = SplitToLinesNew(lineBitmaps[0], minLineHeight, averageLineHeight);
                if (split.Count < 2 || split.All(l => l.NikseBitmap?.GetNonTransparentHeight() >= minLineHeight / 2))
                {
                    lineBitmaps = split;
                }
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

            var lineStart = list.Count;
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

            SetBaseline(list, lineStart);
        }

        return list;
    }

    /// <summary>
    /// Sets <see cref="ImageSplitterItem2.Baseline"/> on the line's glyphs from
    /// <paramref name="lineStart"/> on: the median glyph bottom, as descenders and raised marks
    /// (apostrophes, quotes) are the minority in any line of text.
    /// </summary>
    private static void SetBaseline(List<ImageSplitterItem2> list, int lineStart)
    {
        var maxHeight = 0;
        for (var i = lineStart; i < list.Count; i++)
        {
            maxHeight = Math.Max(maxHeight, list[i].NikseBitmap?.Height ?? 0);
        }

        var glyphs = new List<(int Bottom, bool Mark)>();
        for (var i = lineStart; i < list.Count; i++)
        {
            var bitmap = list[i].NikseBitmap;
            if (bitmap != null)
            {
                // Under 45% of the tallest glyph is a mark: quotes and apostrophes reach ~40%,
                // x-height letters stay above ~55% (even beside tall brackets).
                glyphs.Add((list[i].Top + bitmap.Height, bitmap.Height * 20 < maxHeight * 9));
            }
        }

        if (glyphs.Count == 0)
        {
            return;
        }

        glyphs.Sort((a, b) => a.Bottom.CompareTo(b.Bottom));

        // Lower median: with an even count the upper one is a descender in a two-glyph line
        // ("Ну"), which made the other glyph look capital-height.
        var median = glyphs[(glyphs.Count - 1) / 2];
        var baseline = median.Bottom;

        // In a short line the marks can outnumber the letters ("-Tom--", "''Okay!'' Y--"); a
        // median on a dash or quote floating above every letter is no baseline, so the floating
        // marks are left out. Periods and commas stay in: they sit on (or hang below) the line.
        var letterBottoms = glyphs.Where(g => !g.Mark).Select(g => g.Bottom).ToList();
        var tolerance = Math.Max(1, maxHeight / 20);
        if (median.Mark && letterBottoms.Count > 0 && median.Bottom < letterBottoms.Min() - tolerance)
        {
            var grounded = glyphs.Where(g => g.Bottom >= letterBottoms.Min() - tolerance).ToList();
            baseline = grounded[(grounded.Count - 1) / 2].Bottom;
        }

        for (var i = lineStart; i < list.Count; i++)
        {
            if (list[i].NikseBitmap != null)
            {
                list[i].Baseline = baseline;
            }
        }
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

                if (keysInSequence >= minTransparentLines && lastTransparentY - startY > minLineHeight)
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

        // "started" latches across scan lines, as it does in SE4. Declared inside the loop it was
        // reset every y, and since every "started = true" breaks out of the x loop before the
        // "if (started)" below, points was ALWAYS empty - so this splitter returned the image
        // unsplit every single time and the whole "allows for up/down" path was dead.
        var started = false;

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

            while (x < bmpWidth)
            {
                var currentY = y + yChange;
                if (currentY < 0 || currentY >= bmpHeight)
                {
                    // The up/down slides can push the path past the bitmap when minLineHeight is
                    // small (adaptive values go below 10 for DVD-sized fonts) - treat it like any
                    // other blocked path instead of reading out of bounds.
                    started = true;
                    break;
                }

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

        for (var myX = x; myX < x + right && myX < bmp.Width; myX++)
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

        // Per row: everything left of this x is already transparent. Each cut used to clear its
        // rows from x = 0 again, so the clearing grew with line width times glyph count.
        var clearedUpTo = new int[bmpHeight];

        // One list for every column - the points are only used within the same iteration.
        var pointBuffer = new List<NiksePoint>(bmpHeight + 8);

        for (var x = 0; x < bmpWidth; x++)
        {
            var points = IsVerticalLineTransparent(bmp, x, pointBuffer, out var right, out var clean) ? pointBuffer : null;

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
                width = FindMaxX(points, x) - startX - 1;
                startX++;
                // Copy only the glyph rectangle and blank the trailing pixels inside it - blanking
                // outside the copied rectangle never affected the result, so a full clone of the
                // line bitmap was wasted work per glyph.
                var b1 = bmp.CopyRectangle(new NikseRectangle(startX, 0, width, bmpHeight));
                var b1Width = b1.Width;
                if (b1Width > 0)
                {
                    // Remove pixels after current
                    for (var index = 0; index < points.Count; index++)
                    {
                        var p = points[index];
                        var xStart = p.X - startX;
                        var xEnd = p.X + index - startX;
                        if (xEnd < 0 || xStart >= b1Width)
                        {
                            continue;
                        }

                        b1.MakeVerticalLinePartTransparent(xStart, xEnd, p.Y);
                    }
                }

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
                    if (p.Y < 0 || p.Y >= bmpHeight)
                    {
                        bmp.MakeVerticalLinePartTransparent(0, p.X, p.Y);
                        continue;
                    }

                    var cleared = clearedUpTo[p.Y];
                    if (p.X >= cleared)
                    {
                        bmp.MakeVerticalLinePartTransparent(cleared, p.X, p.Y);
                        clearedUpTo[p.Y] = Math.Min(p.X, bmpWidth - 1) + 1;
                    }
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

        RemoveCrumbs(parts);
        return parts;
    }

    /// <summary>
    /// The vertical path search above slides around strokes, and on a bold "a" it slips
    /// between the bowl and the 2 px foot that sticks out at the bottom right (also the leg of
    /// "R", "x", "k", "A" and the accented "à"): the foot comes out as its own 2x2 part that no
    /// database entry can match, so the OCR prompts for an unusable "character" (#16195). A
    /// part a hundredth the area of the glyph whose box it touches is a crumb of that glyph.
    /// It is dropped rather than merged back: the glyph matched without it before, and giving
    /// "à" its foot back changed its match to "á".
    /// </summary>
    private static void RemoveCrumbs(List<ImageSplitterItem2> parts)
    {
        for (var i = parts.Count - 1; i >= 0; i--)
        {
            var crumb = parts[i];
            if (crumb.NikseBitmap == null || (!IsCrumbOf(parts, i, -1) && !IsCrumbOf(parts, i, 1)))
            {
                continue;
            }

            parts.RemoveAt(i);

            // A space item on both sides of the crumb is one gap now.
            if (i > 0 && i < parts.Count && parts[i - 1].NikseBitmap == null && parts[i].NikseBitmap == null &&
                parts[i - 1].SpecialCharacter == " " && parts[i].SpecialCharacter == " ")
            {
                parts[i - 1].SpacePixels += parts[i].SpacePixels + crumb.NikseBitmap.Width;
                parts.RemoveAt(i);
            }
        }
    }

    private static bool IsCrumbOf(List<ImageSplitterItem2> parts, int crumbIndex, int direction)
    {
        // The path that cut the crumb off can count as a clean column, so a space item may sit
        // between the crumb and its glyph although their boxes touch; the boxes decide.
        var index = crumbIndex + direction;
        while (index >= 0 && index < parts.Count && parts[index].NikseBitmap == null)
        {
            index += direction;
        }

        if (index < 0 || index >= parts.Count)
        {
            return false;
        }

        var crumb = parts[crumbIndex];
        var glyph = parts[index];
        var a = crumb.NikseBitmap!;
        var b = glyph.NikseBitmap!;
        if (a.Width * a.Height * 100 > b.Width * b.Height || a.Width * 4 > b.Width || a.Height * 4 > b.Height)
        {
            return false;
        }

        // Touching boxes: no column between them, and no row either (the foot of "R" starts
        // on the row under its box).
        var gap = direction < 0 ? crumb.X - (glyph.X + b.Width) : glyph.X - (crumb.X + a.Width);
        return gap <= 0 && crumb.Y <= glyph.Y + b.Height && glyph.Y <= crumb.Y + a.Height;
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

    /// <returns>False where the old version returned null; <paramref name="points"/> is cleared and refilled.</returns>
    private static bool IsVerticalLineTransparent(NikseBitmap2 bmp, int x, List<NiksePoint> points, out bool right, out bool clean)
    {
        right = false;
        var left = false;
        var leftCount = 0;
        var rightCount = 0;
        clean = true;
        points.Clear();
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
                    return false;
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
                        return false;
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
                        return false;
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
                    return false;
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
                    return false;
                }
            }
            else
            {
                points.Add(new NiksePoint(x, y));
                y++;
            }
        }

        return true;
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