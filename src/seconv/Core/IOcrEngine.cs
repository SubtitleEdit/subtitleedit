using SkiaSharp;

namespace SeConv.Core;

/// <summary>
/// Common interface for all OCR engines used by seconv. Engines are owned by the caller
/// (use <c>using</c>) and may hold subprocess handles, HTTP clients, or in-memory model state.
/// </summary>
internal interface IOcrEngine : IDisposable
{
    /// <summary>Human-readable engine identifier (used in progress messages).</summary>
    string Name { get; }

    /// <summary>Recognises a single subtitle bitmap.</summary>
    string Recognize(SKBitmap bitmap);

    /// <summary>
    /// Recognises multiple subtitle bitmaps. The default implementation calls
    /// <see cref="Recognize(SKBitmap)"/> once per bitmap and reports completed images.
    /// </summary>
    IReadOnlyList<string> Recognize(IReadOnlyList<SKBitmap> bitmaps, Action<int>? progress = null)
    {
        var results = new List<string>(bitmaps.Count);
        for (var i = 0; i < bitmaps.Count; i++)
        {
            results.Add(Recognize(bitmaps[i]));
            progress?.Invoke(i + 1);
        }

        return results;
    }
}
