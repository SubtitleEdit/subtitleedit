using Nikse.SubtitleEdit.Logic.Config;
using System.Text.Json;
using Xunit;

namespace UITests.Logic.Config;

public class BatchConvertPresetJsonTests
{
    // Split/break settings are shared tool settings batch convert edits directly, so a preset
    // carries its own copies - they must survive the settings JSON round-trip.
    [Fact]
    public void SplitBreakSettingsRoundTrip()
    {
        var preset = new SeBatchConvertPreset
        {
            Name = "A",
            SplitRebalanceLongLinesSplit = false,
            SplitRebalanceLongLinesRebalance = true,
            SplitRebalanceLongLinesRebalanceOnlyTooLong = true,
            SplitRebalanceLongLinesSingleLineMaxLength = 42,
            SplitRebalanceLongLinesMaxNumberOfLines = 2,
            SplitRebalanceLongLinesUnbreakShorterThan = 10,
        };

        var json = JsonSerializer.Serialize(preset, SeJsonContext.Default.SeBatchConvertPreset);
        var loaded = JsonSerializer.Deserialize(json, SeJsonContext.Default.SeBatchConvertPreset)!;

        Assert.False(loaded.SplitRebalanceLongLinesSplit);
        Assert.True(loaded.SplitRebalanceLongLinesRebalance);
        Assert.True(loaded.SplitRebalanceLongLinesRebalanceOnlyTooLong);
        Assert.Equal(42, loaded.SplitRebalanceLongLinesSingleLineMaxLength);
        Assert.Equal(2, loaded.SplitRebalanceLongLinesMaxNumberOfLines);
        Assert.Equal(10, loaded.SplitRebalanceLongLinesUnbreakShorterThan);

        // Presets saved before these fields existed leave them null (keep current settings).
        var old = JsonSerializer.Deserialize("{\"Name\":\"B\"}", SeJsonContext.Default.SeBatchConvertPreset)!;
        Assert.Null(old.SplitRebalanceLongLinesSingleLineMaxLength);
        Assert.Null(old.SplitRebalanceLongLinesSplit);
    }
}
