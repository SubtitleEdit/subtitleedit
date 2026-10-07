using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Line counting as the Netflix check, sort-by-number-of-lines, the teletext line-count watch and
/// the translate merge/split helper do it: <c>SplitToLines().Count</c> allocates a list plus one
/// substring per line only to read the count; <c>CountLines()</c> scans the same breaks and
/// allocates nothing.
/// </summary>
[MemoryDiagnoser]
public class CountLinesBenchmarks
{
    private string _text = string.Empty;

    [Params("OneLine", "TwoLines", "ThreeLines", "Long")]
    public string Shape { get; set; } = "TwoLines";

    [GlobalSetup]
    public void Setup()
    {
        var nl = Environment.NewLine;
        _text = Shape switch
        {
            "OneLine" => "It was the best of times, it was the worst of times.",
            "TwoLines" => "- Are you coming with us?" + nl + "- No, I will stay right here.",
            "ThreeLines" => "<i>It was the best of times,</i>" + nl + "it was the worst of times," + nl + "it was the age of wisdom.",
            _ => string.Join(nl, Enumerable.Repeat("It was the best of times, it was the worst of times.", 50)),
        };
    }

    [Benchmark(Baseline = true)]
    public int SplitToLinesCount() => _text.SplitToLines().Count;

    [Benchmark]
    public int CountLines() => _text.CountLines();
}
