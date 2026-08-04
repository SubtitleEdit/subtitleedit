using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic;
using System;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// A <see cref="TextBox"/> that colors HTML tags and ASS/SSA override tags while typing, using
/// the shared subtitle syntax color scheme (SubtitleSyntaxTokenizer) - or a whole source format
/// when <see cref="SourceHighlighter"/> is set.
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

    /// <summary>
    /// Colors a whole source document (media info, a subtitle format preview) instead of the
    /// default subtitle tag coloring. Set it when the box is created; it is not meant to change
    /// afterwards (the presenter caches its spans per text, not per highlighter).
    /// </summary>
    public ISourceSyntaxHighlighter? SourceHighlighter { get; init; }

    public ISpellCheckManager? SpellCheckManager { get; private set; }

    public bool IsSpellCheckEnabled { get; private set; }

    public SyntaxHighlightingTextBox()
    {
        PseudoClasses.Add(":syntaxHighlighting");

        // Turn off standard/contextual ligatures: with e.g. DejaVu Sans, "ffi" is otherwise
        // shaped into the single ﬃ glyph, and since the caret and mouse hit testing work on
        // glyph clusters, the three letters behave as one un-enterable character (#12585).
        // An edit box must navigate per character. Required ligatures ("rlig", used by e.g.
        // Arabic lam-alef) and contextual alternates ("calt", cursive joining forms) are
        // deliberately left on - they are script-critical and do not merge Latin letters.
        FontFeatures = FontFeatureCollection.Parse("-liga, -clig");
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
