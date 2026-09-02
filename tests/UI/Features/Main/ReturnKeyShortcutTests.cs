using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// Return in custom shortcuts (#14401). Dispatch already handled a persisted Alt+Return binding,
/// but the "press a key" dialog could never capture one: its focused OK button clicks on any
/// Return chord (Avalonia's Button checks no modifiers) and marks the event handled, so the
/// window-level capture never ran and the dialog closed with nothing assigned. The same applied
/// to Space chords. Capture now runs in the tunnel phase; a bare Return after a capture confirms.
/// </summary>
public class ReturnKeyShortcutTests
{
    [AvaloniaFact]
    public void AltReturn_BoundToSurroundWith_RunsTheShortcutInsteadOfInsertingALineBreak()
    {
        var left = Se.Settings.Surround1Left;
        var right = Se.Settings.Surround1Right;
        Se.Settings.Surround1Left = "<i>";
        Se.Settings.Surround1Right = "</i>";
        try
        {
            WithShortcut(nameof(MainViewModel.SurroundWith1Command), ["Alt", nameof(Key.Return)], () =>
            {
                var (window, vm) = ShowMainWindowWithOneLine("Hello world");
                try
                {
                    vm.EditTextBox.Focus();
                    vm.EditTextBox.CaretIndex = "Hello".Length;
                    Settle(window);

                    window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.Alt);
                    Settle(window);

                    Assert.Equal("<i>Hello world</i>", vm.Subtitles[0].Text);
                    Assert.DoesNotContain('\n', vm.EditTextBox.Text);
                }
                finally
                {
                    window.Close();
                }
            });
        }
        finally
        {
            Se.Settings.Surround1Left = left;
            Se.Settings.Surround1Right = right;
        }
    }

    [AvaloniaFact]
    public void GetKeyWindow_CapturesAltReturn_InsteadOfClickingOk()
    {
        var (window, vm) = ShowGetKeyWindow();
        try
        {
            window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.Alt);
            Settle(window);

            Assert.Equal("Alt+Return", vm.PressedKey);
            Assert.True(vm.IsAltPressed);
            Assert.False(vm.OkPressed);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GetKeyWindow_BareReturn_IsCapturedFirstAndConfirmsSecond()
    {
        var (window, vm) = ShowGetKeyWindow();
        try
        {
            window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
            Settle(window);
            Assert.Equal("Return", vm.PressedKey);
            Assert.False(vm.OkPressed);

            window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
            Settle(window);
            Assert.Equal("Return", vm.PressedKey);
            Assert.True(vm.OkPressed);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GetKeyWindow_BareReturnAfterAnotherKey_ConfirmsThatKey()
    {
        var (window, vm) = ShowGetKeyWindow();
        try
        {
            window.KeyPressQwerty(PhysicalKey.F7, RawInputModifiers.Control);
            Settle(window);
            Assert.Equal("Ctrl+F7", vm.PressedKey);

            window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
            Settle(window);
            Assert.Equal("Ctrl+F7", vm.PressedKey);
            Assert.True(vm.OkPressed);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GetKeyWindow_CapturesCtrlSpace_WithoutClickingOkOnRelease()
    {
        var (window, vm) = ShowGetKeyWindow();
        try
        {
            window.KeyPressQwerty(PhysicalKey.Space, RawInputModifiers.Control);
            window.KeyReleaseQwerty(PhysicalKey.Space, RawInputModifiers.Control);
            Settle(window);

            Assert.Equal("Ctrl+Space", vm.PressedKey);
            Assert.False(vm.OkPressed);
        }
        finally
        {
            window.Close();
        }
    }

    private static (GetKeyWindow Window, GetKeyViewModel Vm) ShowGetKeyWindow()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var vm = new GetKeyViewModel();
        var window = new GetKeyWindow(vm);
        window.Show();
        Settle(window);
        return (window, vm);
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

    private static (Window Window, MainViewModel Vm) ShowMainWindowWithOneLine(string text)
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
        window.SuppressSaveChangesPromptOnClose(vm);
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, 0, 2000), null!) { Number = 1 });
        Settle(window);

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
