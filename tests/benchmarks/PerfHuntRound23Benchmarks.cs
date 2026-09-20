using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.UiLogic.AudioToText;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;

namespace Nikse.SubtitleEdit.Benchmarks;

// Round 23: libuilogic + UI. Every class holds the shape the code had before ("Old", copied
// here) next to the shape it has now ("New" - the real method where it is public, a copy where
// it is private), and [GlobalSetup] throws when the two disagree. No stash baseline needed.

/// <summary>GermanNouns.UppercaseNouns: 4641 substring searches per line vs a lookup per word.</summary>
[MemoryDiagnoser]
public class GermanNounsBenchmarks
{
    private GermanNouns _nouns = new();
    private List<string> _allNouns = new();
    private string[] _lines = Array.Empty<string>();

    private static readonly Regex DasEssenUpper = new(@"\bDas essen\b", RegexOptions.Compiled);
    private static readonly Regex DasEssenLower = new(@"\bdas essen\b", RegexOptions.Compiled);

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        _nouns = new GermanNouns();
        _allNouns = FileUtil.ReadAllLinesShared(Path.Combine(Configuration.DictionariesDirectory, "deu_Nouns.txt"), Encoding.UTF8)
            .Select(p => p.Trim()).Where(p => p.Length > 1).ToList();
        if (_allNouns.Count < 4000)
        {
            throw new InvalidOperationException("deu_Nouns.txt not found");
        }

        // Whisper-style lower-cased German: real nouns from the list between filler words, plus
        // the awkward cases (tags, hyphens, multi-word noun, noun inside a longer word, line break).
        var filler = new[] { "und", "dann", "ging", "er", "mit", "dem", "in", "die", "aber", "nicht", "sehr", "schnell" };
        var random = new Random(23);
        var lines = new List<string>
        {
            "<i>wir fahren mit der u-bahn nach new york.</i>",
            "- das essen war gut.\r\n- die t-shirts auch, herr müller!",
            "{\\an8}hausaufgabenheft und haus",
            "ALLES GROSS GESCHRIEBEN",
            "das haus. der hund? \"die katze\" [der mann] 'das kind'",
        };
        for (var i = 0; i < 200; i++)
        {
            var sb = new StringBuilder();
            for (var w = 0; w < 9; w++)
            {
                sb.Append(w % 3 == 1 ? _allNouns[random.Next(_allNouns.Count)].ToLowerInvariant() : filler[random.Next(filler.Length)]);
                sb.Append(w == 4 ? Environment.NewLine : w == 8 ? "." : w == 6 ? ", " : " ");
            }

            lines.Add(sb.ToString());
        }

        _lines = lines.ToArray();
        foreach (var line in _lines)
        {
            var oldResult = OldUppercaseNouns(line);
            var newResult = _nouns.UppercaseNouns(line);
            if (oldResult != newResult)
            {
                throw new InvalidOperationException($"GermanNouns differ for '{line}': '{oldResult}' vs '{newResult}'");
            }
        }
    }

    private string OldUppercaseNouns(string text)
    {
        var textNoTags = HtmlUtil.RemoveHtmlTags(text, true);
        if (textNoTags != textNoTags.ToUpperInvariant() && !string.IsNullOrEmpty(text))
        {
            var st = new StrippableText(text);
            st.FixCasing(_allNouns, true, false, false, string.Empty);
            st.StrippedText = DasEssenUpper.Replace(st.StrippedText, "Das Essen");
            st.StrippedText = DasEssenLower.Replace(st.StrippedText, "das Essen");
            return st.MergedString;
        }

        return text;
    }

    [Benchmark(Baseline = true)]
    public int Old()
    {
        var sum = 0;
        foreach (var line in _lines)
        {
            sum += OldUppercaseNouns(line).Length;
        }

        return sum;
    }

    [Benchmark]
    public int New()
    {
        var sum = 0;
        foreach (var line in _lines)
        {
            sum += _nouns.UppercaseNouns(line).Length;
        }

        return sum;
    }
}

