using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

/// <summary>
/// Builds the mpv "vf" filter string for the Letterboxing ribbon (#14845) from persisted
/// settings. Pulled out of <see cref="LibMpvDynamicPlayer"/> so the actual filter-string logic
/// (percent clamping, empty-string-when-off, color formatting) can be unit-tested without a real
/// mpv instance - <see cref="LibMpvDynamicPlayer.SetOptionString"/> is a thin P/Invoke wrapper
/// with no test seam of its own.
///
/// Heights are written as ffmpeg expressions ("ih*0.15") rather than pixel values, so this never
/// needs to know the video's actual width/height.
/// </summary>
public static class LetterboxFilterBuilder
{
    /// <returns>
    /// The "vf" value to set: a comma-joined chain of "drawbox" filters, or an empty string when
    /// the ribbon is off (which callers should still write, to clear any previously-set filter).
    /// </returns>
    public static string Build(SeVideoLetterbox settings)
    {
        var topFraction = ClampFraction(settings.TopHeightPercent);
        var bottomFraction = ClampFraction(settings.BottomHeightPercent);

        if (!settings.Enabled || (topFraction <= 0 && bottomFraction <= 0))
        {
            return string.Empty;
        }

        var color = string.IsNullOrWhiteSpace(settings.Color) ? "#000000" : settings.Color;
        var boxes = new List<string>();
        if (topFraction > 0)
        {
            boxes.Add($"drawbox=x=0:y=0:w=iw:h=ih*{topFraction.ToString(CultureInfo.InvariantCulture)}:color={color}:t=fill");
        }

        if (bottomFraction > 0)
        {
            boxes.Add($"drawbox=x=0:y=ih-ih*{bottomFraction.ToString(CultureInfo.InvariantCulture)}:w=iw:h=ih*{bottomFraction.ToString(CultureInfo.InvariantCulture)}:color={color}:t=fill");
        }

        return string.Join(",", boxes);
    }

    /// <summary>
    /// A stored percentage can be stale (from an old settings file, or a future SE version that
    /// allows a wider range) - clamp to a sane 0-50% instead of letting a bad value collapse or
    /// invert the picture. Public: <see cref="Ffmpeg.FfmpegSoftwareControl"/> reuses this for its
    /// own (non-mpv) bar rendering, so both backends agree on what a given percentage means.
    /// </summary>
    public static double ClampFraction(double percent)
    {
        if (double.IsNaN(percent) || percent <= 0)
        {
            return 0;
        }

        return Math.Min(percent, 50) / 100.0;
    }
}
