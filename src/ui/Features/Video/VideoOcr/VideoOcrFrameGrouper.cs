using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr;

/// <summary>
/// Collapses the sampled video frames into groups of consecutive near-identical frames so
/// only one frame per on-screen subtitle needs OCR. Frames are compared on a small
/// grayscale thumbnail; with a brightness minimum set, pixels below it are zeroed first,
/// which makes the comparison follow the (bright) subtitle text instead of the moving
/// video behind it - and makes frames without any bright pixels skippable as blank.
/// </summary>
public static class VideoOcrFrameGrouper
{
    private const int ThumbnailWidth = 96;

    // The brightness mask is thresholded at this width, then max-pooled down to
    // ThumbnailWidth. Thresholding a 96px thumbnail directly destroys the mask: at that
    // scale the anti-aliased blend of white glyphs and their dark outlines averages below
    // any sensible brightness minimum, so whole subtitles read as "blank" and the frames
    // are never OCR'ed. At ~360px the glyph cores survive thresholding, and max-pooling
    // the binary mask keeps thin strokes visible to the group comparison.
    private const int MaskSourceWidth = 360;

    // Less than this fraction of bright pixels counts as "no text on screen".
    private const double BlankFraction = 0.002;

    public static List<VideoOcrFrameGroup> Group(
        IReadOnlyList<string> frameFileNames,
        int brightnessMinimum,
        int imageSimilarityPercent,
        Action<int, int>? progress,
        CancellationToken cancellationToken)
    {
        var groups = new List<VideoOcrFrameGroup>();
        byte[]? lastThumbnail = null;
        VideoOcrFrameGroup? current = null;
        var currentFileList = new List<string>();

        for (var index = 0; index < frameFileNames.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Invoke(index + 1, frameFileNames.Count);

            var thumbnail = MakeThumbnail(frameFileNames[index], brightnessMinimum);
            if (thumbnail == null)
            {
                // Unreadable frame - keep it inside the current group. Skipping it outright left
                // the group's EndFrame behind, which shortens the subtitle's end time.
                if (current != null)
                {
                    current.EndFrame = index;
                    currentFileList.Add(frameFileNames[index]);
                }

                continue;
            }

            var isBlank = brightnessMinimum > 0 && IsBlank(thumbnail);

            var isSameAsLast = current != null &&
                               lastThumbnail != null &&
                               lastThumbnail.Length == thumbnail.Length &&
                               current.IsBlank == isBlank &&
                               (current.IsBlank ||
                                (brightnessMinimum > 0
                                    ? GetMaskSimilarityPercent(lastThumbnail, thumbnail)
                                    : GetSimilarityPercent(lastThumbnail, thumbnail)) >= imageSimilarityPercent);

            if (isSameAsLast)
            {
                current!.EndFrame = index;
                currentFileList.Add(frameFileNames[index]);
            }
            else
            {
                CloseGroup(groups, current, currentFileList);
                current = new VideoOcrFrameGroup
                {
                    StartFrame = index,
                    EndFrame = index,
                    IsBlank = isBlank,
                };
                currentFileList = new List<string> { frameFileNames[index] };
            }

            lastThumbnail = thumbnail;
        }

        CloseGroup(groups, current, currentFileList);

        return groups;
    }

    private static void CloseGroup(List<VideoOcrFrameGroup> groups, VideoOcrFrameGroup? group, List<string> files)
    {
        if (group == null || files.Count == 0)
        {
            return;
        }

        // The middle frame avoids fade-in/fade-out edges of the subtitle.
        group.RepresentativeFileName = files[files.Count / 2];
        groups.Add(group);
    }

    private static bool IsBlank(byte[] thumbnail)
    {
        var bright = 0;
        foreach (var b in thumbnail)
        {
            if (b > 0)
            {
                bright++;
            }
        }

        return bright < thumbnail.Length * BlankFraction;
    }

    /// <summary>
    /// Overlap (Jaccard) similarity of two bright-pixel masks in percent. Unlike a plain
    /// pixel difference this is relative to the amount of bright pixels, so a subtitle text
    /// change registers as a big change even when a bright background dominates the area.
    /// </summary>
    internal static int GetMaskSimilarityPercent(byte[] a, byte[] b)
    {
        if (a.Length == 0 || a.Length != b.Length)
        {
            return 0;
        }

        var intersection = 0;
        var union = 0;
        for (var i = 0; i < a.Length; i++)
        {
            var inA = a[i] > 0;
            var inB = b[i] > 0;
            if (inA || inB)
            {
                union++;
                if (inA && inB)
                {
                    intersection++;
                }
            }
        }

        if (union == 0)
        {
            return 100;
        }

        return (int)Math.Round(intersection * 100.0 / union);
    }

    internal static int GetSimilarityPercent(byte[] a, byte[] b)
    {
        if (a.Length == 0 || a.Length != b.Length)
        {
            return 0;
        }

        long diff = 0;
        for (var i = 0; i < a.Length; i++)
        {
            diff += Math.Abs(a[i] - b[i]);
        }

        var meanDiff = diff / (double)a.Length;
        return (int)Math.Round(100.0 - meanDiff * 100.0 / 255.0);
    }