/// <summary>SpellCheckWordLists.IsWordInUserPhrases: enumerate the HashSet vs two Contains.</summary>
[MemoryDiagnoser]
public class UserPhraseLookupBenchmarks
{
    private readonly HashSet<string> _userPhraseList = new();
    private string[] _words = Array.Empty<string>();

    [Params(50, 1000)]
    public int Phrases { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        for (var i = 0; i < Phrases; i++)
        {
            _userPhraseList.Add("phrase" + i + " word" + i);
        }

        _words = string.Join(" ", BenchmarkSubtitles.Sentences).Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Concat(new[] { "phrase7", "word7", "phrase3" }).ToArray();
        if (Old() != New())
        {
            throw new InvalidOperationException("user phrase lookups differ");
        }
    }

    [Benchmark(Baseline = true)]
    public int Old()
    {
        var hits = 0;
        for (var i = 0; i < _words.Length; i++)
        {
            var withNext = _words[i] + " " + (i < _words.Length - 1 ? _words[i + 1] : "-");
            var withPrev = (i > 0 ? _words[i - 1] : "-") + " " + _words[i];
            foreach (var userPhrase in _userPhraseList)
            {
                if (userPhrase == withNext || userPhrase == withPrev)
                {
                    hits++;
                    break;
                }
            }
        }

        return hits;
    }

    [Benchmark]
    public int New()
    {
        var hits = 0;
        for (var i = 0; i < _words.Length; i++)
        {
            if (_userPhraseList.Contains(_words[i] + " " + (i < _words.Length - 1 ? _words[i + 1] : "-")) ||
                _userPhraseList.Contains((i > 0 ? _words[i - 1] : "-") + " " + _words[i]))
            {
                hits++;
            }
        }

        return hits;
    }
}

/// <summary>SubtitleMarksPersistence.Apply: scan every paragraph per mark vs a sorted start-time window.</summary>
[MemoryDiagnoser]
public class MarksStartTimeLookupBenchmarks
{
    private List<Paragraph> _paragraphs = new();
    private double[] _marks = Array.Empty<double>();

