using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System;
using System.Text;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Fast syntax highlighting for XML-based subtitle formats (TTML, DFXP, etc.)
/// Automatically reformats single-line XML for better performance
/// </summary>
public partial class XmlSourceSyntaxHighlighting : DocumentColorizingTransformer
{
    // Color scheme
    private static readonly IBrush XmlTagBrush = new SolidColorBrush(Color.Parse("#569CD6"));
    private static readonly IBrush XmlAttributeBrush = new SolidColorBrush(Color.Parse("#9CDCFE"));
    private static readonly IBrush XmlValueBrush = new SolidColorBrush(Color.Parse("#CE9178"));
    private static readonly IBrush CommentBrush = new SolidColorBrush(Color.Parse("#6A9955"));

    private bool _hasReformatted;

    protected override void ColorizeLine(DocumentLine line)
    {
        // On first run, check if we need to reformat
        if (!_hasReformatted)
        {
            _hasReformatted = true;
            var doc = CurrentContext.Document;

            // Check if document is mostly on one line (line count < 10% of tag count)
            if (ShouldReformat(doc.Text))
            {
                var formatted = FormatXml(doc.Text);
                if (formatted != doc.Text)
                {
                    doc.Text = formatted;
                    return; // Exit, let next render handle the formatted document
                }
            }
        }

        var lineText = CurrentContext.Document.GetText(line);
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        // Fast single-pass colorization
        ColorizeLineContent(line, lineText);
    }

