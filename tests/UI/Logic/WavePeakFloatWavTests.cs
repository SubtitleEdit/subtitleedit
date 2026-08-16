using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace UITests.Logic;

/// <summary>
/// The peak reader used to accept integer PCM only, so a float wav - what DAWs export, and what
/// "-c:a pcm_f32le" produces - drew a garbage waveform: the sample reader read the float bit
/// pattern as an int32. These tests pin the float and WAVE_FORMAT_EXTENSIBLE handling by feeding
/// the same waveform through both encodings and comparing the peaks.
/// </summary>
public class WavePeakFloatWavTests
{
    private const int SampleRate = 8000;
    private const int SampleCount = 8000;

    private static float SampleAt(int i)
    {
        // Speech-ish: a decaying tone, so peaks vary from chunk to chunk instead of being flat.
        var t = i / (double)SampleRate;
        return (float)(Math.Sin(2 * Math.PI * 220 * t) * Math.Exp(-t * 1.5));
    }

    private static byte[] BuildWav(int audioFormat, int bitsPerSample, Func<int, byte[]> sampleWriter, bool extensible = false)
    {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream, Encoding.UTF8);
        var bytesPerSample = bitsPerSample / 8;
        var fmtChunkSize = extensible ? 40 : 16;
        var dataSize = SampleCount * bytesPerSample;

        writer.Write(Encoding.UTF8.GetBytes("RIFF"));
        writer.Write(20 + fmtChunkSize + dataSize);
        writer.Write(Encoding.UTF8.GetBytes("WAVE"));
        writer.Write(Encoding.UTF8.GetBytes("fmt "));
        writer.Write(fmtChunkSize);
        writer.Write((short)(extensible ? 0xFFFE : audioFormat));
        writer.Write((short)1); // mono
        writer.Write(SampleRate);
        writer.Write(SampleRate * bytesPerSample);
        writer.Write((short)bytesPerSample);
        writer.Write((short)bitsPerSample);
        if (extensible)
        {
            writer.Write((short)22); // cbSize
            writer.Write((short)bitsPerSample); // wValidBitsPerSample
            writer.Write(0x4); // dwChannelMask - front center
            writer.Write((short)audioFormat); // first two bytes of the SubFormat GUID
            writer.Write(new byte[14]); // rest of the GUID
        }

        writer.Write(Encoding.UTF8.GetBytes("data"));
        writer.Write(dataSize);
        for (var i = 0; i < SampleCount; i++)
        {
            writer.Write(sampleWriter(i));
        }

        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] BuildPcm16Wav() =>
        BuildWav(1, 16, i => BitConverter.GetBytes((short)(SampleAt(i) * short.MaxValue)));

    private static byte[] BuildFloat32Wav() =>
        BuildWav(3, 32, i => BitConverter.GetBytes(SampleAt(i)));

    private static byte[] BuildFloat64Wav() =>
        BuildWav(3, 64, i => BitConverter.GetBytes((double)SampleAt(i)));

    private static WavePeakData2 GeneratePeaks(byte[] wav)
    {
        using var generator = new WavePeakGenerator2(new MemoryStream(wav));
        Assert.True(generator.IsSupported);
        return generator.GeneratePeaks(0, string.Empty);
    }

    [Fact]
    public void FloatWavIsSupported()
    {
        using var float32 = new WavePeakGenerator2(new MemoryStream(BuildFloat32Wav()));
        Assert.True(float32.IsSupported);

        using var float64 = new WavePeakGenerator2(new MemoryStream(BuildFloat64Wav()));
        Assert.True(float64.IsSupported);
    }

    [Fact]
    public void ExtensibleWavResolvesItsSubFormat()
    {
        // 0xFFFE says nothing on its own - the real tag lives in the SubFormat GUID. ffmpeg writes
        // this form for 24-bit and for more than two channels.
        using var extensiblePcm = new WavePeakGenerator2(new MemoryStream(BuildWav(1, 24, i =>
        {
            var value = (int)(SampleAt(i) * 8388607);
            return new[] { (byte)value, (byte)(value >> 8), (byte)(value >> 16) };
        }, extensible: true)));
        Assert.True(extensiblePcm.IsSupported);
        Assert.Equal(WaveHeader2.AudioFormatPcm, extensiblePcm.Header.AudioFormat);

        using var extensibleFloat = new WavePeakGenerator2(new MemoryStream(
            BuildWav(3, 32, i => BitConverter.GetBytes(SampleAt(i)), extensible: true)));
        Assert.True(extensibleFloat.IsSupported);
        Assert.Equal(WaveHeader2.AudioFormatIeeeFloat, extensibleFloat.Header.AudioFormat);
    }

    [Fact]
    public void FloatPeaksMatchIntegerPeaks()
    {
        var pcmPeaks = GeneratePeaks(BuildPcm16Wav());
        var float32Peaks = GeneratePeaks(BuildFloat32Wav());
        var float64Peaks = GeneratePeaks(BuildFloat64Wav());

        Assert.Equal(pcmPeaks.Peaks.Count, float32Peaks.Peaks.Count);
        Assert.Equal(pcmPeaks.Peaks.Count, float64Peaks.Peaks.Count);
        Assert.NotEmpty(pcmPeaks.Peaks);

        for (var i = 0; i < pcmPeaks.Peaks.Count; i++)
        {
            // Tolerance covers 16-bit quantization of the reference only - a wrongly read float
            // is off by orders of magnitude, not by a few counts.
            Assert.True(Math.Abs(pcmPeaks.Peaks[i].Max - float32Peaks.Peaks[i].Max) <= 4,
                $"peak {i}: pcm max {pcmPeaks.Peaks[i].Max} vs float32 max {float32Peaks.Peaks[i].Max}");
            Assert.True(Math.Abs(pcmPeaks.Peaks[i].Min - float32Peaks.Peaks[i].Min) <= 4,
                $"peak {i}: pcm min {pcmPeaks.Peaks[i].Min} vs float32 min {float32Peaks.Peaks[i].Min}");
            Assert.Equal(float32Peaks.Peaks[i].Max, float64Peaks.Peaks[i].Max);
            Assert.Equal(float32Peaks.Peaks[i].Min, float64Peaks.Peaks[i].Min);
        }
    }

    [Fact]
    public void FloatSamplesAboveFullScaleAreClamped()
    {
        // Float wav keeps headroom above 1.0 - which is exactly why "volume=1.75" into float does
        // not clip. The reader has to clamp rather than overflow the int cast.
        var wav = BuildWav(3, 32, i => BitConverter.GetBytes(SampleAt(i) * 2.5f));
        var peaks = GeneratePeaks(wav);

        foreach (var peak in peaks.Peaks)
        {
            Assert.InRange(peak.Max, (short)-short.MaxValue, short.MaxValue);
            Assert.InRange(peak.Min, (short)-short.MaxValue, short.MaxValue);
        }
    }
}
