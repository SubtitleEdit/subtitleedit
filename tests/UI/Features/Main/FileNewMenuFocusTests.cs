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
using System.Reflection;

namespace UITests.Features.Main;

/// <summary>
/// File > New (Ctrl+N) clears the subtitle grid, and clearing the rows removes the focused row
/// container with them - keyboard focus dropped to nothing at all. The next arrow key then walked
/// focus from the window root into the first focusable control, the menu bar, which read as "the
/// menu bar activated itself after Ctrl+N" and took two Alt presses to leave (#13111 beta-4/5
/// feedback). ResetSubtitle now puts focus back on the (empty-but-focusable) grid.
/// </summary>
public class FileNewMenuFocusTests : IDisposable
{
    // Close every window in Dispose so a failed test cannot leave one racing the headless
    // session teardown (same pattern as MainMenuKeyboardActivationTests).
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private (Window Window, MainViewModel Vm) ShowMainWindowWithLines(int lineCount = 3)
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
        vm.Menu.IsVisible = true;

        for (var i = 0; i < lineCount; i++)
        {
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph($"Line {i + 1}", i * 2000, i * 2000 + 1500), null!)
            {
                Number = i + 1,
            });
        }

        Settle(window);
        return (window, vm);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    /// <summary>
    /// Marks the current content as saved so File > New skips the "save changes?" prompt -
    /// a modal would block the headless run.
    /// </summary>
    private static void MarkUnchanged(MainViewModel vm)
    {
        typeof(MainViewModel).GetField("_changeSubtitleHash", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(vm, vm.GetFastHash());
    }

    /// <summary>
    /// Polls for a state the focus machinery is expected to reach: even with Settle, focus
    /// changes can be a few dispatcher frames behind on a loaded runner (same pattern as
    /// MainMenuKeyboardActivationTests.WaitUntil).
    /// </summary>
    private static async Task WaitUntil(Func<bool> condition, string failureMessage, int timeoutMs = 500)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (!condition())
        {
            if (stopwatch.ElapsedMilliseconds > timeoutMs)
            {
                Assert.Fail(failureMessage);
            }

            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }
    }

    [AvaloniaFact]
    public async Task FileNew_KeepsFocusOutOfTheMenuBar()
    {
        var (window, vm) = ShowMainWindowWithLines();

        TableViewExtras.FocusRow(vm.SubtitleGrid);
        Settle(window);
        await WaitUntil(() => vm.SubtitleGrid.IsKeyboardFocusWithin,
            "the grid should hold keyboard focus before File > New");

        MarkUnchanged(vm);
        vm.CommandFileNewCommand.Execute(null);
        Settle(window);

        // Focus must not be dropped: with nothing focused, the next arrow key walks focus
        // into the menu bar. (Generous timeout: the restore runs in a Background-priority
        // dispatcher pass after layout, which can lag on a loaded runner.)
        await WaitUntil(() => ReferenceEquals(vm.SubtitleGrid, window.FocusManager?.GetFocusedElement()),
            "File > New should leave keyboard focus on the (now empty) grid", timeoutMs: 2000);

        window.KeyPressQwerty(PhysicalKey.ArrowDown, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        window.KeyReleaseQwerty(PhysicalKey.ArrowDown, RawInputModifiers.None);
        Settle(window);

        Assert.False(window.FocusManager?.GetFocusedElement() is MenuItem);
        Assert.False(vm.Menu.IsOpen);

        window.Close();
    }
}
