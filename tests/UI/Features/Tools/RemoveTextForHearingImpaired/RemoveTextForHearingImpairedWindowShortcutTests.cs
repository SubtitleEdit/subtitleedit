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
using Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;

namespace UITests.Features.Tools.RemoveTextForHearingImpaired;

/// <summary>
/// The fixes grid offers tick all / untick all / invert via context menu and gestures (#13496).
/// With the grid focused those gestures have to beat the TableView's own Ctrl+A ("select all
/// rows"), which is why the window takes them in the tunneling phase.
/// </summary>
public class RemoveTextForHearingImpairedWindowShortcutTests
{
    private static RemoveTextForHearingImpairedViewModel Resolve()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        return services.BuildServiceProvider().GetRequiredService<RemoveTextForHearingImpairedViewModel>();
    }

    private static (RemoveTextForHearingImpairedViewModel Vm, Window Window) OpenWithTwoFixes()
    {
        var vm = Resolve();
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("[door slams]", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("(sighs)", 1000, 2000));
        vm.Initialize(subtitle, _ => { });
        vm.Fixes.Add(new RemoveItem(true, 0, "[door slams]", string.Empty, subtitle.Paragraphs[0]));
        vm.Fixes.Add(new RemoveItem(true, 1, "(sighs)", string.Empty, subtitle.Paragraphs[1]));

        var window = new RemoveTextForHearingImpairedWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        return (vm, window);
    }

    private static TableView FixesGrid(Window window)
    {
        return window.GetVisualDescendants().OfType<TableView>().First();
    }

    // Clicking a row is the only way to get real keyboard focus into the grid - TableView.Focus()
    // does not move it (the row containers take focus, not the control).
    private static TableView FocusFixesGrid(Window window, RemoveTextForHearingImpairedViewModel vm)
    {
        var grid = FixesGrid(window);
        var container = (Visual?)grid.ContainerFromItem(vm.Fixes[0]);
        Assert.NotNull(container);

        var bounds = container!.Bounds;
        var point = container.TranslatePoint(new Point(bounds.Width / 2, bounds.Height / 2), window)!.Value;
        window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(grid.IsKeyboardFocusWithin, "the gestures have to work with the grid focused");
        return grid;
    }

    [AvaloniaFact]
    public void GridCtrlD_UntticksEveryFix()
    {
        var (vm, window) = OpenWithTwoFixes();
        FocusFixesGrid(window, vm);

        window.KeyPressQwerty(PhysicalKey.D, RawInputModifiers.Control);
        Dispatcher.UIThread.RunJobs();

        Assert.All(vm.Fixes, f => Assert.False(f.Apply));

        window.Close();
    }

    [AvaloniaFact]
    public void GridCtrlA_TicksEveryFix_InsteadOfSelectingAllRows()
    {
        var (vm, window) = OpenWithTwoFixes();
        foreach (var fix in vm.Fixes)
        {
            fix.Apply = false;
        }

        var grid = FocusFixesGrid(window, vm);

        window.KeyPressQwerty(PhysicalKey.A, RawInputModifiers.Control);
        Dispatcher.UIThread.RunJobs();

        Assert.All(vm.Fixes, f => Assert.True(f.Apply));
        // ...and the TableView did not turn it into "select all rows" on top of that.
        Assert.Equal(1, grid.SelectedItems?.Count ?? 0);

        window.Close();
    }

    [AvaloniaFact]
    public void GridCtrlShiftI_InvertsEveryFix()
    {
        var (vm, window) = OpenWithTwoFixes();
        vm.Fixes[1].Apply = false;

        FocusFixesGrid(window, vm);

        window.KeyPressQwerty(PhysicalKey.I, RawInputModifiers.Control | RawInputModifiers.Shift);
        Dispatcher.UIThread.RunJobs();

        Assert.False(vm.Fixes[0].Apply);
        Assert.True(vm.Fixes[1].Apply);

        window.Close();
    }

    [AvaloniaFact]
    public void FixesGrid_HasSelectAllSelectNoneAndInvertInItsContextMenu()
    {
        var (_, window) = OpenWithTwoFixes();

        var headers = FixesGrid(window).ContextMenu?.Items.OfType<MenuItem>().Select(m => m.Header as string).ToList();

        Assert.NotNull(headers);
        Assert.Equal(3, headers!.Count);
        Assert.Contains(Nikse.SubtitleEdit.Logic.Config.Se.Language.General.SelectAll, headers);
        Assert.Contains(Nikse.SubtitleEdit.Logic.Config.Se.Language.General.SelectNone, headers);
        Assert.Contains(Nikse.SubtitleEdit.Logic.Config.Se.Language.General.InvertSelection, headers);

        window.Close();
    }
}
