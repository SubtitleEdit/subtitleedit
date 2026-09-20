using System.Buffers.Binary;

namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// A 3D Blu-ray "3D-Plane" (offset sequence) read from an OFS file: the depth of the subtitles for
/// every frame of the movie. On the disc the subtitle stream itself is flat; the player shifts it
/// by the depth stored in the MVC video, frame by frame. BD3D2MK3D and OFSExtractor extract those
/// depths to OFS files (the Scenarist format), one per 3D-Plane, and tsMuxeR shows which 3D-Plane
/// each subtitle track uses ("3d-plane: 2").
/// </summary>
/// <remarks>
/// Layout, after BD3D2MK3D's 3DPlanes2OFS: an 8 byte signature (0x89 "OFS" CR LF 0x1A LF), a 4 byte
/// version, a 16 byte GUID, one byte with the frame rate code in the high nibble and the drop
/// frame flag in the low one, the number of rolls and two reserved bytes. Each roll has a marker
/// byte, a 4 byte start time code, a 4 byte frame count and then one byte per frame: bit 7 is the
/// direction (set = behind the screen), bits 0-6 the offset in pixels, and 0x80 means no depth.
/// </remarks>
public sealed class Stereo3DPlane
{
    /// <summary>A frame without a depth - no subtitle is expected on it.</summary>
    public const byte UndefinedOffset = 0x80;

    /// <summary>The offsets are pixels of a 3D Blu-ray's 1920 wide view.</summary>
    public const int SourceWidth = 1920;

    private static readonly byte[] Signature = [0x89, 0x4F, 0x46, 0x53, 0x0D, 0x0A, 0x1A, 0x0A];
    private const int FrameRateIndex = 28;
    private const int NumberOfRollsIndex = 29;
    private const int FirstRollIndex = 32;
    private const int RollHeaderSize = 9;

    private readonly byte[] _offsets;

    private Stereo3DPlane(byte[] offsets, double framesPerSecond)
    {
        _offsets = offsets;
        FramesPerSecond = framesPerSecond;
    }

    public double FramesPerSecond { get; }

    public int FrameCount => _offsets.Length;

    /// <summary>Frames that have a depth.</summary>
    public int DefinedFrameCount => _offsets.Count(b => b != UndefinedOffset);

    public TimeSpan Duration => TimeSpan.FromSeconds(FrameCount / FramesPerSecond);

    /// <summary>The nearest and farthest depth in the file, or null when no frame has one.</summary>
    public (int Min, int Max)? GetDepthRange()
    {
        int? min = null;
        int? max = null;
        foreach (var b in _offsets)
        {
            if (b == UndefinedOffset)
            {
                continue;
            }

            var offset = ToOffset(b);
            min = Math.Min(min ?? offset, offset);
            max = Math.Max(max ?? offset, offset);
        }

        return min.HasValue && max.HasValue ? (min.Value, max.Value) : null;
    }

    /// <summary>
    /// The offset of a subtitle shown from <paramref name="start"/> to <paramref name="end"/>: the
    /// nearest to the viewer of the frames it covers, so an image that cannot follow the depth
    /// frame by frame is never pushed behind what the disc placed in front of the screen. Null
    /// when none of those frames has a depth.
    /// </summary>
    public int? GetOffset(TimeSpan start, TimeSpan end)
    {
        if (FrameCount == 0)
        {
            return null;
        }

        // A frame is shown when its time is within [start, end). Times are rounded to whole
        // milliseconds, so allow a little slack at the frame boundaries.
        const double slack = 0.05;
        var first = (int)Math.Ceiling(start.TotalSeconds * FramesPerSecond - slack);
        var last = (int)Math.Ceiling(end.TotalSeconds * FramesPerSecond - slack) - 1;
        first = Math.Max(0, first);
        last = Math.Min(FrameCount - 1, Math.Max(first, last));
        if (first >= FrameCount)
        {
            return null;
        }

        int? nearest = null;
        for (var i = first; i <= last; i++)
        {
            var b = _offsets[i];
            if (b != UndefinedOffset)
            {
                nearest = Math.Max(nearest ?? int.MinValue, ToOffset(b));
            }
        }

        return nearest;
    }

    public static Stereo3DPlane Load(string fileName)
    {
        return FromBytes(File.ReadAllBytes(fileName));
    }

    /// <exception cref="InvalidDataException">Not an OFS file, or cut short.</exception>
    public static Stereo3DPlane FromBytes(byte[] data)
    {
        if (data.Length < FirstRollIndex + RollHeaderSize || !data.AsSpan(0, Signature.Length).SequenceEqual(Signature))
        {
            throw new InvalidDataException("Not an OFS (3D-Plane) file");
        }

        var framesPerSecond = GetFramesPerSecond(data[FrameRateIndex] >> 4);

        // Some writers leave the number of rolls at zero - there is always at least one.
        var numberOfRolls = Math.Max(1, (int)data[NumberOfRollsIndex]);
        var offsets = new List<byte>();
        var position = FirstRollIndex;
        for (var roll = 0; roll < numberOfRolls && position + RollHeaderSize <= data.Length; roll++)
        {
            position += 5; // marker bits and start time code
            var count = GetFrameCount(data, ref position, isLastRoll: roll == numberOfRolls - 1);
            offsets.AddRange(data.AsSpan(position, count).ToArray());
            position += count;
        }

        return new Stereo3DPlane(offsets.ToArray(), framesPerSecond);
    }

    /// <summary>
    /// Reads the frame count at <paramref name="position"/> and moves past it. Writers disagree on
    /// its byte order, and OFSExtractor also writes an 8 byte length before the offsets, so the
    /// count that matches the file size wins.
    /// </summary>
    private static int GetFrameCount(byte[] data, ref int position, bool isLastRoll)
    {
        long bigEndian = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(position));
        long littleEndian = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(position));
        position += 4;
        var remaining = data.Length - position;

        if (isLastRoll)
        {
            if (bigEndian == remaining || littleEndian == remaining)
            {
                return remaining;
            }

            if (remaining >= 8 && (bigEndian == remaining - 8 || littleEndian == remaining - 8) &&
                BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(position)) == (ulong)(remaining - 8))
            {
                position += 8;
                return remaining - 8;
            }
        }

        if (bigEndian <= remaining)
        {
            return (int)bigEndian;
        }

        if (littleEndian <= remaining)
        {
            return (int)littleEndian;
        }

        throw new InvalidDataException("OFS (3D-Plane) file is cut short");
    }

    /// <summary>The Blu-ray frame rate codes; 3D Blu-rays are 23.976 fps, so that is the fallback.</summary>
    private static double GetFramesPerSecond(int frameRateCode)
    {
        return frameRateCode switch
        {
            2 => 24.0,
            3 => 25.0,
            4 => 30000.0 / 1001.0,
            6 => 50.0,
            7 => 60000.0 / 1001.0,
            _ => 24000.0 / 1001.0,
        };
    }

    private static int ToOffset(byte b)
    {
        var value = b & 0x7F;
        return (b & 0x80) == 0 ? value : -value;
    }
}
