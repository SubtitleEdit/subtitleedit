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
    private readonly List<Window> _windows = new();
    private readonly bool _allowOverlap = Se.Settings.Waveform.AllowOverlap;
    private readonly double _frameRate = Se.Settings.General.CurrentFrameRate;

    public void Dispose()
    {
        Se.Settings.Waveform.AllowOverlap = _allowOverlap;
        Se.Settings.General.CurrentFrameRate = _frameRate;
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
    private (Window Window, MainViewModel Vm) TwoLines(int gapMs, bool allowOverlap = false)
    {
        Se.Settings.Waveform.AllowOverlap = allowOverlap;
        Se.Settings.General.CurrentFrameRate = 25; // one frame = 40 ms

        var (window, vm) = CreateMainViewModel();
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("one", 1000, 3000), null!) { Number = 1 });
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("two", 3000 + gapMs, 6000), null!) { Number = 2 });
        Dispatcher.UIThread.RunJobs();
        return (window, vm);
    }

    private static double MinGapMs => Se.Settings.General.MinimumBetweenLines.GetMilliseconds();

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
        Assert.True(vm.Subtitles[1].StartTime.TotalMilliseconds > vm.Subtitles[0].EndTime.TotalMilliseconds);
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
        Assert.True(vm.Subtitles[0].EndTime.TotalMilliseconds < vm.Subtitles[1].StartTime.TotalMilliseconds);
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

        Assert.Equal(before - 40, vm.Subtitles[1].StartTime.TotalMilliseconds, 3);
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
}
