using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls;

namespace UITests.Controls;

/// <summary>
/// Canary for the reverted PR #12567 (ASSA tag backslash rendering in RTL lines).
///
/// The fix was reverted because Avalonia 12.1.0 cannot describe a line whose display
/// differs from its raw characters: permuting the layout string breaks click-to-caret
/// (no overridable seam on the hit-test path, so clicking inside a tag crashed in
/// TextSelectionHandleCanvas), and directional marks in the text break TextLine
/// geometry. Both are tracked upstream in AvaloniaUI/Avalonia#21792.
///
/// These tests assert the Avalonia limitations still hold. A FAILURE here is good
/// news: it means an Avalonia upgrade delivered one of the missing pieces, and the
/// tag rendering fix can come back - either resurrect PR #12567 with hit-test index
/// translation, or drop the workaround entirely if Avalonia now orders the tag
/// characters correctly on its own.
/// </summary>
public class AssaTagRtlAvaloniaCanaryTests
{
    // PR #12567's approach needs to translate hit-test/caret indices from the permuted
    // layout back to Text. A click goes TextBox -> TextPresenter.MoveCaretToPoint ->
    // TextLayout.HitTestPoint; the approach becomes viable as soon as any point on that
    // path can be overridden.
    [Fact]
    public void ClickToCaretPathHasNoOverridableSeam()
    {
        var moveCaretToPoint = typeof(TextPresenter).GetMethod(
            "MoveCaretToPoint", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(moveCaretToPoint);
        Assert.False(IsOverridable(moveCaretToPoint!),
            "TextPresenter.MoveCaretToPoint is now overridable - PR #12567 can map layout indices back to Text.");

        foreach (var name in new[] { "HitTestPoint", "HitTestTextRange", "HitTestTextPosition" })
        {
            var method = typeof(TextLayout).GetMethod(
                name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);
            Assert.False(IsOverridable(method!),
                $"TextLayout.{name} is now overridable - PR #12567 can map layout indices back to Text.");
        }
    }

    private static bool IsOverridable(MethodInfo method) => method.IsVirtual && !method.IsFinal;

    // The symptom itself, measured through SE's real template (SyntaxHighlightingTextPresenter
    // + Skia shaping): in an RTL line the tag's backslash resolves to the paragraph bidi
    // level and lands on the wrong side of the letter run, so "{\an8}" reads as "{an8\}"
    // (the braces render mirrored, so the tag is read left-to-right). Displaced means the
    // backslash sits visually to the RIGHT of "an8"; the correct rendering puts it to the
    // LEFT, between the opening brace and the "a".
    [AvaloniaFact]
    public void AssaTagBackslashStillRendersDisplacedInRtlLines()
    {
        var styles = (Styles)AvaloniaXamlLoader.Load(new Uri("avares://SubtitleEdit/Styles.axaml"));
        var textBox = new SyntaxHighlightingTextBox
        {
            Text = "{\\an8}هذا",
            FlowDirection = FlowDirection.RightToLeft,
            FontSize = 16,
        };
        var window = new Window { Content = textBox, Width = 400, Height = 120 };
        window.Styles.Add(styles);
        window.Show();
        textBox.ApplyTemplate();
        window.UpdateLayout();

        var presenter = textBox.GetVisualDescendants().OfType<SyntaxHighlightingTextPresenter>().Single();
        var line = presenter.TextLayout.TextLines[0];

        // Text = { \ a n 8 } ... ; visual x position of the backslash vs the letter run.
        var backslashLeft = line.GetTextBounds(1, 1).Single().Rectangle.Left;
        var letterRunRight = line.GetTextBounds(2, 3).Max(b => b.Rectangle.Right);

        Assert.True(backslashLeft >= letterRunRight,
            "The ASSA tag backslash now renders in its true spot in RTL lines - Avalonia has fixed " +
            "the bidi ordering (AvaloniaUI/Avalonia#21792), and the workaround from PR #12567 is " +
            "either unnecessary or can be resurrected. Re-evaluate RTL tag rendering.");
    }
}
