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
/// "Move selected lines X ms back/forward" and the "and all following" variants (#14789):
/// durations are kept, only the intended lines move, and a backward move is clamped so no start
/// goes below zero.
/// </summary>
public class MoveSelectedLinesStepTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly int _stepMs = Se.Settings.General.MoveSelectedLinesStepMs;
    private readonly bool _locked = Se.Settings.General.LockTimeCodes;

    public void Dispose()
    {
        Se.Settings.General.MoveSelectedLinesStepMs = _stepMs;
        Se.Settings.General.LockTimeCodes = _locked;
        foreach (var w in _windows)
        {
            w.Close();
        }
    }

    private (Window Window, MainViewModel Vm) ThreeLines()
    {
        Se.Settings.General.MoveSelectedLinesStepMs = 100;
        Se.Settings.General.LockTimeCodes = false;

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

        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("one", 50, 1000), null!) { Number = 1 });
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("two", 2000, 3000), null!) { Number = 2 });
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("three", 4000, 5000), null!) { Number = 3 });
        Dispatcher.UIThread.RunJobs();
        return (window, vm);
    }

    private static void Select(MainViewModel vm, int index)
    {
        vm.SelectedSubtitle = vm.Subtitles[index];
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void Forward_MovesOnlySelectedLine_KeepsDuration()
    {
        var (_, vm) = ThreeLines();
        Select(vm, 1);

        vm.MoveSelectedLinesXMsForwardCommand.Execute(null);

        Assert.Equal(2100, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(3100, vm.Subtitles[1].EndTime.TotalMilliseconds);
        Assert.Equal(50, vm.Subtitles[0].StartTime.TotalMilliseconds);
        Assert.Equal(4000, vm.Subtitles[2].StartTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void AndForward_MovesSelectedAndFollowing_NotEarlier()
    {
        var (_, vm) = ThreeLines();
        Select(vm, 1);

        vm.MoveSelectedLinesAndForwardXMsBackCommand.Execute(null);

        Assert.Equal(50, vm.Subtitles[0].StartTime.TotalMilliseconds);
        Assert.Equal(1900, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(2900, vm.Subtitles[1].EndTime.TotalMilliseconds);
        Assert.Equal(3900, vm.Subtitles[2].StartTime.TotalMilliseconds);
        Assert.Equal(4900, vm.Subtitles[2].EndTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void Back_ClampsAtZero_AndStopsOnceThere()
    {
        var (_, vm) = ThreeLines();
        Select(vm, 0);

        vm.MoveSelectedLinesXMsBackCommand.Execute(null);
        Assert.Equal(0, vm.Subtitles[0].StartTime.TotalMilliseconds);
        Assert.Equal(950, vm.Subtitles[0].EndTime.TotalMilliseconds);

        vm.MoveSelectedLinesXMsBackCommand.Execute(null);
        Assert.Equal(0, vm.Subtitles[0].StartTime.TotalMilliseconds);
        Assert.Equal(950, vm.Subtitles[0].EndTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void UsesConfiguredStep()
    {
        var (_, vm) = ThreeLines();
        Se.Settings.General.MoveSelectedLinesStepMs = 250;
        Select(vm, 2);

        vm.MoveSelectedLinesXMsForwardCommand.Execute(null);

        Assert.Equal(4250, vm.Subtitles[2].StartTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void LockedTimeCodes_DoesNothing()
    {
        var (_, vm) = ThreeLines();
        vm.LockTimeCodes = true;
        Select(vm, 1);

        vm.MoveSelectedLinesXMsForwardCommand.Execute(null);

        Assert.Equal(2000, vm.Subtitles[1].StartTime.TotalMilliseconds);
    }
}
