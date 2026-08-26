using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr;

/// <summary>
/// Sharpens the subtitle boundaries after line building. The scan samples the video at a
/// few frames per second, so every start/end time snaps to that grid - 200ms bins at the
/// default 5 fps. For each boundary this re-extracts just the one coarse interval around
/// it at <see cref="FineFps"/>, computes the same brightness masks the grouper uses, and
/// finds the frame where the subtitle actually appears or disappears. No OCR is involved,
/// so the cost is two tiny ffmpeg calls per subtitle.
/// </summary>
public static class VideoOcrTimingRefiner
{
    // 20ms bins - the residual error is then the ffmpeg seek/source-frame jitter, not the grid.
    private const int FineFps = 50;

    public sealed class Context
    {
        public string VideoFileName { get; init; } = string.Empty;

        /// <summary>Folder with the coarse scan frames ("img%06d.jpg"), used for reference masks.</summary>
        public string FramesFolder { get; init; } = string.Empty;

        public double CoarseFps { get; init; }
        public int BrightnessMinimum { get; init; }
        public int ImageSimilarityPercent { get; init; }

        /// <summary>The crop/scale part of the extraction filter, identical to the scan's.</summary>
        public string CropAndScaleFilter { get; init; } = string.Empty;
    }

    public static async Task RefineAsync(
        List<VideoOcrLineBuilder.OcrLine> lines,
        Context context,
        Action<int, int>? progress,
        CancellationToken cancellationToken)
    {
        if (lines.Count == 0 || context.CoarseFps <= 0 || context.CoarseFps >= FineFps)
        {
            return; // already sampling at or above the refinement rate
        }

        var done = 0;
        var options = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Math.Max(2, Environment.ProcessorCount / 2),
        };

        await Parallel.ForEachAsync(lines, options, async (line, ct) =>
        {
            await RefineLineAsync(line, context, ct);
            progress?.Invoke(Interlocked.Increment(ref done), lines.Count);
        });

