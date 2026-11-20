using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Ocr;

/// <summary>
/// Holds the image source (Blu-ray sup, VobSub, BDN, transport stream, Matroska, ...)
/// from the most recent OCR session so other features (e.g. spell check, the main grid's
/// OCR image column) can show the original images for the OCR'd lines without re-parsing
/// the source file. (#11719, #17066)
/// </summary>
public interface IOcrImageSourceHolder
{
    IOcrSubtitle? Source { get; }
    string? FileName { get; }

    /// <summary>Raised when the source is set or cleared.</summary>
    event EventHandler? Changed;

    /// <summary>
    /// Keeps <paramref name="source"/> and which image each OCR'd line came from, keyed by the
    /// line's id - the id survives edits, undo and time code changes (offset, frame rate, sync).
    /// </summary>
    void Set(IOcrSubtitle source, string? fileName, IReadOnlyDictionary<Guid, int> lineSourceIndices);

    void Clear();

    /// <summary>
    /// The index into <see cref="Source"/> of the image a line was OCR'd from, or -1 when the
    /// line has none. Lines that were not in the OCR result (split, merged or inserted later)
    /// fall back to the image whose display time overlaps the line the most, so a split line
    /// keeps showing its image while a line typed in elsewhere shows nothing.
    /// </summary>
    int GetSourceIndex(Guid lineId, TimeSpan startTime, TimeSpan endTime);
}

public class OcrImageSourceHolder : IOcrImageSourceHolder
{
    private Dictionary<Guid, int> _lineSourceIndices = new();

    public IOcrSubtitle? Source { get; private set; }
    public string? FileName { get; private set; }

    public event EventHandler? Changed;

    public void Set(IOcrSubtitle source, string? fileName, IReadOnlyDictionary<Guid, int> lineSourceIndices)
    {
        Source = source;
        FileName = fileName;
        _lineSourceIndices = new Dictionary<Guid, int>(lineSourceIndices);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        if (Source == null && FileName == null && _lineSourceIndices.Count == 0)
        {
            return;
        }

        Source = null;
        FileName = null;
        _lineSourceIndices = new Dictionary<Guid, int>();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public int GetSourceIndex(Guid lineId, TimeSpan startTime, TimeSpan endTime)
    {
        var source = Source;
        if (source == null || source.Count == 0)
        {
            return -1;
        }

        if (_lineSourceIndices.TryGetValue(lineId, out var index) && index >= 0 && index < source.Count)
        {
            return index;
        }

        var bestIndex = -1;
        var bestOverlap = 0.0;
        for (var i = 0; i < source.Count; i++)
        {
            var overlapStart = Math.Max(startTime.TotalMilliseconds, source.GetStartTime(i).TotalMilliseconds);
            var overlapEnd = Math.Min(endTime.TotalMilliseconds, source.GetEndTime(i).TotalMilliseconds);
            var overlap = overlapEnd - overlapStart;
            if (overlap > bestOverlap)
            {
                bestOverlap = overlap;
                bestIndex = i;
            }
        }

        return bestIndex;
    }
}

public static class OcrSourceImage
{
    /// <summary>
    /// Decodes image <paramref name="index"/> of <paramref name="source"/> with the transparent
    /// screen area around the text trimmed (full-screen VobSub/DVB frames would otherwise show
    /// as a tiny strip), keeping nothing: like the OCR window's thumbnails, caching the full
    /// frame of every row scrolled past would add up to gigabytes. Null when it cannot be decoded.
    /// </summary>
    public static Avalonia.Media.Imaging.Bitmap? GetCroppedBitmap(IOcrSubtitle? source, int index)
    {
        if (source == null || index < 0 || index >= source.Count)
        {
            return null;
        }

        try
        {
            // Not disposed: some sources (image parameters, imported images) hand out the bitmap
            // they own rather than a fresh decode.
            var bitmap = source.GetBitmap(index);
            if (bitmap == null || bitmap.Width <= 1 && bitmap.Height <= 1)
            {
                return null;
            }

            var cropped = bitmap.CropTransparentColors();
            try
            {
                return cropped.ToAvaloniaBitmap();
            }
            finally
            {
                if (!ReferenceEquals(cropped, bitmap))
                {
                    cropped.Dispose();
                }
            }
        }
        catch (Exception exception)
        {
            Config.Se.LogError(exception, "OcrSourceImage: could not decode image " + index);
            return null;
        }
    }
}