    [Params(2000, 10000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _paragraphs = SubtitleFactory.Make(Lines).Select(l => l.ToParagraph()).ToList();

        // A mark on every second line; a third of them shifted by a frame, some orphaned.
        _marks = _paragraphs.Where((_, i) => i % 2 == 0)
            .Select((p, i) => p.StartTime.TotalMilliseconds + (i % 3 == 0 ? 42 : i % 50 == 0 ? 600 : 0))
            .ToArray();

        if (!Resolve(true).SequenceEqual(Resolve(false)))
        {
            throw new InvalidOperationException("mark resolution differs");
        }
    }

    // Same two passes as Apply: exact (0.5 ms) first, then the 100 ms tolerance.
    private List<int> Resolve(bool old)
    {
        var result = new List<int>();
        var taken = new HashSet<int>();
        var pending = new List<double>();
        var index = old ? null : new StartTimeIndex(_paragraphs);
        foreach (var mark in _marks)
        {
            var i = old ? OldFind(mark, 0.5, taken) : index!.FindClosest(mark, 0.5, taken);
            if (i < 0)
            {
                pending.Add(mark);
                continue;
            }

            taken.Add(i);
            result.Add(i);
        }

        foreach (var mark in pending)
        {
            var i = old ? OldFind(mark, 100.0, taken) : index!.FindClosest(mark, 100.0, taken);
            if (i >= 0)
            {
                taken.Add(i);
            }

            result.Add(i);
        }

        return result;
    }

    private int OldFind(double milliseconds, double toleranceMs, HashSet<int> taken)
    {
        var best = -1;
        var bestDistance = double.MaxValue;
        for (var i = 0; i < _paragraphs.Count; i++)
        {
            if (taken.Contains(i))
            {
                continue;
            }

            var distance = Math.Abs(_paragraphs[i].StartTime.TotalMilliseconds - milliseconds);
            if (distance <= toleranceMs && distance < bestDistance)
            {
                best = i;
                bestDistance = distance;
            }
        }

        return best;
    }

    private sealed class StartTimeIndex
    {
        private readonly double[] _startTimes;
        private readonly int[] _paragraphIndexes;

        public StartTimeIndex(List<Paragraph> paragraphs)
        {
            _startTimes = new double[paragraphs.Count];
            _paragraphIndexes = new int[paragraphs.Count];
            for (var i = 0; i < paragraphs.Count; i++)
            {
                _startTimes[i] = paragraphs[i].StartTime.TotalMilliseconds;
                _paragraphIndexes[i] = i;
            }

            Array.Sort(_startTimes, _paragraphIndexes);
        }

        public int FindClosest(double milliseconds, double toleranceMs, HashSet<int> taken)
        {
            if (double.IsNaN(milliseconds))
            {
                return -1;
            }

            var first = Array.BinarySearch(_startTimes, milliseconds - toleranceMs - 1);
            if (first < 0)
            {
                first = ~first;
            }

            while (first > 0 && _startTimes[first - 1] >= milliseconds - toleranceMs - 1)
            {
                first--;
            }

            var best = -1;
            var bestDistance = double.MaxValue;
            for (var i = first; i < _startTimes.Length && _startTimes[i] <= milliseconds + toleranceMs + 1; i++)
            {
                var paragraphIndex = _paragraphIndexes[i];
                if (taken.Contains(paragraphIndex))
                {
                    continue;
                }

                var distance = Math.Abs(_startTimes[i] - milliseconds);
                if (distance <= toleranceMs &&
                    (distance < bestDistance || (distance == bestDistance && paragraphIndex < best)))
                {
                    best = paragraphIndex;
                    bestDistance = distance;
                }
            }

            return best;
        }
    }

    [Benchmark(Baseline = true)]
    public int Old() => Resolve(true).Count;

    [Benchmark]
    public int New() => Resolve(false).Count;
}

/// <summary>OcrFixReplaceList2.CreateGuessesFromLetters: List.Contains + list copy per pair vs HashSet + count.</summary>
[MemoryDiagnoser]
public class OcrGuessListBenchmarks
{
    private OcrFixReplaceList2 _replaceList = null!;
    private List<KeyValuePair<string, string>> _partialWordReplaceList = new();

    // Unknown OCR words with several replaceable letter pairs - the case that fans out.
    private readonly string[] _words = { "lllumlnatlon", "rnillirnetre", "vvaterfaIIs", "Iittle", "hello", "cIoseIy", "rnornlng" };

