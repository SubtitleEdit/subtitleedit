using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Nikse.SubtitleEdit.Features.Video.SyncToAudio;

/// <summary>
/// Finds the offset (and a possible frame-rate mismatch) of a whole subtitle against the audio,
/// the way ffsubsync and alass do: the subtitle becomes a signal that is +1 while a line is
/// shown and -1 while none is, the audio becomes a signal that is +1 while someone speaks and -1
/// while nobody does, and the shift that makes the two agree best is the offset. The agreement
/// over every shift at once is a cross-correlation, computed with an FFT so a three-hour film
/// takes well under a second per candidate.
///
/// A frame-rate mismatch (a subtitle timed for 25 fps shown on 23.976 fps video) makes the
/// offset grow along the film, which smears the correlation peak. So the subtitle is also
/// stretched by every ratio between the standard frame rates, and the ratio whose peak is
/// clearly the sharpest wins.
///
/// The speech signal comes from the waveform peaks SE already has, with an adaptive loudness
/// threshold, not a neural VAD: the correlation only needs the two signals to agree more often
/// than chance, and music or effects that get through only lower the confidence. A subtitle that
/// is out of sync differently in different parts (another cut of the film) has no single answer
/// here; that is what Point sync and Improve time codes are for.
/// </summary>
public static class AudioSubtitleSync
{
    /// <summary>Frames per second of both signals: 10 ms steps, finer than anyone places a cue.</summary>
    public const int Rate = 100;

    public static readonly double[] StandardFrameRates = { 23.976, 24, 25, 29.97, 30 };

    public sealed record Options(double MaxOffsetSeconds = 120, bool TryFrameRates = true, double VideoFrameRate = 0);

    /// <param name="OffsetSeconds">Add this to every time code (after scaling by <paramref name="Ratio"/>).</param>
    /// <param name="Ratio">Multiply every time code by this; 1 when the frame rate matches.</param>
    /// <param name="FromFrameRate">The frame rate the subtitle seems to be timed for (0 when the ratio is 1).</param>
    /// <param name="ToFrameRate">The frame rate it is converted to (0 when the ratio is 1).</param>
    /// <param name="Confidence">
    /// How clearly the best shift stands out from every other shift, 0..1 - see <see cref="Prominence"/>.
    /// Below <see cref="LowConfidence"/> the subtitle probably does not belong to this audio.
    /// </param>
    public sealed record Result(double OffsetSeconds, double Ratio, double FromFrameRate, double ToFrameRate, double Confidence)
    {
        public bool IsFrameRateChanged => Math.Abs(Ratio - 1) > 1e-9;
    }

    /// <summary>One frame-rate ratio tried by <see cref="Evaluate"/>.</summary>
    internal sealed record Candidate(double Ratio, double FromFrameRate, double ToFrameRate, int Lag, double Prominence, double Agreement);

    /// <summary>Below this the dialog warns that the result is doubtful.</summary>
    public const double LowConfidence = 0.25;

    /// <summary>
    /// Prominence of the peak (how many standard deviations above the other shifts) that counts
    /// as no confidence, and the one that counts as full. On two sitcom episodes a complete
    /// in-sync subtitle scored 13-18, half of it 11, its last third 7-11, and the other episode's
    /// subtitle 3-4.
    /// </summary>
    private const double ProminenceNone = 4;

    private const double ProminenceFull = 16;

    /// <summary>A stretched candidate must beat the unstretched one by this much to be believed.</summary>
    private const double FrameRateMargin = 1.25;

    /// <summary>
    /// The speech/silence signal (+1/-1 per 10 ms) of a waveform: the peaks SE draws, on an
    /// adaptive loudness threshold. Null when the audio is flat (silence, or one long tone).
    /// </summary>
    public static double[]? SpeechFromPeaks(WavePeakData2 peaks)
    {
        var span = peaks.AsSpan();
        var loudness = new int[span.Length];
        for (var i = 0; i < span.Length; i++)
        {
            loudness[i] = span[i].Abs;
        }

        return SpeechFromLoudness(loudness, peaks.SampleRate);
    }

    /// <summary>
    /// <see cref="SpeechFromPeaks"/> on raw loudness values at any rate. The threshold sits
    /// between the quiet floor (10th percentile of the smoothed log loudness) and the loud
    /// ceiling (97th) - 55 % of the way up to switch on, 40 % to switch off, the hysteresis
    /// keeping a word from flickering; blips shorter than 80 ms are dropped and pauses shorter
    /// than 200 ms filled, so the speech blobs look like subtitle lines, which also span the
    /// gaps between words. (On two sitcom episodes 55 % gave the best agreement with the
    /// subtitle at both 50 % and 70 % speech share; lower thresholds let the laugh track in.)
    /// </summary>
    public static double[]? SpeechFromLoudness(IReadOnlyList<int> loudness, int loudnessRate)
    {
        return SpeechFromLoudness(loudness, loudnessRate, 0.55, 0.40);
    }

