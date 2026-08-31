using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// Issue #14313: the Advanced speech-to-text parameters dialog could not be driven from the
/// keyboard. Focus opened in the Parameters box, which accepted Tab as a character, so Tab and
/// Shift+Tab typed a tab into the whisper command line instead of moving on - the only way out
/// was the mouse. PageUp/PageDown did nothing in the help pane, and Alt+Tabbing back into the
/// dialog threw focus back to the Parameters box.
/// </summary>
public class SpeechToTextAdvancedKeyboardTests : IDisposable
{
    // A window left open outlives the test: it keeps the application-wide activation and focused
    // element, so a later test's input is delivered to it instead.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private (SpeechToTextAdvancedViewModel Vm, Window Window) Open()
    {
        var vm = new SpeechToTextAdvancedViewModel();
        var window = new SpeechToTextAdvancedWindow(vm);
        _windows.Add(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return (vm, window);
    }

    // The editable Parameters box: the only TextBox in the dialog that is not read-only.
    private static TextBox ParametersBox(Window window) =>
        window.GetVisualDescendants().OfType<TextBox>().First(t => !t.IsReadOnly);

    private static TextBox HelpBox(Window window) =>
        window.GetVisualDescendants().OfType<TextBox>().First(t => t.IsReadOnly);

    private static ScrollViewer HelpScrollViewer(Window window) =>
        HelpBox(window).GetVisualAncestors().OfType<ScrollViewer>().First();

    // The reported trap: Tab typed a tab character and focus never left the box.
    [AvaloniaFact]
    public void Tab_LeavesTheParametersBox_InsteadOfTypingATab()
    {
        var (vm, window) = Open();
        var parameters = ParametersBox(window);
        parameters.Focus();
        Dispatcher.UIThread.RunJobs();
        Assert.True(parameters.IsFocused, "the test needs focus to start in the Parameters box");

        window.KeyPressQwerty(PhysicalKey.Tab, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.DoesNotContain('\t', vm.Parameters ?? string.Empty);
        Assert.False(parameters.IsFocused, "Tab should move focus out of the Parameters box");
    }

    // Shift+Tab typed a tab too - it behaved exactly like Tab rather than moving backwards.
    [AvaloniaFact]
    public void ShiftTab_LeavesTheParametersBox_InsteadOfTypingATab()
    {
        var (vm, window) = Open();
        var parameters = ParametersBox(window);
        parameters.Focus();
        Dispatcher.UIThread.RunJobs();

        window.KeyPressQwerty(PhysicalKey.Tab, RawInputModifiers.Shift);
        Dispatcher.UIThread.RunJobs();

        Assert.DoesNotContain('\t', vm.Parameters ?? string.Empty);
        Assert.False(parameters.IsFocused, "Shift+Tab should move focus out of the Parameters box");
    }

    [AvaloniaFact]
    public void PageDownAndPageUp_ScrollTheHelpPane()
    {
        var (vm, window) = Open();
        vm.HelpText = string.Join(Environment.NewLine, Enumerable.Range(0, 400).Select(i => $"help line {i}"));
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var scrollViewer = HelpScrollViewer(window);
        Assert.True(scrollViewer.Extent.Height > scrollViewer.Viewport.Height,
            "the test needs help text taller than the pane");

        HelpBox(window).Focus();
        Dispatcher.UIThread.RunJobs();

        window.KeyPressQwerty(PhysicalKey.PageDown, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        var afterPageDown = scrollViewer.Offset.Y;
        Assert.True(afterPageDown > 0, "PageDown should scroll the help pane down");

        window.KeyPressQwerty(PhysicalKey.PageUp, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.True(scrollViewer.Offset.Y < afterPageDown, "PageUp should scroll the help pane back up");
    }

    // Alt+Tabbing away and back re-raises Activated. The initial-focus handler used to run on
    // every activation, so returning to the dialog dropped focus back on the Parameters box.
    // (Verified that Window.Activate() really does re-raise Activated here, so this is a real
    // round trip and not a test that passes because nothing happened.)
    [AvaloniaFact]
    public void Reactivating_DoesNotPullFocusBackToTheParametersBox()
    {
        var (_, window) = Open();
        var parameters = ParametersBox(window);
        Assert.True(parameters.IsFocused, "the first activation should focus the Parameters box");

        var help = HelpBox(window);
        help.Focus();
        Dispatcher.UIThread.RunJobs();
        Assert.True(help.IsFocused);

        window.Activate();
        Dispatcher.UIThread.RunJobs();

        Assert.True(help.IsFocused, "returning to the dialog should leave focus where the user put it");
        Assert.False(parameters.IsFocused);
    }
}
