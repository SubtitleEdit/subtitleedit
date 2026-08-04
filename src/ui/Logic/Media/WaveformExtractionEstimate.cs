using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Rough, codec-based estimate of how long ffmpeg will take to decode an audio track to PCM for
/// waveform extraction. Used only to show a "~25 sec" / "~5 min" hint in the waveform audio-track
/// picker so the user can prefer a lighter track (e.g. AC3) over a heavy lossless one (e.g. TrueHD).
/// The numbers are order-of-magnitude decode speeds (x realtime) measured on a typical desktop CPU;
/// they are intentionally approximate and never used for anything correctness-sensitive.
/// </summary>
public static class WaveformExtractionEstimate
{
    /// <summary>Approximate ffmpeg decode-to-PCM speed (x realtime) for a codec.</summary>
    public static double GetDecodeSpeedFactor(string? codec)
    {
        var c = (codec ?? string.Empty).ToLowerInvariant();

        if (c.Length == 0)
        {
            return 200; // unknown - assume a typical compressed codec
        }

        // Lossless / object-based: slow to decode.
        if (c.Contains("truehd") || c.Contains("mlp"))
        {
            return 30;
        }

        // Already PCM - essentially just a copy/resample.
        if (c.Contains("pcm") || c.Contains("lpcm"))
        {
            return 1000;
        }

        if (c.Contains("dts"))
        {
            return 150; // dts, dts-hd
        }

        if (c.Contains("flac") || c.Contains("alac") || c.Contains("wavpack") || c.Contains("tta"))
        {
            return 200;
        }

        // Common lossy codecs decode very fast.
        if (c.Contains("eac3") || c.Contains("e-ac-3") || c.Contains("ac3") || c.Contains("ac-3") ||
            c.Contains("aac") || c.Contains("opus") || c.Contains("vorbis") ||
            c.Contains("mp3") || c.Contains("mp2") || c.Contains("mpeg"))
        {
            return 350;
        }

        return 200;
    }

    /// <summary>Estimated wall-clock seconds to extract the whole track, or 0 if it can't be estimated.</summary>
    public static double EstimateSeconds(string? codec, double mediaDurationSeconds)
    {
        var speed = GetDecodeSpeedFactor(codec);
        if (speed <= 0 || mediaDurationSeconds <= 0)
        {
            return 0;
        }

        return mediaDurationSeconds / speed;
    }

    /// <summary>
    /// Short human hint like "~25 sec" or "~5 min". Rounds up so it never promises faster than reality.
    /// Returns an empty string when no estimate is available.
    /// </summary>
    public static string Format(double estimateSeconds)
    {
        if (estimateSeconds <= 0)
        {
            return string.Empty;
        }

        if (estimateSeconds < 60)
        {
            var seconds = Math.Max(1, (int)Math.Ceiling(estimateSeconds));
            return $"~{seconds.ToString(CultureInfo.InvariantCulture)} sec";
        }

        var minutes = (int)Math.Ceiling(estimateSeconds / 60.0);
        return $"~{minutes.ToString(CultureInfo.InvariantCulture)} min";
    }

    /// <summary>A friendly display name for a codec (e.g. "truehd" -> "TrueHD"), for the picker label.</summary>
    public static string GetCodecDisplayName(string? codec)
    {
        var c = (codec ?? string.Empty).ToLowerInvariant();

        if (c.Length == 0)
        {
            return string.Empty;
        }

        if (c.Contains("truehd"))
        {
            return "TrueHD";
        }

        if (c.Contains("eac3") || c.Contains("e-ac-3"))
        {
            return "E-AC3";
        }

        if (c.Contains("ac3") || c.Contains("ac-3"))
        {
            return "AC3";
        }

        if (c.Contains("dts"))
        {
            return "DTS";
        }

        if (c.Contains("aac"))
        {
            return "AAC";
        }

        if (c.Contains("flac"))
        {
            return "FLAC";
        }

        if (c.Contains("alac"))
        {
            return "ALAC";
        }

        if (c.Contains("opus"))
        {
            return "Opus";
        }

        if (c.Contains("vorbis"))
        {
            return "Vorbis";
        }

        if (c.Contains("pcm") || c.Contains("lpcm"))
        {
            return "PCM";
        }

        if (c.Contains("mp3"))
        {
            return "MP3";
        }

        return codec!.ToUpperInvariant();
    }

    /// <summary>A friendly channel-layout label (8 -> "7.1", 6 -> "5.1", 2 -> "Stereo", 1 -> "Mono").</summary>
    public static string GetChannelLayoutLabel(int? channels)
    {
        return channels switch
        {
            1 => "Mono",
            2 => "Stereo",
            6 => "5.1",
            7 => "6.1",
            8 => "7.1",
            null or <= 0 => string.Empty,
            _ => $"{channels.Value.ToString(CultureInfo.InvariantCulture)}ch",
        };
    }
}
