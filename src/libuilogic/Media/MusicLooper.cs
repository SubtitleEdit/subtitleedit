using Nikse.SubtitleEdit.UiLogic.Common;

namespace Nikse.SubtitleEdit.UiLogic.Media;

/// <summary>Where a generated track loops, and how good the seam is.</summary>
public sealed class MusicLoop
{
    /// <summary>Loop start: playback wraps back to this time.</summary>
    public double StartSeconds { get; init; }

    /// <summary>Loop end: the crossfade into <see cref="StartSeconds"/> finishes here.</summary>
    public double EndSeconds { get; init; }

    public double CrossfadeSeconds { get; init; }

    /// <summary>0..1 rhythm + harmony similarity of the two sides of the seam; 1 is identical.</summary>
    public double SeamScore { get; init; }

    public double MeasuredBpm { get; init; }

    /// <summary>True when the phrase grid was found; false for the plain fallback crossfade.</summary>
    public bool IsBarAligned { get; init; }

    /// <summary>
    /// Equal-gain fade for (near) identical material — an equal-power fade would sum it up to
    /// +3 dB and clip. Different takes of a phrase get equal-power, which keeps the level steady.
    /// </summary>
    public bool EqualGainCrossfade { get; init; }

    public int StartBar { get; init; }
    public int Bars { get; init; }

    public double LengthSeconds => EndSeconds - StartSeconds;
}

/// <summary>
/// Turns a generated music clip (ACE-Step 1.5 through audio.cpp) into a bed of any length:
/// finds a musically aligned loop and renders intro + repeated loop + fade-out.
/// <para>
/// What the audio looks like, measured on ACE-Step output: it ends in a 6–9 s outro fade, so the
/// tail cannot simply wrap to the head. The tempo lands within a few percent of the requested
/// bpm (110 came out as 113), which is enough to put a fixed bar grid a second off after 18
/// bars, so the bar length and beat phase are measured from onsets. Sections are sometimes
/// repeated sample-for-sample and sometimes only musically, so seams are scored on rhythm
/// (onset strength) and harmony (chroma) rather than on the waveform.
/// </para>
/// </summary>
public static class MusicLooper
{
    private const int ChromaFftSize = 4096;
    private const double FramesPerSecond = 100.0;

    public static MusicLoop FindLoop(MusicAudio audio, double requestedBpm)
    {
        var mono = audio.ToMono();
        var sr = audio.SampleRate;
        var duration = audio.DurationSeconds;
        var features = ComputeFeatures(mono, sr);

        if (requestedBpm > 0 && features.FrameCount > 0)
        {
            var result = FindBarAlignedLoop(mono, sr, duration, features, requestedBpm);
            if (result != null)
            {
                return result;
            }
        }

        return MakeFallbackLoop(mono, sr, duration);
    }

