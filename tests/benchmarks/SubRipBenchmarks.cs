using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Reading an .srt file. Note that opening a file runs this twice: format detection calls
/// <see cref="SubRip.IsMine"/>, which parses the whole thing, and the winning format is then
/// asked to parse it again.
/// </summary>
[MemoryDiagnoser]
public class SubRipBenchmarks
{
    private List<string> _canonicalLines = new();
    private List<string> _messyLines = new();

    [Params(1000, 10000)]
    public int Paragraphs { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _canonicalLines = MakeLines(canonical: true);
        _messyLines = MakeLines(canonical: false);
    }

    private List<string> MakeLines(bool canonical)
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
            lines.Add((i + 1).ToString());
            lines.Add(canonical
                ? $"{Format(startMs)} --> {Format(endMs)}"
                // Separator and decimal-point variants the tolerant path has to normalize.
                : $"{Format(startMs).Replace(',', '.')} -> {Format(endMs).Replace(',', '.')}");

            foreach (var textLine in texts[i % texts.Length].Split('|'))
            {
                lines.Add(textLine);
            }

            lines.Add(string.Empty);
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

    [Benchmark]
    public int LoadCanonical()
    {
        var subtitle = new Subtitle();
        new SubRip().LoadSubtitle(subtitle, _canonicalLines, "test.srt");
        return subtitle.Paragraphs.Count;
    }

    [Benchmark]
    public bool IsMineCanonical() => new SubRip().IsMine(_canonicalLines, "test.srt");

    /// <summary>
    /// The app's real load path: MainViewModel builds a SubtitleLineViewModel per paragraph and
    /// each one reads Paragraph.Id (SubtitleLineViewModel.cs:166). Kept as its own benchmark so a
    /// future change to how ids are minted shows up here as well as in LoadCanonical.
    /// </summary>
    [Benchmark]
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

    [Benchmark]
    public int LoadMessySeparators()
    {
        var subtitle = new Subtitle();
        new SubRip().LoadSubtitle(subtitle, _messyLines, "test.srt");
        return subtitle.Paragraphs.Count;
    }
}
