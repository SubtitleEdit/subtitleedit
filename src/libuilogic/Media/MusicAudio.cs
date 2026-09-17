using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.Media;

/// <summary>
/// Interleaved float audio in memory, as generated music comes back from audio.cpp. Samples are
/// kept as float and unclamped: ACE-Step decodes above ±1.0, and clipping must only happen once,
/// after the gain has been applied.
/// </summary>
public sealed class MusicAudio
{
    public int SampleRate { get; }
    public int Channels { get; }
    public float[] Samples { get; }

    public MusicAudio(int sampleRate, int channels, float[] samples)
    {
        if (sampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
        }

        if (channels <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(channels));
        }

        SampleRate = sampleRate;
        Channels = channels;
        Samples = samples;
    }

    public int FrameCount => Samples.Length / Channels;

    public double DurationSeconds => (double)FrameCount / SampleRate;

    /// <summary>Channel average, one value per frame.</summary>
    public float[] ToMono()
    {
        var frames = FrameCount;
        var mono = new float[frames];
        for (var f = 0; f < frames; f++)
        {
            var sum = 0f;
            var baseIndex = f * Channels;
            for (var c = 0; c < Channels; c++)
            {
                sum += Samples[baseIndex + c];
            }

            mono[f] = sum / Channels;
        }

        return mono;
    }

    public float GetPeak()
    {
        var peak = 0f;
        foreach (var s in Samples)
        {
            var a = Math.Abs(s);
            if (a > peak)
            {
                peak = a;
            }
        }

        return peak;
    }

    /// <summary>
    /// Reads a RIFF/WAVE file: 16/24/32-bit PCM or 32-bit IEEE float, plain or
    /// WAVE_FORMAT_EXTENSIBLE. audio.cpp writes 16-bit PCM by default and float with
    /// <c>--out-format float32</c>.
    /// </summary>
    public static MusicAudio ReadWav(string fileName)
    {
        using var stream = File.OpenRead(fileName);
        using var reader = new BinaryReader(stream);

        if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "RIFF")
        {
            throw new InvalidDataException("Not a RIFF file: " + fileName);
        }

        reader.ReadUInt32();
        if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "WAVE")
        {
            throw new InvalidDataException("Not a WAVE file: " + fileName);
        }

        int formatTag = 0, channels = 0, sampleRate = 0, bitsPerSample = 0;
        var hasFormat = false;
        while (stream.Position + 8 <= stream.Length)
        {
            var chunkId = Encoding.ASCII.GetString(reader.ReadBytes(4));
            var chunkSize = reader.ReadUInt32();
            var chunkStart = stream.Position;

            if (chunkId == "fmt ")
            {
                formatTag = reader.ReadUInt16();
                channels = reader.ReadUInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadInt32(); // byte rate
                reader.ReadUInt16(); // block align
                bitsPerSample = reader.ReadUInt16();
                if (formatTag == 0xFFFE && chunkSize >= 40)
                {
                    reader.ReadUInt16(); // cbSize
                    reader.ReadUInt16(); // valid bits
                    reader.ReadUInt32(); // channel mask
                    formatTag = reader.ReadUInt16(); // first two bytes of the sub-format GUID
                }

                hasFormat = true;
            }
            else if (chunkId == "data")
            {
                if (!hasFormat)
                {
                    throw new InvalidDataException("WAVE data chunk before fmt chunk: " + fileName);
                }

                var available = Math.Min(chunkSize, stream.Length - chunkStart);
                return new MusicAudio(sampleRate, channels, DecodeSamples(reader.ReadBytes((int)available), formatTag, bitsPerSample));
            }

            stream.Position = chunkStart + chunkSize + (chunkSize & 1);
        }

        throw new InvalidDataException("WAVE file has no data chunk: " + fileName);
    }

    private static float[] DecodeSamples(byte[] data, int formatTag, int bitsPerSample)
    {
        const int pcm = 1;
        const int ieeeFloat = 3;

        if (formatTag == ieeeFloat && bitsPerSample == 32)
        {
            var result = new float[data.Length / 4];
            Buffer.BlockCopy(data, 0, result, 0, result.Length * 4);
            return result;
        }

        if (formatTag != pcm)
        {
            throw new InvalidDataException($"Unsupported WAVE format tag {formatTag} ({bitsPerSample} bit)");
        }

        switch (bitsPerSample)
        {
            case 16:
            {
                var result = new float[data.Length / 2];
                for (var i = 0; i < result.Length; i++)
                {
                    result[i] = BitConverter.ToInt16(data, i * 2) / 32768f;
                }

                return result;
            }
            case 24:
            {
                var result = new float[data.Length / 3];
                for (var i = 0; i < result.Length; i++)
                {
                    var o = i * 3;
                    var v = (data[o] | (data[o + 1] << 8) | ((sbyte)data[o + 2] << 16));
                    result[i] = v / 8388608f;
                }

                return result;
            }
            case 32:
            {
                var result = new float[data.Length / 4];
                for (var i = 0; i < result.Length; i++)
                {
                    result[i] = BitConverter.ToInt32(data, i * 4) / 2147483648f;
                }

                return result;
            }
            default:
                throw new InvalidDataException($"Unsupported PCM bit depth {bitsPerSample}");
        }
    }

    /// <summary>Writes 16-bit PCM, clamping to ±1 (call after the final gain).</summary>
    public void WritePcm16Wav(string fileName)
    {
        using var stream = File.Create(fileName);
        using var writer = new BinaryWriter(stream);
        var dataBytes = Samples.Length * 2;

        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataBytes);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)Channels);
        writer.Write(SampleRate);
        writer.Write(SampleRate * Channels * 2);
        writer.Write((short)(Channels * 2));
        writer.Write((short)16);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataBytes);

        var buffer = new byte[Math.Min(dataBytes, 1 << 20)];
        var bufferPos = 0;
        foreach (var s in Samples)
        {
            var v = (short)Math.Round(Math.Clamp(s, -1f, 1f) * 32767f);
            buffer[bufferPos++] = (byte)v;
            buffer[bufferPos++] = (byte)(v >> 8);
            if (bufferPos == buffer.Length)
            {
                writer.Write(buffer, 0, bufferPos);
                bufferPos = 0;
            }
        }

        if (bufferPos > 0)
        {
            writer.Write(buffer, 0, bufferPos);
        }
    }
}
