using Nikse.SubtitleEdit.Core.BluRaySup;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// One point of an ASSA fade curve: how opaque the subtitle is
/// <paramref name="OffsetMs"/> milliseconds after it appears. The alpha between two
/// keyframes is linear, as in ASSA.
/// </summary>
public record ExportFadeKeyframe(long OffsetMs, int AlphaPercent);

/// <summary>
/// Reads the ASSA fade tags - "{\fad(200,300)}" and the seven argument
/// "{\fade(a1,a2,a3,t1,t2,t3,t4)}" - and turns them into the alpha steps the Blu-ray sup
/// writer sends as palette update display sets.
/// <para>
/// Only image formats that can animate a palette use these; the other export handlers ignore
/// them and keep drawing the subtitle fully opaque, exactly as before.
/// </para>
/// </summary>
public static class ExportFade
{
    /// <summary>
    /// Ceiling for the number of palette update display sets one subtitle may cost. A step is
    /// a full palette (~1.3 KB), so a long fade sampled per frame would add hundreds of
    /// kilobytes to the file for alpha differences no one can see; beyond this the fade is
    /// sampled coarser instead.
    /// </summary>
    public const int MaxSteps = 60;

    // "{\fad(200,300)}", also inside a multi tag block ("{\an8\fad(200,300)}") and with the
    // closing parenthesis missing, which is what SE's own effects write ("{\fad(300,300}").
    private static readonly Regex FadTagRegex = new(
        @"\\fad\(\s*(\d+)\s*,\s*(\d+)\s*\)?",
        RegexOptions.Compiled);

    // "{\fade(255,0,255,0,500,2000,2200)}"
    private static readonly Regex FadeTagRegex = new(
        @"\\fade\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*(-?\d+)\s*,\s*(-?\d+)\s*,\s*(-?\d+)\s*,\s*(-?\d+)\s*\)?",
        RegexOptions.Compiled);

    /// <summary>
    /// The fade curve of the text, or null when it has no fade tag. Times are clamped to the
    /// duration, so a "{\fad(1000,1000)}" on a 500 ms line fades in and out inside those 500 ms
    /// instead of never becoming visible.
    /// </summary>
    public static List<ExportFadeKeyframe>? Parse(string? text, long durationMs)
    {
        if (string.IsNullOrEmpty(text) || durationMs <= 0 || !text.Contains("\\fad", StringComparison.Ordinal))
        {
            return null;
        }

        // "\fade" with seven arguments is the general form and wins - "\fad" only matches its
        // first four letters, so a line carrying both would otherwise be read as "\fad".
        var fade = FadeTagRegex.Match(text);
        if (fade.Success)
        {
            return FromFade(
                ToPercent(fade.Groups[1].Value),
                ToPercent(fade.Groups[2].Value),
                ToPercent(fade.Groups[3].Value),
                ToMs(fade.Groups[4].Value),
                ToMs(fade.Groups[5].Value),
                ToMs(fade.Groups[6].Value),
                ToMs(fade.Groups[7].Value),
                durationMs);
        }

        var fad = FadTagRegex.Match(text);
        if (fad.Success)
        {
            return FromFad(ToMs(fad.Groups[1].Value), ToMs(fad.Groups[2].Value), durationMs);
        }

        return null;
    }

    /// <summary>
    /// Samples the curve into the alpha steps of a Blu-ray epoch: the first step is the alpha
    /// the subtitle appears with, the rest become palette update display sets. Sampling starts
    /// at one step per video frame - a decoder cannot show more than that - and gets coarser
    /// when a long fade would need more than <see cref="MaxSteps"/> of them.
    /// </summary>
    public static List<BluRaySupFadeStep> CreateSteps(IReadOnlyList<ExportFadeKeyframe>? keyframes, long startMs, long endMs, double fps)
    {
        var steps = new List<BluRaySupFadeStep>();
        if (keyframes == null || keyframes.Count == 0 || endMs <= startMs)
        {
            return steps;
        }

        var frameMs = fps > 1 ? 1000.0 / fps : 40.0;
        var durationMs = endMs - startMs;
        for (var intervalMs = frameMs; ; intervalMs *= 2)
        {
            steps = Sample(keyframes, startMs, durationMs, intervalMs);
            if (steps.Count <= MaxSteps || intervalMs >= durationMs)
            {
                break;
            }
        }

        // A curve that never leaves 100% (an "{\fad(0,0)}") is not a fade - do not pay a
        // display set for it.
        return steps.Count == 1 && steps[0].AlphaPercent == 100 ? new List<BluRaySupFadeStep>() : steps;
    }

