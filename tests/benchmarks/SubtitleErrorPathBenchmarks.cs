using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.ErrorList;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// The "find errors" paths: list errors (whole file), go to next/previous error (scan
/// until a hit) and the per-row accessible error text the grid reads on every repaint.
/// </summary>
[MemoryDiagnoser]
public class SubtitleErrorPathBenchmarks
{
    private List<SubtitleLineViewModel> _lines = new();
    private List<Paragraph> _paragraphs = new();

    /// <summary>Lines in a typical feature-length subtitle.</summary>
    [Params(1000)]
    public int LineCount { get; set; }

    /// <summary>Off by default; on it shapes every line with HarfBuzz.</summary>
    [Params(false, true)]
    public bool ColorTextTooWide { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var general = Se.Settings.General;
        general.ColorDurationTooShort = true;
        general.ColorDurationTooLong = true;
        general.ColorTextTooLong = true;
        general.ColorTextTooManyLines = true;
        general.ColorCharactersPerSecond = true;
        general.ColorWordsPerMinute = true;
        general.ColorTimeCodeOverlap = true;
        general.ColorGapTooShort = true;
        general.ColorTextTooWide = ColorTextTooWide;
        general.SubtitleLineMaximumLength = 43;
        general.MaxNumberOfLines = 2;
        general.SubtitleMinimumDisplayMilliseconds = 1000;
        general.SubtitleMaximumDisplayMilliseconds = 8000;
        general.SubtitleMaximumCharactersPerSeconds = 25.0;
        general.SubtitleMaximumWordsPerMinute = 400;
        general.ColorTextTooWidePixels = 1200;
        general.MinimumBetweenLines.Milliseconds = 24;

        _lines = SubtitleFactory.Make(LineCount);

        // Realistic error density (~15%): every 7th line runs too fast, every 11th overlaps.
        for (var i = 0; i < _lines.Count; i++)
        {
            if (i % 7 == 3)
            {
                _lines[i].SetTimes(_lines[i].StartTime, _lines[i].StartTime + TimeSpan.FromMilliseconds(400));
            }

            if (i % 11 == 5 && i > 0)
            {
                _lines[i].SetTimes(_lines[i - 1].EndTime - TimeSpan.FromMilliseconds(200), _lines[i].EndTime);
            }
        }

        SubtitleTextInfoHelper.UpdateGaps(_lines);
        _paragraphs = _lines.Select(l => l.ToParagraph()).ToList();
    }

