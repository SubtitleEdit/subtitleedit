using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

/// <summary>
/// The presentation time of every picture in a video stream, plus which of them are key frames.
/// Built once per file by <see cref="FfmpegFrameIndexer"/> from the packet time stamps (no
/// decoding), so "the frame before this one" and "the key frame nearest to here" are lookups
/// instead of guesses from the average frame rate - which is wrong for variable frame rate files
/// and drifts off the real frames at rates like 23.976.
/// <para>
/// Times are seconds from the start of the file, the same clock as <see cref="VideoFrame.Pts"/>.
/// The raw time stamps (stream time base) are kept as well, so a seek can ask libavformat for
/// exactly the key frame it wants without a rounding step in between.
/// </para>
/// </summary>
public sealed class FfmpegFrameIndex
{
    private readonly double[] _seconds; // ascending
    private readonly long[] _ticks; // same order, stream time base
    private readonly int[] _keyFrames; // indexes into _seconds, ascending

    /// <summary>Half the smallest distance between two neighbouring frames; pictures closer than this are the same frame.</summary>
    public double MatchTolerance { get; }

    /// <summary>
    /// How far before a frame's start a time still counts as that frame: subtitle times are whole
    /// milliseconds, so the time of a frame at 41.708 ms arrives as 41 or 42.
    /// </summary>
    private double RoundingSlack => Math.Min(0.001, MatchTolerance);

    public int Count => _seconds.Length;
    public int KeyFrameCount => _keyFrames.Length;

    private FfmpegFrameIndex(double[] seconds, long[] ticks, int[] keyFrames)
    {
        _seconds = seconds;
        _ticks = ticks;
        _keyFrames = keyFrames;

        var smallest = double.MaxValue;
        for (var i = 1; i < seconds.Length; i++)
        {
            smallest = Math.Min(smallest, seconds[i] - seconds[i - 1]);
        }

        MatchTolerance = smallest == double.MaxValue ? 0.001 : Math.Clamp(smallest / 2, 0.0002, 0.02);
    }

    /// <summary>
    /// Builds the index from packets in any order (B-frames put them in decode order). Pictures
    /// sharing a time stamp are kept once; a key frame flag on any of them wins.
    /// </summary>
    public static FfmpegFrameIndex Create(IReadOnlyList<long> ticks, IReadOnlyList<bool> keyFrames, double secondsPerTick, double startTimeSeconds)
    {
        var count = Math.Min(ticks.Count, keyFrames.Count);
        var order = new int[count];
        for (var i = 0; i < count; i++)
        {
            order[i] = i;
        }

        Array.Sort(order, (a, b) => ticks[a].CompareTo(ticks[b]));

        var seconds = new List<double>(count);
        var sortedTicks = new List<long>(count);
        var keys = new List<int>();
        var lastIsKey = false;
        foreach (var source in order)
        {
            var tick = ticks[source];
            if (sortedTicks.Count > 0 && sortedTicks[^1] == tick)
            {
                if (keyFrames[source] && !lastIsKey)
                {
                    keys.Add(sortedTicks.Count - 1);
                    lastIsKey = true;
                }

                continue;
            }

            sortedTicks.Add(tick);
            seconds.Add(tick * secondsPerTick - startTimeSeconds);
            lastIsKey = keyFrames[source];
            if (lastIsKey)
            {
                keys.Add(sortedTicks.Count - 1);
            }
        }

        return new FfmpegFrameIndex(seconds.ToArray(), sortedTicks.ToArray(), keys.ToArray());
    }

    /// <summary>Presentation time of frame <paramref name="index"/> in seconds.</summary>
    public double SecondsAt(int index) => _seconds[index];

    /// <summary>Presentation time of frame <paramref name="index"/> in the stream's time base.</summary>
    public long TicksAt(int index) => _ticks[index];

    /// <summary>Index of the frame closest to <paramref name="seconds"/>, or -1 for an empty index.</summary>
    public int NearestIndex(double seconds)
    {
        if (_seconds.Length == 0)
        {
            return -1;
        }

        var after = LowerBound(seconds);
        if (after == 0)
        {
            return 0;
        }

        if (after == _seconds.Length)
        {
            return _seconds.Length - 1;
        }

        return seconds - _seconds[after - 1] <= _seconds[after] - seconds ? after - 1 : after;
    }

