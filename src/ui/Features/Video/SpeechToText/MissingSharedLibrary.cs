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

    /// <summary>
    /// True for libraries that are part of an engine download rather than something the user
    /// installs from their distro. A missing one means the engine folder is incomplete - which
    /// is what happened to the whisper.cpp Linux archives in issue #13680, where whisper-cli
    /// shipped without the libwhisper.so.1 and libggml.so.0 it links against.
    /// </summary>
    public static bool IsBundledWithEngine(string? libraryName)
    {
        if (string.IsNullOrEmpty(libraryName))
        {
            return false;
        }

        // dyld reports the full install name, e.g. "@rpath/libwhisper.1.dylib"
        var name = libraryName;
        var slash = name.LastIndexOfAny(new[] { '/', '\\' });
        if (slash >= 0)
        {
            name = name.Substring(slash + 1);
        }

        foreach (var prefix in BundledLibraryPrefixes)
        {
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static readonly string[] BundledLibraryPrefixes =
    {
        "libwhisper", // whisper.cpp
        "libggml",    // whisper.cpp / qwen3-asr.cpp - the ggml core and its backends
        "libllama",   // llama.cpp based engines
        "libmtmd",    // llama.cpp multimodal helper
    };

    private static string? GetAfterMarker(string line, string marker)
    {
        var index = line.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return index < 0 ? null : line.Substring(index + marker.Length);
    }
}
