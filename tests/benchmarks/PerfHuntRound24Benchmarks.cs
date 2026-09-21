using System.Collections.ObjectModel;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.MergeShortLines;

namespace Nikse.SubtitleEdit.Benchmarks;

// Round 24: list scans per row / per line / per timer tick. Every class holds the shape the code
// had before ("Old", copied here) next to the shape it has now ("New" - the real method where it
// is reachable, a copy where it is private), and [GlobalSetup] throws when the two disagree.
// The binary OCR size index is measured end to end with a dump harness instead (see the PR).

internal static class Round24Rows
{
    internal static List<SubtitleLineViewModel> Create(int count)
    {
        var rows = new List<SubtitleLineViewModel>(count);
        for (var i = 0; i < count; i++)
        {
            rows.Add(new SubtitleLineViewModel
            {
                Number = i + 1,
                Text = "Line " + i,
                StartTime = TimeSpan.FromMilliseconds(i * 3000.0),
                EndTime = TimeSpan.FromMilliseconds(i * 3000.0 + 2000),
            });
        }

        return rows;
    }
}

/// <summary>Merge short lines: "is there a shot change between these two lines", per adjacent pair.</summary>
[MemoryDiagnoser]
public class MergeShortLinesShotChangeBenchmarks
{
    private List<SubtitleLineViewModel> _rows = new();
    private List<double> _shotChanges = new();
    private double[] _sorted = Array.Empty<double>();

    [Params(2000, 20000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _rows = Round24Rows.Create(Lines);
        var random = new Random(24);
        _shotChanges = new List<double>();
        var t = 0.0;
        var end = Lines * 3.0;
        while (t < end)
        {
            t += 1.5 + random.NextDouble() * 6; // a cut every ~4.5 s
            _shotChanges.Add(t);
        }

        // some exactly on a cue, which the strict compares must not count
        _shotChanges.Add(_rows[10].EndTime.TotalSeconds);
        _shotChanges.Add(_rows[11].StartTime.TotalSeconds);

        _sorted = _shotChanges.ToArray();
        Array.Sort(_sorted);
        if (Old() != New())
        {
            throw new InvalidOperationException("shot change lookups differ");
        }
    }

    [Benchmark(Baseline = true)]
    public int Old()
    {
        var hits = 0;
        for (var i = 0; i < _rows.Count - 1; i++)
        {
            var current = _rows[i];
            var next = _rows[i + 1];
            if (_shotChanges.Any(s => s > current.EndTime.TotalSeconds && s < next.StartTime.TotalSeconds))
            {
                hits++;
            }
        }

        return hits;
    }

    [Benchmark]
    public int New()
    {
        var hits = 0;
        for (var i = 0; i < _rows.Count - 1; i++)
        {
            if (MergeShortLinesHelper.HasShotChangeBetween(_sorted, _rows[i].EndTime.TotalSeconds, _rows[i + 1].StartTime.TotalSeconds))
            {
                hits++;
            }
        }

        return hits;
    }
}

/// <summary>Fix continuation style: "does the line start with a name", per candidate line.</summary>
[MemoryDiagnoser]
public class StartsWithNameBenchmarks
{
    private List<string> _names = new();
    private HashSet<string> _nameSet = new();
    private int _nameMaxLength;
    private string[] _lines = Array.Empty<string>();

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        _names = new NameList(Configuration.DictionariesDirectory, "en", false, string.Empty).GetAllNames();
        if (_names.Count < 4000)
        {
            throw new InvalidOperationException("names.xml not found");
        }

        _nameSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var name in _names)
        {
            _nameSet.Add(name);
            _nameMaxLength = Math.Max(_nameMaxLength, name.Length);
        }

        var random = new Random(24);
        var lines = new List<string>
        {
            "", " ", ",", ":", "John", "John ", "John,", "John:", "John.", "Johnny was here",
            "New York is big", "New York, maybe", "new york is big", " John is late", "and then he left",
        };
        for (var i = 0; i < 300; i++)
        {
            lines.Add(i % 4 == 0
                ? _names[random.Next(_names.Count)] + (i % 8 == 0 ? ", come here" : " said so")
                : "and then we went to the old house by the lake " + i);
        }

