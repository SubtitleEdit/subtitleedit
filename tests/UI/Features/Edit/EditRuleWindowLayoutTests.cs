using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Edit;

/// <summary>
/// The "Edit rule" window used to be a fixed-size dialog with three 300 px single-line text
/// boxes, so a long regular expression could only ever be read ~60 characters at a time and
/// the window could not be resized to see more (#13530). These tests pin the layout that
/// fixes it: a resizable window whose find/replace boxes stretch with it and wrap.
/// </summary>
public class EditRuleWindowLayoutTests : IDisposable
{
    // An unclosed window outlives the test and races with the headless session teardown -
    // and a stranded modal steals app-wide keyboard focus from every later test.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            // InitializeWindow posts a clamp-to-screen callback from Opened; flush it while the
            // window is still alive so it does not run against a disposed platform implementation.
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.Close();
        }

        _windows.Clear();
    }

    private EditRuleWindow BuildWindow(string findWhat = "", string replaceWith = "")
    {
        var vm = new EditRuleViewModel
        {
            FindWhat = findWhat,
            ReplaceWith = replaceWith,
        };
        var window = new EditRuleWindow(vm);
        _windows.Add(window);
        return window;
    }

    private static List<TextBox> TextBoxes(EditRuleWindow window)
    {
        return window.GetLogicalDescendants().OfType<TextBox>().ToList();
    }

    [AvaloniaFact]
    public void Window_IsResizable_AndOpensWideEnoughForALongRule()
    {
        var window = BuildWindow();

        Assert.True(window.CanResize, "The edit rule window must be resizable.");
        Assert.True(window.Width >= 600, $"The edit rule window opens only {window.Width:0.#} wide.");
    }

    [AvaloniaFact]
    public void FindAndReplaceBoxes_FillTheWindowAndWrap()
    {
        var window = BuildWindow();
        window.Show();
        window.UpdateLayout();

        var textBoxes = TextBoxes(window);
        Assert.Equal(3, textBoxes.Count); // find, replace with, description

        foreach (var textBox in textBoxes.Take(2))
        {
            Assert.Equal(TextWrapping.Wrap, textBox.TextWrapping);

            // Enter is the dialog's OK shortcut, so the boxes wrap but never take a newline.
            Assert.False(textBox.AcceptsReturn);

            Assert.True(
                textBox.Bounds.Width > window.Width / 2,
                $"Text box is only {textBox.Bounds.Width:0.#} wide in a {window.Width:0.#} wide window.");
        }
    }

    // The boxes can only follow the window if nothing pins their width: the old layout had a
    // hard Width = 300 in an Auto column, so resizing the window just added empty space.
    [AvaloniaFact]
    public void NothingPinsTheWidthOfTheFieldColumn()
    {
        var window = BuildWindow();
        window.Show();
        window.UpdateLayout();

        var grid = Assert.IsType<Grid>(window.Content);
        var fieldColumn = grid.ColumnDefinitions[1];
        Assert.Equal(GridUnitType.Star, fieldColumn.Width.GridUnitType);

        foreach (var textBox in TextBoxes(window))
        {
            Assert.True(double.IsNaN(textBox.Width), $"Text box has a fixed width of {textBox.Width:0.#}.");
            Assert.Equal(HorizontalAlignment.Stretch, textBox.HorizontalAlignment);
        }
    }

    [AvaloniaFact]
    public void ALongRule_IsLaidOutOverSeveralLines()
    {
        // ~180 characters - the kind of rule that used to scroll past a 60 character peephole.
        var longRule = string.Concat(Enumerable.Repeat("(?<=[a-z])(foo|bar|baz)(?=[A-Z])|", 6));
        var window = BuildWindow(longRule);
        window.Show();
        window.UpdateLayout();

        var presenter = TextBoxes(window)[0].GetVisualDescendants().OfType<TextPresenter>().FirstOrDefault();
        Assert.NotNull(presenter);
        Assert.True(
            presenter.TextLayout.TextLines.Count > 1,
            $"A {longRule.Length} character rule was laid out on {presenter.TextLayout.TextLines.Count} line(s).");
    }
}
