using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace UITests.Features.Files.Compare;

/// <summary>
/// A right-click on the gutter arrow (Ctrl+Click on macOS) offers taking only the text or only
/// the timing, instead of opening the inline editor for it (#15621).
/// </summary>
public class CompareGutterMenuTests : IDisposable
{
    private readonly SeCompare _savedSettings = Se.Settings.File.Compare;

    // A closed CompareWindow saves its size for the next one - see CompareEditTests.
    private readonly SettingsScope _windowPositions = new("General.WindowPositions");
    private Window? _window;

    public CompareGutterMenuTests()
    {
        Se.Settings.File.Compare = new SeCompare();
        Se.Settings.General.WindowPositions = new List<SeWindowPosition>();
    }

    public void Dispose()
    {
        _window?.Close();
        Se.Settings.File.Compare = _savedSettings;
        _windowPositions.Dispose();
    }

    [AvaloniaFact]
    public void RightClickOnTheArrow_OffersTakeTextAndTakeTiming_AndTakeTextKeepsTheTiming()
    {
        var (vm, window) = Show(MakeLines(("One", 0), ("Too", 2000)), MakeLines(("One", 0), ("Two", 2500)));
        var arrow = FindArrow(vm, row: 1);
        var point = arrow.TranslatePoint(new Point(arrow.Bounds.Width / 2, arrow.Bounds.Height / 2), window)!.Value;

        window.MouseDown(point, MouseButton.Right);
        window.MouseUp(point, MouseButton.Right);
        Settle(window);

        var flyout = Assert.IsType<MenuFlyout>(vm.RowsView!.ContextFlyout);
        Assert.True(flyout.IsOpen);
        Assert.Same(vm.Rows[1], vm.SelectedRow);
        Assert.Equal(0, vm.PendingChangeCount); // the right-click itself took nothing
        var visible = flyout.Items.OfType<MenuItem>().Where(p => p.IsVisible).Select(p => p.Header).Take(3).ToList();
        Assert.Equal(new object[] { Se.Language.File.CompareTakeFromReference, Se.Language.File.CompareTakeText, Se.Language.File.CompareTakeTiming }, visible);

        var takeText = flyout.Items.OfType<MenuItem>().Single(p => Equals(p.Header, Se.Language.File.CompareTakeText));
        takeText.Command!.Execute(takeText.CommandParameter);
        flyout.Hide();
        Settle(window);

        var edited = vm.GetEditedLines();
        Assert.Equal("Two", edited[1].Text);
        Assert.Equal(2000, edited[1].StartTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void Menu_OnlyOffersTheTakesThatChangeSomething()
    {
        var (vm, _) = Show(MakeLines(("One", 0), ("Same", 2000)), MakeLines(("Uno", 0), ("Same", 2500)));

        Assert.True(vm.Rows[0].CanTakeText);
        Assert.False(vm.Rows[0].CanTakeTiming);
        Assert.False(vm.Rows[1].CanTakeText);
        Assert.True(vm.Rows[1].CanTakeTiming);
    }

    [AvaloniaFact]
    public void MacCtrlClickOnTheArrow_OpensTheMenu_InsteadOfTakingTheLine()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return; // Ctrl+Click is only the secondary click on macOS
        }

        var (vm, window) = Show(MakeLines(("One", 0), ("Too", 2000)), MakeLines(("One", 0), ("Two", 2500)));
        var arrow = FindArrow(vm, row: 1);
        var point = arrow.TranslatePoint(new Point(arrow.Bounds.Width / 2, arrow.Bounds.Height / 2), window)!.Value;

        window.MouseDown(point, MouseButton.Left, RawInputModifiers.Control);
        window.MouseUp(point, MouseButton.Left, RawInputModifiers.Control);
        Settle(window);

        var flyout = Assert.IsType<MenuFlyout>(vm.RowsView!.ContextFlyout);
        Assert.True(flyout.IsOpen);
        Assert.Same(vm.Rows[1], vm.SelectedRow);
        Assert.Equal(0, vm.PendingChangeCount);
        flyout.Hide();
    }

    private static Button FindArrow(CompareViewModel vm, int row)
    {
        var container = vm.RowsView!.GetRealizedContainers().ElementAt(row);
        return container.GetVisualDescendants().OfType<Button>().Single(p => p.Command == vm.TakeReferenceCommand && p.IsEffectivelyVisible);
    }

    private (CompareViewModel vm, Window window) Show(ObservableCollection<SubtitleLineViewModel> left, ObservableCollection<SubtitleLineViewModel> right)
    {
        var vm = new CompareViewModel(new FileHelper(), new FolderHelper());
        vm.Initialize(left, "left.srt", right, "right.srt", false);
        var window = new CompareWindow(vm) { Width = 1300, Height = 800 };
        _window = window;
        window.Show();
        Settle(window);
        vm.SelectRow(0);
        Settle(window);
        return (vm, window);
    }

    private static ObservableCollection<SubtitleLineViewModel> MakeLines(params (string Text, int StartMs)[] lines)
    {
        var result = new ObservableCollection<SubtitleLineViewModel>();
        for (var i = 0; i < lines.Length; i++)
        {
            result.Add(new SubtitleLineViewModel(new Paragraph(lines[i].Text, lines[i].StartMs, lines[i].StartMs + 1500), null!) { Number = i + 1 });
        }

        return result;
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 12; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }
}