    /// <summary>
    /// Renders <paramref name="targetSeconds"/> of music: the source up to the loop start, the loop
    /// repeated, and a fade-out at the end. A target no longer than the source's usable part is a
    /// plain trim with a fade-out. The result is not normalized — see <see cref="Normalize"/>.
    /// </summary>
    public static MusicAudio Render(MusicAudio source, MusicLoop loop, double targetSeconds, double fadeOutSeconds = 3.0)
    {
        var ch = source.Channels;
        var sr = source.SampleRate;
        var targetFrames = (int)Math.Round(Math.Max(0, targetSeconds) * sr);
        var output = new float[targetFrames * ch];

        var p = Math.Clamp((int)Math.Round(loop.StartSeconds * sr), 0, source.FrameCount);
        var e = Math.Clamp((int)Math.Round(loop.EndSeconds * sr), 0, source.FrameCount);
        var xf = Math.Clamp((int)Math.Round(loop.CrossfadeSeconds * sr), 0, Math.Min(p, Math.Max(0, e - p)));

        if (targetFrames <= e || e - p - xf <= 0)
        {
            var take = Math.Min(targetFrames, source.FrameCount);
            Array.Copy(source.Samples, 0, output, 0, take * ch);
        }
        else
        {
            // Loop unit: body [p, e - xf) then the seam, which fades the tail [e - xf, e) out while
            // [p - xf, p) fades in. The seam ends on the frame before p, so the next repetition's
            // body continues sample-continuously — and so does the intro [0, p) into the first body.
            var unitFrames = e - p;
            var unit = new float[unitFrames * ch];
            Array.Copy(source.Samples, p * ch, unit, 0, (e - xf - p) * ch);
            var seamOffset = (e - xf - p) * ch;
            for (var i = 0; i < xf; i++)
            {
                var t = (i + 0.5) / xf;
                double fadeOut, fadeIn;
                if (loop.EqualGainCrossfade)
                {
                    fadeOut = 1 - t;
                    fadeIn = t;
                }
                else
                {
                    fadeOut = Math.Cos(t * Math.PI / 2);
                    fadeIn = Math.Sin(t * Math.PI / 2);
                }

                for (var c = 0; c < ch; c++)
                {
                    unit[seamOffset + i * ch + c] = (float)(source.Samples[(e - xf + i) * ch + c] * fadeOut + source.Samples[(p - xf + i) * ch + c] * fadeIn);
                }
            }

            Array.Copy(source.Samples, 0, output, 0, Math.Min(p, targetFrames) * ch);
            var pos = p;
            while (pos < targetFrames)
            {
                var take = Math.Min(unitFrames, targetFrames - pos);
                Array.Copy(unit, 0, output, pos * ch, take * ch);
                pos += take;
            }
        }

        var fadeFrames = Math.Min(targetFrames, (int)Math.Round(fadeOutSeconds * sr));
        for (var i = 0; i < fadeFrames; i++)
        {
            var gain = (float)((double)(fadeFrames - i) / fadeFrames);
            var baseIndex = (targetFrames - fadeFrames + i) * ch;
            for (var c = 0; c < ch; c++)
            {
                output[baseIndex + c] *= gain;
            }
        }

        return new MusicAudio(sr, ch, output);
    }

    /// <summary>
    /// Scales to an RMS level (ignoring near-silent frames, so a fade-out does not pull it up)
    /// while keeping the peak at or below <paramref name="peakCeilingDb"/>. Presets come out of
    /// the model between -22 and -11 LUFS with peaks up to +2.4 dBFS; this evens them out.
    /// </summary>
    public static MusicAudio Normalize(MusicAudio audio, double targetRmsDb = -20.0, double peakCeilingDb = -1.0)
    {
        var mono = audio.ToMono();
        var block = Math.Max(1, audio.SampleRate / 10);
        double sum = 0;
        long count = 0;
        for (var i = 0; i + block <= mono.Length; i += block)
        {
            double blockSum = 0;
            for (var j = i; j < i + block; j++)
            {
                blockSum += mono[j] * mono[j];
            }

            if (blockSum / block > 1e-6) // -60 dBFS
            {
                sum += blockSum;
                count += block;
            }
        }

        var peak = audio.GetPeak();
        if (count == 0 || peak <= 0)
        {
            return audio;
        }

        var rms = Math.Sqrt(sum / count);
        var gain = Math.Min(Math.Pow(10, targetRmsDb / 20) / rms, Math.Pow(10, peakCeilingDb / 20) / peak);
        return ApplyGain(audio, gain);
    }

    public static MusicAudio ApplyGain(MusicAudio audio, double gain)
    {
        var samples = new float[audio.Samples.Length];
        var g = (float)gain;
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = audio.Samples[i] * g;
        }

