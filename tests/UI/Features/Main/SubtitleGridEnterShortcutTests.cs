using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// A user-assigned bare Return shortcut must fire while the subtitle grid is focused (#12734).
/// The built-in "go to subtitle and set video position" Enter handling used to swallow the key
/// unconditionally, so shortcut dispatch (e.g. "Play next (and stop)") was never reached for
/// bare Return; Shift+Return already worked because it never matched that branch.
/// </summary>
public class SubtitleGridEnterShortcutTests
{
    private static (Window Window, MainViewModel Vm, TableView Grid) ShowMainWindowWithLines(int lineCount = 3)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var vm = (MainViewModel)view.DataContext!;
        for (var i = 0; i < lineCount; i++)
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

    private static void ClickRow(Window window, TableView grid, int index)
    {
        var container = (Visual?)grid.ContainerFromItem(((IList<SubtitleLineViewModel>)grid.ItemsSource!)[index]);
        Assert.NotNull(container);
        var bounds = container!.Bounds;
        var point = container.TranslatePoint(new Point(bounds.Width / 2, bounds.Height / 2), window)!.Value;

        window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Settle(window);
    }

    [AvaloniaFact]
    public void Enter_RunsUserAssignedReturnShortcut_WhenGridIsFocused()
    {
        // The user assigned bare Return to "go to next line" (any grid/general action works;
        // the real-world report used "Play next (and stop)", which needs a video player and is
        // not observable headless - this proxy action is).
        // The settings singleton is shared across tests in the run, so make sure the action has
        // exactly ONE binding - the user's - or GetUsedShortcuts' GroupBy(First()) would prefer
        // the pre-existing default binding (Alt+Down) over it.
        var originalGoToNextLine = Se.Settings.Shortcuts.Where(s => s.ActionName == nameof(MainViewModel.GoToNextLineCommand)).ToList();
        Se.Settings.Shortcuts.RemoveAll(s => s.ActionName == nameof(MainViewModel.GoToNextLineCommand));
        Se.Settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.GoToNextLineCommand), ["Return"]));
        try
        {
            var (window, vm, grid) = ShowMainWindowWithLines();

            ClickRow(window, grid, 0);
            Assert.Equal(0, vm.SelectedSubtitleIndex);

            window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Settle(window);

            // The user-assigned bare Return shortcut ran "go to next line" instead of being
            // swallowed by the built-in Enter handling.
            Assert.Equal(1, vm.SelectedSubtitleIndex);

            window.Close();
        }
        finally
        {
            Se.Settings.Shortcuts.RemoveAll(s => s.ActionName == nameof(MainViewModel.GoToNextLineCommand));
            Se.Settings.Shortcuts.AddRange(originalGoToNextLine);
        }
    }
}
