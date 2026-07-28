# UI benchmarks

BenchmarkDotNet harness for the hot paths in the subtitle grid and the waveform. Deliberately
**not** part of `SubtitleEdit.sln`: it references the UI app project, and a normal solution build
should not pay for a benchmark host.

Run everything:

```bash
dotnet run -c Release --project tests/benchmarks/UiBenchmarks.csproj -- --filter '*'
```

Run one class, quickly (3 iterations instead of the default ~15):

```bash
dotnet run -c Release --project tests/benchmarks/UiBenchmarks.csproj -- --filter '*IsSelectedHelper*' --job short
```

Reports land in `tests/benchmarks/BenchmarkDotNet.Artifacts/results/` (gitignored). Pass
`--artifacts <dir>` to put them somewhere else.

## What is covered

| Benchmark | Hot path it stands in for |
| --- | --- |
| `IsSelectedHelperBenchmarks` | The waveform's per-pixel "is this sample selected" probe, run on every geometry rebuild (scroll / zoom / resize / selection change). |
| `WaveformSelectionBenchmarks` | The selected-paragraph hash set that `DrawParagraphs` builds every frame. |
| `SubtitleLineViewModelBenchmarks` | The row getters the grid re-reads as rows are recycled - text error highlight and CPS. |
| `SubtitleGridSelectionBenchmarks` | What `SubtitleGridSelectionChanged` does per selection change: copy the selection, find the line's index. |
| `WaveformBufferBenchmarks` | The 50 ms position timer's refill of the waveform's subtitle buffer. |
| `AudioVisualizerRenderBenchmarks` | One full playback frame of the real `AudioVisualizer.Render` (headless, real Skia, record-only): static view vs center-mode scrolling vs a forced geometry rebuild. |

Benchmarks that model a collection the app gets from Avalonia (e.g. `DataGrid.SelectedItems`)
reproduce its interface surface rather than substituting a `List<T>` - `SelectedItems` only
implements the non-generic `IList`, which is what makes `Cast<T>().ToList()` slow there, and a
`List<T>` stand-in would silently measure LINQ's fast path instead.
