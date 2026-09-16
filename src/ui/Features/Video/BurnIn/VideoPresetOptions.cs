using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

/// <summary>
/// The "-preset" and "-tune" values offered for the NVIDIA hardware encoders, and the migration
/// of the old presets that ffmpeg no longer accepts.
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
    /// <summary>"No tune" - whitespace, so nothing is written to the command line.</summary>
    public const string BlankTune = " ";

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

    /// <summary>
    /// nvenc tuning modes. This is what the removed "ll"/"lossless" presets really did - they
    /// picked a p-preset and set one of these. The blank entry is ffmpeg's own default (high
    /// quality) and emits no "-tune" at all; it is a space rather than an empty string so the
    /// drop-down row stays tall enough to click, the way the CRF list does it.
    /// </summary>
    private static readonly List<string> NvencTunes = new()
    {
        BlankTune,
        "hq",
        "ll",
        "ull",
        "lossless",
    };

    /// <summary>The tuning mode each removed alias set on top of its p-preset.</summary>
    private static readonly Dictionary<string, string> RemovedNvencPresetTunes = new()
    {
        { "ll", "ll" },
        { "llhq", "ll" },
        { "llhp", "ll" },
        { "lossless", "lossless" },
        { "losslesshp", "lossless" },
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
    /// The "-tune" values for <paramref name="videoCodec"/>. Only nvenc has one in the burn-in
    /// window; every other encoder gets a single blank entry, the way the preset list does for
    /// AMF and VideoToolbox.
    /// </summary>
    public static List<string> GetTunes(string videoCodec)
    {
        return IsNvenc(videoCodec) ? new List<string>(NvencTunes) : new List<string> { BlankTune };
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

    /// <summary>
    /// The tuning mode a stored preset name implies, for the aliases that set one - "lossless"
    /// was never just a speed, so migrating it to "p4" alone would silently turn a lossless
    /// encode into a lossy one. Empty for everything else.
    /// </summary>
    public static string MigrateTune(string videoCodec, string? preset)
    {
        if (preset != null && IsNvenc(videoCodec) && RemovedNvencPresetTunes.TryGetValue(preset, out var tune))
        {
            return tune;
        }

        return string.Empty;
    }
}