    private static List<BluRaySupFadeStep> Sample(IReadOnlyList<ExportFadeKeyframe> keyframes, long startMs, long durationMs, double intervalMs)
    {
        var steps = new List<BluRaySupFadeStep> { new BluRaySupFadeStep(startMs, AlphaPercentAt(keyframes, 0)) };
        var lastAlpha = steps[0].AlphaPercent;
        for (var offset = intervalMs; offset < durationMs; offset += intervalMs)
        {
            var alpha = AlphaPercentAt(keyframes, (long)Math.Round(offset));
            if (alpha == lastAlpha)
            {
                continue;
            }

            var timeMs = startMs + (long)Math.Round(offset);
            if (timeMs > steps[steps.Count - 1].TimeMs)
            {
                steps.Add(new BluRaySupFadeStep(timeMs, alpha));
                lastAlpha = alpha;
            }
        }

        return steps;
    }

    /// <summary>
    /// The alpha of the curve <paramref name="offsetMs"/> after the subtitle appeared.
    /// </summary>
    public static int AlphaPercentAt(IReadOnlyList<ExportFadeKeyframe> keyframes, long offsetMs)
    {
        if (keyframes.Count == 0)
        {
            return 100;
        }

        if (offsetMs <= keyframes[0].OffsetMs)
        {
            return keyframes[0].AlphaPercent;
        }

        for (var i = 1; i < keyframes.Count; i++)
        {
            var to = keyframes[i];
            if (offsetMs > to.OffsetMs)
            {
                continue;
            }

            var from = keyframes[i - 1];
            var span = to.OffsetMs - from.OffsetMs;
            if (span <= 0)
            {
                return to.AlphaPercent;
            }

            var progress = (offsetMs - from.OffsetMs) / (double)span;
            return (int)Math.Round(from.AlphaPercent + (to.AlphaPercent - from.AlphaPercent) * progress);
        }

        return keyframes[keyframes.Count - 1].AlphaPercent;
    }

    private static List<ExportFadeKeyframe> FromFad(long fadeInMs, long fadeOutMs, long durationMs)
    {
        // The two ramps may not overlap - shrink both to fit rather than letting the subtitle
        // start fading out before it has faded in.
        if (fadeInMs + fadeOutMs > durationMs)
        {
            var scale = durationMs / (double)(fadeInMs + fadeOutMs);
            fadeInMs = (long)(fadeInMs * scale);
            fadeOutMs = (long)(fadeOutMs * scale);
        }

        var keyframes = new List<ExportFadeKeyframe>();
        if (fadeInMs > 0)
        {
            keyframes.Add(new ExportFadeKeyframe(0, 0));
        }

        keyframes.Add(new ExportFadeKeyframe(fadeInMs, 100));
        if (fadeOutMs > 0)
        {
            keyframes.Add(new ExportFadeKeyframe(durationMs - fadeOutMs, 100));
            keyframes.Add(new ExportFadeKeyframe(durationMs, 0));
        }

        return keyframes;
    }

    private static List<ExportFadeKeyframe> FromFade(int alpha1, int alpha2, int alpha3, long t1, long t2, long t3, long t4, long durationMs)
    {
        // Times are offsets into the subtitle and must not run backwards; a tag that overshoots
        // the line is cut at its end, as a player would show it.
        t1 = Math.Min(Math.Max(t1, 0), durationMs);
        t2 = Math.Min(Math.Max(t2, t1), durationMs);
        t3 = Math.Min(Math.Max(t3, t2), durationMs);
        t4 = Math.Min(Math.Max(t4, t3), durationMs);

        return new List<ExportFadeKeyframe>
        {
            new ExportFadeKeyframe(0, alpha1),
            new ExportFadeKeyframe(t1, alpha1),
            new ExportFadeKeyframe(t2, alpha2),
            new ExportFadeKeyframe(t3, alpha2),
            new ExportFadeKeyframe(t4, alpha3),
            new ExportFadeKeyframe(durationMs, alpha3),
        };
    }

    /// <summary>
    /// ASSA counts transparency, not opacity: 0 is opaque and 255 is invisible.
    /// </summary>
    private static int ToPercent(string assaAlpha)
    {
        var value = int.TryParse(assaAlpha, NumberStyles.Integer, CultureInfo.InvariantCulture, out var alpha)
            ? Math.Min(Math.Max(alpha, 0), 255)
            : 0;
        return (int)Math.Round((255 - value) * 100.0 / 255.0);
    }

    private static long ToMs(string value)
    {
        return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ms) ? ms : 0;
    }
}
