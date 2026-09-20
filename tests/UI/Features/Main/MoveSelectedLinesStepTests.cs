using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Main;

/// <summary>
/// "Move selected lines X ms back/forward", the "and all following" and "all lines" variants, and
/// the per-shortcut "custom milliseconds" slots (#14789): durations are kept, only the intended
/// lines move, and a backward move is clamped so no start goes below zero.
/// </summary>
public class MoveSelectedLinesStepTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly int _stepMs = Se.Settings.General.MoveSelectedLinesStepMs;
    private readonly int _allCustom1Ms = Se.Settings.General.MoveAllLinesCustom1Ms;
    private readonly int _selectedCustom2Ms = Se.Settings.General.MoveSelectedLinesCustom2Ms;
    private readonly bool _locked = Se.Settings.General.LockTimeCodes;

    public void Dispose()
    {
        Se.Settings.General.MoveSelectedLinesStepMs = _stepMs;
        Se.Settings.General.MoveAllLinesCustom1Ms = _allCustom1Ms;
        Se.Settings.General.MoveSelectedLinesCustom2Ms = _selectedCustom2Ms;
        Se.Settings.General.LockTimeCodes = _locked;
        ShortcutsMain.ReloadCommandTranslations();
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
    public void AllLines_MovesEveryLine_WithoutSelection()
    {
        var (_, vm) = ThreeLines();
        vm.SelectedSubtitle = null;
        Dispatcher.UIThread.RunJobs();

        vm.MoveAllLinesXMsForwardCommand.Execute(null);

        Assert.Equal(150, vm.Subtitles[0].StartTime.TotalMilliseconds);
        Assert.Equal(1100, vm.Subtitles[0].EndTime.TotalMilliseconds);
        Assert.Equal(2100, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(4100, vm.Subtitles[2].StartTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void AllLines_Back_ClampsToEarliestStart()
    {
        var (_, vm) = ThreeLines();
        Select(vm, 2);

        vm.MoveAllLinesXMsBackCommand.Execute(null);

        // First line started at 50 ms, so every line moves 50 ms rather than the 100 ms step.
        Assert.Equal(0, vm.Subtitles[0].StartTime.TotalMilliseconds);
        Assert.Equal(950, vm.Subtitles[0].EndTime.TotalMilliseconds);
        Assert.Equal(1950, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(3950, vm.Subtitles[2].StartTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void CustomSlot_UsesItsOwnMilliseconds_ForBothDirections()
    {
        var (_, vm) = ThreeLines();
        Se.Settings.General.MoveSelectedLinesCustom2Ms = 375;
        Select(vm, 1);

        vm.MoveSelectedLinesCustom2ForwardCommand.Execute(null);
        Assert.Equal(2375, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(3375, vm.Subtitles[1].EndTime.TotalMilliseconds);

        vm.MoveSelectedLinesCustom2BackCommand.Execute(null);
        Assert.Equal(2000, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(4000, vm.Subtitles[2].StartTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void CustomSlot_AllLines_MovesEveryLine()
    {
        var (_, vm) = ThreeLines();
        Se.Settings.General.MoveAllLinesCustom1Ms = 20;
        Select(vm, 1);

        vm.MoveAllLinesCustom1ForwardCommand.Execute(null);

        Assert.Equal(70, vm.Subtitles[0].StartTime.TotalMilliseconds);
        Assert.Equal(2020, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(4020, vm.Subtitles[2].StartTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void CustomSlot_TitleShowsMillisecondsAndSlotNumber()
    {
        Se.Settings.General.MoveAllLinesCustom1Ms = 1500;

        var title = ShortcutsMain.GetMoveLinesCustomTitle(MoveLinesScope.All, 1, back: true);

        Assert.Contains(1500.ToString("#,###,##0"), title);
        Assert.EndsWith(", 1", title);
        Assert.Equal("MoveAllLinesCustom1BackCommand",
            ShortcutsMain.GetMoveLinesCustomCommandName(MoveLinesScope.All, 1, back: true));
    }

    [AvaloniaFact]
    public void CustomSlot_RowInShortcutsWindow_ShowsGearAndMilliseconds()
    {
        var (_, vm) = ThreeLines();
        Se.Settings.General.MoveAllLinesCustom1Ms = 42;
        // Titles are cached in a lazy static lookup (refreshed on save/language switch); rebuild
        // it so the row reflects the value above rather than whatever an earlier test saw.
        ShortcutsMain.ReloadCommandTranslations();
        var shortcuts = new ShortcutsViewModel(null!, null!);
        shortcuts.LoadShortCuts(vm);

        var node = shortcuts.FlatNodes.Single(n => n.ShortCut?.Action == vm.MoveAllLinesCustom1BackCommand);
        shortcuts.ShortcutsGrid_SelectionChanged(null,
            new SelectionChangedEventArgs(SelectingItemsControl.SelectionChangedEvent, new List<object>(), new List<object> { node }));

        Assert.True(shortcuts.IsConfigureVisible);
        Assert.Equal(ShortcutsMain.GetMoveLinesCustomTitle(MoveLinesScope.All, 1, back: true, 42), node.Title);
        Assert.Contains("42", node.Title);
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
