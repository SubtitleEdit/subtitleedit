using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Computes the exact duration of an AAC (ADTS) file by walking every audio frame
/// and summing per-frame durations. Used as a robust method for computing accurate
/// durations of raw AAC or ADTS streams.
/// </summary>
public static class AacDurationReader
{
    // Sampling frequency table indexed by the 4-bit sampling frequency index from the ADTS header.
    private static readonly int[] SamplingRateTable = new int[16]
    {
        96000, 88200, 64000, 48000, 44100, 32000, 24000, 22050,
        16000, 12000, 11025,  8000,  7350,     0,     0,    -1
    };

    /// <summary>
    /// Returns the exact duration of <paramref name="path"/> in seconds, or
    /// null if the file is not a recognisable AAC/ADTS file (or no frames could be
    /// decoded). The file is opened read-only and never modified.
    /// </summary>
    public static double? TryGetDurationSeconds(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            return null;
        }

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return ReadDurationSeconds(stream);
        }
        catch
        {
            return null;
        }
    }

    private static double? ReadDurationSeconds(Stream stream)
    {
        var length = stream.Length;
        var pos = SkipId3v2(stream);

        stream.Position = pos;
        double totalSeconds = 0;
        var frameCount = 0;

        Span<byte> header = stackalloc byte[7];
        while (pos + 7 <= length)
        {
            stream.Position = pos;
            if (stream.Read(header) != 7)
            {
                break;
            }

            // ADTS syncword consists of 12 set bits: 0xFF in byte 0, and the top 4 bits of byte 1 set (0xF0).
            if (header[0] != 0xFF || (header[1] & 0xF0) != 0xF0)
            {
                pos++;
                continue;
            }

            if (!TryParseFrame(header, out var frameSize, out var frameDurationSeconds))
            {
                pos++;
                continue;
            }

            if (frameSize < 7 || pos + frameSize > length)
            {
                break;
            }

            totalSeconds += frameDurationSeconds;
            frameCount++;
            pos += frameSize;
        }

        return frameCount > 0 ? totalSeconds : null;
    }

    private static long SkipId3v2(Stream stream)
    {
        if (stream.Length < 10)
        {
            return 0;
        }

        stream.Position = 0;
        Span<byte> header = stackalloc byte[10];
        if (stream.Read(header) != 10)
        {
            return 0;
        }

        if (header[0] != (byte)'I' || header[1] != (byte)'D' || header[2] != (byte)'3')
        {
            return 0;
        }

        // Bytes 6..9 are a synchsafe 28-bit integer giving the tag size after the header.
        var size = ((header[6] & 0x7F) << 21)
                 | ((header[7] & 0x7F) << 14)
                 | ((header[8] & 0x7F) << 7)
                 |  (header[9] & 0x7F);

        // Bit 4 of the flags byte (header[5]) indicates a footer (extra 10 bytes).
        var hasFooter = (header[5] & 0x10) != 0;
        return 10L + size + (hasFooter ? 10L : 0L);
    }

    private static bool TryParseFrame(ReadOnlySpan<byte> header, out int frameSize, out double frameDurationSeconds)
    {
        frameSize = 0;
        frameDurationSeconds = 0;

        // Layer must always be 00 for ADTS
        var layer = (header[1] >> 1) & 0x03;
        if (layer != 0)
        {
            return false;
        }

        var sampleRateIndex = (header[2] >> 2) & 0x0F;
        if (sampleRateIndex >= SamplingRateTable.Length)
        {
            return false;
        }

        var sampleRate = SamplingRateTable[sampleRateIndex];
        if (sampleRate <= 0)
        {
            return false;
        }

        // Frame length is a 13-bit field spanning across bytes 3, 4, and 5:
        // - Last 2 bits of byte 3
        // - All 8 bits of byte 4
        // - Top 3 bits of byte 5
        frameSize = ((header[3] & 0x03) << 11)
                  | (header[4] << 3)
                  | ((header[5] & 0xE0) >> 5);

        if (frameSize < 7)
        {
            return false;
        }

        // Standard AAC-LC audio block uncompresses to 1024 PCM samples per frame
        const int samplesPerFrame = 1024;
        frameDurationSeconds = (double)samplesPerFrame / sampleRate;
        return true;
    }
}