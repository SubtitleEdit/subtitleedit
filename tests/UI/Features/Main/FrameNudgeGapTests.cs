using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;

namespace UITests.Features.Main;

/// <summary>
/// "Move start/end one frame back/forward" and the minimum gap (issue #13999).
///
/// SE 4's MoveStartCurrent clamped a backwards nudge to "previous end + minimum gap"; the SE 5 port
/// checked only the nudged line's own end, so repeated presses walked the cue straight through its
/// neighbour and produced overlaps even with "Allow overlap (when moving/resizing)" off.
///
/// The KeepGapPrev / KeepGapNext variants are deliberately exempt - they carry the neighbour along,
/// so there is nothing to run into.
/// </summary>
public class FrameNudgeGapTests : IDisposable
{
    private const int GapMs = 24;
    private const double Fps = 25;
    private const double OneFrameMs = 1000.0 / Fps;

    private readonly List<Window> _windows = new();
    private readonly bool _allowOverlap = Se.Settings.Waveform.AllowOverlap;
    private readonly double _frameRate = Se.Settings.General.CurrentFrameRate;
    private readonly double _coreFrameRate = Configuration.Settings.General.CurrentFrameRate;
    private readonly bool _useFrameMode = Se.Settings.General.UseFrameMode;
    private readonly int _minBetweenMs = Se.Settings.General.MinimumBetweenLines.Milliseconds;
    private readonly int _minBetweenFrames = Se.Settings.General.MinimumBetweenLines.Frames;
    private readonly int _moveStartEndStepMs = Se.Settings.General.MoveStartEndStepMs;

    public void Dispose()
    {
        Se.Settings.Waveform.AllowOverlap = _allowOverlap;
        Se.Settings.General.CurrentFrameRate = _frameRate;
        Configuration.Settings.General.CurrentFrameRate = _coreFrameRate;
        Se.Settings.General.UseFrameMode = _useFrameMode;
        Se.Settings.General.MinimumBetweenLines.Milliseconds = _minBetweenMs;
        Se.Settings.General.MinimumBetweenLines.Frames = _minBetweenFrames;
        Se.Settings.General.MoveStartEndStepMs = _moveStartEndStepMs;
        foreach (var w in _windows)
        {
            w.Close();
        }
    }

    private (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        _windows.Add(window);
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        return (window, vm);
    }

