using System.Reflection;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameText;
using Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameTimeCodes;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Tools;

/// <summary>
/// Merge lines with same text / same time codes take their gap in frames in frame mode, like
/// Bridge gaps and Apply min gap. The setting stays in milliseconds.
/// </summary>
public class MergeSameFrameModeTests : IDisposable
{
    private readonly SettingsScope _settings = new(
        "General.UseFrameMode",
        "General.CurrentFrameRate",
        "Tools.MergeSameText.MaxMillisecondsBetweenLines",
        "Tools.MergeSameTimeCode.MaxMillisecondsDifference");

    private readonly double _currentFrameRate = Configuration.Settings.General.CurrentFrameRate;

    public MergeSameFrameModeTests()
    {
        // Both copies: saving the settings syncs Se's frame rate into libse.
        Configuration.Settings.General.CurrentFrameRate = 25;
        Se.Settings.General.CurrentFrameRate = 25;
    }

    public void Dispose()
    {
        Configuration.Settings.General.CurrentFrameRate = _currentFrameRate;
        _settings.Dispose();
    }

    private static void Invoke(object vm, string method) =>
        vm.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(vm, null);

    // Two lines one frame (40 ms at 25 fps) apart.
    private static List<SubtitleLineViewModel> MakeSubtitles() => new()
    {
        new SubtitleLineViewModel { Text = "Hello", StartTime = TimeSpan.FromMilliseconds(1000), EndTime = TimeSpan.FromMilliseconds(2000) },
        new SubtitleLineViewModel { Text = "Hello", StartTime = TimeSpan.FromMilliseconds(2040), EndTime = TimeSpan.FromMilliseconds(3000) },
    };

    private static List<SubtitleLineViewModel> MakeSameTimeSubtitles() => new()
    {
        new SubtitleLineViewModel { Text = "One", StartTime = TimeSpan.FromMilliseconds(1000), EndTime = TimeSpan.FromMilliseconds(2000) },
        new SubtitleLineViewModel { Text = "Two", StartTime = TimeSpan.FromMilliseconds(1040), EndTime = TimeSpan.FromMilliseconds(2040) },
    };

    [AvaloniaFact]
    public void MergeSameText_FrameModeGapIsInFrames()
    {
        Se.Settings.General.UseFrameMode = true;
        Se.Settings.Tools.MergeSameText.MaxMillisecondsBetweenLines = 250;

        var vm = new MergeSameTextViewModel();
        vm.Initialize(MakeSubtitles());
        vm.OnClosingCleanup();

        Assert.Equal(Se.Language.Tools.MergeLinesWithSameText.MaxFramesBetweenLines, vm.MaxBetweenLinesLabel);
        Assert.Equal(6, vm.MaxMsOrFramesBetweenLines);

        vm.MaxMsOrFramesBetweenLines = 0;
        Invoke(vm, "UpdatePreview");
        Assert.Empty(vm.MergeItems);

        vm.MaxMsOrFramesBetweenLines = 1;
        Invoke(vm, "UpdatePreview");
        Assert.NotEmpty(vm.MergeItems);

        // Saved back in milliseconds.
        Invoke(vm, "SaveSettings");
        Assert.Equal(40, Se.Settings.Tools.MergeSameText.MaxMillisecondsBetweenLines);
    }

    [AvaloniaFact]
    public void MergeSameText_UnchangedFramesKeepTheSavedMilliseconds()
    {
        Se.Settings.General.UseFrameMode = true;
        Se.Settings.Tools.MergeSameText.MaxMillisecondsBetweenLines = 250; // shown as 6 frames = 240 ms

        var vm = new MergeSameTextViewModel();
        vm.OnClosingCleanup();
        Invoke(vm, "SaveSettings");

        Assert.Equal(250, Se.Settings.Tools.MergeSameText.MaxMillisecondsBetweenLines);
    }

    [AvaloniaFact]
    public void MergeSameText_TimeModeStaysInMilliseconds()
    {
        Se.Settings.General.UseFrameMode = false;
        Se.Settings.Tools.MergeSameText.MaxMillisecondsBetweenLines = 250;

        var vm = new MergeSameTextViewModel();
        vm.OnClosingCleanup();

        Assert.Equal(Se.Language.Tools.MergeLinesWithSameText.MaxMsBetweenLines, vm.MaxBetweenLinesLabel);
        Assert.Equal(250, vm.MaxMsOrFramesBetweenLines);
    }

