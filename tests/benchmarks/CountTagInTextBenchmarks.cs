using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Utilities.CountTagInText(string, char): the net8+ build uses MemoryExtensions.Count (one
/// vectorized pass), the netstandard2.1 build keeps the original IndexOf loop, which re-enters
/// the search once per hit. "Loop" below is a verbatim copy of that netstandard path; "Span"
/// calls the real Utilities method (this host is net10.0).
/// Inputs are typical subtitle lines: a quote-heavy dialog line, a no-match line, and a long
/// ASSA-tagged line.
/// </summary>
[MemoryDiagnoser]
public class CountTagInTextBenchmarks
{
    private const string NoTags = "It was the best of times, it was the worst of times.";
    private const string Quotes = "\"Are you coming?\" she asked.\r\n\"No,\" he said. \"I'll stay.\"";
    private const string LongAssa = "{\\an8}{\\pos(10,20)}It was the best of times, it was the worst of times, it was the age of wisdom, it was the age of foolishness, it was the epoch of belief, it was the epoch of incredulity.";

    [Benchmark(Baseline = true)] public int Loop_Quotes() => CountLoop(Quotes, '"');
    [Benchmark] public int Span_Quotes() => Utilities.CountTagInText(Quotes, '"');

    [Benchmark] public int Loop_NoMatch() => CountLoop(NoTags, '"');
    [Benchmark] public int Span_NoMatch() => Utilities.CountTagInText(NoTags, '"');

    [Benchmark] public int Loop_LongBraces() => CountLoop(LongAssa, '{');
    [Benchmark] public int Span_LongBraces() => Utilities.CountTagInText(LongAssa, '{');

    // Verbatim copy of the netstandard2.1 branch of Utilities.CountTagInText(string, char).
    private static int CountLoop(string text, char tag)
    {
        int count = 0;
        int index = text.IndexOf(tag);
        while (index >= 0)
        {
            count++;
            if ((index + 1) == text.Length)
            {
                return count;
            }

            index = text.IndexOf(tag, index + 1);
        }
        return count;
    }
}
