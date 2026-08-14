using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features;

/// <summary>
/// Avalonia's text box pastes the clipboard string verbatim, so text copied from a LF file would
/// leave a paragraph with a line break nothing else in SE produces - and tools that rebuild the
/// text would then report a change that renders identically (#13591).
/// </summary>
public class TextBoxPasteNormalizerTests
{
    [AvaloniaTheory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\r")]
    public void Paste_InsertsTextWithSeLineBreak(string lineBreak)
    {
        var textBox = new TextBox { AcceptsReturn = true, Text = string.Empty };

        TextBoxPasteNormalizer.InsertNormalized(textBox, "first" + lineBreak + "second");

        Assert.Equal("first" + Environment.NewLine + "second", textBox.Text);
    }

    [AvaloniaFact]
    public void Paste_ReplacesTheSelection()
    {
        var textBox = new TextBox { AcceptsReturn = true, Text = "keep drop", SelectionStart = 5, SelectionEnd = 9 };

        TextBoxPasteNormalizer.InsertNormalized(textBox, "one\ntwo");

        Assert.Equal("keep one" + Environment.NewLine + "two", textBox.Text);
    }

    [AvaloniaFact]
    public void Paste_LeavesAReadOnlyBoxAlone()
    {
        var textBox = new TextBox { AcceptsReturn = true, Text = "untouched", IsReadOnly = true };

        TextBoxPasteNormalizer.InsertNormalized(textBox, "pasted\ntext");

        Assert.Equal("untouched", textBox.Text);
    }

    // The wiring: without the handler on the edit text box the whole fix is dead, and nothing else
    // in the app would notice.
    [AvaloniaFact]
    public void MainEditTextBox_TakesOverPaste()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var vm = (MainViewModel)view.DataContext!;
        try
        {
            var e = new RoutedEventArgs(TextBox.PastingFromClipboardEvent);
            vm.EditTextBox.TextControl.RaiseEvent(e);

            Assert.True(e.Handled);
        }
        finally
        {
            window.Closing -= vm.OnClosing;
            window.Close();
        }
    }
}
