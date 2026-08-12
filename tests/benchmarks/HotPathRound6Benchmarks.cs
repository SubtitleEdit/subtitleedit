using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// The change-detection hash the editor runs over the whole file about five times a second:
/// the 333 ms undo change-detection timer (working text and original folded into one pass) and
/// the 400 ms title/auto-save tick (working text and original as two passes). All of it happens
/// on the UI thread - GetFastHash marshals itself there - so it is pure typing latency.
///
/// <see cref="InlineTextGetHashCode"/> is the loop verbatim as it was; <see cref="MemoizedTextHash"/>
/// is the same loop reading the memoized per-instance hash. Two separate methods with direct
/// calls, not one method behind a delegate, so tiered PGO cannot devirtualize one into looking
/// faster than the other.
/// </summary>
[MemoryDiagnoser]
public class ChangeDetectionHashBenchmarks
{
    private List<SubtitleLineViewModel> _lines = null!;

    /// <summary>A feature-length subtitle.</summary>
    [Params(2000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _lines = SubtitleFactory.Make(Lines);

        // A translation job has both columns filled; the undo hash covers both.
        for (var i = 0; i < _lines.Count; i++)
        {
            _lines[i].OriginalText = "Original line " + i + ": the text this line was translated from.";
            _lines[i].Style = "Default";
            _lines[i].Actor = string.Empty;
            _lines[i].Extra = string.Empty;
        }

        // One warm-up pass, matching the app: the first tick after an edit populates the memo
        // and the four or five ticks before the next edit are the ones being measured.
        MemoizedTextHash();
    }

    [Benchmark(Baseline = true)]
    public int InlineTextGetHashCode()
    {
        unchecked
        {
            var hash = 17;
            for (var i = 0; i < _lines.Count; i++)
            {
                var p = _lines[i];
                hash = hash * 23 + p.Number;
                hash = hash * 23 + p.StartTime.TotalMilliseconds.GetHashCode();
                hash = hash * 23 + p.EndTime.TotalMilliseconds.GetHashCode();
                hash = hash * 23 + (p.Text?.GetHashCode() ?? 0);
                hash = hash * 23 + (p.OriginalText?.GetHashCode() ?? 0);
                hash = hash * 23 + (p.Style?.GetHashCode() ?? 0);
                hash = hash * 23 + (p.Extra?.GetHashCode() ?? 0);
                hash = hash * 23 + (p.Actor?.GetHashCode() ?? 0);
                hash = hash * 23 + p.Layer;
            }

            return hash;
        }
    }

    [Benchmark]
    public int MemoizedTextHash()
    {
        unchecked
        {
            var hash = 17;
            for (var i = 0; i < _lines.Count; i++)
            {
                var p = _lines[i];
                hash = hash * 23 + p.Number;
                hash = hash * 23 + p.StartTime.TotalMilliseconds.GetHashCode();
                hash = hash * 23 + p.EndTime.TotalMilliseconds.GetHashCode();
                hash = hash * 23 + p.TextHash;
                hash = hash * 23 + p.OriginalTextHash;
                hash = hash * 23 + (p.Style?.GetHashCode() ?? 0);
                hash = hash * 23 + (p.Extra?.GetHashCode() ?? 0);
                hash = hash * 23 + (p.Actor?.GetHashCode() ?? 0);
                hash = hash * 23 + p.Layer;
            }

            return hash;
        }
    }
}

/// <summary>
/// The 400 ms slow timer's whole-file gap refresh. Included as a measured "is this worth
/// touching" probe rather than as a candidate: the generated observable setters skip the
/// notification when the value is unchanged, which is the case on every idle tick.
/// </summary>
[MemoryDiagnoser]
public class UpdateGapsBenchmarks
{
    private List<SubtitleLineViewModel> _lines = null!;

    [Params(2000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _lines = SubtitleFactory.Make(Lines);
        SubtitleTextInfoHelper.UpdateGaps(_lines);
    }

    [Benchmark]
    public void IdleTick() => SubtitleTextInfoHelper.UpdateGaps(_lines);
}

/// <summary>
/// The CPS / WPM cells were the most expensive converter per cell in
/// <c>GridCellConverterBenchmarks</c> (~55 ns each), and all they do is
/// <c>d.ToString("0.0", culture)</c>. A custom format string is re-parsed on every call, while
/// the standard "F1" specifier takes the fast path - this checks whether that is worth taking
/// (an equivalence sweep over the whole double range lives in the UI tests).
/// </summary>
[MemoryDiagnoser]
public class OneDecimalFormatBenchmarks
{
    private double[] _values = null!;
    private readonly System.Globalization.CultureInfo _culture = System.Globalization.CultureInfo.InvariantCulture;

    [GlobalSetup]
    public void Setup()
    {
        // CPS and WPM as a grid shows them: CPS around 5-25, WPM around 100-250.
        _values = new double[400];
        for (var i = 0; i < _values.Length; i++)
        {
            _values[i] = i % 2 == 0 ? 8.5 + i % 17 * 1.13 : 95.0 + i % 160 * 1.07;
        }
    }

    [Benchmark(Baseline = true)]
    public string CustomFormat()
    {
        var last = string.Empty;
        for (var i = 0; i < _values.Length; i++)
        {
            last = _values[i].ToString("0.0", _culture);
        }

        return last;
    }

    [Benchmark]
    public string StandardFormat()
    {
        var last = string.Empty;
        for (var i = 0; i < _values.Length; i++)
        {
            last = _values[i].ToString("F1", _culture);
        }

        return last;
    }
}