    /// <summary>Two lines, the second starting <paramref name="gapMs"/> after the first ends.</summary>
    private (Window Window, MainViewModel Vm) TwoLines(int gapMs, bool allowOverlap = false, double fps = Fps, int minGapMs = GapMs)
    {
        // Everything the clamp reads is pinned here. The minimum gap comes from
        // MinimumBetweenLines *and* UseFrameMode, and the frame rate has two independent homes
        // (Se.Settings and the libse Configuration) - leaving any of them to whatever another test
        // in the run happened to set makes this test order-dependent, which is how it passed
        // locally and failed on CI.
        Se.Settings.Waveform.AllowOverlap = allowOverlap;
        Se.Settings.General.CurrentFrameRate = fps; // default: one frame = 40 ms
        Configuration.Settings.General.CurrentFrameRate = fps;
        Se.Settings.General.UseFrameMode = false;
        Se.Settings.General.MinimumBetweenLines.Milliseconds = minGapMs;

        var (window, vm) = CreateMainViewModel();
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("one", 1000, 3000), null!) { Number = 1 });
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("two", 3000 + gapMs, 6000), null!) { Number = 2 });
        Dispatcher.UIThread.RunJobs();
        return (window, vm);
    }

    private static double MinGapMs => Se.Settings.General.MinimumBetweenLines.GetMilliseconds();


    // "Move start/end X ms" (Reddit: steps finer than 100 ms to hit waveform edges) - same clamps
    // as the frame variants, but the step is the "Move start/end shortcut step (ms)" setting.
    [AvaloniaFact]
    public void MoveStartEndXMs_MovesBySettingStep()
    {
        var (window, vm) = TwoLines(gapMs: 1000);
        Se.Settings.General.MoveStartEndStepMs = 10;
        vm.SelectedSubtitle = vm.Subtitles[1];
        Dispatcher.UIThread.RunJobs();
        var start = vm.Subtitles[1].StartTime.TotalMilliseconds;
        var end = vm.Subtitles[1].EndTime.TotalMilliseconds;

        vm.MoveStartXMsBackCommand.Execute(null);
        Assert.Equal(start - 10, vm.Subtitles[1].StartTime.TotalMilliseconds, 3);
        vm.MoveStartXMsForwardCommand.Execute(null);
        vm.MoveStartXMsForwardCommand.Execute(null);
        Assert.Equal(start + 10, vm.Subtitles[1].StartTime.TotalMilliseconds, 3);

        vm.MoveEndXMsForwardCommand.Execute(null);
        Assert.Equal(end + 10, vm.Subtitles[1].EndTime.TotalMilliseconds, 3);
        vm.MoveEndXMsBackCommand.Execute(null);
        vm.MoveEndXMsBackCommand.Execute(null);
        Assert.Equal(end - 10, vm.Subtitles[1].EndTime.TotalMilliseconds, 3);
        window.Close();
    }

    [AvaloniaFact]
    public void MoveStartXMsBack_StopsAtTheMinimumGap()
    {
        var (window, vm) = TwoLines(gapMs: 100);
        Se.Settings.General.MoveStartEndStepMs = 7;
        vm.SelectedSubtitle = vm.Subtitles[1];
        Dispatcher.UIThread.RunJobs();

        var floor = vm.Subtitles[0].EndTime.TotalMilliseconds + MinGapMs;
        for (var i = 0; i < 50; i++)
        {
            vm.MoveStartXMsBackCommand.Execute(null);
        }

        Assert.True(vm.Subtitles[1].StartTime.TotalMilliseconds >= floor - 0.001,
            $"start {vm.Subtitles[1].StartTime.TotalMilliseconds} went past the floor {floor}");
        window.Close();
    }

    [AvaloniaFact]
    public void MoveStartBack_StopsAtTheMinimumGap()
    {
        var (window, vm) = TwoLines(gapMs: 1000);
        vm.SelectedSubtitle = vm.Subtitles[1];
        Dispatcher.UIThread.RunJobs();

        var floor = vm.Subtitles[0].EndTime.TotalMilliseconds + MinGapMs;
        for (var i = 0; i < 60; i++) // far more presses than the 1000 ms gap allows
        {
            vm.MoveStartOneFrameBackCommand.Execute(null);
        }

        Assert.True(vm.Subtitles[1].StartTime.TotalMilliseconds >= floor - 0.001,
            $"start {vm.Subtitles[1].StartTime.TotalMilliseconds} went past the floor {floor}");
        Assert.True(vm.Subtitles[1].StartTime.TotalMilliseconds >= vm.Subtitles[0].EndTime.TotalMilliseconds,
            "the nudged start overlapped the previous line");
        window.Close();
    }

    [AvaloniaFact]
    public void MoveEndForward_StopsAtTheMinimumGap()
    {
        var (window, vm) = TwoLines(gapMs: 1000);
        vm.SelectedSubtitle = vm.Subtitles[0];
        Dispatcher.UIThread.RunJobs();

        var ceiling = vm.Subtitles[1].StartTime.TotalMilliseconds - MinGapMs;
        for (var i = 0; i < 60; i++)
        {
            vm.MoveEndOneFrameForwardCommand.Execute(null);
        }

        Assert.True(vm.Subtitles[0].EndTime.TotalMilliseconds <= ceiling + 0.001,
            $"end {vm.Subtitles[0].EndTime.TotalMilliseconds} went past the ceiling {ceiling}");
        Assert.True(vm.Subtitles[0].EndTime.TotalMilliseconds <= vm.Subtitles[1].StartTime.TotalMilliseconds,
            "the nudged end overlapped the next line");
        window.Close();
    }

    // A single press well clear of the neighbour must still move a whole frame - the clamp must not
    // quietly round every nudge to the gap.
    [AvaloniaFact]
    public void MoveStartBack_AwayFromTheNeighbour_MovesAFullFrame()
    {
        var (window, vm) = TwoLines(gapMs: 1000);
        vm.SelectedSubtitle = vm.Subtitles[1];
        Dispatcher.UIThread.RunJobs();

        var before = vm.Subtitles[1].StartTime.TotalMilliseconds;
        vm.MoveStartOneFrameBackCommand.Execute(null);

        Assert.Equal(before - OneFrameMs, vm.Subtitles[1].StartTime.TotalMilliseconds, 3);
        window.Close();
    }

    [AvaloniaFact]
    public void MoveStartBack_WithAllowOverlapOn_IsNotClamped()
    {
        var (window, vm) = TwoLines(gapMs: 1000, allowOverlap: true);
        vm.SelectedSubtitle = vm.Subtitles[1];
        Dispatcher.UIThread.RunJobs();

        for (var i = 0; i < 60; i++)
        {
            vm.MoveStartOneFrameBackCommand.Execute(null);
        }

        // The user asked for overlap, so the cue is free to cross the previous line's end.
        Assert.True(vm.Subtitles[1].StartTime.TotalMilliseconds < vm.Subtitles[0].EndTime.TotalMilliseconds);
        window.Close();
    }

    // Lines that already overlap are left alone: a one-frame nudge is not a repair tool, and
    // jumping the cue forward to the gap would be a surprise.
    [AvaloniaFact]
    public void MoveStartBack_WhenAlreadyOverlapping_DoesNothing()
    {
        var (window, vm) = TwoLines(gapMs: -500);
        vm.SelectedSubtitle = vm.Subtitles[1];
        Dispatcher.UIThread.RunJobs();

        var before = vm.Subtitles[1].StartTime.TotalMilliseconds;
        vm.MoveStartOneFrameBackCommand.Execute(null);

        Assert.Equal(before, vm.Subtitles[1].StartTime.TotalMilliseconds, 3);
        window.Close();
    }

    // Issue #15511: with "Min gap" = 2 frames, walking the end towards the next line with the
    // KeepGapNext shortcut stopped at a 3-frame gap - the "close" test allowed a whole frame of
    // slack, so a gap of minimum + 1 frame already counted as close and was then preserved.
    [AvaloniaFact]
    public void MoveEndForwardKeepGapNext_ApproachingTheNeighbour_SettlesOnTheMinimumGap()
    {
        var (window, vm) = TwoLinesInFrameMode(gapMs: 400, minGapFrames: 2);
        vm.SelectedSubtitle = vm.Subtitles[0];
        Dispatcher.UIThread.RunJobs();

        for (var i = 0; i < 20; i++)
        {
            vm.MoveEndOneFrameForwardKeepGapNextCommand.Execute(null);
        }

        var gap = vm.Subtitles[1].StartTime.TotalMilliseconds - vm.Subtitles[0].EndTime.TotalMilliseconds;
        Assert.Equal(2 * OneFrameMs, gap, 3);
        window.Close();
    }

    [AvaloniaFact]
    public void MoveStartBackKeepGapPrev_ApproachingTheNeighbour_SettlesOnTheMinimumGap()
    {
        var (window, vm) = TwoLinesInFrameMode(gapMs: 400, minGapFrames: 2);
        vm.SelectedSubtitle = vm.Subtitles[1];
        Dispatcher.UIThread.RunJobs();

        for (var i = 0; i < 20; i++)
        {
            vm.MoveStartOneFrameBackKeepGapPrevCommand.Execute(null);
        }

        var gap = vm.Subtitles[1].StartTime.TotalMilliseconds - vm.Subtitles[0].EndTime.TotalMilliseconds;
        Assert.Equal(2 * OneFrameMs, gap, 3);
        window.Close();
    }

    // Once at the minimum gap the shortcut carries the neighbour along instead of closing the gap.
    [AvaloniaFact]
    public void MoveEndForwardKeepGapNext_AtTheMinimumGap_KeepsIt()
    {
        var (window, vm) = TwoLinesInFrameMode(gapMs: 80, minGapFrames: 2);
        vm.SelectedSubtitle = vm.Subtitles[0];
        Dispatcher.UIThread.RunJobs();

        vm.MoveEndOneFrameForwardKeepGapNextCommand.Execute(null);

        Assert.Equal(3000 + OneFrameMs, vm.Subtitles[0].EndTime.TotalMilliseconds, 3);
        Assert.Equal(80, vm.Subtitles[1].StartTime.TotalMilliseconds - vm.Subtitles[0].EndTime.TotalMilliseconds, 3);
        window.Close();
    }

    // The smallest on-frame gap at or above a millisecond minimum can be most of a frame above it
    // (24 ms at 50 fps: 40 ms; 24 ms at 59.94 fps: 33 ms; 100 ms at 23.976 fps: 125 ms). Such a gap
    // is "close": the KeepGap variants must carry the neighbour along rather than clamp to
    // "neighbour + minimum", which is off the frame grid.
    [AvaloniaTheory]
    [InlineData(50.0, 24, 40)]
    [InlineData(59.94, 24, 33)]
    [InlineData(23.976, 100, 125)]
    public void MoveEndForwardKeepGapNext_SmallestOnFrameGapAboveMsMinimum_CarriesTheNeighbour(double fps, int minGapMs, int gapMs)
    {
        var (window, vm) = TwoLines(gapMs, fps: fps, minGapMs: minGapMs);
        vm.SelectedSubtitle = vm.Subtitles[0];
        Dispatcher.UIThread.RunJobs();

        vm.MoveEndOneFrameForwardKeepGapNextCommand.Execute(null);

        Assert.True(vm.Subtitles[0].EndTime.TotalMilliseconds > 3000);
        Assert.Equal(gapMs, vm.Subtitles[1].StartTime.TotalMilliseconds - vm.Subtitles[0].EndTime.TotalMilliseconds, 3);
        window.Close();
    }

    [AvaloniaTheory]
    [InlineData(50.0, 24, 40)]
    [InlineData(59.94, 24, 33)]
    [InlineData(23.976, 100, 125)]
    public void MoveStartBackKeepGapPrev_SmallestOnFrameGapAboveMsMinimum_CarriesTheNeighbour(double fps, int minGapMs, int gapMs)
    {
        var (window, vm) = TwoLines(gapMs, fps: fps, minGapMs: minGapMs);
        vm.SelectedSubtitle = vm.Subtitles[1];
        Dispatcher.UIThread.RunJobs();

        vm.MoveStartOneFrameBackKeepGapPrevCommand.Execute(null);

        Assert.True(vm.Subtitles[0].EndTime.TotalMilliseconds < 3000);
        Assert.Equal(gapMs, vm.Subtitles[1].StartTime.TotalMilliseconds - vm.Subtitles[0].EndTime.TotalMilliseconds, 3);
        window.Close();
    }

    private (Window Window, MainViewModel Vm) TwoLinesInFrameMode(int gapMs, int minGapFrames)
    {
        Se.Settings.General.MinimumBetweenLines.Frames = minGapFrames;
        var result = TwoLines(gapMs);
        Se.Settings.General.UseFrameMode = true;
        return result;
    }
}
