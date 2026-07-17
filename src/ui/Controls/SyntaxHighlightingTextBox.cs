using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// A <see cref="TextBox"/> that colors HTML tags and ASS/SSA override tags while typing, using
/// the same color scheme as the AvaloniaEdit based editor (SubtitleSyntaxHighlighting).
/// It behaves exactly like a normal text box (alignment, selection, IME, context menu) -
/// the coloring is done by <see cref="SyntaxHighlightingTextPresenter"/>, which the
/// ":syntaxHighlighting" style in Styles.axaml swaps in for the default text presenter.
/// The presenter also renders live spell check underlines when armed via
/// <see cref="EnableSpellCheck"/>.
/// </summary>
public class SyntaxHighlightingTextBox : TextBox
{
    protected override Type StyleKeyOverride => typeof(TextBox);

    /// <summary>
    /// The presenter from the applied template; registers itself when attached to the
    /// visual tree so spell check refreshes can invalidate the cached text layout.
    /// </summary>
    internal SyntaxHighlightingTextPresenter? SyntaxPresenter { get; set; }

    public ISpellCheckManager? SpellCheckManager { get; private set; }

    public bool IsSpellCheckEnabled { get; private set; }

    public SyntaxHighlightingTextBox()
    {
        PseudoClasses.Add(":syntaxHighlighting");
    }

    public void EnableSpellCheck(ISpellCheckManager spellCheckManager)
    {
        SpellCheckManager = spellCheckManager;
        IsSpellCheckEnabled = true;
        RefreshSpellCheck();
    }

    public void DisableSpellCheck()
    {
        IsSpellCheckEnabled = false;
        RefreshSpellCheck();
    }

    /// <summary>
    /// Re-evaluates the underlines (e.g. after "Add to user dictionary" / "Ignore all").
    /// </summary>
    public void RefreshSpellCheck()
    {
        SyntaxPresenter?.InvalidateSpellCheck();
    }

    /// <summary>
    /// The character index under the pointer, or null when there is no text layout yet.
    /// </summary>
    public int? GetCharIndexAtPoint(PointerEventArgs e)
    {
        var presenter = SyntaxPresenter;
        if (presenter == null)
        {
            return null;
        }

        var point = e.GetPosition(presenter);
        var hit = presenter.TextLayout.HitTestPoint(point);
        return hit.TextPosition;
    }
}
