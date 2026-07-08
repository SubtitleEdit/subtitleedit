using SkiaSharp;
using System;
using System.Collections.Generic;
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
                continue; // unreadable frame - treat as part of the current group
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
            using var bitmap = DecodeScaledDown(fileName);
            if (bitmap == null || bitmap.Width == 0 || bitmap.Height == 0)
            {
                return null;
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
                var luma = (byte)((c.Red * 299 + c.Green * 587 + c.Blue * 114) / 1000);
                if (brightnessMinimum > 0)
                {
                    result[i] = luma >= brightnessMinimum ? (byte)255 : (byte)0;
                }
                else
                {
                    result[i] = luma;
                }
            }

            return result;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Decodes an image at reduced size when the codec supports it (JPEG decodes natively
    /// at 1/2, 1/4, 1/8 scale) - much cheaper than a full decode for thumbnail use.
    /// </summary>
    private static SKBitmap? DecodeScaledDown(string fileName)
    {
        try
        {
            using var codec = SKCodec.Create(fileName);
            if (codec == null || codec.Info.Width <= 0)
            {
                return SKBitmap.Decode(fileName);
            }

            var scaled = codec.GetScaledDimensions(ThumbnailWidth / (float)codec.Info.Width);
            var info = new SKImageInfo(scaled.Width, scaled.Height);
            return SKBitmap.Decode(codec, info) ?? SKBitmap.Decode(fileName);
        }
        catch
        {
            return SKBitmap.Decode(fileName);
        }
    }
}
