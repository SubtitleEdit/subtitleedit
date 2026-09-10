using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace UITests.Logic;

/// <summary>
/// A waveform/spectrogram cache file left half-written by a crash used to wedge the app: the
/// loader sized its arrays straight from the header, so garbage there meant a huge allocation on
/// the UI thread, and since the load path only checked that the file existed, the same ruined
/// file was re-read on every open of that video (#14751). These tests pin the two halves of the
/// fix - every size is bounded by the bytes the file really holds, and a finished write is moved
/// into place rather than streamed into the destination.
/// </summary>
public class WaveformCacheCorruptionTests
{
    private const int PeaksPerSecond = 100;

    private static string NewTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "se-wave-cache-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static List<WavePeak2> BuildPeaks(int count)
    {
        var peaks = new List<WavePeak2>(count);
        for (var i = 0; i < count; i++)
        {
            peaks.Add(new WavePeak2((short)(i % 1000), (short)-(i % 1000)));
        }

        return peaks;
    }

    private static byte[] BuildPeakFileBytes(int peakCount)
    {
        var stream = new MemoryStream();
        WavePeakGenerator2.WriteWaveformData(stream, PeaksPerSecond, BuildPeaks(peakCount));
        return stream.ToArray();
    }

    [Fact]
    public void FromDisk_ValidPeakFile_RoundTrips()
    {
        var dir = NewTempDir();
        try
        {
            var path = Path.Combine(dir, "peaks.wav");
            File.WriteAllBytes(path, BuildPeakFileBytes(500));

            var peaks = WavePeakData2.FromDisk(path);

            Assert.Equal(PeaksPerSecond, peaks.SampleRate);
            Assert.Equal(500, peaks.Peaks.Count - 5); // LoadPeaks pads the array by 5
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void FromDisk_EmptyFile_Throws()
    {
        var dir = NewTempDir();
        try
        {
            var path = Path.Combine(dir, "peaks.wav");
            File.WriteAllBytes(path, Array.Empty<byte>());

            Assert.Throws<InvalidDataException>(() => WavePeakData2.FromDisk(path));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    /// <summary>
    /// What a journalling filesystem hands back after a crash: the file has its size but the
    /// data never landed. A zero fmt chunk size used to read past an empty buffer.
    /// </summary>
    [Fact]
    public void FromDisk_ZeroFilledFile_Throws()
    {
        var dir = NewTempDir();
        try
        {
            var path = Path.Combine(dir, "peaks.wav");
            File.WriteAllBytes(path, new byte[4096]);

            Assert.Throws<InvalidDataException>(() => WavePeakData2.FromDisk(path));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    /// <summary>
    /// The allocation that froze the app: a garbage fmt chunk size is rejected against the file
    /// length instead of being handed to <c>new byte[FmtChunkSize]</c>.
    /// </summary>
    [Fact]
    public void FromDisk_FmtChunkSizeLargerThanFile_ThrowsWithoutAllocating()
    {
        var dir = NewTempDir();
        try
        {
            var path = Path.Combine(dir, "peaks.wav");
            var bytes = BuildPeakFileBytes(100);
            BitConverter.GetBytes(int.MaxValue - 8).CopyTo(bytes, 16);
            File.WriteAllBytes(path, bytes);

            Assert.Throws<InvalidDataException>(() => WavePeakData2.FromDisk(path));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void FromDisk_NegativeFmtChunkSize_Throws()
    {
        var dir = NewTempDir();
        try
        {
            var path = Path.Combine(dir, "peaks.wav");
            var bytes = BuildPeakFileBytes(100);
            BitConverter.GetBytes(-1).CopyTo(bytes, 16);
            File.WriteAllBytes(path, bytes);

            Assert.Throws<InvalidDataException>(() => WavePeakData2.FromDisk(path));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    /// <summary>
    /// The other allocation that froze the app. 0xFFFFFFFF in the data chunk size asked for a 4 GB
    /// byte array plus a 4 GB peak array; now it is capped at the bytes the file actually has, so
    /// the load stays bounded and returns the peaks that survived.
    /// </summary>
    [Fact]
    public void FromDisk_DataChunkSizeLargerThanFile_ClampsToWhatIsThere()
    {
        var dir = NewTempDir();
        try
        {
            var path = Path.Combine(dir, "peaks.wav");
            var bytes = BuildPeakFileBytes(250);
            BitConverter.GetBytes(uint.MaxValue).CopyTo(bytes, 40);
            File.WriteAllBytes(path, bytes);

            var peaks = WavePeakData2.FromDisk(path);

            // 250 peaks survived the "corruption"; the array is padded by 5 either way.
            Assert.Equal(255, peaks.Peaks.Count);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    /// <summary>
    /// The crash case the atomic write now prevents, read through the loader: a header promising
    /// the full peak count over a body that stops early loads the part that made it to disk.
    /// </summary>
    [Fact]
    public void FromDisk_TruncatedBody_LoadsTheSurvivingPeaks()
    {
        var dir = NewTempDir();
        try
        {
            var path = Path.Combine(dir, "peaks.wav");
            var bytes = BuildPeakFileBytes(1000);
            // Keep the 44 byte header and 400 of the 4000 payload bytes = 100 peaks.
            var truncated = new byte[44 + 400];
            Array.Copy(bytes, truncated, truncated.Length);
            File.WriteAllBytes(path, truncated);

            var peaks = WavePeakData2.FromDisk(path);

            Assert.Equal(105, peaks.Peaks.Count);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void FromDisk_ZeroSampleRate_Throws()
    {
        var dir = NewTempDir();
        try
        {
            var path = Path.Combine(dir, "peaks.wav");
            var bytes = BuildPeakFileBytes(100);
            BitConverter.GetBytes(0).CopyTo(bytes, 24);
            File.WriteAllBytes(path, bytes);

            Assert.Throws<InvalidDataException>(() => WavePeakData2.FromDisk(path));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void FromDisk_ZeroChannels_Throws()
    {
        var dir = NewTempDir();
        try
        {
            var path = Path.Combine(dir, "peaks.wav");
            var bytes = BuildPeakFileBytes(100);
            BitConverter.GetBytes((short)0).CopyTo(bytes, 22);
            File.WriteAllBytes(path, bytes);

            Assert.Throws<InvalidDataException>(() => WavePeakData2.FromDisk(path));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    private static byte[] BuildSpectrogramBytes(int fftSize, int imageWidth, double sampleDuration, int sampleCount)
    {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream, Encoding.UTF8);
        writer.Write(fftSize);
        writer.Write(imageWidth);
        writer.Write(sampleDuration);
        for (var i = 0; i < sampleCount; i++)
        {
            writer.Write((float)Math.Sin(i / 32.0) * 0.5f);
        }

        writer.Flush();
        return stream.ToArray();
    }

    [Fact]
    public void SpectrogramLoad_ValidFile_ReturnsTrue()
    {
        var dir = NewTempDir();
        try
        {
            var path = Path.Combine(dir, "a.spectrogram");
            File.WriteAllBytes(path, BuildSpectrogramBytes(256, 1024, 256 / 8000.0, 256 * 1024));

            using var spectrogram = SpectrogramData2.FromDisk(path);

            Assert.True(spectrogram.Load());
            Assert.Single(spectrogram.Images);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    /// <summary>
    /// The unbounded hang: the image count is the sample count divided by fftSize * imageWidth, so
    /// a corrupt 1 x 1 asked for one SKBitmap per sample and never came back.
    /// </summary>
    [Theory]
    [InlineData(1, 1)]
    [InlineData(0, 1024)]
    [InlineData(256, 0)]
    [InlineData(-256, 1024)]
    [InlineData(256, -1024)]
    [InlineData(255, 1024)] // odd - the image height is fftSize / 2
    public void SpectrogramLoad_CorruptMetadata_ReturnsFalse(int fftSize, int imageWidth)
    {
        var dir = NewTempDir();
        try
        {
            var path = Path.Combine(dir, "a.spectrogram");
            File.WriteAllBytes(path, BuildSpectrogramBytes(fftSize, imageWidth, 0.032, 4096));

            using var spectrogram = SpectrogramData2.FromDisk(path);

            Assert.False(spectrogram.Load());
            Assert.Empty(spectrogram.Images);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void SpectrogramLoad_TooSmallFile_ReturnsFalse()
    {
        var dir = NewTempDir();
        try
        {
            var path = Path.Combine(dir, "a.spectrogram");
            File.WriteAllBytes(path, new byte[8]);

            using var spectrogram = SpectrogramData2.FromDisk(path);

            Assert.False(spectrogram.Load());
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void SpectrogramLoad_MissingFile_ReturnsFalse()
    {
        var dir = NewTempDir();
        try
        {
            using var spectrogram = SpectrogramData2.FromDisk(Path.Combine(dir, "nope.spectrogram"));

            Assert.False(spectrogram.Load());
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void CacheFileWrite_CreatesTheDestinationAndLeavesNoTempFile()
    {
        var dir = NewTempDir();
        try
        {
            var path = Path.Combine(dir, "sub", "peaks.wav");

            WaveCacheFile.Write(path, stream => stream.Write(new byte[] { 1, 2, 3 }));

            Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(path));
            Assert.False(File.Exists(path + WaveCacheFile.TempSuffix));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    /// <summary>
    /// The heart of it: an interrupted write must leave the previous cache untouched rather than
    /// overwrite it with a partial file that the loader would then choke on at every open.
    /// </summary>
    [Fact]
    public void CacheFileWrite_FailedWrite_LeavesTheExistingFileIntact()
    {
        var dir = NewTempDir();
        try
        {
            var path = Path.Combine(dir, "peaks.wav");
            var original = BuildPeakFileBytes(100);
            File.WriteAllBytes(path, original);

            Assert.Throws<IOException>(() => WaveCacheFile.Write(path, stream =>
            {
                stream.Write(new byte[] { 9, 9, 9 });
                throw new IOException("disk full");
            }));

            Assert.Equal(original, File.ReadAllBytes(path));
            Assert.False(File.Exists(path + WaveCacheFile.TempSuffix));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void CacheFileWrite_ReplacesAnExistingFile()
    {
        var dir = NewTempDir();
        try
        {
            var path = Path.Combine(dir, "peaks.wav");
            File.WriteAllBytes(path, new byte[] { 7, 7, 7, 7, 7, 7 });

            WaveCacheFile.Write(path, stream => stream.Write(new byte[] { 1, 2 }));

            Assert.Equal(new byte[] { 1, 2 }, File.ReadAllBytes(path));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}
