using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

/// <summary>
/// "Auto-detect ASSA alignment" for OCR: maps where a subtitle image sits in the video frame
/// to an ASSA <c>{\anN}</c> tag. Shared by the OCR window and seconv so both tag the same way.
/// </summary>
public static class OcrAssaAlignment
{
    private static readonly Regex AlignmentTagRegex = new(@"^\{\\an[1-9]\}", RegexOptions.Compiled);

    // Fraction of the frame height: the empty gap that separates two blocks of lines. Lines of one
    // block are far closer than this at any resolution, also in tall images of many lines.
    private const double MinBlockGapRatio = 0.15;

    /// <summary>
    /// Returns <paramref name="text"/> with an alignment tag matching the image's place in the
    /// frame. A tall image (more than a third of the frame) with a big empty gap between its
    /// lines is taken to be several separately placed blocks, which are then tagged one by one.
    /// Nothing is added when the frame size is unknown.
    /// </summary>
    public static (string Text, bool AlignmentAdded) Detect(
        SKBitmap? bitmap, int positionX, int positionY, int screenWidth, int screenHeight, string text, bool writeAn2Tag)
    {
        if (bitmap == null || screenWidth <= 0 || screenHeight <= 0)
        {
            return (text, false);
        }

        // DVB subtitles come as an image of the whole frame with the text drawn in place, plus
        // the position of that text. Adding the two counted the offset twice and pushed every
        // line to the right/bottom third, so go by the ink alone.
        if (bitmap.Width >= screenWidth && bitmap.Height >= screenHeight)
        {
            var ink = BitmapInkBounds.Crop(bitmap);
            if (ink is null)
            {
                return (text, false);
            }

            using var inkBitmap = ink.Value.Bitmap;
            return DetectFromPlacedImage(
                inkBitmap, ink.Value.Position.X, ink.Value.Position.Y, screenWidth, screenHeight, text, writeAn2Tag);
        }

        return DetectFromPlacedImage(bitmap, positionX, positionY, screenWidth, screenHeight, text, writeAn2Tag);
    }

    /// <summary>
    /// True when <see cref="Detect"/> would see separately placed blocks in the image. Their text
    /// must keep its line breaks: re-breaking it moves words from one block to the other (#16501).
    /// </summary>
    public static bool HasSeparatedBlocks(SKBitmap? bitmap, int screenWidth, int screenHeight)
    {
        if (bitmap == null || screenWidth <= 0 || screenHeight <= 0)
        {
            return false;
        }

        if (bitmap.Width >= screenWidth && bitmap.Height >= screenHeight)
        {
            var ink = BitmapInkBounds.Crop(bitmap);
            if (ink is null)
            {
                return false;
            }

            using var inkBitmap = ink.Value.Bitmap;
            return GetImageLineBlockCentersY(inkBitmap, screenHeight) != null;
        }

        return GetImageLineBlockCentersY(bitmap, screenHeight) != null;
    }

    private static (string Text, bool AlignmentAdded) DetectFromPlacedImage(
        SKBitmap bitmap, int positionX, int positionY, int screenWidth, int screenHeight, string text, bool writeAn2Tag)
    {
        // Try to split lines and set alignment for each line
        var lines = text.Trim().SplitToLines();
        var blockCentersY = lines.Count > 1 ? GetImageLineBlockCentersY(bitmap, screenHeight) : null;
        if (blockCentersY != null)
        {
            var lineAlignments = new List<string>();
            var multiLineCenterX = positionX + bitmap.Width / 2.0;
            var multiLineRelativeX = multiLineCenterX / screenWidth;

            for (var i = 0; i < lines.Count; i++)
            {
                // OCR can find more or fewer lines than the image has: keep them in order, first to first and last to last
                var imageLineIndex = (int)Math.Round(i * (blockCentersY.Length - 1.0) / (lines.Count - 1.0));
                var lineRelativeY = (positionY + blockCentersY[imageLineIndex]) / screenHeight;
                lineAlignments.Add(GetAssaPositionFromScreen(multiLineRelativeX, lineRelativeY));
            }

            return ApplyLineAlignmentTags(lines, lineAlignments, text, writeAn2Tag);
        }

        // Calculate center point of the image on screen
        var centerX = positionX + bitmap.Width / 2.0;
        var centerY = positionY + bitmap.Height / 2.0;

        // Convert to relative position (0.0 = left/top, 1.0 = right/bottom)
        var relativeX = centerX / screenWidth;
        var relativeY = centerY / screenHeight;

        return ApplyAlignmentTag(text, GetAssaPositionFromScreen(relativeX, relativeY), writeAn2Tag);
    }

