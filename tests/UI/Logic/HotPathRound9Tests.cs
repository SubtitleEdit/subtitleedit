using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using NikseBitmap = Nikse.SubtitleEdit.Core.Common.NikseBitmap;

namespace UITests.Logic;

/// <summary>
/// Equivalence tests for the round-9 span/SIMD rewrites: every fast path is compared against a
/// faithful replica of the code it replaced, on data that includes the edge values (short.MinValue,
/// zero alpha with garbage color bytes, odd lengths that exercise the vector tails).
/// </summary>
public class HotPathRound9Tests
{
    private static short[] MakeShorts(int count, int seed)
    {
        var random = new Random(seed);
        var data = new short[count];
        for (var i = 0; i < count; i++)
        {
            data[i] = (short)random.Next(short.MinValue, short.MaxValue + 1);
        }

        // Plant the edge values so the abs/min/max math is exercised at the extremes.
        if (count > 4)
        {
            data[1] = short.MinValue;
            data[2] = short.MaxValue;
            data[3] = 0;
        }

        return data;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(64)]
    [InlineData(1001)] // odd length: vector loop + scalar tail
    public void CalculateHighestPeak_MatchesScalarReference(int count)
    {
        var shorts = MakeShorts(count * 2, seed: count);
        var peaks = new WavePeak2[count];
        for (var i = 0; i < count; i++)
        {
            peaks[i] = new WavePeak2(shorts[i * 2], shorts[i * 2 + 1]);
        }

        var expected = 0;
        foreach (var peak in peaks)
        {
            var abs = peak.Abs;
            if (abs > expected)
            {
                expected = abs;
            }
        }

        Assert.Equal(expected, WavePeakData2.CalculateHighestPeak(peaks));
    }

    [Fact]
    public void CalculateHighestPeak_ShortMinValue_Is32768()
    {
        var peaks = new[] { new WavePeak2(0, short.MinValue) };
        Assert.Equal(32768, WavePeakData2.CalculateHighestPeak(peaks));
    }

