using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Waveform peak extraction, 16-bit stereo PCM (what SE's own ffmpeg extraction produces):
/// the old two-step pipeline per chunk (a scalar convert writes a float neg/pos pair per
/// frame, then CalculatePeak SIMD-reduces the floats; the convert is preserved below as the
/// baseline) against the shipped fused single-pass SIMD peak
/// (WavePeakGenerator2.CalculatePeak16BitStereo), which never materializes the intermediate
/// float buffer. See that method for why the integer-space max is bit-identical to the float
/// pipeline's result; GlobalSetup asserts it over random and adversarial data (including the
/// short.MinValue negation trap).
///
/// Both candidates are exercised chunk-by-chunk (375 frames = 48 kHz / 128 peaks-per-second),
/// exactly how GeneratePeaks consumes them, over 10 s of speech-like audio per invocation.
///
/// Also here: the GenerateEmptyPeaks write shape - one 4-byte stream.Write per peak versus
/// one bulk span write of the whole peak list (byte-identical output, asserted in setup).
/// </summary>
[MemoryDiagnoser]
public class WaveformPeakSimdBenchmarks
{
    private const int ChunkFrames = 375; // 48000 Hz / 128 peaks-per-second
    private const int Seconds = 10;
    private const float Scale = 1f / 65536f; // 16-bit stereo: (1/2^15)/2

    private short[] _samples = null!;
    private float[] _chunkSamples = null!;

    private WavePeak2[] _emptyPeaks = null!;

    [GlobalSetup]
    public void Setup()
    {
        _samples = MakeSpeechLikeSamples(48000 * Seconds);
        // A little extra room: the equivalence cases include chunks slightly longer than
        // ChunkFrames (the benchmark itself only ever uses ChunkFrames-sized chunks).
        _chunkSamples = new float[(ChunkFrames + 4) * 2];

        // ~40 minutes of empty peaks (126 peaks/s), the shape GenerateEmptyPeaks builds.
        _emptyPeaks = new WavePeak2[126 * 2400 + 2];
        _emptyPeaks[0] = new WavePeak2(1000, -1000);
        for (var i = 1; i < _emptyPeaks.Length - 1; i++)
        {
            _emptyPeaks[i] = new WavePeak2(1, -1);
        }

        _emptyPeaks[^1] = new WavePeak2(1000, -1000);

        AssertEquivalence();
    }

    private void AssertEquivalence()
    {
        // Adversarial chunks: silence, DC extremes, short.MinValue everywhere (the negation
        // overflow trap), alternating extremes, single frame, odd tails, and random data.
        var cases = new List<short[]>
        {
            new short[ChunkFrames * 2], // silence
            Enumerable.Repeat(short.MinValue, ChunkFrames * 2).ToArray(),
            Enumerable.Repeat(short.MaxValue, ChunkFrames * 2).ToArray(),
            Enumerable.Range(0, ChunkFrames * 2).Select(i => (i & 1) == 0 ? short.MinValue : short.MaxValue).ToArray(),
            new short[] { short.MinValue, short.MinValue },
            new short[] { -1, 1 },
            new short[] { 123, -456, 789, -12, 345 }, // odd length - trailing sample ignored by both
        };

        var random = new Random(1234);
        for (var n = 0; n < 200; n++)
        {
            var len = random.Next(1, ChunkFrames * 2 + 3);
            var chunk = new short[len];
            for (var i = 0; i < len; i++)
            {
                chunk[i] = (short)random.Next(short.MinValue, short.MaxValue + 1);
            }

            cases.Add(chunk);
        }

        foreach (var chunk in cases)
        {
            var expected = CurrentPipeline(chunk);
            var actual = WavePeakGenerator2.CalculatePeak16BitStereo(chunk, Scale);
            if (expected.Max != actual.Max || expected.Min != actual.Min)
            {
                throw new InvalidOperationException(
                    $"Peak mismatch for len={chunk.Length}: expected ({expected.Max},{expected.Min}), got ({actual.Max},{actual.Min})");
            }
        }

        // Empty-peaks write shapes must produce byte-identical streams.
        using var perPeak = new MemoryStream();
        WriteEmptyPeaksPerPeak(perPeak, _emptyPeaks);
        using var bulk = new MemoryStream();
        WriteEmptyPeaksBulk(bulk, _emptyPeaks);
        if (!perPeak.ToArray().AsSpan().SequenceEqual(bulk.ToArray()))
        {
            throw new InvalidOperationException("Empty-peaks write shapes differ");
        }
    }

    private WavePeak2 CurrentPipeline(ReadOnlySpan<short> chunk)
    {
        var frames = chunk.Length / 2;
        ConvertPeakChunk16BitStereoOld(chunk, _chunkSamples, Scale);
        return WavePeakGenerator2.CalculatePeak(_chunkSamples, frames * 2);
    }

    [Benchmark(Baseline = true)]
    public int PeakChunks_Current()
    {
        var acc = 0;
        var samples = _samples.AsSpan();
        for (var offset = 0; offset + ChunkFrames * 2 <= samples.Length; offset += ChunkFrames * 2)
        {
            var chunk = samples.Slice(offset, ChunkFrames * 2);
            ConvertPeakChunk16BitStereoOld(chunk, _chunkSamples, Scale);
            var peak = WavePeakGenerator2.CalculatePeak(_chunkSamples, ChunkFrames * 2);
            acc += peak.Max + peak.Min;
        }

        return acc;
    }

    /// <summary>The old two-step pipeline's convert, preserved here as the baseline.</summary>
    private static void ConvertPeakChunk16BitStereoOld(ReadOnlySpan<short> samples, Span<float> chunkSamples, float scale)
    {
        var chunkSampleOffset = 0;
        for (var sIdx = 0; sIdx + 1 < samples.Length; sIdx += 2)
        {
            short v1 = samples[sIdx];
            short v2 = samples[sIdx + 1];

            float pos = 0, neg = 0;

            if (v1 < 0)
            {
                neg += v1;
            }
            else
            {
                pos += v1;
            }

            if (v2 < 0)
            {
                neg += v2;
            }
            else
            {
                pos += v2;
            }

            chunkSamples[chunkSampleOffset++] = neg * scale;
            chunkSamples[chunkSampleOffset++] = pos * scale;
        }
    }

    [Benchmark]
    public int PeakChunks_FusedSimd()
    {
        var acc = 0;
        var samples = _samples.AsSpan();
        for (var offset = 0; offset + ChunkFrames * 2 <= samples.Length; offset += ChunkFrames * 2)
        {
            var peak = WavePeakGenerator2.CalculatePeak16BitStereo(samples.Slice(offset, ChunkFrames * 2), Scale);
            acc += peak.Max + peak.Min;
        }

        return acc;
    }

    [Benchmark]
    public long EmptyPeaksWrite_PerPeak()
    {
        using var stream = new MemoryStream(_emptyPeaks.Length * 4);
        WriteEmptyPeaksPerPeak(stream, _emptyPeaks);
        return stream.Length;
    }

    [Benchmark]
    public long EmptyPeaksWrite_Bulk()
    {
        using var stream = new MemoryStream(_emptyPeaks.Length * 4);
        WriteEmptyPeaksBulk(stream, _emptyPeaks);
        return stream.Length;
    }

    /// <summary>Old GenerateEmptyPeaks write shape: 4 bytes per peak.</summary>
    private static void WriteEmptyPeaksPerPeak(Stream stream, WavePeak2[] peaks)
    {
        var buffer = new byte[4];
        foreach (var peak in peaks)
        {
            buffer[0] = (byte)peak.Max;
            buffer[1] = (byte)(peak.Max >> 8);
            buffer[2] = (byte)peak.Min;
            buffer[3] = (byte)(peak.Min >> 8);
            stream.Write(buffer, 0, 4);
        }
    }

    /// <summary>Candidate: one bulk write of the (Max, Min) short pairs.</summary>
    private static void WriteEmptyPeaksBulk(Stream stream, WavePeak2[] peaks)
    {
        stream.Write(MemoryMarshal.AsBytes(peaks.AsSpan()));
    }

    private static short[] MakeSpeechLikeSamples(int frames)
    {
        // Bursts of loud audio separated by near-silence, mirroring
        // AudioVisualizerRenderBenchmarks.MakeSpeechLikePeaks.
        var random = new Random(42);
        var samples = new short[frames * 2];
        for (var f = 0; f < frames; f++)
        {
            var seconds = f / 48000.0;
            var inBurst = seconds % 0.5 < 0.3;
            var amplitude = inBurst ? random.Next(4000, 28000) : random.Next(0, 900);
            samples[f * 2] = (short)(random.Next(2) == 0 ? amplitude : -amplitude);
            samples[f * 2 + 1] = (short)(random.Next(2) == 0 ? amplitude : -amplitude);
        }

        return samples;
    }
}