    /// <summary>
    /// Index of the frame on screen at <paramref name="seconds"/>: the last one that starts at or
    /// before it (with a millisecond of slack, so a time rounded to milliseconds still finds its
    /// own frame). -1 before the first frame.
    /// </summary>
    public int IndexAtOrBefore(double seconds)
    {
        return UpperBound(seconds + RoundingSlack) - 1;
    }

    /// <summary>
    /// The frame one step away from the one shown at <paramref name="seconds"/>, or -1 when there
    /// is none in that direction. A time between two frames counts as the earlier one, the frame
    /// that is on screen then.
    /// </summary>
    public int NeighbourIndex(double seconds, bool forward)
    {
        if (_seconds.Length == 0)
        {
            return -1;
        }

        var current = IndexAtOrBefore(seconds);
        if (forward)
        {
            return current + 1 < _seconds.Length ? current + 1 : -1;
        }

        if (current < 0)
        {
            return -1;
        }

        // Standing between two frames, "back" first goes to the start of the one on screen.
        if (seconds - _seconds[current] > RoundingSlack)
        {
            return current;
        }

        return current - 1;
    }

    /// <summary>True when a frame of the index starts at <paramref name="seconds"/> (within <see cref="MatchTolerance"/>).</summary>
    public bool Contains(double seconds)
    {
        var nearest = NearestIndex(seconds);
        return nearest >= 0 && Math.Abs(_seconds[nearest] - seconds) <= MatchTolerance;
    }

    /// <summary>The last key frame at or before frame <paramref name="index"/> (frame index), or -1.</summary>
    public int KeyFrameAtOrBefore(int index)
    {
        var position = UpperBound(_keyFrames, index) - 1;
        return position >= 0 ? _keyFrames[position] : -1;
    }

    /// <summary>The key frame closest in time to <paramref name="seconds"/> (frame index), or -1 without key frames.</summary>
    public int NearestKeyFrame(double seconds)
    {
        if (_keyFrames.Length == 0)
        {
            return -1;
        }

        var frame = NearestIndex(seconds);
        var position = UpperBound(_keyFrames, frame);
        if (position == 0)
        {
            return _keyFrames[0];
        }

        if (position == _keyFrames.Length)
        {
            return _keyFrames[^1];
        }

        var before = _keyFrames[position - 1];
        var after = _keyFrames[position];
        return seconds - _seconds[before] <= _seconds[after] - seconds ? before : after;
    }

    /// <summary>Presentation times of the key frames, in seconds.</summary>
    public double[] KeyFrameSeconds()
    {
        var result = new double[_keyFrames.Length];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = _seconds[_keyFrames[i]];
        }

        return result;
    }

    /// <summary>
    /// True when every frame lasts the same (within a millisecond), i.e. a frame number times a
    /// fixed duration describes the file. False for variable frame rate.
    /// </summary>
    public bool IsConstantFrameRate
    {
        get
        {
            if (_seconds.Length < 3)
            {
                return true;
            }

            var expected = (_seconds[^1] - _seconds[0]) / (_seconds.Length - 1);
            for (var i = 1; i < _seconds.Length; i++)
            {
                if (Math.Abs(_seconds[i] - _seconds[i - 1] - expected) > 0.001)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>First position whose time is >= <paramref name="seconds"/>.</summary>
    private int LowerBound(double seconds)
    {
        var low = 0;
        var high = _seconds.Length;
        while (low < high)
        {
            var middle = (low + high) >>> 1;
            if (_seconds[middle] < seconds)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }

        return low;
    }

    /// <summary>First position whose time is > <paramref name="seconds"/>.</summary>
    private int UpperBound(double seconds)
    {
        var low = 0;
        var high = _seconds.Length;
        while (low < high)
        {
            var middle = (low + high) >>> 1;
            if (_seconds[middle] <= seconds)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }

        return low;
    }

    /// <summary>First position whose value is > <paramref name="value"/>.</summary>
    private static int UpperBound(int[] sorted, int value)
    {
        var low = 0;
        var high = sorted.Length;
        while (low < high)
        {
            var middle = (low + high) >>> 1;
            if (sorted[middle] <= value)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }

        return low;
    }
}
