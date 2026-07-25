using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Per-line helpers from Utilities. RemoveUnneededSpaces runs on every line the "auto trim white
/// space" setting touches and throughout fix-common-errors; GetMaxLineLength and GetNumberOfLines
/// back grid sorting and error checks.
/// </summary>
[MemoryDiagnoser]
public class UtilitiesBenchmarks
{
    private const string Plain = "It was the best of times, it was the worst of times.";
    private const string TwoLines = "- Are you coming with us?\r\n- No, I'll stay here and wait.";
    private const string Tagged = "<i>It was the best of times,</i>\r\n<i>it was the worst of times.</i>";
    private const string Dots = "Well ... I don't know ...\r\n<i>... maybe later .</i>";
    private const string Long = "It was the best of times, it was the worst of times, it was the age of wisdom, it was the age of foolishness.";

    [Benchmark] public string RemoveUnneededSpaces_Plain() => Utilities.RemoveUnneededSpaces(Plain, "en");
    [Benchmark] public string RemoveUnneededSpaces_TwoLines() => Utilities.RemoveUnneededSpaces(TwoLines, "en");
    [Benchmark] public string RemoveUnneededSpaces_Dots() => Utilities.RemoveUnneededSpaces(Dots, "en");
    [Benchmark] public string RemoveUnneededSpaces_French() => Utilities.RemoveUnneededSpaces(Dots, "fr");

    [Benchmark] public int GetMaxLineLength_Plain() => Utilities.GetMaxLineLength(Plain);
    [Benchmark] public int GetMaxLineLength_Tagged() => Utilities.GetMaxLineLength(Tagged);
    [Benchmark] public int GetNumberOfLines() => Utilities.GetNumberOfLines(TwoLines);

    [Benchmark] public string AutoBreakLine() => Utilities.AutoBreakLine(Long);
    [Benchmark] public string RemoveLineBreaks() => Utilities.RemoveLineBreaks(Tagged);
    [Benchmark] public string RemoveSsaTags() => Utilities.RemoveSsaTags("{\\an8}{\\pos(10,20)}Hello there");
    [Benchmark] public double GetOptimalDisplayMilliseconds() => Utilities.GetOptimalDisplayMilliseconds(Plain);
}