    /// <summary>Main menu "List errors" - and the same scan behind go to next/previous error.</summary>
    [Benchmark]
    public int ListErrorsScan()
    {
        var count = 0;
        for (var i = 0; i < _lines.Count; i++)
        {
            var s = _lines[i];
            var prev = i > 0 ? _lines[i - 1] : null;
            var next = i < _lines.Count - 1 ? _lines[i + 1] : null;
            if (!string.IsNullOrEmpty(s.GetErrors(prev, next)))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// The same scan right after a file is loaded, i.e. with every per-line memo cold.
    /// The iteration setup hands each line a fresh string instance, which is what the
    /// ReferenceEquals-keyed memos in the view model key on.
    /// </summary>
    [IterationSetup(Target = nameof(ListErrorsScanCold))]
    public void InvalidateMemos()
    {
        foreach (var line in _lines)
        {
            line.Text = new string(line.Text.AsSpan());
        }
    }

    [Benchmark]
    public int ListErrorsScanCold()
    {
        var count = 0;
        for (var i = 0; i < _lines.Count; i++)
        {
            var s = _lines[i];
            var prev = i > 0 ? _lines[i - 1] : null;
            var next = i < _lines.Count - 1 ? _lines[i + 1] : null;
            if (!string.IsNullOrEmpty(s.GetErrors(prev, next)))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>The scan as the commands do it now: ask for the verdict, not the message.</summary>
    [Benchmark]
    public int HasErrorsScan()
    {
        var count = 0;
        for (var i = 0; i < _lines.Count; i++)
        {
            var s = _lines[i];
            var prev = i > 0 ? _lines[i - 1] : null;
            var next = i < _lines.Count - 1 ? _lines[i + 1] : null;
            if (s.HasErrors(prev, next))
            {
                count++;
            }
        }

        return count;
    }

    [IterationSetup(Target = nameof(HasErrorsScanCold))]
    public void InvalidateMemosForHasErrors() => InvalidateMemos();

    [Benchmark]
    public int HasErrorsScanCold() => HasErrorsScan();

    /// <summary>"List errors" now: filter with the verdict, build the message only for hits.</summary>
    [Benchmark]
    public int ListErrorsFullNew()
    {
        var items = new List<ErrorListItem>();
        for (var i = 0; i < _lines.Count; i++)
        {
            var s = _lines[i];
            var prev = i > 0 ? _lines[i - 1] : null;
            var next = i < _lines.Count - 1 ? _lines[i + 1] : null;
            if (s.HasErrors(prev, next))
            {
                items.Add(new ErrorListItem(s, prev, next));
            }
        }

        return items.Count;
    }

    /// <summary>Batch convert's error list now: one view model per paragraph, reused as neighbour.</summary>
    [Benchmark]
    public int BatchErrorListInitializeNew()
    {
        var count = 0;
        var format = new SubRip();
        var lines = new List<SubtitleLineViewModel>(_paragraphs.Count);
        foreach (var p in _paragraphs)
        {
            lines.Add(new SubtitleLineViewModel(p, format));
        }

        for (var i = 0; i < lines.Count; i++)
        {
            var prev = i > 0 ? lines[i - 1] : null;
            var next = i < lines.Count - 1 ? lines[i + 1] : null;
            if (lines[i].HasErrors(prev, next))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>The full "List errors" command: scan, then build the dialog's item list.</summary>
    [Benchmark]
    public int ListErrorsFull()
    {
        var list = new List<SubtitleLineViewModel>();
        for (var i = 0; i < _lines.Count; i++)
        {
            var s = _lines[i];
            var prev = i > 0 ? _lines[i - 1] : null;
            var next = i < _lines.Count - 1 ? _lines[i + 1] : null;
            if (!string.IsNullOrEmpty(s.GetErrors(prev, next)))
            {
                list.Add(s);
            }
        }

        var items = new List<ErrorListItem>(list.Count);
        for (var i = 0; i < list.Count; i++)
        {
            var prev = i > 0 ? list[i - 1] : null;
            var next = i < list.Count - 1 ? list[i + 1] : null;
            items.Add(new ErrorListItem(list[i], prev, next));
        }

        return items.Count;
    }

    /// <summary>
    /// Batch convert's error list, as BatchErrorListViewModel.Initialize + the
    /// BatchErrorListItem constructor do it: a view model for the line and one for each
    /// neighbour, then the full error message, for every paragraph of every file.
    /// </summary>
    [Benchmark]
    public int BatchErrorListInitialize()
    {
        var count = 0;
        var format = new SubRip();
        for (var i = 0; i < _paragraphs.Count; i++)
        {
            var p = _paragraphs[i];
            var prev = i > 0 ? _paragraphs[i - 1] : null;
            var next = i < _paragraphs.Count - 1 ? _paragraphs[i + 1] : null;

            var line = new SubtitleLineViewModel(p, format);
            var itemFormat = new SubRip();
            var prevLine = prev != null ? new SubtitleLineViewModel(prev, itemFormat) : null;
            var nextLine = next != null ? new SubtitleLineViewModel(next, itemFormat) : null;
            if (!string.IsNullOrEmpty(line.GetErrors(prevLine, nextLine)))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Row accessible name: read for every realized row on every invalidation.</summary>
    [Benchmark]
    public int AccessibleErrorTextVisibleRows()
    {
        var length = 0;
        for (var i = 0; i < 30; i++)
        {
            length += _lines[i].AccessibleErrorText.Length;
        }

        return length;
    }
}
