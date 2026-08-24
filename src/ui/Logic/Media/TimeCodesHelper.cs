using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Disk cache for a video's exact frame time codes (presentation time stamps, in seconds).
///
/// "Beautify time codes" snaps cues to real frame boundaries. With no time codes it has to assume
/// a perfect n/fps grid, which is only true for genuinely constant-frame-rate material - variable
/// frame rate, remux artifacts and time stamp discontinuities all put the real frames somewhere
/// else. <see cref="TimeCodesGenerator"/> extracts the real ones; this stores them so the (full
/// video decode) extraction only has to happen once per file.
/// </summary>
public static class TimeCodesHelper
{
    private const string Extension = ".timecodes";

    /// <summary>
    /// Load cached time codes for a video file. Returns an empty list when nothing is cached, so
    /// callers can pass the result straight to the beautifier (which then falls back to n/fps).
    /// </summary>
    public static List<double> FromDisk(string? videoFileName)
    {
        if (string.IsNullOrEmpty(videoFileName))
        {
            return new List<double>();
        }

        var fileName = FindTimeCodesFileName(videoFileName);
        if (string.IsNullOrEmpty(fileName))
        {
            return new List<double>();
        }

        var list = new List<double>();
        foreach (var line in File.ReadLines(fileName))
        {
            if (!string.IsNullOrWhiteSpace(line) &&
                double.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
            {
                list.Add(seconds);
            }
        }

        // A cache file can be truncated by a crash or a full disk, so it gets the same treatment
        // as freshly extracted values rather than being trusted on sight.
        return Normalize(list);
    }

    /// <summary>Saves time codes (in seconds) for a video file.</summary>
    public static void Save(string videoFileName, IReadOnlyList<double> timeCodes)
    {
        var sb = new StringBuilder();
        foreach (var seconds in timeCodes)
        {
            sb.AppendLine(seconds.ToString("0.######", CultureInfo.InvariantCulture));
        }

        File.WriteAllText(GetTimeCodesFileName(videoFileName), sb.ToString().TrimEnd());
    }

    /// <summary>Deletes the cached time codes for a video file, if any.</summary>
    public static void Delete(string videoFileName)
    {
        var fileName = FindTimeCodesFileName(videoFileName);
        if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
        {
            File.Delete(fileName);
        }
    }

    /// <summary>
    /// Makes a raw list safe for <see cref="Core.Forms.TimeCodesBeautifier"/>, which both binary
    /// searches the list (<c>ClosestIndexTo</c>) and indexes it by frame number. Both require
    /// strictly increasing values, and neither validates - a list that is merely out of order
    /// yields silently wrong frame numbers instead of an error. Demuxed/packet-ordered sources are
    /// exactly that, so sort, and drop anything that cannot be a frame time (non-finite, negative,
    /// or a duplicate of the previous frame).
    /// </summary>
    public static List<double> Normalize(IEnumerable<double> timeCodes)
    {
        var sorted = new List<double>();
        foreach (var seconds in timeCodes)
        {
            if (double.IsFinite(seconds) && seconds >= 0)
            {
                sorted.Add(seconds);
            }
        }

        sorted.Sort();

        var result = new List<double>(sorted.Count);
        foreach (var seconds in sorted)
        {
            if (result.Count == 0 || seconds > result[result.Count - 1])
            {
                result.Add(seconds);
            }
        }

        return result;
    }

    /// <summary>
    /// True when a list looks like a complete frame list for a video of the given length, i.e. it
    /// covers most of the running time. A short list is worse than none at all: the beautifier
    /// treats index as frame number, so cues past the end of a partial list snap to the wrong
    /// frames instead of degrading to n/fps.
    /// </summary>
    public static bool IsUsableFor(IReadOnlyList<double> timeCodes, double durationSeconds)
    {
        if (timeCodes.Count < 2)
        {
            return false;
        }

        if (durationSeconds <= 0)
        {
            // Unknown length - a partial list cannot be told apart from a complete one, and
            // accepting a partial one snaps every cue to its few entries. Callers that know
            // the extraction ran to completion can pass the last frame time as the duration.
            return false;
        }

        var covered = timeCodes[timeCodes.Count - 1];
        return covered >= durationSeconds * 0.9;
    }

    private static string GetTimeCodesFileName(string videoFileName)
    {
        var dir = Se.TimeCodesFolder;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        return Path.Combine(dir, MakeBaseName(videoFileName) + Extension);
    }

    private static string FindTimeCodesFileName(string videoFileName)
    {
        var dir = Se.TimeCodesFolder;
        if (!Directory.Exists(dir))
        {
            return string.Empty;
        }

        var fileName = Path.Combine(dir, MakeBaseName(videoFileName) + Extension);
        if (File.Exists(fileName))
        {
            return fileName;
        }

        // The readable suffix is cosmetic, so a file cached under an older/renamed video name is
        // still a hit - the hash is what identifies the video.
        var files = Directory.GetFiles(dir, MovieHasher.GenerateHash(videoFileName) + "*" + Extension);
        return files.Length > 0 ? files[0] : string.Empty;
    }

    /// <summary>
    /// Hash (identity) plus a short readable part, so the cache folder can be made sense of by a
    /// human without opening every file.
    /// </summary>
    private static string MakeBaseName(string videoFileName)
    {
        var readable = Path.GetFileNameWithoutExtension(videoFileName)
            .Replace(".", string.Empty)
            .Replace("_", string.Empty);
        if (readable.Length > 25)
        {
            readable = readable.Substring(0, 25);
        }

        return $"{MovieHasher.GenerateHash(videoFileName)}_{readable}";
    }
}