    private static byte[]? MakeThumbnail(string fileName, int brightnessMinimum)
    {
        try
        {
            using var bitmap = DecodeScaledDown(fileName, brightnessMinimum > 0 ? MaskSourceWidth : ThumbnailWidth);
            if (bitmap == null || bitmap.Width == 0 || bitmap.Height == 0)
            {
                return null;
            }

            if (brightnessMinimum > 0)
            {
                return MakePooledMask(bitmap, brightnessMinimum);
            }

            var height = Math.Max(1, (int)Math.Round(bitmap.Height * ThumbnailWidth / (double)bitmap.Width));
            using var small = bitmap.Resize(new SKImageInfo(ThumbnailWidth, height), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
            if (small == null)
            {
                return null;
            }

            var pixels = small.Pixels;
            var result = new byte[pixels.Length];
            for (var i = 0; i < pixels.Length; i++)
            {
                var c = pixels[i];
                result[i] = (byte)((c.Red * 299 + c.Green * 587 + c.Blue * 114) / 1000);
            }

            return result;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Thresholds the bitmap at its own (working) resolution and max-pools the resulting
    /// binary mask down to a ThumbnailWidth-wide grid: a grid cell is bright when any pixel
    /// in its source block clears the brightness minimum, so thin glyph strokes survive.
    /// </summary>
    private static byte[] MakePooledMask(SKBitmap bitmap, int brightnessMinimum)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var thumbHeight = Math.Max(1, (int)Math.Round(height * ThumbnailWidth / (double)width));
        var result = new byte[ThumbnailWidth * thumbHeight];
        var pixels = bitmap.Pixels;

        for (var y = 0; y < height; y++)
        {
            var thumbRow = Math.Min(thumbHeight - 1, y * thumbHeight / height) * ThumbnailWidth;
            var sourceRow = y * width;
            for (var x = 0; x < width; x++)
            {
                var c = pixels[sourceRow + x];
                var luma = (c.Red * 299 + c.Green * 587 + c.Blue * 114) / 1000;
                if (luma >= brightnessMinimum)
                {
                    result[thumbRow + Math.Min(ThumbnailWidth - 1, x * ThumbnailWidth / width)] = 255;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Writes a copy of a video frame with everything below the brightness minimum blacked
    /// out - the same input filtering VideOCR applies before recognition. Detector-based
    /// OCR (PaddleOCR) then only sees the bright subtitle text, not darker scene text like
    /// shirt prints or credits, which its detector otherwise picks up and prepends to
    /// subtitles. The keep-mask is dilated a little so anti-aliased glyph edges survive.
    /// Vision/VLM engines are better off with the natural frame - measured: masking cost
    /// Apple Vision accuracy while it clearly helped PaddleOCR - so only the Paddle path
    /// uses this.
    /// </summary>
    public static bool WriteMaskedCopy(string sourceFileName, string targetFileName, int brightnessMinimum)
    {
        try
        {
            using var bitmap = SKBitmap.Decode(sourceFileName);
            if (bitmap == null || bitmap.Width == 0 || bitmap.Height == 0)
            {
                return false;
            }

            var width = bitmap.Width;
            var height = bitmap.Height;
            var pixels = bitmap.Pixels;
            var keep = new bool[pixels.Length];
            for (var i = 0; i < pixels.Length; i++)
            {
                var c = pixels[i];
                keep[i] = (c.Red * 299 + c.Green * 587 + c.Blue * 114) / 1000 >= brightnessMinimum;
            }

            const int dilate = 2;
            var keepDilated = new bool[pixels.Length];
            for (var y = 0; y < height; y++)
            {
                var row = y * width;
                for (var x = 0; x < width; x++)
                {
                    if (!keep[row + x])
                    {
                        continue;
                    }

                    var yEnd = Math.Min(height - 1, y + dilate);
                    var xEnd = Math.Min(width - 1, x + dilate);
                    for (var yy = Math.Max(0, y - dilate); yy <= yEnd; yy++)
                    {
                        var rowOut = yy * width;
                        for (var xx = Math.Max(0, x - dilate); xx <= xEnd; xx++)
                        {
                            keepDilated[rowOut + xx] = true;
                        }
                    }
                }
            }

            var black = new SKColor(0, 0, 0);
            for (var i = 0; i < pixels.Length; i++)
            {
                if (!keepDilated[i])
                {
                    pixels[i] = black;
                }
            }

            bitmap.Pixels = pixels;
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 92);
            File.WriteAllBytes(targetFileName, data.ToArray());
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Decodes an image at reduced size when the codec supports it (JPEG decodes natively
    /// at 1/2, 1/4, 1/8 scale) - much cheaper than a full decode for thumbnail use.
    /// </summary>
    private static SKBitmap? DecodeScaledDown(string fileName, int targetWidth)
    {
        try
        {
            using var codec = SKCodec.Create(fileName);
            if (codec == null || codec.Info.Width <= 0)
            {
                return SKBitmap.Decode(fileName);
            }

            var scaled = codec.GetScaledDimensions(targetWidth / (float)codec.Info.Width);
            var info = new SKImageInfo(scaled.Width, scaled.Height);
            return SKBitmap.Decode(codec, info) ?? SKBitmap.Decode(fileName);
        }
        catch
        {
            return SKBitmap.Decode(fileName);
        }
    }
}
