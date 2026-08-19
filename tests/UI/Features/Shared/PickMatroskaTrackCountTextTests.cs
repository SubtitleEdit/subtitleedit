using Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Shared;

/// <summary>
/// A movie can hold several image-based subtitle tracks in the same language where only one is the
/// forced/signs track. The count line under the preview reports how many cues carry the forced
/// flag, so the tracks can be told apart without opening each one (#13453).
/// </summary>
public class PickMatroskaTrackCountTextTests
{
    [Fact]
    public void FormatSubtitleCount_WithoutForcedFlag_ShowsPlainCount()
    {
        var text = PickMatroskaTrackViewModel.FormatSubtitleCount(42, null);

        Assert.Equal(string.Format(Se.Language.File.Import.NumberOfSubtitlesX, 42), text);
        Assert.DoesNotContain("forced", text, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(842, 19)]
    [InlineData(19, 19)]
    [InlineData(842, 0)] // shown even at zero: "no forced cues" is the answer the user is after
    public void FormatSubtitleCount_WithForcedFlag_ShowsBothCounts(int count, int forcedCount)
    {
        var text = PickMatroskaTrackViewModel.FormatSubtitleCount(count, forcedCount);

        Assert.Equal(string.Format(Se.Language.File.Import.NumberOfSubtitlesXForcedY, count, forcedCount), text);
        Assert.Contains(count.ToString(), text);
        Assert.Contains($"({forcedCount} forced)", text);
    }
}
