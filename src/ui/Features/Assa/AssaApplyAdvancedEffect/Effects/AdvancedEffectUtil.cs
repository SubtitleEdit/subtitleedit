using Avalonia.Media;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Shared helpers for the advanced effect generators.
/// </summary>
internal static class AdvancedEffectUtil
{
    /// <summary>
    /// Safety cap for particle generators whose event count scales with the covered time
    /// span (rain, snow, grain, ...). Keeps the 750 ms preview rebuild and the temp-file
    /// serialization bounded when the effect is applied across a long subtitle file.
    /// </summary>
    public const int MaxGeneratedEvents = 20_000;

    /// <summary>
    /// Formats a fractional override tag value with an invariant decimal point. Tag
    /// arguments must never use the current culture: a decimal comma both corrupts the
    /// number and acts as an argument separator inside \t, \fad, \move, ...
    /// </summary>
    public static string Tag(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    /// <inheritdoc cref="Tag(double)"/>
    public static string Tag(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>
    /// Clones a line that an effect passes through unchanged, so the returned list never
    /// aliases the dialog's working set.
    /// </summary>
    public static SubtitleLineViewModel PassThrough(SubtitleLineViewModel sub) => new(sub, generateNewId: true);

    private static readonly Regex PositionTagRegex = new(@"\\(?:pos|move)\([^)]*\)|\\an\d|\\a\d+", RegexOptions.Compiled);

    /// <summary>
    /// Strips positioning override tags (\pos, \move, \an, \a) from a line's text so an
    /// effect can apply its own positioning without emitting conflicting tags.
    /// </summary>
    public static string RemovePositionTags(string text)
    {
        if (string.IsNullOrEmpty(text) || !text.Contains('\\'))
        {
            return text;
        }

        return PositionTagRegex.Replace(text, string.Empty).Replace("{}", string.Empty);
    }

    /// <summary>
    /// Formats an Avalonia color as an ASS primary/secondary color value (BGR order).
    /// </summary>
    public static string ToAssColor(Color color) => $"&H{color.B:X2}{color.G:X2}{color.R:X2}&";

    /// <summary>
    /// Resolves the coordinate space for positioning tags. \pos, \move and \clip use
    /// SCRIPT coordinates (the header's PlayResX/PlayResY), not video pixels - with e.g.
    /// the 384x288 default header, video-pixel geometry lands far outside the visible
    /// area. Falls back to the video dimensions, then 1280x720, when the header has no
    /// resolution.
    /// </summary>
    public static (int Width, int Height) GetScriptResolution(string header, int videoWidth, int videoHeight)
    {
        int w = 0, h = 0;
        if (!string.IsNullOrEmpty(header))
        {
            w = ParseHeaderInt(AdvancedSubStationAlpha.GetTagFromHeader("PlayResX", "[Script Info]", header));
            h = ParseHeaderInt(AdvancedSubStationAlpha.GetTagFromHeader("PlayResY", "[Script Info]", header));
        }

        if (w <= 0)
        {
            w = videoWidth > 0 ? videoWidth : 1280;
        }
        if (h <= 0)
        {
            h = videoHeight > 0 ? videoHeight : 720;
        }
        return (w, h);
    }

    private static int ParseHeaderInt(string? tagLine)
    {
        var idx = tagLine?.IndexOf(':') ?? -1;
        return idx >= 0 && int.TryParse(tagLine![(idx + 1)..].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    private static readonly Regex FirstTagBlockRegex = new(@"^\{([^}]*)\}", RegexOptions.Compiled);
    private static readonly Regex AlignTagRegex = new(@"\\an\d", RegexOptions.Compiled);
    private static readonly Regex PosTagRegex = new(@"\\pos\([^)]+\)", RegexOptions.Compiled);
    private static readonly Regex MoveTagPatternRegex = new(@"\\move\([^)]+\)", RegexOptions.Compiled);
    private static readonly Regex MoveTagArgsRegex = new(@"\\move\(([^)]*)\)", RegexOptions.Compiled);

    /// <summary>
    /// Extracts the positioning tags (\an, \pos, \move) from a line's first override block
    /// as one "{...}" block, or an empty string when there are none.
    /// </summary>
    public static string ExtractPositionalTags(string text)
    {
        var firstBlock = FirstTagBlockRegex.Match(text);
        if (!firstBlock.Success)
        {
            return string.Empty;
        }
        string inner = firstBlock.Groups[1].Value;
        var sb = new StringBuilder("{");
        var anM = AlignTagRegex.Match(inner);
        if (anM.Success)
        {
            sb.Append(anM.Value);
        }
        var posM = PosTagRegex.Match(inner);
        if (posM.Success)
        {
            sb.Append(posM.Value);
        }
        var moveM = MoveTagPatternRegex.Match(inner);
        if (moveM.Success)
        {
            sb.Append(moveM.Value);
        }
        sb.Append("}");
        return sb.Length > 2 ? sb.ToString() : string.Empty;
    }

    /// <summary>
    /// Rewrites a \move inside <paramref name="posTags"/> for a sub-segment of the original
    /// line, so that a line split into sequential word-events keeps one continuous motion.
    /// The original motion runs from t1 to t2 (relative to the original line start, or the
    /// whole line for the 4-argument form); each segment gets the interpolated start/end
    /// coordinates and clamped times for its own time slice.
    /// </summary>
    public static string AdjustMoveForSegment(string posTags, double segmentOffsetMs, double segmentDurationMs, double lineDurationMs)
    {
        var match = MoveTagArgsRegex.Match(posTags);
        if (!match.Success)
        {
            return posTags;
        }

        var args = match.Groups[1].Value.Split(',');
        if (args.Length != 4 && args.Length != 6)
        {
            return posTags;
        }

        var v = new double[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            if (!double.TryParse(args[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v[i]))
            {
                return posTags;
            }
        }

        double x1 = v[0], y1 = v[1], x2 = v[2], y2 = v[3];
        var t1 = args.Length == 6 ? v[4] : 0;
        var t2 = args.Length == 6 ? v[5] : lineDurationMs;
        if (t2 <= t1)
        {
            t1 = 0;
            t2 = lineDurationMs;
        }

        double Lerp(double a, double b, double t) =>
            t <= t1 ? a : t >= t2 ? b : a + (b - a) * (t - t1) / (t2 - t1);

        var s1 = Math.Clamp(t1 - segmentOffsetMs, 0, segmentDurationMs);
        var s2 = Math.Clamp(t2 - segmentOffsetMs, s1, segmentDurationMs);
        var nx1 = (int)Math.Round(Lerp(x1, x2, segmentOffsetMs + s1));
        var ny1 = (int)Math.Round(Lerp(y1, y2, segmentOffsetMs + s1));
        var nx2 = (int)Math.Round(Lerp(x1, x2, segmentOffsetMs + s2));
        var ny2 = (int)Math.Round(Lerp(y1, y2, segmentOffsetMs + s2));

        var replacement = s2 <= s1 || (nx1 == nx2 && ny1 == ny2)
            ? $"\\pos({nx2},{ny2})"
            : $"\\move({nx1},{ny1},{nx2},{ny2},{(int)Math.Round(s1)},{(int)Math.Round(s2)})";

        return posTags.Replace(match.Value, replacement);
    }
}
