using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// The libse paths moved from greedy / brute force to dynamic programming: the OCR word
/// splitter and the 3+ line auto-break. Public entry points only, so the same file runs on
/// the commit before and after; <see cref="SubRipControl"/> is the drift control.
///
/// Default job, i7-13700, .NET 10, a machine that was not quiet (SubRipControl 56.1 us -> 52.1 us,
/// identical allocations):
///
///   AutoBreak 3+ lines, word longer than max   29,970 us -> 6.6 us    4,500x  65.6 MB -> 13.6 KB
///   SplitWord, unsplittable (16 words)           8,536 us -> 4.2 us    2,000x  14 KB -> 7.9 KB
///   SplitWord, merged words (16 words)           4,719 us -> 10.5 us     450x  same allocations
///   AutoBreak 3+ lines, max 37 (5 lines)           125 us -> 53 us       2.3x  187 KB -> 90 KB
///   AutoBreak 3+ lines, max 20 (5 lines)            88 us -> 55 us       1.6x  194 KB -> 102 KB
///
/// The N-way text split, SplitToThree and merge short lines were tried too and undone: the
/// first two got slower (the partition is quadratic in the words without a maximum length),
/// the last one measured the same.
/// </summary>
[MemoryDiagnoser]
public class DynamicProgrammingBenchmarks
{
    private string[] _wordSplitList = Array.Empty<string>();
    private string[] _mergedWords = Array.Empty<string>();
    private string[] _unsplittableWords = Array.Empty<string>();
    private string[] _longLines = Array.Empty<string>();
    private string _lineWithLongWord = string.Empty;
    private Subtitle _controlSubtitle = new();

    [GlobalSetup]
    public void Setup()
    {
        Configuration.Settings.Tools.OcrUseWordSplitList = true;
        Configuration.Settings.Tools.UseNoLineBreakAfter = false;

        var dictionaries = FindDictionariesFolder();
        var names = new NameList(dictionaries, "en", false, string.Empty).GetAllNames();
        _wordSplitList = StringWithoutSpaceSplitToWords.LoadWordSplitList(dictionaries, "eng", names);

        // what OCR hands over: two or three words run together, and unknown words that are not
        _mergedWords = new[]
        {
            "thequick", "overthere", "whatisthis", "comewithme", "nothingelse", "goodmorning", "areyousure",
            "iknowthat", "somethinghappened", "neveragain", "lookatthat", "yesterdaymorning", "wherewereyou",
            "thankyouverymuch", "thisisimportant", "rightbehindyou",
        };
        _unsplittableWords = new[]
        {
            "xqzvkrtw", "bvnmqpzx", "wrtplkjh", "zzxxccvv", "qwrtypsd", "mnbvcxzl", "plkmjnhb", "ghfdtrsw",
            "thequickxq", "overtherezv", "whatisthiskq", "nothingelsezx", "goodmorningqj", "areyousurevk",
            "somethingxqz", "yesterdayzvq",
        };

        _longLines = new[]
        {
            "The quick brown fox jumps over the lazy dog while the farmer sleeps quietly under the old oak tree near the river bank.",
            "We hold these truths to be self-evident, that all men are created equal, that they are endowed by their Creator with certain unalienable Rights.",
            "I told you yesterday. You never listen to me! Why would today be any different from all the other days we have spent together?",
            "<i>The quick brown fox jumps over the lazy dog while the farmer sleeps quietly</i> under the old oak tree near the river bank.",
            "It was the best of times, it was the worst of times, it was the age of wisdom, it was the age of foolishness, it was the epoch of belief, it was the epoch of incredulity.",
        };
        _lineWithLongWord = "Supercalifragilisticexpialidocious is a word that Mr. Banks never wanted to hear in his house again, not even once, not ever.";

        _controlSubtitle = new Subtitle();
        for (var i = 0; i < 400; i++)
        {
            _controlSubtitle.Paragraphs.Add(new Paragraph($"<i>The quick brown fox number {i}</i>{Environment.NewLine}jumps over the lazy dog.", i * 3000, i * 3000 + 2500));
        }
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

    // 1. OCR word splitter, real eng_WordSplitList + names

    [Benchmark]
    public int SplitWord_MergedWords()
    {
        var splits = 0;
        foreach (var word in _mergedWords)
        {
            if (StringWithoutSpaceSplitToWords.SplitWord(_wordSplitList, word, "eng") != word)
            {
                splits++;
            }
        }

        return splits;
    }

    [Benchmark]
    public int SplitWord_UnsplittableWords()
    {
        var splits = 0;
        foreach (var word in _unsplittableWords)
        {
            if (StringWithoutSpaceSplitToWords.SplitWord(_wordSplitList, word, "eng") != word)
            {
                splits++;
            }
        }

        return splits;
    }

    // 2. 3+ line auto-break

    [Benchmark]
    public int AutoBreakMoreThanTwoLines_Max37()
    {
        var length = 0;
        foreach (var line in _longLines)
        {
            length += Utilities.AutoBreakLineMoreThanTwoLines(line, 37, 25, "en").Length;
        }

        return length;
    }

    [Benchmark]
    public int AutoBreakMoreThanTwoLines_Max20()
    {
        var length = 0;
        foreach (var line in _longLines)
        {
            length += Utilities.AutoBreakLineMoreThanTwoLines(line, 20, 25, "en").Length;
        }

        return length;
    }

    [Benchmark]
    public int AutoBreakMoreThanTwoLines_WordLongerThanMax()
    {
        return Utilities.AutoBreakLineMoreThanTwoLines(_lineWithLongWord, 30, 25, "en").Length;
    }

    [Benchmark]
    public int SubRipControl() => new SubRip().ToText(_controlSubtitle, "t").Length;
}