    [Fact]
    public void LoadPeaks_RoundTripsWriteWaveformData()
    {
        var random = new Random(9);
        var peaks = new List<WavePeak2>();
        for (var i = 0; i < 12345; i++)
        {
            peaks.Add(new WavePeak2((short)random.Next(short.MinValue, short.MaxValue + 1),
                (short)random.Next(short.MinValue, short.MaxValue + 1)));
        }

        using var stream = new MemoryStream();
        WavePeakGenerator2.WriteWaveformData(stream, 126, peaks);
        stream.Position = 0;

        var loaded = WavePeakData2.FromStream(stream);

        Assert.Equal(126, loaded.SampleRate);
        Assert.True(loaded.Peaks.Count >= peaks.Count);
        for (var i = 0; i < peaks.Count; i++)
        {
            Assert.Equal(peaks[i].Max, loaded.Peaks[i].Max);
            Assert.Equal(peaks[i].Min, loaded.Peaks[i].Min);
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(380)]
    [InlineData(762)]
    public void ConvertPeakChunk16BitStereo_MatchesOldLoop(int sampleCount)
    {
        var samples = MakeShorts(sampleCount, seed: sampleCount);
        const float scale = 0.5f / short.MaxValue;

        var expected = new float[sampleCount];
        var chunkSampleOffset = 0;
        for (var sIdx = 0; sIdx + 1 < samples.Length; sIdx += 2)
        {
            short v1 = samples[sIdx];
            short v2 = samples[sIdx + 1];
            float pos = 0, neg = 0;
            if (v1 < 0) { neg += v1; } else { pos += v1; }
            if (v2 < 0) { neg += v2; } else { pos += v2; }
            expected[chunkSampleOffset++] = neg * scale;
            expected[chunkSampleOffset++] = pos * scale;
        }

        var actual = new float[sampleCount];
        WavePeakGenerator2.ConvertPeakChunk16BitStereo(samples, actual, scale);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ConvertSpectrogramChunks_MatchOldDoubleAccumulation()
    {
        var samples = MakeShorts(2048, seed: 7);
        const double scale = 0.5 / short.MaxValue;

        // Stereo reference: double accumulation over both channels.
        var expectedStereo = new float[samples.Length / 2];
        for (var s = 0; s < samples.Length; s += 2)
        {
            double value = (double)samples[s] + samples[s + 1];
            expectedStereo[s / 2] = (float)(value * scale);
        }

        var actualStereo = new float[samples.Length / 2];
        WavePeakGenerator2.ConvertSpectrogramChunk16BitStereo(samples, actualStereo, scale);
        Assert.Equal(expectedStereo, actualStereo);

        // Mono reference.
        var expectedMono = new float[samples.Length];
        for (var s = 0; s < samples.Length; s++)
        {
            expectedMono[s] = (float)(samples[s] * scale);
        }

        var actualMono = new float[samples.Length];
        WavePeakGenerator2.ConvertSpectrogramChunk16BitMono(samples, actualMono, scale);
        Assert.Equal(expectedMono, actualMono);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(763)] // odd: vector loop + tail
    public void CalculatePeak_MatchesScalarReference(int count)
    {
        var random = new Random(count);
        var chunk = new float[count + 3];
        for (var i = 0; i < chunk.Length; i++)
        {
            chunk[i] = (float)(random.NextDouble() * 2 - 1);
        }

        float max = chunk[0], min = chunk[0];
        for (var i = 1; i < count; i++)
        {
            max = Math.Max(max, chunk[i]);
            min = Math.Min(min, chunk[i]);
        }

        var expected = new WavePeak2((short)(short.MaxValue * max), (short)(short.MaxValue * min));
        var actual = WavePeakGenerator2.CalculatePeak(chunk, count);

        Assert.Equal(expected.Max, actual.Max);
        Assert.Equal(expected.Min, actual.Min);
    }

    [Fact]
    public void GeneratePeaks_16BitMonoAndStereo_MatchReferenceImplementation()
    {
        foreach (var channels in new[] { 1, 2 })
        {
            const int sampleRate = 8064; // divisible by 126 so peaksPerSecond stays 126
            const int seconds = 3;
            var samples = MakeShorts(sampleRate * seconds * channels, seed: channels);

            using var stream = new MemoryStream();
            WaveHeader2.WriteHeader(stream, sampleRate, channels, 16, samples.Length / channels);
            stream.Write(MemoryMarshal.AsBytes(samples.AsSpan()));
            stream.Position = 0;

            using var generator = new WavePeakGenerator2(stream);
            var data = generator.GeneratePeaks(0, string.Empty);

            // Reference: the old per-sample logic, chunked identically.
            var chunkSampleCount = sampleRate / data.SampleRate;
            var scale = (float)(1.0 / Math.Pow(2.0, 15) / channels); // GetSampleAndChannelScale for 16-bit
            var expected = new List<WavePeak2>();
            for (var offset = 0; offset < samples.Length; offset += chunkSampleCount * channels)
            {
                var frameCount = Math.Min(chunkSampleCount, (samples.Length - offset) / channels);
                float max = float.MinValue, min = float.MaxValue;
                var any = false;
                for (var f = 0; f < frameCount; f++)
                {
                    float pos = 0, neg = 0;
                    for (var c = 0; c < channels; c++)
                    {
                        var v = samples[offset + f * channels + c];
                        if (v < 0) { neg += v; } else { pos += v; }
                    }

                    var negScaled = neg * scale;
                    var posScaled = pos * scale;
                    max = Math.Max(max, Math.Max(negScaled, posScaled));
                    min = Math.Min(min, Math.Min(negScaled, posScaled));
                    any = true;
                }

                if (any)
                {
                    expected.Add(new WavePeak2((short)(short.MaxValue * max), (short)(short.MaxValue * min)));
                }
            }

            Assert.Equal(expected.Count, data.Peaks.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.True(expected[i].Max == data.Peaks[i].Max && expected[i].Min == data.Peaks[i].Min,
                    $"channels={channels} peak {i}: expected ({expected[i].Max},{expected[i].Min}) got ({data.Peaks[i].Max},{data.Peaks[i].Min})");
            }
        }
    }

    [Fact]
    public void CopyPremulRow_MatchesScalarBranches()
    {
        var random = new Random(11);
        var src = new uint[1023]; // odd: vector loop + tail
        for (var i = 0; i < src.Length; i++)
        {
            src[i] = (uint)random.Next();
        }

        // a == 0 with garbage color bytes must be forced to fully zero.
        src[0] = 0x00ABCDEF;
        src[5] = 0x00FFFFFF;

        var expected = new uint[src.Length];
        for (var i = 0; i < src.Length; i++)
        {
            var pixel = src[i];
            var a = pixel >> 24;
            expected[i] = a == 0 ? 0u : pixel;
        }

        var actual = new uint[src.Length];
        SkBitmapExtensions.CopyPremulRow(src, actual);

        Assert.Equal(expected, actual);
    }

    private static SKBitmap MakeBitmap(SKColorType colorType, SKAlphaType alphaType, int seed, uint backgroundPixel)
    {
        var bmp = new SKBitmap(new SKImageInfo(97, 41, colorType, alphaType)); // odd width: tails
        var random = new Random(seed);
        unsafe
        {
            var ptr = (uint*)bmp.GetPixels();
            var count = bmp.Width * bmp.Height;
            for (var i = 0; i < count; i++)
            {
                var roll = random.Next(100);
                if (roll < 60)
                {
                    ptr[i] = backgroundPixel;
                }
                else if (roll < 70)
                {
                    ptr[i] = 0x20304050; // semi-transparent
                }
                else
                {
                    ptr[i] = (uint)random.Next() | 0xFF000000;
                }
            }
        }

        return bmp;
    }

    [Fact]
    public void MakeTransparent_ClearsBackgroundColoredPixels_KeepsOthers()
    {
        using var original = MakeBitmap(SKColorType.Bgra8888, SKAlphaType.Unpremul, seed: 3, backgroundPixel: 0xFF102030);
        using var result = OcrSubtitleBdn.MakeTransparent(original);

        Assert.Equal(original.Width, result.Width);
        Assert.Equal(original.Height, result.Height);

        unsafe
        {
            var src = (uint*)original.GetPixels();
            var dst = (uint*)result.GetPixels();
            var count = original.Width * original.Height;
            var background = src[0] & 0x00FFFFFF;
            for (var i = 0; i < count; i++)
            {
                var expected = (src[i] & 0x00FFFFFF) == background ? 0u : src[i];
                Assert.Equal(expected, dst[i]);
            }
        }
    }

    [Fact]
    public void MakeTransparentBlack_MatchesOldPixelsBasedVersion()
    {
        using var forOld = MakeBitmap(SKColorType.Bgra8888, SKAlphaType.Unpremul, seed: 5, backgroundPixel: 0x00000000);
        using var forNew = forOld.Copy();

        // Old version replica.
        var colors = forOld.Pixels;
        var blackOpaque = new SKColor(0, 0, 0, 255);
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i].Alpha < 100)
            {
                colors[i] = blackOpaque;
            }
        }

