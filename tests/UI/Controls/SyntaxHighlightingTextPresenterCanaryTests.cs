using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls;

namespace UITests.Controls;

/// <summary>
/// Canary for the Avalonia-internals access in <see cref="SyntaxHighlightingTextPresenter"/>.
///
/// The presenter's CreateTextLayout replicates Avalonia's TextPresenter implementation and
/// reaches its private members ("_constraint", "GetCombinedText", "CreateTextLayoutInternal")
/// with UnsafeAccessor. If an Avalonia upgrade renames any of them, the app still compiles and
/// only blows up at runtime on the first layout of the main edit box. This test forces that
/// first layout through the real template, so the breakage surfaces as a red test at upgrade
/// time instead.
/// </summary>
public class SyntaxHighlightingTextPresenterCanaryTests
{
    private static (Window window, SyntaxHighlightingTextBox textBox) ShowTextBox(string text)
    {
        var styles = (Styles)AvaloniaXamlLoader.Load(new Uri("avares://SubtitleEdit/Styles.axaml"));
        var textBox = new SyntaxHighlightingTextBox
        {
            Text = text,
            FontSize = 16,
        };
        var window = new Window { Content = textBox, Width = 400, Height = 120 };
        window.Styles.Add(styles);
        window.Show();
        textBox.ApplyTemplate();
        window.UpdateLayout();
        return (window, textBox);
    }

    [AvaloniaFact]
    public void CreateTextLayoutStillReachesAvaloniaInternals()
    {
        var (_, textBox) = ShowTextBox("{\\1c&H0000FF&}Hello <i>world</i>");

        var presenter = textBox.GetVisualDescendants().OfType<SyntaxHighlightingTextPresenter>().Single();

        // Accessing TextLayout runs CreateTextLayout, which invokes all three UnsafeAccessor
        // hooks; a MissingFieldException/MissingMethodException here means Avalonia renamed a
        // private member the presenter depends on.
        var layout = presenter.TextLayout;
        Assert.NotNull(layout);
        Assert.Equal(textBox.Text!.Length, layout.TextLines.Sum(l => l.Length));

        // A selection takes the selection-highlight overlay path through the same internals.
        textBox.SelectionStart = 0;
        textBox.SelectionEnd = 5;
        presenter.InvalidateSpellCheck(); // public wrapper around InvalidateTextLayout
        Assert.NotNull(presenter.TextLayout);
    }

    [AvaloniaFact]
    public void PresenterUnregistersFromOwnerOnDetach()
    {
        var (window, textBox) = ShowTextBox("Hello");
        Assert.NotNull(textBox.SyntaxPresenter);

        window.Content = null;
        window.UpdateLayout();

        Assert.Null(textBox.SyntaxPresenter);
    }
}
