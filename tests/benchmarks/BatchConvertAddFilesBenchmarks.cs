using System.Collections.ObjectModel;
using System.Text;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace Nikse.SubtitleEdit.Benchmarks;

// Batch convert "Add files" / "Add folder" (#15742). The old code parsed one file at a time and
// posted two UI updates per file; BatchConvertFileLoader parses with a few workers (format
// detection stays serialized - the SubtitleFormat instances are shared) and posts one UI update
// per ~100 ms.

/// <summary>Builds the temp folders the add-files benchmarks parse.</summary>
internal static class BatchConvertAddFilesData
{
    public static string FindFixturesDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var fixtures = Path.Combine(dir.FullName, "tests", "seconv", "Fixtures");
            if (Directory.Exists(fixtures))
            {
                return fixtures;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("tests/seconv/Fixtures not found above " + AppContext.BaseDirectory);
    }

    /// <summary>"Text": 300 subtitle files (SubRip/ASSA/WebVTT, 800 lines). "Containers": 20 copies each of the
    /// MKV/MP4/TS/SUP fixtures. "Mixed": both.</summary>
    public static List<string> Create(string mix, out DirectoryInfo dir)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        dir = Directory.CreateTempSubdirectory("se-batch-add-bench");
        var fileNames = new List<string>();

        if (mix is "Text" or "Mixed")
        {
            var subtitle = new Subtitle();
            for (var i = 0; i < 800; i++)
            {
                subtitle.Paragraphs.Add(new Paragraph($"Line number {i} of the subtitle\nand a <i>second</i> line.", i * 3000, i * 3000 + 2000));
            }

            var formats = new SubtitleFormat[] { new SubRip(), new AdvancedSubStationAlpha(), new WebVTT() };
            for (var i = 0; i < 300; i++)
            {
                var format = formats[i % formats.Length];
                var fileName = Path.Combine(dir.FullName, $"text{i:000}{format.Extension}");
                File.WriteAllText(fileName, format.ToText(subtitle, "bench"));
                fileNames.Add(fileName);
            }
        }

        if (mix is "Containers" or "Mixed")
        {
            var fixtures = FindFixturesDirectory();
            var containers = new[] { "container_text.mkv", "container_image.mkv", "container_text_undeclared_lang.mkv", "container_text.mp4", "container_cea608_708.mp4", "container_teletext.ts", "sample.sup" };
            for (var copy = 0; copy < 20; copy++)
            {
                foreach (var name in containers)
                {
                    var fileName = Path.Combine(dir.FullName, $"c{copy:00}_{name}");
                    File.Copy(Path.Combine(fixtures, name), fileName);
                    fileNames.Add(fileName);
                }
            }
        }

        return fileNames;
    }

    // Both keep every item (with its parsed Subtitle), as the batch convert list does - the
    // retained subtitles are a large part of the GC cost of adding text files.

    /// <summary>The old AddFilesAsync loop, minus the UI posts.</summary>
    public static int Sequential(List<string> fileNames, Func<string, IReadOnlyList<BatchConvertItem>> parse)
    {
        var all = new List<BatchConvertItem>();
        foreach (var fileName in fileNames)
        {
            all.AddRange(parse(fileName));
        }

        return all.Count;
    }

    public static int Queue(List<string> fileNames, Func<string, IReadOnlyList<BatchConvertItem>> parse, int workers)
    {
        var all = new List<BatchConvertItem>();
        BatchConvertFileLoader.Load(fileNames, parse, workers, BatchConvertFileLoader.DefaultFlushInterval,
            p => all.AddRange(p.NewItems), CancellationToken.None);
        return all.Count;
    }
}

/// <summary>
/// Real <c>AddFile</c> on local files (warm OS cache): the CPU side of adding files. Text files
/// mostly serialize on the format-detection lock; containers parse fully in parallel.
/// </summary>
[MemoryDiagnoser]
public class BatchConvertAddFilesBenchmarks
{
    private List<string> _fileNames = null!;
    private DirectoryInfo _dir = null!;

    [Params("Text", "Containers", "Mixed")]
    public string Mix { get; set; } = "Mixed";

