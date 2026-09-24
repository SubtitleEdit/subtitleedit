using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// An .srt line holding only a number is a cue number when it opens a block, and subtitle text
/// ("1984") when it follows the time code - the highlighter needs the line above to tell.
/// </summary>
public class SubRipSourceSyntaxHighlightingTests : IDisposable
{
    private const string Source = "1\n00:00:01,000 --> 00:00:03,000\n1984\n\n2\n00:00:04,000 --> 00:00:05,000\nText\n";

    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private static bool IsStyledAsCueNumber(List<SourceSyntaxSpan> spans, int start, int length) =>
        spans.Exists(span => span.Start == start && span.Length == length && span.Bold);

    [AvaloniaFact]
    public void ANumberOpeningABlockIsACueNumber()
    {
        var spans = SourceSyntaxTokenizer.Tokenize(Source, new SubRipSourceSyntaxHighlighting());

        Assert.True(IsStyledAsCueNumber(spans, 0, 1));
        Assert.True(IsStyledAsCueNumber(spans, Source.IndexOf("\n2\n", StringComparison.Ordinal) + 1, 1));
    }

    [AvaloniaFact]
    public void ANumberUnderTheTimeCodeIsSubtitleText()
    {
        var spans = SourceSyntaxTokenizer.Tokenize(Source, new SubRipSourceSyntaxHighlighting());
        var start = Source.IndexOf("1984", StringComparison.Ordinal);

        Assert.DoesNotContain(spans, span => span.Start < start + 4 && span.Start + span.Length > start);
    }

    [AvaloniaFact]
    public void EditingALineRestylesTheLineBelow()
    {
        var editor = new SyntaxTextEditor { Text = Source, SourceHighlighter = new SubRipSourceSyntaxHighlighting() };
        var window = new Window { Content = editor, Width = 400, Height = 300 };
        _windows.Add(window);
        window.Show();
        window.UpdateLayout();

        var before = editor.View.GetLineLayout(2);

        // Clearing the time code line (same line count) turns "1984" below it into a cue number.
        var start = editor.View.Document.GetLineStartOffset(1);
        editor.View.Select(start, editor.View.Document.GetLineEndOffset(1) - start);
        editor.View.DeleteSelection();

        Assert.NotSame(before, editor.View.GetLineLayout(2));
    }
}
