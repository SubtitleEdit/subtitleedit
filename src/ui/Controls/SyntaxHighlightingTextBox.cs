using Avalonia.Controls;
using System;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// A <see cref="TextBox"/> that colors HTML tags and ASS/SSA override tags while typing, using
/// the same color scheme as the AvaloniaEdit based editor (SubtitleSyntaxHighlighting).
/// It behaves exactly like a normal text box (alignment, selection, IME, context menu) -
/// the coloring is done by <see cref="SyntaxHighlightingTextPresenter"/>, which the
/// ":syntaxHighlighting" style in Styles.axaml swaps in for the default text presenter.
/// </summary>
public class SyntaxHighlightingTextBox : TextBox
{
    protected override Type StyleKeyOverride => typeof(TextBox);

    public SyntaxHighlightingTextBox()
    {
        PseudoClasses.Add(":syntaxHighlighting");
    }
}