    [GlobalSetup]
    public void Setup()
    {
        var dataDirectory = BenchmarkSubtitles.FindDataDirectory();
        _replaceList = new OcrFixReplaceList2(Path.Combine(dataDirectory, "Dictionaries", "eng_OCRFixReplaceList.xml"));
        _partialWordReplaceList = (List<KeyValuePair<string, string>>)typeof(OcrFixReplaceList2)
            .GetField("_partialWordReplaceList", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(_replaceList)!;
        if (_partialWordReplaceList.Count < 10)
        {
            throw new InvalidOperationException("eng_OCRFixReplaceList.xml PartialWords not loaded");
        }

        foreach (var word in _words)
        {
            // "xxx" as language: skips the dan/eng/swe post filter, which is unchanged.
            if (!OldCreateGuesses(word).SequenceEqual(_replaceList.CreateGuessesFromLetters(word, "xxx")))
            {
                throw new InvalidOperationException("guess lists differ for " + word);
            }
        }
    }

    private static void OldAddToGuessList(List<string> list, string guess)
    {
        if (string.IsNullOrEmpty(guess))
        {
            return;
        }

        if (!list.Contains(guess))
        {
            list.Add(guess);
        }
    }

    private List<string> OldCreateGuesses(string word)
    {
        var list = new List<string>();
        var previousGuesses = new List<string>();
        foreach (var kv in _partialWordReplaceList)
        {
            var letter = kv.Key;
            var replacement = kv.Value;
            var indexes = new List<int>();
            for (var i = 0; i <= word.Length - letter.Length; i++)
            {
                if (word.AsSpan(i).StartsWith(letter, StringComparison.Ordinal))
                {
                    if (i == word.Length - letter.Length && !replacement.Contains(' '))
                    {
                        OldAddToGuessList(list, word.Remove(i, letter.Length).Insert(i, replacement));
                    }
                    else
                    {
                        indexes.Add(i);
                        OldAddToGuessList(list, word.Remove(i, letter.Length).Insert(i, replacement));
                    }
                }
            }

            if (indexes.Count > 1)
            {
                if (!replacement.Contains(' '))
                {
                    var multiGuess = word;
                    for (var i = indexes.Count - 1; i >= 0; i--)
                    {
                        var idx = indexes[i];
                        multiGuess = multiGuess.Remove(idx, letter.Length).Insert(idx, replacement);
                        OldAddToGuessList(list, multiGuess);
                    }

                    OldAddToGuessList(list, word.Replace(letter, replacement));
                }
            }
            else if (indexes.Count > 0)
            {
                OldAddToGuessList(list, word.Replace(letter, replacement));
            }

            if (indexes.Count > 0)
            {
                for (var i = indexes.Count - 1; i >= 0; i--)
                {
                    var idx = indexes[i];
                    if (idx > 1 && idx < word.Length - 2)
                    {
                        OldAddToGuessList(list, word.Remove(idx, letter.Length).Insert(idx, replacement));
                    }
                }
            }

            foreach (var previousGuess in previousGuesses)
            {
                for (var i = 0; i <= previousGuess.Length - letter.Length; i++)
                {
                    if (previousGuess.AsSpan(i).StartsWith(letter, StringComparison.Ordinal))
                    {
                        OldAddToGuessList(list, previousGuess.Remove(i, letter.Length).Insert(i, replacement));
                    }
                }
            }

            previousGuesses = new List<string>(list);
        }

        return list;
    }

    [Benchmark(Baseline = true)]
    public int Old()
    {
        var sum = 0;
        foreach (var word in _words)
        {
            sum += OldCreateGuesses(word).Count;
        }

        return sum;
    }

    [Benchmark]
    public int New()
    {
        var sum = 0;
        foreach (var word in _words)
        {
            sum += _replaceList.CreateGuessesFromLetters(word, "xxx").Count();
        }

        return sum;
    }
}

/// <summary>FixCommonErrors rescan: _oldFixes.FirstOrDefault per new fix vs a (paragraph id, action) dictionary.</summary>
[MemoryDiagnoser]
public class FixCommonErrorsOldFixLookupBenchmarks
{
    private sealed class Fix
    {
        public Paragraph Paragraph = null!;
        public string Action = string.Empty;
        public bool IsSelected;
    }

    private List<Fix> _fixes = new();

    [Params(2000, 20000)]
    public int Fixes { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var actions = new[] { "Remove unneeded spaces", "Fix missing periods", "Add missing quotes", "Fix casing" };
        var paragraphs = SubtitleFactory.Make(Fixes / 2 + 1).Select(l => l.ToParagraph()).ToList();
        _fixes = Enumerable.Range(0, Fixes)
            .Select(i => new Fix { Paragraph = paragraphs[i / 2], Action = actions[i % 2 + i / 2 % 2 * 2], IsSelected = i % 5 != 0 })
            .ToList();
        if (Old() != New())
        {
            throw new InvalidOperationException("old-fix lookups differ");
        }
    }

