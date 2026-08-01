using System;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

/// <summary>
/// Detects the Linux/macOS dynamic loader error that fires when a downloaded speech-to-text
/// engine cannot start because one of the shared libraries it was linked against is missing.
///
/// The CrispASR Linux builds link against libopenblas.so.0 but do not ship it, so the engine
/// exits immediately with this message. Without detection the run just produced an empty
/// subtitle and only the console log hinted at why (issue #12970).
/// </summary>
public static class MissingSharedLibrary
{
    // glibc:  <exe>: error while loading shared libraries: libopenblas.so.0: cannot open shared object file: ...
    // macOS:  dyld: Library not loaded: @rpath/libfoo.dylib
    private const string GlibcMarker = "error while loading shared libraries: ";
    private const string DyldMarker = "Library not loaded: ";

    /// <summary>
    /// Returns the name of the missing library, or null if the line is not a loader error.
    /// </summary>
    public static string? GetName(string? line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return null;
        }

        var name = GetAfterMarker(line, GlibcMarker) ?? GetAfterMarker(line, DyldMarker);
        if (name == null)
        {
            return null;
        }

        // glibc appends ": cannot open shared object file: ..." after the library name
        var colon = name.IndexOf(':');
        if (colon > 0)
        {
            name = name.Substring(0, colon);
        }

        name = name.Trim();

        return name.Length == 0 ? null : name;
    }

    private static string? GetAfterMarker(string line, string marker)
    {
        var index = line.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return index < 0 ? null : line.Substring(index + marker.Length);
    }
}