        forOld.Pixels = colors;

        var result = PaddleOcr.MakeTransparentBlack(forNew);

        Assert.Equal(forOld.Pixels, result.Pixels);
    }

    [Fact]
    public void GetInkPercent_MatchesOldGetPixelVersion()
    {
        using var bmp = MakeBitmap(SKColorType.Bgra8888, SKAlphaType.Unpremul, seed: 8, backgroundPixel: 0xFFFFFFFF);
        var nbmp = new NikseBitmap(bmp);

        long total = (long)nbmp.Width * nbmp.Height;
        long ink = 0;
        for (var y = 0; y < nbmp.Height; y++)
        {
            for (var x = 0; x < nbmp.Width; x++)
            {
                var c = nbmp.GetPixel(x, y);
                if (c.Alpha > 0 && c.Red < 128 && c.Green < 128 && c.Blue < 128)
                {
                    ink++;
                }
            }
        }

        var expected = ink * 100.0 / total;

        Assert.Equal(expected, TesseractOcr.GetInkPercent(nbmp));
    }

    [Fact]
    public void InvertColors_MatchesOldUnpackRepackLoop()
    {
        using var bmp = MakeBitmap(SKColorType.Bgra8888, SKAlphaType.Unpremul, seed: 13, backgroundPixel: 0xFF808080);

        var expected = new uint[bmp.Width * bmp.Height];
        unsafe
        {
            var src = (uint*)bmp.GetPixels();
            for (var i = 0; i < expected.Length; i++)
            {
                var pixel = src[i];
                var a = (pixel >> 24) & 0xFF;
                var r = (pixel >> 16) & 0xFF;
                var g = (pixel >> 8) & 0xFF;
                var b = pixel & 0xFF;
                expected[i] = (a << 24) | ((255 - r) << 16) | ((255 - g) << 8) | (255 - b);
            }
        }

        using var inverted = PreProcessingSettings.InvertColors(bmp);
        unsafe
        {
            var dst = (uint*)inverted.GetPixels();
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], dst[i]);
            }
        }
    }

    [AvaloniaFact]
    public void ToAvaloniaBitmap_Premul_MatchesOldScalarLoop()
    {
        using var skBitmap = MakeBitmap(SKColorType.Bgra8888, SKAlphaType.Premul, seed: 21, backgroundPixel: 0xFF112233);
        unsafe
        {
            // Plant a==0-with-garbage pixels: the old loop forced them to zero.
            var ptr = (uint*)skBitmap.GetPixels();
            ptr[3] = 0x00DEADBE;
        }

        var bitmap = (Avalonia.Media.Imaging.WriteableBitmap)skBitmap.ToAvaloniaBitmap();
        using var locked = bitmap.Lock();

        unsafe
        {
            var src = (byte*)skBitmap.GetPixels();
            var dst = (byte*)locked.Address;
            for (var y = 0; y < skBitmap.Height; y++)
            {
                var srcRow = (uint*)(src + y * skBitmap.RowBytes);
                var dstRow = (uint*)(dst + y * locked.RowBytes);
                for (var x = 0; x < skBitmap.Width; x++)
                {
                    var pixel = srcRow[x];
                    var a = pixel >> 24;
                    uint expected = a == 0 ? 0u : pixel;
                    Assert.Equal(expected, dstRow[x]);
                }
            }
        }
    }
}
