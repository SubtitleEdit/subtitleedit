using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Syntax highlighting for JSON-based subtitle formats
/// </summary>
public partial class JsonSourceSyntaxHighlighting : DocumentColorizingTransformer
{
    // Color scheme (VS Code-like JSON colors)
    private static readonly IBrush PropertyNameBrush = new SolidColorBrush(Color.Parse("#9CDCFE")); // Light blue for property names
    private static readonly IBrush StringBrush = new SolidColorBrush(Color.Parse("#CE9178")); // Orange for string values
    private static readonly IBrush NumberBrush = new SolidColorBrush(Color.Parse("#B5CEA8")); // Light green for numbers
    private static readonly IBrush BooleanBrush = new SolidColorBrush(Color.Parse("#569CD6")); // Blue for true/false/null
    private static readonly IBrush BraceBrush = new SolidColorBrush(Color.Parse("#FFD700")); // Gold for braces and brackets
    private static readonly IBrush ColonCommaBrush = new SolidColorBrush(Color.Parse("#D4D4D4")); // Light gray for : and ,
    private static readonly Typeface BoldTypeface = new(FontFamily.Default, weight: FontWeight.Bold);

    // JSON property names (e.g., "start": or "text":)
    [GeneratedRegex(@"""([^""\\]*(\\.[^""\\]*)*)""(?=\s*:)", RegexOptions.Compiled)]
    private static partial Regex JsonPropertyNameRegex();

    // JSON string values (not followed by colon)
    [GeneratedRegex(@"""([^""\\]*(\\.[^""\\]*)*)""(?!\s*:)", RegexOptions.Compiled)]
    private static partial Regex JsonStringValueRegex();

    // JSON numbers (integers and decimals, including negative)
    [GeneratedRegex(@"-?\d+\.?\d*", RegexOptions.Compiled)]
    private static partial Regex JsonNumberRegex();

    // JSON boolean and null values
    [GeneratedRegex(@"\b(true|false|null)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex JsonBooleanNullRegex();

    // JSON structural characters
    [GeneratedRegex(@"[{}\[\]]", RegexOptions.Compiled)]
    private static partial Regex JsonBracesRegex();

    // Colons and commas
    [GeneratedRegex(@"[,:]", RegexOptions.Compiled)]
    private static partial Regex JsonDelimitersRegex();

    protected override void ColorizeLine(DocumentLine line)
    {
        var lineText = CurrentContext.Document.GetText(line);
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        // Order matters: we need to colorize in the right sequence to avoid overlaps
        
        // 1. Colorize property names first (strings followed by colon)
        ColorizePropertyNames(line, lineText);

        // 2. Colorize string values (strings not followed by colon)
        ColorizeStringValues(line, lineText);

        // 3. Colorize booleans and null
        ColorizeBooleanNull(line, lineText);

        // 4. Colorize numbers
        ColorizeNumbers(line, lineText);

        // 5. Colorize structural characters (braces, brackets)
        ColorizeBraces(line, lineText);

        // 6. Colorize delimiters (colons, commas)
        ColorizeDelimiters(line, lineText);
    }

    private void ColorizePropertyNames(DocumentLine line, string lineText)
    {
        foreach (Match match in JsonPropertyNameRegex().Matches(lineText))
        {
            ChangeLinePart(
                line.Offset + match.Index,
                line.Offset + match.Index + match.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(PropertyNameBrush);
                });
        }
    }

    private void ColorizeStringValues(DocumentLine line, string lineText)
    {
        foreach (Match match in JsonStringValueRegex().Matches(lineText))
        {
            // Skip if this is already colored as a property name
            if (IsAlreadyColored(lineText, match.Index))
            {
                continue;
            }

            ChangeLinePart(
                line.Offset + match.Index,
                line.Offset + match.Index + match.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(StringBrush);
                });
        }
    }

    private void ColorizeNumbers(DocumentLine line, string lineText)
    {
        foreach (Match match in JsonNumberRegex().Matches(lineText))
        {
            // Skip if inside quotes
            if (IsInsideQuotes(lineText, match.Index))
            {
                continue;
            }

            // Validate it's a proper JSON number (not part of a string or property name)
            if (IsValidJsonNumber(lineText, match.Index, match.Length))
            {
                ChangeLinePart(
                    line.Offset + match.Index,
                    line.Offset + match.Index + match.Length,
                    element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(NumberBrush);
                    });
            }
        }
    }

    private void ColorizeBooleanNull(DocumentLine line, string lineText)
    {
        foreach (Match match in JsonBooleanNullRegex().Matches(lineText))
        {
            // Skip if inside quotes
            if (IsInsideQuotes(lineText, match.Index))
            {
                continue;
            }

            ChangeLinePart(
                line.Offset + match.Index,
                line.Offset + match.Index + match.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(BooleanBrush);
                    element.TextRunProperties.SetTypeface(BoldTypeface);
                });
        }
    }

    private void ColorizeBraces(DocumentLine line, string lineText)
    {
        foreach (Match match in JsonBracesRegex().Matches(lineText))
        {
            // Skip if inside quotes
            if (IsInsideQuotes(lineText, match.Index))
            {
                continue;
            }

            ChangeLinePart(
                line.Offset + match.Index,
                line.Offset + match.Index + match.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(BraceBrush);
                    element.TextRunProperties.SetTypeface(BoldTypeface);
                });
        }
    }

    private void ColorizeDelimiters(DocumentLine line, string lineText)
    {
        foreach (Match match in JsonDelimitersRegex().Matches(lineText))
        {
            // Skip if inside quotes
            if (IsInsideQuotes(lineText, match.Index))
            {
                continue;
            }

            ChangeLinePart(
                line.Offset + match.Index,
                line.Offset + match.Index + match.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(ColonCommaBrush);
                });
        }
    }

    private static bool IsInsideQuotes(string lineText, int position)
    {
        var quoteCount = 0;
        var escaped = false;

        for (var i = 0; i < position && i < lineText.Length; i++)
        {
            if (lineText[i] == '\\' && !escaped)
            {
                escaped = true;
                continue;
            }

            if (lineText[i] == '"' && !escaped)
            {
                quoteCount++;
            }

            escaped = false;
        }

        return quoteCount % 2 == 1;
    }

    private static bool IsAlreadyColored(string lineText, int position)
    {
        // Check if this position is part of a property name (followed by colon)
        var colonIndex = lineText.IndexOf(':', position);
        if (colonIndex == -1)
        {
            return false;
        }

        var textBetween = lineText.Substring(position, colonIndex - position);
        return textBetween.Contains('"') && textBetween.Trim().EndsWith('"');
    }

    private static bool IsValidJsonNumber(string lineText, int position, int length)
    {
        // Check if the character before and after are valid JSON separators
        var before = position > 0 ? lineText[position - 1] : ' ';
        var after = position + length < lineText.Length ? lineText[position + length] : ' ';

        var validBefore = char.IsWhiteSpace(before) || before == ':' || before == '[' || before == '{' || before == ',';
        var validAfter = char.IsWhiteSpace(after) || after == ',' || after == ']' || after == '}' || after == ':';

        return validBefore && validAfter;
    }
}
