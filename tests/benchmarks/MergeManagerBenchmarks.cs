using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// "Merge selected lines" is a single user gesture, but it used to auto-detect the language of the
/// whole file once per merged line - and every detection deep-copied the entire subtitle twice
/// before running ~30 word-count passes over the joined text. Merging a handful of lines in a
/// feature-length file therefore cost seconds.
/// </summary>
[MemoryDiagnoser]
public class MergeManagerBenchmarks
{
    private ObservableCollection<SubtitleLineViewModel> _lines = new();
    private List<SubtitleLineViewModel> _selected = new();

    /// <summary>Lines in the loaded file - a feature-length subtitle is 1500-2500 lines.</summary>
    [Params(2000)]
    public int Lines { get; set; }

    /// <summary>Lines the user selected and merged in one gesture.</summary>
    [Params(2, 20)]
    public int Selected { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Configuration.Settings.General.ContinuationStyle = ContinuationStyle.OnlyTrailingDots;
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _lines = new ObservableCollection<SubtitleLineViewModel>(SubtitleFactory.Make(Lines));
        _selected = _lines.Skip(10).Take(Selected).ToList();
    }

    [Benchmark]
    public int MergeSelectedLines()
    {
        new MergeManager().MergeSelectedLines(_lines, _selected);
        return _lines.Count;
    }
}
