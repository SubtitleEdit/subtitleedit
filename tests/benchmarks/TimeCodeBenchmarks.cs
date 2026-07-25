using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// TimeCode's display and parse paths. The ToString family runs for every start/end/duration/gap
/// cell the subtitle grid repaints and for the waveform paragraph labels; ParseToMilliseconds is
/// the shared parse entry for many format importers, called per line while loading.
/// </summary>
[MemoryDiagnoser]
public class TimeCodeBenchmarks
{
    private readonly TimeCode _tc = new TimeCode(1, 23, 45, 678);
    private readonly TimeCode _short = new TimeCode(0, 0, 2, 500);
    private readonly TimeCode _negative = new TimeCode(-90_000);

    [Benchmark] public string ToStringInvariant() => _tc.ToString(false);
    [Benchmark] public string ToStringLocalized() => _tc.ToString(true);
    [Benchmark] public string ToShortStringLocalized() => _short.ToShortString(true);
    [Benchmark] public string ToStringNegative() => _negative.ToString(false);
    [Benchmark] public string ToHHMMSSFF() => _tc.ToHHMMSSFF();
    [Benchmark] public string ToShortStringHHMMSSFF() => _tc.ToShortStringHHMMSSFF();

    /// <summary>Every component getter builds its own TimeSpan from TotalMilliseconds.</summary>
    [Benchmark]
    public int ReadAllComponents() => _tc.Hours + _tc.Minutes + _tc.Seconds + _tc.Milliseconds;

    /// <summary>What SubRip's ms-as-frames fixup does per paragraph: read then write.</summary>
    [Benchmark]
    public double RoundTripMilliseconds()
    {
        var tc = new TimeCode(3_723_456);
        tc.Milliseconds = tc.Milliseconds;
        return tc.TotalMilliseconds;
    }

    [Benchmark] public double ParseToMilliseconds() => TimeCode.ParseToMilliseconds("01:23:45,678");
    [Benchmark] public double ParseHHMMSSFF() => TimeCode.ParseHHMMSSFFToMilliseconds("01:23:45:12");
}