    [Benchmark(Baseline = true)]
    public int Old()
    {
        var oldFixes = new List<Fix>(_fixes);
        var selected = 0;
        foreach (var fix in _fixes)
        {
            var p = fix.Paragraph;
            var action = fix.Action;
            var oldFix = oldFixes.FirstOrDefault(f => f.Paragraph.Id == p.Id && f.Action == action);
            if (oldFix is not { IsSelected: false })
            {
                selected++;
            }
        }

        return selected;
    }

    [Benchmark]
    public int New()
    {
        var oldFixes = new Dictionary<(Guid? ParagraphId, string Action), Fix>(_fixes.Count);
        foreach (var fix in _fixes)
        {
            oldFixes.TryAdd((fix.Paragraph.Id, fix.Action), fix);
        }

        var selected = 0;
        foreach (var fix in _fixes)
        {
            oldFixes.TryGetValue((fix.Paragraph.Id, fix.Action), out var oldFix);
            if (oldFix is not { IsSelected: false })
            {
                selected++;
            }
        }

        return selected;
    }
}

/// <summary>FixNetflixErrors preview: Paragraphs.IndexOf per record vs a paragraph -> index dictionary.</summary>
[MemoryDiagnoser]
public class NetflixRecordIndexBenchmarks
{
    private List<Paragraph> _paragraphs = new();
    private List<Paragraph> _recordParagraphs = new();

    [Params(2000, 20000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _paragraphs = SubtitleFactory.Make(Lines).Select(l => l.ToParagraph()).ToList();

        // A broad check (max cps, min duration) flags most lines; one record is not in the list.
        _recordParagraphs = _paragraphs.Where((_, i) => i % 4 != 0).Append(new Paragraph()).ToList();
        if (Old() != New())
        {
            throw new InvalidOperationException("record indexes differ");
        }
    }

    [Benchmark(Baseline = true)]
    public long Old()
    {
        long sum = 0;
        foreach (var p in _recordParagraphs)
        {
            var idx = _paragraphs.IndexOf(p);
            if (idx < 0)
            {
                continue;
            }

            sum += idx;
        }

        return sum;
    }

    [Benchmark]
    public long New()
    {
        var paragraphIndexes = new Dictionary<Paragraph, int>(_paragraphs.Count);
        for (var i = 0; i < _paragraphs.Count; i++)
        {
            paragraphIndexes.TryAdd(_paragraphs[i], i);
        }

        long sum = 0;
        foreach (var p in _recordParagraphs)
        {
            if (!paragraphIndexes.TryGetValue(p, out var idx))
            {
                continue;
            }

            sum += idx;
        }

        return sum;
    }
}

/// <summary>Multiple replace preview: SplitToLines + Join per regex rule per line vs only when the text has a \r.</summary>
[MemoryDiagnoser]
public class MultipleReplaceLineFeedBenchmarks
{
    private string[] _texts = Array.Empty<string>();
    private Regex[] _rules = Array.Empty<Regex>();

    /// <summary>True: "\r\n" between lines (Windows), false: "\n" only.</summary>
    [Params(false, true)]
    public bool CarriageReturns { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _texts = SubtitleFactory.Make(5000)
            .Select(l => CarriageReturns ? l.Text : l.Text.Replace("\r\n", "\n"))
            .ToArray();
        _rules = Enumerable.Range(0, 50)
            .Select(i => new Regex(@"\bword" + i + @"\b|\bJohn" + i + @"\n", RegexOptions.Compiled))
            .Append(new Regex(@"No\.\n- Then", RegexOptions.Compiled))
            .ToArray();
        if (Old() != New() || Old() == 0)
        {
            throw new InvalidOperationException("match counts differ");
        }
    }

    [Benchmark(Baseline = true)]
    public int Old()
    {
        var hits = 0;
        foreach (var newText in _texts)
        {
            foreach (var r in _rules)
            {
                if (r.IsMatch(string.Join("\n", newText.SplitToLines())))
                {
                    hits++;
                }
            }
        }

        return hits;
    }