    [AvaloniaFact]
    public void MergeSameTimeCodes_FrameModeDifferenceIsInFrames()
    {
        Se.Settings.General.UseFrameMode = true;
        Se.Settings.Tools.MergeSameTimeCode.MaxMillisecondsDifference = 250;

        var vm = new MergeSameTimeCodesViewModel();
        vm.Initialize(MakeSameTimeSubtitles(), new Subtitle());
        vm.OnClosingCleanup();

        Assert.Equal(Se.Language.Tools.MergeLinesWithSameTimeCodes.MaxFramesDifference, vm.MaxDifferenceLabel);
        Assert.Equal(6, vm.MaxMsOrFramesDifference);

        vm.MaxMsOrFramesDifference = 0;
        Invoke(vm, "UpdatePreview");
        Assert.Empty(vm.MergeItems);

        vm.MaxMsOrFramesDifference = 1;
        Invoke(vm, "UpdatePreview");
        Assert.NotEmpty(vm.MergeItems);

        Invoke(vm, "SaveSettings");
        Assert.Equal(40, Se.Settings.Tools.MergeSameTimeCode.MaxMillisecondsDifference);
    }

    private static void Use2997()
    {
        Configuration.Settings.General.CurrentFrameRate = 29.97;
        Se.Settings.General.CurrentFrameRate = 29.97;
    }

    [AvaloniaFact]
    public void MergeSameText_OneFrameGapAt2997IsFoundWhenItRoundsTo34Ms()
    {
        // Frame 1 is 33 ms and frame 2 is 67 ms: one frame apart, but 34 ms - more than the
        // 33 ms one frame converts to.
        Use2997();
        Se.Settings.General.UseFrameMode = true;

        var vm = new MergeSameTextViewModel();
        vm.Initialize(new List<SubtitleLineViewModel>
        {
            new() { Text = "Hello", StartTime = TimeSpan.Zero, EndTime = TimeSpan.FromMilliseconds(33) },
            new() { Text = "Hello", StartTime = TimeSpan.FromMilliseconds(67), EndTime = TimeSpan.FromMilliseconds(1000) },
        });
        vm.OnClosingCleanup();

        vm.MaxMsOrFramesBetweenLines = 1;
        Invoke(vm, "UpdatePreview");
        Assert.NotEmpty(vm.MergeItems);

        vm.MaxMsOrFramesBetweenLines = 0;
        Invoke(vm, "UpdatePreview");
        Assert.Empty(vm.MergeItems);
    }

    [AvaloniaFact]
    public void MergeSameTimeCodes_OneFrameDifferenceAt2997IsFoundWhenItRoundsTo34Ms()
    {
        Use2997();
        Se.Settings.General.UseFrameMode = true;

        var vm = new MergeSameTimeCodesViewModel();
        vm.Initialize(new List<SubtitleLineViewModel>
        {
            // Starts at frames 1 and 2 (33 / 67 ms), ends at frames 30 and 31 (1001 / 1034 ms).
            new() { Text = "One", StartTime = TimeSpan.FromMilliseconds(33), EndTime = TimeSpan.FromMilliseconds(1001) },
            new() { Text = "Two", StartTime = TimeSpan.FromMilliseconds(67), EndTime = TimeSpan.FromMilliseconds(1034) },
        }, new Subtitle());
        vm.OnClosingCleanup();

        vm.MaxMsOrFramesDifference = 1;
        Invoke(vm, "UpdatePreview");
        Assert.NotEmpty(vm.MergeItems);

        vm.MaxMsOrFramesDifference = 0;
        Invoke(vm, "UpdatePreview");
        Assert.Empty(vm.MergeItems);
    }

    [Fact]
    public void MergeSameTimeCodes_MillisecondModeIsUnchanged()
    {
        var p = new SubtitleLineViewModel { StartTime = TimeSpan.FromMilliseconds(33), EndTime = TimeSpan.FromMilliseconds(1000) };
        var next = new SubtitleLineViewModel { StartTime = TimeSpan.FromMilliseconds(67), EndTime = TimeSpan.FromMilliseconds(1000) };

        Assert.False(MergeSameTimeCodesViewModel.QualifiesForMerge(p, next, 33));
        Assert.True(MergeSameTimeCodesViewModel.QualifiesForMerge(p, next, 34));
    }
}
