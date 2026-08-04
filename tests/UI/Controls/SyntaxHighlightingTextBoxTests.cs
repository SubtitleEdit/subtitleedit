using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls;

namespace UITests.Controls;

public class SyntaxHighlightingTextBoxTests
{
    // Ligatures merge letter sequences like "ffi" into one glyph cluster, making the caret
    // and mouse selection treat them as a single character (#12585) - the edit box must
    // disable standard/contextual ligatures ("liga"/"clig") while keeping script-critical
    // features (e.g. "rlig") untouched.
    [AvaloniaFact]
    public void DisablesStandardAndContextualLigatures()
    {
        var textBox = new SyntaxHighlightingTextBox();

        Assert.NotNull(textBox.FontFeatures);
        var features = textBox.FontFeatures!.ToList();
        Assert.Equal(2, features.Count);
        Assert.All(features, f => Assert.Equal(0, f.Value));
        Assert.Contains(features, f => f.Tag == "liga");
        Assert.Contains(features, f => f.Tag == "clig");
    }

    // The presenter builds its text runs from its own FontFeatures property, so the value
    // set on the text box must reach the templated presenter via property inheritance.
    [AvaloniaFact]
    public void FontFeaturesReachTheTemplatedPresenter()
    {
        var textBox = new SyntaxHighlightingTextBox();
        var window = new Window { Content = textBox, Width = 320, Height = 120 };
        window.Show();
        textBox.ApplyTemplate();

        var presenter = textBox.GetVisualDescendants().OfType<TextPresenter>().Single();
        Assert.Same(textBox.FontFeatures, presenter.FontFeatures);
    }
}
