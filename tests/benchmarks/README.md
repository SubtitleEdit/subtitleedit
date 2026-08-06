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
| `TextMeasurerBenchmarks` | The per-line text measurement the statistics window and batch convert run over every line of a subtitle. |
| `SubtitleImageAdjusterBenchmarks` | The full-frame pixel adjustments the binary-edit dialogs run on every debounced slider tick (brightness/contrast/gamma, alpha, colorize). |
| `FixCommonErrorsAllowFixBenchmarks` | One apply pass worth of `AllowFix` probes - what every libse fix rule asks once per paragraph during "Apply selected fixes". |
| `ModifySelectionRuleBenchmarks` | One 250 ms preview-timer tick of the modify-selection window: the selected rule evaluated against every line (regex, line-count and style rules). |
| `GridCellConverterBenchmarks` | One repaint of a 200-row viewport through the time-code, gap, CPS/WPM, flow-direction and duration-background value converters. |
| `SyntaxHighlightingConverterBenchmarks` | The same repaint through the syntax highlighting converter - the most expensive per-row work in the grid - in all three formatting modes. |
| `SmallConverterBenchmarks` | The handful of small converters a row goes through (boolean/null bindings, color swatches, ellipsis, batch convert status). |
| `BrushCreationBenchmarks` | Why the converters hand out `ImmutableSolidColorBrush`: a `SolidColorBrush` is an AvaloniaObject with a property store, for a color that never changes. |

Benchmarks that model a collection the app gets from Avalonia (e.g. `DataGrid.SelectedItems`)
reproduce its interface surface rather than substituting a `List<T>` - `SelectedItems` only
implements the non-generic `IList`, which is what makes `Cast<T>().ToList()` slow there, and a
`List<T>` stand-in would silently measure LINQ's fast path instead.