        return new MusicAudio(audio.SampleRate, audio.Channels, samples);
    }

    private sealed class Features
    {
        /// <summary>Spectral flux from the 4096-sample window: smooth, good for comparing passages, but early by up to the window length.</summary>
        public required double[] Onset { get; init; }

        /// <summary>Spectral flux from a ~21 ms window: noisier, but on time — used to place the beat grid.</summary>
        public required double[] OnsetShort { get; init; }
        public required double[][] Chroma { get; init; }
        public int FrameCount => Onset.Length;
    }

    /// <summary>
    /// Features every 10 ms, frame f centred on sample f * hop. Log-compressed spectral flux jumps
    /// as soon as an attack enters the window, so flux from the 4096-sample window reports beats
    /// early by most of its length (fine for comparing two passages, which share the bias) and a
    /// short ~21 ms window is used to place the beat grid. Chroma needs the long window.
    /// </summary>
    private static Features ComputeFeatures(float[] mono, int sr)
    {
        var hop = (int)(sr / FramesPerSecond);
        var frameCount = mono.Length >= ChromaFftSize ? mono.Length / hop : 0;
        var onset = new double[frameCount];
        var onsetShort = new double[frameCount];
        var chroma = new double[frameCount][];
        if (frameCount == 0)
        {
            return new Features { Onset = onset, OnsetShort = onsetShort, Chroma = chroma };
        }

        var onsetFftSize = 256;
        while (onsetFftSize < sr * 0.02)
        {
            onsetFftSize *= 2;
        }

        var onsetWindow = MakeHannWindow(onsetFftSize);
        var chromaWindow = MakeHannWindow(ChromaFftSize);

        var chromaBins = ChromaFftSize / 2;
        var pitchClass = new int[chromaBins];
        for (var b = 0; b < chromaBins; b++)
        {
            var freq = (double)b * sr / ChromaFftSize;
            pitchClass[b] = freq is >= 55 and <= 4000
                ? (int)(((Math.Round(12 * Math.Log2(freq / 440.0)) % 12) + 12) % 12)
                : -1;
        }

        var onsetFft = new RealFFT(onsetFftSize);
        var chromaFft = new RealFFT(ChromaFftSize);
        var onsetBuffer = new double[onsetFftSize];
        var chromaBuffer = new double[ChromaFftSize];
        var onsetBins = onsetFftSize / 2;
        var logMag = new double[onsetBins];
        var prevLogMag = new double[onsetBins];
        var logMagLong = new double[chromaBins];
        var prevLogMagLong = new double[chromaBins];
        for (var f = 0; f < frameCount; f++)
        {
            var center = f * hop;

            FillWindowed(mono, center - onsetFftSize / 2, onsetWindow, onsetBuffer);
            onsetFft.ComputeForward(onsetBuffer);
            var flux = 0.0;
            for (var b = 0; b < onsetBins; b++)
            {
                var re = onsetBuffer[2 * b];
                var im = onsetBuffer[2 * b + 1];
                logMag[b] = Math.Log(1 + 100 * Math.Sqrt(re * re + im * im));
                if (f > 0)
                {
                    flux += Math.Max(0, logMag[b] - prevLogMag[b]);
                }
            }

            onsetShort[f] = flux;
            (logMag, prevLogMag) = (prevLogMag, logMag);

            FillWindowed(mono, center - ChromaFftSize / 2, chromaWindow, chromaBuffer);
            chromaFft.ComputeForward(chromaBuffer);
            var ch = new double[12];
            var fluxLong = 0.0;
            for (var b = 0; b < chromaBins; b++)
            {
                var re = chromaBuffer[2 * b];
                var im = chromaBuffer[2 * b + 1];
                var mag = Math.Sqrt(re * re + im * im);
                logMagLong[b] = Math.Log(1 + 100 * mag);
                if (f > 0)
                {
                    fluxLong += Math.Max(0, logMagLong[b] - prevLogMagLong[b]);
                }

                if (pitchClass[b] >= 0)
                {
                    ch[pitchClass[b]] += mag;
                }
            }

            onset[f] = fluxLong;
            (logMagLong, prevLogMagLong) = (prevLogMagLong, logMagLong);

            var norm = Math.Sqrt(ch.Sum(v => v * v)) + 1e-9;
            for (var k = 0; k < 12; k++)
            {
                ch[k] /= norm;
            }

            chroma[f] = ch;
        }

        Standardize(onset);
        Standardize(onsetShort);
        return new Features { Onset = onset, OnsetShort = onsetShort, Chroma = chroma };
    }

    private static void Standardize(double[] values)
    {
        var mean = values.Average();
        var std = Math.Sqrt(values.Select(v => (v - mean) * (v - mean)).Average()) + 1e-9;
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = (values[i] - mean) / std;
        }
    }

    private static double[] MakeHannWindow(int size)
    {
        var window = new double[size];
        for (var i = 0; i < size; i++)
        {
            window[i] = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / (size - 1)));
        }

        return window;
    }

    /// <summary>Copies a window of <paramref name="mono"/> starting at <paramref name="start"/>, zero outside the signal.</summary>
    private static void FillWindowed(float[] mono, int start, double[] window, double[] buffer)
    {
        for (var i = 0; i < window.Length; i++)
        {
            var index = start + i;
            buffer[i] = index >= 0 && index < mono.Length ? mono[index] * window[i] : 0;
        }
    }

    private static MusicLoop? FindBarAlignedLoop(float[] mono, int sr, double duration, Features features, double requestedBpm)
    {
        var onset = features.Onset;
        var nfr = features.FrameCount;

        // Bar length from the onset autocorrelation peak near k requested bars (a long lag gives
        // sub-percent resolution), refined with a parabola through the peak.
        var barRequested = 240.0 / requestedBpm;
        var k = 8 * barRequested < duration * 0.6 ? 8 : 4;
        var lo = (int)(k * barRequested * 0.94 * FramesPerSecond);
        var hi = (int)(k * barRequested * 1.06 * FramesPerSecond);
        if (hi + 1 >= nfr || lo < 1)
        {
            return null;
        }

        double AutoCorrelation(int lag)
        {
            var s = 0.0;
            for (var t = 0; t + lag < nfr; t++)
            {
                s += onset[t] * onset[t + lag];
            }

            return s;
        }

        var bestLag = lo;
        var bestValue = double.MinValue;
        for (var lag = lo; lag <= hi; lag++)
        {
            var v = AutoCorrelation(lag);
            if (v > bestValue)
            {
                bestValue = v;
                bestLag = lag;
            }
        }

        double peakLag = bestLag;
        var a = AutoCorrelation(bestLag - 1);
        var c = AutoCorrelation(bestLag + 1);
        var denominator = a - 2 * bestValue + c;
        if (Math.Abs(denominator) > 1e-9)
        {
            peakLag += Math.Clamp(0.5 * (a - c) / denominator, -1, 1);
        }

        var bar = peakLag / FramesPerSecond / k;
        var beat = bar / 4;

        // Beat phase: the offset that puts the most onset energy on the beat grid. Searched
        // around zero, so a beat detected a few ms before the start of the file is not pushed a
        // whole beat later.
        var phase = 0.0;
        var bestPhaseValue = double.MinValue;
        for (var phi = -beat / 2; phi < beat / 2; phi += 0.005)
        {
            var s = 0.0;
            for (var t = phi + beat; t < duration - 1; t += beat)
            {
                s += features.OnsetShort[Math.Clamp((int)Math.Round(t * FramesPerSecond), 0, nfr - 1)];
            }

            if (s > bestPhaseValue)
            {
                bestPhaseValue = s;
                phase = phi;
            }
        }

        // Downbeat: which of the four beats starts a bar. Chords change on downbeats, so take the
        // beat offset with the most harmonic change (chroma novelty) on its bar lines.
        var halfBeatFrames = Math.Max(1, (int)(beat / 2 * FramesPerSecond));
        double ChromaNovelty(double t)
        {
            var f = (int)Math.Round(t * FramesPerSecond);
            if (f - halfBeatFrames < 0 || f + halfBeatFrames >= nfr)
            {
                return 0;
            }

            var before = new double[12];
            var after = new double[12];
            for (var i = 1; i <= halfBeatFrames; i++)
            {
                for (var q = 0; q < 12; q++)
                {
                    before[q] += features.Chroma[f - i][q];
                    after[q] += features.Chroma[f + i][q];
                }
            }

            double dot = 0, nb = 0, na = 0;
            for (var q = 0; q < 12; q++)
            {
                dot += before[q] * after[q];
                nb += before[q] * before[q];
                na += after[q] * after[q];
            }

            return 1 - dot / (Math.Sqrt(nb * na) + 1e-9);
        }

        var bestDownbeat = 0;
        var bestNovelty = double.MinValue;
        for (var d = 0; d < 4; d++)
        {
            var novelty = 0.0;
            for (var t = phase + d * beat; t < duration - 1; t += bar)
            {
                novelty += ChromaNovelty(t);
            }

            if (novelty > bestNovelty)
            {
                bestNovelty = novelty;
                bestDownbeat = d;
            }
        }

        phase += bestDownbeat * beat;
        if (phase >= bar / 2)
        {
            phase -= bar;
        }

        var barCount = (int)((duration - phase) / bar);
        double BarTime(int b) => phase + b * bar;

        // Outro: drop trailing bars quieter than the median by more than 4 dB, then a descending
        // tail, plus one bar of safety.
        var barDb = new double[barCount];
        for (var i = 0; i < barCount; i++)
        {
            barDb[i] = RmsDb(mono, sr, BarTime(i), BarTime(i + 1));
        }

        if (barCount < 12)
        {
            return null;
        }

        var median = barDb.OrderBy(v => v).ElementAt(barCount / 2);
        var usable = barCount;
        while (usable > 0 && barDb[usable - 1] < median - 4)
        {
            usable--;
        }

        // A slow fade starts gently: keep trimming while each bar is quieter than the one before.
        while (usable > 1 && barDb[usable - 1] < barDb[usable - 2] - 1)
        {
            usable--;
        }

        usable--;

        int Frame(double seconds) => (int)Math.Round(seconds * FramesPerSecond);

        (double Score, double Lag) SeamScore(double tp, double te)
        {
            var a0 = Frame(tp - bar);
            var a1 = Frame(tp + 2 * bar);
            var best = (Score: double.MinValue, Lag: 0.0);
            if (a0 < 0)
            {
                return best;
            }

            for (var lag = -6; lag <= 6; lag++)
            {
                var b0 = Frame(te - bar) + lag;
                var b1 = b0 + (a1 - a0);
                if (b0 < 0 || b1 > nfr || a1 > nfr)
                {
                    continue;
                }

                double dot = 0, na = 0, nb = 0, harmony = 0;
                for (var i = 0; i < a1 - a0; i++)
                {
                    var oa = onset[a0 + i];
                    var ob = onset[b0 + i];
                    dot += oa * ob;
                    na += oa * oa;
                    nb += ob * ob;

                    var ca = features.Chroma[a0 + i];
                    var cb = features.Chroma[b0 + i];
                    for (var q = 0; q < 12; q++)
                    {
                        harmony += ca[q] * cb[q];
                    }
                }

                var rhythm = dot / (Math.Sqrt(na * nb) + 1e-9);
                harmony /= a1 - a0;
                var score = 0.5 * rhythm + 0.5 * harmony;
                if (score > best.Score)
                {
                    best = (score, lag / FramesPerSecond);
                }
            }

            return best;
        }

        // Seams may start on any beat (the detected downbeat is only preferred on a tie - a
        // phrase often breathes better across beat 3), but loops are whole bars long and end
        // before the outro.
        var candidates = new List<(double Score, int Bars, int StartBeat, double Lag)>();
        var usableEnd = BarTime(usable);
        for (var startBeat = 4; startBeat <= 35; startBeat++)
        {
            var startTime = phase + startBeat * beat;
            for (var n = 8; startTime + n * bar <= usableEnd + beat / 2; n += 2)
            {
                var endTime = startTime + n * bar;
                if (endTime + 2 * bar > duration)
                {
                    break;
                }

                var (score, lag) = SeamScore(startTime, endTime);
                if (score > double.MinValue)
                {
                    candidates.Add((score, n, startBeat, lag));
                }
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        // The longest loop (whole 4-bar phrases first) whose seam is within 0.05 of the best one:
        // picking greedily ("longer and nearly as good") lets the score drift down step by step.
        var top = candidates.Max(x => x.Score);
        var chosen = candidates
            .Where(x => x.Score >= top - 0.05)
            .OrderByDescending(x => x.Bars % 4 == 0)
            .ThenByDescending(x => x.Bars)
            .ThenByDescending(x => x.StartBeat % 4 == 0)
            .ThenByDescending(x => x.Score)
            .First();

        var start = phase + chosen.StartBeat * beat;
        var end = start + chosen.Bars * bar + chosen.Lag;
        return new MusicLoop
        {
            StartSeconds = start,
            EndSeconds = end,
            CrossfadeSeconds = bar,
            SeamScore = Math.Clamp(chosen.Score, 0, 1),
            MeasuredBpm = 240.0 / bar,
            IsBarAligned = true,
            EqualGainCrossfade = WaveformCorrelation(mono, sr, end - bar, start - bar, bar) > 0.5,
            StartBar = chosen.StartBeat / 4,
            Bars = chosen.Bars,
        };
    }

    /// <summary>
    /// No tempo grid (a very short or beatless clip): loop everything before the outro with a
    /// one-second equal-power crossfade.
    /// </summary>
    private static MusicLoop MakeFallbackLoop(float[] mono, int sr, double duration)
    {
        var end = duration;
        const double blockSeconds = 0.5;
        var levels = new List<double>();
        for (var t = 0.0; t + blockSeconds <= duration; t += blockSeconds)
        {
            levels.Add(RmsDb(mono, sr, t, t + blockSeconds));
        }

        if (levels.Count > 4)
        {
            var median = levels.OrderBy(v => v).ElementAt(levels.Count / 2);
            var i = levels.Count;
            while (i > 1 && levels[i - 1] < median - 4)
            {
                i--;
            }

            end = Math.Max(duration / 2, i * blockSeconds);
        }

        var crossfade = Math.Min(1.0, end / 4);
        return new MusicLoop
        {
            StartSeconds = crossfade,
            EndSeconds = end,
            CrossfadeSeconds = crossfade,
            SeamScore = 0,
            IsBarAligned = false,
            EqualGainCrossfade = false,
        };
    }

    private static double RmsDb(float[] mono, int sr, double t0, double t1)
    {
        var i0 = Math.Clamp((int)(t0 * sr), 0, mono.Length);
        var i1 = Math.Clamp((int)(t1 * sr), i0, mono.Length);
        if (i1 <= i0)
        {
            return -120;
        }

        double sum = 0;
        for (var i = i0; i < i1; i++)
        {
            sum += mono[i] * mono[i];
        }

        return 20 * Math.Log10(Math.Sqrt(sum / (i1 - i0)) + 1e-9);
    }

    private static double WaveformCorrelation(float[] mono, int sr, double ta, double tb, double seconds)
    {
        var a0 = (int)Math.Round(ta * sr);
        var b0 = (int)Math.Round(tb * sr);
        var n = (int)Math.Round(seconds * sr);
        if (a0 < 0 || b0 < 0 || a0 + n > mono.Length || b0 + n > mono.Length)
        {
            return 0;
        }

        double dot = 0, na = 0, nb = 0;
        for (var i = 0; i < n; i++)
        {
            dot += mono[a0 + i] * mono[b0 + i];
            na += mono[a0 + i] * mono[a0 + i];
            nb += mono[b0 + i] * mono[b0 + i];
        }

        return dot / (Math.Sqrt(na * nb) + 1e-9);
    }
}
