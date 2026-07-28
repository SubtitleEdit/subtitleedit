using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Per-line ASSA tag strippers used by batch convert and image export. The "Plain" cases are
/// the common path: ordinary subtitle text that cannot contain the tags at all.
/// </summary>
[MemoryDiagnoser]
public class HtmlUtilAssaTagBenchmarks
{
    private const string Plain = "It was the best of times, it was the worst of times.";
    private const string Aligned = "{\\an8}It was the best of times, it was the worst of times.";
    private const string Colored = "{\\c&HFF0000&}It was the best of times,{\\c} it was the worst of times.";

    [Benchmark] public string RemoveAssAlignmentTags_Plain() => HtmlUtil.RemoveAssAlignmentTags(Plain);
    [Benchmark] public string RemoveAssAlignmentTags_Aligned() => HtmlUtil.RemoveAssAlignmentTags(Aligned);
    [Benchmark] public string RemoveAssaColor_Plain() => HtmlUtil.RemoveAssaColor(Plain);
    [Benchmark] public string RemoveAssaColor_Colored() => HtmlUtil.RemoveAssaColor(Colored);
}
