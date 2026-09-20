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
/// "Move lines: shorten previous/next line instead of overlapping it" (#15098): a "move selected
/// lines X ms" that runs into the line before/after trims that line (keeping the minimum gap)
/// rather than overlapping it, never below the minimum duration, and leaves alone a neighbor that
/// was overlapping already.
/// </summary>
public class MoveLinesShortenNeighborTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly int _stepMs = Se.Settings.General.MoveSelectedLinesStepMs;
    private readonly bool _shorten = Se.Settings.General.MoveLinesShortenNeighbor;
    private readonly bool _locked = Se.Settings.General.LockTimeCodes;
    private readonly bool _frameMode = Se.Settings.General.UseFrameMode;
    private readonly int _gapMs = Se.Settings.General.MinimumBetweenLines.Milliseconds;
    private readonly int _minDurMs = Se.Settings.General.SubtitleMinimumDisplayMilliseconds;

    public void Dispose()
    {
        Se.Settings.General.MoveSelectedLinesStepMs = _stepMs;
        Se.Settings.General.MoveLinesShortenNeighbor = _shorten;
        Se.Settings.General.LockTimeCodes = _locked;
        Se.Settings.General.UseFrameMode = _frameMode;
        Se.Settings.General.MinimumBetweenLines.Milliseconds = _gapMs;
        Se.Settings.General.SubtitleMinimumDisplayMilliseconds = _minDurMs;
        foreach (var w in _windows)
        {
            w.Close();
        }
    }

    // one: 1000-2000, two: 2050-3000, three: 3050-4000 (50 ms gaps, minimum gap 24 ms)
    private MainViewModel ThreeCloseLines(bool shorten = true)
    {
        Se.Settings.General.MoveSelectedLinesStepMs = 100;
        Se.Settings.General.MoveLinesShortenNeighbor = shorten;
        Se.Settings.General.LockTimeCodes = false;
        Se.Settings.General.UseFrameMode = false;
        Se.Settings.General.MinimumBetweenLines.Milliseconds = 24;
        Se.Settings.General.SubtitleMinimumDisplayMilliseconds = 500;

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

        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("one", 1000, 2000), null!) { Number = 1 });
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("two", 2050, 3000), null!) { Number = 2 });
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("three", 3050, 4000), null!) { Number = 3 });
        Dispatcher.UIThread.RunJobs();
        return vm;
    }

    private static void Select(MainViewModel vm, int index)
    {
        vm.SelectedSubtitle = vm.Subtitles[index];
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void Off_Back_OverlapsPreviousLine()
    {
        var vm = ThreeCloseLines(shorten: false);
        Select(vm, 1);

        vm.MoveSelectedLinesXMsBackCommand.Execute(null);

        Assert.Equal(1950, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(2000, vm.Subtitles[0].EndTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void Back_ShortensPreviousLine_KeepsMinimumGap()
    {
        var vm = ThreeCloseLines();
        Select(vm, 1);

        vm.MoveSelectedLinesXMsBackCommand.Execute(null);

        Assert.Equal(1950, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(2900, vm.Subtitles[1].EndTime.TotalMilliseconds);
        Assert.Equal(1000, vm.Subtitles[0].StartTime.TotalMilliseconds);
        Assert.Equal(1926, vm.Subtitles[0].EndTime.TotalMilliseconds);
        Assert.Equal(3050, vm.Subtitles[2].StartTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void Forward_PushesNextLineStart_KeepsItsEnd()
    {
        var vm = ThreeCloseLines();
        Select(vm, 1);

        vm.MoveSelectedLinesXMsForwardCommand.Execute(null);

        Assert.Equal(2150, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(3100, vm.Subtitles[1].EndTime.TotalMilliseconds);
        Assert.Equal(3124, vm.Subtitles[2].StartTime.TotalMilliseconds);
        Assert.Equal(4000, vm.Subtitles[2].EndTime.TotalMilliseconds);
        Assert.Equal(2000, vm.Subtitles[0].EndTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void AndForward_Back_ShortensLineBeforeTheBlock()
    {
        var vm = ThreeCloseLines();
        Select(vm, 1);

        vm.MoveSelectedLinesAndForwardXMsBackCommand.Execute(null);

        Assert.Equal(1926, vm.Subtitles[0].EndTime.TotalMilliseconds);
        Assert.Equal(1950, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(2900, vm.Subtitles[1].EndTime.TotalMilliseconds);
        Assert.Equal(2950, vm.Subtitles[2].StartTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void NoContact_LeavesNeighborAlone()
    {
        var vm = ThreeCloseLines();
        vm.Subtitles[0].EndTime = TimeSpan.FromMilliseconds(1500);
        Select(vm, 1);

        vm.MoveSelectedLinesXMsBackCommand.Execute(null);

        Assert.Equal(1950, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(1500, vm.Subtitles[0].EndTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void AlreadyOverlapping_LeavesNeighborAlone()
    {
        var vm = ThreeCloseLines();
        vm.Subtitles[0].EndTime = TimeSpan.FromMilliseconds(2500);
        Select(vm, 1);

        vm.MoveSelectedLinesXMsBackCommand.Execute(null);

        Assert.Equal(1950, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(2500, vm.Subtitles[0].EndTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void Back_StopsWhenPreviousLineIsAtMinimumDuration()
    {
        var vm = ThreeCloseLines();
        Se.Settings.General.SubtitleMinimumDisplayMilliseconds = 950;
        Select(vm, 1);

        // "one" may shrink to 1000-1950, so "two" can only reach 1974 (76 ms of the 100 ms step).
        vm.MoveSelectedLinesXMsBackCommand.Execute(null);
        Assert.Equal(1974, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(2924, vm.Subtitles[1].EndTime.TotalMilliseconds);
        Assert.Equal(1950, vm.Subtitles[0].EndTime.TotalMilliseconds);

        vm.MoveSelectedLinesXMsBackCommand.Execute(null);
        Assert.Equal(1974, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(1950, vm.Subtitles[0].EndTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void MultiSelection_OnlyTheLineOutsideTheSelectionIsShortened()
    {
        var vm = ThreeCloseLines();
        Select(vm, 1);
        vm.SubtitleGrid.SelectedItems!.Add(vm.Subtitles[2]);
        Dispatcher.UIThread.RunJobs();

        vm.MoveSelectedLinesXMsBackCommand.Execute(null);

        Assert.Equal(1926, vm.Subtitles[0].EndTime.TotalMilliseconds);
        Assert.Equal(2900, vm.Subtitles[1].EndTime.TotalMilliseconds);
        Assert.Equal(2950, vm.Subtitles[2].StartTime.TotalMilliseconds);
    }
}
