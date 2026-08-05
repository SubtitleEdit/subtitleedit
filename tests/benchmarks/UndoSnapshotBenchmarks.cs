using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Undo change detection snapshots the whole subtitle collection (MainViewModel.MakeUndoRedoObject),
/// and before issue #13234 it did so several times a second during a waveform drag. The copy
/// constructor is the whole cost of that snapshot.
///
/// <see cref="CopyViaProperties"/> is the pre-#13234 shape kept here on purpose: it copies through
/// the public observable setters, so the A/B runs in one build instead of across a git stash.
/// </summary>
[MemoryDiagnoser]
public class UndoSnapshotBenchmarks
{
    private List<SubtitleLineViewModel> _lines = new();

    [Params(500, 3000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup() => _lines = SubtitleFactory.Make(Lines);

    /// <summary>What MakeUndoRedoObject does today.</summary>
    [Benchmark(Baseline = true)]
    public SubtitleLineViewModel[] CopyViaFields()
    {
        var result = new SubtitleLineViewModel[_lines.Count];
        for (var i = 0; i < _lines.Count; i++)
        {
            result[i] = new SubtitleLineViewModel(_lines[i]);
        }

        return result;
    }

    /// <summary>The old copy constructor, reproduced through the public setters.</summary>
    [Benchmark]
    public SubtitleLineViewModel[] CopyViaProperties()
    {
        var result = new SubtitleLineViewModel[_lines.Count];
        for (var i = 0; i < _lines.Count; i++)
        {
            var p = _lines[i];
            var copy = new SubtitleLineViewModel
            {
                Text = p.Text,
                OriginalText = p.OriginalText,
                StartTime = p.StartTime,
                EndTime = p.EndTime,
                Style = p.Style,
                Actor = p.Actor,
                Layer = p.Layer,
                Number = p.Number,
                Language = p.Language,
                Region = p.Region,
                Extra = p.Extra,
                Effect = p.Effect,
                IsComment = p.IsComment,
                MarginL = p.MarginL,
                MarginR = p.MarginR,
                MarginV = p.MarginV,
                NewSection = p.NewSection,
                Forced = p.Forced,
                Bookmark = p.Bookmark,
                Id = p.Id,
            };
            copy.UpdateDuration();
            result[i] = copy;
        }

        return result;
    }
}
