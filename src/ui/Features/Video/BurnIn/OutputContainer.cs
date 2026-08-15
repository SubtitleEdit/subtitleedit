using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

/// <summary>
/// Which output containers a burn-in video codec can be muxed into, and which audio encoders
/// each container accepts.
/// </summary>
/// <remarks>
/// This is an allow-list, not a deny-list, because a mismatch is not always a loud failure:
/// ffmpeg refuses e.g. ProRes in .mp4 or AAC in .webm with an error, but happily writes VP9 or
/// ProRes video (and Vorbis audio) into MPEG-TS with exit code 0 - as an unplayable "bin_data"
/// stream. Only pairs verified to produce a playable file are offered.
/// </remarks>
public static class OutputContainer
{
    public const string DefaultExtension = ".mkv";

    public const string AudioEncodingCopy = "copy";

    private static readonly List<string> Vp9Extensions = new() { ".mkv", ".webm", ".mp4" };
    private static readonly List<string> ProResExtensions = new() { ".mov", ".mkv" };
    private static readonly List<string> H264H265Extensions = new() { ".mkv", ".mp4", ".mov", ".ts" };

    /// <summary>
    /// Output file extensions that can hold video encoded with <paramref name="videoCodec"/>.
    /// The first entry is the default. ".mkv" is always present, so it stays a safe fallback.
    /// </summary>
    public static List<string> GetExtensions(string videoCodec)
    {
        if (string.IsNullOrEmpty(videoCodec))
        {
            return new List<string>(H264H265Extensions);
        }

        if (videoCodec.Contains("vp9", StringComparison.OrdinalIgnoreCase) ||
            videoCodec.Contains("vp8", StringComparison.OrdinalIgnoreCase) ||
            videoCodec.Contains("av1", StringComparison.OrdinalIgnoreCase))
        {
            return new List<string>(Vp9Extensions);
        }

        if (videoCodec.Contains("prores", StringComparison.OrdinalIgnoreCase))
        {
            return new List<string>(ProResExtensions);
        }

        return new List<string>(H264H265Extensions);
    }

    /// <summary>
    /// Audio encoders that <paramref name="extension"/> can carry. "copy" is offered wherever the
    /// container can hold the common source codecs; WebM takes only Opus/Vorbis, so copying an
    /// AAC track there would always fail and the choice is left out.
    /// </summary>
    public static List<string> GetAudioEncodings(string extension)
    {
        switch (extension?.ToLowerInvariant())
        {
            case ".webm":
                return new List<string> { "libopus", "libvorbis" };
            case ".ts":
                // Vorbis in MPEG-TS mixes to an unreadable stream instead of failing.
                return new List<string> { AudioEncodingCopy, "aac", "ac3", "mp3", "libopus" };
            case ".mov":
                // The mov muxer has no tag for Opus.
                return new List<string> { AudioEncodingCopy, "aac", "ac3", "mp3", "libvorbis" };
            default:
                return new List<string> { AudioEncodingCopy, "aac", "ac3", "mp3", "libopus", "libvorbis" };
        }
    }

    /// <summary>
    /// The audio encoder to actually use for an output file: the wanted one when the container can
    /// carry it, otherwise the container's default. The extension combo box is kept in sync with
    /// the container, but the "save as" dialog lets the user pick another one at generate time.
    /// </summary>
    public static string GetAudioEncodingFor(string extension, string audioEncoding)
    {
        var items = GetAudioEncodings(extension);
        var wanted = MigrateAudioEncoding(audioEncoding);

        return !string.IsNullOrEmpty(wanted) && items.Contains(wanted) ? wanted : items[0];
    }

    /// <summary>
    /// ffmpeg muxer name for an output extension - needed for "-f" (the two-pass first pass writes
    /// to the null device, where ffmpeg cannot infer the format from the file name).
    /// </summary>
    public static string GetMuxerName(string extension)
    {
        var ext = (extension ?? string.Empty).TrimStart('.').ToLowerInvariant();

        return ext switch
        {
            "mkv" or "mka" or "mks" => "matroska",
            "ts" or "m2ts" or "mts" => "mpegts",
            "" => "matroska",
            _ => ext,
        };
    }

    /// <summary>
    /// True for the MP4/QuickTime family, the only containers where "-movflags" and "-use_editlist"
    /// mean anything.
    /// </summary>
    public static bool IsMp4Family(string extension)
    {
        var ext = (extension ?? string.Empty).TrimStart('.').ToLowerInvariant();

        return ext is "mp4" or "m4v" or "mov";
    }

    /// <summary>
    /// Maps a stored audio encoding name to one that works. "opus" and "vorbis" select ffmpeg's
    /// native encoders, which are experimental and abort the encode ("add '-strict -2'"), so old
    /// settings are moved to the libopus/libvorbis wrappers.
    /// </summary>
    public static string MigrateAudioEncoding(string audioEncoding)
    {
        return audioEncoding switch
        {
            "opus" => "libopus",
            "vorbis" => "libvorbis",
            _ => audioEncoding,
        };
    }
}
