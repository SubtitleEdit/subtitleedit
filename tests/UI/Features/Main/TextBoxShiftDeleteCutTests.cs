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
/// Shift+Delete in a text box must cut, not just delete (#13711). The shipped "Text box: Cut
/// (alternative)" binding never fired: the single-key guard in OnKeyDownHandler treats Shift as
/// "no modifier" - true for Shift+&lt;letter&gt;, which is just an uppercase letter, but wrong for
/// editing keys - so Shift+Delete looked like a bare Delete and returned before the TextBox
/// category was ever dispatched, leaving Avalonia's plain delete to run.
/// </summary>
public class TextBoxShiftDeleteCutTests
{
    [AvaloniaFact]
    public void ShiftDelete_CutsTheSelection_InsteadOfOnlyDeletingIt()
    {
        WithShortcut(nameof(MainViewModel.TextBoxCut2Command), ["Shift", nameof(Key.Delete)], () =>
        {
            var (window, vm) = ShowMainWindowWithOneLine("Hello world");
            try
            {
                ClipboardHelper.SetTextAsync(window, "clipboard before the cut").GetAwaiter().GetResult();
                Settle(window);

                vm.EditTextBox.Focus();
                vm.EditTextBox.Select(0, "Hello ".Length);
                Settle(window);

                window.KeyPressQwerty(PhysicalKey.Delete, RawInputModifiers.Shift);
                Settle(window);

                Assert.Equal("world", vm.EditTextBox.Text);
                Assert.Equal("Hello ", ClipboardHelper.GetTextAsync(window).GetAwaiter().GetResult());
            }
            finally
            {
                window.Close();
            }
        });
    }

    /// <summary>
    /// Bare Delete stays plain text editing - the guard only stops treating Shift+Delete as if it
    /// were the same key, so an unmodified Delete must still delete forward in the text box.
    /// </summary>
    [AvaloniaFact]
    public void Delete_WithoutShift_StillDeletesForwardInTheTextBox()
    {
        WithShortcut(nameof(MainViewModel.TextBoxCut2Command), ["Shift", nameof(Key.Delete)], () =>
        {
            var (window, vm) = ShowMainWindowWithOneLine("Hello world");
            try
            {
                ClipboardHelper.SetTextAsync(window, "clipboard before the delete").GetAwaiter().GetResult();
                Settle(window);

                vm.EditTextBox.Focus();
                vm.EditTextBox.Select(0, "Hello ".Length);
                Settle(window);

                window.KeyPressQwerty(PhysicalKey.Delete, RawInputModifiers.None);
                Settle(window);

                Assert.Equal("world", vm.EditTextBox.Text);
                Assert.Equal("clipboard before the delete", ClipboardHelper.GetTextAsync(window).GetAwaiter().GetResult());
            }
            finally
            {
                window.Close();
            }
        });
    }

    /// <summary>
    /// The same guard also swallowed the shipped Shift+Back forward delete (added for Apple
    /// keyboards, whose Delete key is only a backspace), so that binding never worked either.
    /// </summary>
    [AvaloniaFact]
    public void ShiftBack_DeletesForward_InsteadOfBackspacing()
    {
        WithShortcut(nameof(MainViewModel.TextBoxDeleteForwardCommand), ["Shift", nameof(Key.Back)], () =>
        {
            var (window, vm) = ShowMainWindowWithOneLine("Hello world");
            try
            {
                vm.EditTextBox.Focus();
                vm.EditTextBox.CaretIndex = "Hello".Length;
                Settle(window);

                window.KeyPressQwerty(PhysicalKey.Backspace, RawInputModifiers.Shift);
                Settle(window);

                // Forward delete removed the space after the caret; a backspace would have
                // removed the "o" before it.
                Assert.Equal("Helloworld", vm.EditTextBox.Text);
            }
            finally
            {
                window.Close();
            }
        });
    }

    /// <summary>
    /// The settings singleton is shared across the whole test run, so give the action exactly one
    /// binding - the one under test - and put the original bindings back afterwards.
    /// </summary>
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
