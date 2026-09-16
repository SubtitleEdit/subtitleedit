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

    public static bool IsAmf(string videoCodec)
    {
        return videoCodec is "h264_amf" or "hevc_amf";
    }

    /// <summary>
    /// AMF "-quality" is not a quality number but a three-way preference, and the integers behind
    /// the names differ per codec - h264_amf takes 0-2 (balanced/speed/quality) while hevc_amf
    /// takes 0/5/10 (quality/balanced/speed). Burn-in used to offer 0-10 for both, so on an AMD
    /// GPU every H.264 value above 2 killed the encode outright with
    /// "Value 5.000000 for parameter 'quality' out of range [0 - 2]", and the values that did
    /// work meant the opposite of the hint. ffmpeg accepts the names for both encoders, so those
    /// are offered instead and the codec decides what they map to.
    /// </summary>
    private static readonly List<string> AmfQualities = new()
    {
        BlankTune,
        "quality",
        "balanced",
        "speed",
    };

    /// <summary>
    /// The old numeric values, mapped to the name that meant the same thing for that codec. A
    /// value the codec never accepted (3-10 for H.264) has no meaning to preserve and returns
    /// null, leaving the field blank so ffmpeg uses its own default.
    /// </summary>
    private static readonly Dictionary<string, Dictionary<string, string>> RemovedAmfQualities = new()
    {
        {
            "h264_amf", new Dictionary<string, string>
            {
                { "0", "balanced" },
                { "1", "speed" },
                { "2", "quality" },
            }
        },
        {
            "hevc_amf", new Dictionary<string, string>
            {
                { "0", "quality" },
                { "5", "balanced" },
                { "10", "speed" },
            }
        },
    };

    /// <summary>The "-quality" values offered for the AMD hardware encoders.</summary>
    public static List<string> GetAmfQualities()
    {
        return new List<string>(AmfQualities);
    }

    /// <summary>
    /// Maps a stored AMF quality to a name the encoder accepts, or null when the stored value has
    /// no equivalent. Non-AMF codecs and values that are already names are returned unchanged.
    /// </summary>
    public static string? MigrateAmfQuality(string videoCodec, string? quality)
    {
        if (string.IsNullOrWhiteSpace(quality) || !IsAmf(videoCodec))
        {
            return quality;
        }

        if (AmfQualities.Contains(quality))
        {
            return quality;
        }

        return RemovedAmfQualities[videoCodec].TryGetValue(quality, out var name) ? name : null;
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
