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
/// An Alt+&lt;key&gt; shortcut must not leave the menu bar activated (#14743, discussion #11744).
///
/// Avalonia's AccessKeyHandler notes "Alt was part of a chord" in its own tunnelling key-down
/// handler, registered in the TopLevel constructor with handledEventsToo:false. Tunnel handlers on
/// one element run in reverse registration order, so the window key handler added by MainView runs
/// first: as soon as it marks an Alt shortcut handled - Alt+Down is the shipped "go to next line"
/// binding - the built-in handler never sees the chord key and the Alt release runs MainMenu.Open().
/// The bar then owns the keyboard and OnKeyDownHandler bails on IsMainMenuFocused, so the next
/// Ctrl+Left/Right walked the menu items instead of moving the start time.
///
/// The boundary case - Alt+&lt;access key&gt;, which opens the bar on the key *down* and must keep
/// working - is covered by MainMenuKeyboardActivationTests.AltLetter_OpensTheMenuBarItself.
/// </summary>
public class AltChordMenuActivationTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    [AvaloniaFact]
    public void AltShortcutChord_DoesNotActivateTheMenuBar()
    {
        var (window, vm) = ShowMainWindowWithLines();
        vm.EditTextBox.Focus();
        Settle(window);
        Assert.True(vm.EditTextBox.IsFocused, "the text box should hold focus before the Alt chord");

        PressAltChord(window, PhysicalKey.ArrowDown);

        Assert.False(vm.Menu.IsOpen, "an Alt shortcut must not open the menu bar on the Alt release");
        Assert.False(window.FocusManager?.GetFocusedElement() is MenuItem, "focus must not land on a menu item");
        Assert.True(vm.EditTextBox.IsFocused, "the text box should keep focus across the Alt shortcut");
    }

    [AvaloniaFact]
    public void AltShortcutChord_LeavesTheFollowingShortcutWorking()
    {
        var allowTextNavigation = Se.Settings.Tools.AllowTextNavigationShortcutsInTextbox;
        Se.Settings.Tools.AllowTextNavigationShortcutsInTextbox = true;
        try
        {
            // The reported cycle: Alt+Down to step to the next line, then Ctrl+Left to nudge its
            // start time - both from the text box, without touching the grid or the waveform.
            WithShortcut(nameof(MainViewModel.MoveStartOneFrameBackCommand), ["Control", nameof(Key.Left)], () =>
            {
                var (window, vm) = ShowMainWindowWithLines();
                vm.EditTextBox.Focus();
                Settle(window);

                PressAltChord(window, PhysicalKey.ArrowDown);
                Assert.Equal(1, vm.SelectedSubtitleIndex);

                var startBefore = vm.Subtitles[1].StartTime;
                window.KeyPressQwerty(PhysicalKey.ArrowLeft, RawInputModifiers.Control);
                Settle(window);
                window.KeyReleaseQwerty(PhysicalKey.ArrowLeft, RawInputModifiers.Control);
                Settle(window);

                Assert.True(vm.Subtitles[1].StartTime < startBefore,
                    "Ctrl+Left should move the start time back a frame instead of walking the menu bar");
            });
        }
        finally
        {
            Se.Settings.Tools.AllowTextNavigationShortcutsInTextbox = allowTextNavigation;
        }
    }

    [AvaloniaFact]
    public void AltPointerGesture_DoesNotLeaveTheMenuBarActivated()
    {
        // The same reverse-order trap disabled the Alt+drag guard (#11744): its key-up handler
        // consumed the armed state in the tunnel pass, where Avalonia has not opened the bar yet.
        var (window, vm) = ShowMainWindowWithLines();
        vm.EditTextBox.Focus();
        Settle(window);

        window.KeyPressQwerty(PhysicalKey.AltLeft, RawInputModifiers.Alt);
        Settle(window);
        window.MouseDown(new Avalonia.Point(400, 500), MouseButton.Left, RawInputModifiers.Alt);
        Settle(window);
        window.MouseUp(new Avalonia.Point(400, 500), MouseButton.Left, RawInputModifiers.Alt);
        Settle(window);
        window.KeyReleaseQwerty(PhysicalKey.AltLeft, RawInputModifiers.None);
        Settle(window);

        Assert.False(vm.Menu.IsOpen, "an Alt+click gesture must not leave the menu bar open");
    }

    /// <summary>
    /// Alt held down, a second key pressed and released, then Alt released - the order in which
    /// Avalonia's AccessKeyHandler arms and settles its menu-bar activation.
    /// </summary>
    private static void PressAltChord(Window window, PhysicalKey key)
    {
        window.KeyPressQwerty(PhysicalKey.AltLeft, RawInputModifiers.Alt);
        Settle(window);
        window.KeyPressQwerty(key, RawInputModifiers.Alt);
        Settle(window);
        window.KeyReleaseQwerty(key, RawInputModifiers.Alt);
        Settle(window);
        window.KeyReleaseQwerty(PhysicalKey.AltLeft, RawInputModifiers.None);
        Settle(window);
    }

    private static void WithShortcut(string actionName, string[] keys, Action test)
    {
        var original = Se.Settings.Shortcuts.Where(s => s.ActionName == actionName).ToList();
        Se.Settings.Shortcuts.RemoveAll(s => s.ActionName == actionName);
        Se.Settings.Shortcuts.Add(new SeShortCut(actionName, [.. keys]));
        try
        {
            test();
        }
        finally
        {
            Se.Settings.Shortcuts.RemoveAll(s => s.ActionName == actionName);
            Se.Settings.Shortcuts.AddRange(original);
        }
    }

    private (Window Window, MainViewModel Vm) ShowMainWindowWithLines()
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
        for (var i = 0; i < 5; i++)
        {
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("Line " + i, i * 3000, i * 3000 + 2000), null!) { Number = i + 1 });
        }

        Settle(window);

        // On macOS the in-window menu is hidden in favor of the native menu bar; these tests
        // exercise the in-window menu (the Windows/Linux path), so show it regardless of host.
        vm.Menu.IsVisible = true;
        vm.SelectedSubtitleIndex = 0;
        vm.SubtitleGrid.SelectedItem = vm.Subtitles[0];
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
}
