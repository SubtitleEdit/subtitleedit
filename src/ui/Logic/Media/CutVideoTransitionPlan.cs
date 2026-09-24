using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Optional effects for "Cut video": a transition (ffmpeg xfade/acrossfade) at every join of the
/// kept ranges, plus a fade in at the start and a fade out at the end of the output.
/// </summary>
public class CutVideoTransitionOptions
{
    /// <summary>xfade transition name ("fade", "wipeleft", ...); empty for a hard cut.</summary>
    public string Transition { get; set; } = string.Empty;
    public double TransitionSeconds { get; set; }
    public double FadeInSeconds { get; set; }
    public double FadeOutSeconds { get; set; }

    /// <summary>Input frame rate, 0 when unknown. Used to put the joins on whole frames.</summary>
    public double FrameRate { get; set; }

    /// <summary>Input duration, 0 when unknown. Needed to end a range running to the end of the file.</summary>
    public double InputDurationSeconds { get; set; }

    public bool HasTransition => !string.IsNullOrEmpty(Transition) && TransitionSeconds > 0;
    public bool HasEffects => HasTransition || FadeInSeconds > 0 || FadeOutSeconds > 0;

    /// <summary>
    /// True when the cut goes through <see cref="CutVideoTransitionPlan"/>: with effects, and for a
    /// plain cut whenever the frame rate is known. Trimming before making the video constant rate
    /// drops a held picture (a first frame shown for 1.6 s, a still, a screen recording that only
    /// writes changes) and puts the next frame at the start of the part - the picture ran up to
    /// the length of the hold ahead of the sound, then froze until the next part.
    /// </summary>
    public bool UsesPlan => HasEffects || FrameRate > 0;
}

/// <summary>
/// The kept ranges of a cut with transitions, resolved once so the ffmpeg command line and the
/// re-timed subtitle agree: open ends filled in from the input duration, ranges put on whole
/// frames (xfade offsets are computed from these lengths, so they must match what trim actually
/// keeps, or audio and video drift apart at every join), empty ranges dropped, and the
/// transition shortened to fit the shortest range.
/// </summary>
public class CutVideoTransitionPlan
{
    public List<(double Start, double? End)> Ranges { get; }

    /// <summary>xfade transition name, used when <see cref="TransitionSeconds"/> is above zero.</summary>
    public string Transition { get; }
    public double TransitionSeconds { get; }
    public double FadeInSeconds { get; }
    public double FadeOutSeconds { get; }

    /// <summary>ffmpeg frame rate expression for the fps filter, empty when unknown.</summary>
    public string FrameRateExpression { get; }
    public double FrameRate { get; }

    /// <summary>Length of the output, null when a range runs to an unknown end of file.</summary>
    public double? OutputSeconds { get; }

    private CutVideoTransitionPlan(
        List<(double Start, double? End)> ranges,
        string transition,
        double transitionSeconds,
        double fadeInSeconds,
        double fadeOutSeconds,
        string frameRateExpression,
        double frameRate,
        double? outputSeconds)
    {
        Ranges = ranges;
        Transition = transition;
        TransitionSeconds = transitionSeconds;
        FadeInSeconds = fadeInSeconds;
        FadeOutSeconds = fadeOutSeconds;
        FrameRateExpression = frameRateExpression;
        FrameRate = frameRate;
        OutputSeconds = outputSeconds;
    }

    public static CutVideoTransitionPlan Create(IReadOnlyList<(double? Start, double? End)> ranges, CutVideoTransitionOptions options)
    {
        var (frameRateExpression, frameRate) = GetFrameRate(options.FrameRate);
        var duration = options.InputDurationSeconds > 0 ? options.InputDurationSeconds : (double?)null;
        var minimumLength = frameRate > 0 ? 1.0 / frameRate : 0.04;

        var resolved = new List<(double Start, double? End)>();
        foreach (var range in ranges)
        {
            var start = Math.Max(0, range.Start.GetValueOrDefault());
            var end = range.End ?? duration;
            if (duration.HasValue)
            {
                start = Math.Min(start, duration.Value);
                end = Math.Min(end!.Value, duration.Value);
            }

            if (frameRate > 0)
            {
                start = Math.Round(start * frameRate, MidpointRounding.AwayFromZero) / frameRate;
                if (end.HasValue)
                {
                    end = Math.Round(end.Value * frameRate, MidpointRounding.AwayFromZero) / frameRate;
                }
            }

            // A range of nothing (a segment ending at the end of the file leaves an empty
            // remainder) is harmless to concat but stalls xfade/acrossfade waiting for input.
            if (end.HasValue && end.Value - start < minimumLength)
            {
                continue;
            }

            resolved.Add((start, end));
        }

        var transitionSeconds = 0d;
        if (options.HasTransition && resolved.Count > 1)
        {
            // A range is blended into both of its neighbours, so it must hold two transitions.
            var shortest = resolved.Where(r => r.End.HasValue).Select(r => r.End!.Value - r.Start).DefaultIfEmpty(double.MaxValue).Min();
            transitionSeconds = options.TransitionSeconds;
            if (frameRate > 0)
            {
                transitionSeconds = Math.Round(transitionSeconds * frameRate, MidpointRounding.AwayFromZero) / frameRate;
                if (transitionSeconds > shortest / 2.0)
                {
                    transitionSeconds = Math.Floor(shortest / 2.0 * frameRate) / frameRate;
                }
            }
            else
            {
                transitionSeconds = Math.Min(transitionSeconds, shortest / 2.0);
            }

            if (transitionSeconds < minimumLength)
            {
                transitionSeconds = 0;
            }
        }

        double? outputSeconds = null;
        if (resolved.Count > 0 && resolved.All(r => r.End.HasValue))
        {
            outputSeconds = resolved.Sum(r => r.End!.Value - r.Start) - (resolved.Count - 1) * transitionSeconds;
        }

        var fadeInSeconds = Math.Max(0, options.FadeInSeconds);
        var fadeOutSeconds = outputSeconds.HasValue ? Math.Max(0, options.FadeOutSeconds) : 0; // no end to fade out from
        if (outputSeconds.HasValue)
        {
            fadeInSeconds = Math.Min(fadeInSeconds, outputSeconds.Value);
            fadeOutSeconds = Math.Min(fadeOutSeconds, outputSeconds.Value);
        }

        return new CutVideoTransitionPlan(resolved, options.Transition, transitionSeconds, fadeInSeconds, fadeOutSeconds, frameRateExpression, frameRate, outputSeconds);
    }

    /// <summary>
    /// The NTSC rates are given as exact fractions - "fps=29.97" is not 30000/1001, and the joins
    /// would slowly walk off the frames of the input.
    /// </summary>
    private static (string Expression, double Value) GetFrameRate(double frameRate)
    {
        if (frameRate <= 0 || double.IsNaN(frameRate) || double.IsInfinity(frameRate))
        {
            return (string.Empty, 0);
        }

        foreach (var numerator in new[] { 24000, 30000, 48000, 60000, 120000 })
        {
            var ntsc = numerator / 1001.0;
            if (Math.Abs(frameRate - ntsc) < 0.01)
            {
                return ($"{numerator}/1001", ntsc);
            }
        }

        var expression = frameRate.ToString("0.###", CultureInfo.InvariantCulture);
        return (expression, double.Parse(expression, CultureInfo.InvariantCulture));
    }
}
