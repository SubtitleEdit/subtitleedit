using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

public class RightToLeftEditorTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }
    }

    private (Window Window, TextBox Box, TextPresenter Presenter) Show(bool colorTags, string text)
    {
        TextBox box = colorTags ? new SyntaxHighlightingTextBox() : new TextBox();
        box.AcceptsReturn = true;
        box.TextWrapping = TextWrapping.Wrap;
        box.FontSize = 24;
        RightToLeftHelper.ConfigureTextBox(box);
        box.Text = text;

        var window = new Window { Content = box, Width = 520, Height = 200 };
        _windows.Add(window);
        window.Styles.Add((Styles)AvaloniaXamlLoader.Load(new Uri("avares://SubtitleEdit/Styles.axaml")));
        window.Show();
        Settle(window);
        box.Focus();
        Assert.True(box.IsFocused);
        return (window, box, box.GetVisualDescendants().OfType<TextPresenter>().Single());
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    [AvaloniaTheory]
    [InlineData(false, "\r\n", "שלום עולם")]
    [InlineData(true, "\r\n", "שלום עולם")]
    [InlineData(false, "\n", "שלום עולם")]
    [InlineData(true, "\n", "שלום עולם")]
    [InlineData(false, "\n", "שלום abc")]
    [InlineData(true, "\n", "שלום abc")]
    [InlineData(false, "\n", "שלום 😀")]
    [InlineData(true, "\n", "שלום 😀")]
    [InlineData(false, "\n", "שָׁלוֹם")]
    [InlineData(true, "\n", "שָׁלוֹם")]
    public void EndInHebrewStopsBeforeTheLineBreak(bool colorTags, string lineBreak, string first)
    {
        var (window, box, presenter) = Show(colorTags, first + lineBreak + "שורה שנייה");
        box.CaretIndex = 2;
        Settle(window);
        var caretY = presenter.CursorRectangle.Y;

        window.KeyPress(Key.End, RawInputModifiers.None, PhysicalKey.End, string.Empty);
        Settle(window);

        Assert.Equal(first.Length, box.CaretIndex);
        Assert.Equal(first.Length, presenter.CaretIndex);
        Assert.Equal(caretY, presenter.CursorRectangle.Y);
        window.KeyPress(Key.End, RawInputModifiers.None, PhysicalKey.End, string.Empty);
        Settle(window);
        Assert.Equal(first.Length, box.CaretIndex);
        Assert.Equal(first.Length, presenter.CaretIndex);
        window.KeyTextInput("!");
        Settle(window);
        Assert.Equal(first + "!" + lineBreak + "שורה שנייה", box.Text);
    }

    [AvaloniaTheory]
    [InlineData(false, "\n", false)]
    [InlineData(true, "\n", false)]
    [InlineData(false, "\r\n", false)]
    [InlineData(true, "\r\n", false)]
    [InlineData(false, "\n", true)]
    [InlineData(true, "\n", true)]
    [InlineData(false, "\r\n", true)]
    [InlineData(true, "\r\n", true)]
    public void EndDrawsTheCaretAfterTheLastHebrewGlyph(bool colorTags, string lineBreak, bool bold)
    {
        const string first = "שלום עולם";
        var (window, box, presenter) = Show(colorTags, first + lineBreak + "שורה שנייה");
        box.FontWeight = bold ? FontWeight.Bold : FontWeight.Normal;
        box.CaretIndex = 2;
        Settle(window);
        var lastGlyph = presenter.TextLayout.TextLines[0].GetTextBounds(first.Length - 1, 1).Single().Rectangle;

        window.KeyPress(Key.End, RawInputModifiers.None, PhysicalKey.End, string.Empty);
        Settle(window);

        Assert.Equal(first.Length, box.CaretIndex);
        Assert.Equal(lastGlyph.Left, presenter.CursorRectangle.X, 3);

        // A new measure pass must keep the drawn caret at the same logical edge.
        window.Width += 40;
        Settle(window);
        lastGlyph = presenter.TextLayout.TextLines[0].GetTextBounds(first.Length - 1, 1).Single().Rectangle;
        Assert.Equal(lastGlyph.Left, presenter.CursorRectangle.X, 3);
        window.KeyPress(Key.End, RawInputModifiers.None, PhysicalKey.End, string.Empty);
        Settle(window);
        Assert.Equal(lastGlyph.Left, presenter.CursorRectangle.X, 3);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ControlShiftOverridesContentDirectionWhileEditing(bool colorTags)
    {
        var (window, box, _) = Show(colorTags, "שלום עולם");
        Assert.Equal(FlowDirection.RightToLeft, box.FlowDirection);

        window.KeyPress(Key.LeftShift, RawInputModifiers.Control | RawInputModifiers.Shift,
            PhysicalKey.ShiftLeft, string.Empty);
        window.KeyRelease(Key.LeftShift, RawInputModifiers.Control, PhysicalKey.ShiftLeft, string.Empty);
        Settle(window);
        Assert.Equal(FlowDirection.LeftToRight, box.FlowDirection);

        box.CaretIndex = box.Text!.Length;
        window.KeyTextInput("!");
        Settle(window);
        Assert.Equal(FlowDirection.LeftToRight, box.FlowDirection);
        Assert.Equal("שלום עולם!", box.Text);

        window.KeyPress(Key.RightShift, RawInputModifiers.Control | RawInputModifiers.Shift,
            PhysicalKey.ShiftRight, string.Empty);
        window.KeyRelease(Key.RightShift, RawInputModifiers.Control, PhysicalKey.ShiftRight, string.Empty);
        Settle(window);
        Assert.Equal(FlowDirection.RightToLeft, box.FlowDirection);
    }

    [AvaloniaTheory]
    [InlineData(false, "\n")]
    [InlineData(true, "\n")]
    [InlineData(false, "\r\n")]
    [InlineData(true, "\r\n")]
    public void ShiftEndSelectsOnlyTheRestOfTheCurrentLine(bool colorTags, string lineBreak)
    {
        const string first = "שלום עולם";
        var (window, box, _) = Show(colorTags, first + lineBreak + "שורה שנייה");
        box.CaretIndex = 2;
        Settle(window);

        window.KeyPress(Key.End, RawInputModifiers.Shift, PhysicalKey.End, string.Empty);
        Settle(window);

        Assert.Equal(first[2..], box.SelectedText);
        Assert.Equal(first.Length, box.CaretIndex);
        window.KeyPress(Key.End, RawInputModifiers.Shift, PhysicalKey.End, string.Empty);
        Settle(window);
        Assert.Equal(first[2..], box.SelectedText);

        window.KeyPress(Key.End, RawInputModifiers.Control, PhysicalKey.End, string.Empty);
        Settle(window);
        Assert.Equal(box.Text!.Length, box.CaretIndex);
        Assert.Empty(box.SelectedText);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void EndAtASoftWrapStaysOnTheSameVisualLine(bool colorTags)
    {
        var (window, box, presenter) = Show(colorTags, string.Join(" ", Enumerable.Repeat("שלום", 30)));
        var first = presenter.TextLayout.TextLines[0];
        Assert.Equal(0, first.NewLineLength);
        Assert.True(presenter.TextLayout.TextLines.Count > 1);
        box.CaretIndex = 2;
        Settle(window);

        window.KeyPress(Key.End, RawInputModifiers.None, PhysicalKey.End, string.Empty);
        Settle(window);

        Assert.Equal(first.Length, box.CaretIndex);
        Assert.Equal(0, presenter.CursorRectangle.Y);
        window.KeyPress(Key.End, RawInputModifiers.None, PhysicalKey.End, string.Empty);
        Settle(window);
        Assert.Equal(first.Length, box.CaretIndex);
        Assert.Equal(0, presenter.CursorRectangle.Y);
    }

    [AvaloniaTheory]
    [InlineData(false, "\n")]
    [InlineData(true, "\n")]
    [InlineData(false, "\r\n")]
    [InlineData(true, "\r\n")]
    public void RightArrowAfterEndMovesFromTheEndOfTheLine(bool colorTags, string lineBreak)
    {
        const string first = "שלום עולם";
        var (window, box, presenter) = Show(colorTags, first + lineBreak + "שורה שנייה");
        box.CaretIndex = 2;
        window.KeyPress(Key.End, RawInputModifiers.None, PhysicalKey.End, string.Empty);
        Settle(window);

        window.KeyPress(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, string.Empty);
        Settle(window);

        Assert.Equal(first.Length - 1, box.CaretIndex);
        Assert.Equal(box.CaretIndex, presenter.CaretIndex);
        Assert.Equal(0, presenter.CursorRectangle.Y);
    }

    [AvaloniaFact]
    public void PlainEditorKeepsTagsInTheDefaultTextColor()
    {
        var (_, _, presenter) = Show(false, "<i>שלום</i>");
        var runs = presenter.TextLayout.TextLines.SelectMany(l => l.TextRuns).OfType<ShapedTextRun>().ToList();

        Assert.NotEmpty(runs);
        Assert.All(runs, run => Assert.Equal(presenter.Foreground, run.Properties!.ForegroundBrush));
    }

    [AvaloniaTheory]
    [InlineData(false, "\n")]
    [InlineData(true, "\n")]
    [InlineData(false, "\r\n")]
    [InlineData(true, "\r\n")]
    public void EmptyLinesAndDocumentEndKeepTheirTextOffsets(bool colorTags, string lineBreak)
    {
        const string first = "שלום עולם";
        var text = first + lineBreak + lineBreak + "שורה שנייה" + lineBreak;
        var (window, box, presenter) = Show(colorTags, text);
        Assert.Equal(4, presenter.TextLayout.TextLines.Count);
        Assert.Equal(new[] { 0, first.Length + lineBreak.Length, first.Length + lineBreak.Length * 2, text.Length },
            presenter.TextLayout.TextLines.Select(l => l.FirstTextSourceIndex));
        box.CaretIndex = first.Length + lineBreak.Length;

        window.KeyPress(Key.End, RawInputModifiers.None, PhysicalKey.End, string.Empty);
        Settle(window);
        Assert.Equal(first.Length + lineBreak.Length, box.CaretIndex);
        Assert.Equal(presenter.TextLayout.TextLines[0].Height, presenter.CursorRectangle.Y, 3);

        window.KeyPress(Key.End, RawInputModifiers.Control, PhysicalKey.End, string.Empty);
        Settle(window);
        Assert.Equal(text.Length, box.CaretIndex);
        Assert.Equal(text.Length, presenter.CaretIndex);
        Assert.Equal(presenter.TextLayout.TextLines.Take(3).Sum(l => l.Height), presenter.CursorRectangle.Y, 3);
        Assert.Equal(text, box.Text);
    }

    [AvaloniaFact]
    public void DirectionOverrideSurvivesContentAndModeChangesAndPreservesSelection()
    {
        var (window, box, _) = Show(true, "שלום עולם");
        box.SelectionStart = 2;
        box.SelectionEnd = 5;
        Settle(window);
        var caret = box.CaretIndex;

        window.KeyPress(Key.LeftCtrl, RawInputModifiers.Control | RawInputModifiers.Shift,
            PhysicalKey.ControlLeft, string.Empty);
        window.KeyRelease(Key.LeftCtrl, RawInputModifiers.Shift, PhysicalKey.ControlLeft, string.Empty);
        Settle(window);

        Assert.Equal(FlowDirection.LeftToRight, box.FlowDirection);
        Assert.Equal("שלום עולם", box.Text);
        Assert.Equal(2, box.SelectionStart);
        Assert.Equal(5, box.SelectionEnd);
        Assert.Equal(caret, box.CaretIndex);

        box.Text = "שורה אחרת";
        RightToLeftHelper.SetRightToLeftForDataGridAndText(window);
        Assert.Equal(FlowDirection.LeftToRight, box.FlowDirection);

        window.KeyPress(Key.RightCtrl, RawInputModifiers.Control | RawInputModifiers.Shift,
            PhysicalKey.ControlRight, string.Empty);
        window.KeyRelease(Key.RightCtrl, RawInputModifiers.Shift, PhysicalKey.ControlRight, string.Empty);
        Settle(window);
        box.Text = "English text";
        Assert.Equal(FlowDirection.RightToLeft, box.FlowDirection);
    }

    [AvaloniaFact]
    public void ControlShiftLetterDoesNotOverrideDirectionEvenWhenTheWindowHandlesIt()
    {
        var (window, box, _) = Show(false, "שלום עולם");
        window.AddHandler(InputElement.KeyDownEvent, (_, e) =>
        {
            if (e.Key == Key.S)
            {
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        window.KeyPress(Key.LeftShift, RawInputModifiers.Control | RawInputModifiers.Shift,
            PhysicalKey.ShiftLeft, string.Empty);
        window.KeyPress(Key.S, RawInputModifiers.Control | RawInputModifiers.Shift, PhysicalKey.S, string.Empty);
        window.KeyRelease(Key.LeftShift, RawInputModifiers.Control, PhysicalKey.ShiftLeft, string.Empty);
        Settle(window);

        Assert.Equal(FlowDirection.RightToLeft, box.FlowDirection);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void MainWindowConfiguresBothSubtitleEditors(bool colorTags)
    {
        using var settings = new SettingsScope("Appearance.SubtitleTextBoxColorTags", "General.LibMpvPath");
        Se.Settings.Appearance.SubtitleTextBoxColorTags = colorTags;
        // No video is loaded; suppress the Windows startup download prompt so it
        // cannot disable keyboard input to the main window under test.
        Se.Settings.General.LibMpvPath = "headless-test-no-video";
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();
        var window = new Window { Width = 1400, Height = 900 };
        _windows.Add(window);
        MainView.NextHostWindow = window;
        var view = new MainView();
        MainViewModel? vm = null;
        try
        {
            window.Content = view;
            window.Show();
            Settle(window);
            vm = (MainViewModel)view.DataContext!;
            vm.ShowColumnOriginalText = true;
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("שלום עולם\nשורה שנייה", 0, 2000), null!)
            {
                OriginalText = "שלום עולם\nשורה שנייה",
                Number = 1,
            });
            vm.SelectedSubtitleIndex = 0;
            vm.SubtitleGrid.SelectedItem = vm.Subtitles[0];
            Settle(window);

            foreach (var wrapper in new[] { vm.EditTextBox, vm.EditTextBoxOriginal })
            {
                var box = Assert.IsAssignableFrom<TextBox>(wrapper.TextControl);
                Assert.Equal(colorTags, box is SyntaxHighlightingTextBox);
                box.Focus();
                Assert.True(box.IsFocused);
                window.KeyPress(Key.LeftShift, RawInputModifiers.Control | RawInputModifiers.Shift,
                    PhysicalKey.ShiftLeft, string.Empty);
                window.KeyRelease(Key.LeftShift, RawInputModifiers.Control, PhysicalKey.ShiftLeft, string.Empty);
                Settle(window);
                Assert.Equal(FlowDirection.LeftToRight, box.FlowDirection);

                box.CaretIndex = 2;
                window.KeyPress(Key.End, RawInputModifiers.None, PhysicalKey.End, string.Empty);
                Settle(window);
                Assert.Equal("שלום עולם".Length, box.CaretIndex);
            }
        }
        finally
        {
            if (vm != null)
            {
                window.Closing -= vm.OnClosing;
            }
            window.Close();
        }
    }
}