        _lines = lines.ToArray();
        foreach (var line in _lines)
        {
            if (OldStartsWithName(line) != NewStartsWithName(line))
            {
                throw new InvalidOperationException("StartsWithName differs for '" + line + "'");
            }
        }
    }

    private bool OldStartsWithName(string input)
    {
        foreach (var name in _names)
        {
            if (input.StartsWith(name + " ", StringComparison.Ordinal) || input.StartsWith(name + ",", StringComparison.Ordinal) || input.StartsWith(name + ":", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private bool NewStartsWithName(string input)
    {
        var max = Math.Min(input.Length - 1, _nameMaxLength);
        for (var i = 0; i <= max; i++)
        {
            var ch = input[i];
            if ((ch == ' ' || ch == ',' || ch == ':') && _nameSet.Contains(input.Substring(0, i)))
            {
                return true;
            }
        }

        return false;
    }

    [Benchmark(Baseline = true)]
    public int Old()
    {
        var hits = 0;
        foreach (var line in _lines)
        {
            if (OldStartsWithName(line))
            {
                hits++;
            }
        }

        return hits;
    }

    [Benchmark]
    public int New()
    {
        var hits = 0;
        foreach (var line in _lines)
        {
            if (NewStartsWithName(line))
            {
                hits++;
            }
        }

        return hits;
    }
}

/// <summary>
/// Select all + a command that needs every selected row's position (extend to next/previous,
/// recalculate duration, snap to shot change, ...), and the merge validation.
/// </summary>
[MemoryDiagnoser]
public class SelectedRowIndexBenchmarks
{
    private ObservableCollection<SubtitleLineViewModel> _subtitles = new();
    private List<SubtitleLineViewModel> _selected = new();

    [Params(2000, 20000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _subtitles = new ObservableCollection<SubtitleLineViewModel>(Round24Rows.Create(Lines));
        _selected = _subtitles.ToList();
        if (IndexOfPerRow() != IndexMap() || MergeValidationOld() != MergeValidationNew())
        {
            throw new InvalidOperationException("row positions differ");
        }
    }

    [Benchmark(Baseline = true)]
    public long IndexOfPerRow()
    {
        long sum = 0;
        foreach (var row in _selected)
        {
            sum += _subtitles.IndexOf(row);
        }

        return sum;
    }

    [Benchmark]
    public long IndexMap()
    {
        var map = new Dictionary<SubtitleLineViewModel, int>(_subtitles.Count);
        for (var i = 0; i < _subtitles.Count; i++)
        {
            map.TryAdd(_subtitles[i], i);
        }

        long sum = 0;
        foreach (var row in _selected)
        {
            sum += map.TryGetValue(row, out var index) ? index : -1;
        }

        return sum;
    }

    [Benchmark]
    public long MergeValidationOld()
    {
        var indices = _selected.Select(_subtitles.IndexOf).ToList();
        for (var i = 0; i < indices.Count; i++)
        {
            if (indices[i] < 0 || (i > 0 && indices[i] != indices[0] + i))
            {
                return -1;
            }
        }

        long sum = 0;
        foreach (var row in _selected)
        {
            sum += _subtitles.IndexOf(row);
        }

        return sum;
    }

    [Benchmark]
    public long MergeValidationNew()
    {
        var first = _subtitles.IndexOf(_selected[0]);
        if (first < 0 || first + _selected.Count > _subtitles.Count)
        {
            return -1;
        }

        for (var i = 1; i < _selected.Count; i++)
        {
            if (!ReferenceEquals(_subtitles[first + i], _selected[i]))
            {
                return -1;
            }
        }

        long sum = 0;
        for (var i = 0; i < _selected.Count; i++)
        {
            sum += first + i;
        }

        return sum;
    }
}

/// <summary>Select all + paste subtitle lines over the selection: removing the selected rows.</summary>
[MemoryDiagnoser]
public class PasteOverRemoveBenchmarks
{
    private List<SubtitleLineViewModel> _rows = new();
    private List<SubtitleLineViewModel> _selected = new();

    [Params(2000, 20000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _rows = Round24Rows.Create(Lines);
        _selected = _rows.Where((_, i) => i % 10 != 0).ToList(); // a selection with holes
        if (!Old().SequenceEqual(New()))
        {
            throw new InvalidOperationException("removal differs");
        }
    }

    [Benchmark(Baseline = true)]
    public ObservableCollection<SubtitleLineViewModel> Old()
    {
        var subtitles = new ObservableCollection<SubtitleLineViewModel>(_rows);
        foreach (var item in _selected)
        {
            subtitles.Remove(item);
        }

        return subtitles;
    }

    [Benchmark]
    public ObservableCollection<SubtitleLineViewModel> New()
    {
        var subtitles = new ObservableCollection<SubtitleLineViewModel>(_rows);
        var removeSet = new HashSet<SubtitleLineViewModel>(_selected);
        for (var i = subtitles.Count - 1; i >= 0 && removeSet.Count > 0; i--)
        {
            if (removeSet.Remove(subtitles[i]))
            {
                subtitles.RemoveAt(i);
            }
        }

        return subtitles;
    }
}

/// <summary>Batch convert statistics: the next paragraph, per paragraph.</summary>
[MemoryDiagnoser]
public class BatchStatisticsNextParagraphBenchmarks
{
    private Subtitle _subtitle = new();

    [Params(2000, 20000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _subtitle = new Subtitle();
        for (var i = 0; i < Lines; i++)
        {
            _subtitle.Paragraphs.Add(new Paragraph("Line " + i, i * 3000.0, i * 3000.0 + 2000 + i % 7));
        }

        if (Math.Abs(Old() - New()) > 0.000001)
        {
            throw new InvalidOperationException("gaps differ");
        }
    }

    [Benchmark(Baseline = true)]
    public double Old()
    {
        var gapTotal = 0.0;
        foreach (var p in _subtitle.Paragraphs)
        {
            var next = _subtitle.GetParagraphOrDefault(_subtitle.GetIndex(p) + 1);
            if (next != null)
            {
                gapTotal += next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds;
            }
        }

        return gapTotal;
    }

    [Benchmark]
    public double New()
    {
        var gapTotal = 0.0;
        for (var i = 0; i < _subtitle.Paragraphs.Count; i++)
        {
            var p = _subtitle.Paragraphs[i];
            var next = _subtitle.GetParagraphOrDefault(i + 1);
            if (next != null)
            {
                gapTotal += next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds;
            }
        }

        return gapTotal;
    }
}

/// <summary>
/// "Find the fix / merge item for this line" per line: the OK click of Merge lines with same
/// text / same time codes, and the hearing impaired preview carrying the checkbox states over.
/// </summary>
[MemoryDiagnoser]
public class PerLineItemLookupBenchmarks
{
    private sealed class Item
    {
        public int Id;
        public bool Apply;
        public List<SubtitleLineViewModel> Lines = new();
    }

    private List<SubtitleLineViewModel> _rows = new();
    private List<Item> _items = new();

    [Params(2000, 20000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _rows = Round24Rows.Create(Lines);
        _items = new List<Item>();
        for (var i = 0; i + 1 < _rows.Count; i += 4)
        {
            _items.Add(new Item { Id = i + 1, Apply = i % 12 != 0, Lines = { _rows[i], _rows[i + 1] } }); // every other pair merges
        }

        // a line in two items: the first ticked one must win
        _items.Add(new Item { Id = 999999, Apply = true, Lines = { _rows[4], _rows[5] } });
        if (Old() != New())
        {
            throw new InvalidOperationException("item lookups differ");
        }
    }

    [Benchmark(Baseline = true)]
    public long Old()
    {
        long sum = 0;
        foreach (var s in _rows)
        {
            var match = _items.FirstOrDefault(p => p.Apply && p.Lines.Contains(s));
            if (match != null)
            {
                sum += match.Id;
            }
        }

        return sum;
    }

    [Benchmark]
    public long New()
    {
        var byLine = new Dictionary<SubtitleLineViewModel, Item>();
        for (var i = 0; i < _items.Count; i++)
        {
            if (_items[i].Apply)
            {
                foreach (var line in _items[i].Lines)
                {
                    byLine.TryAdd(line, _items[i]);
                }
            }
        }

        long sum = 0;
        foreach (var s in _rows)
        {
            if (byLine.TryGetValue(s, out var match))
            {
                sum += match.Id;
            }
        }

        return sum;
    }
}