    internal static double[]? SpeechFromLoudness(IReadOnlyList<int> loudness, int loudnessRate, double onFraction, double offFraction)
    {
        if (loudness.Count == 0 || loudnessRate <= 0)
        {
            return null;
        }

        var frames = (int)Math.Ceiling((double)loudness.Count * Rate / loudnessRate);
        var energy = new double[frames];
        for (var j = 0; j < frames; j++)
        {
            var from = (int)((long)j * loudnessRate / Rate);
            var to = Math.Min(loudness.Count, (int)Math.Ceiling((double)(j + 1) * loudnessRate / Rate));
            var max = 0;
            for (var i = from; i < to; i++)
            {
                if (loudness[i] > max)
                {
                    max = loudness[i];
                }
            }

            energy[j] = Math.Log(1 + max);
        }

        // 50 ms moving average: one frame of a plosive should not count as speech on its own.
        var smooth = new double[frames];
        const int half = 2;
        for (var j = 0; j < frames; j++)
        {
            var sum = 0.0;
            var count = 0;
            for (var k = Math.Max(0, j - half); k <= Math.Min(frames - 1, j + half); k++)
            {
                sum += energy[k];
                count++;
            }

            smooth[j] = sum / count;
        }

        var sorted = (double[])smooth.Clone();
        Array.Sort(sorted);
        var floor = sorted[(int)(0.10 * (sorted.Length - 1))];
        var ceiling = sorted[(int)(0.97 * (sorted.Length - 1))];
        if (ceiling - floor < 0.5)
        {
            return null;
        }

        var high = floor + onFraction * (ceiling - floor);
        var low = floor + offFraction * (ceiling - floor);
        var speech = new bool[frames];
        var on = false;
        for (var j = 0; j < frames; j++)
        {
            on = on ? smooth[j] >= low : smooth[j] >= high;
            speech[j] = on;
        }

        FillRuns(speech, false, 20); // pauses under 200 ms are inside a sentence
        FillRuns(speech, true, 8); // noises under 80 ms are not words

        var signal = new double[frames];
        for (var j = 0; j < frames; j++)
        {
            signal[j] = speech[j] ? 1 : -1;
        }

        return signal;
    }

    /// <summary>Flips every run of <paramref name="value"/> shorter than <paramref name="maxLength"/> frames.</summary>
    private static void FillRuns(bool[] signal, bool value, int maxLength)
    {
        var start = -1;
        for (var j = 0; j <= signal.Length; j++)
        {
            var isValue = j < signal.Length && signal[j] == value;
            if (isValue && start < 0)
            {
                start = j;
            }
            else if (!isValue && start >= 0)
            {
                // Runs touching either end are kept: the film's lead-in silence is real.
                if (j - start < maxLength && start > 0 && j < signal.Length)
                {
                    for (var k = start; k < j; k++)
                    {
                        signal[k] = !value;
                    }
                }

                start = -1;
            }
        }
    }

    /// <summary>
    /// The shown/not-shown signal (+1/-1 per 10 ms) of a subtitle, with its times multiplied by
    /// <paramref name="ratio"/>. <paramref name="minLength"/> pads the end so a reference and a
    /// subtitle can share a length.
    /// </summary>
    public static double[] SignalFromLines(IReadOnlyList<(double StartSeconds, double EndSeconds)> lines, double ratio = 1, int minLength = 0)
    {
        var end = lines.Count == 0 ? 0 : lines.Max(l => l.EndSeconds) * ratio;
        var frames = Math.Max(minLength, (int)Math.Ceiling(end * Rate) + 1);
        var signal = new double[frames];
        Array.Fill(signal, -1);
        foreach (var line in lines)
        {
            var from = Math.Max(0, (int)Math.Round(line.StartSeconds * ratio * Rate));
            var to = Math.Min(frames, (int)Math.Round(line.EndSeconds * ratio * Rate));
            for (var j = from; j < to; j++)
            {
                signal[j] = 1;
            }
        }

        return signal;
    }