    /// <summary>
    /// Finds blocks of lines placed apart in a tall image, e.g. a sign at the top and dialogue at
    /// the bottom: lines further apart than <see cref="MinBlockGapRatio"/> of the frame are in
    /// different blocks. Returns the Y center, in the image, of the block of each image line, or
    /// null when the image holds a single block.
    /// </summary>
    private static double[]? GetImageLineBlockCentersY(SKBitmap bitmap, int screenHeight)
    {
        // Check if image height is larger than approximately 1/3 of screen height
        var imageHeightRatio = (double)bitmap.Height / screenHeight;
        if (imageHeightRatio <= 0.33)
        {
            return null;
        }

        var nbmp = new NikseBitmap2(bitmap);
        nbmp.MakeOneColor(SKColors.White);
        var imageLines = NikseBitmapImageSplitter2.SplitToLinesTransparentOrBlack(nbmp);
        if (imageLines.Count < 2)
        {
            imageLines = NikseBitmapImageSplitter2.SplitToLines(nbmp, 20);
        }

        var minGap = screenHeight * MinBlockGapRatio;
        var blockCentersY = new double[imageLines.Count];
        var blockStart = 0;
        for (var i = 1; i <= imageLines.Count; i++)
        {
            var bottom = imageLines[i - 1].Y + imageLines[i - 1].NikseBitmap!.Height;
            if (i < imageLines.Count && imageLines[i].Y - bottom <= minGap)
            {
                continue;
            }

            var centerY = (imageLines[blockStart].Y + bottom) / 2.0;
            Array.Fill(blockCentersY, centerY, blockStart, i - blockStart);
            blockStart = i;
        }

        return imageLines.Count == 0 || blockCentersY[0] == blockCentersY[^1] ? null : blockCentersY;
    }

    // "an2" is the default bottom-center alignment, so no tag is needed for it (#12393)
    public static (string Text, bool AlignmentAdded) ApplyAlignmentTag(string text, string assaPosition, bool writeAn2Tag)
    {
        if (assaPosition == "an2" && !writeAn2Tag)
        {
            return (text, false);
        }

        return ($"{{\\{assaPosition}}}{text}", true);
    }

    public static (string Text, bool AlignmentAdded) ApplyLineAlignmentTags(List<string> lines, List<string> lineAlignments, string originalText, bool writeAn2Tag)
    {
        if (!writeAn2Tag && lineAlignments.All(p => p == "an2"))
        {
            return (originalText, false);
        }

        // A line with the same alignment as the one above needs no tag of its own
        var perLine = new List<string>();
        for (var i = 0; i < lines.Count; i++)
        {
            var tag = i > 0 && lineAlignments[i] == lineAlignments[i - 1] ? string.Empty : $"{{\\{lineAlignments[i]}}}";
            perLine.Add(tag + lines[i].Trim());
        }

        return (string.Join("\n", perLine), true);
    }

    /// <summary>
    /// Maps a relative screen position (0.0 = left/top, 1.0 = right/bottom) to the 3x3 ASSA
    /// grid. ASSA numbers the grid from the bottom: an1-an3 bottom, an4-an6 middle, an7-an9 top.
    /// </summary>
    public static string GetAssaPositionFromScreen(double relativeX, double relativeY)
    {
        var column = relativeX < 0.33 ? 0 : relativeX > 0.67 ? 2 : 1; // left, center, right
        var row = relativeY < 0.33 ? 2 : relativeY > 0.67 ? 0 : 1;    // top of screen = an7-an9
        return "an" + (row * 3 + column + 1);
    }

    /// <summary>
    /// Splits text into groups of lines sharing the same leading {\anN} alignment tag.
    /// A line without a tag continues the current group. Returns the original text as a single
    /// group when fewer than two distinct alignments are present.
    /// </summary>
    public static List<string> SplitTextByAlignmentGroups(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new List<string> { text };
        }

        var lines = text.SplitToLines();
        var groups = new List<List<string>>();
        var currentTag = string.Empty;
        var currentLines = new List<string>();

        foreach (var line in lines)
        {
            var match = AlignmentTagRegex.Match(line);
            var tag = match.Success ? match.Value : string.Empty;

            if (currentLines.Count == 0)
            {
                currentTag = tag;
                currentLines.Add(line);
            }
            else if (tag.Length > 0 && tag != currentTag)
            {
                groups.Add(currentLines);
                currentTag = tag;
                currentLines = new List<string> { line };
            }
            else
            {
                currentLines.Add(line);
            }
        }

        if (currentLines.Count > 0)
        {
            groups.Add(currentLines);
        }

        if (groups.Count <= 1)
        {
            return new List<string> { text };
        }

        return groups.Select(g => string.Join("\n", g)).ToList();
    }
}
