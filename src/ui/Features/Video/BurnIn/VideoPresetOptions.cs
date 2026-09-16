using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

/// <summary>
/// The "-preset" values offered for the NVIDIA hardware encoders, and the migration of the old
/// ones that ffmpeg no longer accepts.
/// </summary>
/// <remarks>
/// ffmpeg 9.0 dropped the deprecated nvenc preset aliases ("default", "hp", "hq", "bd", "ll",
/// "llhq", "llhp", "lossless", "losslesshp") - passing one now fails the encode outright with
/// "exit code -22" instead of warning, and ffmpeg 9.0.1 is what Subtitle Edit downloads on
/// Windows, so every one of those entries was a dead end (issue #14927). Only the names that
/// ffmpeg still has are offered; they have existed since ffmpeg 4.4, so this also works with an
/// older ffmpeg the user may have on PATH.
/// </remarks>
public static class VideoPresetOptions
{
    private static readonly List<string> NvencPresets = new()
    {
        "slow",
        "medium",
        "fast",
        "p1",
        "p2",
        "p3",
        "p4",
        "p5",
        "p6",
        "p7",
    };

    /// <summary>
    /// The aliases ffmpeg removed, mapped to what it used to expand them to internally. The
    /// low-latency and lossless variants also set a tuning mode, which burn-in has no control
    /// for, so those keep only the speed/quality half of the old meaning.
    /// </summary>
    private static readonly Dictionary<string, string> RemovedNvencPresets = new()
    {
        { "default", "p4" },
        { "hp", "p1" },
        { "hq", "p7" },
        { "bd", "p5" },
        { "ll", "p4" },
        { "llhq", "p7" },
        { "llhp", "p1" },
        { "lossless", "p4" },
        { "losslesshp", "p1" },
    };

    public static bool IsNvenc(string videoCodec)
    {
        return videoCodec is "h264_nvenc" or "hevc_nvenc";
    }

    public static List<string> GetNvencPresets()
    {
        return new List<string>(NvencPresets);
    }

    /// <summary>
    /// Maps a stored preset name to one the current ffmpeg still knows. Without this, a settings
    /// file holding e.g. "hq" would silently fall back to the default preset and encode at a
    /// quality the user never picked.
    /// </summary>
    public static string? Migrate(string videoCodec, string? preset)
    {
        if (preset != null && IsNvenc(videoCodec) && RemovedNvencPresets.TryGetValue(preset, out var replacement))
        {
            return replacement;
        }

        return preset;
    }
}
