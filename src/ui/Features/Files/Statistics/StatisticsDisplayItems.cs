using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.Statistics;

/// <summary>
/// A min/avg/max metric shown as a meter row ("Timing and pacing" card).
/// Fractions are 0..1 of the row's own maximum.
/// </summary>
public class StatRangeItem
{
    public string Label { get; init; } = string.Empty;
    public string MinText { get; init; } = string.Empty;
    public string AvgText { get; init; } = string.Empty;
    public string MaxText { get; init; } = string.Empty;
    public double MinFraction { get; init; }
    public double AvgFraction { get; init; }
}

public enum StatCheckSeverity
{
    Good,
    Warning,
    Serious,
    Critical,
}

/// <summary>
/// A profile-rule counter ("Checks" card). Severity applies when the count is
/// above zero; a zero count always renders as good.
/// </summary>
public class StatCheckItem
{
    public string Label { get; init; } = string.Empty;
    public int Count { get; init; }
    public StatCheckSeverity Severity { get; init; }
}

/// <summary>A ranked text + count entry (most used words/lines).</summary>
public class StatCountItem
{
    public string Text { get; init; } = string.Empty;
    public int Count { get; init; }
}

/// <summary>Characters-per-second distribution for the histogram.</summary>
public class StatCpsHistogram
{
    public const double BinSize = 2.0;
    public const double AxisMax = 30.0;

    /// <summary>Subtitle counts per CPS bin of <see cref="BinSize"/>, 0 to <see cref="AxisMax"/> (last bin catches the overflow).</summary>
    public List<int> Bins { get; init; } = new();
    public double OptimalCps { get; init; }
    public double MaximumCps { get; init; }
}
