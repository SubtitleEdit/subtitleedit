using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// The grid's own SelectedItems follows the order rows were picked, not the order they appear in:
/// shift-clicking upwards leaves the row that was already selected in front, and ctrl-clicking
/// around scrambles the list entirely. Everything downstream (copy, copy text only, merge, export
/// selected lines, ...) reads <see cref="MainViewModel.SubtitleGridSelectedItems"/>, so that is
/// where subtitle order gets restored - see issue #13173, where "Copy (text only)" put the lines
/// on the clipboard out of order after a reverse selection.
/// </summary>
public class SubtitleGridSelectionOrderTests : IDisposable
{
    private const int LineCount = 20;

    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private (Window Window, MainViewModel Vm, TableView Grid) ShowMainWindowWithLines()
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
        for (var i = 0; i < LineCount; i++)
        {
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph($"Line {i + 1}", i * 2000, i * 2000 + 1500), null!)
            {
                Number = i + 1,
            });
        }

        Settle(window);
        return (window, vm, vm.SubtitleGrid);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private static void Click(Window window, TableView grid, int index, RawInputModifiers modifiers)
    {
        var container = (Visual?)grid.ContainerFromItem(((IList<SubtitleLineViewModel>)grid.ItemsSource!)[index]);
        Assert.NotNull(container);
        var bounds = container!.Bounds;
        var point = container.TranslatePoint(new Point(bounds.Width / 2, bounds.Height / 2), window)!.Value;

        window.MouseDown(point, MouseButton.Left, modifiers);
        window.MouseUp(point, MouseButton.Left, modifiers);
        Settle(window);
    }

    private static int[] SelectedNumbers(MainViewModel vm) =>
        vm.SubtitleGridSelectedItems.Select(p => p.Number).ToArray();

    [AvaloniaTheory]
    [InlineData(2, 6)] // downwards
    [InlineData(6, 2)] // upwards - the reverse selection from issue #13173
    public void ShiftClick_SelectsInSubtitleOrder_InBothDirections(int first, int second)
    {
        var (window, vm, grid) = ShowMainWindowWithLines();

        Click(window, grid, first, RawInputModifiers.None);
        Click(window, grid, second, RawInputModifiers.Shift);

        var lo = Math.Min(first, second);
        var hi = Math.Max(first, second);
        Assert.Equal(Enumerable.Range(lo + 1, hi - lo + 1).ToArray(), SelectedNumbers(vm));

        window.Close();
    }

    /// <summary>
    /// Adding a row above the current one puts it last in the control's own SelectedItems; the
    /// view model still hands out the pair top row first. (Only one added row: in the headless
    /// harness the multi-select modifier is Ctrl, and the grid's context flyout - which Ctrl+click
    /// opens on macOS - then swallows every ctrl-click after the first.)
    /// </summary>
    [AvaloniaFact]
    public void CtrlClick_OnARowAbove_StillSelectsInSubtitleOrder()
    {
        var (window, vm, grid) = ShowMainWindowWithLines();

        Click(window, grid, 7, RawInputModifiers.None);
        Click(window, grid, 1, RawInputModifiers.Control);

        Assert.Equal(new[] { 2, 8 }, SelectedNumbers(vm));

        window.Close();
    }

    /// <summary>
    /// The anchor row - the one the edit box and waveform follow - is still the row that was
    /// clicked last, even though it is no longer first in the selection.
    /// </summary>
    [AvaloniaFact]
    public void ReverseShiftClick_KeepsTheClickedRowAsTheCurrentRow()
    {
        var (window, vm, grid) = ShowMainWindowWithLines();

        Click(window, grid, 6, RawInputModifiers.None);
        Click(window, grid, 2, RawInputModifiers.Shift);

        Assert.Same(vm.Subtitles[2], grid.SelectedItem);
        Assert.Equal(new[] { 3, 4, 5, 6, 7 }, SelectedNumbers(vm));

        window.Close();
    }
}