    /// <summary>
    /// The offset and frame-rate ratio that fit <paramref name="lines"/> to <paramref name="reference"/>
    /// best (a speech signal from <see cref="SpeechFromPeaks"/>, or another subtitle's signal from
    /// <see cref="SignalFromLines"/>). Null when there is nothing to match.
    /// </summary>
    public static Result? Find(double[] reference, IReadOnlyList<(double StartSeconds, double EndSeconds)> lines, Options options,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var candidates = Evaluate(reference, lines, options, progress, cancellationToken);
        if (candidates.Count == 0)
        {
            return null;
        }

        // A stretched match wins only when its peak is clearly sharper: on a short subtitle a
        // wrong ratio can edge ahead by luck, and a false frame-rate change is worse than none.
        var unstretched = candidates[0];
        var best = candidates.OrderByDescending(c => c.Prominence).First();
        if (best != unstretched && best.Prominence < unstretched.Prominence * FrameRateMargin)
        {
            best = unstretched;
        }

        var confidence = Math.Clamp((best.Prominence - ProminenceNone) / (ProminenceFull - ProminenceNone), 0, 1);
        return new Result(best.Lag / (double)Rate, best.Ratio, best.FromFrameRate, best.ToFrameRate, confidence);
    }

    /// <summary>
    /// Every frame-rate ratio tried, the unstretched one first, each with its best shift.
    /// </summary>
    internal static List<Candidate> Evaluate(double[] reference, IReadOnlyList<(double StartSeconds, double EndSeconds)> lines, Options options,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var candidates = new List<Candidate>();
        if (reference.Length == 0 || lines.Count == 0 || lines.All(l => l.EndSeconds <= l.StartSeconds))
        {
            return candidates;
        }

        var ratios = new List<(double Ratio, double From, double To)> { (1, 0, 0) };
        if (options.TryFrameRates)
        {
            foreach (var from in StandardFrameRates)
            {
                foreach (var to in StandardFrameRates)
                {
                    var ratio = from / to;
                    if (Math.Abs(ratio - 1) < 1e-6 || ratios.Any(c => Math.Abs(c.Ratio - ratio) < 1e-4))
                    {
                        continue;
                    }

                    ratios.Add((ratio, from, to));
                }
            }

            // Two pairs can share a ratio (24→23.976 and 30→29.97): name the one that converts
            // to the video's own frame rate when that is known.
            if (options.VideoFrameRate > 0)
            {
                for (var i = 0; i < ratios.Count; i++)
                {
                    var ratio = ratios[i].Ratio;
                    var named = ratios[i];
                    foreach (var from in StandardFrameRates)
                    {
                        foreach (var to in StandardFrameRates)
                        {
                            if (Math.Abs(from / to - ratio) < 1e-4 && Math.Abs(to - options.VideoFrameRate) < Math.Abs(named.To - options.VideoFrameRate))
                            {
                                named = (ratio, from, to);
                            }
                        }
                    }

                    ratios[i] = named;
                }
            }
        }

        var maxLag = (int)Math.Max(1, Math.Round(options.MaxOffsetSeconds * Rate));
        var longest = (int)Math.Ceiling(lines.Max(l => l.EndSeconds) * ratios.Max(c => c.Ratio) * Rate) + 1;
        var n = NextPowerOfTwo(reference.Length + longest);
        var fft = new RealFFT(n);
        var referenceSpectrum = new double[n];
        Array.Copy(reference, referenceSpectrum, reference.Length);
        fft.ComputeForward(referenceSpectrum);

        var subtitleSpectrum = new double[n];
        var buffer = new double[n];
        for (var c = 0; c < ratios.Count; c++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((double)c / ratios.Count);

            var (ratio, from, to) = ratios[c];
            var subtitle = SignalFromLines(lines, ratio);
            Array.Clear(subtitleSpectrum);
            Array.Copy(subtitle, subtitleSpectrum, subtitle.Length);
            fft.ComputeForward(subtitleSpectrum);

            // reference * conj(subtitle), back to the time domain: buffer[k] is the sum of
            // subtitle[i] * reference[i + k], the agreement with the subtitle moved k frames
            // later; negative moves wrap round to the end.
            Array.Copy(referenceSpectrum, buffer, n);
            MultiplyByConjugate(buffer, subtitleSpectrum);
            fft.ComputeReverse(buffer);

            var (lag, prominence) = Prominence(buffer, maxLag);
            candidates.Add(new Candidate(ratio, from, to, lag, prominence, Agreement(reference, subtitle, lag)));
        }

        progress?.Report(1);
        return candidates;
    }

    /// <summary>
    /// Width of the moving baseline the correlation is measured against, in frames: 20 s, far
    /// wider than a peak (a few seconds, as lines are) and far narrower than the swell of a
    /// talky scene against a quiet one.
    /// </summary>
    private const int BaselineFrames = 20 * Rate;