    [Benchmark]
    public int New()
    {
        var hits = 0;
        foreach (var newText in _texts)
        {
            foreach (var r in _rules)
            {
                var lineFeedText = newText.AsSpan().IndexOfAny('\r', '\u2028') < 0
                    ? newText
                    : string.Join("\n", newText.SplitToLines());
                if (r.IsMatch(lineFeedText))
                {
                    hits++;
                }
            }
        }

        return hits;
    }
}

/// <summary>SubtitleLineViewModel.ToParagraph over all rows (GetUpdateSubtitle): "new Paragraph()" + initializer vs the 3-arg ctor.</summary>
[MemoryDiagnoser]
public class ToParagraphBenchmarks
{
    private List<SubtitleLineViewModel> _lines = new();

    [GlobalSetup]
    public void Setup()
    {
        _lines = SubtitleFactory.Make(10000);
        for (var i = 0; i < 50; i++)
        {
            var o = OldToParagraph(_lines[i]);
            var n = _lines[i].ToParagraph();
            if (o.Text != n.Text || o.Number != n.Number ||
                o.StartTime.TotalMilliseconds != n.StartTime.TotalMilliseconds ||
                o.EndTime.TotalMilliseconds != n.EndTime.TotalMilliseconds)
            {
                throw new InvalidOperationException("ToParagraph differs");
            }
        }
    }

    private static Paragraph OldToParagraph(SubtitleLineViewModel l)
    {
        return new Paragraph()
        {
            Number = l.Number,
            StartTime = new TimeCode(l.StartTime),
            EndTime = new TimeCode(l.EndTime),
            Text = l.Text.TrimEnd(),
            Actor = l.Actor,
            Style = l.Style,
            Language = l.Language,
            Region = l.Region,
            Effect = l.Effect,
            IsComment = l.IsComment,
            MarginL = l.MarginL,
            MarginR = l.MarginR,
            MarginV = l.MarginV,
            NewSection = l.NewSection,
            Forced = l.Forced,
            Layer = l.Layer,
            Bookmark = l.Bookmark,
        };
    }

    [Benchmark(Baseline = true)]
    public int Old()
    {
        var sum = 0;
        foreach (var line in _lines)
        {
            sum += OldToParagraph(line).Number;
        }

        return sum;
    }

    [Benchmark]
    public int New()
    {
        var sum = 0;
        foreach (var line in _lines)
        {
            sum += line.ToParagraph().Number;
        }

        return sum;
    }
}

/// <summary>OCR window, delete selected rows: Contains + Remove per row on both collections vs index map + RemoveAt + RemoveAll.</summary>
[MemoryDiagnoser]
public class OcrDeleteSelectedBenchmarks
{
    private sealed class Row
    {
        public int Number;
    }

    private Row[] _rows = Array.Empty<Row>();
    private Row[] _selected = Array.Empty<Row>();
    private Row[] _unknownWordRows = Array.Empty<Row>();

    [Params(2000, 20000)]
    public int Rows { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _rows = Enumerable.Range(0, Rows).Select(i => new Row { Number = i }).ToArray();
        _selected = _rows.Skip(Rows / 4).Take(Rows / 2).ToArray(); // "select everything from here down to there"
        _unknownWordRows = _rows.Where((_, i) => i % 10 == 0).ToArray();
        if (Old() != New())
        {
            throw new InvalidOperationException("delete results differ");
        }
    }

    private static int Checksum(ObservableCollection<Row> visible, List<Row> all, List<Row> unknownWords)
    {
        var hash = new HashCode();
        foreach (var row in visible)
        {
            hash.Add(row.Number);
        }

        hash.Add(-1);
        foreach (var row in all)
        {
            hash.Add(row.Number);
        }

        hash.Add(-1);
        foreach (var row in unknownWords)
        {
            hash.Add(row.Number);
        }

        return hash.ToHashCode();
    }

