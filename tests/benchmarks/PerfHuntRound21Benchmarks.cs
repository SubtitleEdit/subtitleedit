using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System.Text;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Round 21 performance hunt, outside the subtitle formats: translation pre-processing,
/// the find/replace service, the statistics word table, the names list, the user word list,
/// lenient file-name matching, CEA-608 character decode and bulk paragraph removal. Public entry points, plus self-contained old/new copies where the
/// method is private (statistics); <see cref="SubRipControl"/> is the drift control.
///
/// Default job, in-process, Apple M4, .NET 10, quiet machine (SubRipControl 80.6 us -> 78.3 us,
/// identical allocations):
///
///   NameList.ContainsCaseInsensitive (400)   1,475 us -> 2.6 us    570x
///   Statistics word table (5000 entries)    16,108 us -> 129 us    125x   311 MB -> 336 KB allocated
///   PreTranslate, English (400 lines)        7,232 us -> 252 us    29x    28.7 MB -> 339 KB
///   LoadUserWordList (5000 words)           20,204 us -> 931 us    22x
///   Lenient file names (400)                 1,195 us -> 118 us    10x    578 KB -> 50 KB
///   CEA-608 char for byte (400 x 96)           137 us -> 21 us     6.5x   922 KB -> 0
///   FindService.ReplaceAll (400 lines)         106 us -> 47 us     2.25x  1.2 MB -> 166 KB
///   RemoveParagraphsByIndices (5000 -> 2500)  1,456 us -> 1,154 us 1.26x  (includes the subtitle copy)
///   Formatting.SetTagsAndReturnTrimmed (eu)    129 us -> 103 us    1.25x
/// </summary>
[MemoryDiagnoser]
public class PerfHuntRound21Benchmarks
{
    private Subtitle _taggedSubtitle = new();
    private string[] _contractionLines = Array.Empty<string>();
    private string[] _twoLineTexts = Array.Empty<string>();
    private List<string> _replaceLines = new();
    private NameList _names = null!;
    private string[] _nameProbes = Array.Empty<string>();
    private string[] _fileNames = Array.Empty<string>();
    private Subtitle _bigSubtitle = new();
    private List<int> _removeIndices = new();
    private SortedDictionary<string, string> _wordTable = new();

    [GlobalSetup]
    public void Setup()
    {
        _taggedSubtitle = BuildTaggedSubtitle(400);
        _contractionLines = Enumerable.Range(0, 400).Select(i => (i % 3) switch
        {
            0 => $"I'm sure you're right, it's what they're after {i}.{Environment.NewLine}Don't you think? 'Cause I've seen it.",
            1 => $"She's here and he's there, who's counting {i}?",
            _ => $"No contractions in this line at all, number {i}.",
        }).ToArray();
        _twoLineTexts = Enumerable.Range(0, 400).Select(i => $"<i>The quick brown fox number {i}</i>{Environment.NewLine}jumps over the lazy dog.").ToArray();
        _replaceLines = Enumerable.Range(0, 400).Select(i => $"fox fox fox, the fox {i} chased a fox and another fox until the fox slept fox").ToList();

        var dictionaries = FindDictionariesFolder();
        _names = new NameList(dictionaries, "en", false, string.Empty);
        _nameProbes = new[] { "john", "MARY", "jean-luc", "zzznotaname", "new york", "Sherlock holmes", "nobody here", "smith" };

        _fileNames = Enumerable.Range(0, 400).Select(i => (i % 4) switch
        {
            0 => $"/videos/Show.S01E{i:00} - Copy (2).mkv",
            1 => $"/videos/Show.S01E{i:00} - kopia.mkv",
            2 => $"/videos/Show.S01E{i:00} (3).mkv",
            _ => $"/videos/Show.S01E{i:00}.mkv",
        }).ToArray();

        _bigSubtitle = BuildTaggedSubtitle(5000);
        _removeIndices = Enumerable.Range(0, 5000).Where(i => i % 2 == 0).ToList();

        for (var i = 0; i < 5000; i++)
        {
            var count = 2 + i % 40;
            _wordTable.Add($"{count:0000}_word{i}", count + ": word" + i);
        }

        var dir = Path.Combine(Path.GetTempPath(), "se-perf-round21", "Dictionaries");
        Directory.CreateDirectory(dir);
        var xml = new StringBuilder("<words>");
        for (var i = 0; i < 5000; i++)
        {
            xml.Append("<word>userword").Append(i).Append("</word>");
        }

        xml.Append("</words>");
        File.WriteAllText(Path.Combine(dir, "en_user.xml"), xml.ToString());
        Configuration.DataDirectory = Path.Combine(Path.GetTempPath(), "se-perf-round21") + Path.DirectorySeparatorChar;

        AssertEquivalence();
    }

