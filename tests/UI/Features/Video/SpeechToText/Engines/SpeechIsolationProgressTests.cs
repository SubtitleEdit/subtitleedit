using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using System;
using System.IO;
using Xunit;

namespace UITests.Features.Video.SpeechToText.Engines;

/// <summary>
/// The separator's chunking, as measured on CrispASR 0.8.34: a 30 s clip is "segmented 1323000
/// samples into 5 chunks of 352800 (stride 264600)", 9 s makes 2 chunks, and anything up to the
/// 8 s window runs whole (#15176). Its per-chunk lines are the only progress there is, so the
/// count has to be right up front for the percentage to be.
/// </summary>
public class SpeechIsolationProgressTests
{
    [Theory]
    [InlineData(30.0, 5)]
    [InlineData(9.0, 2)]
    [InlineData(8.0, 1)]
    [InlineData(7.0, 1)]
    [InlineData(0.5, 1)]
    [InlineData(600.0, 100)]
    public void ChunkCountMatchesTheSeparator(double seconds, int expected)
    {
        Assert.Equal(expected, SpeechIsolationProgress.GetChunkCount(seconds));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    public void NoLengthMeansNoChunkCount(double seconds)
    {
        Assert.Equal(0, SpeechIsolationProgress.GetChunkCount(seconds));
    }

    [Fact]
    public void ChunkLinesAndLayerLinesMoveTheFraction()
    {
        var progress = new SpeechIsolationProgress(2);
        Assert.Null(progress.Fraction);
        Assert.Null(progress.Percent);

        Assert.False(progress.TryUpdate("crispasr 0.8.34 (git 7bd1d606, Release) [backends: cpu,metal,blas]"));
        Assert.False(progress.TryUpdate("mel_band_roformer: backend = CPU"));
        Assert.Null(progress.Fraction);

        Assert.True(progress.TryUpdate("mel_band_roformer: separating (T=801 frames, 6 layers, 60 bands)..."));
        Assert.Equal(1, progress.CurrentChunk);
        Assert.Equal(0.0, progress.Fraction!.Value, 6);

        Assert.True(progress.TryUpdate("mel_band_roformer: layer 3/6"));
        Assert.Equal(0.25, progress.Fraction!.Value, 6);
        Assert.Equal(25, progress.Percent);

        Assert.True(progress.TryUpdate("mel_band_roformer: layer 6/6"));
        Assert.Equal(0.5, progress.Fraction!.Value, 6);

        Assert.True(progress.TryUpdate("mel_band_roformer: separating (T=801 frames, 6 layers, 60 bands)..."));
        Assert.Equal(2, progress.CurrentChunk);
        Assert.Equal(0.5, progress.Fraction!.Value, 6);

        Assert.True(progress.TryUpdate("mel_band_roformer: layer 6/6"));
        Assert.Equal(1.0, progress.Fraction!.Value, 6);
        Assert.Equal(100, progress.Percent);
    }

    [Fact]
    public void UnknownChunkCountGivesNoPercentage()
    {
        var progress = new SpeechIsolationProgress(0);
        Assert.True(progress.TryUpdate("mel_band_roformer: separating (T=801 frames, 6 layers, 60 bands)..."));
        Assert.True(progress.TryUpdate("mel_band_roformer: layer 3/6"));
        Assert.Null(progress.Fraction);
        Assert.Null(progress.Percent);
    }

    [Fact]
    public void MoreChunksThanEstimatedNeverPassHundredPercent()
    {
        var progress = new SpeechIsolationProgress(1);
        Assert.True(progress.TryUpdate("mel_band_roformer: separating (T=801 frames, 6 layers, 60 bands)..."));
        Assert.True(progress.TryUpdate("mel_band_roformer: layer 6/6"));
        Assert.Equal(100, progress.Percent);

        Assert.True(progress.TryUpdate("mel_band_roformer: separating (T=801 frames, 6 layers, 60 bands)..."));
        Assert.Equal(2, progress.ChunkCount);
        Assert.Equal(50, progress.Percent);
    }

    [Fact]
    public void SegmentedSummaryEndsOnHundredPercent()
    {
        var progress = new SpeechIsolationProgress(6); // estimate one too many
        for (var i = 0; i < 5; i++)
        {
            progress.TryUpdate("mel_band_roformer: separating (T=801 frames, 6 layers, 60 bands)...");
        }

        Assert.Equal(66, progress.Percent);
        Assert.True(progress.TryUpdate("mel_band_roformer: segmented 1323000 samples into 5 chunks of 352800 (stride 264600, overlap 25%)"));
        Assert.Equal(5, progress.ChunkCount);
        Assert.Equal(100, progress.Percent);
    }

    [Fact]
    public void ChunkCountIsReadFromTheWaveHeader()
    {
        var fileName = Path.Combine(Path.GetTempPath(), "se-isolation-progress-" + Guid.NewGuid().ToString("N") + ".wav");
        try
        {
            WriteSilentWave(fileName, sampleRate: 16000, channels: 1, seconds: 30);
            Assert.Equal(5, SpeechIsolationProgress.GetChunkCountFromWaveFile(fileName));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void UnreadableWaveGivesNoChunkCount()
    {
        Assert.Equal(0, SpeechIsolationProgress.GetChunkCountFromWaveFile(Path.Combine(Path.GetTempPath(), "does-not-exist-" + Guid.NewGuid().ToString("N") + ".wav")));
    }

    /// <summary>A minimal 16-bit PCM wav: 44-byte header and a data chunk of zeros.</summary>
    private static void WriteSilentWave(string fileName, int sampleRate, int channels, int seconds)
    {
        var blockAlign = channels * 2;
        var dataSize = sampleRate * seconds * blockAlign;
        using var stream = File.Create(fileName);
        using var writer = new BinaryWriter(stream);
        writer.Write("RIFF"u8);
        writer.Write(36 + dataSize);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * blockAlign);
        writer.Write((short)blockAlign);
        writer.Write((short)16);
        writer.Write("data"u8);
        writer.Write(dataSize);
        writer.Write(new byte[dataSize]);
    }
}