    [Benchmark(Baseline = true)]
    public int Old()
    {
        var visible = new ObservableCollection<Row>(_rows);
        var all = new List<Row>(_rows);
        var unknownWords = new List<Row>(_unknownWordRows);

        var itemsToRemove = _selected.Where(item => visible.Contains(item)).ToList();
        foreach (var item in itemsToRemove)
        {
            visible.Remove(item);
            all.Remove(item);
        }

        var toRemove = new List<Row>();
        foreach (var unknownWord in unknownWords)
        {
            if (!all.Contains(unknownWord))
            {
                toRemove.Add(unknownWord);
            }
        }

        foreach (var item in toRemove)
        {
            unknownWords.Remove(item);
        }

        return Checksum(visible, all, unknownWords);
    }

    [Benchmark]
    public int New()
    {
        var visible = new ObservableCollection<Row>(_rows);
        var all = new List<Row>(_rows);
        var unknownWords = new List<Row>(_unknownWordRows);

        var rowIndexes = new Dictionary<Row, int>(visible.Count);
        for (var i = 0; i < visible.Count; i++)
        {
            rowIndexes.TryAdd(visible[i], i);
        }

        var itemsToRemove = _selected.Where(item => rowIndexes.ContainsKey(item)).Distinct().ToList();
        foreach (var index in itemsToRemove.Select(item => rowIndexes[item]).OrderByDescending(index => index))
        {
            visible.RemoveAt(index);
        }

        var removed = new HashSet<Row>(itemsToRemove);
        all.RemoveAll(removed.Contains);

        var remaining = new HashSet<Row>(all);
        var toRemove = new List<Row>();
        foreach (var unknownWord in unknownWords)
        {
            if (!remaining.Contains(unknownWord))
            {
                toRemove.Add(unknownWord);
            }
        }

        foreach (var item in toRemove)
        {
            unknownWords.Remove(item);
        }

        return Checksum(visible, all, unknownWords);
    }
}

/// <summary>MainViewModel.RemoveBlankLinesFromGrid: Subtitles.Remove(line) per blank line vs RemoveAt from the bottom by flag.</summary>
[MemoryDiagnoser]
public class RemoveBlankLinesBenchmarks
{
    private List<SubtitleLineViewModel> _lines = new();

    [Params(2000, 20000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _lines = SubtitleFactory.Make(Lines);
        for (var i = 0; i < _lines.Count; i++)
        {
            if (i % 3 == 0) // e.g. after "remove text for hearing impaired" on an SDH-heavy file
            {
                _lines[i].Text = i % 6 == 0 ? string.Empty : " ​";
            }
        }

        if (Old() != New())
        {
            throw new InvalidOperationException("blank line removal differs");
        }
    }

    private static int Checksum(ObservableCollection<SubtitleLineViewModel> subtitles)
    {
        var hash = new HashCode();
        foreach (var s in subtitles)
        {
            hash.Add(s.Number);
        }

        return hash.ToHashCode();
    }

    [Benchmark(Baseline = true)]
    public int Old()
    {
        var subtitles = new ObservableCollection<SubtitleLineViewModel>(_lines);
        var blank = subtitles.Select(s => s.Text.IsOnlyControlCharactersOrWhiteSpace()).ToList();
        if (blank.Count(b => b) == 0)
        {
            return 0;
        }

        var blankLines = subtitles.Where(s => s.Text.IsOnlyControlCharactersOrWhiteSpace()).ToList();
        foreach (var line in blankLines)
        {
            subtitles.Remove(line);
        }

        return Checksum(subtitles);
    }

    [Benchmark]
    public int New()
    {
        var subtitles = new ObservableCollection<SubtitleLineViewModel>(_lines);
        var blank = subtitles.Select(s => s.Text.IsOnlyControlCharactersOrWhiteSpace()).ToList();
        if (blank.Count(b => b) == 0)
        {
            return 0;
        }

        for (var i = blank.Count - 1; i >= 0; i--)
        {
            if (blank[i])
            {
                subtitles.RemoveAt(i);
            }
        }

        return Checksum(subtitles);
    }
}