    private static string FindDictionariesFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "Dictionaries");
            if (File.Exists(Path.Combine(candidate, "names.xml")))
            {
                return candidate + Path.DirectorySeparatorChar;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Dictionaries/names.xml not found above " + AppContext.BaseDirectory);
    }

    private static string Sentence(int i) =>
        $"The quick brown fox number {i} jumps over the lazy dog, twice, and then rests.";

    private static Subtitle BuildTaggedSubtitle(int count)
    {
        var s = new Subtitle();
        for (var i = 0; i < count; i++)
        {
            var text = (i % 4) switch
            {
                0 => $"<i>{Sentence(i)}</i>{Environment.NewLine}Second line of cue {i}.",
                1 => $"He said: <b>no</b>.{Environment.NewLine}<font color=\"#ffff00\">Yellow {i}</font> and <u>more</u>.",
                2 => $"{Sentence(i)}{Environment.NewLine}<i>Whispered</i> reply.",
                _ => Sentence(i),
            };
            s.Paragraphs.Add(new Paragraph(text, i * 3000, i * 3000 + 2500));
        }

        return s;
    }

    private void AssertEquivalence()
    {
        if (WordTableOld(_wordTable) != WordTableNew(_wordTable))
        {
            throw new Exception("statistics word table old/new differ");
        }

        var expected = new List<string>();
        foreach (var line in _replaceLines)
        {
            expected.Add(line.Replace("fox", "cat", StringComparison.OrdinalIgnoreCase));
        }

        var service = new FindService();
        var lines = new List<string>(_replaceLines);
        service.Initialize(lines, 0, false, FindService.FindMode.CaseInsensitive);
        service.ReplaceAll("fox", "cat");
        if (!lines.SequenceEqual(expected))
        {
            throw new Exception("FindService.ReplaceAll differs from string.Replace");
        }
    }

    // ------------------------------------------------------------------ translate

    [Benchmark]
    public int PreTranslateEnglish()
    {
        var total = 0;
        foreach (var line in _contractionLines)
        {
            total += TranslationHelper.PreTranslate(line, "en").Length;
        }

        return total;
    }

    [Benchmark]
    public int FormattingSetTagsBasque()
    {
        var total = 0;
        foreach (var text in _twoLineTexts)
        {
            total += new Formatting().SetTagsAndReturnTrimmed(text, "eu").Length;
        }

        return total;
    }

    // ------------------------------------------------------------------ find / replace

    [Benchmark]
    public int FindServiceReplaceAll()
    {
        var service = new FindService();
        var lines = new List<string>(_replaceLines);
        service.Initialize(lines, 0, false, FindService.FindMode.CaseInsensitive);
        return service.ReplaceAll("fox", "cat");
    }

    // ------------------------------------------------------------------ statistics word table (self-contained old vs new)

    private static string WordTableOld(SortedDictionary<string, string> sortedTable)
    {
        var sb = new StringBuilder();
        var temp = string.Empty;
        foreach (KeyValuePair<string, string> item in sortedTable)
        {
            temp = item.Value + Environment.NewLine + temp;
        }

        sb.AppendLine(temp);
        return sb.ToString();
    }

    private static string WordTableNew(SortedDictionary<string, string> sortedTable)
    {
        var sb = new StringBuilder();
        foreach (var item in sortedTable.Reverse())
        {
            sb.Append(item.Value).Append(Environment.NewLine);
        }

        sb.AppendLine();
        return sb.ToString();
    }

    [Benchmark]
    public int StatisticsWordTableOld() => WordTableOld(_wordTable).Length;

    [Benchmark]
    public int StatisticsWordTableNew() => WordTableNew(_wordTable).Length;

    // ------------------------------------------------------------------ dictionaries

    [Benchmark]
    public int NameListContainsCaseInsensitive()
    {
        var hits = 0;
        for (var i = 0; i < 50; i++)
        {
            foreach (var probe in _nameProbes)
            {
                if (_names.ContainsCaseInsensitive(probe, out _))
                {
                    hits++;
                }
            }
        }

        return hits;
    }

    [Benchmark]
    public int LoadUserWordList()
    {
        var list = new List<string>();
        Utilities.LoadUserWordList(list, "en");
        return list.Count;
    }

    // ------------------------------------------------------------------ misc helpers

    [Benchmark]
    public int LenientFileNames()
    {
        var total = 0;
        foreach (var f in _fileNames)
        {
            total += Utilities.GetLenientPathAndFileNameWithoutExtension(f).Length;
        }

        return total;
    }

    [Benchmark]
    public int Cea608CharForByte()
    {
        var total = 0;
        for (var i = 0; i < 400; i++)
        {
            for (var b = 0x20; b < 0x80; b++)
            {
                total += CcRow.GetCharForByte(b).Length;
            }
        }

        return total;
    }

    [Benchmark]
    public int RemoveParagraphsByIndices()
    {
        var copy = new Subtitle(_bigSubtitle);
        return copy.RemoveParagraphsByIndices(_removeIndices);
    }

    // ------------------------------------------------------------------ drift control

    [Benchmark]
    public int SubRipControl() => new SubRip().ToText(_taggedSubtitle, "t").Length;
}