    private static bool ShouldReformat(string text)
    {
        // Count lines and tags
        int lineCount = 1;
        int tagCount = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                lineCount++;
            }
            else if (text[i] == '<' && i + 1 < text.Length && text[i + 1] != '!')
            {
                tagCount++;
            }
        }

        // If we have many tags but few lines, reformat
        return tagCount > 20 && lineCount < (tagCount / 3);
    }

    private static string FormatXml(string xml)
    {
        var sb = new StringBuilder(xml.Length + xml.Length / 4); // Pre-allocate with buffer
        int indent = 0;
        bool inTag = false;
        bool inComment = false;
        bool afterCloseTag = false;
        int i = 0;

        while (i < xml.Length)
        {
            char c = xml[i];

            // Handle comments
            if (!inComment && c == '<' && i + 3 < xml.Length &&
                xml[i + 1] == '!' && xml[i + 2] == '-' && xml[i + 3] == '-')
            {
                inComment = true;
                if (afterCloseTag)
                {
                    sb.AppendLine();
                    sb.Append(' ', indent * 2);
                }
                sb.Append("<!--");
                i += 4;
                afterCloseTag = false;
                continue;
            }

            if (inComment)
            {
                sb.Append(c);
                if (c == '-' && i + 2 < xml.Length && xml[i + 1] == '-' && xml[i + 2] == '>')
                {
                    sb.Append("-->");
                    i += 3;
                    inComment = false;
                    afterCloseTag = true;
                }
                else
                {
                    i++;
                }
                continue;
            }

            // Handle tags
            if (c == '<')
            {
                bool isClosing = i + 1 < xml.Length && xml[i + 1] == '/';

                // Check if self-closing
                int tagEnd = i;
                while (tagEnd < xml.Length && xml[tagEnd] != '>')
                {
                    if (tagEnd > i && xml[tagEnd] == '/' && tagEnd + 1 < xml.Length && xml[tagEnd + 1] == '>')
                    {
                        break;
                    }
                    tagEnd++;
                }

                if (isClosing)
                {
                    indent--;
                }

                // Add newline and indent before opening tags (unless first tag)
                if (sb.Length > 0 && (isClosing || afterCloseTag))
                {
                    sb.AppendLine();
                    sb.Append(' ', Math.Max(0, indent * 2));
                }

                inTag = true;
                sb.Append(c);
                afterCloseTag = false;
            }
            else if (c == '>')
            {
                sb.Append(c);
                inTag = false;

                // Check if this was a self-closing tag
                bool wasSelfClosing = i > 0 && xml[i - 1] == '/';

                // Check if opening tag (not closing, not self-closing)
                bool wasOpening = true;
                for (int j = i - 1; j >= 0 && j > i - 50; j--)
                {
                    if (xml[j] == '<')
                    {
                        if (j + 1 < xml.Length && (xml[j + 1] == '/' || xml[j + 1] == '!'))
                        {
                            wasOpening = false;
                        }
                        break;
                    }
                }

                if (wasOpening && !wasSelfClosing)
                {
                    indent++;
                }

                afterCloseTag = true;
            }
            else if (!inTag && char.IsWhiteSpace(c))
            {
                // Skip whitespace between tags
                i++;
                continue;
            }
            else
            {
                sb.Append(c);
                if (!inTag && !char.IsWhiteSpace(c))
                {
                    afterCloseTag = false;
                }
            }

            i++;
        }

        return sb.ToString();
    }

    private void ColorizeLineContent(DocumentLine line, string lineText)
    {
        int i = 0;
        int len = lineText.Length;

        while (i < len)
        {
            char c = lineText[i];

            // Handle comments
            if (c == '<' && i + 3 < len && lineText[i + 1] == '!' && lineText[i + 2] == '-' && lineText[i + 3] == '-')
            {
                int start = i;
                i += 4;

                while (i < len - 2)
                {
                    if (lineText[i] == '-' && lineText[i + 1] == '-' && lineText[i + 2] == '>')
                    {
                        i += 3;
                        break;
                    }
                    i++;
                }

                ChangeLinePart(
                    line.Offset + start,
                    line.Offset + i,
                    element => element.TextRunProperties.SetForegroundBrush(CommentBrush));
                continue;
            }

            // Handle tags
            if (c == '<')
            {
                int tagStart = i;
                i++;

                // Skip to end of tag
                while (i < len && lineText[i] != '>')
                {
                    i++;
                }

                if (i < len)
                {
                    i++; // Include >
                }

                int tagEnd = i;

                // Colorize entire tag
                ChangeLinePart(
                    line.Offset + tagStart,
                    line.Offset + tagEnd,
                    element => element.TextRunProperties.SetForegroundBrush(XmlTagBrush));

                // Now find and colorize attributes
                ColorizeAttributes(line, lineText, tagStart, tagEnd);
                continue;
            }

            i++;
        }
    }

    private void ColorizeAttributes(DocumentLine line, string lineText, int tagStart, int tagEnd)
    {
        int i = tagStart + 1;

        // Skip tag name
        while (i < tagEnd && !char.IsWhiteSpace(lineText[i]) && lineText[i] != '>' && lineText[i] != '/')
        {
            i++;
        }

        while (i < tagEnd - 1)
        {
            // Skip whitespace
            while (i < tagEnd && char.IsWhiteSpace(lineText[i]))
            {
                i++;
            }

            if (i >= tagEnd - 1 || lineText[i] == '>' || lineText[i] == '/')
            {
                break;
            }

            // Find attribute name
            int attrStart = i;
            while (i < tagEnd && (char.IsLetterOrDigit(lineText[i]) || lineText[i] == ':' || lineText[i] == '-' || lineText[i] == '_' || lineText[i] == '.'))
            {
                i++;
            }

            if (i > attrStart)
            {
                // Colorize attribute name
                ChangeLinePart(
                    line.Offset + attrStart,
                    line.Offset + i,
                    element => element.TextRunProperties.SetForegroundBrush(XmlAttributeBrush));

                // Skip whitespace and =
                while (i < tagEnd && (char.IsWhiteSpace(lineText[i]) || lineText[i] == '='))
                {
                    i++;
                }

                // Find quoted value
                if (i < tagEnd && (lineText[i] == '"' || lineText[i] == '\''))
                {
                    char quote = lineText[i];
                    int valueStart = i;
                    i++;

                    while (i < tagEnd && lineText[i] != quote)
                    {
                        i++;
                    }

                    if (i < tagEnd)
                    {
                        i++; // Include closing quote
                    }

                    // Colorize value
                    ChangeLinePart(
                        line.Offset + valueStart,
                        line.Offset + i,
                        element => element.TextRunProperties.SetForegroundBrush(XmlValueBrush));
                }
            }
            else
            {
                i++;
            }
        }
    }

    public void Reset()
    {
        _hasReformatted = false;
    }
}