using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// Avalonia's AccessKeyHandler owns bare-Alt menu-bar activation and only settles its private
/// state on the Alt *release*. When a modal window steals focus mid-gesture (e.g. Alt, O, ...
/// opening the Shortcuts window), the release never reaches the window and the handler is left
/// with "ignore the next Alt up" armed - so the next bare Alt press fails to open the menu
/// (#13083). OnWindowDeactivated raises a synthetic Alt KeyUp via
/// <see cref="UiUtil.RaiseSyntheticAltKeyUp"/> to complete the cycle; these tests pin both the
/// broken Avalonia behavior (a canary for upstream fixes) and the recovery.
/// </summary>
public class AltMenuAccessKeyResetTests : IDisposable
{
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

    private Window MakeWindowWithMenu(out Menu menu)
    {
        menu = new Menu { Items = { new MenuItem { Header = "_File" } } };
        var editor = new TextBox();
        var window = new Window
        {
            Content = new StackPanel { Children = { menu, editor } },
        };
        _windows.Add(window);

        window.Show();
        Dispatcher.UIThread.RunJobs();
        editor.Focus();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    /// <summary>Alt goes down, another key is pressed (menu navigation / accelerator), and the
    /// Alt release is lost to a modal window stealing focus.</summary>
    private static void SimulateMissedAltRelease(Window window)
    {
        window.KeyPress(Key.LeftAlt, RawInputModifiers.Alt, PhysicalKey.AltLeft, null);
        window.KeyPress(Key.Q, RawInputModifiers.Alt, PhysicalKey.Q, "q");
        window.KeyRelease(Key.Q, RawInputModifiers.Alt, PhysicalKey.Q, "q");
    }

    private static void PressAndReleaseBareAlt(Window window)
    {
        window.KeyPress(Key.LeftAlt, RawInputModifiers.Alt, PhysicalKey.AltLeft, null);
        window.KeyRelease(Key.LeftAlt, RawInputModifiers.None, PhysicalKey.AltLeft, null);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void BareAltOpensMenu()
    {
        var window = MakeWindowWithMenu(out var menu);

        PressAndReleaseBareAlt(window);

        Assert.True(menu.IsOpen);
        window.Close();
    }

    // Canary for the Avalonia behavior the synthetic release works around: if this starts
    // failing (menu opens), Avalonia recovers from a missed Alt KeyUp on its own and
    // UiUtil.RaiseSyntheticAltKeyUp can likely be removed.
    [AvaloniaFact]
    public void MissedAltReleaseLeavesNextBareAltDeadWithoutReset()
    {
        var window = MakeWindowWithMenu(out var menu);

        SimulateMissedAltRelease(window);
        PressAndReleaseBareAlt(window);

        Assert.False(menu.IsOpen);
        window.Close();
    }

    [AvaloniaFact]
    public void SyntheticAltKeyUpRestoresBareAltAfterMissedRelease()
    {
        var window = MakeWindowWithMenu(out var menu);

        SimulateMissedAltRelease(window);

        // What OnWindowDeactivated does when the modal steals focus: complete the Alt cycle,
        // then close the menu in case the synthetic release opened it.
        UiUtil.RaiseSyntheticAltKeyUp(window);
        menu.Close();
        Dispatcher.UIThread.RunJobs();

        PressAndReleaseBareAlt(window);

        Assert.True(menu.IsOpen);
        window.Close();
    }
}
