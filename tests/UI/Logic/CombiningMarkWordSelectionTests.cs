using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// Combining marks (Arabic tanween, Hebrew niqqud, decomposed accents) belong to the word for
/// double-click selection and Ctrl+Backspace/Delete in every text box (#16750).
/// </summary>
public class CombiningMarkWordSelectionTests
{
    private const string Marhaban = "مرحبًا";   // tanween U+064B at index 4
    private const string Shukran = "شكراً";    // tanween U+064B last

    [Theory]
    [InlineData('ً', true)] // Arabic fathatan (Mn)
    [InlineData('ָ', true)] // Hebrew qamats (Mn)
    [InlineData('́', true)] // combining acute (Mn)
    [InlineData('‌', true)] // ZWNJ (Persian)
    [InlineData('a', true)]
    [InlineData('_', false)]
    [InlineData(' ', false)]
    [InlineData('.', false)]
    public void IsWordChar(char c, bool expected)
    {
        Assert.Equal(expected, CombiningMarkWordSelection.IsWordChar(c));
    }

    [Theory]
    [InlineData(0, 4)] // Avalonia's "مرحب"
    [InlineData(5, 6)] // Avalonia's "ا"
    [InlineData(4, 5)] // just the mark
    public void SnapToWordEdges_WidensToTheWholeWord(int start, int end)
    {
        var text = "يا " + Marhaban + " صديقي";
        var (s, e) = CombiningMarkWordSelection.SnapToWordEdges(text, start + 3, end + 3);
        Assert.Equal(Marhaban, text[s..e]);
    }

    [Fact]
    public void SnapToWordEdges_TrailingMark()
    {
        var text = Shukran + " جزيلا";
        var (s, e) = CombiningMarkWordSelection.SnapToWordEdges(text, 0, 4);
        Assert.Equal(Shukran, text[s..e]);
    }

    [Theory]
    [InlineData("hello brave world", 6, 11)]
    [InlineData("foo_bar baz", 0, 3)]
    [InlineData("hello, world", 5, 6)]
    public void SnapToWordEdges_LeavesPlainTextAlone(string text, int start, int end)
    {
        Assert.Equal((start, end), CombiningMarkWordSelection.SnapToWordEdges(text, start, end));
    }

    [Fact]
    public void PreviousWordStart_IncludesTheMark()
    {
        var text = "قال " + Shukran;
        Assert.Equal(4, CombiningMarkWordSelection.PreviousWordStart(text, text.Length));
    }

    [Fact]
    public void NextWordEnd_IncludesTheMarkAndFollowingSpace()
    {
        var text = Marhaban + " يا";
        Assert.Equal(Marhaban.Length + 1, CombiningMarkWordSelection.NextWordEnd(text, 0));
    }

    private static (Window window, TextBox textBox) Show(string text)
    {
        CombiningMarkWordSelection.Register();
        var textBox = new TextBox { AcceptsReturn = true, Text = text, Width = 400, FontSize = 14 };
        var window = new Window { Width = 500, Height = 200, Content = textBox };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        textBox.Focus();
        Dispatcher.UIThread.RunJobs();
        return (window, textBox);
    }

    private static KeyModifiers WordModifiers =>
        Application.Current?.PlatformSettings?.HotkeyConfiguration.WholeWordTextActionModifiers ?? KeyModifiers.Control;

    private static RawInputModifiers RawWordModifiers =>
        WordModifiers == KeyModifiers.Alt ? RawInputModifiers.Alt : RawInputModifiers.Control;

    [AvaloniaFact]
    public void SnapSelection_FixesAvaloniasHalfWordSelection()
    {
        var (window, textBox) = Show(Marhaban + " يا");
        try
        {
            textBox.SelectionStart = 0;
            textBox.SelectionEnd = 4; // what Avalonia's double-click selected
            CombiningMarkWordSelection.SnapSelection(textBox);
            Assert.Equal(Marhaban, textBox.SelectedText);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DoubleClick_SelectsWholeWordWithMark()
    {
        var (window, textBox) = Show("hello " + Marhaban + " world");
        try
        {
            var presenter = textBox.GetVisualDescendants().OfType<TextPresenter>().First();
            var rect = presenter.TextLayout.HitTestTextPosition(7);
            var point = presenter.TranslatePoint(new Point(rect.X + 1, rect.Y + rect.Height / 2), window)!.Value;

            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(Marhaban, textBox.SelectedText);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void WordBackspace_DeletesWholeWordWithTrailingMark()
    {
        var (window, textBox) = Show("قال " + Shukran);
        try
        {
            textBox.CaretIndex = textBox.Text!.Length;
            window.KeyPress(Key.Back, RawWordModifiers, PhysicalKey.Backspace, null);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("قال ", textBox.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void WordDelete_DeletesWholeWordWithMark()
    {
        var (window, textBox) = Show(Marhaban + " يا");
        try
        {
            textBox.CaretIndex = 0;
            window.KeyPress(Key.Delete, RawWordModifiers, PhysicalKey.Delete, null);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("يا", textBox.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void WordBackspace_PlainTextUnchanged()
    {
        var (window, textBox) = Show("hello world");
        try
        {
            textBox.CaretIndex = textBox.Text!.Length;
            window.KeyPress(Key.Back, RawWordModifiers, PhysicalKey.Backspace, null);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("hello ", textBox.Text);
        }
        finally
        {
            window.Close();
        }
    }
}
