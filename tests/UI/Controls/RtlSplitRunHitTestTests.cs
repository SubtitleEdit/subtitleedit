using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Styling;
using Avalonia.Utilities;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls;

namespace UITests.Controls;

/// <summary>
/// #15531: a Latin stretch split into several style runs inside a right-to-left line made
/// Avalonia 12.1.3 (BidiReorderer) map runs to the wrong visual positions, so hit testing
/// threw "Covered length must be greater than zero" and SE froze/crashed when selecting
/// next to a colored tag. Fixed upstream in Avalonia 12.1.4 (SE's BidiRunIndexFixer workaround
/// was removed); these tests guard against a regression on future Avalonia upgrades.
/// </summary>
public class RtlSplitRunHitTestTests : IDisposable
{
    // The line from #15531 ("Surround with" adds "<font size=30>\N" to lift the subtitle).
    private const string RtlLineWithTags = "أريد أن أقول...<font size=30>\\N<font size=30>\\N";

    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    // "أريد abc" with "a" and "bc" in different styles: two LTR runs inside an RTL line.
    private static TextLayout CreateSplitLatinInRtlLayout()
    {
        const string text = "أريد abc";
        var typeface = new Typeface(FontFamily.Default);
        var black = new GenericTextRunProperties(typeface, 24, foregroundBrush: Brushes.Black);
        var blue = new GenericTextRunProperties(typeface, 24, foregroundBrush: Brushes.Blue);
        var spans = new List<ValueSpan<TextRunProperties>>
        {
            new(0, 6, black),
            new(6, 2, blue),
        };

        return new TextLayout(text, typeface, 24, Brushes.Black, TextAlignment.Left, TextWrapping.Wrap,
            TextTrimming.None, null, FlowDirection.RightToLeft, double.PositiveInfinity, double.PositiveInfinity,
            double.NaN, 0, 0, null, spans);
    }

    private static List<string> GetFailingRanges(TextLayout layout, int length)
    {
        var failures = new List<string>();
        for (var start = 0; start < length; start++)
        {
            for (var end = start + 1; end <= length; end++)
            {
                try
                {
                    layout.HitTestTextRange(start, end - start);
                }
                catch (Exception exception)
                {
                    failures.Add($"{start}+{end - start}: {exception.Message}");
                }
            }
        }

        return failures;
    }

    [AvaloniaFact]
    public void SplitLatinInRtlLayoutHitTestsEveryRangeAndMeasuresTheRightRuns()
    {
        var layout = CreateSplitLatinInRtlLayout();

        Assert.Empty(GetFailingRanges(layout, 8));

        // "abc" reads left to right at the left edge of the line: "a" must start where the
        // whole Latin stretch starts and end before "c" starts.
        var latin = layout.HitTestTextRange(5, 3).Single();
        var a = layout.HitTestTextRange(5, 1).Single();
        var c = layout.HitTestTextRange(7, 1).Single();
        Assert.Equal(latin.Left, a.Left, 3);
        Assert.True(a.Right <= c.Left + 0.01, $"a={a} c={c}");
        Assert.Equal(latin.Right, c.Right, 3);
    }

    [AvaloniaFact]
    public void SelectingAnywhereInRtlLineWithColoredTagsDoesNotThrow()
    {
        var styles = (Styles)AvaloniaXamlLoader.Load(new Uri("avares://SubtitleEdit/Styles.axaml"));
        var textBox = new SyntaxHighlightingTextBox
        {
            Text = RtlLineWithTags,
            FlowDirection = FlowDirection.RightToLeft,
            FontSize = 24,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
        };
        var window = new Window { Content = textBox, Width = 520, Height = 160 };
        _windows.Add(window);
        window.Styles.Add(styles);
        window.Show();
        textBox.ApplyTemplate();
        window.UpdateLayout();

        var presenter = textBox.GetVisualDescendants().OfType<SyntaxHighlightingTextPresenter>().Single();
        var length = RtlLineWithTags.Length;

        Assert.Empty(GetFailingRanges(presenter.TextLayout, length));

        // Selections rebuild the layout with the selection overlay; the selection handles and
        // the render pass hit test the selected range (the stack traces in #15531).
        var failures = new List<string>();
        for (var start = 0; start <= length; start += 3)
        {
            for (var end = 0; end <= length; end += 2)
            {
                try
                {
                    textBox.SelectionStart = start;
                    textBox.SelectionEnd = end;
                    window.UpdateLayout();
                    if (start != end)
                    {
                        presenter.TextLayout.HitTestTextRange(Math.Min(start, end), Math.Abs(end - start));
                    }
                }
                catch (Exception exception)
                {
                    failures.Add($"{start}->{end}: {exception.Message}");
                }
            }
        }

        Assert.Empty(failures);
    }
}
