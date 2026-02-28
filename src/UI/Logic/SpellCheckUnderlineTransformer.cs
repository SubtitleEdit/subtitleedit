using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public class SpellCheckUnderlineTransformer : DocumentColorizingTransformer
{
    private ISpellCheckManager? _spellCheckManager;
    private bool _isEnabled;
    private TextView? _textView;

    private static readonly Color ErrorColor = Colors.Red;
    private static readonly TextDecoration WavyUnderline = new()
    {
        Location = TextDecorationLocation.Underline,
        Stroke = new SolidColorBrush(ErrorColor),
        StrokeThickness = 1.5,
        StrokeLineCap = PenLineCap.Round,
        StrokeThicknessUnit = TextDecorationUnit.FontRecommended
    };

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                OnEnabledChanged();
            }
        }
    }

    public void SetTextView(TextView textView)
    {
        _textView = textView;
    }

    public void SetSpellCheckManager(ISpellCheckManager? spellCheckManager)
    {
        _spellCheckManager = spellCheckManager;
        Refresh();
    }

    public void Refresh()
    {
        _textView?.Redraw();
    }

    private void OnEnabledChanged()
    {
        _textView?.Redraw();
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        if (!_isEnabled || _spellCheckManager == null || line.Length == 0)
        {
            return;
        }

        try
        {
            var lineText = CurrentContext.Document.GetText(line.Offset, line.Length);
            if (string.IsNullOrWhiteSpace(lineText))
            {
                return;
            }

            var words = SpellCheckWordLists2.Split(lineText);
            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word.Text) || word.Length < 2)
                {
                    continue;
                }

                // Skip words that are numbers, URLs, or special patterns
                if (IsSpecialPattern(word, lineText))
                {
                    continue;
                }

                // Check if word is correct
                if (!_spellCheckManager.IsWordCorrect(word.Text))
                {
                    var startOffset = line.Offset + word.Index;
                    var endOffset = startOffset + word.Length;

                    // Apply wavy underline to misspelled word
                    ChangeLinePart(startOffset, endOffset, element =>
                    {
                        if (element.TextRunProperties.TextDecorations == null)
                        {
                            element.TextRunProperties.SetTextDecorations(new TextDecorationCollection { WavyUnderline });
                        }
                        else
                        {
                            var decorations = new List<TextDecoration>(element.TextRunProperties.TextDecorations)
                            {
                                WavyUnderline
                            };
                            element.TextRunProperties.SetTextDecorations(new TextDecorationCollection(decorations));
                        }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex);
        }
    }

    private static bool IsSpecialPattern(SpellCheckWord word, string text)
    {
        // Skip numbers
        if (word.Text.All(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-'))
        {
            return true;
        }

        // Skip URLs
        if (word.Text.Contains("http://", StringComparison.OrdinalIgnoreCase) ||
            word.Text.Contains("https://", StringComparison.OrdinalIgnoreCase) ||
            word.Text.Contains("www.", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Skip email-like patterns
        if (word.Text.Contains('@'))
        {
            return true;
        }

        // Skip hashtags
        if (word.Text.StartsWith('#'))
        {
            return true;
        }

        if (IsBetweenAssaTags(word, text))
        { 
            return true;
        }

        if (IsInsideHtmlTag(word, text)) 
        {
            return true;
        }   

        return false;
    }

    private static bool IsBetweenAssaTags(SpellCheckWord word, string text)
    {
        if (word == null || string.IsNullOrEmpty(text))
        {
            return false;
        }

        // 1. Find the last occurrence of an opening brace before the word starts
        var openBrace = text.LastIndexOf('{', word.Index);

        // 2. Find the first occurrence of a closing brace after the word starts
        var closeBrace = text.IndexOf('}', word.Index);

        // If both exist, check if there is another closing brace between 
        // the opening brace and our word. 
        // If not, it means we are currently inside an unclosed tag.
        if (openBrace != -1 && closeBrace != -1 && openBrace < closeBrace)
        {
            // Check if there's a '}' between the '{' and the word.
            // If there is, the word is actually OUTSIDE a tag.
            var closingBeforeWord = text.IndexOf('}', openBrace, word.Index - openBrace);

            return closingBeforeWord == -1;
        }

        return false;
    }

    private static bool IsInsideHtmlTag(SpellCheckWord word, string text)
    {
        if (word == null || string.IsNullOrEmpty(text))
        {
            return false;
        }

        // 1. Find the last opening bracket before the word
        var openBracket = text.LastIndexOf('<', word.Index);

        // 2. Find the next closing bracket after the word starts
        var closeBracket = text.IndexOf('>', word.Index);

        // If both exist in the correct order
        if (openBracket != -1 && closeBracket != -1 && openBracket < closeBracket)
        {
            // Ensure there isn't a '>' between the opening '<' and the word
            // (which would mean the word is outside a tag)
            var closingBeforeWord = text.IndexOf('>', openBracket, word.Index - openBracket);

            return closingBeforeWord == -1;
        }

        return false;
    }

    /// <summary>
    /// Checks if a specific word is misspelled according to the spell checker.
    /// </summary>
    public bool IsWordMisspelled(SpellCheckWord word, string text)
    {
        if (!_isEnabled || _spellCheckManager == null || string.IsNullOrWhiteSpace(word.Text))
        {
            return false;
        }

        if (IsSpecialPattern(word, text))
        {
            return false;
        }

        return !_spellCheckManager.IsWordCorrect(word.Text);
    }

    /// <summary>
    /// Gets spelling suggestions for a word.
    /// </summary>
    public List<string>? GetSuggestions(string word)
    {
        if (_spellCheckManager == null || string.IsNullOrWhiteSpace(word))
        {
            return null;
        }

        return _spellCheckManager.GetSuggestions(word);
    }
}