        // Refining both sides of a shared boundary independently can create a small
        // overlap; the earlier line's (usually better-anchored) end wins.
        for (var i = 1; i < lines.Count; i++)
        {
            if (lines[i].StartMs < lines[i - 1].EndMs)
            {
                lines[i].StartMs = lines[i - 1].EndMs;
            }
        }
    }

    private static async Task RefineLineAsync(VideoOcrLineBuilder.OcrLine line, Context context, CancellationToken cancellationToken)
    {
        var coarseStepMs = 1000.0 / context.CoarseFps;
        var fineStepMs = 1000.0 / FineFps;

        // Start: the subtitle was first seen on the coarse frame at StartMs, so the true
        // appearance lies within the coarse step before it.
        var startReference = LoadCoarseMask(context, (int)Math.Round(line.StartMs * context.CoarseFps / 1000.0));
        if (startReference != null)
        {
            var windowStart = Math.Max(0, line.StartMs - coarseStepMs);
            var frames = await ExtractWindowAsync(context, windowStart, line.StartMs - windowStart + fineStepMs * 1.5, cancellationToken);
            var refined = PickTransition(GetSimilarities(frames, windowStart, fineStepMs, startReference, context), findStart: true, fineStepMs);
            CleanUp(frames);
            if (refined.HasValue && refined.Value < line.StartMs)
            {
                line.StartMs = refined.Value;
            }
        }

        // End: the subtitle was last seen on the coarse frame before EndMs, so the true
        // disappearance lies within the final coarse step.
        var endReference = LoadCoarseMask(context, (int)Math.Round(line.EndMs * context.CoarseFps / 1000.0) - 1);
        if (endReference != null)
        {
            var windowStart = Math.Max(0, line.EndMs - coarseStepMs);
            var frames = await ExtractWindowAsync(context, windowStart, line.EndMs - windowStart + fineStepMs * 1.5, cancellationToken);
            var refined = PickTransition(GetSimilarities(frames, windowStart, fineStepMs, endReference, context), findStart: false, fineStepMs);
            CleanUp(frames);
            if (refined.HasValue && refined.Value < line.EndMs && refined.Value > line.StartMs)
            {
                line.EndMs = refined.Value;
            }
        }
    }

    /// <summary>
    /// The pure decision: given the boundary window's frames flagged similar/not-similar to
    /// the in-subtitle reference, the subtitle starts at the first similar frame - and ends
    /// one frame duration after the last similar one. Null when no frame matches (a fade or
    /// borderline mask - the coarse time is kept).
    /// </summary>
    internal static double? PickTransition(IReadOnlyList<(double TimeMs, bool IsSimilar)> frames, bool findStart, double fineStepMs)
    {
        if (findStart)
        {
            foreach (var frame in frames)
            {
                if (frame.IsSimilar)
                {
                    return frame.TimeMs;
                }
            }

            return null;
        }

        for (var i = frames.Count - 1; i >= 0; i--)
        {
            if (frames[i].IsSimilar)
            {
                return frames[i].TimeMs + fineStepMs;
            }
        }

        return null;
    }

    private static List<(double TimeMs, bool IsSimilar)> GetSimilarities(
        List<string> frameFileNames, double windowStartMs, double fineStepMs, byte[] reference, Context context)
    {
        var result = new List<(double, bool)>(frameFileNames.Count);
        for (var i = 0; i < frameFileNames.Count; i++)
        {
            var mask = VideoOcrFrameGrouper.MakeThumbnail(frameFileNames[i], context.BrightnessMinimum);
            var similar = mask != null &&
                          mask.Length == reference.Length &&
                          (context.BrightnessMinimum > 0
                              ? !VideoOcrFrameGrouper.IsBlank(mask) &&
                                VideoOcrFrameGrouper.GetMaskSimilarityPercent(reference, mask) >= context.ImageSimilarityPercent
                              : VideoOcrFrameGrouper.GetSimilarityPercent(reference, mask) >= context.ImageSimilarityPercent);
            result.Add((windowStartMs + i * fineStepMs, similar));
        }

        return result;
    }

    private static byte[]? LoadCoarseMask(Context context, int frameIndex)
    {
        if (frameIndex < 0)
        {
            return null;
        }

        var fileName = Path.Combine(context.FramesFolder, $"img{frameIndex:000000}.jpg");
        if (!File.Exists(fileName))
        {
            return null;
        }

        var mask = VideoOcrFrameGrouper.MakeThumbnail(fileName, context.BrightnessMinimum);
        if (mask == null || (context.BrightnessMinimum > 0 && VideoOcrFrameGrouper.IsBlank(mask)))
        {
            return null;
        }

        return mask;
    }

    private static async Task<List<string>> ExtractWindowAsync(Context context, double windowStartMs, double lengthMs, CancellationToken cancellationToken)
    {
        var folder = Path.Combine(context.FramesFolder, "refine_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);

        var arguments = "-nostdin -y " +
                        $"-ss {(windowStartMs / 1000.0).ToString("0.###", CultureInfo.InvariantCulture)} " +
                        $"-i \"{context.VideoFileName}\" " +
                        $"-t {(lengthMs / 1000.0).ToString("0.###", CultureInfo.InvariantCulture)} " +
                        $"-vf \"fps={FineFps},{context.CropAndScaleFilter}\" " +
                        $"-q:v 2 \"{Path.Combine(folder, "f%03d.jpg")}\"";

        using var process = FfmpegGenerator.GetProcess(arguments, null);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                }
            }
            catch
            {
                // ignore
            }

            throw;
        }

        return Directory.GetFiles(folder, "*.jpg").OrderBy(p => p, StringComparer.Ordinal).ToList();
    }

    private static void CleanUp(List<string> frameFileNames)
    {
        if (frameFileNames.Count == 0)
        {
            return;
        }

        try
        {
            Directory.Delete(Path.GetDirectoryName(frameFileNames[0])!, true);
        }
        catch
        {
            // ignore
        }
    }
}