    [GlobalSetup]
    public void Setup()
    {
        _fileNames = BatchConvertAddFilesData.Create(Mix, out _dir);
        BatchConvertAddFilesData.Sequential(_fileNames, BatchConvertViewModel.AddFile); // warm the OS cache + format list
    }

    [GlobalCleanup]
    public void Cleanup() => _dir.Delete(recursive: true);

    [Benchmark(Baseline = true)]
    public int Sequential() => BatchConvertAddFilesData.Sequential(_fileNames, BatchConvertViewModel.AddFile);

    [Benchmark]
    public int Queue1() => BatchConvertAddFilesData.Queue(_fileNames, BatchConvertViewModel.AddFile, 1);

    [Benchmark]
    public int Queue2() => BatchConvertAddFilesData.Queue(_fileNames, BatchConvertViewModel.AddFile, 2);

    [Benchmark]
    public int Queue4() => BatchConvertAddFilesData.Queue(_fileNames, BatchConvertViewModel.AddFile, 4);

    [Benchmark]
    public int Queue8() => BatchConvertAddFilesData.Queue(_fileNames, BatchConvertViewModel.AddFile, 8);
}

/// <summary>
/// Slow storage (network share, USB/HDD): the real <c>AddFile</c> on the container files plus a
/// simulated per-file I/O wait outside the format-detection lock - where the MKV/MP4/TS reads are.
/// </summary>
public class BatchConvertAddFilesSlowStorageBenchmarks
{
    private List<string> _fileNames = null!;
    private DirectoryInfo _dir = null!;

    [Params(2, 10)]
    public int LatencyMs { get; set; }

    [GlobalSetup]
    public void Setup() => _fileNames = BatchConvertAddFilesData.Create("Containers", out _dir);

    [GlobalCleanup]
    public void Cleanup() => _dir.Delete(recursive: true);

    private IReadOnlyList<BatchConvertItem> SlowAddFile(string fileName)
    {
        Thread.Sleep(LatencyMs);
        return BatchConvertViewModel.AddFile(fileName);
    }

    [Benchmark(Baseline = true)]
    public int Sequential() => BatchConvertAddFilesData.Sequential(_fileNames, SlowAddFile);

    [Benchmark]
    public int Queue4() => BatchConvertAddFilesData.Queue(_fileNames, SlowAddFile, 4);

    [Benchmark]
    public int Queue8() => BatchConvertAddFilesData.Queue(_fileNames, SlowAddFile, 8);
}

/// <summary>
/// The UI-thread side: what each post did with the parsed items - append to the all-items list
/// and the visible collection, then rebuild the "N files" info, which scans every item. Per file
/// (old) that scan makes adding N files O(N²); batched it runs once per ~100 ms flush. Does not
/// include the grid's own layout work, which batching also cuts (one layout per flush).
/// </summary>
public class BatchConvertAddFilesUiUpdateBenchmarks
{
    private List<BatchConvertItem> _items = null!;

    [Params(1000, 10000)]
    public int Files { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _items = Enumerable.Range(0, Files)
            .Select(i => new BatchConvertItem($"/folder/file{i}.srt", 50_000, "SubRip", null))
            .ToList();
    }

    private static string MakeInfo(List<BatchConvertItem> all)
    {
        // as MakeBatchItemsInfo
        var hasTransportStream = all.Any(p => p.Format != null && p.Format.StartsWith("Transport Stream", StringComparison.Ordinal));
        return hasTransportStream + " " + all.Count;
    }

    private int Run(int batchSize)
    {
        var all = new List<BatchConvertItem>();
        var visible = new ObservableCollection<BatchConvertItem>();
        var changes = 0;
        visible.CollectionChanged += (_, _) => changes++;
        var info = string.Empty;
        for (var start = 0; start < _items.Count; start += batchSize)
        {
            var batch = _items.GetRange(start, Math.Min(batchSize, _items.Count - start));
            all.AddRange(batch);
            foreach (var item in batch)
            {
                visible.Add(item);
            }

            info = MakeInfo(all);
        }

        return info.Length + changes;
    }

    [Benchmark(Baseline = true)]
    public int PerFile() => Run(1);

    // ~100 ms of parsing at the measured ~1-2 ms per text file
    [Benchmark]
    public int Batched100() => Run(100);
}
