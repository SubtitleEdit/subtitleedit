using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// ContinuationUtilities.SanitizeString runs several times per paragraph pair in the
/// continuation-style and unnecessary-leading-dots fixes.
/// </summary>
[MemoryDiagnoser]
public class ContinuationUtilitiesBenchmarks
{
    private const string Plain = "It was the best of times, it was the worst of times";
    private const string Tagged = "<i>It was the best of times,</i>" + "\r\n" + "(sighs) it was the worst of times [groans]";

    [Benchmark] public string SanitizeString_Plain() => ContinuationUtilities.SanitizeString(Plain);
    [Benchmark] public string SanitizeString_Tagged() => ContinuationUtilities.SanitizeString(Tagged);
}
