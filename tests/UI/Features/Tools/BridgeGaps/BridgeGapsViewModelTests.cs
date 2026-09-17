using System;
using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Settings.MinGapCalculate;
using Nikse.SubtitleEdit.Features.Tools.BridgeGaps;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Tools.BridgeGaps;

/// <summary>
/// Style guides give bridge gaps in whole frames. Subtitle Edit 4 took frames in HH:MM:SS:FF
/// mode, 5.2 only milliseconds (#14959). In frame mode the boxes now hold frames again, kept in
/// their own settings keys so a value is never read back in the other unit.
/// </summary>
public class BridgeGapsViewModelTests : IDisposable
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private readonly SettingsScope _settings = new(
        "General.UseFrameMode",
        "Tools.BridgeGaps.BridgeGapsSmallerThanMs",
        "Tools.BridgeGaps.MinGapMs",
        "Tools.BridgeGaps.PercentForLeft",
        "Tools.BridgeGaps.BridgeGapsSmallerThanFrames",
        "Tools.BridgeGaps.MinGapFrames");

    private readonly double _currentFrameRate = Configuration.Settings.General.CurrentFrameRate;
    private readonly List<BridgeGapsViewModel> _viewModels = new();

    public BridgeGapsViewModelTests()
    {
        Configuration.Settings.General.CurrentFrameRate = 25;
        Se.Settings.Tools.BridgeGaps.PercentForLeft = 100;
    }

    public void Dispose()
    {
        foreach (var vm in _viewModels)
        {
            vm.OnClosingCleanup();
        }

        Configuration.Settings.General.CurrentFrameRate = _currentFrameRate;
        _settings.Dispose();
    }

    private BridgeGapsViewModel MakeViewModel(bool frameMode)
    {
        Se.Settings.General.UseFrameMode = frameMode;
        var vm = new BridgeGapsViewModel(new WindowService(new NullServiceProvider()));
        _viewModels.Add(vm);
        return vm;
    }

    // A 400 ms gap after the first line.
    private static List<SubtitleLineViewModel> MakeSubtitles() => new()
    {
        new SubtitleLineViewModel { Text = "One", StartTime = TimeSpan.FromMilliseconds(1000), EndTime = TimeSpan.FromMilliseconds(2000) },
        new SubtitleLineViewModel { Text = "Two", StartTime = TimeSpan.FromMilliseconds(2400), EndTime = TimeSpan.FromMilliseconds(3000) },
    };

    private static double EndOfFirstLineAfterOk(BridgeGapsViewModel vm)
    {
        vm.Initialize(MakeSubtitles());
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();
        return vm.Subtitles[0].EndTime.TotalMilliseconds;
    }

    [AvaloniaFact]
    public void FrameModeLabelsTheBoxesInFramesAndHidesTheMillisecondCalculators()
    {
        var vm = MakeViewModel(frameMode: true);

        Assert.Equal(Se.Language.Tools.BridgeGaps.BridgeGapsSmallerThanFrames, vm.BridgeGapsSmallerThanLabel);
        Assert.Equal(Se.Language.Tools.BridgeGaps.MinGapFrames, vm.MinGapLabel);
        Assert.False(vm.IsMsMode);
    }

    [AvaloniaFact]
    public void MillisecondModeKeepsMillisecondLabelsAndOffersTheCalculators()
    {
        var vm = MakeViewModel(frameMode: false);

        Assert.Equal(Se.Language.Tools.BridgeGaps.BridgeGapsSmallerThan, vm.BridgeGapsSmallerThanLabel);
        Assert.Equal(Se.Language.Tools.BridgeGaps.MinGap, vm.MinGapLabel);
        Assert.True(vm.IsMsMode);
    }

    [AvaloniaFact]
    public void FrameValuesAreCountedAtTheFrameRate()
    {
        Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanFrames = 12; // 480 ms at 25 fps
        Se.Settings.Tools.BridgeGaps.MinGapFrames = 2; // 80 ms
        var vm = MakeViewModel(frameMode: true);

        Assert.Equal(12, vm.BridgeGapsSmallerThanMsOrFrames);
        Assert.Equal(2, vm.MinGapMsOrFrames);
        Assert.Equal(2320, EndOfFirstLineAfterOk(vm));
    }

    [AvaloniaFact]
    public void AGapWiderThanTheFrameLimitIsLeftAlone()
    {
        Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanFrames = 8; // 320 ms, the gap is 400 ms
        Se.Settings.Tools.BridgeGaps.MinGapFrames = 2;
        var vm = MakeViewModel(frameMode: true);

        Assert.Equal(2000, EndOfFirstLineAfterOk(vm));
    }

    [AvaloniaFact]
    public void FirstFrameModeUseStartsFromTheMillisecondValues()
    {
        Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanMs = 2000;
        Se.Settings.Tools.BridgeGaps.MinGapMs = 80;
        Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanFrames = null;
        Se.Settings.Tools.BridgeGaps.MinGapFrames = null;

        var vm = MakeViewModel(frameMode: true);

        Assert.Equal(50, vm.BridgeGapsSmallerThanMsOrFrames);
        Assert.Equal(2, vm.MinGapMsOrFrames);
    }

    [AvaloniaFact]
    public void FramesAreSavedAsFramesAndTheMillisecondKeysFollow()
    {
        Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanMs = 2000;
        Se.Settings.Tools.BridgeGaps.MinGapMs = 24;
        var vm = MakeViewModel(frameMode: true);
        vm.BridgeGapsSmallerThanMsOrFrames = 12;
        vm.MinGapMsOrFrames = 2;

        EndOfFirstLineAfterOk(vm);

        Assert.Equal(12, Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanFrames);
        Assert.Equal(2, Se.Settings.Tools.BridgeGaps.MinGapFrames);
        // Merge short lines reads the millisecond key as its gap threshold.
        Assert.Equal(480, Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanMs);
        Assert.Equal(80, Se.Settings.Tools.BridgeGaps.MinGapMs);
    }

    [AvaloniaFact]
    public void MillisecondsNeverOverwriteTheFrameValues()
    {
        Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanFrames = 12;
        Se.Settings.Tools.BridgeGaps.MinGapFrames = 2;
        var vm = MakeViewModel(frameMode: false);
        vm.BridgeGapsSmallerThanMsOrFrames = 500;
        vm.MinGapMsOrFrames = 100;

        Assert.Equal(2300, EndOfFirstLineAfterOk(vm));

        Assert.Equal(500, Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanMs);
        Assert.Equal(100, Se.Settings.Tools.BridgeGaps.MinGapMs);
        Assert.Equal(12, Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanFrames);
        Assert.Equal(2, Se.Settings.Tools.BridgeGaps.MinGapFrames);

        var reopened = MakeViewModel(frameMode: true);
        Assert.Equal(12, reopened.BridgeGapsSmallerThanMsOrFrames);
        Assert.Equal(2, reopened.MinGapMsOrFrames);
    }

    [AvaloniaFact]
    public void TheCalculatorCanNameTheBridgeGapLimitInsteadOfTheMinimumGap()
    {
        var calculator = new MinGapCalculateViewModel();
        calculator.Initialize(
            12,
            Se.Language.Tools.BridgeGaps.BridgeGapsSmallerThan,
            Se.Language.Tools.BridgeGaps.BridgeGapsSmallerThanFrames,
            Se.Language.Tools.BridgeGaps.UseXMsAsBridgeGapsSmallerThan);

        Assert.Equal(480, calculator.MinGapMs);
        Assert.Equal(Se.Language.Tools.BridgeGaps.BridgeGapsSmallerThan, calculator.Title);
        Assert.Equal(Se.Language.Tools.BridgeGaps.BridgeGapsSmallerThanFrames, calculator.FramesLabel);
        Assert.Equal(string.Format(Se.Language.Tools.BridgeGaps.UseXMsAsBridgeGapsSmallerThan, 480), calculator.UseAsNewGapText);
    }

    [AvaloniaFact]
    public void TheCalculatorKeepsTheMinimumGapTextsByDefault()
    {
        var calculator = new MinGapCalculateViewModel();
        calculator.Initialize(2);

        Assert.Equal(Se.Language.Options.Settings.MinGapCalculateTitle, calculator.Title);
        Assert.Equal(Se.Language.Options.Settings.MinGapCalculateFrames, calculator.FramesLabel);
        Assert.Equal(string.Format(Se.Language.Options.Settings.MinGapCalculateUseXAsNewGap, 80), calculator.UseAsNewGapText);
    }
}
