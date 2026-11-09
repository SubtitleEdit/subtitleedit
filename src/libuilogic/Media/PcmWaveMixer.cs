using System.Buffers.Binary;

namespace Nikse.SubtitleEdit.UiLogic.Media;

/// <summary>
/// Builds a 16-bit PCM wave file by adding short clips into a silent track at given start times,
/// in one pass over the output.
/// </summary>
/// <remarks>
/// Text to speech used to merge each line with an ffmpeg <c>amix</c> that read the whole
/// accumulated track and wrote a new full-length copy, so the work grew with lines × duration:
/// a 2-hour, 1,500-line project wrote terabytes to the temp folder and took hours (#16767).
/// Here the silent track is written once and each clip only touches its own region, so the
/// cost is the track length plus the clips' length. Overlapping clips are summed with clamping
/// to the 16-bit range, like <c>amix normalize=false</c> followed by the s16 encoder.
/// </remarks>
public sealed class PcmWaveMixer : IDisposable
{
    private const int HeaderSize = 44;
    private const int BytesPerSample = 2;

    private readonly FileStream _stream;
    private long _dataBytes;
    private bool _completed;

    public int SampleRate { get; }
    public int Channels { get; }
    public int BlockAlign => Channels * BytesPerSample;

    /// <summary>Creates <paramref name="fileName"/> as a silent track of <paramref name="durationSeconds"/>.</summary>
    public PcmWaveMixer(string fileName, int sampleRate, int channels, double durationSeconds)
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

        var frames = (long)Math.Round(Math.Max(0, durationSeconds) * sampleRate);
        _dataBytes = frames * BlockAlign;

        _stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1024 * 64);
        WriteHeader();

        // Extending the file zero-fills it, which is exactly the silent base track.
        _stream.SetLength(HeaderSize + _dataBytes);
    }

    /// <summary>Length of the track in seconds, including any growth from clips past the initial end.</summary>
    public double DurationSeconds => (double)_dataBytes / BlockAlign / SampleRate;

    /// <summary>
    /// Adds <paramref name="pcm"/> (interleaved 16-bit little-endian samples in this mixer's
    /// sample rate and channel count) to the track starting at <paramref name="startSeconds"/>.
    /// A clip running past the end extends the track; a negative start drops the clip's head.
    /// </summary>
    public void Mix(double startSeconds, ReadOnlySpan<byte> pcm)
    {
        ObjectDisposedException.ThrowIf(_completed, this);

        // A trailing partial frame would shift every later frame's channels - drop it.
        pcm = pcm[..(pcm.Length - pcm.Length % BlockAlign)];

        var startFrame = (long)Math.Round(startSeconds * SampleRate);
        if (startFrame < 0)
        {
            var skipBytes = -startFrame * BlockAlign;
            if (skipBytes >= pcm.Length)
            {
                return;
            }

            pcm = pcm[(int)skipBytes..];
            startFrame = 0;
        }

        if (pcm.IsEmpty)
        {
            return;
        }

        var offset = startFrame * BlockAlign;
        // Past the current end the base is silence, which the fresh buffer already holds.
        var existing = new byte[pcm.Length];
        if (offset < _dataBytes)
        {
            _stream.Position = HeaderSize + offset;
            _stream.ReadExactly(existing, 0, (int)Math.Min(pcm.Length, _dataBytes - offset));
        }

        for (var i = 0; i < pcm.Length; i += BytesPerSample)
        {
            var sum = BinaryPrimitives.ReadInt16LittleEndian(existing.AsSpan(i)) +
                      BinaryPrimitives.ReadInt16LittleEndian(pcm[i..]);
            BinaryPrimitives.WriteInt16LittleEndian(existing.AsSpan(i), (short)Math.Clamp(sum, short.MinValue, short.MaxValue));
        }

        if (offset > _dataBytes)
        {
            _stream.SetLength(HeaderSize + offset); // zero-filled gap up to the clip
        }

        _stream.Position = HeaderSize + offset;
        _stream.Write(existing, 0, existing.Length);
        _dataBytes = Math.Max(_dataBytes, offset + pcm.Length);
    }

    /// <summary>Writes the final header sizes and closes the file.</summary>
    public void Complete()
    {
        if (_completed)
        {
            return;
        }

        WriteHeader();
        _stream.Flush();
        _stream.Dispose();
        _completed = true;
    }

    public void Dispose()
    {
        if (!_completed)
        {
            _stream.Dispose();
            _completed = true;
        }
    }

    private void WriteHeader()
    {
        // RIFF sizes are 32-bit; a track over ~4 GB (6.7 h of 44.1 kHz stereo) gets the max
        // value, which ffmpeg and most players read as "until end of file".
        var dataSize = (uint)Math.Min(_dataBytes, uint.MaxValue - HeaderSize);
        Span<byte> header = stackalloc byte[HeaderSize];
        "RIFF"u8.CopyTo(header);
        BinaryPrimitives.WriteUInt32LittleEndian(header[4..], dataSize + HeaderSize - 8);
        "WAVE"u8.CopyTo(header[8..]);
        "fmt "u8.CopyTo(header[12..]);
        BinaryPrimitives.WriteUInt32LittleEndian(header[16..], 16);
        BinaryPrimitives.WriteUInt16LittleEndian(header[20..], 1); // PCM
        BinaryPrimitives.WriteUInt16LittleEndian(header[22..], (ushort)Channels);
        BinaryPrimitives.WriteUInt32LittleEndian(header[24..], (uint)SampleRate);
        BinaryPrimitives.WriteUInt32LittleEndian(header[28..], (uint)(SampleRate * BlockAlign));
        BinaryPrimitives.WriteUInt16LittleEndian(header[32..], (ushort)BlockAlign);
        BinaryPrimitives.WriteUInt16LittleEndian(header[34..], BytesPerSample * 8);
        "data"u8.CopyTo(header[36..]);
        BinaryPrimitives.WriteUInt32LittleEndian(header[40..], dataSize);
        _stream.Position = 0;
        _stream.Write(header);
    }
}
