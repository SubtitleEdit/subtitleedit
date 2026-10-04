using Nikse.SubtitleEdit.Features.Files.Compare;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeCompare
{
    /// <summary>
    /// Name of a <see cref="CompareVisualType"/> value - enums travel as strings in Settings.json,
    /// so a reordered enum cannot silently change what a saved setting means.
    /// </summary>
    public string Show { get; set; } = nameof(CompareVisualType.All);

    public bool IgnoreWhitespace { get; set; }
    public bool IgnoreFormatting { get; set; }
    public bool IgnoreNumbering { get; set; }

    /// <summary>Start and end times this many milliseconds apart or closer count as equal (#15620).</summary>
    public int IgnoreTimeDifferenceMs { get; set; }
}
