using Nikse.SubtitleEdit.Features.Shared.BinaryEdit;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustDuration;
using System.Collections.Generic;

namespace UITests.Features.Shared.BinaryEdit;

/// <summary>
/// Guard tests for the 2026-08-28 bug hunt (sweep 20). The Binary edit window keeps its own
/// copies of the timing tools, and they had not been given the fixes their text-subtitle twins
/// received - two image subtitles on screen at once is invalid in Blu-ray SUP and VobSub, not
/// merely untidy, and a zero-length cue is worse still.
/// </summary>
public class BugHunt20BinaryEditTests
{
    private static BinarySubtitleItem Item(int startMs, int endMs) =>
        new(TimeSpan.FromMilliseconds(startMs), TimeSpan.FromMilliseconds(endMs));

    private static BinaryAdjustDurationViewModel MakeViewModel(BinaryAdjustDurationType type)
    {
        var vm = new BinaryAdjustDurationViewModel
        {
            SelectedAdjustType = BinaryAdjustDurationDisplay.ListAll().First(p => p.Type == type),
        };
        return vm;
    }

    [Fact]
    public void AdjustDurationFixed_TwoCuesSharingAStart_DoesNotProduceAZeroLengthCue()
    {
        // Capping the end flat at the next cue's start left nothing on screen at all when the
        // two share a start time. The Seconds branch in the same file already floors its result.
        var subtitles = new List<BinarySubtitleItem> { Item(1000, 3000), Item(1000, 4000) };
        var vm = MakeViewModel(BinaryAdjustDurationType.Fixed);
        vm.AdjustFixed = 5;

        vm.AdjustDuration(subtitles);

        Assert.True(subtitles[0].Duration > TimeSpan.Zero, "the first cue was left with no duration");
    }

    [Fact]
    public void AdjustDurationPercent_TwoCuesSharingAStart_DoesNotProduceAZeroLengthCue()
    {
        var subtitles = new List<BinarySubtitleItem> { Item(1000, 3000), Item(1000, 4000) };
        var vm = MakeViewModel(BinaryAdjustDurationType.Percent);
        vm.AdjustPercent = 200;

        vm.AdjustDuration(subtitles);

        Assert.True(subtitles[0].Duration > TimeSpan.Zero, "the first cue was left with no duration");
    }

    [Fact]
    public void AdjustDurationFixed_LeavesAGapBeforeTheNextCue()
    {
        var subtitles = new List<BinarySubtitleItem> { Item(1000, 2000), Item(3000, 4000) };
        var vm = MakeViewModel(BinaryAdjustDurationType.Fixed);
        vm.AdjustFixed = 10;

        vm.AdjustDuration(subtitles);

        Assert.True(
            subtitles[0].EndTime < subtitles[1].StartTime,
            "the capped cue ends exactly where the next one starts - two images at once");
    }

    [Fact]
    public void AdjustDurationRecalculate_WithoutText_LeavesTheCueAlone()
    {
        // Image subtitles carry no text until they are OCR'd. The window blocks Recalculate in
        // that case; this pins the model itself so a caller cannot collapse a cue to zero.
        var subtitles = new List<BinarySubtitleItem> { Item(1000, 3000), Item(5000, 7000) };
        var vm = MakeViewModel(BinaryAdjustDurationType.Recalculate);
        vm.AdjustRecalculateOptimalCharacterPerSecond = 15;
        vm.AdjustRecalculateMaxCharacterPerSecond = 20;

        vm.AdjustDuration(subtitles);

        Assert.Equal(TimeSpan.FromMilliseconds(2000), subtitles[0].Duration);
    }
}
