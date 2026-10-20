using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.SubtitleFormats.Legacy;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Reading and writing an .srt file, the current <see cref="SubRip"/> against the pre-rewrite
/// <see cref="SubRipLegacy"/>. Note that opening a file runs the reader twice: format detection
/// calls <see cref="SubRip.IsMine"/>, which parses the whole thing, and the winning format is
/// then asked to parse it again.
///
///   dotnet run -c Release --project tests/benchmarks/UiBenchmarks.csproj -- --filter *SubRipBenchmarks*
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class SubRipBenchmarks
{
    private List<string> _canonicalLines = new();
    private List<string> _messyLines = new();
    private List<string> _damagedLines = new();
    private Subtitle _subtitle = new();

    [Params(1000, 10000)]
    public int Paragraphs { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _canonicalLines = MakeLines(Kind.Canonical);
        _messyLines = MakeLines(Kind.MessySeparators);
        _damagedLines = MakeLines(Kind.Damaged);
        _subtitle = new Subtitle();
        new SubRip().LoadSubtitle(_subtitle, _canonicalLines, "test.srt");
    }

    private enum Kind
    {
        Canonical,
        MessySeparators,
        Damaged,
    }

    private List<string> MakeLines(Kind kind)
    {
        var texts = new[]
        {
            "It was the best of times, it was the worst of times.",
            "<i>Are you coming with us?</i>",
            "- No.|- Then stay here and wait.",
            "1999 was a very long time ago.",
            "Hello.",
        };

        var lines = new List<string>(Paragraphs * 4);
        var startMs = 1000;
        for (var i = 0; i < Paragraphs; i++)
        {
            var endMs = startMs + 2000;
            switch (kind)
            {
                case Kind.Canonical:
                    lines.Add((i + 1).ToString());
                    lines.Add($"{Format(startMs)} --> {Format(endMs)}");
                    break;
                case Kind.MessySeparators:
                    // Separator and decimal-point variants the tolerant path has to normalize.
                    lines.Add((i + 1).ToString());
                    lines.Add($"{Format(startMs).Replace(',', '.')} -> {Format(endMs).Replace(',', '.')}");
                    break;
                default:
                    // Missing numbers, missing/extra blank lines, trailing spaces, odd arrows.
                    if (i % 3 != 0)
                    {
                        lines.Add((i + 1).ToString());
                    }

                    if (i % 4 == 0)
                    {
                        lines.Add(string.Empty);
                    }

                    lines.Add(i % 2 == 0
                        ? $"{Format(startMs)} --> {Format(endMs)}  "
                        : $"{Format(startMs)}—>{Format(endMs).Replace(',', ':')}");
                    break;
            }

            foreach (var textLine in texts[i % texts.Length].Split('|'))
            {
                lines.Add(textLine);
            }

            if (kind != Kind.Damaged || i % 5 != 0)
            {
                lines.Add(string.Empty);
            }

            if (kind == Kind.Damaged && i % 7 == 0)
            {
                lines.Add(string.Empty);
            }

            startMs = endMs + 100;
        }

        return lines;
    }

    private static string Format(int totalMs)
    {
        var ms = totalMs % 1000;
        var totalSeconds = totalMs / 1000;
        return $"{totalSeconds / 3600:00}:{totalSeconds / 60 % 60:00}:{totalSeconds % 60:00},{ms:000}";
    }

    private static int Load(SubtitleFormat format, List<string> lines)
    {
        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, lines, "test.srt");
        return subtitle.Paragraphs.Count;
    }

    [Benchmark(Baseline = true), BenchmarkCategory("LoadCanonical")]
    public int LoadCanonicalLegacy() => Load(new SubRipLegacy(), _canonicalLines);

    [Benchmark, BenchmarkCategory("LoadCanonical")]
    public int LoadCanonical() => Load(new SubRip(), _canonicalLines);

    [Benchmark(Baseline = true), BenchmarkCategory("LoadMessySeparators")]
    public int LoadMessySeparatorsLegacy() => Load(new SubRipLegacy(), _messyLines);

    [Benchmark, BenchmarkCategory("LoadMessySeparators")]
    public int LoadMessySeparators() => Load(new SubRip(), _messyLines);

    [Benchmark(Baseline = true), BenchmarkCategory("LoadDamaged")]
    public int LoadDamagedLegacy() => Load(new SubRipLegacy(), _damagedLines);

    [Benchmark, BenchmarkCategory("LoadDamaged")]
    public int LoadDamaged() => Load(new SubRip(), _damagedLines);

    [Benchmark(Baseline = true), BenchmarkCategory("IsMine")]
    public bool IsMineCanonicalLegacy() => new SubRipLegacy().IsMine(_canonicalLines, "test.srt");

    [Benchmark, BenchmarkCategory("IsMine")]
    public bool IsMineCanonical() => new SubRip().IsMine(_canonicalLines, "test.srt");

    [Benchmark(Baseline = true), BenchmarkCategory("ToText")]
    public int ToTextLegacy() => new SubRipLegacy().ToText(_subtitle, "test").Length;

    [Benchmark, BenchmarkCategory("ToText")]
    public int ToText() => new SubRip().ToText(_subtitle, "test").Length;

    /// <summary>
    /// The app's real load path: MainViewModel builds a SubtitleLineViewModel per paragraph and
    /// each one reads Paragraph.Id (SubtitleLineViewModel.cs:166). Kept as its own benchmark so a
    /// future change to how ids are minted shows up here as well as in LoadCanonical.
    /// </summary>
    [Benchmark, BenchmarkCategory("LoadAndReadIds")]
    public int LoadCanonicalAndReadIds()
    {
        var subtitle = new Subtitle();
        new SubRip().LoadSubtitle(subtitle, _canonicalLines, "test.srt");
        var total = 0;
        foreach (var p in subtitle.Paragraphs)
        {
            if (p.Id.HasValue)
            {
                total++;
            }
        }

        return total;
    }
}
