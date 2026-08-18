using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// Shift+Delete must cut in EVERY text box, not only the two main-window edit boxes (#13711).
/// The main-window boxes are covered by the shortcut manager, but Avalonia's native keymap ships
/// Ctrl+Insert copy and Shift+Insert paste without the matching Shift+Delete cut, so any other
/// text box (Find/Replace, dialogs, batch convert) silently deleted the selection instead of
/// cutting it. <see cref="NativeKeymap.AddShiftDeleteCut"/> closes that gap app-wide, and the
/// source editor - which does its own key handling - mirrors the gesture itself.
/// </summary>
public class NativeKeymapShiftDeleteCutTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose, so a failing test cannot leak a
    // window into the shared per-assembly headless session.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private Window Show(Control content)
    {
        var window = new Window { Content = content, Width = 600, Height = 400 };
        _windows.Add(window);
        window.Show();
        Settle(window);
        return window;
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
    /// The cut writes the clipboard from an async continuation, so pump the dispatcher until the
    /// expected state appears instead of asserting instantly (same pattern as the other headless
    /// suites - an instant assert is a CI flake under load).
    /// </summary>
    private static async Task WaitUntil(Func<bool> condition, string failureMessage, int timeoutMs = 1000)
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
    public async Task ShiftDelete_Cuts_InAPlainWindowTextBox()
    {
        var textBox = new TextBox { Text = "Hello world" };
        var window = Show(textBox);
        NativeKeymap.AddShiftDeleteCut();

        await ClipboardHelper.SetTextAsync(window, "clipboard before the cut");
        textBox.Focus();
        textBox.SelectionStart = 0;
        textBox.SelectionEnd = "Hello ".Length;
        Settle(window);

        window.KeyPressQwerty(PhysicalKey.Delete, RawInputModifiers.Shift);

        await WaitUntil(() => textBox.Text == "world",
            $"expected the selection to be cut away, text is '{textBox.Text}'");
        await WaitUntil(() => ClipboardHelper.GetTextAsync(window).GetAwaiter().GetResult() == "Hello ",
            "the cut selection never reached the clipboard - Shift+Delete only deleted");
    }

    [AvaloniaFact]
    public async Task Delete_WithoutShift_StillJustDeletes_InAPlainWindowTextBox()
    {
        var textBox = new TextBox { Text = "Hello world" };
        var window = Show(textBox);
        NativeKeymap.AddShiftDeleteCut();

        await ClipboardHelper.SetTextAsync(window, "clipboard before the delete");
        textBox.Focus();
        textBox.SelectionStart = 0;
        textBox.SelectionEnd = "Hello ".Length;
        Settle(window);

        window.KeyPressQwerty(PhysicalKey.Delete, RawInputModifiers.None);

        await WaitUntil(() => textBox.Text == "world",
            $"expected a plain delete of the selection, text is '{textBox.Text}'");
        Assert.Equal("clipboard before the delete", await ClipboardHelper.GetTextAsync(window));
    }

    /// <summary>
    /// The native keymap cut is gated on CanCut, so a read-only box must keep both its text and
    /// the user's clipboard (the read-only original-subtitle box regression from #13711).
    /// </summary>
    [AvaloniaFact]
    public async Task ShiftDelete_InAReadOnlyTextBox_TouchesNeitherTextNorClipboard()
    {
        var textBox = new TextBox { Text = "Hello world", IsReadOnly = true };
        var window = Show(textBox);
        NativeKeymap.AddShiftDeleteCut();

        await ClipboardHelper.SetTextAsync(window, "precious clipboard");
        textBox.Focus();
        textBox.SelectionStart = 0;
        textBox.SelectionEnd = "Hello ".Length;
        Settle(window);

        window.KeyPressQwerty(PhysicalKey.Delete, RawInputModifiers.Shift);
        Settle(window);
        await Task.Delay(50);
        Settle(window);

        Assert.Equal("Hello world", textBox.Text);
        Assert.Equal("precious clipboard", await ClipboardHelper.GetTextAsync(window));
    }

    [AvaloniaFact]
    public async Task ShiftDelete_Cuts_InTheSyntaxTextEditor()
    {
        var editor = new SyntaxTextEditor
        {
            Text = "Hello world",
            FontFamily = new FontFamily("Courier New"),
            FontSize = 12,
        };
        var window = Show(editor);

        await ClipboardHelper.SetTextAsync(window, "clipboard before the cut");
        editor.View.Focus();
        editor.Select(0, "Hello ".Length);
        Settle(window);

        window.KeyPress(Key.Delete, RawInputModifiers.Shift, PhysicalKey.Delete, string.Empty);

        await WaitUntil(() => editor.Text == "world",
            $"expected the selection to be cut away, text is '{editor.Text}'");
        await WaitUntil(() => ClipboardHelper.GetTextAsync(window).GetAwaiter().GetResult() == "Hello ",
            "the cut selection never reached the clipboard - Shift+Delete only deleted");
    }

    [AvaloniaFact]
    public async Task Delete_WithoutShift_StillJustDeletes_InTheSyntaxTextEditor()
    {
        var editor = new SyntaxTextEditor
        {
            Text = "Hello world",
            FontFamily = new FontFamily("Courier New"),
            FontSize = 12,
        };
        var window = Show(editor);

        await ClipboardHelper.SetTextAsync(window, "clipboard before the delete");
        editor.View.Focus();
        editor.Select(0, "Hello ".Length);
        Settle(window);

        window.KeyPress(Key.Delete, RawInputModifiers.None, PhysicalKey.Delete, string.Empty);

        await WaitUntil(() => editor.Text == "world",
            $"expected a plain delete of the selection, text is '{editor.Text}'");
        Assert.Equal("clipboard before the delete", await ClipboardHelper.GetTextAsync(window));
    }
}
