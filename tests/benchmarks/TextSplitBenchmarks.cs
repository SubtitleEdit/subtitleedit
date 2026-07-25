using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Auto line-breaking. TextSplit builds a candidate split at every space in the line and measures
/// both of its lines with Skia, so the cost scales with the number of spaces - and this runs per
/// line in "auto balance lines", fix-common-errors and several import paths.
/// </summary>
[MemoryDiagnoser]
public class TextSplitBenchmarks
{
    private const string Short = "Are you coming with us tonight?";
    private const string Medium = "It was the best of times, it was the worst of times, it was the age of wisdom.";
    private const string Long = "It was the best of times, it was the worst of times, it was the age of wisdom, "
                                + "it was the age of foolishness, it was the epoch of belief, it was the epoch of incredulity.";
    private const string Dialog = "- Are you coming with us? - No, I think I will stay right here and wait for them.";

    [Benchmark] public string AutoBreak_Short() => Utilities.AutoBreakLine(Short, "en");
    [Benchmark] public string AutoBreak_Medium() => Utilities.AutoBreakLine(Medium, "en");
    [Benchmark] public string AutoBreak_Long() => Utilities.AutoBreakLine(Long, "en");
    [Benchmark] public string AutoBreak_Dialog() => Utilities.AutoBreakLine(Dialog, "en");

    /// <summary>Just the candidate construction, without the ordering that follows it.</summary>
    [Benchmark]
    public object BuildSplitsOnly() => new TextSplit(Long, 43, "en");
}