    /// <summary>
    /// The best shift within ±<paramref name="maxLag"/>, and how far its correlation stands above
    /// the rest: (peak - baseline) / standard deviation over the other shifts, leaving out the
    /// ones within 3 s of the peak. The baseline is a moving average, because the raw correlation
    /// swells and sinks with how much is said in each part of the film, and against that swell
    /// even a perfect match is only a few deviations up; against the local baseline it is a
    /// needle. Agreement beyond chance cannot tell a good match from a lucky one either - a
    /// short or sparse subtitle agrees little with anything - but a needle above a flat floor can.
    /// </summary>
    private static (int Lag, double Prominence) Prominence(double[] correlation, int maxLag)
    {
        var n = correlation.Length;
        var length = 2 * maxLag + 1;
        var half = Math.Max(1, Math.Min(BaselineFrames, length / 4) / 2);

        // The correlation exists beyond ±maxLag too (it is zero past the signals' overlap), so
        // the baseline window is full-width right up to the edges - a shrinking window there
        // turned the ends of the range into false peaks.
        var extended = length + 2 * half;
        var prefix = new double[extended + 1];
        for (var i = 0; i < extended; i++)
        {
            var lag = i - half - maxLag;
            var index = lag >= 0 ? lag : n + lag;
            prefix[i + 1] = prefix[i] + (index >= 0 && index < n ? correlation[index] : 0);
        }

        var detrended = new double[length];
        for (var i = 0; i < length; i++)
        {
            var lag = i - maxLag;
            var index = lag >= 0 ? lag : n + lag;
            var value = index >= 0 && index < n ? correlation[index] : 0;
            detrended[i] = value - (prefix[i + 2 * half + 1] - prefix[i]) / (2 * half + 1);
        }

        var bestIndex = 0;
        for (var i = 1; i < length; i++)
        {
            if (detrended[i] > detrended[bestIndex])
            {
                bestIndex = i;
            }
        }

        var exclude = Math.Min(3 * Rate, maxLag / 4);
        var count = 0L;
        var sum = 0.0;
        var sumSquares = 0.0;
        for (var i = 0; i < length; i++)
        {
            if (Math.Abs(i - bestIndex) <= exclude)
            {
                continue;
            }

            count++;
            sum += detrended[i];
            sumSquares += detrended[i] * detrended[i];
        }

        var bestLag = bestIndex - maxLag;
        if (count < 2)
        {
            return (bestLag, 0);
        }

        var mean = sum / count;
        var deviation = Math.Sqrt(Math.Max(0, sumSquares / count - mean * mean));
        return (bestLag, deviation < 1e-9 ? 0 : (detrended[bestIndex] - mean) / deviation);
    }

    /// <summary>
    /// Agreement beyond chance (Cohen's kappa) of the two signals with the subtitle moved
    /// <paramref name="lag"/> frames, over the part where both exist; 0 when no better than chance.
    /// </summary>
    internal static double Agreement(double[] reference, double[] subtitle, int lag)
    {
        var agree = 0L;
        var referenceOn = 0L;
        var subtitleOn = 0L;
        var count = 0L;
        var from = Math.Max(0, -lag);
        var to = Math.Min(subtitle.Length, reference.Length - lag);
        for (var i = from; i < to; i++)
        {
            var r = reference[i + lag] > 0;
            var s = subtitle[i] > 0;
            count++;
            if (r == s)
            {
                agree++;
            }

            if (r)
            {
                referenceOn++;
            }

            if (s)
            {
                subtitleOn++;
            }
        }

        if (count == 0)
        {
            return 0;
        }

        var pr = referenceOn / (double)count;
        var ps = subtitleOn / (double)count;
        var chance = pr * ps + (1 - pr) * (1 - ps);
        var observed = agree / (double)count;
        if (chance >= 1 - 1e-9)
        {
            return 0;
        }

        return Math.Max(0, (observed - chance) / (1 - chance));
    }

    /// <summary>
    /// <paramref name="a"/> *= conj(<paramref name="b"/>) in the packed layout of <see cref="RealFFT"/>:
    /// [Re 0, Re N/2, Re 1, Im 1, Re 2, Im 2, ...].
    /// </summary>
    private static void MultiplyByConjugate(double[] a, double[] b)
    {
        a[0] *= b[0];
        a[1] *= b[1];
        for (var k = 2; k < a.Length; k += 2)
        {
            var re = a[k] * b[k] + a[k + 1] * b[k + 1];
            var im = a[k + 1] * b[k] - a[k] * b[k + 1];
            a[k] = re;
            a[k + 1] = im;
        }
    }

    private static int NextPowerOfTwo(int value)
    {
        var n = 2;
        while (n < value)
        {
            n <<= 1;
        }

        return n;
    }
}
