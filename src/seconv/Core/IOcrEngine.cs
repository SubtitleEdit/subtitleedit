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
}
